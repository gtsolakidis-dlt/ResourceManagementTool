# Low Level Design (LLD) - Resource Management Tool
**Version:** 3.0 (Validated & Updated)  
**Date:** 2026-02-03  
**Status:** Implementation Complete

## 1. Database Schema (SQL Server)

### 1.1 Core Entities

**Roster (Resources)**
```sql
CREATE TABLE Roster (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SapCode NVARCHAR(50) NOT NULL UNIQUE,
    FullNameEn NVARCHAR(200) NOT NULL,
    LegalEntity NVARCHAR(100),
    FunctionBusinessUnit NVARCHAR(100),
    CostCenterCode NVARCHAR(50),
    Level NVARCHAR(50),
    
    -- Financial Fields
    MonthlySalary DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Migrated from NewAmendedSalary
    MonthlyEmployerContributions DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Renamed from EmployerContributions
    Cars DECIMAL(18,2) NOT NULL DEFAULT 0,
    TicketRestaurant DECIMAL(18,2) NOT NULL DEFAULT 0,
    Metlife DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_Roster_SapCode ON Roster(SapCode);
```

**Note**: Authentication fields (Username, PasswordHash, Role) are NOT implemented. Planned for future enhancement.

**Project**
```sql
CREATE TABLE Project (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Wbs NVARCHAR(100) NOT NULL UNIQUE,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    ActualBudget DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Migrated from Budget
    NominalBudget DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Added
    Discount DECIMAL(5,2) NOT NULL DEFAULT 0,  -- Added
    Recoverability DECIMAL(5,2) NOT NULL DEFAULT 0,
    TargetMargin DECIMAL(5,2) NOT NULL DEFAULT 0,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_Project_Wbs ON Project(Wbs);
```

**ForecastVersion**
```sql
CREATE TABLE ForecastVersion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    VersionNumber INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
    UNIQUE (ProjectId, VersionNumber)
);
```

**ResourceAllocation**
```sql
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
```

**GlobalRate**
```sql
CREATE TABLE GlobalRate (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Level NVARCHAR(50) NOT NULL,
    NominalRate DECIMAL(18,2) NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_GlobalRate_Level ON GlobalRate(Level);
```

**ProjectRate (Project-Specific Overrides)**
```sql
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
```

### 1.2 Financial Tables

**Billing**
```sql
CREATE TABLE Billing (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    Month DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
    UNIQUE (ProjectId, Month)
);
```

**Expense**
```sql
CREATE TABLE Expense (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    Month DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
    UNIQUE (ProjectId, Month)
);
```

**Override (Legacy - Deprecated)**
```sql
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
```

**Note**: The `Override` table is maintained for backward compatibility but the **Snapshot system** is now the primary mechanism for financial workflow.

### 1.3 Snapshot System (**NEW - PRIMARY FINANCIAL WORKFLOW**)

**ProjectMonthlySnapshot**
```sql
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
```

**Snapshot Status Enum**:
```csharp
public enum SnapshotStatus
{
    Pending = 0,     // Future months, not yet active
    Editable = 1,    // Current active month (only ONE per project/version)
    Confirmed = 2    // Locked months, cannot be changed
}
```

---

## 2. API Endpoints

