namespace Ugradier.Core;

public sealed class FileBatchCacheManager : IBatchCacheManager
{
    private readonly string _basePath;

    public FileBatchCacheManager(string basePath)
    {
        _basePath = basePath;
    }

    public async Task Store(long versionId, string provider, int threadId, string batch, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(Path.Combine(_basePath, provider, threadId.ToString(), $"{versionId}.sql"), batch, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BatchCacheResult> TryLoad(long versionId, string provider, int threadId, CancellationToken cancellationToken)
    {
        FileInfo batchFile = new(Path.Combine(_basePath, provider, threadId.ToString(), $"{versionId}.sql"));
        if (batchFile.Exists)
        {
            return new BatchCacheResult(true, await File.ReadAllTextAsync(batchFile.FullName, cancellationToken).ConfigureAwait(false));
        }
        return BatchCacheResult.Fail;
    }
}
