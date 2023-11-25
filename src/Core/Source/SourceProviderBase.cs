namespace Upgradier.Core;

public abstract class SourceProviderBase : ISourceProvider
{
    protected SourceProviderBase(string name, LogAdapter logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        Logger = logger;
        Environment = EnvironmentVariables.GetExecutionEnvironment();
    }

    public string Name { get; }

    protected LogAdapter Logger { get; }

    public string? Environment { get; }

    public abstract Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}