### 2.1 Authentication Controller (`/api/auth`)
**Note**: Placeholder implementation. Full RBAC not yet enforced.

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/auth/me` | Verify credentials & return user info (placeholder) | None |
| POST | `/api/auth/login` | Login (placeholder) | None |

**Response**: `{ username, role, rosterId }` (mocked data)

### 2.2 Projects Controller (`/api/projects`)
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| GET | `/api/projects` | List projects | All |
| GET | `/api/projects/{id}` | Project details | All |
| POST | `/api/projects` | Create project | All (to be restricted) |
| PUT | `/api/projects/{id}` | Update project | All (to be restricted) |
| GET | `/api/projects/export` | Excel export | All |
| POST | `/api/projects/import` | Excel import | All (to be restricted) |

**Note**: RBAC logic is designed but not enforced. `canEdit` flag exists but is not populated based on actual user roles.

### 2.3 Roster Controller (`/api/roster`)
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| GET | `/api/roster` | List all members | All |
| GET | `/api/roster/{id}` | Member details | All |
| POST | `/api/roster` | Create member | All (to be restricted) |
| PUT | `/api/roster/{id}` | Update member | All (to be restricted) |
| GET | `/api/roster/export` | Excel export | All |
| POST | `/api/roster/import` | Excel import | All (to be restricted) |

### 2.4 Forecasts Controller (`/api/forecasts`)
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| GET | `/{projectId}/versions` | List forecast versions | All |
| GET | `/allocations/{versionId}` | Get allocations | All |
| POST | `/allocations` | Upsert allocations (batch) | All (to be restricted) |
| POST | `/clone` | Clone version | All (to be restricted) |
| DELETE | `/allocations/{versionId}/{rosterId}` | Delete allocation | All (to be restricted) |

### 2.5 Financials Controller (`/api/financials`)

#### Legacy Endpoints
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| GET | `/{projectId}/calculate/{versionId}` | Calculate financials (legacy) | All |
| POST | `/billing` | Upsert billing | All |
| POST | `/expense` | Upsert expense | All |
| POST | `/override` | Upsert override (deprecated) | All |

#### Snapshot Endpoints (**PRIMARY SYSTEM**)
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| **GET** | **`/{projectId}/snapshots/{forecastVersionId}`** | **Get all monthly snapshots** | All |
| **POST** | **`/snapshots/confirm`** | **Confirm/lock editable month** | All (to be restricted) |
| **POST** | **`/snapshots/overwrite`** | **Override calculated values** | All (to be restricted) |
| **POST** | **`/snapshots/clear-override`** | **Restore calculated values** | All (to be restricted) |

**Snapshot Request/Response Models**:
```csharp
// GetSnapshotsQuery Response
public class ProjectMonthlySnapshotDto
{
    public int Id { get; set; }
    public DateTime Month { get; set; }
    public SnapshotStatus Status { get; set; }  // Enum: Pending, Editable, Confirmed
    
    // Financial values
    public decimal OpeningBalance { get; set; }
    public decimal CumulativeBillings { get; set; }
    public decimal Wip { get; set; }
    public decimal DirectExpenses { get; set; }
    public decimal OperationalCost { get; set; }
    public decimal Nsr { get; set; }
    public decimal Margin { get; set; }
    
    // Override tracking
    public bool IsOverridden { get; set; }
    public DateTime? OverriddenAt { get; set; }
    public string? OverriddenBy { get; set; }
    
    // Original values (if overridden)
    public decimal? OriginalOpeningBalance { get; set; }
    public decimal? OriginalCumulativeBillings { get; set; }
    public decimal? OriginalWip { get; set; }
    public decimal? OriginalDirectExpenses { get; set; }
    public decimal? OriginalOperationalCost { get; set; }
    
    // Confirmation tracking
    public DateTime? ConfirmedAt { get; set; }
    public string? ConfirmedBy { get; set; }
}

// ConfirmMonthCommand
public class ConfirmMonthCommand : IRequest<bool>
{
    public int ProjectId { get; set; }
    public int ForecastVersionId { get; set; }
    public DateTime Month { get; set; }
    public string ConfirmedBy { get; set; }
}

// OverwriteSnapshotCommand
public class OverwriteSnapshotCommand : IRequest<bool>
{
    public int ProjectId { get; set; }
    public int ForecastVersionId { get; set; }
    public DateTime Month { get; set; }
    public string OverriddenBy { get; set; }
    
    // Editable fields
    public decimal OpeningBalance { get; set; }
    public decimal Wip { get; set; }
    public decimal DirectExpenses { get; set; }
    public decimal OperationalCost { get; set; }
    public decimal CumulativeBillings { get; set; }
}

