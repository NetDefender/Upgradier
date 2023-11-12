using Upgradier.Core;

namespace Ugradier.Core;

public sealed class CachedBatchStrategy : IBatchStrategy
{
    private readonly IBatchStrategy _baseBatchStrategy;
    private readonly IBatchCacheManager _cacheManager;

    public CachedBatchStrategy(IBatchStrategy baseBatchStrategy, IBatchCacheManager cacheManager)
    {
        _baseBatchStrategy = baseBatchStrategy;
        _cacheManager = cacheManager;
    }

    public string Name { get => _baseBatchStrategy.Name; }

    public Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
    {
        return _baseBatchStrategy.GetBatchesAsync(cancellationToken);
    }

    public async Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken)
    {
        BatchCacheResult cacheResult = await _cacheManager.TryLoad(batch.VersionId, provider, cancellationToken).ConfigureAwait(false);
        if (cacheResult.Success)
        {
            return cacheResult.Contents!;
        }
        string contents = await _baseBatchStrategy.GetBatchContentsAsync(batch, provider, cancellationToken).ConfigureAwait(false);
        try
        {
            await _cacheManager.Store(batch.VersionId, provider, contents, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // TODO: log
        }
        return contents;
    }
}
