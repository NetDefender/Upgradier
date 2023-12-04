using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Upgradier.PostgreSql;

public class PostgreSqlEngine : IDatabaseEngine
{
    public const string NAME = "PostgreSql";

    private LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;
    private readonly int? _connectionTimeout;

    public PostgreSqlEngine(LogAdapter logger, string? environment, int? commandTimeout, int? connectionTimeout)
    {
        ArgumentNullException.ThrowIfNull(logger);
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
        if (sourceDatabase is PostgreSqlSourceDatabase sqlSourceDatabase)
        {
            return new PostgreSqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        NpgsqlConnectionStringBuilder connectionBuilder = new(connectionString);
        if (_connectionTimeout is not null)
        {
            connectionBuilder.Timeout = _connectionTimeout.Value;
        }
        DbContextOptionsBuilder<PostgreSqlSourceDatabase> builder = new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(connectionString, options => options.CommandTimeout(_commandTimeout));
        return new PostgreSqlSourceDatabase(builder.Options, _logger, _environment);
    }
}