// ClearOverrideCommand
public class ClearOverrideCommand : IRequest<bool>
{
    public int ProjectId { get; set; }
    public int ForecastVersionId { get; set; }
    public DateTime Month { get; set; }
}
```

### 2.6 GlobalRates Controller (`/api/globalrates`)
| Method | Endpoint | Description | RBAC |
|--------|----------|-------------|------|
| GET | `/api/globalrates` | List rates | All |
| POST | `/api/globalrates` | Create rate | All (to be restricted to Admin) |
| PUT | `/api/globalrates/{id}` | Update rate | All (to be restricted to Admin) |

---

## 3. Frontend Architecture

### 3.1 Context Providers Implementation

**AuthContext.tsx**
```typescript
interface AuthContextType {
    user: User | null;
    login: (username: string, password: string) => Promise<void>;
    logout: () => void;
    isAuthenticated: boolean;
}

// Current: Placeholder implementation
// Future: Integrate with Roster-based authentication
```

**ThemeContext.tsx**
```typescript
interface ThemeContextType {
    theme: 'dark' | 'light';
    toggleTheme: () => void;
}

// Persists theme in localStorage
// Updates CSS root class for theme switching
```

**NavigationContext.tsx**
```typescript
interface NavigationContextType {
    breadcrumbs: Breadcrumb[];
    setBreadcrumbs: (crumbs: Breadcrumb[]) => void;
    activeSection: string;
    setActiveSection: (section: string) => void;
    sidebarSubItems: SubItem[];
    setSidebarSubItems: (items: SubItem[]) => void;
}

// Manages dynamic breadcrumbs and sidebar sub-navigation
```

**NotificationContext.tsx**
```typescript
interface NotificationContextType {
    notify: {
        success: (message: string) => void;
        error: (message: string) => void;
        info: (message: string) => void;
        warning: (message: string) => void;
    };
    notifications: Notification[];
    unreadCount: number;
    markAsRead: (id: string) => void;
    markAllAsRead: () => void;
    clearAll: () => void;
}

// Integrates with react-hot-toast + custom history
```

### 3.2 Component Hierarchy
```
App.tsx
├── AuthContext.Provider
│   ├── ThemeContext.Provider
│   │   ├── NavigationContext.Provider
│   │   │   ├── NotificationContext.Provider
│   │   │   │   ├── Router
│   │   │   │   │   ├── Layout.tsx
│   │   │   │   │   │   ├── Sidebar.tsx (with dynamic sub-items)
│   │   │   │   │   │   ├── TopBar.tsx (breadcrumbs, theme, notifications, search)
│   │   │   │   │   │   └── Outlet (page content)
│   │   │   │   │   └── CommandPalette.tsx
│   │   │   │   └── Toaster (react-hot-toast)
```

### 3.3 Key Page Components

**ProjectDetailsPage.tsx**
- Displays project KPI cards (Budget, Recoverability, Target Margin)
- Renders `ProjectOverviewWithSnapshots` component
- Manages project edit modal
- Sets up breadcrumbs and sidebar sub-navigation

**ProjectOverviewWithSnapshots.tsx** (**MOST COMPLEX COMPONENT**)
```typescript
Features:
- Fetches snapshots via snapshotService.getSnapshots()
- Displays monthly financial matrix (transposed layout)
- Edit mode toggle (pencil icon in Editable column header)
- Conditional actions row (only renders when isEditMode=true)
- Manual override with diff preview dialog
- Confirm month with warning dialog
- Clear override with confirmation dialog
- Override indicators (green dot) with tooltip
- Pending columns styled with striped pattern
- Confirmed columns show lock icon
- Editable column highlighted with green tint
```

**State Management**:
```typescript
const [snapshots, setSnapshots] = useState<ProjectMonthlySnapshot[]>([]);
const [editableMonth, setEditableMonth] = useState<ProjectMonthlySnapshot | null>(null);
const [editedValues, setEditedValues] = useState<Partial<ProjectMonthlySnapshot>>({});
const [isEditMode, setIsEditMode] = useState(false);
const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
```

**FinancialInputPage.tsx**
- Horizontal month-by-month grid for Billings or Expenses
- Auto-save on blur with debounce
- Visual feedback for save states

### 3.4 API Service Layer

**services.ts**
```typescript
export const snapshotService = {
    getSnapshots: (projectId: number, forecastVersionId: number) =>
        api.get<ProjectMonthlySnapshot[]>(`/financials/${projectId}/snapshots/${forecastVersionId}`),
    
    confirmMonth: (data: ConfirmMonthRequest) =>
        api.post<boolean>('/financials/snapshots/confirm', data),
    
    overwriteSnapshot: (data: OverwriteSnapshotRequest) =>
        api.post<boolean>('/financials/snapshots/overwrite', data),
    
    clearOverride: (projectId: number, forecastVersionId: number, month: string) =>
        api.post<boolean>('/financials/snapshots/clear-override', { projectId, forecastVersionId, month }),
};

