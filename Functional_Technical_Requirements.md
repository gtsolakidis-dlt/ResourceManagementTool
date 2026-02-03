# Functional & Technical Requirements Document
**Project:** Resource Management Tool  
**Version:** 3.0 (Validated & Updated)  
**Date:** 2026-02-03  
**Status:** Implementation Complete

## 1. Project Overview
The Resource Management Tool is a centralized web platform designed to replace Excel-based spreadsheets for managing consultant resources (Roster), project portfolios, and financial forecasting. It allows Project Managers (PMs) and Admins to track costs, forecast revenues, and monitor project profitability in real-time with automated calculations and role-based access control.

**Current Status**: Fully implemented with advanced snapshot-based monthly financial tracking and workflow management.

---

## 2. Domain Dictionary
*   **WIP (Work In Progress)**: Revenue recognized based on effort spent, calculated automatically from resource allocations.
*   **NSR (Net Service Revenue)**: The actual revenue recognized after expenses (Calculated as WIP + Billings - Opening Balance - Expenses).
*   **Margin**: Profitability percentage `(NSR - Cost) / NSR * 100`.
*   **Roster**: The pool of all available employees and contractors.
*   **Nominal Budget**: The estimated budget value derived from the signed Actual Budget divided by the complement of the discount rate.
*   **Global Rate**: The standard daily billing rate associated with a resource's seniority level (Grade/Level).
*   **RBAC**: Role-Based Access Control - permission system based on user roles.
*   **Project Monthly Snapshot**: Persisted monthly financial state with workflow status (Pending, Editable, Confirmed).
*   **Snapshot Status**: Workflow state - Only ONE month is "Editable" at a time; others are "Pending" or "Confirmed" (locked).

---

## 3. Functional Requirements (Detailed)

### 3.1 Module: Resource Management (Roster)
**Goal**: Maintain an up-to-date list of all resources and their cost to the company.

#### 3.1.1 Data Structure & Fields
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `SapCode` | String (Unique) | Employee ID from SAP system. |
| `FullNameEn` | String | English Full Name. |
| `Level` | String | Seniority Grade (e.g., "A", "C", "SC", "M"). Links to **Global Rates**. |
| `MonthlySalary` | Decimal | Basic monthly gross salary (migrated from NewAmendedSalary). |
| `MonthlyEmployerContributions` | Decimal | Social security/pension costs (renamed from EmployerContributions). |
| `Cars`, `Metlife`, `TicketRestaurant`| Decimal | Monthly benefit values. |
| `Username` | String (Unique) | **NOT IMPLEMENTED** - Planned for future authentication integration. |
| `PasswordHash` | String | **NOT IMPLEMENTED** - Planned for future authentication integration. |
| `Role` | String | **NOT IMPLEMENTED** - Access role: Employee, Manager, Partner, Admin (future). |

**Note**: Authentication fields are not yet implemented in the Roster table. Current authentication uses a placeholder mechanism.

#### 3.1.2 Business Logic: Cost Calculation
The system **automatically computes** the cost of a resource. This logic is hard-coded in the backend (`Roster` Entity).
*   **14-Month Cost (Annualized Monthly Average)**: Used for Greek market logic where 14 salaries are paid annually.
    *   `Formula`: `((MonthlySalary + MonthlyEmployerContributions) * 14 / 12) + Cars + TicketRestaurant + Metlife`
*   **Daily Cost**: The actual cost per working day.
    *   `Formula`: `MonthlyCost_14months / 18.0` (Note: Divisor is **18.0**, based on average billable days).

#### 3.1.3 Import / Export
*   **Import**: Accepts `.xlsx` files via `POST /api/roster/import`. Updates existing records based on `SapCode` and inserts new ones.
*   **Export**: Generates an `.xlsx` download via `GET /api/roster/export`.

---

### 3.2 Module: Project Management
**Goal**: Track the portfolio of engagements and their high-level financial targets.

