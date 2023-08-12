using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.ScriptStrategy.Azure;

public static class Extensions
{
    public static void ThrowWhenError(this Response response, Uri uri)
    {
        if (!response.IsError)
        {
            throw new RequestFailedException($"An error ocurred when downloading from {uri}");
        }
    }

    public static UpdateBuilder WithAzureBlobScriptStrategy(this UpdateBuilder builder, Func<BlobContainerClient> factory, string provider, string? environment = null)
    {
        Func<IScriptStrategy> scriptStrategy = () =>
        {
            BlobContainerClient azureClient = factory();
            return new AzureBlobScriptStrategy(provider, environment, azureClient);
        };
        builder.WithScriptStrategy(scriptStrategy);
        return builder;
    }
}
