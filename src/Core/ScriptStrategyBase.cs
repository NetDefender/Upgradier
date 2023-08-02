namespace Upgradier.Core;

public abstract class ScriptStrategyBase : IScriptStragegy
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
    public abstract ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken);
    public abstract ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken);
}

public interface IScriptStragegy
{
    string Name { get; }
    ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken);
    ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken);
}