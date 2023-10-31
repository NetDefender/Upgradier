
using System.Text.Json;

public sealed class FileSourceProvider : SourceProviderBase
{
    public FileSourceProvider(string fileName) : base(nameof(FileSourceProvider))
    {
        FileName = fileName;
    }

    public string FileName { get; }

    public override async Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        using FileStream fs = new (FileName, FileMode.Open, FileAccess.Read);
        IEnumerable<Source>? sources = await JsonSerializer.DeserializeAsync<IEnumerable<Source>>(fs, cancellationToken: cancellationToken);
        return sources ?? Enumerable.Empty<Source>();
    }
}