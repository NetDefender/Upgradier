using Microsoft.EntityFrameworkCore;

namespace Upgradier.PostgreSql;

public class PostgreSqlEngine : IDatabaseEngine
{
    public const string NAME = "PostgreSql";

    private LogAdapter _logger;

    public PostgreSqlEngine(LogAdapter logger)
    {
        _logger = logger;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is PostgreSqlSourceDatabase sqlSourceDatabase)
        {
            return new PostgreSqlLockManager(sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<PostgreSqlSourceDatabase> builder = new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(connectionString);
        return new PostgreSqlSourceDatabase(builder.Options);
    }
}
