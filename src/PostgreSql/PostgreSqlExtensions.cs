using Microsoft.Extensions.DependencyInjection;

namespace Upgradier.PostgreSql;

public static class PostgreSqlExtensions
{
    public static UpdateBuilder AddPostgreSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(options => new PostgreSqlEngine(options.Logger));
        return builder;
    }
}
