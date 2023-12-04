using Microsoft.Extensions.Logging;
using NSubstitute;
using Upgradier.Core;
using Upgradier.SqlServer;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

public class UpdateManger_Sql_Tests : IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;
    private IEnumerable<Source> _sources;

    public UpdateManger_Sql_Tests(ITestOutputHelper output, SqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _sources = [new Source("One-Database", SqlEngine.NAME, _connectionString)];
        _output = output;
    }

    [Fact]
    public async Task Update_One_Clean_Database_Migrates_And_Updates_Database()
    {
        ILogger logger = LoggerFactory.Create(options => options.SetMinimumLevel(LogLevel.Debug)).CreateLogger("Basic");

        SourceProviderBase sourceFactory(SourceProviderCreationOptions options)
        {
            SourceProviderBase sourceProvider = Substitute.For<SourceProviderBase>("One", options.Logger, options.Environment);
            sourceProvider.GetSourcesAsync(CancellationToken.None).Returns(Task.FromResult(_sources));
            return sourceProvider;
        }

        UpdateBuilder builder = new UpdateBuilder()
            .WithSourceProvider((Func<SourceProviderCreationOptions, SourceProviderBase>)sourceFactory)
            .WithFileBatchStrategy("Core/Batches")
            .WithCacheManager(options => new FileBatchCacheManager("Core/Cache", options.Logger, options.Environment))
            .WithParallelism(1)
            .AddSqlServerEngine()
            .WithConnectionTimeout(30)
            .WithCommandTimeout(30)
            .WithLogger(logger);

        UpdateManager updater = builder.Build();

        IEnumerable<UpdateResult> results = await updater.UpdateAsync(CancellationToken.None);

        Assert.NotNull(results);
        Assert.Single(results);
        UpdateResult uniqueResult = results.First();
        Assert.Null(uniqueResult.Error);
        Assert.Equal(2, uniqueResult.Version);
    }
}
