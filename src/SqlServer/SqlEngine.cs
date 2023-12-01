using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlEngine : IDatabaseEngine
{
    public const string NAME = "SqlServer";
    
    private readonly LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;

    public string Name => NAME;

    public SqlEngine(LogAdapter logger, string? environment, int? commandTimeout)
    {
        _logger = logger;
        _environment = environment;
        _commandTimeout = commandTimeout;
    }

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            return new SqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString, options => options.CommandTimeout(_commandTimeout));
        return new SqlSourceDatabase(builder.Options, _logger, _environment);
    }
}