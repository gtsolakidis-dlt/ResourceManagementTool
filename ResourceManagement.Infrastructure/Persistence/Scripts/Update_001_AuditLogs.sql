USE [ResourceManagementDb];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [EntityName] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(50) NOT NULL, -- Create, Update, Delete, Confirm
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [ChangedBy] NVARCHAR(100) NOT NULL,
        [ChangedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AuditLogs_Entity ON [dbo].[AuditLogs] (EntityName, EntityId);
END
GO
