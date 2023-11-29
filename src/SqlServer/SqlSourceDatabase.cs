using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public sealed class SqlSourceDatabase : SourceDatabase
{
    public SqlSourceDatabase(DbContextOptions<SqlSourceDatabase> options, LogAdapter logger, string? environment) 
        : base(options, logger, environment)
    {
    }
}
