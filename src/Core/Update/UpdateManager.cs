using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy _batchStrategy;
    private readonly IDictionary<string, IDatabaseEngine> _databaseEngines;
    private readonly UpdateEventDispatcher _eventDispatcher;
    private readonly int _parallelism;
    private readonly LogAdapter _logger;

    private UpdateManager()
    {
        throw new InvalidOperationException("Empty constructor is not allowed");
    }

    internal UpdateManager(ISourceProvider sourceProvider
        , IDictionary<string, IDatabaseEngine> databaseEngines
        , IBatchStrategy batchStrategy
        , UpdateEventDispatcher eventDispatcher
        , LogAdapter logger
        , int parallelism)
    {
        ArgumentNullException.ThrowIfNull(sourceProvider);
        ArgumentNullException.ThrowIfNull(databaseEngines);
        ArgumentNullException.ThrowIfNull(batchStrategy);
        ArgumentOutOfRangeException.ThrowIfZero(databaseEngines.Count);
        ArgumentOutOfRangeException.ThrowIfLessThan(parallelism, UpdateResultTaskBuffer.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parallelism, UpdateResultTaskBuffer.MaxValue);

        _sourceProvider = sourceProvider;
        _databaseEngines = databaseEngines;
        _batchStrategy = batchStrategy;
        _eventDispatcher = eventDispatcher;
        _parallelism = parallelism;
        _logger = logger;
    }

    public async Task<IEnumerable<UpdateResult>> UpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogStarting();
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogSources(sources);
        UpdateResultTaskBuffer updateTaskBuffer = new (_parallelism, _logger);
        IEnumerable<Batch> batches = await _batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);
        List<UpdateResult> updateResults = [];

        foreach (Source source in sources)
        {
            Task<UpdateResult> updateTask = UpdateSource(source, batches, cancellationToken);
            if(!updateTaskBuffer.TryAdd(updateTask))
            {
                UpdateResult[] bufferResults = await updateTaskBuffer.WhenAll().ConfigureAwait(false);
                updateTaskBuffer.Clear();
                updateTaskBuffer.Add(updateTask);
                updateResults.AddRange(bufferResults);
            }
        }

        UpdateResult[] pendingResults = await updateTaskBuffer.WhenAll().ConfigureAwait(false);
        updateTaskBuffer.Clear();
        updateResults.AddRange(pendingResults);
       _logger.LogResults(updateResults);
        
        return updateResults.AsReadOnly();
    }
    private async Task<UpdateResult> UpdateSource(Source source, IEnumerable<Batch> batches, CancellationToken cancellationToken = default)
    {
        using IDisposable? updatingLoggingScope = _logger.LogBeginUpdatingSource(source);
        _logger.LogUpdatingSource(source);
        UpdateResult updateResult = new(source.Name, default!, source.ConnectionString, -1, -1, -1, null);
        Batch? nextBatchToExecute = null;
        try
        {
            IDatabaseEngine dbEngine = _databaseEngines[source.Provider];
            updateResult = updateResult with { DatabaseEngine = dbEngine.Name };

            using SourceDatabase database = dbEngine.CreateSourceDatabase(source.ConnectionString);
            using ILockManager @lock = dbEngine.CreateLockStrategy(database);

            if (await @lock.TryAdquireAsync(cancellationToken).ConfigureAwait(false))
            {
                ArgumentNullException.ThrowIfNull(@lock.Transaction);
                DatabaseVersion currentVersion = await database.Version.FirstAsync(cancellationToken).ConfigureAwait(false);
                long startVersion = currentVersion.VersionId;
                updateResult = updateResult with { Version = startVersion, OriginalVersion = startVersion };
                _logger.LogGettingInitialSourceVersion(source, currentVersion);

                foreach (Batch batch in batches.Where(b => b.VersionId > startVersion).OrderBy(b => b.VersionId))
                {
                    await _eventDispatcher.NotifyBeforeBatchProcessingAsync(currentVersion.VersionId, @lock.Transaction!, cancellationToken).ConfigureAwait(false);

                    nextBatchToExecute = batch;
                    string batchContents = await _batchStrategy.GetBatchContentsAsync(batch, source.Provider, cancellationToken).ConfigureAwait(false);
                    await database.ExecuteBatchAsync(batchContents, cancellationToken).ConfigureAwait(false);
                    await database.ChangeCurrentVersionAsync(currentVersion, batch.VersionId, cancellationToken).ConfigureAwait(false);

                    await _eventDispatcher.NotifyAfterBatchProcessedAsync(currentVersion.VersionId, @lock.Transaction!, cancellationToken).ConfigureAwait(false);
                }
                updateResult = updateResult with { Version = currentVersion.VersionId };
                await @lock.FreeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogUpdatingSourceError(source, nextBatchToExecute, ex);
            updateResult = updateResult with { Version = updateResult.OriginalVersion, ErrorVersion = nextBatchToExecute?.VersionId,  Error = ex };
        }

        return updateResult;
    }

    public async Task<UpdateResult> UpdateSourceAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        IEnumerable<Batch> batches = await _batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source source = sources.First(s => s.Name == sourceName);
        return await UpdateSource(source, batches, cancellationToken).ConfigureAwait(false);
    }
}