using System.Text;

namespace Upgradier.Core;

public sealed class FileBatchCacheManager : IBatchCacheManager
{
    private readonly string _basePath;

    public FileBatchCacheManager(string basePath)
    {
        basePath.ThrowIfDirectoryNotExists();
        _basePath = basePath;
        Environment = EnvironmentVariables.GetExecutionEnvironment();
    }

    public string? Environment { get; }

    public async Task<bool> TryStore(long versionId, string provider, string batch, CancellationToken cancellationToken)
    {
        try
        {
            using FileStream fsLock = new(Path.Combine(_basePath, provider, Environment.EmptyIfNull(), $"{versionId}.sql"), FileMode.Create, FileAccess.Write, FileShare.None);
            using StreamWriter writer = new(fsLock, Encoding.UTF8);
            await writer.WriteAsync(batch).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            // TODO: log
            return false;
        }
    }

    public async Task<BatchCacheResult> TryLoad(long versionId, string provider, CancellationToken cancellationToken)
    {
        try
        {
            FileInfo batchFile = new(Path.Combine(_basePath, provider, Environment.EmptyIfNull(), $"{versionId}.sql"));
            if (batchFile.Exists)
            {
                using StreamReader reader = batchFile.OpenText();
                string contents = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                return new BatchCacheResult(true, false, contents);
            }
            return BatchCacheResult.Miss;
        }
        catch (Exception)
        {
            // TODO: log
            return BatchCacheResult.Locked;
        }
    }
}