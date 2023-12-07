using Testcontainers.LocalStack;

namespace Upgradier.Tests.BatchStrategies.Aws;

public class AwsS3Fixture : IAsyncLifetime
{
    private readonly LocalStackContainer _container;
    private const string AwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
    private const string AwsSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

    static AwsS3Fixture()
    {
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", AwsAccessKey);
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", AwsSecretKey);
    }

    public AwsS3Fixture()
    {
        _container = new LocalStackBuilder()
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
