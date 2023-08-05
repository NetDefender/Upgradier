namespace Upgradier.Core;

public abstract class SourceProviderBase : ISourceProvider
{
    protected SourceProviderBase(string? environment, string name)
    {
        Environment = environment;
        Name = name;
    }
    public string Name { get; }
    protected string? Environment { get; }
    public abstract ValueTask<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}    

public interface ISourceProvider
{
    string Name { get; }
    ValueTask<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}