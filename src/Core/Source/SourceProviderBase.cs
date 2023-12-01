namespace Upgradier.Core;

public abstract class SourceProviderBase : ISourceProvider
{
    protected SourceProviderBase(string name, LogAdapter logger, string? environment)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(logger);
        Name = name;
        Logger = logger;
        Environment = environment;
    }

    public string Name { get; }

    protected LogAdapter Logger { get; }

    public string? Environment { get; }

    public abstract Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}