CREATE TABLE [__Version] (
    [VersionId] bigint NOT NULL,
    CONSTRAINT [PK_Version] PRIMARY KEY ([VersionId])
);
INSERT INTO [__UpgradientMigrationHistory] ([MigrationId])
VALUES (1);

INSERT INTO [__Version]([VersionId])
VALUES(0);