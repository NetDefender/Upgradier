using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Azure;

public class AzureBlobBatchStrategy : BatchStrategyBase
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobBatchStrategy(BlobContainerClient containerClient, LogAdapter logger, string? environment) 
        : base(nameof(AzureBlobBatchStrategy), logger, environment)
    {
        ArgumentNullException.ThrowIfNull(containerClient);
        _containerClient = containerClient;
    }

    public override async Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        using MemoryStream downloadStream = new ();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        List<Batch>? batches = await JsonSerializer.DeserializeAsync<List<Batch>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(batches);
        return batches.AsReadOnly().AsEnumerable();
    }

    public override async Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken)
    {
        StringBuilder batchesPath = new StringBuilder(50)
            .Append(provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(batch.VersionId).Append(".sql");
        BlobClient blobClient = _containerClient.GetBlobClient(batchesPath.ToString());
        using MemoryStream downloadStream = new();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        using StreamReader reader = new (downloadStream, leaveOpen: false);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }
}