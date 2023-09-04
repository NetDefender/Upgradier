namespace Ugradier.Core;

public sealed class FileBatchCacheManager : IBatchCacheManager
{
    private readonly string _basePath;

    public FileBatchCacheManager(string basePath)
    {
        _basePath = basePath;
    }

    public void Store(long versionId, int threadId, string batch)
    {
        File.WriteAllText(Path.Combine(_basePath, threadId.ToString(), $"{versionId}.sql"), batch);
    }

    public bool TryLoad(long versionId, int threadId, out string? batch)
    {
        FileInfo batchFile = new (Path.Combine(_basePath, threadId.ToString(), $"{versionId}.sql"));
        if(batchFile.Exists )
        {
            batch = File.ReadAllText(batchFile.FullName);
            return true;
        }
        batch = null;
        return false;
    }
}
