using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.PostgreSql;
using Xunit.Abstractions;

namespace Upgradier.Tests.PostgreSql;

public sealed class PostgreSqlEngine_Tests : IClassFixture<PostgreSqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public PostgreSqlEngine_Tests(ITestOutputHelper output, PostgreSqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    [Fact]
    public void Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new PostgreSqlEngine(null, null, null, null));
    }

    [Fact]
    public void Throws_When_CommandTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PostgreSqlEngine(new LogAdapter(null), null, -1, null));
    }

    [Fact]
    public void Throws_When_ConnectionTimeout_Is_Less_Than_0()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PostgreSqlEngine(new LogAdapter(null), null, null, -1));
    }

    [Fact]
    public void No_Throws_When_Timeouts_Are_Greater_Than_0()
    {
        PostgreSqlEngine engine = new (new LogAdapter(null), null, 1, 1);
        Assert.NotNull(engine);
    }

    [Fact]
    public void Name_Is_PostgreSql()
    {
        PostgreSqlEngine engine = new (new LogAdapter(null), null, null, null);
        Assert.Equal(PostgreSqlEngine.NAME, engine.Name);
    }

    [Fact]
    public void CreateLockStrategy_Throws_When_SourceDatabase_Is_Not_SqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        Assert.Throws<InvalidCastException>(() =>
        {
            new PostgreSqlEngine(logger, null, null, null)
                .CreateLockStrategy
                (new SourceDatabase(new DbContextOptionsBuilder()
                    .UseNpgsql(_connectionString)
                    .Options, logger, null)
                );
        });
    }

    [Fact]
    public void CreateLockStrategy_Is_SqlLockStrategy_When_SourceDatabase_Is_SqlSourceDatabase()
    {
        LogAdapter logger = new (null);
        PostgreSqlEngine engine = new(logger, "Dev", null, null);
        ILockManager lockManager = engine.CreateLockStrategy(new PostgreSqlSourceDatabase(new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(_connectionString)
            .Options, logger, "Dev"));
        Assert.True(lockManager is PostgreSqlLockManager);
    }

    [Fact]
    public void CreateSourceDatabase_Is_SqlSourceDatabase()
    {
        PostgreSqlEngine engine = new(new LogAdapter(null), "Dev", null, null);
        SourceDatabase sourceDatabase = engine.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is PostgreSqlSourceDatabase);
    }
}
