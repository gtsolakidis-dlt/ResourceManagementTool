-- Seed Data for End-to-End Testing
-- This script adds sample data for a complete workflow

-- =====================================================
-- 1. ROSTER MEMBERS (5 team members with different levels)
-- =====================================================
PRINT 'Inserting Roster Members...'

-- Clear existing roster data (optional - comment out if you want to keep existing data)
-- DELETE FROM ResourceAllocation;
-- DELETE FROM Roster WHERE SapCode LIKE 'SAP%';

-- Insert new roster members only if they don't exist
IF NOT EXISTS (SELECT 1 FROM Roster WHERE SapCode = 'SAP001')
BEGIN
    INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level, MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife)
    VALUES ('SAP001', 'John Smith', 'Deloitte Greece', 'Technology', 'CC100', 'D', 8500.00, 2125.00, 500.00, 176.00, 50.00);
END

IF NOT EXISTS (SELECT 1 FROM Roster WHERE SapCode = 'SAP002')
BEGIN
    INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level, MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife)
    VALUES ('SAP002', 'Maria Garcia', 'Deloitte Greece', 'Technology', 'CC100', 'SM', 6500.00, 1625.00, 400.00, 176.00, 50.00);
END

IF NOT EXISTS (SELECT 1 FROM Roster WHERE SapCode = 'SAP003')
BEGIN
    INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level, MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife)
    VALUES ('SAP003', 'Andreas Papadopoulos', 'Deloitte Greece', 'Technology', 'CC100', 'SC', 4500.00, 1125.00, 300.00, 176.00, 50.00);
END

IF NOT EXISTS (SELECT 1 FROM Roster WHERE SapCode = 'SAP004')
BEGIN
    INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level, MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife)
    VALUES ('SAP004', 'Elena Konstantinou', 'Deloitte Greece', 'Consulting', 'CC200', 'C', 3200.00, 800.00, 0.00, 176.00, 50.00);
END

IF NOT EXISTS (SELECT 1 FROM Roster WHERE SapCode = 'SAP005')
BEGIN
    INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level, MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife)
    VALUES ('SAP005', 'Nikos Georgiou', 'Deloitte Greece', 'Technology', 'CC100', 'BA', 1800.00, 450.00, 0.00, 176.00, 50.00);
END

PRINT 'Roster members inserted successfully.'

-- =====================================================
-- 2. PROJECT (Digital Transformation Project)
-- =====================================================
PRINT 'Inserting Project...'

DECLARE @ProjectId INT;

IF NOT EXISTS (SELECT 1 FROM Project WHERE Wbs = 'WBS-DT-2026-001')
BEGIN
    INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
    VALUES ('Digital Transformation - Alpha Bank', 'WBS-DT-2026-001', '2026-01-01', '2026-06-30', 450000.00, 500000.00, 10.00, 90.00, 35.00);
END

SELECT @ProjectId = Id FROM Project WHERE Wbs = 'WBS-DT-2026-001';
PRINT 'Project ID: ' + CAST(@ProjectId AS NVARCHAR(10));

-- =====================================================
-- 3. FORECAST VERSION (Version 1 for the project)
-- =====================================================
PRINT 'Inserting Forecast Version...'

DECLARE @ForecastVersionId INT;

IF NOT EXISTS (SELECT 1 FROM ForecastVersion WHERE ProjectId = @ProjectId AND VersionNumber = 1)
BEGIN
    INSERT INTO ForecastVersion (ProjectId, VersionNumber)
    VALUES (@ProjectId, 1);
END

SELECT @ForecastVersionId = Id FROM ForecastVersion WHERE ProjectId = @ProjectId AND VersionNumber = 1;
PRINT 'Forecast Version ID: ' + CAST(@ForecastVersionId AS NVARCHAR(10));

-- =====================================================
-- 4. RESOURCE ALLOCATIONS (Monthly allocations for 6 months)
-- =====================================================
PRINT 'Inserting Resource Allocations...'

-- Get Roster IDs
DECLARE @RosterId1 INT, @RosterId2 INT, @RosterId3 INT, @RosterId4 INT, @RosterId5 INT;
SELECT @RosterId1 = Id FROM Roster WHERE SapCode = 'SAP001';
SELECT @RosterId2 = Id FROM Roster WHERE SapCode = 'SAP002';
SELECT @RosterId3 = Id FROM Roster WHERE SapCode = 'SAP003';
SELECT @RosterId4 = Id FROM Roster WHERE SapCode = 'SAP004';
SELECT @RosterId5 = Id FROM Roster WHERE SapCode = 'SAP005';

-- Clear existing allocations for this forecast version
DELETE FROM ResourceAllocation WHERE ForecastVersionId = @ForecastVersionId;

-- January 2026 Allocations
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-01-01', 15.0),  -- John Smith (Principal) - 15 days
(@ForecastVersionId, @RosterId2, '2026-01-01', 20.0),  -- Maria Garcia (Lead) - 20 days
(@ForecastVersionId, @RosterId3, '2026-01-01', 18.0),  -- Andreas (Senior) - 18 days
(@ForecastVersionId, @RosterId4, '2026-01-01', 20.0),  -- Elena (Mid) - 20 days
(@ForecastVersionId, @RosterId5, '2026-01-01', 22.0);  -- Nikos (Junior) - 22 days

