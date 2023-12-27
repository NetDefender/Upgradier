using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;
using Upgradier.SourceProviders.Aws;

namespace Upgradier.BatchStrategies.Aws;

public static class Extensions
{
    public static UpdateBuilder WithAwsS3SourceProvider(this UpdateBuilder builder, IAmazonS3 awsClient, string bucketName, string prefixKey, string fileName)
    {
        TransferUtility utility = new(awsClient);
        return builder.WithSourceProvider(options => new AwsSourceProvider(utility, options.Logger, options.Environment, bucketName, prefixKey, fileName));
    }
}