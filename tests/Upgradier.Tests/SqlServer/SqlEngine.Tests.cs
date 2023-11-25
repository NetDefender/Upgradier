using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.SqlServer;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

public sealed class SqlEngine_Tests : IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public SqlEngine_Tests(ITestOutputHelper output, SqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    [Fact]
    public async Task SqlFactory_Name_Is_SqlServer()
    {
        SqlEngine factory = new (new LogAdapter(null));
        Assert.Equal(SqlEngine.NAME, factory.Name);
    }

    [Fact]
    public async Task CreateLockStrategy_Throws_When_SourceDatabase_Is_Not_SqlSourceDatabase()
    {
        Assert.Throws<InvalidCastException>(() =>
        {
            new SqlEngine(new LogAdapter(null))
                .CreateLockStrategy
                (new SourceDatabase(new DbContextOptionsBuilder()
                    .UseSqlServer(_connectionString)
                    .Options)
                );
        });
    }

    [Fact]
    public async Task CreateLockStrategy_Is_SqlLockStrategy_When_SourceDatabase_Is_SqlSourceDatabase()
    {
        EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
        SqlEngine factory = new(new LogAdapter(null));
        ILockManager lockStrategy = factory.CreateLockStrategy(new SqlSourceDatabase(new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString)
            .Options));
        Assert.True(lockStrategy is SqlLockManager);
        EnvironmentVariables.SetExecutionEnvironment(null);
    }

    [Fact]
    public async Task CreateSourceDatabase_Is_SqlSourceDatabase()
    {
        EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
        SqlEngine factory = new(new LogAdapter(null));
        SourceDatabase sourceDatabase = factory.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is SqlSourceDatabase);
        EnvironmentVariables.SetExecutionEnvironment(null);
    }
}
