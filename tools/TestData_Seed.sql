-- =====================================================
-- TEST DATA SCRIPT - Resource Management Tool
-- Version: 2.0
-- Date: 2026-01-26
-- Purpose: Seed database with data for all test cases
-- =====================================================

-- =====================================================
-- CLEANUP (Optional - uncomment to reset)
-- =====================================================
-- DELETE FROM ResourceAllocation;
-- DELETE FROM ForecastVersion;
-- DELETE FROM Billing;
-- DELETE FROM Expense;
-- DELETE FROM [Override];
-- DELETE FROM Project WHERE Name LIKE 'TEST_%';
-- DELETE FROM GlobalRate;
-- DELETE FROM Roster WHERE SapCode LIKE 'TEST_%';

-- =====================================================
-- 1. GLOBAL RATES (Epic 4, Test 3.2.1, Test 4.1.x)
-- =====================================================
PRINT 'Setting up Global Rates...';

-- Delete existing rates to avoid conflicts
DELETE FROM GlobalRate;

INSERT INTO GlobalRate (Level, NominalRate) VALUES 
    ('A', 300.00),      -- Associate
    ('C', 400.00),      -- Consultant
    ('SC', 500.00),     -- Senior Consultant (for Test 3.2.1: WIP = 10 * 500 * 0.9 = 4500)
    ('M', 800.00),      -- Manager (for Test 4.1.1, 4.1.2)
    ('SM', 900.00),     -- Senior Manager
    ('D', 1200.00),     -- Director
    ('P', 1500.00);     -- Partner

PRINT 'Global Rates created.';

-- =====================================================
-- 2. ROSTER MEMBERS (Epic 1, Epic 5)
-- =====================================================
PRINT 'Setting up Roster Members...';

-- Clear test users
DELETE FROM Roster WHERE SapCode LIKE 'TEST_%';

-- Test User 1: ADMIN (for Test 5.5.1, 5.6.1, 5.6.2)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_ADMIN_001', 'Admin User', 'DBS', 'Operations', 'D',
     8000.00, 1500.00, 500.00, 150.00, 100.00,
     'admin', 'admin123', 'Admin');
     -- Password: admin123

-- Test User 2: PARTNER (for Test 5.5.2)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_PARTNER_001', 'Partner User', 'DBS', 'Operations', 'P',
     12000.00, 2500.00, 800.00, 150.00, 150.00,
     'partner', 'admin123', 'Partner');
     -- Password: admin123

-- Test User 3: MANAGER (for Test 5.4.x)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_MANAGER_001', 'Manager User', 'DBS', 'Engineering', 'M',
     6000.00, 1200.00, 400.00, 150.00, 80.00,
     'manager', 'admin123', 'Manager');
     -- Password: admin123

-- Test User 4: EMPLOYEE (for Test 5.3.x, 5.6.3)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_EMP_001', 'Employee User', 'DBS', 'Engineering', 'SC',
     4000.00, 800.00, 0.00, 150.00, 50.00,
     'employee', 'admin123', 'Employee');
     -- Password: admin123

-- Test User 5: "John Smith" for search test (Test 1.3.1)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_JOHN_001', 'John Smith', 'DBS', 'Engineering', 'SC',
     4500.00, 900.00, 200.00, 150.00, 60.00,
     'john', 'admin123', 'Employee');
     -- Password: admin123

-- More SC members for filter test (Test 1.3.2)
INSERT INTO Roster (SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, Level, 
    MonthlySalary, EmployerContributions, Cars, TicketRestaurant, Metlife,
    Username, PasswordHash, Role)
