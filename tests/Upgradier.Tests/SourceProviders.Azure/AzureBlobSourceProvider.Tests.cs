using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Upgradier.Core;
using Upgradier.DatabaseEngines.SqlServer;
using Upgradier.Tests.BatchStrategies.Azure;
using Upgradier.Tests.DatabaseEngines.SqlServer;
using System.Text.Json;
using Upgradier.SourceProviders.Azure;
using Upgradier.BatchStrategies.Azure;

namespace Upgradier.Tests.SourceProviders.Azure;

public class AzureBlobSourceProvider_Tests : IClassFixture<AzuriteFixture>, IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _azuriteConnectionString;
    private readonly string _sqlServerConnectionString;
    private const string TESTSOURCENAME = "test-source";

    public AzureBlobSourceProvider_Tests(AzuriteFixture azuriteFixture, SqlServerDatabaseFixture sqlServerFixture)
    {
        _azuriteConnectionString = azuriteFixture.ConnectionString;
        _sqlServerConnectionString = sqlServerFixture.ConnectionString;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_Using_Sources_And_Batches_From_Azurite_To_SqlServer_Database(bool useCache)
    {
        BlobContainerClient blobContainerClient = await CreateBlobContainerClient();
        ILogger logger = LoggerFactory.Create(options => options.SetMinimumLevel(LogLevel.Debug)).CreateLogger("Basic");

        UpdateBuilder builder = new UpdateBuilder()
            .WithAzureBlobBatchStrategy(blobContainerClient)
            .WithAzureBlobSourceProvider(blobContainerClient, "Sources.json")
            .AddSqlServerEngine()
            .WithConnectionTimeout(30)
            .WithCommandTimeout(30)
            .WithLogger(logger);

        if (useCache)
        {
            builder.WithCacheManager(options => new FileBatchCacheManager("Core/Cache", options.Logger, options.Environment));
        }

        UpdateManager updater = builder.Build();
        IEnumerable<UpdateResult> results = await updater.UpdateAsync(CancellationToken.None);
        Assert.NotNull(results);
        Assert.Single(results);
        UpdateResult result = results.First();

        Assert.Null(result.Error);
        Assert.Equal(TESTSOURCENAME, result.Source);
        Assert.Equal(2, result.Version);
        Assert.Equal(_sqlServerConnectionString, result.ConnectionString);
    }

    private async Task<BlobContainerClient> CreateBlobContainerClient()
    {
        BlobContainerClient containerClient = new(_azuriteConnectionString, "files-location");
        if (await containerClient.ExistsAsync())
        {
            return containerClient;
        }

        Response<BlobContainerInfo> containerCreatedResponse = await containerClient.CreateAsync();
        Assert.Equal((int)HttpStatusCode.Created, containerCreatedResponse.GetRawResponse().Status);

        Source source = new(TESTSOURCENAME, SqlEngine.NAME, _sqlServerConnectionString);
        Source[] sources = [source];
        await using MemoryStream sourcesStream = new();
        JsonSerializer.Serialize(sourcesStream, sources);
        sourcesStream.Position = 0L;
        await using FileStream indexFile = new("Core/Batches/Index.json", FileMode.Open, FileAccess.Read);
        await using FileStream oneSqlFile = new("Core/Batches/SqlServer/1.sql", FileMode.Open, FileAccess.Read);
        await using FileStream twoSqlFile = new("Core/Batches/SqlServer/2.sql", FileMode.Open, FileAccess.Read);

        Response<BlobContentInfo> indexUploadResponse = await containerClient.UploadBlobAsync("Index.json", indexFile);
        Assert.Equal((int)HttpStatusCode.Created, indexUploadResponse.GetRawResponse().Status);
        Response<BlobContentInfo> oneUploadResponse = await containerClient.UploadBlobAsync("SqlServer/1.sql", oneSqlFile);
        Assert.Equal((int)HttpStatusCode.Created, oneUploadResponse.GetRawResponse().Status);
        Response<BlobContentInfo> twoUploadResponse = await containerClient.UploadBlobAsync("SqlServer/2.sql", twoSqlFile);
        Assert.Equal((int)HttpStatusCode.Created, twoUploadResponse.GetRawResponse().Status);
        Response<BlobContentInfo> sourceUploadResponse = await containerClient.UploadBlobAsync("Sources.json", sourcesStream);
        Assert.Equal((int)HttpStatusCode.Created, sourceUploadResponse.GetRawResponse().Status);

        return containerClient;
    }
}