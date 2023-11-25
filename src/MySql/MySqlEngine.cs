using Microsoft.EntityFrameworkCore;

namespace Upgradier.MySql;

public class MySqlEngine : IDatabaseEngine
{
    public const string NAME = "MySql";

    private LogAdapter _logger;

    public MySqlEngine(LogAdapter logger)
    {
        _logger = logger;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is MySqlSourceDatabase sqlSourceDatabase)
        {
            return new MySqlLockManager(sqlSourceDatabase, _logger);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<MySqlSourceDatabase> builder = new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseMySQL(connectionString);
        return new MySqlSourceDatabase(builder.Options, _logger);
    }
}
