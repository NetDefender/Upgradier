using Testcontainers.MariaDb;

namespace Upgradier.Tests.DatabaseEngines.MySql;

public sealed class MultipleMySqlDatabaseFixture : IAsyncLifetime
{
    private readonly MariaDbContainer _containerOne;
    private readonly MariaDbContainer _containerTwo;

    public MultipleMySqlDatabaseFixture()
    {
        _containerOne = new MariaDbBuilder()
          .Build();
        _containerTwo = new MariaDbBuilder()
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