using Testcontainers.MsSql;

namespace Upgradier.Tests.DatabaseEngines.SqlServer;

public sealed class SqlServerDatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerDatabaseFixture()
    {
        _container = new MsSqlBuilder()
          .Build();
    }

    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync().ConfigureAwait(false);
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}
