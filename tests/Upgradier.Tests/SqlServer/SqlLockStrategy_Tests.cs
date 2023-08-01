using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.SqlServer;
using Upgradier.Tests.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

[TestCaseOrderer("Upgradier.Tests.Core.ManualTestOrderer", "Upgradier.Tests")]
public sealed class SqlLockStrategy_Tests : IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public SqlLockStrategy_Tests(ITestOutputHelper output, SqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    private SqlSourceDatabase CreateSourceDatabase(string? environment)
    {
        return new SqlSourceDatabase(environment, new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString).Options);
    }

    private SqlLockStrategy CreateLockStrategy(string? environment) 
    {
        return new SqlLockStrategy(environment, CreateSourceDatabase(environment));
    }

    [Fact]
    [TestOrder(1)]
    public async Task TryAdquireAsync_FirstTime_Initializes_Database_With_MigrationId_1()
    {
        using SqlLockStrategy strategy = CreateLockStrategy("dev");
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        bool adquired = await strategy.TryAdquireAsync(cancellationTokenSource.Token);
        Assert.True(adquired, "Lock shoudl be adquired");
        MigrationHistory migrationHistory = await strategy.Context.MigrationHistory.FirstAsync(cancellationTokenSource.Token);
        Assert.Equal(1, migrationHistory.MigrationId);
    }

    [Fact]
    [TestOrder(2)]
    public async Task EnsureSchema_Called_Multiple_Times_Doesnt_Change_MigrationId()
    {
        using SqlLockStrategy strategy = CreateLockStrategy("dev");
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await strategy.EnsureSchema(cancellationTokenSource.Token);
        await strategy.EnsureSchema(cancellationTokenSource.Token);
        await strategy.EnsureSchema(cancellationTokenSource.Token);
        await strategy.EnsureSchema(cancellationTokenSource.Token);
        MigrationHistory migrationHistory = await strategy.Context.MigrationHistory.FirstAsync(cancellationTokenSource.Token);
        Assert.Equal(1, migrationHistory.MigrationId);
    }
}
