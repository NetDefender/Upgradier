﻿using Azure;
using Azure.Storage.Blobs;
using Upgradier.Core;

namespace Upgradier.BatchStrategies.Azure;

public static class Extensions
{
    internal static void ThrowWhenError(this Response response, Uri uri)
    {
        if (response.IsError)
        {
            throw new RequestFailedException($"An error ocurred when downloading from {uri}");
        }
    }

    public static UpdateBuilder WithAzureBlobBatchStrategy(this UpdateBuilder builder, BlobContainerClient blobClient)
    {
        return builder.WithBatchStrategy(options => new AzureBlobBatchStrategy(blobClient, options.Logger, options.Environment));
    }
}