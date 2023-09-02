using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Aws;

public static class Extensions
{
    public static UpdateBuilder WithAwsS3BatchStrategy(this UpdateBuilder builder, Func<AmazonS3Client> factory, string bucketName, string provider, string? environment = null)
    {
        Func<IBatchStrategy> batchStrategy = () =>
        {
            AmazonS3Client awsClient = factory();
            TransferUtility utility = new (awsClient);
            return new AwsS3BatchStrategy(bucketName, provider, environment, utility);
        };
        builder.WithBatchStrategy(batchStrategy);
        return builder;
    }
}