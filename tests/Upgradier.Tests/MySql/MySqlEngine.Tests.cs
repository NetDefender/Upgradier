using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.MySql;
using Xunit.Abstractions;

namespace Upgradier.Tests.MySql;

public sealed class MySqlEngine_Tests : IClassFixture<MySqlDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public MySqlEngine_Tests(ITestOutputHelper output, MySqlDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    [Fact]
    public void Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlEngine(null, null, null, null));
    }

    [Fact]
    public void Throws_When_CommandTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MySqlEngine(new LogAdapter(null), null, -1, null));
    }

    [Fact]
    public void Throws_When_ConnectionTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MySqlEngine(new LogAdapter(null), null, null, -1));
    }

    [Fact]
    public void No_Throws_When_Timeouts_Are_Greater_Than_0()
    {
        MySqlEngine engine = new (new LogAdapter(null), null, 1, 1);
        Assert.NotNull(engine);
    }

    [Fact]
    public void Name_Is_MySql()
    {
        MySqlEngine engine = new (new LogAdapter(null), null, null, null);
        Assert.Equal(MySqlEngine.NAME, engine.Name);
    }

    [Fact]
    public void CreateLockStrategy_Throws_When_SourceDatabase_Is_Not_MySqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        Assert.Throws<InvalidCastException>(() =>
        {
            new MySqlEngine(logger, null, null, null)
                .CreateLockStrategy
                (new SourceDatabase(new DbContextOptionsBuilder()
                    .UseMySql(ServerVersion.AutoDetect(_connectionString))
                    .Options, logger, null)
                );
        });
    }

    [Fact]
    public void CreateLockStrategy_Is_SqlLockStrategy_When_SourceDatabase_Is_MySqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        MySqlEngine engine = new(logger, "Dev", null, null);
        ILockManager lockManager = engine.CreateLockStrategy(new MySqlSourceDatabase(new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseSqlServer(_connectionString)
            .Options, logger, "Dev"));
        Assert.True(lockManager is MySqlLockManager);
    }

    [Fact]
    public void CreateSourceDatabase_Is_MySqlSourceDatabase()
    {
        MySqlEngine engine = new(new LogAdapter(null), "Dev", null, null);
        SourceDatabase sourceDatabase = engine.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is MySqlSourceDatabase);
    }
}
