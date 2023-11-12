namespace Upgradier.Core;

public abstract class BatchStrategyBase : IBatchStrategy
{
    protected BatchStrategyBase(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        Environment = EnvironmentVariables.GetExecutionEnvironment();
    }

    public string Name { get; }

    public string? Environment { get; }

    public abstract Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    public abstract Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken);
}
