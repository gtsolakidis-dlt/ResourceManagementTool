# High Level Design (HLD) - Resource Management Tool
**Version:** 3.0 (Validated & Updated)  
**Date:** 2026-02-03  
**Status:** Implementation Complete

## 1. Introduction
The Resource Management Tool is an enterprise web application designed to manage employee consulting resources, project allocations, and financial forecasting with advanced workflow management. It replaces manual Excel-based workflows with a centralized, collaborative platform featuring monthly financial snapshots with state-based workflow (Pending, Editable, Confirmed).

## 2. Architecture Overview
The system follows a modern **Client-Server Architecture**:

*   **Frontend**: Single Page Application (SPA) built with React 18 + TypeScript + Vite.
*   **Backend**: RESTful API built with ASP.NET Core 9 implementing CQRS pattern with MediatR.
*   **Database**: SQL Server (relational data) with 11 core tables.
*   **Authentication**: Placeholder implementation (full RBAC planned).
*   **Integration**: Excel Import/Export via EPPlus library.

### Architecture Diagram
```
[Browser] <-> [React App (Vite)] <--(HTTP/JSON + Auth Header)--> [ASP.NET Core 9 API] <--(Dapper)--> [SQL Server]
                   |
          [Context Providers]
          ├── AuthContext (session management)
          ├── ThemeContext (dark/light mode)
          ├── NavigationContext (breadcrumbs, sidebar)
          └── NotificationContext (toast + history)
```

## 3. Technology Stack

### 3.1 Frontend Stack
*   **Core**: React 18.2, TypeScript 5.x, Vite 5.x
*   **Routing**: React Router v6
*   **State Management**: Context API (4 contexts: Auth, Theme, Navigation, Notification)
*   **Styling**: Vanilla CSS with CSS Variables (theme-aware)
*   **Icons**: lucide-react (feather-style icons)
*   **HTTP Client**: Axios with interceptors
*   **Notifications**: react-hot-toast + custom StatusCapsule component
*   **Build Tool**: Vite (fast HMR, optimized bundling)

### 3.2 Backend Stack
*   **Framework**: .NET 9 (ASP.NET Core Web API)
*   **Architecture Pattern**: Clean Architecture 
    - ResourceManagement.Api (Controllers)
    - ResourceManagement.Application (CQRS Handlers)
    - ResourceManagement.Domain (Entities, Interfaces)
    - ResourceManagement.Infrastructure (Dapper Repositories)
    - ResourceManagement.Contracts (DTOs)
*   **ORM**: Dapper (Micro-ORM for high performance)
*   **Mediator**: MediatR (CQRS pattern - Commands & Queries)
*   **Validation**: FluentValidation
*   **Authentication**: Placeholder (planned: BasicAuthenticationHandler with BCrypt)
*   **Excel**: EPPlus library
*   **Migrations**: SQL Scripts (InitialSchema.sql)

### 3.3 Database
*   **RDBMS**: SQL Server
*   **Schema**: 11 core tables with foreign key constraints
*   **Migration Strategy**: IF NOT EXISTS checks with ALTER TABLE for schema evolution

## 4. Key Modules

### 4.1 Resource Roster
**Purpose**: Central repository of all employees/contractors.

**Features**:
*   CRUD operations for roster members
*   Automatic cost calculation (14-month Greek logic)
*   Search and filter by name, level, function
*   Excel import/export
*   Daily cost formula: `((MonthlySalary + MonthlyEmployerContributions) * 14 / 12 + Benefits) / 18.0`

**Database**: `Roster` table (migrated field names: NewAmendedSalary → MonthlySalary, EmployerContributions → MonthlyEmployerContributions)

**API Endpoints**:
- `GET /api/roster` - List all members
- `GET /api/roster/{id}` - Get member details
- `POST /api/roster` - Create member
- `PUT /api/roster/{id}` - Update member
- `GET /api/roster/export` - Export to Excel
- `POST /api/roster/import` - Import from Excel

### 4.2 Project Management
**Purpose**: Directory of engagement projects with financial targets.

**Features**:
*   Project CRUD with auto-calculated Nominal Budget
*   Hierarchical navigation (Overview, Billings, Expenses, Forecast)
*   Dynamic sidebar with project-specific sub-items
*   Budget tracking: Actual Budget, Nominal Budget, Discount %
*   Target metrics: Recoverability, Target Margin

**Database**: `Project` table with ActualBudget, NominalBudget, Discount

**API Endpoints**:
- `GET /api/projects` - List projects (RBAC-filtered - to be implemented)
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `GET /api/projects/export` - Export to Excel
- `POST /api/projects/import` - Import from Excel

### 4.3 Forecast Management
**Purpose**: Resource allocation planning with versioning (scenarios).

**Features**:
*   Monthly allocation matrix (resources × months)
*   Forecast version cloning for scenario planning
*   Allocated days per resource per month
*   Integration with Global/Project Rates for revenue calculation

**Database**: `ForecastVersion`, `ResourceAllocation` tables

