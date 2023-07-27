using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Upgradier.BatchStrategies.Azure;
using Upgradier.Core;
using Upgradier.DatabaseEngines.SqlServer;
using Upgradier.Tests.DatabaseEngines.SqlServer;

namespace Upgradier.Tests.BatchStrategies.Azure;

public class AzureBlobBatchStrategy_Tests : IClassFixture<AzuriteFixture>, IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _azuriteConnectionString;
    private readonly string _sqlServerConnectionString;

    public AzureBlobBatchStrategy_Tests(AzuriteFixture azuriteFixture, SqlServerDatabaseFixture sqlServerFixture)
    {
        _azuriteConnectionString = azuriteFixture.ConnectionString;
        _sqlServerConnectionString = sqlServerFixture.ConnectionString;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_Using_Batches_From_Azurite_To_SqlServer_Database(bool useCache)
    {
        Source source = new ("test-source", SqlEngine.NAME, _sqlServerConnectionString);
        IEnumerable<Source> sources = [source];
        BlobContainerClient blobContainerClient = await CreateBlobContainerClient();
        ILogger logger = LoggerFactory.Create(options => options.SetMinimumLevel(LogLevel.Debug)).CreateLogger("Basic");

        SourceProviderBase sourceFactory(SourceProviderCreationOptions options)
        {
            SourceProviderBase sourceProvider = Substitute.For<SourceProviderBase>("CustomSourceProviderName", options.Logger, options.Environment);
            sourceProvider.GetSourcesAsync(CancellationToken.None).Returns(Task.FromResult(sources));
            return sourceProvider;
        }

        UpdateBuilder builder = new UpdateBuilder()
            .WithSourceProvider((Func<SourceProviderCreationOptions, SourceProviderBase>)sourceFactory)
            .WithAzureBlobBatchStrategy(blobContainerClient)
            .AddSqlServerEngine()
            .WithConnectionTimeout(30)
            .WithCommandTimeout(30)
            .WithLogger(logger);

        if(useCache)
        {
            builder.WithCacheManager(options => new FileBatchCacheManager("Core/Cache", options.Logger, options.Environment));
        }

        UpdateManager updater = builder.Build();
        IEnumerable<UpdateResult> results = await updater.UpdateAsync(CancellationToken.None);
        Assert.NotNull(results);
        Assert.Single(results);
        UpdateResult result = results.First();

        Assert.Null(result.Error);
        Assert.Equal(source.Name, result.Source);
        Assert.Equal(2, result.Version);
        Assert.Equal(source.ConnectionString, result.ConnectionString);
    }

    private async Task<BlobContainerClient> CreateBlobContainerClient()
    {
        BlobContainerClient containerClient = new(_azuriteConnectionString, "batches-location");
        if (await containerClient.ExistsAsync())
        {
            return containerClient;
        }

        Response<BlobContainerInfo> containerCreatedResponse = await containerClient.CreateAsync();
        Assert.Equal((int)HttpStatusCode.Created, containerCreatedResponse.GetRawResponse().Status);
        await using FileStream indexFile = new("Core/Batches/Index.json", FileMode.Open, FileAccess.Read);
        await using FileStream oneSqlFile = new("Core/Batches/SqlServer/1.sql", FileMode.Open, FileAccess.Read);
        await using FileStream twoSqlFile = new("Core/Batches/SqlServer/2.sql", FileMode.Open, FileAccess.Read);

        Response<BlobContentInfo> indexUploadResponse = await containerClient.UploadBlobAsync("Index.json", indexFile);
        Assert.Equal((int)HttpStatusCode.Created, indexUploadResponse.GetRawResponse().Status);
        Response<BlobContentInfo> oneUploadResponse = await containerClient.UploadBlobAsync("SqlServer/1.sql", oneSqlFile);
        Assert.Equal((int)HttpStatusCode.Created, oneUploadResponse.GetRawResponse().Status);
        Response<BlobContentInfo> twoUploadResponse = await containerClient.UploadBlobAsync("SqlServer/2.sql", twoSqlFile);
        Assert.Equal((int)HttpStatusCode.Created, twoUploadResponse.GetRawResponse().Status);
        return containerClient;
    }
}