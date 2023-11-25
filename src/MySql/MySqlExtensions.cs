namespace Upgradier.MySql;

public static class MySqlExtensions
{
    public static UpdateBuilder AddMySqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(options => new MySqlEngine(options.Logger));
        return builder;
    }
}
