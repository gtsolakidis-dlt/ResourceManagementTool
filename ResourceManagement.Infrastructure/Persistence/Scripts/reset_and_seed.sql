-- =========================================================================================
-- SCRIPT: reset_and_seed.sql
-- PURPOSE: Resets transactional data for testing and seeds new projects with dependent entities.
-- WARNING: This deletes ALL data from Project, key financial tables, and Forecasts.
--          It PRESERVES the Roster (Resources).
-- =========================================================================================

PRINT 'Starting Database Reset and Seed process...'

-- 1. DISABLE CONSTRAINTS & CLEANUP
-- =========================================================================================
PRINT 'Cleaning up existing transactional data...'

-- Disable constraints to allow deletion order flexibility
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

BEGIN TRY
    BEGIN TRANSACTION;

        -- Delete in reverse dependency order to be safe
        DELETE FROM ResourceAllocation;
        DELETE FROM ProjectMonthlySnapshot;
        DELETE FROM Billing;
        DELETE FROM Expense;
        DELETE FROM ProjectRate;
        DELETE FROM Override;
        DELETE FROM ForecastVersion;
        DELETE FROM Project;
        DELETE FROM GlobalRate;
        
        -- NOTE: Roster is PRESERVED.
        -- DELETE FROM Roster; 

        -- Reseed Identity columns so new IDs start from 1
        DBCC CHECKIDENT ('Project', RESEED, 0);
        DBCC CHECKIDENT ('ForecastVersion', RESEED, 0);
        DBCC CHECKIDENT ('ResourceAllocation', RESEED, 0);
        DBCC CHECKIDENT ('ProjectMonthlySnapshot', RESEED, 0);
        DBCC CHECKIDENT ('Billing', RESEED, 0);
        DBCC CHECKIDENT ('Expense', RESEED, 0);
        DBCC CHECKIDENT ('GlobalRate', RESEED, 0);
        DBCC CHECKIDENT ('ProjectRate', RESEED, 0);
        -- DBCC CHECKIDENT ('Override', RESEED, 0); 

    COMMIT TRANSACTION;
    PRINT 'Cleanup complete.'
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'Error during cleanup: ' + ERROR_MESSAGE();
    
    -- Re-enable constraints and exit
    EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
    RETURN;
END CATCH

-- Re-enable constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"


-- 2. SEED GLOBAL RATES
-- =========================================================================================
PRINT 'Seeding Global Rates...'
INSERT INTO GlobalRate (Level, NominalRate, UpdatedAt) VALUES 
('P', 2000.00, GETUTCDATE()),
('D', 1500.00, GETUTCDATE()),
('SM', 1200.00, GETUTCDATE()),
('M', 1000.00, GETUTCDATE()),
('AM', 800.00, GETUTCDATE()),
('SC', 600.00, GETUTCDATE()),
('C', 400.00, GETUTCDATE()),
('BA', 300.00, GETUTCDATE());


-- 3. SEED PROJECTS
-- =========================================================================================
PRINT 'Seeding Projects...'

DECLARE @P1_Id INT, @P2_Id INT, @P3_Id INT;

-- Project 1: Cloud Migration
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin, CreatedAt, UpdatedAt, IsDeleted)
VALUES ('Cloud Migration Initiative', 'P-2026-001', '2026-01-01', '2026-12-31', 500000.00, 500000.00, 0.00, 1.00, 0.30, GETUTCDATE(), GETUTCDATE(), 0);
SET @P1_Id = SCOPE_IDENTITY();

-- Project 2: AI Research (Has 10% Discount)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin, CreatedAt, UpdatedAt, IsDeleted)
VALUES ('AI Research', 'P-2026-002', '2026-06-01', '2026-12-31', 270000.00, 300000.00, 10.00, 0.80, 0.25, GETUTCDATE(), GETUTCDATE(), 0);
SET @P2_Id = SCOPE_IDENTITY();

-- Project 3: Legacy Support (Short term)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin, CreatedAt, UpdatedAt, IsDeleted)
VALUES ('Legacy Support', 'P-2026-003', '2026-01-01', '2026-03-31', 45000.00, 45000.00, 0.00, 1.00, 0.15, GETUTCDATE(), GETUTCDATE(), 0);
SET @P3_Id = SCOPE_IDENTITY();


-- 4. SEED PROJECT RATES (Derived from Global Rates)
-- =========================================================================================
PRINT 'Seeding Project Rates...'

INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
SELECT @P1_Id, Level, NominalRate, NominalRate * (1 - (0.00/100.0)) FROM GlobalRate;

INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
SELECT @P2_Id, Level, NominalRate, NominalRate * (1 - (10.00/100.0)) FROM GlobalRate;

INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
SELECT @P3_Id, Level, NominalRate, NominalRate * (1 - (0.00/100.0)) FROM GlobalRate;


