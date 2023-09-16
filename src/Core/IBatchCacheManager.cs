namespace Ugradier.Core;

public interface IBatchCacheManager
{
    Task Store(long versionId, string provider, int threadId, string batch, CancellationToken cancellationToken);

    Task<BatchCacheResult> TryLoad(long versionId, string provider, int threadId, CancellationToken cancellationToken);
}
