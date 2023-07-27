namespace Upgradier.Core;

public abstract class ScriptAdapterBase : IScriptAdapter
{
    protected ScriptAdapterBase(string? environment, string provider, string name)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(name);
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

public interface IScriptAdapter
{
    string Name { get; }
    ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken);
    ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken);
}