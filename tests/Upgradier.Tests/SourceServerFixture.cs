using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Upgradier.Tests;

public class SourceServerFixture : IAsyncLifetime
{
    private readonly IContainer _container;
    private const int CONTAINER_PORT = 80;

    public SourceServerFixture()
    {
        _container = new ContainerBuilder()
          .WithName(Guid.NewGuid().ToString("D"))
          .WithImage("nginx")
          .WithPortBinding(CONTAINER_PORT, true)
          .WithResourceMapping(new DirectoryInfo($"Core{Path.DirectorySeparatorChar}Files"), "/usr/share/nginx/html/")
          .Build();
    }

    public Uri? SourcesUri { get; private set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        SourcesUri = new UriBuilder("http", _container.Hostname, _container.GetMappedPublicPort(CONTAINER_PORT)).Uri;
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
