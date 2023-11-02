using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly int _waitTimeout;
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy _batchStrategy;
    private readonly IBatchCacheManager? _cacheManager;
    private readonly IDictionary<string, IDatabaseEngine> _databaseEngines;
    private readonly SemaphoreSlim _globalLock;
    private bool _isDisposed;

    private UpdateManager()
    { 
    }
    internal UpdateManager(UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.DatabaseEngines);
        ArgumentOutOfRangeException.ThrowIfNegative(options.WaitTimeout);
        ArgumentOutOfRangeException.ThrowIfZero(options.DatabaseEngines.Count());
        ArgumentNullException.ThrowIfNull(options.SourceProvider);

        _waitTimeout = options.WaitTimeout;
        _databaseEngines = options.DatabaseEngines
            .Select(databaseEngine => databaseEngine()!)
            .ToDictionary(databaseEngine => databaseEngine.Name);
        _sourceProvider = options.SourceProvider();
        _cacheManager = options.CacheManager?.Invoke();
        _batchStrategy = options.BatchStrategy.Invoke();
        _globalLock = new SemaphoreSlim(1, 1);
    }

    public async Task<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default)
    {
        bool isGlobalLockAdquired = false;
        List<UpdateResult> results = [];
        try
        {
            if (await _globalLock.WaitAsync(_waitTimeout, cancellationToken).ConfigureAwait(false))
            {
                isGlobalLockAdquired = true;
                IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);

                foreach (Source source in sources)
                {
                    UpdateResult result = await UpdateSource(source, cancellationToken).ConfigureAwait(false);
                    results.Add(result);
                }
            }
        }
        finally
        {
            if (isGlobalLockAdquired)
            {
                _globalLock.Release();
            }
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
                    string batchContent = await ReadBatchContentCached(batch, source, cancellationToken).ConfigureAwait(false);
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

    private async Task<string> ReadBatchContentCached(Batch batch, Source source, CancellationToken cancellationToken)
    {
        if (_cacheManager is null)
        {
            return await ReadBatchContentRaw(batch, source, cancellationToken).ConfigureAwait(false);
        }
        BatchCacheResult cacheResult = await _cacheManager.TryLoad(batch.VersionId, source.Provider, cancellationToken).ConfigureAwait(false);
        if (cacheResult.Success)
        {
            return cacheResult.Contents!;
        }
        string contents = await ReadBatchContentRaw(batch, source, cancellationToken).ConfigureAwait(false);
        try
        {
            await _cacheManager.Store(batch.VersionId, source.Provider, contents, cancellationToken).ConfigureAwait(false);
            return contents;
        }
        catch (Exception)
        {
            return contents;
        }
    }

    private async Task<string> ReadBatchContentRaw(Batch batch, Source source, CancellationToken cancellationToken)
    {
        using StreamReader sqlContentStream = await _batchStrategy.GetBatchContentsAsync(batch, source.Provider, cancellationToken).ConfigureAwait(false);
        return await sqlContentStream.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    public bool IsUpdating()
    {
        return _globalLock.CurrentCount == 0;
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _globalLock.Dispose();
            }
            _isDisposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}