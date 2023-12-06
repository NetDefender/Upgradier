IF OBJECT_ID(N'[__UpgradientMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [__UpgradientMigrationHistory] (
        [MigrationId] int NOT NULL,
        CONSTRAINT [PK___UpgradientMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;