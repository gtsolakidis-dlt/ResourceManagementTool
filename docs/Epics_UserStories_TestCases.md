# Epics, User Stories & Test Cases
**Version:** 3.0 (Validated & Updated)  
**Date:** 2026-02-03  
**Status:** Reflects Current Implementation

---

## Epic 1: Resource Roster Management
**Description**: Managing the pool of available consultants, their financials, and metadata.  
**Status**: ✅ Fully Implemented

### User Story 1.1: View Roster
*   **As a** Resource Manager,
*   **I want to** see a list of all resources with their SAP Code, Level, and Monthly Cost,
*   **So that** I have a clear view of the talent pool.
*   **Status**: ✅ Implemented
*   **Test Case 1.1.1**: Open Roster page. Verify table loads with columns: Name, SAP Ref, Function, Seniority, Monthly Cost (14-month formula).
*   **Test Case 1.1.2**: Click Edit on a resource. Verify fields are editable: MonthlySalary, MonthlyEmployerContributions, Cars, Metlife, TicketRestaurant.

### User Story 1.2: Excel Import/Export
*   **As an** Admin,
*   **I want to** import/export roster data,
*   **So that** I can perform bulk updates efficiently.
*   **Status**: ✅ Implemented
*   **Test Case 1.2.1**: Click Import. Select valid `.xlsx`. Verify toast success and table update.
*   **Test Case 1.2.2**: Click Export. Verify file `Roster_{Date}.xlsx` is downloaded.

### User Story 1.3: Search & Filter Roster
*   **As a** User,
*   **I want to** search resources by name and filter by seniority level,
*   **So that** I can quickly find specific talent.
*   **Status**: ⚠️ Partially Implemented (search exists, filter may need enhancement)
*   **Test Case 1.3.1**: Type "John" in search box. Verify only matching names appear.
*   **Test Case 1.3.2**: Select "SC" from level filter. Verify only Senior Consultants shown.

---

## Epic 2: Project Management
**Description**: Managing project portfolio, budgets, and strategic targets.  
**Status**: ✅ Fully Implemented

### User Story 2.1: Project Creation
*   **As a** PM,
*   **I want to** create a project with Actual Budget, Discount %, and Target Margin,
*   **So that** the system can calculate Nominal Budget automatically.
*   **Status**: ✅ Implemented
*   **Test Case 2.1.1**: Open "Create Project" modal. Enter Actual Budget `100000` and Discount `20`. Verify Nominal Budget displays ~`125000`.
*   **Test Case 2.1.2**: Create project. Verify card appears in Projects page with correct metrics.

### User Story 2.2: Project Details Navigation
*   **As a** PM,
*   **I want to** navigate to a project's sub-pages (Overview, Billings, Expenses, Forecast),
*   **So that** I can manage all aspects of the project.
*   **Status**: ✅ Implemented with dynamic sidebar
*   **Test Case 2.2.1**: Click on a project card. Verify sidebar shows sub-navigation with project name header.
*   **Test Case 2.2.2**: Click "Forecast" link. Verify navigates to forecast matrix.
*   **Test Case 2.2.3**: Verify breadcrumbs update: Resource Platform > Projects > [Project Name] > Forecast.

### User Story 2.3: View Project Financial Overview (**ENHANCED**)
*   **As a** PM,
*   **I want to** see a monthly financial matrix with snapshot-based workflow,
*   **So that** I can track project financials with clear status indicators (Pending, Editable, Confirmed).
*   **Status**: ✅ Implemented with `ProjectOverviewWithSnapshots` component
*   **Test Case 2.3.1**: Navigate to Project Overview. Verify monthly financial matrix displays.
*   **Test Case 2.3.2**: Verify Editable month has green tint and edit icon.
*   **Test Case 2.3.3**: Verify Pending months have striped pattern.
*   **Test Case 2.3.4**: Verify Confirmed months show lock icon.

---

## Epic 3: Financials & Forecasting (**ENHANCED**)
**Description**: Tracking WIP, Billings, and Expenses monthly with advanced snapshot workflow.  
**Status**: ✅ Fully Implemented