-- February 2026 Allocations
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-02-01', 12.0),
(@ForecastVersionId, @RosterId2, '2026-02-01', 20.0),
(@ForecastVersionId, @RosterId3, '2026-02-01', 20.0),
(@ForecastVersionId, @RosterId4, '2026-02-01', 18.0),
(@ForecastVersionId, @RosterId5, '2026-02-01', 22.0);

-- March 2026 Allocations
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-03-01', 10.0),
(@ForecastVersionId, @RosterId2, '2026-03-01', 18.0),
(@ForecastVersionId, @RosterId3, '2026-03-01', 22.0),
(@ForecastVersionId, @RosterId4, '2026-03-01', 20.0),
(@ForecastVersionId, @RosterId5, '2026-03-01', 22.0);

-- April 2026 Allocations
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-04-01', 8.0),
(@ForecastVersionId, @RosterId2, '2026-04-01', 15.0),
(@ForecastVersionId, @RosterId3, '2026-04-01', 20.0),
(@ForecastVersionId, @RosterId4, '2026-04-01', 22.0),
(@ForecastVersionId, @RosterId5, '2026-04-01', 20.0);

-- May 2026 Allocations
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-05-01', 5.0),
(@ForecastVersionId, @RosterId2, '2026-05-01', 12.0),
(@ForecastVersionId, @RosterId3, '2026-05-01', 18.0),
(@ForecastVersionId, @RosterId4, '2026-05-01', 20.0),
(@ForecastVersionId, @RosterId5, '2026-05-01', 18.0);

-- June 2026 Allocations (wind-down phase)
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays) VALUES
(@ForecastVersionId, @RosterId1, '2026-06-01', 3.0),
(@ForecastVersionId, @RosterId2, '2026-06-01', 8.0),
(@ForecastVersionId, @RosterId3, '2026-06-01', 10.0),
(@ForecastVersionId, @RosterId4, '2026-06-01', 15.0),
(@ForecastVersionId, @RosterId5, '2026-06-01', 12.0);

PRINT 'Resource Allocations inserted successfully.'

-- =====================================================
-- 5. BILLINGS (Monthly billing amounts)
-- =====================================================
PRINT 'Inserting Billings...'

-- Clear existing billings for this project
DELETE FROM Billing WHERE ProjectId = @ProjectId;

INSERT INTO Billing (ProjectId, Month, Amount) VALUES
(@ProjectId, '2026-01-01', 85000.00),
(@ProjectId, '2026-02-01', 80000.00),
(@ProjectId, '2026-03-01', 75000.00),
(@ProjectId, '2026-04-01', 70000.00),
(@ProjectId, '2026-05-01', 60000.00),
(@ProjectId, '2026-06-01', 40000.00);

PRINT 'Billings inserted successfully.'

-- =====================================================
-- 6. EXPENSES (Monthly expense amounts)
-- =====================================================
PRINT 'Inserting Expenses...'

-- Clear existing expenses for this project
DELETE FROM Expense WHERE ProjectId = @ProjectId;

INSERT INTO Expense (ProjectId, Month, Amount) VALUES
(@ProjectId, '2026-01-01', 2500.00),  -- Travel, software licenses
(@ProjectId, '2026-02-01', 3000.00),  -- Cloud infrastructure
(@ProjectId, '2026-03-01', 2800.00),
(@ProjectId, '2026-04-01', 2200.00),
(@ProjectId, '2026-05-01', 1800.00),
(@ProjectId, '2026-06-01', 1500.00);

PRINT 'Expenses inserted successfully.'

-- =====================================================
-- SUMMARY: Display inserted data
-- =====================================================
PRINT ''
PRINT '========================================='
PRINT 'SEED DATA SUMMARY'
PRINT '========================================='

PRINT 'Roster Members:'
SELECT Id, SapCode, FullNameEn, Level, MonthlySalary FROM Roster WHERE SapCode LIKE 'SAP%';

PRINT 'Project:'
SELECT Id, Name, Wbs, StartDate, EndDate, ActualBudget, Discount FROM Project WHERE Wbs = 'WBS-DT-2026-001';

PRINT 'Forecast Version:'
SELECT * FROM ForecastVersion WHERE ProjectId = @ProjectId;

PRINT 'Resource Allocations (sample):'
SELECT TOP 10 ra.Id, r.FullNameEn, ra.Month, ra.AllocatedDays 
FROM ResourceAllocation ra
JOIN Roster r ON r.Id = ra.RosterId
WHERE ra.ForecastVersionId = @ForecastVersionId
ORDER BY ra.Month, r.Level;

PRINT 'Billings:'
SELECT * FROM Billing WHERE ProjectId = @ProjectId ORDER BY Month;

PRINT 'Expenses:'
SELECT * FROM Expense WHERE ProjectId = @ProjectId ORDER BY Month;

PRINT ''
PRINT 'Seed data insertion complete!'
PRINT 'You can now test the full end-to-end flow in the application.'
