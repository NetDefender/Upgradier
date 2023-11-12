using System.Text;

namespace Upgradier.Core;

public sealed class FileBatchCacheManager : IBatchCacheManager
{
    private readonly string _basePath;

    public FileBatchCacheManager(string basePath)
    {
        _basePath = basePath;
        Environment = EnvironmentVariables.GetExecutionEnvironment();
    }

    public string? Environment { get; }

    public async Task Store(long versionId, string provider, string batch, CancellationToken cancellationToken)
    {
        using FileStream fsLock = new (Path.Combine(_basePath, provider, Environment.EmptyIfNull(), $"{versionId}.sql"), FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new (fsLock, Encoding.UTF8);
        await writer.WriteAsync(batch).ConfigureAwait(false);
    }

    public async Task<BatchCacheResult> TryLoad(long versionId, string provider, CancellationToken cancellationToken)
    {
        FileInfo batchFile = new(Path.Combine(_basePath, provider, Environment.EmptyIfNull(), $"{versionId}.sql"));
        if (batchFile.Exists)
        {
            try
            {
                using StreamReader reader = batchFile.OpenText();
                string contents = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                return new BatchCacheResult(true, false, contents);
            }
            catch (Exception)
            {
                return BatchCacheResult.Locked;
            }
        }
        return BatchCacheResult.Miss;
    }
}