### User Story 3.1: Financial Inputs (Horizontal View)
*   **As a** PM,
*   **I want to** input Billings and Expenses in a horizontal month-by-month view,
*   **So that** I can easily view trends over time.
*   **Status**: ✅ Implemented
*   **Test Case 3.1.1**: Navigate to Project > Billings. Scroll horizontally. Enter `5000` for Jan 2026. Save. Verify persistence.
*   **Test Case 3.1.2**: Navigate to Project > Expenses. Enter values for multiple months. Verify auto-save.

### User Story 3.2: Automated WIP Calculation
*   **As the** System,
*   **I want to** calculate WIP based on Allocated Days, Global/Project Rates, and Project Discount,
*   **So that** revenue is forecasted accurately without manual input.
*   **Logic**: `WIP = Days * Rate * (1 - ProjectDiscount)`
*   **Status**: ✅ Implemented in `SnapshotRecalculationService`
*   **Test Case 3.2.1**: Set Global Rate for 'SC' = 500. Set Project Discount = 10%. Allocate 10 days for an SC. Verify WIP in snapshot = 10 * 500 * 0.9 = 4500.
*   **Test Case 3.2.2**: Change allocation. Verify snapshot recalculates WIP automatically.

### User Story 3.3: Forecast Versioning (Scenarios)
*   **As a** PM,
*   **I want to** create multiple forecast scenarios (versions),
*   **So that** I can compare different planning approaches.
*   **Status**: ✅ Implemented
*   **Test Case 3.3.1**: Click "Clone Scenario" in Forecast page. Verify new version appears in dropdown.
*   **Test Case 3.3.2**: Switch between versions. Verify allocations update correctly.

### User Story 3.4: Edit Mode Toggle (**NEW**)
*   **As a** PM,
*   **I want to** toggle edit mode on the Editable month with a clear visual indicator,
*   **So that** I don't accidentally modify values.
*   **Status**: ✅ Implemented
*   **Test Case 3.4.1**: Click pencil icon in Editable column header. Verify edit mode activates (fields become editable, actions row appears).
*   **Test Case 3.4.2**: Click X icon. Verify edit mode deactivates (actions row hidden, fields read-only).

### User Story 3.5: Manual Override with Audit Trail (**NEW**)
*   **As a** PM,
*   **I want to** manually override calculated financial values for the editable month,
*   **So that** I can adjust for special circumstances while preserving the original calculated values.
*   **Status**: ✅ Implemented with original value preservation
*   **Test Case 3.5.1**: Enable edit mode. Modify WIP value. Click Save. Verify diff dialog shows old vs new values.
*   **Test Case 3.5.2**: Confirm save. Verify green dot indicator appears on overridden cell.
*   **Test Case 3.5.3**: Hover over green dot. Verify tooltip shows: Original Value, Overridden By (username), Overridden At (timestamp).
*   **Test Case 3.5.4**: Verify subsequent months recalculate based on overridden value.

### User Story 3.6: Clear Override (**NEW**)
*   **As a** PM,
*   **I want to** clear manual overrides and restore calculated values,
*   **So that** I can revert incorrect manual adjustments.
*   **Status**: ✅ Implemented
*   **Test Case 3.6.1**: On an overridden editable month, click Reset button.
*   **Test Case 3.6.2**: Verify confirmation dialog explains action ("Clear Manual Overrides?").
*   **Test Case 3.6.3**: Confirm. Verify values revert to calculated state.
*   **Test Case 3.6.4**: Verify green dot indicators disappear.
*   **Test Case 3.6.5**: Verify subsequent months recalculate.

### User Story 3.7: Confirm/Lock Month (**NEW**)
*   **As a** PM,
*   **I want to** confirm and lock the editable month permanently,
*   **So that** historical data is preserved and the next month becomes active for editing.
*   **Status**: ✅ Implemented with irreversible warning
*   **Test Case 3.7.1**: Click Confirm button on editable month.
*   **Test Case 3.7.2**: Verify warning dialog states: "Lock Month Permanently?" with "CANNOT be undone" warning.
*   **Test Case 3.7.3**: Confirm. Verify month status changes to Confirmed (blue styling, lock icon).
*   **Test Case 3.7.4**: Verify next pending month becomes Editable (green tint appears).
*   **Test Case 3.7.5**: Verify confirmed month is read-only (no edit icon).

