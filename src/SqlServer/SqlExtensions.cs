namespace Upgradier.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(options => new SqlEngine(options.Logger));
        return builder;
    }
}