using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.BatchStrategy.Azure;

public static class Extensions
{
    public static void ThrowWhenError(this Response response, Uri uri)
    {
        if (!response.IsError)
        {
            throw new RequestFailedException($"An error ocurred when downloading from {uri}");
        }
    }

    public static UpdateBuilder WithAzureBlobBatchStrategy(this UpdateBuilder builder, Func<BlobContainerClient> factory, string provider, string? environment = null)
    {
        Func<IBatchStrategy> batchStrategy = () =>
        {
            BlobContainerClient azureClient = factory();
            return new AzureBlobBatchStrategy(environment, azureClient);
        };
        builder.WithBatchStrategy(batchStrategy);
        return builder;
    }
}
