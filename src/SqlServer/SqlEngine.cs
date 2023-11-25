using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlEngine : IDatabaseEngine
{
    public const string NAME = "SqlServer";
    
    private readonly LogAdapter _logger;

    public string Name => NAME;

    public SqlEngine(LogAdapter logger)
    {
        _logger = logger;
    }

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            return new SqlLockManager(sqlSourceDatabase, _logger);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString);
        return new SqlSourceDatabase(builder.Options, _logger);
    }
}