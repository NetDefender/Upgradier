using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Upgradier.MySql;

public class MySqlEngine : IDatabaseEngine
{
    public const string NAME = "MySql";

    private LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;
    private readonly int? _connectionTimeout;

    public MySqlEngine(LogAdapter logger, string? environment, int? commandTimeout, int? connectionTimeout)
    {
        _logger = logger;
        _environment = environment;
        if (commandTimeout is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(commandTimeout.Value, 0);
        }
        _commandTimeout = commandTimeout;
        if (connectionTimeout is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(connectionTimeout.Value, 0);
        }
        _connectionTimeout = connectionTimeout;
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
        MySqlConnectionStringBuilder connectionBuilder = new (connectionString);
        if(_connectionTimeout is not null)
        {
            connectionBuilder.ConnectionTimeout = (uint)_connectionTimeout.Value;
        }
        DbContextOptionsBuilder<MySqlSourceDatabase> optionsBuilder = new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionBuilder.ConnectionString), options => options.CommandTimeout(_commandTimeout));
        return new MySqlSourceDatabase(optionsBuilder.Options, _logger, _environment);
    }
}
