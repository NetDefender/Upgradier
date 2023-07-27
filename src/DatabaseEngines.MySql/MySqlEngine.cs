using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Upgradier.DatabaseEngines.MySql;

public class MySqlEngine : IDatabaseEngine
{
    public const string NAME = "MySql";

    private readonly LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;
    private readonly int? _connectionTimeout;

    public MySqlEngine(LogAdapter logger, string? environment, int? commandTimeout, int? connectionTimeout)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThan(commandTimeout.GetValueOrDefault(), 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(connectionTimeout.GetValueOrDefault(), 0);
        _logger = logger;
        _environment = environment;
        _commandTimeout = commandTimeout;
        _connectionTimeout = connectionTimeout;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is MySqlSourceDatabase sqlSourceDatabase)
        {
            _logger.LogCreatingLockStrategy(nameof(MySqlLockManager));
            return new MySqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        _logger.LogCreatingSourceDatabase(nameof(MySqlSourceDatabase));
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