### User Story 3.8: Visual Status Indicators (**NEW**)
*   **As a** PM,
*   **I want to** see clear visual indicators for month status and overridden values,
*   **So that** I can quickly understand the financial data state.
*   **Status**: ✅ Implemented
*   **Test Case 3.8.1**: Verify Pending columns have striped diagonal pattern.
*   **Test Case 3.8.2**: Verify Editable column has green tint and left/right borders.
*   **Test Case 3.8.3**: Verify Confirmed columns are solid with lock icon.
*   **Test Case 3.8.4**: Verify overridden cells show small green dot in top-right corner.
*   **Test Case 3.8.5**: Verify summary rows (NSR, Margin) have distinct background color.

---

## Epic 4: Global System Configuration
**Description**: Managing system-wide parameters like Standard Rates.  
**Status**: ✅ Fully Implemented

### User Story 4.1: Manage Global Rates
*   **As a** System Admin,
*   **I want to** define standard nominal rates for each Grade/Level (e.g., C, SC, M),
*   **So that** all project forecasts use consistent baseline pricing.
*   **Status**: ✅ Implemented
*   **Test Case 4.1.1**: Navigate to Admin > Global Rates. Add Rate for 'M' -> `800`.
*   **Test Case 4.1.2**: Update Rate for 'M' -> `850`. Verify updated timestamp.
*   **Test Case 4.1.3**: Create new allocation using 'M' level. Verify WIP uses new rate.

### User Story 4.2: Project-Specific Rate Overrides (**NEW**)
*   **As a** PM,
*   **I want to** override global rates for specific projects,
*   **So that** I can handle special pricing arrangements.
*   **Status**: ✅ Implemented via `ProjectRate` table
*   **Test Case 4.2.1**: In project settings (if UI exists), set custom rate for 'SC' level.
*   **Test Case 4.2.2**: Verify allocations for that project use project rate instead of global rate.

---

## Epic 5: Authentication & Access Control
**Description**: Secure login and role-based permissions.  
**Status**: ⚠️ Partially Implemented (Placeholder)

### User Story 5.1: User Login
*   **As a** User,
*   **I want to** log in with my username and password,
*   **So that** I can access the platform securely.
*   **Status**: ⚠️ Placeholder implementation
*   **Test Case 5.1.1**: Enter valid credentials. Click Login. Verify redirect to Dashboard. *(Currently: Mocked authentication)*
*   **Test Case 5.1.2**: Enter invalid password. Verify error message "Invalid credentials". *(Not fully implemented)*
*   **Test Case 5.1.3**: Refresh page after login. Verify session persists via localStorage. ✅ Works

### User Story 5.2: User Logout
*   **As a** User,
*   **I want to** log out of the system,
*   **So that** my session is terminated securely.
*   **Status**: ✅ Implemented
*   **Test Case 5.2.1**: Click logout button in sidebar footer. Verify redirect to Login page.
*   **Test Case 5.2.2**: After logout, try accessing `/projects`. Verify redirect to Login.

### User Story 5.3-5.6: Role-Based Access Control
*   **Status**: 📋 Planned but NOT enforced
*   **Current Behavior**: All users have full access to all endpoints
*   **Test Cases**: Cannot be validated until RBAC is implemented

**Note**: The following user stories are designed but awaiting implementation:
- User Story 5.3: Role-Based Project Visibility (Employee)
- User Story 5.4: Role-Based Edit Permissions (Manager)
- User Story 5.5: Full Access (Partner/Admin)
- User Story 5.6: Role Assignment

---

## Epic 6: Notifications & User Feedback
**Description**: Keeping users informed of system actions and events.  
**Status**: ✅ Fully Implemented

