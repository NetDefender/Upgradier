using Microsoft.EntityFrameworkCore;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy _batchStrategy;
    private readonly IDictionary<string, IDatabaseEngine> _databaseEngines;
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
    }

    public async Task<IEnumerable<UpdateResult>> UpdateAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        UpdateResultTaskBuffer updateTaskBuffer = new UpdateResultTaskBuffer(_parallelism);
        List<UpdateResult> updateResults = [];

        foreach (Source source in sources)
        {
            Task<UpdateResult> updateTask = UpdateSource(source, cancellationToken);
            if(!updateTaskBuffer.TryAdd(updateTask))
            {
                UpdateResult[] bufferResults = await updateTaskBuffer.WhenAll().ConfigureAwait(false);
                updateTaskBuffer.Add(updateTask);
                updateResults.AddRange(bufferResults);
            }
        }

        UpdateResult[] lastResults = await updateTaskBuffer.WhenAll().ConfigureAwait(false);
        updateResults.AddRange(lastResults);

        return updateResults.AsReadOnly();
    }

    private async Task<UpdateResult> UpdateSource(Source source, CancellationToken cancellationToken = default)
    {
        UpdateResult updateResult = new(source.Name, default!, source.ConnectionString, -1, -1, null);

        try
        {
            IDatabaseEngine dbEngine = _databaseEngines[source.Provider];
            updateResult = updateResult with { DatabaseEngine = dbEngine.Name };
            IEnumerable<Batch> batches = await _batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);

            using SourceDatabase db = dbEngine.CreateSourceDatabase(source.ConnectionString);
            using ILockStrategy @lock = dbEngine.CreateLockStrategy(db);

            if (await @lock.TryAdquireAsync(cancellationToken).ConfigureAwait(false))
            {
                DatabaseVersion currentVersion = await db.Version.FirstAsync(cancellationToken).ConfigureAwait(false);
                long startVersion = currentVersion.VersionId;
                updateResult = updateResult with { Version = startVersion, OriginalVersion = startVersion };
                foreach (Batch batch in batches.Where(b => b.VersionId > startVersion).OrderBy(b => b.VersionId))
                {
                    string batchContent = await _batchStrategy.GetBatchContentsAsync(batch, source.Provider, cancellationToken).ConfigureAwait(false);
                    await db.Database.ExecuteSqlRawAsync(batchContent!, cancellationToken).ConfigureAwait(false);
                    db.Remove(currentVersion);
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    currentVersion.VersionId = batch.VersionId;
                    db.Add(currentVersion);
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    updateResult = updateResult with { Version = batch.VersionId };
                }
                await @lock.FreeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            updateResult = updateResult with { Version = updateResult.OriginalVersion, Error = ex };
        }

        return updateResult;
    }

    public async Task<UpdateResult> UpdateSourceAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source source = sources.First(s => s.Name == sourceName);
        return await UpdateSource(source, cancellationToken).ConfigureAwait(false);
    }
}