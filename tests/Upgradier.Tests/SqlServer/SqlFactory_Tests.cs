using Microsoft.EntityFrameworkCore;
using Upgradier.Core;
using Upgradier.SqlServer;
using Xunit.Abstractions;

namespace Upgradier.Tests.SqlServer;

public sealed class SqlFactory_Tests : IClassFixture<SqlServerDatabaseFixture>
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public SqlFactory_Tests(ITestOutputHelper output, SqlServerDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString!;
        _output = output;
    }

    [Fact]
    public async Task SqlFactory_Name_Is_SqlServer()
    {
        SqlFactory factory = new (null, string.Empty);
        Assert.Equal(SqlFactory.NAME, factory.Name);
    }

    [Fact]
    public async Task CreateLockStrategy_Throws_When_SourceDatabase_Is_Not_SqlSourceDatabase()
    {
        SqlFactory factory = new(null, string.Empty);
        try
        {
            factory.CreateLockStrategy(new SourceDatabase(new DbContextOptionsBuilder()
                .UseSqlServer(_connectionString)
                .Options));
            Assert.Fail("Cant create ILockStrategy because SourceDatabase is not SqlServerDatabase");
        }
        catch (Exception ex) when (ex is InvalidCastException)
        {
            Assert.True(true);
        }
        catch (Exception)
        {
            Assert.Fail("Exception is not InvalidCastException");
        }
        Assert.Equal("SqlServer", factory.Name);
    }

    [Fact]
    public async Task CreateLockStrategy_Is_SqlLockStrategy_When_SourceDatabase_Is_SqlSourceDatabase()
    {
        SqlFactory factory = new("dev", string.Empty);
        ILockStrategy lockStrategy = factory.CreateLockStrategy(new SqlSourceDatabase("dev", new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(_connectionString)
            .Options));
        Assert.True(lockStrategy is SqlLockStrategy);
    }

    [Theory]
    [InlineData(@"c:\path\scripts")]
    [InlineData(@"\\path.place.com\scripts")]
    public async Task CreateScriptStrategy_Is_FileScriptStrategy(string directory)
    {
        SqlFactory factory = new("dev", directory);
        IScriptStragegy scriptStrategy = factory.CreateScriptStrategy();
        Assert.True(scriptStrategy is FileScriptStrategy);
    }

    [Theory]
    [InlineData("https://enterprise.es/scripts")]
    [InlineData("http://enterprise.es/scripts")]
    public async Task CreateScriptStrategy_Is_WebScriptStrategy(string url)
    {
        SqlFactory factory = new("dev", url);
        IScriptStragegy scriptStrategy = factory.CreateScriptStrategy();
        Assert.True(scriptStrategy is WebScriptStrategy);
    }

    [Fact]
    public async Task CreateSourceDatabase_Is_SqlSourceDatabase()
    {
        SqlFactory factory = new("dev", string.Empty);
        SourceDatabase sourceDatabase = factory.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is SqlSourceDatabase);
    }
}
