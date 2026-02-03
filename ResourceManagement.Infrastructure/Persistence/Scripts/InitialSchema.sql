-- Roster Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roster')
BEGIN
    CREATE TABLE Roster (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SapCode NVARCHAR(50) NOT NULL UNIQUE,
        FullNameEn NVARCHAR(200) NOT NULL,
        LegalEntity NVARCHAR(100),
        FunctionBusinessUnit NVARCHAR(100),
        CostCenterCode NVARCHAR(50),
        Level NVARCHAR(50),
        
        NewAmendedSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
        EmployerContributions DECIMAL(18,2) NOT NULL DEFAULT 0,
        Cars DECIMAL(18,2) NOT NULL DEFAULT 0,
        TicketRestaurant DECIMAL(18,2) NOT NULL DEFAULT 0,
        Metlife DECIMAL(18,2) NOT NULL DEFAULT 0,
        TopusPerMonth DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        GrossRevenue DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountedRevenue DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_Roster_SapCode ON Roster(SapCode);
END

-- Project Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Project')
BEGIN
    CREATE TABLE Project (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Wbs NVARCHAR(100) NOT NULL UNIQUE,
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        Budget DECIMAL(18,2) NOT NULL DEFAULT 0,
        Recoverability DECIMAL(5,2) NOT NULL DEFAULT 0,
        TargetMargin DECIMAL(5,2) NOT NULL DEFAULT 0,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_Project_Wbs ON Project(Wbs);
END

-- ForecastVersion Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ForecastVersion')
BEGIN
    CREATE TABLE ForecastVersion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        VersionNumber INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        UNIQUE (ProjectId, VersionNumber)
    );
END

-- ResourceAllocation Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResourceAllocation')
BEGIN
    CREATE TABLE ResourceAllocation (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ForecastVersionId INT NOT NULL,
        RosterId INT NOT NULL,
        Month DATE NOT NULL,
        AllocatedDays DECIMAL(5,2) NOT NULL DEFAULT 0,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (ForecastVersionId) REFERENCES ForecastVersion(Id) ON DELETE CASCADE,
        FOREIGN KEY (RosterId) REFERENCES Roster(Id),
        UNIQUE (ForecastVersionId, RosterId, Month)
    );
END

-- Billing Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Billing')
BEGIN
    CREATE TABLE Billing (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        Month DATE NOT NULL,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        UNIQUE (ProjectId, Month)
    );
END

-- Expense Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Expense')
BEGIN
    CREATE TABLE Expense (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        Month DATE NOT NULL,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        UNIQUE (ProjectId, Month)
    );
END

-- Override Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Override')
BEGIN
    CREATE TABLE [Override] (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        Month DATE NOT NULL,
        Confirmed BIT NOT NULL DEFAULT 0,
        
        OpeningBalance DECIMAL(18,2) NULL,
        Billings DECIMAL(18,2) NULL,
        Wip DECIMAL(18,2) NULL,
        Expenses DECIMAL(18,2) NULL,
        Cost DECIMAL(18,2) NULL,
        Nsr DECIMAL(18,2) NULL,
        Margin DECIMAL(5,4) NULL,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ConfirmedAt DATETIME2 NULL,
        ConfirmedBy NVARCHAR(200) NULL,
        
        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        UNIQUE (ProjectId, Month)
    );
END

-- Migration: Add GlobalRate Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GlobalRate')
BEGIN
    CREATE TABLE GlobalRate (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Level NVARCHAR(50) NOT NULL,
        NominalRate DECIMAL(18,2) NOT NULL DEFAULT 0,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_GlobalRate_Level ON GlobalRate(Level);
END

-- Migration: Update Roster Table
IF COL_LENGTH('Roster', 'NewAmendedSalary') IS NOT NULL
BEGIN
    EXEC sp_rename 'Roster.NewAmendedSalary', 'MonthlySalary', 'COLUMN';
END

IF COL_LENGTH('Roster', 'TopusPerMonth') IS NOT NULL
BEGIN
    DECLARE @ConstraintName1 nvarchar(200)
    SELECT @ConstraintName1 = Name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('Roster') AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Roster') AND name = 'TopusPerMonth')
    IF @ConstraintName1 IS NOT NULL
    EXEC('ALTER TABLE Roster DROP CONSTRAINT ' + @ConstraintName1)
    
    ALTER TABLE Roster DROP COLUMN TopusPerMonth;
END
IF COL_LENGTH('Roster', 'GrossRevenue') IS NOT NULL
BEGIN
    DECLARE @ConstraintName2 nvarchar(200)
    SELECT @ConstraintName2 = Name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('Roster') AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Roster') AND name = 'GrossRevenue')
    IF @ConstraintName2 IS NOT NULL
    EXEC('ALTER TABLE Roster DROP CONSTRAINT ' + @ConstraintName2)

    ALTER TABLE Roster DROP COLUMN GrossRevenue;
END
IF COL_LENGTH('Roster', 'DiscountedRevenue') IS NOT NULL
BEGIN
    DECLARE @ConstraintName3 nvarchar(200)
    SELECT @ConstraintName3 = Name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('Roster') AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Roster') AND name = 'DiscountedRevenue')
    IF @ConstraintName3 IS NOT NULL
    EXEC('ALTER TABLE Roster DROP CONSTRAINT ' + @ConstraintName3)

    ALTER TABLE Roster DROP COLUMN DiscountedRevenue;
END

-- Migration: Update Project Table
IF COL_LENGTH('Project', 'Budget') IS NOT NULL
BEGIN
    EXEC sp_rename 'Project.Budget', 'ActualBudget', 'COLUMN';
END

IF COL_LENGTH('Project', 'NominalBudget') IS NULL
BEGIN
    ALTER TABLE Project ADD NominalBudget DECIMAL(18,2) NOT NULL DEFAULT 0;
END

IF COL_LENGTH('Project', 'Discount') IS NULL
BEGIN
    ALTER TABLE Project ADD Discount DECIMAL(5,2) NOT NULL DEFAULT 0;
END

-- Migration: Rename EmployerContributions to MonthlyEmployerContributions
IF COL_LENGTH('Roster', 'EmployerContributions') IS NOT NULL
BEGIN
    EXEC sp_rename 'Roster.EmployerContributions', 'MonthlyEmployerContributions', 'COLUMN';
END

-- ProjectRate Table: Stores actual daily rates per project per level
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectRate')
BEGIN
    CREATE TABLE ProjectRate (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        Level NVARCHAR(50) NOT NULL,
        NominalRate DECIMAL(18,2) NOT NULL,
        ActualDailyRate DECIMAL(18,2) NOT NULL,

        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        UNIQUE (ProjectId, Level)
    );
    CREATE INDEX IX_ProjectRate_ProjectId ON ProjectRate(ProjectId);
END

-- ProjectMonthlySnapshot Table: Persisted financial state with workflow
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectMonthlySnapshot')
BEGIN
    CREATE TABLE ProjectMonthlySnapshot (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        ForecastVersionId INT NOT NULL,
        Month DATE NOT NULL,

        -- Status: 0=Pending, 1=Editable, 2=Confirmed
        Status TINYINT NOT NULL DEFAULT 0,

        -- Financial Values (OB, CB, WIP, DE, OC)
        OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CumulativeBillings DECIMAL(18,2) NOT NULL DEFAULT 0,
        Wip DECIMAL(18,2) NOT NULL DEFAULT 0,
        DirectExpenses DECIMAL(18,2) NOT NULL DEFAULT 0,
        OperationalCost DECIMAL(18,2) NOT NULL DEFAULT 0,

        -- Additional metrics
        MonthlyBillings DECIMAL(18,2) NOT NULL DEFAULT 0,
        MonthlyExpenses DECIMAL(18,2) NOT NULL DEFAULT 0,
        CumulativeExpenses DECIMAL(18,2) NOT NULL DEFAULT 0,
        Nsr DECIMAL(18,2) NOT NULL DEFAULT 0,
        Margin DECIMAL(5,4) NOT NULL DEFAULT 0,

        -- Override tracking
        IsOverridden BIT NOT NULL DEFAULT 0,
        OverriddenAt DATETIME2 NULL,
        OverriddenBy NVARCHAR(200) NULL,

        -- Confirmation tracking
        ConfirmedAt DATETIME2 NULL,
        ConfirmedBy NVARCHAR(200) NULL,

        -- Original values (prior to override)
        OriginalOpeningBalance DECIMAL(18,2) NULL,
        OriginalCumulativeBillings DECIMAL(18,2) NULL,
        OriginalWip DECIMAL(18,2) NULL,
        OriginalDirectExpenses DECIMAL(18,2) NULL,
        OriginalOperationalCost DECIMAL(18,2) NULL,

        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
        FOREIGN KEY (ForecastVersionId) REFERENCES ForecastVersion(Id),
        UNIQUE (ProjectId, ForecastVersionId, Month)
    );
    CREATE INDEX IX_ProjectMonthlySnapshot_ProjectId ON ProjectMonthlySnapshot(ProjectId);
    CREATE INDEX IX_ProjectMonthlySnapshot_Status ON ProjectMonthlySnapshot(Status);
    CREATE INDEX IX_ProjectMonthlySnapshot_ForecastVersionId ON ProjectMonthlySnapshot(ForecastVersionId);
END

-- Migration: Add OriginalX columns to ProjectMonthlySnapshot if missing
IF COL_LENGTH('ProjectMonthlySnapshot', 'OriginalOpeningBalance') IS NULL
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalOpeningBalance DECIMAL(18,2) NULL;
END
IF COL_LENGTH('ProjectMonthlySnapshot', 'OriginalCumulativeBillings') IS NULL
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalCumulativeBillings DECIMAL(18,2) NULL;
END
IF COL_LENGTH('ProjectMonthlySnapshot', 'OriginalWip') IS NULL
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalWip DECIMAL(18,2) NULL;
END
IF COL_LENGTH('ProjectMonthlySnapshot', 'OriginalDirectExpenses') IS NULL
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalDirectExpenses DECIMAL(18,2) NULL;
END
IF COL_LENGTH('ProjectMonthlySnapshot', 'OriginalOperationalCost') IS NULL
BEGIN
    ALTER TABLE ProjectMonthlySnapshot ADD OriginalOperationalCost DECIMAL(18,2) NULL;
END



-- AuditLogs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(50) NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        ChangedBy NVARCHAR(200) NOT NULL,
        ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_AuditLogs_EntityName ON AuditLogs(EntityName);
    CREATE INDEX IX_AuditLogs_ChangedAt ON AuditLogs(ChangedAt);
END
