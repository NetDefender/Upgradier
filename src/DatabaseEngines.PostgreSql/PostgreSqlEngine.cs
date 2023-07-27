using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Upgradier.DatabaseEngines.PostgreSql;

public class PostgreSqlEngine : IDatabaseEngine
{
    public const string NAME = "PostgreSql";

    private readonly LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;
    private readonly int? _connectionTimeout;

    public PostgreSqlEngine(LogAdapter logger, string? environment, int? commandTimeout, int? connectionTimeout)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThan(connectionTimeout.GetValueOrDefault(), 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(commandTimeout.GetValueOrDefault(), 0);
        _logger = logger;
        _environment = environment;
        _commandTimeout = commandTimeout;
        _connectionTimeout = connectionTimeout;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is PostgreSqlSourceDatabase sqlSourceDatabase)
        {
            _logger.LogCreatingLockStrategy(nameof(PostgreSqlLockManager));
            return new PostgreSqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        _logger.LogCreatingSourceDatabase(nameof(PostgreSqlSourceDatabase));
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
