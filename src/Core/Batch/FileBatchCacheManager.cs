using System.Text;

namespace Upgradier.Core;

public sealed class FileBatchCacheManager : IBatchCacheManager
{
    private readonly string _basePath;
    private readonly LogAdapter _logger;

    public FileBatchCacheManager(string basePath, LogAdapter logger, string? environment)
    {
        basePath.ThrowIfDirectoryNotExists();
        _basePath = basePath;
        _logger = logger;
        Environment = environment;
    }

    public string? Environment { get; }

    public async Task<bool> TryStore(long versionId, string provider, string batch, CancellationToken cancellationToken)
    {
        try
        {
            DirectoryInfo batchsDirectory = new(Path.Combine(_basePath, provider, Environment.EmptyIfNull()));
            batchsDirectory.CreateIfNotExists();
            using FileStream fsLock = new(Path.Combine(batchsDirectory.FullName, $"{versionId}.sql"), FileMode.Create, FileAccess.Write, FileShare.None);
            using StreamWriter writer = new(fsLock, Encoding.UTF8);
            await writer.WriteAsync(batch).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogTryStoreError(versionId, provider, ex);
            return false;
        }
    }

    public async Task<BatchCacheResult> TryLoad(long versionId, string provider, CancellationToken cancellationToken)
    {
        try
        {
            DirectoryInfo batchsDirectory = new (Path.Combine(_basePath, provider, Environment.EmptyIfNull()));
            batchsDirectory.CreateIfNotExists();
            FileInfo batchFile = new(Path.Combine(batchsDirectory.FullName, $"{versionId}.sql"));
            if (batchFile.Exists)
            {
                using StreamReader reader = batchFile.OpenText();
                string contents = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                return new BatchCacheResult(true, false, contents);
            }
            return BatchCacheResult.Miss;
        }
        catch (Exception ex)
        {
            _logger.LogTryLoadError(versionId, provider, ex);
            return BatchCacheResult.Locked;
        }
    }
}