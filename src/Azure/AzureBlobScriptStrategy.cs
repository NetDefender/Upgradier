using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.ScriptStrategy.Azure;

public class AzureBlobScriptStrategy : ScriptStrategyBase
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobScriptStrategy(string provider, string? environment, BlobContainerClient containerClient) : base(environment, provider, nameof(AzureBlobScriptStrategy))
    {
        ArgumentNullException.ThrowIfNull(containerClient);
        _containerClient = containerClient;
    }

    public override async ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        using MemoryStream downloadStream = new ();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        List<Script>? scripts = await JsonSerializer.DeserializeAsync<List<Script>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(scripts);
        return scripts.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        StringBuilder scriptPath = new StringBuilder(50)
            .Append(Provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(script.VersionId).Append(".sql");
        BlobClient blobClient = _containerClient.GetBlobClient(scriptPath.ToString());
        MemoryStream downloadStream = new();
        using Response response = await blobClient.DownloadToAsync(downloadStream, cancellationToken).ConfigureAwait(false);
        response.ThrowWhenError(blobClient.Uri);
        downloadStream.Position = 0L;
        return new StreamReader(downloadStream, leaveOpen: false);
    }
}