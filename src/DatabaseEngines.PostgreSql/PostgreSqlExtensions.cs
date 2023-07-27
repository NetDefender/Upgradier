namespace Upgradier.DatabaseEngines.PostgreSql;

public static class PostgreSqlExtensions
{
    public static UpdateBuilder AddPostgreSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(options => new PostgreSqlEngine(options.Logger, options.Environment, options.CommandTimeout, options.ConnectionTimeout));
        return builder;
    }
}