VALUES 
    ('TEST_SC_001', 'Maria Garcia', 'DBS', 'AI & Data', 'SC',
     4200.00, 850.00, 0.00, 150.00, 55.00,
     'maria', 'admin123', 'Employee'),
    ('TEST_SC_002', 'Andreas Papadopoulos', 'DBS', 'Engineering', 'SC',
     4300.00, 870.00, 150.00, 150.00, 58.00,
     NULL, NULL, 'Employee'),
    ('TEST_C_001', 'Elena Konstantinou', 'DBS', 'Operations', 'C',
     3000.00, 600.00, 0.00, 150.00, 40.00,
     NULL, NULL, 'Employee'),
    ('TEST_M_001', 'Nikos Georgiou', 'DBS', 'Engineering', 'M',
     5500.00, 1100.00, 350.00, 150.00, 75.00,
     NULL, NULL, 'Employee');

PRINT 'Roster Members created.';

-- =====================================================
-- 3. PROJECTS (Epic 2, Epic 3)
-- =====================================================
PRINT 'Setting up Projects...';

DECLARE @ManagerRosterId INT = (SELECT Id FROM Roster WHERE SapCode = 'TEST_MANAGER_001');
DECLARE @EmployeeRosterId INT = (SELECT Id FROM Roster WHERE SapCode = 'TEST_EMP_001');
DECLARE @JohnRosterId INT = (SELECT Id FROM Roster WHERE SapCode = 'TEST_JOHN_001');
DECLARE @MariaRosterId INT = (SELECT Id FROM Roster WHERE SapCode = 'TEST_SC_001');

-- Delete test projects to avoid conflicts
DELETE FROM Project WHERE Wbs LIKE 'TEST.%';

-- Project 1: For Test 2.1.1 (100k budget, 20% discount = 125k nominal)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
VALUES ('TEST_Budget Calculation Project', 'TEST.001', '2026-01-01', '2026-12-31', 
        100000.00, 125000.00, 20.00, 95.00, 20.00);

-- Project 2: For WIP Calculation Test (Test 3.2.1 - 10% discount)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
VALUES ('TEST_WIP Calculation Project', 'TEST.002', '2026-01-01', '2026-06-30', 
        50000.00, 55555.56, 10.00, 90.00, 25.00);

-- Project 3: Assigned to Manager (for Test 5.4.2)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
VALUES ('TEST_Manager Assigned Project', 'TEST.003', '2026-01-01', '2026-12-31', 
        200000.00, 222222.22, 10.00, 92.00, 22.00);

-- Project 4: NOT assigned to Manager (for Test 5.4.3)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
VALUES ('TEST_Unassigned Project', 'TEST.004', '2026-02-01', '2026-10-31', 
        75000.00, 78947.37, 5.00, 88.00, 18.00);

-- Project 5: Assigned to Employee (for Test 5.3.1)
INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
VALUES ('TEST_Employee Assigned Project', 'TEST.005', '2026-01-01', '2026-09-30', 
        120000.00, 126315.79, 5.00, 91.00, 21.00);

PRINT 'Projects created.';

-- =====================================================
-- 4. FORECAST VERSIONS & ALLOCATIONS
-- =====================================================
PRINT 'Setting up Forecast Versions and Allocations...';

DECLARE @Project2Id INT = (SELECT Id FROM Project WHERE Wbs = 'TEST.002');
DECLARE @Project3Id INT = (SELECT Id FROM Project WHERE Wbs = 'TEST.003');
DECLARE @Project5Id INT = (SELECT Id FROM Project WHERE Wbs = 'TEST.005');

-- Create Forecast Versions
INSERT INTO ForecastVersion (ProjectId, VersionNumber) VALUES (@Project2Id, 1);
INSERT INTO ForecastVersion (ProjectId, VersionNumber) VALUES (@Project3Id, 1);
INSERT INTO ForecastVersion (ProjectId, VersionNumber) VALUES (@Project5Id, 1);

DECLARE @Version2Id INT = (SELECT Id FROM ForecastVersion WHERE ProjectId = @Project2Id AND VersionNumber = 1);
DECLARE @Version3Id INT = (SELECT Id FROM ForecastVersion WHERE ProjectId = @Project3Id AND VersionNumber = 1);
DECLARE @Version5Id INT = (SELECT Id FROM ForecastVersion WHERE ProjectId = @Project5Id AND VersionNumber = 1);