#### 3.2.1 Data Structure & Fields
| Field Name | Type | Key Validation / Logic |
| :--- | :--- | :--- |
| `Wbs` | String (Unique) | WBS Code (e.g., "WBS.001"). |
| `ActualBudget` | Decimal | The signed contract value. **Required**. |
| `Discount` | Decimal | Percentage discount given to client (0-100%). |
| `NominalBudget` | Decimal | **Calculated**. `ActualBudget / (1 - (Discount / 100))`. |
| `Recoverability` | Decimal | Target % (e.g., 0.95). |
| `TargetMargin` | Decimal | Target profit % (e.g., 0.20 for 20%). |
| `CanEdit` | Boolean | **Runtime**. Indicates if current user can edit (based on RBAC - NOT YET FULLY IMPLEMENTED). |

#### 3.2.2 State Transitions
1.  **Initiation**: User creates project. `NominalBudget` is calculated instantly.
2.  **Active**: User adds forecasts and financial records.
3.  **Closed**: (Future scope) Read-only.

---

### 3.3 Module: Global Configuration (Rates)
**Goal**: Define standard pricing to automate revenue forecasting.

#### 3.3.1 Rate Cards
*   Admins define a **Nominal Rate** (Daily) for each Seniority Level (`Level` string) in the `GlobalRate` table.
*   **Example**: Level "SC" (Senior Consultant) -> €500.00 / day.
*   **Logic**: If a Roster member is Level "SC", their forecast calculation pulls this €500 rate.

#### 3.3.2 Project-Specific Rates
*   **ProjectRate Table** (Implemented): Stores project-specific nominal and actual daily rates per level.
*   Allows overriding global rates for specific projects.

---

### 3.4 Module: Financial Forecasting & Snapshot Workflow (**ENHANCED**)
**Goal**: Compute monthly financial KPIs based on resource effort with advanced workflow management.

#### 3.4.1 Inputs
1.  **Allocations** (Forecast Matrix): User inputs `Days` per Resource per Month.
2.  **Monthly Billings**: User inputs invoiced amounts per month via `POST /api/financials/billing`.
3.  **Monthly Expenses**: User inputs project costs per month via `POST /api/financials/expense`.
4.  **Manual Overrides** (Snapshot System): User can manually override calculated values for the "Editable" month.

#### 3.4.2 The Snapshot System (**NEW IMPLEMENTATION**)
**Core Concept**: Monthly financial data is persisted as `ProjectMonthlySnapshot` records with workflow status.

**Workflow States**:
- **Pending**: Future months, calculated but not yet active for editing. Displayed with striped background.
- **Editable**: Current active month. Only ONE month can be Editable at a time. Users can view/edit and manually override values.
- **Confirmed**: Locked months. Values are permanent and cannot be changed.

**Snapshot Fields**:
- **Financial Calculated Values**: `OpeningBalance`, `CumulativeBillings`, `Wip`, `DirectExpenses`, `OperationalCost`, `Nsr`, `Margin`
- **Monthly Inputs**: `MonthlyBillings`, `MonthlyExpenses`, `CumulativeExpenses`
- **Override Tracking**: `IsOverridden`, `OverriddenAt`, `OverriddenBy`
- **Original Values**: `OriginalOpeningBalance`, `OriginalCumulativeBillings`, `OriginalWip`, `OriginalDirectExpenses`, `OriginalOperationalCost` (stored when overridden)
- **Confirmation**: `ConfirmedAt`, `ConfirmedBy`

#### 3.4.3 Snapshot Workflow Operations

**1. View Snapshots** (`GET /api/financials/{projectId}/snapshots/{forecastVersionId}`)
- Returns all monthly snapshots for a project version with status and calculated values.

**2. Overwrite Snapshot** (`POST /api/financials/snapshots/overwrite`)
- Allows manual override of calculated values for the Editable month.
- Original values are preserved in `OriginalX` fields.
- Sets `IsOverridden = true`, `OverriddenBy`, `OverriddenAt`.
- **UI Feature**: Displays green dot indicator on overridden values with tooltip showing original value and who/when changed.

**3. Clear Override** (`POST /api/financials/snapshots/clear-override`)
- Restores calculated values from `OriginalX` fields.
- Sets `IsOverridden = false`, clears override metadata.

**4. Confirm Month** (`POST /api/financials/snapshots/confirm`)
- Locks the Editable month permanently (Status = Confirmed).
- Promotes next Pending month to Editable.
- **Irreversible Operation** - includes confirmation dialog in UI.

#### 3.4.4 The Calculation Algorithm (`SnapshotRecalculationService`)
For every month in the project duration:

