using System.Text;
using System.Text.Json;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.ScriptStrategy.Aws;

public class AwsScriptStrategy : ScriptStrategyBase
{
    private readonly string _bucketName;
    private readonly ITransferUtility _transferUtility;
    private Func<TransferUtilityDownloadRequest, Task> _configureRequest = _ => Task.CompletedTask;

    public AwsScriptStrategy(string bucketName, string provider, string? environment, ITransferUtility transferUtility) : base(environment, provider, nameof(AwsScriptStrategy))
    {
        ArgumentNullException.ThrowIfNull(bucketName);
        ArgumentNullException.ThrowIfNull(transferUtility);
        _bucketName = bucketName;
        _transferUtility = transferUtility;
    }

    public override async ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken)
    {
        FileInfo downloadFile = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        TransferUtilityDownloadRequest request = new()
        {
            FilePath = downloadFile.FullName,
            BucketName = _bucketName,
            Key = string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json",
        };
        await _configureRequest(request).ConfigureAwait(false);
        await _transferUtility.DownloadAsync(request, cancellationToken).ConfigureAwait(false);
        downloadFile.ThrowIfNotExists();
        using FileStream downloadStream = downloadFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        List<Script>? scripts = await JsonSerializer.DeserializeAsync<List<Script>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(scripts);
        return scripts.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        FileInfo downloadFile = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        StringBuilder key = new StringBuilder(50)
            .Append(Provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(script.VersionId).Append(".sql");
        TransferUtilityDownloadRequest request = new()
        {
            FilePath = downloadFile.FullName,
            BucketName = _bucketName,
            Key = key.ToString(),
        };
        await _configureRequest(request).ConfigureAwait(false);
        await _transferUtility.DownloadAsync(request, cancellationToken).ConfigureAwait(false);
        downloadFile.ThrowIfNotExists();
        using FileStream downloadStream = downloadFile.OpenRead();
        return new StreamReader(downloadStream, leaveOpen: false);
    }

    public void ConfigureRequestMessage(Func<TransferUtilityDownloadRequest, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(configureRequest);
        _configureRequest = configureRequest;
    }
}
