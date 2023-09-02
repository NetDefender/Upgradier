using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Upgradier.Tests.Core;

public sealed class BatchServerFixture : IAsyncLifetime
{
    private readonly IContainer _container;
    private const int CONTAINER_PORT = 80;

    public BatchServerFixture()
    {
        _container = new ContainerBuilder()
          .WithName(Guid.NewGuid().ToString("D"))
          .WithImage("nginx")
          .WithPortBinding(CONTAINER_PORT, true)
          .WithResourceMapping(new DirectoryInfo($"Core{Path.DirectorySeparatorChar}Batches"), "/usr/share/nginx/html/")
          .Build();
    }

    public Uri? BatchesUri { get; private set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync().ConfigureAwait(false);
        BatchesUri = new UriBuilder("http", _container.Hostname, _container.GetMappedPublicPort(CONTAINER_PORT)).Uri;
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}
