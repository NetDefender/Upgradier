using Testcontainers.PostgreSql;

namespace Upgradier.Tests.DatabaseEngines.PostgreSql;

public sealed class MultiplePostgreSqlDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _containerOne;
    private readonly PostgreSqlContainer _containerTwo;

    public MultiplePostgreSqlDatabaseFixture()
    {
        _containerOne = new PostgreSqlBuilder()
          .Build();
        _containerTwo = new PostgreSqlBuilder()
          .Build();
    }

    public string ConnectionStringOne { get; private set; }

    public string ConnectionStringTwo { get; private set; }

    public async Task InitializeAsync()
    {
        await _containerOne.StartAsync().ConfigureAwait(false);
        ConnectionStringOne = _containerOne.GetConnectionString();

        await _containerTwo.StartAsync().ConfigureAwait(false);
        ConnectionStringTwo = _containerTwo.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _containerOne.DisposeAsync().ConfigureAwait(false);
        await _containerTwo.DisposeAsync().ConfigureAwait(false);
    }
}
