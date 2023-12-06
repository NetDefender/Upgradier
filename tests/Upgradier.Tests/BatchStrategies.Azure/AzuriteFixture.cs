using DotNet.Testcontainers.Builders;
using Testcontainers.Azurite;

namespace Upgradier.Tests.BatchStrategies.Azure;

public class AzuriteFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _container;
    public AzuriteFixture()
    {
        _container = new AzuriteBuilder()
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
