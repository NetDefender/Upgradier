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
    public async Task Connectionstring_is_not_null()
    {
        _output.WriteLine(_connectionString);
        Assert.NotNull(_connectionString);
    }
}
