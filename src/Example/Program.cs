using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

Logger serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "log.txt")
    .CreateLogger();

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(serilogLogger, dispose: true));
ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);

UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithFileBatchStrategy("Batches")
    .WithCacheManager(options => new FileBatchCacheManager("Cache", options.Logger))
    .WithParallelism(10)
    .WithSourceProvider(options => new FileSourceProvider("Sources.json", options.Logger))
    .AddSqlServerEngine()
    .AddMySqlServerEngine()
    .AddPostgreSqlServerEngine()
    .WithLogger(logger);

UpdateManager updateManager = updateBuilder.Build();
IEnumerable<UpdateResult> updateResults = await updateManager.UpdateAsync();
Console.WriteLine("Press ENTER to exit...");
Console.ReadLine();