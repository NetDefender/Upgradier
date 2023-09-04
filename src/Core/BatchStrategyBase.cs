namespace Upgradier.Core;

public abstract class BatchStrategyBase : IBatchStrategy
{
    protected BatchStrategyBase(string? environment, string provider, string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(provider);
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        Name = name;
        Provider = provider;
        Environment = environment;
    }

    public string Name { get; }

    public string? Environment { get; }

    public string Provider { get; }

    public abstract ValueTask<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    public abstract ValueTask<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken);
}
