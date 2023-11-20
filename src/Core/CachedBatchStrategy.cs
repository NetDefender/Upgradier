using Ugradier.Core;

namespace Upgradier.Core;

public sealed class CachedBatchStrategy : BatchStrategyBase
{
    private readonly IBatchStrategy _baseBatchStrategy;
    private readonly IBatchCacheManager _cacheManager;
    private readonly LogAdapter _logAdapter;

    public CachedBatchStrategy(IBatchStrategy baseBatchStrategy, IBatchCacheManager cacheManager, LogAdapter logAdapter) : base(baseBatchStrategy.Name)
    {
        _baseBatchStrategy = baseBatchStrategy;
        _cacheManager = cacheManager;
        _logAdapter = logAdapter;
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
            _logAdapter.LogBatchCacheHit(batch.VersionId);
            return cacheResult.Contents!;
        }
        _logAdapter.LogBatchCacheMiss(batch.VersionId);
        string contents = await _baseBatchStrategy.GetBatchContentsAsync(batch, provider, cancellationToken).ConfigureAwait(false);
        bool stored = await _cacheManager.TryStore(batch.VersionId, provider, contents, cancellationToken).ConfigureAwait(false);
        _logAdapter.LogBatchCacheStore(batch.VersionId, stored);
        return contents;
    }
}