**API Endpoints**:
- `GET /api/forecasts/{projectId}/versions` - List versions
- `GET /api/forecasts/allocations/{versionId}` - Get allocations
- `POST /api/forecasts/allocations` - Upsert allocations (batch)
- `POST /api/forecasts/clone` - Clone forecast version
- `DELETE /api/forecasts/allocations/{versionId}/{rosterId}` - Delete allocation

### 4.4 Financials & Monthly Snapshots (**CORE MODULE**)
**Purpose**: Monthly financial tracking with advanced workflow management.

#### 4.4.1 Snapshot-Based Architecture
The system uses **persistent monthly snapshots** instead of on-the-fly calculations. Each month has a `ProjectMonthlySnapshot` record with:
- **Status**: Pending (future), Editable (current active), Confirmed (locked)
- **Calculated Values**: OB, CB, WIP, DE, OC, NSR, Margin
- **Override Capability**: Manual adjustments with original value preservation
- **Workflow Tracking**: Who/when confirmed or overridden

#### 4.4.2 Workflow States
```
[Pending] → [Editable] → [Confirmed]
                ↓
         (Override values)
                ↓
         (Clear override)
```

**Rules**:
- Only ONE month is Editable at a time per project/version
- Confirming a month locks it permanently and promotes next month
- Overriding stores original calculated values for audit trail
- Pending months show calculated values but are not editable

#### 4.4.3 Financial Inputs
*   **Billings**: Horizontal monthly input (`Billing` table)
*   **Expenses**: Horizontal monthly input (`Expense` table)
*   **Input Pages**: `FinancialInputPage.tsx` with month-by-month grid

**Database**: 
- `Billing` (ProjectId, Month, Amount)
- `Expense` (ProjectId, Month, Amount)
- `ProjectMonthlySnapshot` (comprehensive monthly state with workflow)

**API Endpoints**:
- `POST /api/financials/billing` - Upsert monthly billing
- `POST /api/financials/expense` - Upsert monthly expense
- `GET /api/financials/{projectId}/snapshots/{versionId}` - **Get all snapshots**
- `POST /api/financials/snapshots/confirm` - **Confirm/lock editable month**
- `POST /api/financials/snapshots/overwrite` - **Override calculated values**
- `POST /api/financials/snapshots/clear-override` - **Restore calculated values**

### 4.5 Global Configuration
**Purpose**: System-wide settings and rate cards.

**Features**:
*   Global Rate management (Level → Nominal Daily Rate)
*   Project-specific rate overrides (ProjectRate table)
*   Used in WIP calculations

**Database**: `GlobalRate`, `ProjectRate` tables

**API Endpoints**:
- `GET /api/globalrates` - List all rates
- `POST /api/globalrates` - Create rate
- `PUT /api/globalrates/{id}` - Update rate

### 4.6 Authentication & Authorization (**PLANNED**)
**Current Status**: Placeholder implementation in AuthController.

**Planned Features**:
*   Username/Password stored in Roster table
*   BCrypt password hashing
*   Role-based access control (Employee, Manager, Partner, Admin)
*   Claims-based authorization in API

**Note**: RBAC is designed but not yet enforced in the backend. All users currently have full access.

## 5. Cross-Cutting Features

### 5.1 Notification System
**Implementation**: Custom NotificationContext + react-hot-toast integration.

**Features**:
*   **Toast Notifications**: Success, Error, Info, Warning, Loading
*   **Notification Center**: Bell icon with unread badge, history panel
*   **Actions**: Mark read, Mark all read, Clear all
*   **Persistence**: In-memory (up to 50 notifications)

**Key Components**:
- `NotificationContext.tsx` - State management
- `NotificationCenter.tsx` - Dropdown panel
- `StatusCapsule.tsx` - Custom toast component

### 5.2 Command Palette (Global Search)
**Implementation**: `CommandPalette.tsx` with fuzzy search.

**Features**:
*   Keyboard shortcut: `Ctrl+K`
*   Navigation commands (Dashboard, Projects, Roster, etc.)
*   Quick actions (Create Project, Add Talent)
*   Click search icon fallback

### 5.3 Theme System
**Implementation**: `ThemeContext` with CSS variables.

**Features**:
*   Dark Mode (default) and Light Mode
*   Toggle via sun/moon icon
*   Persisted in `localStorage`
*   Theme-aware CSS variables: `--bg-color`, `--text-primary`, `--deloitte-green`, etc.

### 5.4 Dynamic Navigation
**Implementation**: `NavigationContext` provides breadcrumbs and sidebar state.

**Features**:
*   **Breadcrumbs**: Hierarchical path (Resource Platform > Projects > [Name] > Forecast)
*   **Contextual Sidebar**: Project details show sub-navigation (Overview, Billings, Expenses, Forecast)
*   **Active Section Tracking**: Highlights current page in sidebar

**Key Components**:
- `NavigationContext.tsx` - State provider
- `Layout.tsx` - Sidebar + TopBar rendering
- `Sidebar.tsx` - Navigation menu with sub-items

## 6. Frontend Component Architecture

### 6.1 Context Hierarchy
```
<AuthContext.Provider>
  <ThemeContext.Provider>
    <NavigationContext.Provider>
      <NotificationContext.Provider>
        <Layout>
          <Sidebar />
          <TopBar />
          <main>{children}</main>
        </Layout>
        <CommandPalette />
      </NotificationContext.Provider>
    </NavigationContext.Provider>
  </ThemeContext.Provider>
</AuthContext.Provider>
```

