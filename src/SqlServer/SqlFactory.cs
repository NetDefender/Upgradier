using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlFactory : IProviderFactory
{
    private readonly string? _environment;
    private readonly string _scriptsDirectoryOrBaseUrl;
    public const string NAME = "SqlServer";

    public SqlFactory(string? environment, string scriptsDirectoryOrBaseUrl)
    {
        _environment = environment;
        _scriptsDirectoryOrBaseUrl = scriptsDirectoryOrBaseUrl;
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

    public virtual IScriptStragegy CreateScriptStrategy()
    {
        if (_scriptsDirectoryOrBaseUrl.IsNullOrEmptyOrWhiteSpace())
        {
            throw new InvalidOperationException($"There is not a {nameof(IScriptStragegy)} for null scheme. {nameof(_scriptsDirectoryOrBaseUrl)} is null.");
        }
        if (_scriptsDirectoryOrBaseUrl.TryCreateUri(out Uri? uri))
        {
            if (!uri.IsHttpScheme())
            {
                throw new InvalidOperationException($"There is not a {nameof(IScriptStragegy)} for {uri?.Scheme ?? "unknown"} scheme");
            }
            return new WebScriptStrategy(_scriptsDirectoryOrBaseUrl, Name, _environment);
        }
        return new FileScriptStrategy(_scriptsDirectoryOrBaseUrl, Name, _environment);
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString);
        return new SqlSourceDatabase(_environment, builder.Options);
    }
}