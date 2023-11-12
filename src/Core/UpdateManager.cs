using Microsoft.EntityFrameworkCore;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy _batchStrategy;
    private readonly IDictionary<string, IDatabaseEngine> _databaseEngines;

    private UpdateManager()
    {
    }

    internal UpdateManager(UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.DatabaseEngines);
        ArgumentOutOfRangeException.ThrowIfZero(options.DatabaseEngines.Count());
        ArgumentNullException.ThrowIfNull(options.SourceProvider);

        _databaseEngines = options.DatabaseEngines
            .Select(databaseEngine => databaseEngine()!)
            .ToDictionary(databaseEngine => databaseEngine.Name);
        _sourceProvider = options.SourceProvider();
        IBatchCacheManager? cacheManager = options.CacheManager?.Invoke();
        _batchStrategy = cacheManager is null ? options.BatchStrategy.Invoke() : new CachedBatchStrategy(options.BatchStrategy.Invoke(), cacheManager);
    }

    public async Task<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default)
    {
        List<UpdateResult> results = [];
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        foreach (Source source in sources)
        {
            UpdateResult result = await UpdateSource(source, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }
        return results.AsReadOnly();
    }

    public async Task<UpdateResult> UpdateSource(string sourceName, CancellationToken cancellationToken = default)
    {
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source source = sources.First(s => s.Name == sourceName);
        return await UpdateSource(source, cancellationToken).ConfigureAwait(false);
    }

    private async Task<UpdateResult> UpdateSource(Source source, CancellationToken cancellationToken = default)
    {
        IDatabaseEngine dbEngine = _databaseEngines[source.Provider];
        IEnumerable<Batch> batches = await _batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);

        UpdateResult updateResult = new(source.Name, dbEngine.Name, source.ConnectionString, -1, -1, null);
        try
        {
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
}