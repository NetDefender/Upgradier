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
        IScriptStrategy scriptStrategy = factory.CreateScriptStrategy();
        Assert.True(scriptStrategy is FileScriptStrategy);
    }

    [Theory]
    [InlineData("https://enterprise.es/scripts")]
    [InlineData("http://enterprise.es/scripts")]
    public async Task CreateScriptStrategy_Is_WebScriptStrategy(string url)
    {
        SqlFactory factory = new("dev", url);
        IScriptStrategy scriptStrategy = factory.CreateScriptStrategy();
        Assert.True(scriptStrategy is WebScriptStrategy);
    }

    [Theory]
    [InlineData("ftp://enterprise.es/scripts")]
    [InlineData("ftps://enterprise.es/scripts")]
    [InlineData("sftp://enterprise.es/scripts")]
    [InlineData("file://enterprise.es/scripts")]
    [InlineData("gopher://enterprise.es/scripts")]
    [InlineData("ws://enterprise.es/scripts")]
    [InlineData("wss://enterprise.es/scripts")]
    [InlineData("mailto://enterprise.es/scripts")]
    [InlineData("news://enterprise.es/scripts")]
    [InlineData("nntp://enterprise.es/scripts")]
    [InlineData("ssh://enterprise.es/scripts")]
    [InlineData("telnet://enterprise.es/scripts")]
    [InlineData("net.tcp://enterprise.es/scripts")]
    [InlineData("net.pipe://enterprise.es/scripts")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("")]
    public async Task CreateScriptStrategy_Fails_For_Unknown_Schemes(string url)
    {
        SqlFactory factory = new("dev", url);
        try
        {
            IScriptStrategy scriptStrategy = factory.CreateScriptStrategy();
            Assert.Fail("By default there is not a strategy for other protocols except http/https but looks like it is");
        }
        catch (InvalidOperationException ex)
        {
            _output.WriteLine($"Expected: {ex.Message}");
        }
    }

    [Fact]
    public async Task CreateSourceDatabase_Is_SqlSourceDatabase()
    {
        SqlFactory factory = new("dev", string.Empty);
        SourceDatabase sourceDatabase = factory.CreateSourceDatabase(_connectionString);
        Assert.True(sourceDatabase is SqlSourceDatabase);
    }
}
