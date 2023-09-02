using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlFactory : IProviderFactory
{
    private readonly string? _environment;
    private readonly string _batchesDirectoryOrBaseUrl;
    public const string NAME = "SqlServer";

    public SqlFactory(string? environment, string batchesDirectoryOrBaseUrl)
    {
        _environment = environment;
        _batchesDirectoryOrBaseUrl = batchesDirectoryOrBaseUrl;
    }
    public string Name => NAME;

    public virtual ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            return new SqlLockStrategy(_environment, sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual IBatchStrategy CreateBatchStrategy()
    {
        if (_batchesDirectoryOrBaseUrl.IsNullOrEmptyOrWhiteSpace())
        {
            throw new InvalidOperationException($"There is not a {nameof(IBatchStrategy)} for null scheme. {nameof(_batchesDirectoryOrBaseUrl)} is null.");
        }
        if (_batchesDirectoryOrBaseUrl.TryCreateUri(out Uri? uri))
        {
            if (!uri.IsHttpScheme())
            {
                throw new InvalidOperationException($"There is not a {nameof(IBatchStrategy)} for {uri?.Scheme ?? "unknown"} scheme");
            }
            return new WebBatchStrategy(new Uri(_batchesDirectoryOrBaseUrl, UriKind.Absolute), Name, _environment);
        }
        return new FileBatchStrategy(_batchesDirectoryOrBaseUrl, Name, _environment);
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString);
        return new SqlSourceDatabase(_environment, builder.Options);
    }
}