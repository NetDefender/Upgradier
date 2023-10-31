using System.Text.Json;

namespace Upgradier.Core;

public class FileBatchStrategy : BatchStrategyBase
{
    private readonly string _baseDirectory;

    public FileBatchStrategy(string baseDirectory) : base(nameof(FileBatchStrategy))
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        _baseDirectory = !Path.EndsInDirectorySeparator(path: baseDirectory) ? baseDirectory + Path.DirectorySeparatorChar : baseDirectory;
    }

    public override Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
    {
        string batchesFile = Path.Combine(_baseDirectory, string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        using FileStream batchesStream = new (batchesFile, FileMode.Open);
        List<Batch>? batches = JsonSerializer.Deserialize<List<Batch>>(batchesStream);
        ArgumentNullException.ThrowIfNull(batches);
        return Task.FromResult(batches.AsReadOnly().AsEnumerable());
    }

    public override Task<StreamReader> GetBatchContentsAsync(Batch batch, string provider,CancellationToken cancellationToken)
    {
        string batchesDirectory = Path.Combine(_baseDirectory, provider, string.IsNullOrEmpty(Environment) ? string.Empty : Environment);
        return Task.FromResult(File.OpenText(Path.Combine(batchesDirectory, $"{batch.VersionId}.sql")));
    }
}
