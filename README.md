# Upgradier - minimalist multiple database updater

A minimalist approach for updating multiple databases to a version based in convention SQL batches

[![Packages](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml/badge.svg)](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/NetDefender/d51c51b9b1e64ce740782fe8db02a889/raw/code-coverage-upgradier.json)

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
    Provider ||--|| IBatchStrategy :creates
    Provider ||--|| SourceDatabase :creates
```