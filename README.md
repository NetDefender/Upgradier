# Upgradier [![Packages](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml/badge.svg)](https://github.com/NetDefender/Ugradier/actions/workflows/packages.yml) ![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/NetDefender/d51c51b9b1e64ce740782fe8db02a889/raw/code-coverage-upgradier.json)

A minimalist approach for updating multiple databases concurrently to a version based in conventions.

[![Frozen Penguin](https://github.com/NetDefender/Ugradier/blob/master/Upgradier.png)](https://github.com/NetDefender/Ugradier)

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Usage](#usage)

## Prerequisites
- [.NET SDK 8.0 or later](https://www.microsoft.com/net/download)

## Quick start

- Install [Upgradier.Core](https://www.nuget.org/packages/Upgradier.Core)
- Install the required database engines:
    - [Upgradier.SqlServer](https://www.nuget.org/packages/Upgradier.SqlServer)
- Install aditional batch strategies:
    - [Upgradier.BatchStrategy.Aws](https://www.nuget.org/packages/Upgradier.BatchStrategy.Aws)
    - [Upgradier.BatchStrategy.Azure](https://www.nuget.org/packages/Upgradier.BatchStrategy.Azure)

## Example
Explore a classic use with SqlServer

## Architecture

```mermaid
erDiagram
    UpdateBuilder||--|| IUpdateManager: creates-one
    UpdateBuilder||--|| Options: has-one
    IUpdateManager ||--|| Environment:has-one
    IUpdateManager ||--|| ISourceProvider:has-one
    ISourceProvider ||--|{ Source :get-many
    IUpdateManager ||--|{ IDatabaseEngine :has-many
    IUpdateManager ||--|| IBatchStrategy :has-one
    IDatabaseEngine ||--|| Name :has-one
    IDatabaseEngine ||--|| ILockStrategy :creates-one
    IDatabaseEngine ||--|| SourceDatabase :creates-many
```
