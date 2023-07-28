# Upgradier

A minimalist approach for updating multiple databases to a version based in convention SQL scripts

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Usage](#usage)

## Prerequisites
- [.NET SDK 8.0 or later](https://www.microsoft.com/net/download)


## Quick start

- Install [Upgradier.Core](https://www.nuget.org/packages/Upgradier.Core)
- Install the required providers
    - [Upgradier.SqlServer](https://www.nuget.org/packages/Upgradier.SqlServer)

## Architecture


```mermaid
erDiagram
    UpdateBuilder||--|| IUpdateManager: creates
    UpdateBuilder||--|| Options: has
    IUpdateManager ||--|| Environment:has
    IUpdateManager ||--|{ Provider :has
    Provider ||--|| Name :has
    Provider ||--|| ILockStrategy :creates
    Provider ||--|| IScriptAdapter :creates
    Provider ||--|| SourceDatabase :creates
```