export const financialService = {
    getFinancials: (projectId: number, forecastVersionId: number) =>
        api.get<MonthlyFinancial[]>(`/financials/${projectId}/calculate/${forecastVersionId}`),
    
    upsertBilling: (data: any) => api.post('/financials/billing', data),
    upsertExpense: (data: any) => api.post('/financials/expense', data),
};
```

---

## 4. Backend Application Layer

### 4.1 CQRS Handlers

**Snapshot Commands**:

**ConfirmMonthCommandHandler.cs**
```csharp
public class ConfirmMonthCommandHandler : IRequestHandler<ConfirmMonthCommand, bool>
{
    // 1. Find editable snapshot
    // 2. Set Status = Confirmed, ConfirmedAt, ConfirmedBy
    // 3. Find next pending snapshot and set Status = Editable
    // 4. Save changes
    // 5. Return true
}
```

**OverwriteSnapshotCommandHandler.cs**
```csharp
public class OverwriteSnapshotCommandHandler : IRequestHandler<OverwriteSnapshotCommand, bool>
{
    // 1. Find editable snapshot
    // 2. If not already overridden, store current values in OriginalX fields
    // 3. Update editable fields with provided values
    // 4. Recalculate NSR and Margin
    // 5. Set IsOverridden = true, OverriddenBy, OverriddenAt
    // 6. Trigger SnapshotRecalculationService for subsequent months
    // 7. Save changes
    // 8. Return true
}
```

**ClearOverrideCommandHandler.cs**
```csharp
public class ClearOverrideCommandHandler : IRequestHandler<ClearOverrideCommand, bool>
{
    // 1. Find overridden editable snapshot
    // 2. Restore values from OriginalX fields
    // 3. Clear OriginalX fields
    // 4. Set IsOverridden = false, clear OverriddenBy/At
    // 5. Trigger SnapshotRecalculationService to recalculate from inputs
    // 6. Save changes
    // 7. Return true
}
```

**Snapshot Queries**:

**GetSnapshotsQueryHandler.cs**
```csharp
public class GetSnapshotsQueryHandler : IRequestHandler<GetSnapshotsQuery, List<ProjectMonthlySnapshotDto>>
{
    // 1. Query ProjectMonthlySnapshot by ProjectId and ForecastVersionId
    // 2. Order by Month
    // 3. Map to DTOs
    // 4. Return list
}
```

### 4.2 Domain Services

**SnapshotRecalculationService.cs** (**CRITICAL BUSINESS LOGIC**)
```csharp
public class SnapshotRecalculationService
{
    // Triggered by:
    // - Allocation changes
    // - Billing/Expense changes
    // - Override operations
    
