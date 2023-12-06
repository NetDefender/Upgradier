using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.DatabaseEngines.SqlServer;
using Upgradier.Tests.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.DatabaseEngines.SqlServer;

[TestCaseOrderer("Upgradier.Tests.Core.ManualTestOrderer", "Upgradier.Tests")]
public sealed class SqlLockManagerTests : IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public SqlLockManagerTests(ITestOutputHelper output, SqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    private SqlLockManager CreateLockManager(string? environment)
    {
        LogAdapter logger = new (null);
        SqlSourceDatabase db = new (new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString).Options, logger, environment);
        return new SqlLockManager(db, logger, environment);
    }

    [Fact]
    [TestOrder(1)]
    public async Task TryAdquireAsync_FirstTime_Initializes_Database_With_MigrationId_1()
    {
        using SqlLockManager manager = CreateLockManager("Dev");
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
        using SqlLockManager manager = CreateLockManager("Dev");
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