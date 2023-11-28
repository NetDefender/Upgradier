using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.SqlServer;
using Upgradier.Tests.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

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

    private SqlLockManager CreateLockManager()
    {
        LogAdapter logger = new (null);
        SqlSourceDatabase db = new (new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString).Options, logger);
        return new SqlLockManager(db, logger);
    }

    [Fact]
    [TestOrder(1)]
    public async Task TryAdquireAsync_FirstTime_Initializes_Database_With_MigrationId_1()
    {
        EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
        using SqlLockManager manager = CreateLockManager();
        using CancellationTokenSource cancellationTokenSource = new ();
        bool adquired = await manager.TryAdquireAsync(cancellationTokenSource.Token);
        Assert.True(adquired, "Lock should be adquired");
        MigrationHistory migrationHistory = await manager.Context.MigrationHistory.FirstAsync(cancellationTokenSource.Token);
        Assert.Equal(1, migrationHistory.MigrationId);
        EnvironmentVariables.SetExecutionEnvironment(null);
    }

    [Fact]
    [TestOrder(2)]
    public async Task EnsureSchema_Called_Multiple_Times_Doesnt_Change_MigrationId()
    {
        EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
        using SqlLockManager manager = CreateLockManager();
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
        EnvironmentVariables.SetExecutionEnvironment(null);
    }
}