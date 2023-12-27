using System.Text.Json;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.SourceProviders.Aws;

public class AwsSourceProvider : SourceProviderBase
{
    private readonly ITransferUtility _transferUtility;
    private readonly string _bucketName;
    private readonly string _fileName;
    private readonly string? _prefixKey;

    public AwsSourceProvider(ITransferUtility transferUtility, LogAdapter logger, string? environment, string bucketName, string? prefixKey, string fileName)
        : base(nameof(AwsSourceProvider), logger, environment)
    {
        _transferUtility = transferUtility;
        _bucketName = bucketName;
        _prefixKey = prefixKey?.TrimEnd('/');
        _fileName = fileName;
    }

    public override async Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        FileInfo downloadFile = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        string baseFileName = Path.GetFileNameWithoutExtension(_fileName);
        string extension = Path.GetExtension(_fileName);
        TransferUtilityDownloadRequest request = new()
        {
            FilePath = downloadFile.FullName,
            BucketName = _bucketName,
            Key = string.IsNullOrEmpty(Environment) ? $"{_prefixKey}/{baseFileName}{extension}" : $"{_prefixKey}/{baseFileName}.{Environment}{extension}",
        };
        await _transferUtility.DownloadAsync(request, cancellationToken).ConfigureAwait(false);
        downloadFile.ThrowIfNotExists();
        await using FileStream downloadStream = downloadFile.Open(new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Options = FileOptions.DeleteOnClose });
        List<Source>? sources = await JsonSerializer.DeserializeAsync<List<Source>>(downloadStream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(sources);
        return sources.AsReadOnly().AsEnumerable();
    }
}