namespace Upgradier.Core;

public abstract class ScriptStrategyBase : IScriptStrategy
{
    protected ScriptStrategyBase(string? environment, string provider, string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(provider);
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        Name = name;
        Provider = provider;
        Environment = environment;
    }
    public string Name { get; }
    protected string? Environment { get; }
    public string Provider { get; }
    public abstract ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken);
    public abstract ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken);
}