### User Story 6.1: Action Feedback (Toast Notifications)
*   **As a** User,
*   **I want to** see toast notifications when I perform actions,
*   **So that** I know if my action succeeded or failed.
*   **Status**: ✅ Implemented with `react-hot-toast` + `StatusCapsule`
*   **Test Case 6.1.1**: Save a resource. Verify green success toast appears at top-center.
*   **Test Case 6.1.2**: Trigger an error (e.g., duplicate SAP code). Verify red error toast.
*   **Test Case 6.1.3**: Verify toast auto-dismisses after 3-4 seconds.

### User Story 6.2: Notification History (Bell Icon)
*   **As a** User,
*   **I want to** view past notifications in a panel,
*   **So that** I can review actions I may have missed.
*   **Status**: ✅ Implemented
*   **Test Case 6.2.1**: Trigger multiple notifications. Click bell icon. Verify panel shows history.
*   **Test Case 6.2.2**: Verify unread count badge on bell icon.
*   **Test Case 6.2.3**: Click "Mark all read". Verify badge disappears.
*   **Test Case 6.2.4**: Click "Clear all". Verify notification list empties.

---

## Epic 7: Global Navigation & Search
**Description**: Quick access to any part of the application.  
**Status**: ✅ Fully Implemented

### User Story 7.1: Command Palette (Quick Search)
*   **As a** User,
*   **I want to** press Ctrl+K to open a command palette,
*   **So that** I can quickly navigate or perform actions.
*   **Status**: ✅ Implemented
*   **Test Case 7.1.1**: Press Ctrl+K. Verify command palette opens.
*   **Test Case 7.1.2**: Type "roster". Verify "Go to Roster" appears in results.
*   **Test Case 7.1.3**: Press Enter. Verify navigates to Roster page.
*   **Test Case 7.1.4**: Click search icon in header. Verify palette opens.
*   **Test Case 7.1.5**: Type "create project". Verify quick action appears.

### User Story 7.2: Breadcrumb Navigation
*   **As a** User,
*   **I want to** see breadcrumbs showing my current location,
*   **So that** I can navigate back easily.
*   **Status**: ✅ Implemented with `NavigationContext`
*   **Test Case 7.2.1**: Navigate to Project > Forecast. Verify breadcrumb shows: Resource Platform > Projects > [Project Name] > Forecast.
*   **Test Case 7.2.2**: Click "Projects" in breadcrumb. Verify navigates to Projects list.
*    **Test Case 7.2.3**: Verify current page segment is not clickable (disabled).

### User Story 7.3: Contextual Sidebar Sub-Navigation (**NEW**)
*   **As a** User,
*   **I want to** see project-specific sub-menu items in the sidebar when viewing a project,
*   **So that** I can quickly switch between project sections.
*   **Status**: ✅ Implemented
*   **Test Case 7.3.1**: Click on a project. Verify sidebar shows: [Project Name] (header), Overview, Billings, Expenses, Forecast.
*   **Test Case 7.3.2**: Click "Billings" in sidebar. Verify navigates to Billings page.
*   **Test Case 7.3.3**: Verify active sub-item is highlighted.

### User Story 7.4: Theme Toggle
*   **As a** User,
*   **I want to** switch between dark and light modes,
*   **So that** I can use my preferred visual style.
*   **Status**: ✅ Implemented
*   **Test Case 7.4.1**: Click sun/moon icon. Verify theme changes (background, text colors update).
*   **Test Case 7.4.2**: Refresh page. Verify theme preference persists (stored in localStorage).

---

## Epic 8: Snapshot Workflow Management (**NEW EPIC**)
**Description**: Advanced monthly financial workflow with state management.  
**Status**: ✅ Fully Implemented

### User Story 8.1: View Snapshot Matrix
*   **As a** PM,
*   **I want to** see all monthly snapshots in a matrix view with clear status indicators,
*   **So that** I understand the financial state and workflow position.
*   **Status**: ✅ Implemented
*   **Test Case 8.1.1**: Navigate to Project Overview. Verify matrix loads with all project months.
*   **Test Case 8.1.2**: Verify columns are color-coded by status: Pending (striped), Editable (green), Confirmed (lock icon).
*   **Test Case 8.1.3**: Verify financial KPIs display: OB, WIP, CB, DE, OC, NSR, Margin.

