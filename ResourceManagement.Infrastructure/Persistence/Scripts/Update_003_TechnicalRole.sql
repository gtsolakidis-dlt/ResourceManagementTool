-- Add TechnicalRole column to Roster table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Roster]') AND name = 'TechnicalRole')
BEGIN
    ALTER TABLE Roster ADD TechnicalRole NVARCHAR(100) NULL;
    PRINT 'Added TechnicalRole column to Roster table.';
END
