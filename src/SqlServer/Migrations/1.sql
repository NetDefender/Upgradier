CREATE TABLE [__Version] (
    [__VersionId] bigint NOT NULL,
    CONSTRAINT [PK___Version] PRIMARY KEY ([__VersionId])
);
INSERT INTO [__UpgradientMigrationHistory] ([MigrationId])
VALUES (1);