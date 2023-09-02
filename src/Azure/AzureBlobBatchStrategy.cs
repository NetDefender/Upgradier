using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Azure;

public class AzureBlobBatchStrategy : BatchStrategyBase
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobBatchStrategy(string provider, string? environment, BlobContainerClient containerClient) : base(environment, provider, nameof(AzureBlobBatchStrategy))
    {
        ArgumentNullException.ThrowIfNull(containerClient);
        _containerClient = containerClient;
    }

    public override async ValueTask<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
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

    public override async ValueTask<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken)
    {
        StringBuilder batchesPath = new StringBuilder(50)
            .Append(Provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(batch.VersionId).Append(".sql");
        BlobClient blobClient = _containerClient.GetBlobClient(batchesPath.ToString());
        MemoryStream downloadStream = new();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        return new StreamReader(downloadStream, leaveOpen: false);
    }
}