-- Allocations for WIP Test (Test 3.2.1): John (SC) allocated 10 days in Jan 2026
-- Expected WIP = 10 * 500 (SC rate) * 0.9 (10% discount) = 4500
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays)
VALUES (@Version2Id, @JohnRosterId, '2026-01-01', 10.00);

-- Allocate Manager to Project 3 (so they can edit it - Test 5.4.2)
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays)
VALUES (@Version3Id, @ManagerRosterId, '2026-01-01', 15.00);

-- Allocate Employee to Project 5 (so they can see it - Test 5.3.1)
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays)
VALUES (@Version5Id, @EmployeeRosterId, '2026-01-01', 12.00);

-- Also add Maria to Project 5 for more test data
INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays)
VALUES (@Version5Id, @MariaRosterId, '2026-01-01', 8.00);

PRINT 'Forecast Versions and Allocations created.';

-- =====================================================
-- 5. BILLINGS & EXPENSES (Epic 3)
-- =====================================================
PRINT 'Setting up Billings and Expenses...';

-- Add some billing data for financial tests
INSERT INTO Billing (ProjectId, Month, Amount) VALUES (@Project3Id, '2026-01-01', 15000.00);
INSERT INTO Billing (ProjectId, Month, Amount) VALUES (@Project3Id, '2026-02-01', 18000.00);

INSERT INTO Expense (ProjectId, Month, Amount) VALUES (@Project3Id, '2026-01-01', 2500.00);
INSERT INTO Expense (ProjectId, Month, Amount) VALUES (@Project3Id, '2026-02-01', 3200.00);

PRINT 'Billings and Expenses created.';

-- =====================================================
-- 6. VERIFICATION QUERIES
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT 'TEST DATA SUMMARY';
PRINT '=====================================================';

SELECT 'Global Rates' AS Entity, COUNT(*) AS Count FROM GlobalRate;
SELECT 'Roster Members (Test)' AS Entity, COUNT(*) AS Count FROM Roster WHERE SapCode LIKE 'TEST_%';
SELECT 'Projects (Test)' AS Entity, COUNT(*) AS Count FROM Project WHERE Wbs LIKE 'TEST.%';
SELECT 'Forecast Versions' AS Entity, COUNT(*) AS Count FROM ForecastVersion WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
SELECT 'Resource Allocations' AS Entity, COUNT(*) AS Count FROM ResourceAllocation WHERE ForecastVersionId IN (SELECT Id FROM ForecastVersion WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%'));

PRINT '';
PRINT '=====================================================';
PRINT 'TEST USER CREDENTIALS';
PRINT '=====================================================';
PRINT 'All test users have password: admin123';
PRINT '';

SELECT Username, Role, FullNameEn, Level 
FROM Roster 
WHERE Username IS NOT NULL AND SapCode LIKE 'TEST_%'
ORDER BY 
    CASE Role 
        WHEN 'Admin' THEN 1
        WHEN 'Partner' THEN 2
        WHEN 'Manager' THEN 3
        WHEN 'Employee' THEN 4
    END;

PRINT '';
PRINT '=====================================================';
PRINT 'PROJECT ASSIGNMENTS (for RBAC testing)';
PRINT '=====================================================';

SELECT 
    p.Name AS ProjectName,
    r.FullNameEn AS AssignedUser,
    r.Role AS UserRole
FROM Project p
JOIN ForecastVersion fv ON p.Id = fv.ProjectId
JOIN ResourceAllocation ra ON fv.Id = ra.ForecastVersionId
JOIN Roster r ON ra.RosterId = r.Id
WHERE p.Wbs LIKE 'TEST.%'
ORDER BY p.Name, r.FullNameEn;

PRINT '';
PRINT 'Script completed successfully!';
PRINT '=====================================================';
