﻿using System.Text;
using System.Text.Json;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategies.Aws;

public class AwsS3BatchStrategy : BatchStrategyBase
{
    private readonly string _bucketName;
    private readonly ITransferUtility _transferUtility;
    private Func<TransferUtilityDownloadRequest, Task> _configureRequest = _ => Task.CompletedTask;

    public AwsS3BatchStrategy(string bucketName, ITransferUtility transferUtility, LogAdapter logger, string? environment) : base(nameof(AwsS3BatchStrategy), logger, environment)
    {
        ArgumentNullException.ThrowIfNull(bucketName);
        ArgumentNullException.ThrowIfNull(transferUtility);
        _bucketName = bucketName;
        _transferUtility = transferUtility;
    }

    public override async Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
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

    public override async Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken)
    {
        FileInfo downloadFile = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        StringBuilder key = new StringBuilder(50)
            .Append(provider)
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
        using StreamReader downloadStream = downloadFile.OpenText();
        return downloadStream.ReadToEnd();
    }

    public void ConfigureRequestMessage(Func<TransferUtilityDownloadRequest, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(configureRequest);
        _configureRequest = configureRequest;
    }
}