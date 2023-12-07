using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.BatchStrategies.Aws;

public static class Extensions
{
    public static UpdateBuilder WithAwsS3BatchStrategy(this UpdateBuilder builder, IAmazonS3 awsClient, string bucketName)
    {
        TransferUtility utility = new(awsClient);
        return builder.WithBatchStrategy( options => new AwsS3BatchStrategy(bucketName, utility, options.Logger, options.Environment));
    }
}