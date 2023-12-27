using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.SourceProviders.Azure;

public static class Extensions
{
    internal static void ThrowWhenError(this Response response, Uri uri)
    {
        if (response.IsError)
        {
            throw new RequestFailedException($"An error ocurred when downloading from {uri}");
        }
    }

    public static UpdateBuilder WithAzureBlobSourceProvider(this UpdateBuilder builder, BlobContainerClient blobClient, string fileName)
    {
        return builder.WithSourceProvider(options => new AzureSourceProvider(blobClient, options.Logger, options.Environment, fileName));
    }
}
