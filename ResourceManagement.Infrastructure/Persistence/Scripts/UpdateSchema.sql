IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMonthlySnapshot]') AND name = 'OriginalOpeningBalance')
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalOpeningBalance DECIMAL(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMonthlySnapshot]') AND name = 'OriginalCumulativeBillings')
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalCumulativeBillings DECIMAL(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMonthlySnapshot]') AND name = 'OriginalWip')
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalWip DECIMAL(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMonthlySnapshot]') AND name = 'OriginalDirectExpenses')
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalDirectExpenses DECIMAL(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMonthlySnapshot]') AND name = 'OriginalOperationalCost')
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalOperationalCost DECIMAL(18,2) NULL;
END