    public async Task RecalculateSnapshots(int projectId, int forecastVersionId)
    {
        // 1. Load project, allocations, billings, expenses, rates
        // 2. Load existing snapshots ordered by month
        // 3. For each month:
        //    a. Calculate base WIP from allocations × rates × (1 - discount)
        //    b. Calculate base Cost from allocations × roster daily cost
        //    c. Get monthly billing/expense inputs
        //    d. Calculate cumulative values (carry forward from previous month)
        //    e. If month is overridden and Status=Editable:
        //       - Use overridden values, skip re-calculation
        //    f. Calculate NSR = WIP + CumulativeBillings - OpeningBalance - CumulativeExpenses
        //    g. Calculate Margin = (NSR - Cost) / NSR
        //    h. Update or create snapshot record
        // 4. Save all snapshots
        // 5. Maintain workflow status (only one Editable)
    }
}
```

**Cost Calculation Formulas**:
```csharp
// Roster Daily Cost
decimal monthlyCost14 = ((MonthlySalary + MonthlyEmployerContributions) * 14m / 12m) 
                        + Cars + TicketRestaurant + Metlife;
decimal dailyCost = monthlyCost14 / 18.0m;

// WIP (Work In Progress)
decimal wip = allocations.Sum(a => a.AllocatedDays * GetRate(a.Level) * (1 - project.Discount / 100m));

// NSR (Net Service Revenue)
decimal nsr = wip + cumulativeBillings - openingBalance - cumulativeExpenses;

// Margin
decimal margin = nsr != 0 ? (nsr - operationalCost) / nsr : 0;
```

### 4.3 Repository Pattern (Dapper)

**ProjectMonthlySnapshotRepository.cs**
```csharp
public class ProjectMonthlySnapshotRepository : IProjectMonthlySnapshotRepository
{
    public async Task<List<ProjectMonthlySnapshot>> GetSnapshotsByProjectAndVersion(
        int projectId, int forecastVersionId)
    {
        const string sql = @"
            SELECT * FROM ProjectMonthlySnapshot 
            WHERE ProjectId = @ProjectId AND ForecastVersionId = @ForecastVersionId
            ORDER BY Month";
        
        return (await _connection.QueryAsync<ProjectMonthlySnapshot>(sql, new { projectId, forecastVersionId }))
            .ToList();
    }

    public async Task UpsertSnapshot(ProjectMonthlySnapshot snapshot)
    {
        // Dapper upsert logic (check existence, INSERT or UPDATE)
    }

    public async Task<ProjectMonthlySnapshot?> GetEditableSnapshot(int projectId, int forecastVersionId)
    {
        const string sql = @"
            SELECT TOP 1 * FROM ProjectMonthlySnapshot 
            WHERE ProjectId = @ProjectId 
              AND ForecastVersionId = @ForecastVersionId 
              AND Status = @Status";
        
        return await _connection.QueryFirstOrDefaultAsync<ProjectMonthlySnapshot>(sql, 
            new { projectId, forecastVersionId, Status = SnapshotStatus.Editable });
    }
}
```

---

## 5. Workflow Sequence Diagrams

### 5.1 Snapshot Override Workflow
```
User → UI: Click Edit icon on Editable month
UI: Show edit mode (actions row visible, fields editable)
User → UI: Modify values
UI → User: Show "unsaved changes" state
User → UI: Click Save
UI → API: POST /financials/snapshots/overwrite {values, overriddenBy}
API → DB: Store original values in OriginalX fields
API → DB: Update snapshot with new values
API → Service: SnapshotRecalculationService.RecalculateSnapshots() [for subsequent months]
API → UI: 200 OK
UI → User: Show success toast + "Last saved" timestamp
```

### 5.2 Confirm Month Workflow
```
User → UI: Click Confirm button
UI → User: Show warning dialog ("Irreversible action")
User → UI: Click "Yes, Lock Month"
UI → API: POST /financials/snapshots/confirm {month, confirmedBy}
API → DB: UPDATE Status=Confirmed, ConfirmedAt, ConfirmedBy for current month
API → DB: UPDATE Status=Editable for next pending month
API → UI: 200 OK
UI: Reload snapshots
UI → User: Show success toast + matrix updates (green → blue, next month green)
```

### 5.3 Clear Override Workflow
```
User → UI: Click Reset button (on overridden month)
UI → User: Show confirmation dialog
User → UI: Confirm
UI → API: POST /financials/snapshots/clear-override {projectId, versionId, month}
API → DB: Restore values from OriginalX fields
API → DB: Clear OriginalX fields, IsOverridden=false
API → Service: SnapshotRecalculationService.RecalculateSnapshots()
API → UI: 200 OK
UI: Reload snapshots
UI → User: Show success toast + updated values
```

---

## 6. CSS Styling Architecture

### 6.1 Theme Variables (index.css)
```css
:root {
    /* Dark Mode (default) */
    --bg-color: #121212;
    --surface-primary: rgba(30, 30, 30, 0.6);
    --surface-secondary: #1e1e1e;
    --text-primary: #ffffff;
    --text-secondary: #b0b0b0;
    --text-muted: #6b7280;
    --deloitte-green: #86bc25;
    --border-primary: rgba(255, 255, 255, 0.1);
}

