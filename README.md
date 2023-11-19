# Upgradier [![Packages](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml/badge.svg)](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml) ![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/NetDefender/d51c51b9b1e64ce740782fe8db02a889/raw/code-coverage-upgradier.json)

A minimalist approach for updating multiple databases concurrently to a version based in conventions.

Declare a place to store de Source databases in json format. You can mix databases of different providers (SqlServer, MySql and PostgreSql).

Store batches in .sql files on Disk, Web Server, Aws S3, Azure Blob, or custom IBatchStrategy.

Optionally use a cache implementing IBatchCacheManager to not hit the server when each batch is requested.

[![Frozen Penguin](https://github.com/NetDefender/Ugradier/blob/master/Upgradier.png)](https://github.com/NetDefender/Ugradier)

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)

## Prerequisites
- [.NET SDK 8.0 or later](https://www.microsoft.com/net/download)

## Quick start

- Install [Upgradier.Core](https://www.nuget.org/packages/Upgradier.Core)
- Install the required database engines:
    - [Upgradier.SqlServer](https://www.nuget.org/packages/Upgradier.SqlServer)
	- [Upgradier.PostgreSql](https://www.nuget.org/packages/Upgradier.PostgreSql)
	- [Upgradier.MySql](https://www.nuget.org/packages/Upgradier.MySql)
- Install aditional batch strategies:
    - [Upgradier.BatchStrategy.Aws](https://www.nuget.org/packages/Upgradier.BatchStrategy.Aws)
    - [Upgradier.BatchStrategy.Azure](https://www.nuget.org/packages/Upgradier.BatchStrategy.Azure)

First set the environment, "Dev" in the example. Environment is used to get the batches and sources:
```csharp
EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
```

Create UpdateBuilder with options:

```csharp
UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithSourceProvider(new FileSourceProvider("c:\\my_files\\sources.json"))
    .WithFileBatchStrategy("c:\\my_files\\batches")
    .WithCacheManager(new FileBatchCacheManager("c:\\my_files\\cache"))
    .AddSqlServerEngine()
    .AddMySqlServerEngine()
    .AddPostgreSqlServerEngine();
```
Build to create the UpdateManager:

```csharp
UpdateManager updateManager = updateBuilder.Build();
```

Update the databases:
```csharp
IEnumerable<UpdateResult> updateResults = await updateManager.Update();
```