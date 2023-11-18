using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy _batchStrategy;
    private readonly IDictionary<string, IDatabaseEngine> _databaseEngines;
    private readonly UpdateEventDispatcher _eventDispatcher;
    private readonly ILogger? _logger;
    private readonly int _parallelism;

    private UpdateManager()
    {
        throw new InvalidOperationException("Empty constructor is not allowed");
    }

    internal UpdateManager(UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.DatabaseEngines);
        ArgumentOutOfRangeException.ThrowIfZero(options.DatabaseEngines.Count());
        ArgumentNullException.ThrowIfNull(options.SourceProvider);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Parallelism, UpdateResultTaskBuffer.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.Parallelism, UpdateResultTaskBuffer.MaxValue);

        _parallelism = options.Parallelism;
        _databaseEngines = options.DatabaseEngines
            .Select(databaseEngine => databaseEngine()!)
            .ToDictionary(databaseEngine => databaseEngine.Name);
        _sourceProvider = options.SourceProvider();
        IBatchCacheManager? cacheManager = options.CacheManager?.Invoke();
        _batchStrategy = cacheManager is null ? options.BatchStrategy.Invoke() : new CachedBatchStrategy(options.BatchStrategy.Invoke(), cacheManager);
        _eventDispatcher = new(options.Events?.Invoke());
        _logger = options.Logger;
    }

    public async Task<IEnumerable<UpdateResult>> UpdateAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        LogSources(sources);
        UpdateResultTaskBuffer updateTaskBuffer = new (_parallelism);
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

        LogResults(updateResults);
        UpdateResult[] resultsWithErrors = updateResults.Where(result => result.Error is not null).ToArray();
        
        return updateResults.AsReadOnly();
    }

    private void LogSources(IEnumerable<Source> sources)
    {
        _logger?.LogInformation("Enumerating sources");
        foreach (Source source in sources)
        {
            _logger?.LogDebug("Source:[{Name}], Provider:[{Provider}], ConnectionString:[{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
        }
    }

    private void LogResults(List<UpdateResult> results)
    {
        if (results.Exists(result => result.Error is not null))
        {
            _logger?.LogWarning("Enumerating results with errors");
            foreach (UpdateResult resultWithError in results.Where(result => result.Error is not null))
            {
                _logger?.LogError("Source:[{Source}], Version:[{Version}], ConnectionString:[{ConnectionString}], ErrorVersion: [{ErrorVersion}], Message:[{Message}]", resultWithError.Source, resultWithError.Version, resultWithError.ConnectionString, resultWithError.ErrorVersion, resultWithError.Error?.Message);
            }
            _logger?.LogWarning("Update completed with errors");
        }
        else
        {
            _logger?.LogDebug("Update completed with success!");
        }
    }

    private async Task<UpdateResult> UpdateSource(Source source, IEnumerable<Batch> batches, CancellationToken cancellationToken = default)
    {
        using IDisposable? loggingScope = _logger?.BeginScope(source.Name);
        _logger?.LogDebug("Updating Source:[{Name}], Provider: [{Provider}], ConnectionString: [{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
        UpdateResult updateResult = new(source.Name, default!, source.ConnectionString, -1, -1, -1, null);
        Batch? currentBatch = null;
        try
        {
            IDatabaseEngine dbEngine = _databaseEngines[source.Provider];
            updateResult = updateResult with { DatabaseEngine = dbEngine.Name };

            using SourceDatabase database = dbEngine.CreateSourceDatabase(source.ConnectionString);
            using ILockManager @lock = dbEngine.CreateLockStrategy(database);

            if (await @lock.TryAdquireAsync(cancellationToken).ConfigureAwait(false))
            {
                DatabaseVersion currentVersion = await database.Version.FirstAsync(cancellationToken).ConfigureAwait(false);
                long startVersion = currentVersion.VersionId;
                updateResult = updateResult with { Version = startVersion, OriginalVersion = startVersion };
                _logger?.LogDebug("Original Version:[{Version}]", currentVersion.VersionId);

                foreach (Batch batch in batches.Where(b => b.VersionId > startVersion).OrderBy(b => b.VersionId))
                {
                    currentBatch = batch;
                    _logger?.LogDebug("Batch:[{VersionId}]", batch.VersionId);
                    await _eventDispatcher.NotifyBeforeExecuteBatchAsync(currentVersion.VersionId, database.Database.CurrentTransaction!, cancellationToken).ConfigureAwait(false);
                    await ExecuteBatchAsync(batch, database, source, cancellationToken).ConfigureAwait(false);
                    await IncrementVersionAsync(batch, database, currentVersion, cancellationToken).ConfigureAwait(false);
                    updateResult = updateResult with { Version = currentVersion.VersionId };
                    await _eventDispatcher.NotifyAfterExecuteBatchAsync(currentVersion.VersionId, database.Database.CurrentTransaction!, cancellationToken).ConfigureAwait(false);
                }
                await @lock.FreeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error ocurred updating the source [{Source}] with connection string [{ConnectionString}], provider [{Provider}] and batch [{Batch}]", source.Name, source.ConnectionString, source.Provider, currentBatch?.VersionId);
            updateResult = updateResult with { Version = updateResult.OriginalVersion, ErrorVersion = currentBatch?.VersionId,  Error = ex };
        }

        return updateResult;
    }

    private async Task ExecuteBatchAsync(Batch batch, SourceDatabase database, Source source, CancellationToken cancellationToken)
    {
        string batchContent = await _batchStrategy.GetBatchContentsAsync(batch, source.Provider, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Batch Contents: {BatchContent}", batchContent);
        await database.Database.ExecuteSqlRawAsync(batchContent!, cancellationToken).ConfigureAwait(false);
    }

    private async Task IncrementVersionAsync(Batch batch, SourceDatabase database, DatabaseVersion version, CancellationToken cancellationToken)
    {
        database.Remove(version);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        version.VersionId = batch.VersionId;
        database.Add(version);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("New Version: {VersionId}", version.VersionId);
    }

    public async Task<UpdateResult> UpdateSourceAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        IEnumerable<Batch> batches = await _batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source source = sources.First(s => s.Name == sourceName);
        return await UpdateSource(source, batches, cancellationToken).ConfigureAwait(false);
    }
}