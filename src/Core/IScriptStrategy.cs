namespace Upgradier.Core;

public interface IScriptStrategy
{
    string Name { get; }
    ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken);
    ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken);
}