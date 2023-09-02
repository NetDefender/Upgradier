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
    [InlineData(@"c:\path\batches")]
    [InlineData(@"\\path.place.com\batches")]
    public async Task CreateBatchStrategy_Is_FileBatchStrategy(string directory)
    {
        SqlFactory factory = new("dev", directory);
        IBatchStrategy batchStrategy = factory.CreateBatchStrategy();
        Assert.True(batchStrategy is FileBatchStrategy);
    }

    [Theory]
    [InlineData("https://enterprise.es/batches")]
    [InlineData("http://enterprise.es/batches")]
    public async Task CreateBatchStrategy_Is_WebBatchStrategy(string url)
    {
        SqlFactory factory = new("dev", url);
        IBatchStrategy batchStrategy = factory.CreateBatchStrategy();
        Assert.True(batchStrategy is WebBatchStrategy);
    }

    [Theory]
    [InlineData("ftp://enterprise.es/batches")]
    [InlineData("ftps://enterprise.es/batches")]
    [InlineData("sftp://enterprise.es/batches")]
    [InlineData("file://enterprise.es/batches")]
    [InlineData("gopher://enterprise.es/batches")]
    [InlineData("ws://enterprise.es/batches")]
    [InlineData("wss://enterprise.es/batches")]
    [InlineData("mailto://enterprise.es/batches")]
    [InlineData("news://enterprise.es/batches")]
    [InlineData("nntp://enterprise.es/batches")]
    [InlineData("ssh://enterprise.es/batches")]
    [InlineData("telnet://enterprise.es/batches")]
    [InlineData("net.tcp://enterprise.es/batches")]
    [InlineData("net.pipe://enterprise.es/batches")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("")]
    public async Task CreateBatchStrategy_Fails_For_Unknown_Schemes(string url)
    {
        SqlFactory factory = new("dev", url);
        try
        {
            IBatchStrategy batchStrategy = factory.CreateBatchStrategy();
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
