using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlEngine : IDatabaseEngine
{
    private readonly string? _environment;
    public const string NAME = "SqlServer";

    public SqlEngine(string? environment)
    {
        _environment = environment;
    }

    public string Name => NAME;

    public virtual ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            return new SqlLockStrategy(_environment, sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString);
        return new SqlSourceDatabase(_environment, builder.Options);
    }
}