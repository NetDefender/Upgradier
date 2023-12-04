﻿using Microsoft.Extensions.Logging;
using NSubstitute;
using Upgradier.Core;
using Upgradier.SqlServer;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

public class UpdateManger_Sql_Tests : IClassFixture<MultipleSqlServerDatabaseFixture>
{
    private string _connectionStringOne;
    private string _connectionStringTwo;
    private readonly ITestOutputHelper _output;
    private IEnumerable<Source> _sources;

    public UpdateManger_Sql_Tests(ITestOutputHelper output, MultipleSqlServerDatabaseFixture fixture)
    {
        _connectionStringOne = fixture.ConnectionStringOne;
        _connectionStringTwo = fixture.ConnectionStringTwo;
        _sources = [new Source("One-Database", SqlEngine.NAME, _connectionStringOne)
            , new Source("Two-Database", SqlEngine.NAME, _connectionStringTwo)];
        _output = output;
    }

    [Fact]
    public async Task Update_One_Clean_Database_Migrates_And_Updates_Database()
    {
        ILogger logger = LoggerFactory.Create(options => options.SetMinimumLevel(LogLevel.Debug)).CreateLogger("Basic");

        Source oneSource = _sources.First();
        Source twoSource = _sources.Last();

        SourceProviderBase sourceFactory(SourceProviderCreationOptions options)
        {
            SourceProviderBase sourceProvider = Substitute.For<SourceProviderBase>("CustomSourceProviderName", options.Logger, options.Environment);
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
        Assert.Equal(2, results.Count());
        UpdateResult oneResult = results.First();
        UpdateResult twoResult = results.Last();
        
        Assert.Null(oneResult.Error);
        Assert.Null(twoResult.Error);
        Assert.Equal(oneSource.Name, oneResult.Source);
        Assert.Equal(twoSource.Name, twoResult.Source);
        Assert.Equal(2, oneResult.Version);
        Assert.Equal(2, twoResult.Version);
        Assert.Equal(oneSource.ConnectionString, oneResult.ConnectionString);
        Assert.Equal(twoSource.ConnectionString, twoResult.ConnectionString);
    }
}