**Step 1: Calculate Base Values**
*   **Cost Base**: `Sum(AllocatedDays * Resource.DailyCost)`
*   **WIP Base** (Revenue):
    *   Look up `GlobalRate` or `ProjectRate` for the Resource's Level.
    *   Apply Project Discount.
    *   `Formula`: `Sum(AllocatedDays * NominalRate * (1 - ProjectDiscount))`

**Step 2: Cumulative Aggregation (Month over Month)**
*   **Opening Balance**: Calculated based on previous month's NSR and billings.
*   **Billings (Cumulative)**: `PreviousMonth.CumulativeBillings + CurrentMonth.MonthlyBilling`.
*   **WIP (Cumulative)**: `PreviousMonth.WIP + CurrentMonth.CalculatedWIP Base`.
*   **Expenses (Cumulative)**: `PreviousMonth.CumulativeExpenses + CurrentMonth.MonthlyExpense`.
*   **Cost (Cumulative - OperationalCost)**: `PreviousMonth.OperationalCost + CurrentMonth.CalculatedCost Base`.

**Step 3: KPI Derivation**
*   **NSR (Net Service Revenue)**:
    *   `Formula`: `WIP + CumulativeBillings - OpeningBalance - CumulativeExpenses`
*   **Margin (Profitability)**:
    *   `Formula`: `(NSR - OperationalCost) / NSR * 100`
    *   *Edge Case*: If NSR is 0, Margin is 0.

#### 3.4.5 Interaction Logic
*   **Missing Rates**: If a resource has a Level not found in Global Rates or Project Rates, their revenue contribution is €0 (but Cost is still calculated from their salary).
*   **Overrides**: When a month is manually overridden, the system stores original calculated values and uses overridden values for subsequent month calculations.

---

### 3.5 Module: Authentication & Authorization (**PARTIALLY IMPLEMENTED**)
**Goal**: Secure access to the platform with role-based permissions.

#### 3.5.1 Current Authentication Status
*   **Type**: Basic implementation in AuthController
*   **Credentials**: Currently NOT stored in Roster table (Username, PasswordHash, Role fields missing)
*   **Session**: Managed via AuthContext in frontend
*   **Status**: **PLACEHOLDER IMPLEMENTATION** - Full RBAC with Roster-based authentication is planned but not yet implemented.

#### 3.5.2 Planned RBAC (Future Enhancement)
| Role | View Projects | Edit Projects | Manage Roster | Assign Roles |
| :--- | :--- | :--- | :--- | :--- |
| **Employee** | Only assigned | None | View only | None |
| **Manager** | All | Only assigned | View only | None |
| **Partner** | All | All | Full | Yes |
| **Admin** | All | All | Full | Yes |

---

### 3.6 Module: Notifications
**Goal**: Provide feedback and history of system actions.

#### 3.6.1 Toast Notifications
*   **Types**: Success (green), Error (red), Warning (yellow), Info (blue), Loading (spinner).
*   **Implementation**: `react-hot-toast` with custom `StatusCapsule` component.
*   **Behavior**: Auto-dismiss after 3-4 seconds.
*   **Position**: Top-center of screen.

#### 3.6.2 Notification Center
*   **Access**: Bell icon with unread count badge.
*   **History**: Up to 50 recent notifications.
*   **Actions**: Mark individual as read, Mark all as read, Clear all.
*   **Implementation**: `NotificationContext` with `NotificationCenter` component.

---

### 3.7 Module: Global Navigation
**Goal**: Enable fast, intuitive navigation across the application.

#### 3.7.1 Command Palette
*   **Trigger**: `Ctrl+K` keyboard shortcut or click search icon.
*   **Features**: Search commands, navigate to pages, quick actions.
*   **Commands**: Go to Dashboard, Projects, Roster, Settings; Create Project; Add Talent.
*   **Implementation**: `CommandPalette.tsx` component.

#### 3.7.2 Breadcrumbs
*   **Display**: Top bar showing hierarchical location.
*   **Interactive**: Click any segment to navigate.
*   **Implementation**: Dynamic breadcrumbs via `NavigationContext`.

#### 3.7.3 Contextual Sidebar
*   **Project Navigation**: When viewing a project, sidebar expands with sub-items:
    - Overview
    - Billings
    - Expenses
    - Forecast
