namespace Upgradier.PostgreSql;

public static class PostgreSqlExtensions
{
    public static UpdateBuilder AddPostgreSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(new PostgreSqlEngine());
        return builder;
    }
}
