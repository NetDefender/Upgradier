using Testcontainers.MariaDb;

namespace Upgradier.Tests.MySql;

public sealed class MySqlDatabaseFixture : IAsyncLifetime
{
    private readonly MariaDbContainer _container;
    public MySqlDatabaseFixture()
    {
        _container = new MariaDbBuilder()
          .Build();
    }

    public string? ConnectionString { get; private set; }

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
