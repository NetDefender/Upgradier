namespace Upgradier.Core;

public abstract class SourceAdapterBase : ISourceAdapter
{
    protected SourceAdapterBase(string? environment, string name)
    {
        Environment = environment;
        Name = name;
    }
    public string Name { get; }
    protected string? Environment { get; }
    public abstract ValueTask<IEnumerable<Source>> GetAllSourcesAsync(CancellationToken cancellationToken);
}    

public interface ISourceAdapter
{
    string Name { get; }
    ValueTask<IEnumerable<Source>> GetAllSourcesAsync(CancellationToken cancellationToken);
}