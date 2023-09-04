namespace Ugradier.Core;

public interface IBatchCacheManager
{
    void Store(long versionId, int threadId, string batch);

    bool TryLoad(long versionId, int threadId, out string? batch);
}
