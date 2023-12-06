using Microsoft.EntityFrameworkCore;

namespace Upgradier.DatabaseEngines.PostgreSql;

public class PostgreSqlSourceDatabase : SourceDatabase
{
    public PostgreSqlSourceDatabase(DbContextOptions<PostgreSqlSourceDatabase> options, LogAdapter logger, string? environment) 
        : base(options, logger, environment)
    {
    }
}
