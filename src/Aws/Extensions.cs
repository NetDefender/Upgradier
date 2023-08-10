using Amazon.S3;
using Amazon.S3.Transfer;
using Upgradier.Core;

namespace Upgradier.ScriptStrategy.Aws;

public static class Extensions
{
    public static void ThrowIfNotExists(this FileInfo file)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException($"Could not download file to {file.FullName}", file.FullName);
        }
    }

    public static UpdateBuilder WithAwsScriptStrategy(this UpdateBuilder builder, Func<AmazonS3Client> factory, string bucketName, string provider, string? environment = null)
    {
        Func<IScriptStrategy> scriptStrategy = () =>
        {
            AmazonS3Client awsClient = factory();
            TransferUtility utility = new (awsClient);
            return new AwsScriptStrategy(bucketName, provider, environment, utility);
        };
        builder.WithScriptStrategy(scriptStrategy);
        return builder;
    }
}