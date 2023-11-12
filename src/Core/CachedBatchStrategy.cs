namespace Upgradier.Core;

public sealed class CachedBatchStrategy : BatchStrategyBase
{
    private readonly IBatchStrategy _baseBatchStrategy;
    private readonly IBatchCacheManager _cacheManager;

    public CachedBatchStrategy(IBatchStrategy baseBatchStrategy, IBatchCacheManager cacheManager) : base(baseBatchStrategy.Name)
    {
        _baseBatchStrategy = baseBatchStrategy;
        _cacheManager = cacheManager;
    }

    public override Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
    {
        return _baseBatchStrategy.GetBatchesAsync(cancellationToken);
    }

    public override async Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken)
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
