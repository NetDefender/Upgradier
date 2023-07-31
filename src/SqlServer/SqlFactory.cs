using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlFactory : IProviderFactory
{
    private readonly string? _environment;
    private readonly string _scriptsDirectoryOrBaseUrl;

    public SqlFactory(string? environment, string scriptsDirectoryOrBaseUrl)
    {
        _environment = environment;
        _scriptsDirectoryOrBaseUrl = scriptsDirectoryOrBaseUrl;
    }
    public string Name => "SqlServer";

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
        if (_scriptsDirectoryOrBaseUrl.IsAbsoluteUri())
        {
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