*   **Implementation**: `setSidebarSubItems` in `NavigationContext`.

#### 3.7.4 Theme Toggle
*   **Modes**: Dark (default), Light.
*   **Persistence**: Saved in localStorage.
*   **Implementation**: `ThemeContext` with CSS variables.

---

## 4. Technical Requirements

### 4.1 Technology Stack
*   **Frontend**: React 18, TypeScript, Vite.
    *   **State**: Context API (AuthContext, ThemeContext, NavigationContext, NotificationContext).
    *   **Styling**: Vanilla CSS (CSS Variables for theming).
    *   **Notifications**: react-hot-toast with custom StatusCapsule.
    *   **Icons**: lucide-react.
*   **Backend**: .NET 9 Web API.
    *   **ORM**: Dapper (Micro-ORM).
    *   **Pattern**: CQRS with MediatR.
    *   **Validation**: FluentValidation.
    *   **Auth**: Placeholder (to be enhanced with BasicAuthenticationHandler).
*   **Database**: SQL Server.
    *   **Schema Management**: SQL Scripts (InitialSchema.sql).

### 4.2 Database Schema Specification

**Key Tables Implemented**:
- `Roster`: Resource data with financial fields
- `Project`: Project metadata with ActualBudget, NominalBudget, Discount
- `ForecastVersion`: Versioned forecast scenarios
- `ResourceAllocation`: Monthly resource allocations
- `Billing`: Monthly billing inputs
- `Expense`: Monthly expense inputs
- `Override`: Legacy override table (deprecated in favor of Snapshot system)
- `GlobalRate`: System-wide rates per level
- `ProjectRate`: Project-specific rate overrides
- **`ProjectMonthlySnapshot`**: **NEW** - Persistent monthly financial state with workflow

### 4.3 UI/UX Specifications
*   **Premium Theme**: Dark Mode default, Light Mode available.
    *   Background: `#121212` (dark) / `#f5f5f5` (light)
    *   Card Background: `rgba(30,30,30,0.6)` + Backdrop Blur
    *   Accent: `var(--deloitte-green)` (`#86bc25`)
*   **Snapshot Matrix**: 
    *   Editable column highlighted with green tint
    *   Pending columns shown with striped pattern
    *   Confirmed columns show lock icon
    *   Override indicators: small green dot with tooltip
*   **Feedback**: Custom toast notifications (`StatusCapsule`) for all save/error actions.
*   **Loading States**: All async actions show spinner (`Loader2` from lucide-react).
*   **Formatting**: Currency with `€` symbol and thousands separators (e.g., `€1,250.00`). Dates: LocaleString format.

---

## 5. Non-Functional Requirements
1.  **Performance**: Financial calculations must complete in < 200ms for a project with 24 months and 50 resources.
2.  **Scalability**: Roster table must handle up to 2000 records without pagination lag.
3.  **Browser**: Support Chrome/Edge (latest).
4.  **Security**: Passwords must be hashed (BCrypt) - **TO BE IMPLEMENTED**.
5.  **Accessibility**: Keyboard navigation support (Command Palette, Arrow key grid navigation, Edit mode toggle).

---

## 6. Implementation Status Summary

### ✅ Fully Implemented
- Project and Roster CRUD operations
- Global Rate management
- Resource allocation forecasting with versioning
- Billings and Expenses horizontal input
- **Advanced Snapshot-based monthly financial workflow**
- **Editable month workflow with Confirm/Override/Clear operations**
- **Visual indicators for overridden values**
- **Edit mode toggle (pencil icon) with conditional actions row**
- Dynamic breadcrumb navigation
- Contextual sidebar sub-navigation
- Theme toggle (Dark/Light)
- Command Palette
- Notification Center with history
- Toast notifications
- Excel Import/Export

### ⚠️ Partially Implemented
- Authentication (placeholder only, no Roster integration)
- RBAC (planned but not enforced in backend)

### 📋 Planned (Future Enhancements)
- Full RBAC with Roster-based auth
- Azure Entra ID integration
- Project archival/closure workflow
- Advanced analytics and reporting
- Audit trail for financial changes

---

**Document Version**: 3.0  
**Last Validated**: 2026-02-03  
**Validated Against**: Current production codebase  
**End of Document**
