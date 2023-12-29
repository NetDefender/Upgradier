# Upgradier [![Packages](https://github.com/NetDefender/Upgradier/actions/workflows/packages.yml/badge.svg)](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml) [![codecov](https://codecov.io/gh/NetDefender/Upgradier/graph/badge.svg?token=4OTDF483FW)](https://codecov.io/gh/NetDefender/Upgradier)

A minimalist approach for updating multiple databases concurrently to a version based in conventions.

Declare a place to store de Source databases in json format. You can mix databases of different providers (actually SqlServer, MySql and PostgreSql).

Store batches in .sql files on Disk, Web Server, Aws S3, Azure Blob, or custom IBatchStrategy.

Optionally use a cache implementing IBatchCacheManager to not hit the server when each batch is requested.

[![Frozen Penguin](https://raw.githubusercontent.com/NetDefender/Upgradier/master/Upgradier.png)](https://github.com/NetDefender/Upgradier)

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)

## Prerequisites
- [.NET SDK 8.0 or later](https://www.microsoft.com/net/download)

## Quick start

- Install [Upgradier.Core](https://www.nuget.org/packages/Upgradier.Core)
- Install the required database engines:
    - [Upgradier.DatabaseEngines.SqlServer](https://www.nuget.org/packages/Upgradier.DatabaseEngines.SqlServer)
	- [Upgradier.DatabaseEngines.PostgreSql](https://www.nuget.org/packages/Upgradier.DatabaseEngines.PostgreSql)
	- [Upgradier.DatabaseEngines.MySql](https://www.nuget.org/packages/Upgradier.DatabaseEngines.MySql)
- Install aditional batch strategies:
    - [Upgradier.BatchStrategies.Aws](https://www.nuget.org/packages/Upgradier.BatchStrategies.Aws)
    - [Upgradier.BatchStrategies.Azure](https://www.nuget.org/packages/Upgradier.BatchStrategies.Azure)

Create UpdateBuilder with options:

```csharp
//Microsoft.Extensions.Logging.ILogger logger ...

BlobContainerClient blobContainerClient = ... // Optional use Azure

UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithSourceProvider(options => new FileSourceProvider("c:\\my_files\\sources.json", options.Logger, options.Environment))
    .WithCacheManager(options => new FileBatchCacheManager("c:\\my_files\\cache", options.Logger, options.Environment))
    .WithAzureBlobBatchStrategy(blobContainerClient)
    .AddSqlServerEngine()
    .AddMySqlServerEngine()
    .AddPostgreSqlServerEngine()
    .WithLogger(logger)
    .WithEnvironment("Dev");
```
Build to create the UpdateManager:

```csharp
UpdateManager updateManager = updateBuilder.Build();
```

Update the databases:
```csharp
IEnumerable<UpdateResult> updateResults = await updateManager.UpdateAsync();
```
