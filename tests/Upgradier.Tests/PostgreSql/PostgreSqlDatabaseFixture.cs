using Testcontainers.PostgreSql;

namespace Upgradier.Tests.PostgreSql;

public sealed class PostgreSqlServerDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public PostgreSqlServerDatabaseFixture()
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
