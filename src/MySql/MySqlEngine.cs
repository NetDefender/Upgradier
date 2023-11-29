using Microsoft.EntityFrameworkCore;

namespace Upgradier.MySql;

public class MySqlEngine : IDatabaseEngine
{
    public const string NAME = "MySql";

    private LogAdapter _logger;
    private readonly string? _environment;

    public MySqlEngine(LogAdapter logger, string? environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is MySqlSourceDatabase sqlSourceDatabase)
        {
            return new MySqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<MySqlSourceDatabase> builder = new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseMySQL(connectionString);
        return new MySqlSourceDatabase(builder.Options, _logger, _environment);
    }
}
