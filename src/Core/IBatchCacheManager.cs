namespace Ugradier.Core;

public interface IBatchCacheManager
{
    Task Store(long versionId, int threadId, string batch, CancellationToken cancellationToken);

    Task<BatchCacheResult> TryLoad(long versionId, int threadId, CancellationToken cancellationToken);
}
