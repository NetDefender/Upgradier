namespace Upgradier.Core;

public interface IBatchCacheManager
{
    Task Store(long versionId, string provider, string batch, CancellationToken cancellationToken);

    Task<BatchCacheResult> TryLoad(long versionId, string provider, CancellationToken cancellationToken);
}
