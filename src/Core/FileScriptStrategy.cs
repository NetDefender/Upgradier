using System.Text.Json;

namespace Upgradier.Core;

public class FileScriptStrategy : ScriptStrategyBase
{
    private readonly string _baseDirectory;

    public FileScriptStrategy(string baseDirectory, string provider, string? environment) : base(environment, provider, nameof(FileScriptStrategy))
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        _baseDirectory = !Path.EndsInDirectorySeparator(path: baseDirectory) ? baseDirectory + Path.DirectorySeparatorChar : baseDirectory;
    }

    public override ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken)
    {
        string scriptsFile = Path.Combine(_baseDirectory, string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        using FileStream scriptsStream = new (scriptsFile, FileMode.Open);
        List<Script>? scripts = JsonSerializer.Deserialize<List<Script>>(scriptsStream);
        ArgumentNullException.ThrowIfNull(scripts);
        return ValueTask.FromResult(scripts.AsReadOnly().AsEnumerable());
    }

    public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        string scriptsDirectory = Path.Combine(_baseDirectory, Provider, string.IsNullOrEmpty(Environment) ? string.Empty : Environment);
        return ValueTask.FromResult(File.OpenText(Path.Combine(scriptsDirectory, $"{script.VersionId}.sql")));
    }
}