-- 5. SEED FORECAST VERSIONS
-- =========================================================================================
PRINT 'Seeding Forecast Versions...'
DECLARE @FV1_Id INT, @FV2_Id INT, @FV3_Id INT;

INSERT INTO ForecastVersion (ProjectId, VersionNumber, CreatedAt) VALUES (@P1_Id, 1, GETUTCDATE());
SET @FV1_Id = SCOPE_IDENTITY();

INSERT INTO ForecastVersion (ProjectId, VersionNumber, CreatedAt) VALUES (@P2_Id, 1, GETUTCDATE());
SET @FV2_Id = SCOPE_IDENTITY();

INSERT INTO ForecastVersion (ProjectId, VersionNumber, CreatedAt) VALUES (@P3_Id, 1, GETUTCDATE());
SET @FV3_Id = SCOPE_IDENTITY();


-- 6. SEED PROJECT MONTHLY SNAPSHOTS
-- =========================================================================================
PRINT 'Seeding Project Monthly Snapshots...'

-- SNAPSHOTS FOR PROJECT 1 (Jan - Dec 2026)
;WITH MonthsCTE AS (
    SELECT CAST('2026-01-01' AS DATE) AS MonthDate
    UNION ALL
    SELECT DATEADD(MONTH, 1, MonthDate)
    FROM MonthsCTE
    WHERE MonthDate < '2026-12-01'
)
INSERT INTO ProjectMonthlySnapshot 
(ProjectId, ForecastVersionId, Month, Status, OpeningBalance, CumulativeBillings, Wip, DirectExpenses, OperationalCost, MonthlyBillings, MonthlyExpenses, CumulativeExpenses, Nsr, Margin, IsOverridden, CreatedAt, UpdatedAt)
SELECT 
    @P1_Id, @FV1_Id, MonthDate, 
    CASE WHEN MonthDate = '2026-01-01' THEN 1 ELSE 0 END, -- Status: First month Editable, others Pending
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE()
FROM MonthsCTE;


-- SNAPSHOTS FOR PROJECT 2 (Jun - Dec 2026)
;WITH MonthsCTE AS (
    SELECT CAST('2026-06-01' AS DATE) AS MonthDate
    UNION ALL
    SELECT DATEADD(MONTH, 1, MonthDate)
    FROM MonthsCTE
    WHERE MonthDate < '2026-12-01'
)
INSERT INTO ProjectMonthlySnapshot 
(ProjectId, ForecastVersionId, Month, Status, OpeningBalance, CumulativeBillings, Wip, DirectExpenses, OperationalCost, MonthlyBillings, MonthlyExpenses, CumulativeExpenses, Nsr, Margin, IsOverridden, CreatedAt, UpdatedAt)
SELECT 
    @P2_Id, @FV2_Id, MonthDate, 
    CASE WHEN MonthDate = '2026-06-01' THEN 1 ELSE 0 END, -- Status: First month Editable, others Pending
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE()
FROM MonthsCTE;


-- SNAPSHOTS FOR PROJECT 3 (Jan - Mar 2026)
;WITH MonthsCTE AS (
    SELECT CAST('2026-01-01' AS DATE) AS MonthDate
    UNION ALL
    SELECT DATEADD(MONTH, 1, MonthDate)
    FROM MonthsCTE
    WHERE MonthDate < '2026-03-01'
)
INSERT INTO ProjectMonthlySnapshot 
(ProjectId, ForecastVersionId, Month, Status, OpeningBalance, CumulativeBillings, Wip, DirectExpenses, OperationalCost, MonthlyBillings, MonthlyExpenses, CumulativeExpenses, Nsr, Margin, IsOverridden, CreatedAt, UpdatedAt)
SELECT 
    @P3_Id, @FV3_Id, MonthDate, 
    CASE WHEN MonthDate = '2026-01-01' THEN 1 ELSE 0 END, -- Status: First month Editable, others Pending
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE()
FROM MonthsCTE;

PRINT ''
PRINT '========================================='
PRINT 'RESET AND SEED COMPLETE'
PRINT '========================================='

-- Summary Selection
SELECT 'Roster (Preserved)' AS Entity, COUNT(*) AS Count FROM Roster
UNION ALL
SELECT 'Global Rates', COUNT(*) FROM GlobalRate
UNION ALL
SELECT 'Projects', COUNT(*) FROM Project
UNION ALL
SELECT 'Project Rates', COUNT(*) FROM ProjectRate
UNION ALL
SELECT 'Forecast Versions', COUNT(*) FROM ForecastVersion
UNION ALL
SELECT 'Snapshots', COUNT(*) FROM ProjectMonthlySnapshot
UNION ALL
SELECT 'Resource Allocations', COUNT(*) FROM ResourceAllocation;

PRINT '========================================='
