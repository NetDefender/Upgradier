using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.DatabaseEngines.PostgreSql;
using Upgradier.Tests.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.DatabaseEngines.PostgreSql;

[TestCaseOrderer("Upgradier.Tests.Core.ManualTestOrderer", "Upgradier.Tests")]
public sealed class PostgreSqlLockManagerTests : IClassFixture<PostgreSqlDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public PostgreSqlLockManagerTests(ITestOutputHelper output, PostgreSqlDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    private PostgreSqlLockManager CreateLockManager(string? environment)
    {
        LogAdapter logger = new (null);
        PostgreSqlSourceDatabase db = new (new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(_connectionString).Options, logger, environment);
        return new PostgreSqlLockManager(db, logger, environment);
    }

    [Fact]
    [TestOrder(1)]
    public async Task TryAdquireAsync_FirstTime_Initializes_Database_With_MigrationId_1()
    {
        using PostgreSqlLockManager manager = CreateLockManager("Dev");
        using CancellationTokenSource cancellationTokenSource = new ();
        bool adquired = await manager.TryAdquireAsync(cancellationTokenSource.Token);
        Assert.True(adquired, "Lock should be adquired");
        MigrationHistory migrationHistory = await manager.Context.MigrationHistory.FirstAsync(cancellationTokenSource.Token);
        Assert.Equal(1, migrationHistory.MigrationId);
    }

    [Fact]
    [TestOrder(2)]
    public async Task EnsureSchema_Called_Multiple_Times_Doesnt_Change_MigrationId()
    {
        using PostgreSqlLockManager manager = CreateLockManager("Dev");
        using CancellationTokenSource cancellationTokenSource = new ();
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        await manager.Context.EnsureSchema(cancellationTokenSource.Token);
        MigrationHistory migrationHistory = await manager.Context.MigrationHistory.FirstAsync(cancellationTokenSource.Token);
        Assert.Equal(1, migrationHistory.MigrationId);
    }
}