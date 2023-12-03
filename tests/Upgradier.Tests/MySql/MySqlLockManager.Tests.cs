using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.MySql;
using Upgradier.Tests.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.MySql;

[TestCaseOrderer("Upgradier.Tests.Core.ManualTestOrderer", "Upgradier.Tests")]
public sealed class MySqlLockManagerTests : IClassFixture<MySqlDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public MySqlLockManagerTests(ITestOutputHelper output, MySqlDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    private MySqlLockManager CreateLockManager(string? environment)
    {
        LogAdapter logger = new (null);
        MySqlSourceDatabase db = new (new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseMySQL(_connectionString).Options, logger, environment);
        return new MySqlLockManager(db, logger, environment);
    }

    [Fact]
    [TestOrder(1)]
    public async Task TryAdquireAsync_FirstTime_Initializes_Database_With_MigrationId_1()
    {
        using MySqlLockManager manager = CreateLockManager("Dev");
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
        using MySqlLockManager manager = CreateLockManager("Dev");
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