using System.Text.Json;

namespace Upgradier.Core;

public class FileScriptAdapter : ScriptAdapterBase
{
    private readonly string _baseDirectory;

    public FileScriptAdapter(string baseDirectory, string provider, string? environment) : base(environment, provider, nameof(FileScriptAdapter))
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        CoreExtensions.ThrowIfDirectoryNotExists(baseDirectory);
        _baseDirectory = baseDirectory;
    }

    public override ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken)
    {
        string scriptsFile = Path.Combine(_baseDirectory, Provider, string.IsNullOrEmpty(Environment) ? "Scripts.json" : $"Scripts.{Environment}.json");
        List<Script>? scripts = JsonSerializer.Deserialize<List<Script>>(scriptsFile);
        ArgumentNullException.ThrowIfNull(scripts);
        ArgumentOutOfRangeException.ThrowIfZero(scripts.Count);
        return ValueTask.FromResult(scripts.AsReadOnly().AsEnumerable());
    }

    public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        string scriptsDirectory = Path.Combine(_baseDirectory, Provider, string.IsNullOrEmpty(Environment) ? "Scripts" : $"Scripts.{Environment}");
        return ValueTask.FromResult(File.OpenText(Path.Combine(scriptsDirectory, $"{script.VersionId}.sql")));
    }
}