### User Story 8.2: Edit Mode Control
*   **As a** PM,
*   **I want to** explicitly enable edit mode before modifying values,
*   **So that** I don't accidentally change data.
*   **Status**: ✅ Implemented
*   **Test Case 8.2.1**: Verify edit icon (pencil) appears only in Editable column header.
*   **Test Case 8.2.2**: Click pencil icon. Verify fields become editable and actions row appears.
*   **Test Case 8.2.3**: Verify actions row shows: Save (disabled if no changes), Reset, Confirm buttons.
*   **Test Case 8.2.4**: Click X icon. Verify edit mode exits and actions row disappears.

### User Story 8.3: Unsaved Changes Warning
*   **As a** PM,
*   **I want to** see clear indication when I have unsaved changes,
*   **So that** I don't lose work.
*   **Status**: ✅ Implemented
*   **Test Case 8.3.1**: Modify a value in edit mode. Verify Save button becomes enabled.
*   **Test Case 8.3.2**: Try to click Confirm. Verify error toast: "Please save your changes before confirming".
*   **Test Case 8.3.3**: Click Reset. Verify unsaved changes are discarded (no API call if not saved).

### User Story 8.4: Diff Preview Before Save
*   **As a** PM,
*   **I want to** see a summary of my changes before saving,
*   **So that** I can review what I'm modifying.
*   **Status**: ✅ Implemented
*   **Test Case 8.4.1**: Make multiple value changes. Click Save.
*   **Test Case 8.4.2**: Verify diff dialog shows: Field name, Old value (strikethrough), New value (green highlight).
*   **Test Case 8.4.3**: Click Cancel. Verify changes are not saved.
*   **Test Case 8.4.4**: Click Confirm Save. Verify API call succeeds and toast shows "Changes saved successfully".

### User Story 8.5: Conditional Actions Row (**UI OPTIMIZATION**)
*   **As a** User,
*   **I want to** see action buttons only when edit mode is active,
*   **So that** the UI is clean and doesn't have unnecessary empty space.
*   **Status**: ✅ Implemented (2026-02-03)
*   **Test Case 8.5.1**: Verify actions row does NOT render when edit mode is OFF.
*   **Test Case 8.5.2**: Enable edit mode. Verify actions row appears below matrix.
*   **Test Case 8.5.3**: Disable edit mode. Verify actions row disappears and no empty space remains.

---

## Test Execution Priority

### P0 - Critical (Must Pass Before Release)
- ✅ Epic 3: Snapshot workflow (override, confirm, clear)
- ✅ Epic 6: Notification system
- ✅ Epic 1: Roster CRUD and cost calculation
- ✅ Epic 2: Project creation and navigation

### P1 - High (Important Features)
- ✅ Epic 7: Command palette and navigation
- ✅ Epic 4: Global rates management
- ✅ Epic 8: Snapshot matrix UI

### P2 - Medium (Enhancement Validation)
- ✅ Theme toggle and persistence
- ✅ Excel import/export

### P3 - Low (Future)
- ⚠️ Epic 5: RBAC enforcement (not yet implemented)

---

## Regression Test Checklist

Run these tests after any significant change:

### Core Financial Calculations
- [ ] Create allocation → Verify WIP calculates correctly
- [ ] Enter billing → Verify cumulative billings update
- [ ] Enter expense → Verify NSR recalculates
- [ ] Override value → Verify original stored → Clear override → Verify restoration

### Workflow State Transitions
- [ ] Confirm month → Verify status changes to Confirmed
- [ ] Verify next month becomes Editable
- [ ] Verify confirmed month is read-only

### UI Consistency
- [ ] Check Dark/Light theme switch across all pages
- [ ] Verify breadcrumbs update correctly during navigation
- [ ] Verify sidebar sub-items appear/disappear appropriately
- [ ] Verify notifications appear and persist in history

---

**Document Version**: 3.0  
**Last Validated**: 2026-02-03  
**Reflects**: Current production implementation  
**End of Document**
