using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Aws;

public static class Extensions
{
    public static UpdateBuilder WithAwsS3BatchStrategy(this UpdateBuilder builder, AmazonS3Client awsClient, string bucketName)
    {
        TransferUtility utility = new(awsClient);
        AwsS3BatchStrategy batchStrategy = new (bucketName, utility);
        builder.WithBatchStrategy(batchStrategy);
        return builder;
    }
}