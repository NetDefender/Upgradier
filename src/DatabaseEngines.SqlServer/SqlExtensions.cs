namespace Upgradier.DatabaseEngines.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(options => new SqlEngine(options.Logger, options.Environment, options.CommandTimeout, options.ConnectionTimeout));
        return builder;
    }
}