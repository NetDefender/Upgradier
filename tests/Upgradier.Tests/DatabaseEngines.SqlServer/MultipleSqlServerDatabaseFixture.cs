using Testcontainers.MsSql;

namespace Upgradier.Tests.DatabaseEngines.SqlServer;

public sealed class MultipleSqlServerDatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _containerOne;
    private readonly MsSqlContainer _containerTwo;

    public MultipleSqlServerDatabaseFixture()
    {
        _containerOne = new MsSqlBuilder()
          .Build();
        _containerTwo = new MsSqlBuilder()
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