### 6.2 Page Components
- `LoginPage.tsx` - Authentication form
- `RosterPage.tsx` - Resource grid with search/filter
- `ProjectsPage.tsx` - Project cards with live metrics
- **`ProjectDetailsPage.tsx`** - Project overview with KPI cards + Snapshot matrix
- **`ProjectOverviewWithSnapshots.tsx`** - Advanced snapshot matrix with edit mode
- `ForecastingPage.tsx` - Allocation matrix (RBAC-aware placeholder)
- `FinancialInputPage.tsx` - Billings/Expenses horizontal input
- `GlobalRatesPage.tsx` - Admin rate management

### 6.3 Common Components
- `PremiumNumericInput.tsx` - Formatted numeric input with step controls
- `PremiumSelect.tsx` - Custom styled dropdown
- `Loader` components - Consistent loading states

## 7. Backend Application Architecture

### 7.1 CQRS Pattern Implementation
**Commands** (Write Operations):
- `UpsertBillingCommand`
- `UpsertExpenseCommand`
- `OverwriteSnapshotCommand` (**NEW**)
- `ConfirmMonthCommand` (**NEW**)
- `ClearOverrideCommand` (**NEW**)

**Queries** (Read Operations):
- `GetProjectFinancialsQuery`
- `GetSnapshotsQuery` (**NEW**)
- `GetProjectQuery`
- `GetAllRosterMembersQuery`

### 7.2 Domain Services
**`SnapshotRecalculationService`** (**CRITICAL SERVICE**):
- Recalculates all monthly snapshots when input data changes
- Handles cumulative calculations (WIP, Cost, Billings, Expenses)
- Respects override values and stores originals
- Maintains workflow status (Pending, Editable, Confirmed)

### 7.3 Repository Pattern
**Dapper-based repositories**:
- `RosterRepository`
- `ProjectRepository`
- `ForecastVersionRepository`
- `ResourceAllocationRepository`
- **`ProjectMonthlySnapshotRepository`** (**NEW**)
- `BillingRepository`
- `ExpenseRepository`
- `GlobalRateRepository`
- `ProjectRateRepository`

## 8. Database Schema Summary

### Core Tables (11 total)
1. **Roster** - Employee/contractor data with costs
2. **Project** - Project metadata with budgets
3. **ForecastVersion** - Versioned allocation scenarios
4. **ResourceAllocation** - Monthly resource allocations
5. **Billing** - Monthly billing inputs
6. **Expense** - Monthly expense inputs
7. **Override** - Legacy override table (deprecated)
8. **GlobalRate** - System-wide level rates
9. **ProjectRate** - Project-specific rate overrides
10. **ProjectMonthlySnapshot** - **NEW** - Monthly financial state with workflow
11. *(Future: User/Role tables for proper RBAC)*

### Key Relationships
```
Project 1:N ForecastVersion
ForecastVersion 1:N ResourceAllocation
ResourceAllocation N:1 Roster
Project 1:N ProjectMonthlySnapshot
ForecastVersion 1:N ProjectMonthlySnapshot
Project 1:N Billing
Project 1:N Expense
Project 1:N ProjectRate
```

## 9. Security & Deployment

### 9.1 Security (**CURRENT STATE**)
*   **Authentication**: Placeholder implementation
*   **Authorization**: Not enforced (planned RBAC)
*   **HTTPS**: Required for production
*   **Password Hashing**: Planned (BCrypt)

### 9.2 Deployment (Planned)
*   **Hosting**: Azure App Service + Azure SQL
*   **CI/CD**: GitHub Actions
*   **Environments**: Development, Staging, Production
*   **Monitoring**: Application Insights

## 10. Performance Considerations

### 10.1 Optimizations
- Dapper for lightweight ORM (faster than Entity Framework)
- Snapshot pre-calculation (no on-the-fly aggregation in UI)
- Indexed columns: SapCode, Wbs, ProjectId, ForecastVersionId, Status
- Batch allocation upserts (single transaction)

### 10.2 Targets
- Financial snapshot recalculation: < 200ms for 24 months × 50 resources
- Roster page load: < 500ms for 2000 records
- Allocation matrix save: < 1s for 300 cells

## 11. Key Differences from Original Design

### 11.1 Enhanced Features
✅ **Snapshot-based workflow** - More robust than original override concept  
✅ **Edit mode toggle** - Contextual actions row only shows in edit mode  
✅ **Override indicators** - Visual feedback for manual changes  
✅ **ProjectRate table** - Project-specific rate overrides  
✅ **Conditional actions row** - UI optimization (removed unnecessary space)

### 11.2 Deferred Features
⚠️ **RBAC** - Designed but not enforced  
⚠️ **Roster-based authentication** - Placeholder only  
⚠️ **Azure Entra ID integration** - Planned

---

**Document Version**: 3.0  
**Last Validated**: 2026-02-03  
**Validated Against**: Current production codebase  
**End of Document**
