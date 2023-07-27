using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Upgradier.DatabaseEngines.SqlServer;

public class SqlEngine : IDatabaseEngine
{
    public const string NAME = "SqlServer";

    private readonly LogAdapter _logger;
    private readonly string? _environment;
    private readonly int? _commandTimeout;
    private readonly int? _connectionTimeout;

    public string Name => NAME;

    public SqlEngine(LogAdapter logger, string? environment, int? commandTimeout, int? connectionTimeout)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThan(commandTimeout.GetValueOrDefault(), 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(connectionTimeout.GetValueOrDefault(), 0);
        _logger = logger;
        _environment = environment;
        _connectionTimeout = connectionTimeout;
    }

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            _logger.LogCreatingLockStrategy(nameof(SqlEngine));
            return new SqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        _logger.LogCreatingSourceDatabase(nameof(SqlSourceDatabase));
        SqlConnectionStringBuilder connectionBuilder = new(connectionString);
        if (_connectionTimeout is not null)
        {
            connectionBuilder.ConnectTimeout = _connectionTimeout.Value;
        }
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString, options => options.CommandTimeout(_commandTimeout));
        return new SqlSourceDatabase(builder.Options, _logger, _environment);
    }
}