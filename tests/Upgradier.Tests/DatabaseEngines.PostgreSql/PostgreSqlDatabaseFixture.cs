using Testcontainers.PostgreSql;

namespace Upgradier.Tests.DatabaseEngines.PostgreSql;

public sealed class PostgreSqlDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public PostgreSqlDatabaseFixture()
    {
        _container = new PostgreSqlBuilder()
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
