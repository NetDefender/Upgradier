using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NSubstitute;
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

UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithFileSourceProvider(Directory.GetCurrentDirectory(), "Sources.json")
    .WithFileBatchStrategy("Batches")
    .WithCacheManager(options => new FileBatchCacheManager("Cache", options.Logger, options.Environment))
    .WithParallelism(10)
    .WithConnectionTimeout(1)
    .AddSqlServerEngine()
    .AddMySqlServerEngine()
    .AddPostgreSqlServerEngine()
    .WithEnvironment("Dev")
    .WithLogger(logger);

UpdateManager updateManager = updateBuilder.Build();
IEnumerable<UpdateResult> updateResults = await updateManager.UpdateAsync();
Console.WriteLine("Press ENTER to exit...");
Console.ReadLine();