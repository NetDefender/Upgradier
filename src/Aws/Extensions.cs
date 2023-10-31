using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Aws;

public static class Extensions
{
    public static UpdateBuilder WithAwsS3BatchStrategy(this UpdateBuilder builder, Func<AmazonS3Client> factory, string bucketName)
    {
        IBatchStrategy batchStrategy()
        {
            AmazonS3Client awsClient = factory();
            TransferUtility utility = new(awsClient);
            return new AwsS3BatchStrategy(bucketName,  utility);
        }
        builder.WithBatchStrategy(batchStrategy);
        return builder;
    }
}