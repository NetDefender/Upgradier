using System.Text.Json;

namespace Upgradier.Core;

public class FileScriptStrategy : ScriptStrategyBase
{
    private readonly string _baseDirectory;

    public FileScriptStrategy(string baseDirectory, string provider, string? environment) : base(environment, provider, nameof(FileScriptStrategy))
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        _baseDirectory = baseDirectory;
    }

    public override ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken)
    {
        string scriptsFile = Path.Combine(_baseDirectory, Provider, string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        List<Script>? scripts = JsonSerializer.Deserialize<List<Script>>(scriptsFile);
        ArgumentNullException.ThrowIfNull(scripts);
        ArgumentOutOfRangeException.ThrowIfZero(scripts.Count);
        return ValueTask.FromResult(scripts.AsReadOnly().AsEnumerable());
    }

    public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        string scriptsDirectory = Path.Combine(_baseDirectory, Provider, string.IsNullOrEmpty(Environment) ? string.Empty : Environment);
        return ValueTask.FromResult(File.OpenText(Path.Combine(scriptsDirectory, $"{script.VersionId}.sql")));
    }
}