.theme-light {
    --bg-color: #f5f5f5;
    --surface-primary: #ffffff;
    --surface-secondary: #f8fafc;
    --text-primary: #0f172a;
    --text-secondary: #475569;
    --text-muted: #94a3b8;
    --border-primary: #e2e8f0;
}
```

### 6.2 Snapshot Matrix Styling (ProjectOverviewWithSnapshots.css)

**Column Status Styles**:
```css
/* Editable Column */
.snapshot-table th.editable-col,
.snapshot-table td.editable-col {
    background: rgba(134, 188, 37, 0.15);
    border-left: 2px solid rgba(134, 188, 37, 0.3);
    border-right: 2px solid rgba(134, 188, 37, 0.3);
}

/* Pending Column (Striped) */
.snapshot-table th.pending-col,
.snapshot-table td.pending-col {
    background-image: repeating-linear-gradient(
        45deg, 
        transparent, 
        transparent 10px, 
        rgba(255, 255, 255, 0.03) 10px, 
        rgba(255, 255, 255, 0.03) 20px
    );
    color: var(--text-muted);
    font-style: italic;
}
```

**Override Indicator**:
```css
.override-indicator {
    position: absolute;
    top: 6px;
    right: 8px;
    width: 6px;
    height: 6px;
    background-color: var(--deloitte-green);
    border-radius: 50%;
    cursor: help;
    box-shadow: 0 0 0 2px rgba(134, 188, 37, 0.15);
    transition: transform 0.3s;
}

.override-indicator:hover {
    transform: scale(1.5);
}
```

**Conditional Actions Row**:
```css
/* Only renders when isEditMode=true in React */
.actions-row td {
    padding-top: 1rem;
    vertical-align: top;
}
```

---

## 7. Key Implementation Notes

### 7.1 Migration Strategy
The `InitialSchema.sql` uses `IF NOT EXISTS` and `IF COL_LENGTH` checks to:
- Create tables if missing
- Rename columns (e.g., `NewAmendedSalary` → `MonthlySalary`)
- Add new columns (e.g., `NominalBudget`, `Discount` to `Project`)
- Maintain backward compatibility

### 7.2 Snapshot System Benefits
✅ **Auditability**: Original values preserved when overridden  
✅ **Performance**: Pre-calculated values, no on-the-fly aggregation  
✅ **Workflow**: Clear state management (Pending → Editable → Confirmed)  
✅ **User Experience**: Visual feedback with status-based styling  

### 7.3 Known Limitations
⚠️ **RBAC**: Designed but not enforced (all endpoints currently accessible)  
⚠️ **Authentication**: Placeholder implementation, no Roster integration  
⚠️ **Concurrent Edits**: No locking mechanism (assumes single-user editing per project)  

---

**Document Version**: 3.0  
**Last Validated**: 2026-02-03  
**Validated Against**: Current production codebase  
**End of Document**
