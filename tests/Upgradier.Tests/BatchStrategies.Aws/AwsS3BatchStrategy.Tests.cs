using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Upgradier.BatchStrategies.Aws;
using Upgradier.Core;
using Upgradier.DatabaseEngines.SqlServer;
using Upgradier.Tests.DatabaseEngines.SqlServer;

namespace Upgradier.Tests.BatchStrategies.Aws;

public class AwsS3BatchStrategy_Tests : IClassFixture<AwsS3Fixture>, IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _awsS3ConnectionString;
    private readonly string _sqlServerConnectionString;
    private const string BucketName = "batches-location";

    public AwsS3BatchStrategy_Tests(AwsS3Fixture awsS3Fixture, SqlServerDatabaseFixture sqlServerFixture)
    {
        _awsS3ConnectionString = awsS3Fixture.ConnectionString;
        _sqlServerConnectionString = sqlServerFixture.ConnectionString;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_Using_AwsS3_To_SqlServer_Database(bool useCache)
    {
        Source source = new("test-source", SqlEngine.NAME, _sqlServerConnectionString);
        IEnumerable<Source> sources = [source];
        IAmazonS3 awsS3Client = await CreateAwsS3Client();

        ILogger logger = LoggerFactory.Create(options => options.SetMinimumLevel(LogLevel.Debug)).CreateLogger("Basic");

        SourceProviderBase sourceFactory(SourceProviderCreationOptions options)
        {
            SourceProviderBase sourceProvider = Substitute.For<SourceProviderBase>("CustomSourceProviderName", options.Logger, options.Environment);
            sourceProvider.GetSourcesAsync(CancellationToken.None).Returns(Task.FromResult(sources));
            return sourceProvider;
        }

        UpdateBuilder builder = new UpdateBuilder()
            .WithSourceProvider((Func<SourceProviderCreationOptions, SourceProviderBase>)sourceFactory)
            .WithAwsS3BatchStrategy(awsS3Client, BucketName)
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
        Assert.Equal(source.Name, result.Source);
        Assert.Equal(2, result.Version);
        Assert.Equal(source.ConnectionString, result.ConnectionString);
    }

    private async Task<AmazonS3Client> CreateAwsS3Client()
    {
        AmazonS3Client awsClient = new(new AmazonS3Config() { ServiceURL = _awsS3ConnectionString });
        ListBucketsResponse bucketsResponse = await awsClient.ListBucketsAsync();
        Assert.Equal(HttpStatusCode.OK, bucketsResponse.HttpStatusCode);
        Assert.NotNull(bucketsResponse.Buckets);

        if (bucketsResponse.Buckets.Exists(bucket => bucket.BucketName == BucketName))
        {
            return awsClient;
        }

        PutBucketResponse bucketCreateResponse = await awsClient.PutBucketAsync(BucketName);
        Assert.Equal(HttpStatusCode.OK, bucketCreateResponse.HttpStatusCode);

        await using FileStream indexFile = new("Core/Batches/Index.json", FileMode.Open, FileAccess.Read);
        await using FileStream oneSqlFile = new("Core/Batches/SqlServer/1.sql", FileMode.Open, FileAccess.Read);
        await using FileStream twoSqlFile = new("Core/Batches/SqlServer/2.sql", FileMode.Open, FileAccess.Read);

        TransferUtility transferUtil = new(awsClient);
        await transferUtil.UploadAsync(indexFile, BucketName, "Index.json");
        await transferUtil.UploadAsync(oneSqlFile, BucketName, "SqlServer/1.sql");
        await transferUtil.UploadAsync(twoSqlFile, BucketName, "SqlServer/2.sql");
        return awsClient;
    }
}