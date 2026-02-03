USE [ResourceManagementDb];
GO

-- Create ApiRequestLogs table for HTTP request/response audit logging
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApiRequestLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApiRequestLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CorrelationId] UNIQUEIDENTIFIER NOT NULL,
        [RequestTimestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ResponseTimestamp] DATETIME2 NULL,
        [DurationMs] INT NULL,
        [HttpMethod] NVARCHAR(10) NOT NULL,
        [RequestPath] NVARCHAR(500) NOT NULL,
        [QueryString] NVARCHAR(2000) NULL,
        [RequestBody] NVARCHAR(MAX) NULL,
        [ResponseStatusCode] INT NULL,
        [ResponseBody] NVARCHAR(MAX) NULL,
        [UserId] INT NULL,
        [Username] NVARCHAR(100) NULL,
        [UserRole] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [ExceptionMessage] NVARCHAR(MAX) NULL,
        [ExceptionStack] NVARCHAR(MAX) NULL
    );

    -- Index for querying by timestamp (most common query pattern)
    CREATE INDEX IX_ApiRequestLogs_Timestamp ON [dbo].[ApiRequestLogs] ([RequestTimestamp] DESC);
    
    -- Index for looking up specific requests by correlation ID
    CREATE INDEX IX_ApiRequestLogs_CorrelationId ON [dbo].[ApiRequestLogs] ([CorrelationId]);
    
    -- Index for filtering by username
    CREATE INDEX IX_ApiRequestLogs_Username ON [dbo].[ApiRequestLogs] ([Username]);
    
    -- Index for filtering by status code (useful for finding errors)
    CREATE INDEX IX_ApiRequestLogs_StatusCode ON [dbo].[ApiRequestLogs] ([ResponseStatusCode]);
    
    -- Index for filtering by request path
    CREATE INDEX IX_ApiRequestLogs_Path ON [dbo].[ApiRequestLogs] ([RequestPath]);

    PRINT 'ApiRequestLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'ApiRequestLogs table already exists.';
END
GO
