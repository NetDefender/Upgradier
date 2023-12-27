using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.SourceProviders.Azure;

public class AzureSourceProvider : SourceProviderBase
{
    private readonly BlobContainerClient _containerClient;
    private readonly string _fileName;

    public AzureSourceProvider(BlobContainerClient containerClient, LogAdapter logger, string? environment, string fileName)
        : base(nameof(AzureSourceProvider), logger, environment)
    {
        _containerClient = containerClient;
        _fileName = fileName;
    }

    public override async Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        string baseFileName = Path.GetFileNameWithoutExtension(_fileName);
        string extension = Path.GetExtension(_fileName);
        BlobClient blobClient = _containerClient.GetBlobClient(string.IsNullOrEmpty(Environment) ? $"{baseFileName}{extension}" : $"{baseFileName}.{Environment}{extension}");
        using MemoryStream downloadStream = new();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        List<Source>? sources = await JsonSerializer.DeserializeAsync<List<Source>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(sources);
        return sources.AsReadOnly().AsEnumerable();
    }
}
