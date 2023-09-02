using System.Text;
using System.Text.Json;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Aws;

public class AwsS3BatchStrategy : BatchStrategyBase
{
    private readonly string _bucketName;
    private readonly ITransferUtility _transferUtility;
    private Func<TransferUtilityDownloadRequest, Task> _configureRequest = _ => Task.CompletedTask;

    public AwsS3BatchStrategy(string bucketName, string provider, string? environment, ITransferUtility transferUtility) : base(environment, provider, nameof(AwsS3BatchStrategy))
    {
        ArgumentNullException.ThrowIfNull(bucketName);
        ArgumentNullException.ThrowIfNull(transferUtility);
        _bucketName = bucketName;
        _transferUtility = transferUtility;
    }

    public override async ValueTask<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
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
        List<Batch>? batches = await JsonSerializer.DeserializeAsync<List<Batch>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(batches);
        return batches.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken)
    {
        FileInfo downloadFile = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        StringBuilder key = new StringBuilder(50)
            .Append(Provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(batch.VersionId).Append(".sql");
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
