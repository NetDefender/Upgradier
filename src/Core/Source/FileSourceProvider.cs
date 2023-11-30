using System.Text.Json;

namespace Upgradier.Core;

public class FileSourceProvider : SourceProviderBase
{
    private readonly string _baseDirectory;
    private readonly string _baseFileName;

    public FileSourceProvider(string name, string baseDirectory, string baseFileName, LogAdapter logger, string? environment)
        : base(name, logger, environment)
    {
        _baseDirectory = baseDirectory;
        _baseFileName = baseFileName;
    }

    public override async Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        byte[] contents = await ReadFileAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<IEnumerable<Source>>(contents);
    }

    protected virtual Task<byte[]> ReadFileAsync()
    {
        string rawFileName = Path.GetFileNameWithoutExtension(_baseFileName);
        string extension = Path.GetExtension(_baseFileName);
        string composedFileName = _baseFileName;

        if(!Environment.IsNullOrEmptyOrWhiteSpace())
        {
            composedFileName = $"{rawFileName}.{Environment}{extension}";
        }

        string fullPath = Path.Combine(_baseDirectory, composedFileName);
        return File.ReadAllBytesAsync(fullPath);
    }
}
