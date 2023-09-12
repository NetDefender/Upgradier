namespace Upgradier.Core;

public abstract class SourceProviderBase : ISourceProvider
{
    protected SourceProviderBase(string? environment, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Environment = environment;
        Name = name;
    }

    public string Name { get; }

    public string? Environment { get; }

    public abstract Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}

public interface ISourceProvider
{
    string Name { get; }

    Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}