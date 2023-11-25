using System.Text.Json;

namespace Upgradier.Core;

public class FileBatchStrategy : BatchStrategyBase
{
    private readonly string _baseDirectory;

    public FileBatchStrategy(string baseDirectory, LogAdapter logger) : base(nameof(FileBatchStrategy), logger)
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

    public override Task<string> GetBatchContentsAsync(Batch batch, string provider,CancellationToken cancellationToken)
    {
        string batchesFile = Path.Combine(_baseDirectory, provider, Environment.EmptyIfNull(), $"{batch.VersionId}.sql");
        return Task.FromResult(File.ReadAllText(batchesFile));
    }
}
