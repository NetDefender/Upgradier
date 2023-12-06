using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.DatabaseEngines.SqlServer;
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
    public void Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlEngine(null, null, null, null));
    }

    [Fact]
    public void Throws_When_CommandTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SqlEngine(new LogAdapter(null), null, -1, null));
    }

    [Fact]
    public void Throws_When_ConnectionTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SqlEngine(new LogAdapter(null), null, null, -1));
    }

    [Fact]
    public void No_Throws_When_Timeouts_Are_Greater_Than_0()
    {
        SqlEngine engine = new (new LogAdapter(null), null, 1, 1);
        Assert.NotNull(engine);
    }

    [Fact]
    public void Name_Is_SqlServer()
    {
        SqlEngine engine = new (new LogAdapter(null), null, null, null);
        Assert.Equal(SqlEngine.NAME, engine.Name);
    }

    [Fact]
    public void CreateLockStrategy_Throws_When_SourceDatabase_Is_Not_SqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        Assert.Throws<InvalidCastException>(() =>
        {
            new SqlEngine(logger, null, null, null)
                .CreateLockStrategy
                (new SourceDatabase(new DbContextOptionsBuilder()
                    .UseSqlServer(_connectionString)
                    .Options, logger, null)
                );
        });
    }

    [Fact]
    public void CreateLockStrategy_Is_SqlLockStrategy_When_SourceDatabase_Is_SqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        SqlEngine engine = new(logger, "Dev", null, null);
        ILockManager lockManager = engine.CreateLockStrategy(new SqlSourceDatabase(new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString)
            .Options, logger, "Dev"));
        Assert.True(lockManager is SqlLockManager);
    }

    [Fact]
    public void CreateSourceDatabase_Is_SqlSourceDatabase()
    {
        SqlEngine engine = new(new LogAdapter(null), "Dev", null, null);
        SourceDatabase sourceDatabase = engine.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is SqlSourceDatabase);
    }
}
