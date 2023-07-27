using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public sealed class SqlSourceDatabase : SourceDatabase
{
    public SqlSourceDatabase(string? environment, DbContextOptions<SqlSourceDatabase> options) : base(environment, options)
    {
    }
}
