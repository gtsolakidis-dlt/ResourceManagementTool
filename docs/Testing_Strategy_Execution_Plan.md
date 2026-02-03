# Testing Strategy & Execution Plan
**Project**: Resource Management Tool  
**Version**: 3.0  
**Date**: 2026-02-03  
**Test Data Version**: 3.0  
**Status**: Ready for Execution

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Test Environment Setup](#test-environment-setup)
3. [Test Data Validation Summary](#test-data-validation-summary)
4. [Testing Phases](#testing-phases)
5. [Detailed Test Execution Steps](#detailed-test-execution-steps)
6. [Test Coverage Matrix](#test-coverage-matrix)
7. [Defect Management](#defect-management)
8. [Success Criteria](#success-criteria)

---

## 1. Executive Summary

### 1.1 Purpose
This document provides a comprehensive, step-by-step testing strategy for validating the Resource Management Tool against all documented requirements and test cases.

### 1.2 Scope
- **In Scope**: All implemented features (Roster, Projects, Financials, Snapshot Workflow, Navigation, Notifications, Theme)
- **Out of Scope**: RBAC/Authentication (placeholder only - marked as future enhancement)

### 1.3 Test Approach
- **Type**: Manual functional testing with exploratory elements
- **Methodology**: Requirements-based testing mapped to User Stories
- **Priority**: Risk-based (P0-P3 classification)
- **Coverage**: 8 Epics, 35+ User Stories, 80+ Test Cases

### 1.4 Test Summary

| Epic | Status | Test Cases | Priority |
|------|--------|-----------|----------|
| Epic 1: Roster Management | ✅ Testable | 9 | P0 |
| Epic 2: Project Management | ✅ Testable | 8 | P0 |
| Epic 3: Financials & Forecasting | ✅ Testable | 22 | P0 |
| Epic 4: Global Configuration | ✅ Testable | 4 | P1 |
| Epic 5: Authentication & Access | ⚠️ Partial | 8 (2 testable) | P3 |
| Epic 6: Notifications | ✅ Testable | 8 | P1 |
| Epic 7: Navigation & Search | ✅ Testable | 12 | P1 |
| Epic 8: Snapshot Workflow | ✅ Testable | 15+ | P0 |

---

## 2. Test Environment Setup

### 2.1 Prerequisites

#### Database Setup
```powershell
# Step 1: Ensure database is initialized
cd C:\Users\gtsolakidis\.gemini\antigravity\scratch\ResourceManagementTool
dotnet run --project ResourceManagement.DbMigrator
```

#### Load Test Data
```powershell
# Step 2: Execute updated test data script
# Open SQL Server Management Studio or Azure Data Studio
# Connect to: Server=GRPC012824\SQLEXPRESS;Database=ResourceManagementDb
# Open: tools/TestData_Seed_v3.sql
# Execute entire script
# Verify success messages in output
```

#### Start Applications
```powershell
# Step 3: Start Backend API
cd C:\Users\gtsolakidis\.gemini\antigravity\scratch\ResourceManagementTool
dotnet run --project ResourceManagement.Api
# Verify: API running on https://localhost:7290

# Step 4: Start Frontend (new terminal)
cd C:\Users\gtsolakidis\.gemini\antigravity\scratch\ResourceManagementTool\frontend
npm run dev
# Verify: Frontend running on http://localhost:5173
```

### 2.2 Test Browser Setup
- **Browser**: Chrome or Edge (latest version)
- **Extensions**: None required (disable ad-blockers)
- **Clear State**: Clear localStorage, cookies before starting
- **Developer Tools**: Keep open (F12) to monitor console errors

### 2.3 Test Accounts
**Note**: Authentication is placeholder only. Manual login simulation may be required.

```
Planned Test Users (for future RBAC testing):
- admin / admin123      [Admin Role]
- partner / admin123    [Partner Role]
- manager / admin123    [Manager Role]
- employee / admin123   [Employee Role]
```

### 2.4 Environment Validation Checklist
Before starting tests, verify:
- [ ] Database has test data (run verification queries from TestData_Seed_v3.sql)
- [ ] Backend API responds at `/api/health`
- [ ] Frontend loads without console errors
- [ ] Dark theme is applied by default
- [ ] No existing snapshots for TEST.003 project (fresh state)

---

## 3. Test Data Validation Summary

### 3.1 Issues Found & Fixed

#### ❌ Issue 1: Missing Snapshot Test Data
**Problem**: Original test data (v2.0) had NO snapshot-related test project.  
**Impact**: Epic 8 test cases could not be executed.  
**Fix**: Added **TEST.003 - Snapshot Workflow Project** with 6 months of allocations, billings, and expenses.

#### ❌ Issue 2: Incorrect Field Names
**Problem**: Test data used `EmployerContributions` (old field name).  
**Impact**: Script would fail on INSERT.  
**Fix**: Updated all references to `MonthlyEmployerContributions`.

#### ❌ Issue 3: Username/Password Fields Don't Exist
**Problem**: Test data attempted to insert `Username`, `PasswordHash`, `Role` into Roster table.  
**Impact**: Script execution failure.  
**Fix**: Removed these fields, added comments for "FUTURE RBAC".

#### ❌ Issue 4: Missing CostCenterCode Field
**Problem**: Roster INSERT statements missing required `CostCenterCode`.  
**Impact**: Potential constraint violation.  
**Fix**: Added CostCenterCode for all test roster members.

#### ❌ Issue 5: Insufficient Global Rate Coverage
**Problem**: Missing 'BA' (Business Analyst) level rate.  
**Impact**: BA allocations would result in €0 WIP.  
**Fix**: Added BA rate (€250/day).

#### ❌ Issue 6: No Project-Specific Rate Test Data
**Problem**: ProjectRate table feature not covered in test data.  
**Impact**: Test case 4.2.2 cannot execute.  
**Fix**: Added ProjectRate for TEST.003 project (SC level override to €550).

### 3.2 Test Data Coverage

| Entity | v2.0 Count | v3.0 Count | Coverage Improvement |
|--------|-----------|-----------|---------------------|
| Global Rates | 7 | 8 | +1 (BA level) |
| Roster Members | 9 | 11 | +2 (more diversity) |
| Projects | 5 | 6 | +1 (Snapshot test project) |
| Forecast Versions | 3 | 4 | +1 (Snapshot project version) |
| Resource Allocations | 5 | 24 | +19 (6 months × 3 resources) |
| Billings | 2 | 7 | +5 (6 months Snapshot project) |
| Expenses | 2 | 7 | +5 (6 months Snapshot project) |
| Project Rates | 0 | 1 | +1 (NEW entity) |

### 3.3 Test Projects Overview

| Project WBS | Purpose | Test Cases Covered |
|-------------|---------|-------------------|
| **TEST.001** | Budget Calculation | TC 2.1.1, 2.1.2 |
| **TEST.002** | WIP Calculation | TC 3.2.1, 3.2.2 |
| **TEST.003** | **Snapshot Workflow** | **TC 3.4-3.8, Epic 8 (all)** |
| **TEST.004** | Manager RBAC | TC 5.4.1-5.4.3 (FUTURE) |
| **TEST.005** | Unassigned RBAC | TC 5.4.3 (FUTURE) |
| **TEST.006** | Employee RBAC | TC 5.3.1-5.3.2 (FUTURE) |

---

## 4. Testing Phases

### Phase 1: Smoke Testing (30 minutes)
**Objective**: Verify basic functionality and environment stability.

**Tests**:
1. Application loads without errors
2. Can navigate to all main pages
3. Test data visible in Roster and Projects
4. Theme toggle works
5. Command Palette opens (Ctrl+K)

**Exit Criteria**: All 5 tests pass, no console errors.

---

### Phase 2: Core Financial Workflow (P0 - 2 hours)
**Objective**: Validate critical financial calculations and snapshot workflow.

**Sequence**:
1. **Epic 3 - Financial Calculations** (30 min)
   - WIP calculation accuracy
   - Billings/Expenses horizontal input
   - NSR and Margin derivation

2. **Epic 8 - Snapshot Workflow** (90 min)
   - View snapshot matrix
   - Edit mode toggle
   - Manual override with audit trail
   - Clear override
   - Confirm/lock month

**Exit Criteria**: All P0 test cases pass, snapshot workflow operates correctly.

---

### Phase 3: Data Management (P0 - 1 hour)
**Objective**: Validate CRUD operations for Roster and Projects.

**Tests**:
1. **Epic 1 - Roster Management**
   - View, search, filter roster
   - Create/edit roster member
   - Cost calculation accuracy

2. **Epic 2 - Project Management**
   - Create project
   - Nominal budget auto-calculation
   - Project navigation

**Exit Criteria**: All CRUD operations successful, calculations accurate.

---

### Phase 4: User Experience Features (P1 - 1 hour)
**Objective**: Validate navigation, notifications, and global configuration.

**Tests**:
1. **Epic 7 - Navigation & Search**
   - Command Palette
   - Breadcrumbs
   - Contextual sidebar
   - Theme toggle

2. **Epic 6 - Notifications**
   - Toast notifications
   - Notification center
   - History management

3. **Epic 4 - Global Rates**
   - View/edit rates
   - Rate updates reflect in WIP

**Exit Criteria**: All UX features work smoothly, no navigation issues.

---

### Phase 5: Regression & Exploratory (P1-P2 - 1.5 hours)
**Objective**: Execute regression checklist and exploratory testing.

**Tests**:
1. Run regression checklist (see Section 7)
2. Exploratory: Edge cases, error handling
3. Cross-browser (if time permits)

**Exit Criteria**: No critical regressions, edge cases handled gracefully.

---

### Phase 6: RBAC Placeholder Validation (P3 - 30 minutes)
**Objective**: Document RBAC placeholders for future implementation.

**Tests**:
1. Verify login page exists
2. Verify logout works
3. Document non-enforced RBAC behavior

**Exit Criteria**: RBAC status documented, no blockers for future implementation.

---

## 5. Detailed Test Execution Steps

### PHASE 1: SMOKE TESTING (30 min)

#### Test Suite 1.1: Application Launch
**Duration**: 5 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 1.1.1 | Navigate to `http://localhost:5173` | Login page loads, dark theme applied | ☐ |
| 1.1.2 | Open DevTools Console (F12) | No red errors visible | ☐ |
| 1.1.3 | Enter any credentials and login | Navigate to Dashboard | ☐ |
| 1.1.4 | Verify sidebar visible | Shows: Dashboard, Projects, Team, System sections | ☐ |

#### Test Suite 1.2: Navigation Smoke Test
**Duration**: 10 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 1.2.1 | Click "Team" > "Roster" | Navigate to Roster page | ☐ |
| 1.2.2 | Verify test data | See roster members with TEST_ prefix | ☐ |
| 1.2.3 | Click "Projects" | See project cards including TEST_ projects | ☐ |
| 1.2.4 | Click on "TEST_Snapshot Workflow Project" | Navigate to Project Overview | ☐ |
| 1.2.5 | Verify sidebar updates | Shows Overview, Billings, Expenses, Forecast sub-items | ☐ |
| 1.2.6 | Click "System" > "Global Rates" | See 8 rate levels | ☐ |

#### Test Suite 1.3: Quick Feature Checks
**Duration**: 15 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 1.3.1 | Press `Ctrl+K` | Command palette opens | ☐ |
| 1.3.2 | Type "roster" | "Go to Roster" command appears | ☐ |
| 1.3.3 | Press Enter | Navigate to Roster page | ☐ |
| 1.3.4 | Click sun/moon icon (theme toggle) | Theme switches to light mode | ☐ |
| 1.3.5 | Click theme icon again | Return to dark mode | ☐ |
| 1.3.6 | Click bell icon (notifications) | Notification panel opens | ☐ |

**Smoke Test Exit Criteria**: ✅ All steps pass = Proceed to Phase 2. ❌ Any failure = Fix before continuing.

---

### PHASE 2: CORE FINANCIAL WORKFLOW (2 hours)

#### Test Suite 2.1: WIP Calculation Accuracy (Epic 3, TC 3.2.1)
**Duration**: 15 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.1.1 | Navigate to Projects | Find "TEST_WIP Calculation Project" | ☐ |
| 2.1.2 | Click on project card | View Project Overview | ☐ |
| 2.1.3 | Scroll to snapshot matrix | See January 2026 column | ☐ |
| 2.1.4 | Locate WIP row, Jan column | **Expected WIP = €4,500.00** | ☐ |
| 2.1.5 | Verify calculation | 10 days × €500 (SC rate) × 0.9 (10% discount) = €4,500 | ☐ |
| 2.1.6 | **PASS CRITERIA** | WIP matches expected value exactly | ☐ |

**Formula Breakdown**:
```
John Smith (TEST_JOHN_001) Level: SC
Global Rate for SC: €500/day
Project Discount: 10%
Allocated Days: 10
WIP = 10 × 500 × (1 - 0.10) = 10 × 500 × 0.9 = €4,500
```

#### Test Suite 2.2: Snapshot Matrix Display (Epic 8, TC 8.1.1-8.1.3)
**Duration**: 20 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.2.1 | Navigate to "TEST_Snapshot Workflow Project" | Project Overview loads | ☐ |
| 2.2.2 | Verify breadcrumbs | "Resource Platform > Projects > TEST_Snapshot... > Overview" | ☐ |
| 2.2.3 | **Check January 2026 column** | Background: Light green tint, Left/Right green border | ☐ |
| 2.2.4 | **Check January header** | Shows "EDITABLE" label or pencil icon | ☐ |
| 2.2.5 | **Check Feb-Jun columns** | Background: Diagonal striped pattern | ☐ |
| 2.2.6 | **Check Feb-Jun headers** | Show "PENDING" label or muted styling | ☐ |
| 2.2.7 | Verify financial rows visible | OB, WIP, CB, DE, OC, NSR, Margin | ☐ |
| 2.2.8 | Check NSR and Margin rows | Distinct background color (summary style) | ☐ |
| 2.2.9 | Verify no override indicators | No green dots on any cells (fresh data) | ☐ |
| 2.2.10 | **PASS CRITERIA** | All visual status indicators correct | ☐ |

**Visual Reference**:
```
Status Indicators:
✅ Editable (Jan):   Green tint + borders + edit icon
✅ Pending (Feb-Jun): Striped pattern + muted text
❌ Confirmed:         Would show lock icon + blue tint (not present initially)
```

#### Test Suite 2.3: Edit Mode Toggle (Epic 8, TC 8.2.1-8.2.4)
**Duration**: 15 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.3.1 | Locate January 2026 column header | Find pencil icon (edit mode OFF) | ☐ |
| 2.3.2 | **Click pencil icon** | Edit mode activates | ☐ |
| 2.3.3 | Verify visual changes | Icon changes to X, fields become editable | ☐ |
| 2.3.4 | **Scroll down below matrix** | Actions row appears (Save, Reset, Confirm buttons) | ☐ |
| 2.3.5 | Verify button states | Save: Disabled (no changes), Reset: Enabled, Confirm: Enabled | ☐ |
| 2.3.6 | Click in WIP cell (Jan) | Cursor appears, can type | ☐ |
| 2.3.7 | **Don't save yet** | Note current WIP value | ☐ |
| 2.3.8 | **Click X icon** (exit edit mode) | Edit mode deactivates | ☐ |
| 2.3.9 | Verify actions row hidden | Row disappears, **no empty space below matrix** | ☐ |
| 2.3.10 | **PASS CRITERIA** | Edit mode toggle works, conditional UI renders correctly | ☐ |

**Critical Check**: Step 2.3.9 validates the recent UI fix (conditional actions row rendering).

#### Test Suite 2.4: Manual Override with Diff Preview (Epic 8, TC 8.4.1-8.4.4)
**Duration**: 25 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.4.1 | Enable edit mode (click pencil) | Edit mode active | ☐ |
| 2.4.2 | Note **original WIP value** for Jan | Example: €27,000 (calculated) | ☐ |
| 2.4.3 | Click in WIP cell, change to **€30,000** | Value updates in cell | ☐ |
| 2.4.4 | Verify Save button enabled | Button no longer disabled | ☐ |
| 2.4.5 | **Click Save button** | Diff dialog appears | ☐ |
| 2.4.6 | **Verify diff dialog content** | Shows: "WIP", Old: €27,000 (strikethrough), New: €30,000 (green) | ☐ |
| 2.4.7 | Click "Cancel" in dialog | Dialog closes, no save occurs | ☐ |
| 2.4.8 | Verify WIP still shows **€30,000** | Local state preserved | ☐ |
| 2.4.9 | Click Save again | Diff dialog reappears | ☐ |
| 2.4.10 | **Click "Confirm Save"** | Dialog closes, success toast appears | ☐ |
| 2.4.11 | **Check for green dot** | Small green circle appears top-right of WIP cell | ☐ |
| 2.4.12 | **Hover over green dot** | Tooltip shows: Original €27,000, Overridden At, Overridden By | ☐ |
| 2.4.13 | Check "Last saved" timestamp | Appears near buttons with current time | ☐ |
| 2.4.14 | **Refresh page (F5)** | Page reloads | ☐ |
| 2.4.15 | **Verify override persists** | WIP still €30,000, green dot still visible | ☐ |
| 2.4.16 | **PASS CRITERIA** | Override saved, audit trail visible, persistence confirmed | ☐ |

**Data Validation**:
```sql
-- Run this query to verify backend storage:
SELECT Month, Wip, OriginalWip, IsOverridden, OverriddenBy, OverriddenAt
FROM ProjectMonthlySnapshot
WHERE ProjectId = (SELECT Id FROM Project WHERE Wbs = 'TEST.003')
  AND Month = '2026-01-01';

Expected Result:
- Wip: 30000.00
- OriginalWip: 27000.00 (approximate, based on actual calculation)
- IsOverridden: 1 (TRUE)
- OverriddenBy: [current user]
- OverriddenAt: [recent timestamp]
```

#### Test Suite 2.5: Clear Override (Epic 8, TC 3.6.1-3.6.5)
**Duration**: 15 minutes

**Prerequisite**: Test Suite 2.4 completed (override exists)

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.5.1 | Verify edit mode active | If not, click pencil icon | ☐ |
| 2.5.2 | Verify WIP shows **€30,000** (overridden) | Green dot visible | ☐ |
| 2.5.3 | **Click Reset button** | Confirmation dialog appears | ☐ |
| 2.5.4 | **Read dialog text** | "Clear Manual Overrides? This will restore calculated values" | ☐ |
| 2.5.5 | Click "Cancel" | Dialog closes, no change | ☐ |
| 2.5.6 | Click Reset again | Dialog reappears | ☐ |
| 2.5.7 | **Click "Confirm"** | Dialog closes, success toast | ☐ |
| 2.5.8 | **Check WIP value** | Reverts to original €27,000 (approx) | ☐ |
| 2.5.9 | **Verify green dot removed** | No override indicator visible | ☐ |
| 2.5.10 | Check subsequent months | Feb-Jun recalculate (cumulative values may change) | ☐ |
| 2.5.11 | **PASS CRITERIA** | Override cleared, calculated value restored, audit trail removed | ☐ |

**Backend Verification**:
```sql
-- Verify IsOverridden = 0 and OriginalWip = NULL
SELECT Wip, OriginalWip, IsOverridden
FROM ProjectMonthlySnapshot
WHERE ProjectId = (SELECT Id FROM Project WHERE Wbs = 'TEST.003')
  AND Month = '2026-01-01';

Expected:
- Wip: ~27000 (calculated)
- OriginalWip: NULL
- IsOverridden: 0 (FALSE)
```

#### Test Suite 2.6: Confirm/Lock Month (Epic 8, TC 3.7.1-3.7.5)
**Duration**: 20 minutes

**⚠️ WARNING**: This test is IRREVERSIBLE. Confirm only AFTER all other tests on January month are complete.

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.6.1 | Verify edit mode active | Actions row visible | ☐ |
| 2.6.2 | Ensure no unsaved changes | Save button should be disabled | ☐ |
| 2.6.3 | **Click Confirm button** | Warning dialog appears | ☐ |
| 2.6.4 | **Read dialog carefully** | "Lock Month Permanently? This action CANNOT be undone" | ☐ |
| 2.6.5 | Click "Cancel" | Dialog closes, no change | ☐ |
| 2.6.6 | Click Confirm again | Dialog reappears | ☐ |
| 2.6.7 | **Click "Yes, Lock Month"** | Dialog closes, processing toast | ☐ |
| 2.6.8 | Wait for page refresh | Matrix reloads | ☐ |
| 2.6.9 | **Check January column** | Background changes: green → blue/solid, lock icon appears | ☐ |
| 2.6.10 | **Check January header** | "CONFIRMED" or "LOCKED" label, NO edit icon | ☐ |
| 2.6.11 | **Check February column** | Now shows green tint (promoted to Editable) | ☐ |
| 2.6.12 | **Check February header** | Shows pencil icon (new active month) | ☐ |
| 2.6.13 | Try to click January cells | Fields are read-only, no cursor | ☐ |
| 2.6.14 | Click pencil on February | Edit mode works for new editable month | ☐ |
| 2.6.15 | **PASS CRITERIA** | Workflow progressed: Jan locked, Feb editable | ☐ |

**Database Verification**:
```sql
SELECT Month, Status, ConfirmedAt, ConfirmedBy
FROM ProjectMonthlySnapshot
WHERE ProjectId = (SELECT Id FROM Project WHERE Wbs = 'TEST.003')
ORDER BY Month;

Expected:
- Jan 2026: Status = 2 (Confirmed), ConfirmedAt = [timestamp], ConfirmedBy = [user]
- Feb 2026: Status = 1 (Editable)
- Mar-Jun: Status = 0 (Pending)
```

#### Test Suite 2.7: Unsaved Changes Warning (Epic 8, TC 8.3.1-8.3.3)
**Duration**: 10 minutes

**Prerequisite**: February is now Editable (after Suite 2.6)

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 2.7.1 | Enable edit mode on February | Pencil icon clicked | ☐ |
| 2.7.2 | Modify any financial value | Change visible in cell | ☐ |
| 2.7.3 | **Don't click Save** | Save button becomes enabled | ☐ |
| 2.7.4 | **Click Confirm button** | Error toast: "Please save your changes before confirming" | ☐ |
| 2.7.5 | Verify month NOT confirmed | Dialog doesn't appear, month remains editable | ☐ |
| 2.7.6 | **Click Reset button** | Confirmation dialog appears | ☐ |
| 2.7.7 | Click Confirm | Changes discarded (no API call since not saved) | ☐ |
| 2.7.8 | **PASS CRITERIA** | Unsaved changes prevent confirm, reset discards correctly | ☐ |

---

### PHASE 3: DATA MANAGEMENT (1 hour)

#### Test Suite 3.1: Roster Management (Epic 1)
**Duration**: 30 minutes

**TC 1.1.1: View Roster**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.1.1 | Navigate to Team > Roster | Roster page loads | ☐ |
| 3.1.2 | Verify table columns | Name, SAP Ref, Function, Seniority (Level), Monthly Cost visible | ☐ |
| 3.1.3 | Count test members | Should see 11 members with TEST_ prefix | ☐ |

**TC 1.3.1: Search Roster**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.1.4 | Locate search box | Top of roster table | ☐ |
| 3.1.5 | Type "John" | Filter applies | ☐ |
| 3.1.6 | Verify results | See "John Smith" (TEST_JOHN_001) | ☐ |
| 3.1.7 | Clear search | All members visible again | ☐ |

**TC 1.3.2: Filter by Level**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.1.8 | Locate level filter dropdown | May be near search box | ☐ |
| 3.1.9 | Select "SC" from dropdown | Filter applies | ☐ |
| 3.1.10 | Count results | Should see 4 SC members (John, Maria, Andreas, Employee) | ☐ |
| 3.1.11 | Clear filter | All members visible | ☐ |

**TC 1.1.2: Edit Roster Member**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.1.12 | Click Edit on "John Smith" | Edit modal opens | ☐ |
| 3.1.13 | Verify editable fields | MonthlySalary, MonthlyEmployerContributions, Cars, TicketRestaurant, Metlife | ☐ |
| 3.1.14 | Change MonthlySalary to 5000 | Value updates | ☐ |
| 3.1.15 | Click Save | Success toast, modal closes | ☐ |
| 3.1.16 | Verify update persisted | Monthly Cost column updated (reflects new salary) | ☐ |
| 3.1.17 | **Revert change** | Edit again, set back to 4500 | ☐ |

**Cost Calculation Verification**:
```
John Smith Original Values:
- MonthlySalary: €4,500
- MonthlyEmployerContributions: €900
- Cars: €200
- TicketRestaurant: €150
- Metlife: €60

14-Month Cost = ((4500 + 900) * 14 / 12) + 200 + 150 + 60
              = (5400 * 1.1667) + 410
              = 6300 + 410
              = €6,710

Daily Cost = 6710 / 18 = €372.78
```

#### Test Suite 3.2: Project Management (Epic 2)
**Duration**: 30 minutes

**TC 2.1.1: Nominal Budget Calculation**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.2.1 | Navigate to Projects page | Project cards visible | ☐ |
| 3.2.2 | Locate "TEST_Budget Calculation Project" | Card displays | ☐ |
| 3.2.3 | Verify Actual Budget | Shows €100,000 | ☐ |
| 3.2.4 | Verify Nominal Budget | Shows €125,000 | ☐ |
| 3.2.5 | **Verify calculation** | 100000 / (1 - 0.20) = 100000 / 0.80 = 125000 ✓ | ☐ |

**TC 2.1.2: Create Project**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.2.6 | Click "Create Project" or "+" button | Modal opens | ☐ |
| 3.2.7 | Enter Name: "Test Dynamic Budget Project" | Value accepted | ☐ |
| 3.2.8 | Enter WBS: "TEST.999" | Value accepted | ☐ |
| 3.2.9 | Select dates: 2026-01-01 to 2026-12-31 | Dates accepted | ☐ |
| 3.2.10 | Enter Actual Budget: **€150,000** | Value accepted | ☐ |
| 3.2.11 | Enter Discount: **15%** | Value accepted | ☐ |
| 3.2.12 | **Check Nominal Budget field** | Auto-calculates to €176,470.59 | ☐ |
| 3.2.13 | Verify calculation | 150000 / (1 - 0.15) = 150000 / 0.85 = 176470.59 ✓ | ☐ |
| 3.2.14 | Enter Recoverability: 90%, Target Margin: 25% | Values accepted | ☐ |
| 3.2.15 | Click Save | Success toast | ☐ |
| 3.2.16 | **Verify new card appears** | "Test Dynamic Budget Project" visible | ☐ |
| 3.2.17 | **Cleanup: Delete project** | Click delete, confirm | ☐ |

**TC 2.2.1-2.2.3: Project Navigation**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 3.2.18 | Click on "TEST_Snapshot Workflow Project" card | Navigate to Overview | ☐ |
| 3.2.19 | **Verify sidebar updates** | Shows project name + sub-items (Overview, Billings, Expenses, Forecast) | ☐ |
| 3.2.20 | **Verify breadcrumbs** | "Resource Platform > Projects > TEST_Snapshot... > Overview" | ☐ |
| 3.2.21 | Click "Billings" in sidebar | Navigate to Billings page | ☐ |
| 3.2.22 | **Verify breadcrumb updates** | Last segment changes to "Billings" | ☐ |
| 3.2.23 | Click "Projects" in breadcrumb | Navigate back to projects list | ☐ |
| 3.2.24 | **PASS CRITERIA** | Navigation works, breadcrumbs/sidebar update correctly | ☐ |

---

### PHASE 4: USER EXPERIENCE FEATURES (1 hour)

#### Test Suite 4.1: Command Palette (Epic 7, TC 7.1.1-7.1.5)
**Duration**: 15 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.1.1 | From any page, press **Ctrl+K** | Command palette modal opens | ☐ |
| 4.1.2 | Verify backdrop | Page behind is dimmed/blurred | ☐ |
| 4.1.3 | Type "roster" in search | "Go to Roster" command appears in list | ☐ |
| 4.1.4 | **Press Enter** | Palette closes, navigate to Roster page | ☐ |
| 4.1.5 | Press Ctrl+K again | Palette reopens | ☐ |
| 4.1.6 | Type "create project" | "Create Project" action appears | ☐ |
| 4.1.7 | Press Esc | Palette closes | ☐ |
| 4.1.8 | Click search icon in top bar | Palette opens (alternative method) | ☐ |
| 4.1.9 | Click outside palette | Closes without action | ☐ |
| 4.1.10 | **PASS CRITERIA** | Keyboard shortcut works, search filters commands, navigation executes | ☐ |

#### Test Suite 4.2: Notifications (Epic 6)
**Duration**: 15 minutes

**TC 6.1.1-6.1.3: Toast Notifications**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.2.1 | Navigate to Roster, click Edit on any member | Modal opens | ☐ |
| 4.2.2 | Change Monthly Salary, click Save | **Green success toast appears** top-center | ☐ |
| 4.2.3 | Read toast message | "Member updated successfully" or similar | ☐ |
| 4.2.4 | Wait 4 seconds | Toast auto-dismisses | ☐ |
| 4.2.5 | **Trigger an error** (e.g., duplicate SAP code) | **Red error toast appears** | ☐ |
| 4.2.6 | Verify toast styling | Red background, error icon | ☐ |

**TC 6.2.1-6.2.4: Notification Center**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.2.7 | After triggering 2+ toasts, click **bell icon** | Notification panel slides out | ☐ |
| 4.2.8 | **Verify unread badge** | Bell icon shows count (e.g., "2") | ☐ |
| 4.2.9 | Review notification list | See history of recent toasts | ☐ |
| 4.2.10 | Click "Mark all read" | Badge disappears | ☐ |
| 4.2.11 | Trigger another toast | New notification appears in panel | ☐ |
| 4.2.12 | Click "Clear all" | Notification list empties | ☐ |
| 4.2.13 | **PASS CRITERIA** | Notifications persist in history, read/clear actions work | ☐ |

#### Test Suite 4.3: Theme Toggle (Epic 7, TC 7.3.1-7.3.2)
**Duration**: 10 minutes

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.3.1 | Verify current theme | Dark mode (default) | ☐ |
| 4.3.2 | Note background color | Dark gray (~#121212) | ☐ |
| 4.3.3 | Click **sun/moon icon** in top bar | Theme switches to light | ☐ |
| 4.3.4 | Verify changes | Background: white/light gray, text: dark | ☐ |
| 4.3.5 | Click icon again | Return to dark mode | ☐ |
| 4.3.6 | **Refresh page (F5)** | Page reloads | ☐ |
| 4.3.7 | **Verify theme persists** | Still dark mode (stored in localStorage) | ☐ |
| 4.3.8 | Switch to light, refresh again | Light mode persists | ☐ |
| 4.3.9 | **PASS CRITERIA** | Theme toggles smoothly, preference persists across sessions | ☐ |

#### Test Suite 4.4: Global Rates (Epic 4)
**Duration**: 20 minutes

**TC 4.1.1: View Rates**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.4.1 | Navigate to System > Global Rates | Rates page loads | ☐ |
| 4.4.2 | Count rate entries | Should see 8 levels (BA, A, C, SC, M, SM, D, P) | ☐ |
| 4.4.3 | Verify SC rate | Shows €500.00 | ☐ |
| 4.4.4 | Verify M rate | Shows €800.00 | ☐ |

**TC 4.1.2: Update Rate**

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.4.5 | Click Edit on "M" (Manager) rate | Edit mode or modal opens | ☐ |
| 4.4.6 | Change rate from €800 to **€850** | Value updates | ☐ |
| 4.4.7 | Click Save | Success toast | ☐ |
| 4.4.8 | **Verify updated timestamp** | "Updated At" column shows recent time | ☐ |
| 4.4.9 | Refresh page | Rate still shows €850 | ☐ |

**TC 4.2.2: Project-Specific Rate Override** (if UI exists)

| Step | Action | Expected Result | Status |
|------|--------|----------------|--------|
| 4.4.10 | Navigate to TEST_Snapshot Workflow Project | Project page loads | ☐ |
| 4.4.11 | Check for "Project Rates" section or tab | May be in settings/configuration | ☐ |
| 4.4.12 | **If UI exists**: Verify SC override shows €550 | Test data has ProjectRate for this project | ☐ |
| 4.4.13 | **If UI doesn't exist**: Document as enhancement needed | Note in test report | ☐ |

---

### PHASE 5: REGRESSION & EXPLORATORY (1.5 hours)

#### Test Suite 5.1: Regression Checklist
**Duration**: 45 minutes

Execute this checklist after any code changes:

| Area | Test | Expected Outcome | Status |
|------|------|-----------------|--------|
| **Financial Calculations** | Create allocation → Check WIP | WIP = Days × Rate × (1 - Discount) | ☐ |
| | Enter billing → Check cumulative | Cumulative Billings increments | ☐ |
| | Enter expense → Check NSR | NSR = WIP + CB - OB - Expenses | ☐ |
| | Override value → Clear override | Restoration to calculated value | ☐ |
| **Workflow State** | Confirm month → Check status | Status = Confirmed, lock icon visible | ☐ |
| | Verify next month promotion | Next month becomes Editable | ☐ |
| | Try to edit confirmed month | Fields are read-only | ☐ |
| **UI Consistency** | Switch Dark/Light theme | All pages render correctly in both themes | ☐ |
| | Navigate: Dashboard → Projects → Roster → Settings | Breadcrumbs update on each page | ☐ |
| | Open project → Check sidebar | Sub-items appear (Overview, Billings, etc.) | ☐ |
| | Leave project → Check sidebar | Sub-items disappear | ☐ |
| **Notifications** | Trigger success action | Green toast appears, persists in history | ☐ |
| | Trigger error action | Red toast appears, error details shown | ☐ |
| | Bell icon badge | Updates with unread count | ☐ |

#### Test Suite 5.2: Exploratory Testing
**Duration**: 45 minutes

**Session 1: Edge Cases (20 min)**

Test these scenarios without strict steps:
1. **Large numbers**: Enter €999,999,999 in budget fields → Verify formatting, no overflow
2. **Zero values**: Set allocation to 0 days → Verify WIP = €0, no errors
3. **Negative values**: Try entering negative numbers → Verify validation prevents it
4. **Special characters**: Try entering "test@#$%" in text fields → Verify sanitization
5. **Very long names**: Enter 200-character project name → Verify truncation/ellipsis in UI
6. **Rapid clicking**: Click Save button 10 times rapidly → Verify debouncing prevents duplicate API calls
7. **Browser back button**: Navigate deep, hit Back → Verify state doesn't break

**Session 2: Error Handling (15 min)**

1. **Network Failure Simulation**:
   - Open DevTools → Network tab → Set to "Offline"
   - Try to save a roster member
   - **Expected**: Error toast with "Network error" or similar
   - Restore network, retry → Should work

2. **API Error**:
   - Try creating project with duplicate WBS code
   - **Expected**: Error toast with specific validation message

3. **Session Timeout** (if applicable):
   - Wait for session to expire (or manually clear localStorage)
   - Try to perform an action
   - **Expected**: Redirect to login or session expired message

**Session 3: Usability (10 min)**

Free exploration looking for:
- Confusing UI labels
- Misaligned elements
- Inconsistent spacing
- Missing tooltips where helpful
- Slow loading indicators
- Document findings in test report

---

### PHASE 6: RBAC PLACEHOLDER VALIDATION (30 min)

#### Test Suite 6.1: Authentication Placeholder Documentation
**Duration**: 30 minutes

**TC 5.1.1-5.1.3: Login/Logout**

| Step | Action | Expected Result | Status | Notes |
|------|--------|----------------|--------|-------|
| 6.1.1 | Open app in incognito window | Login page appears | ☐ | |
| 6.1.2 | Enter any credentials, click Login | Navigate to Dashboard | ☐ | Placeholder auth |
| 6.1.3 | Refresh page | Still logged in (session persists) | ☐ | Via localStorage |
| 6.1.4 | Click Logout in sidebar footer | Redirect to Login page | ☐ | |
| 6.1.5 | Try accessing /projects URL directly | Redirect to Login | ☐ | Route guard works |
| 6.1.6 | **Document**: Authentication is placeholder | Note in report: No validation, any credentials work | ☐ | |

**TC 5.3-5.6: RBAC Behavior (NOT ENFORCED)**

| Step | Action | Observation | Status | Notes |
|------|--------|-------------|--------|-------|
| 6.1.7 | Login as any user | All projects visible | ☐ | RBAC not enforced |
| 6.1.8 | Navigate to all pages (Roster, Projects, Settings) | All accessible | ☐ | No restrictions |
| 6.1.9 | Try editing any project | Edit controls visible | ☐ | `canEdit` flag not set |
| 6.1.10 | **Document**: RBAC designed but not implemented | List test cases 5.3-5.6 as "FUTURE TESTING" | ☐ | |
| 6.1.11 | **Verify test data readiness** | Check Roster table has TEST_ADMIN, TEST_MANAGER, etc. | ☐ | Ready for future RBAC |
| 6.1.12 | **Note missing fields** | Username, PasswordHash, Role columns don't exist yet | ☐ | Schema change needed |

**Deliverable**: Create separate document "RBAC_Implementation_Testing_Plan.md" for when feature is implemented.

---

## 6. Test Coverage Matrix

### Coverage by Epic

| Epic | Total TCs | Testable | Not Testable | Coverage % |
|------|-----------|----------|--------------|------------|
| Epic 1: Roster | 9 | 9 | 0 | 100% |
| Epic 2: Projects | 8 | 8 | 0 | 100% |
| Epic 3: Financials | 22 | 22 | 0 | 100% |
| Epic 4: Global Config | 4 | 4 | 0 | 100% |
| Epic 5: Auth/RBAC | 8 | 2 | 6 | 25% ⚠️ |
| Epic 6: Notifications | 8 | 8 | 0 | 100% |
| Epic 7: Navigation | 12 | 12 | 0 | 100% |
| Epic 8: Snapshots | 15 | 15 | 0 | 100% |
| **TOTAL** | **86** | **80** | **6** | **93%** |

### Coverage by Priority

| Priority | Test Cases | % of Total | Risk |
|----------|-----------|-----------|------|
| P0 (Critical) | 51 | 59% | High impact on core business |
| P1 (High) | 23 | 27% | Important UX features |
| P2 (Medium) | 6 | 7% | Enhancements |
| P3 (Low/Future) | 6 | 7% | RBAC - not yet implemented |

### Traceability Matrix Sample

| Requirement ID | User Story | Test Case ID | Status |
|----------------|-----------|--------------|--------|
| FR-3.4.2 | Snapshot Workflow | TC-8.1.1 - 8.1.3 | ✅ Testable |
| FR-3.4.3 | Override Values | TC-8.4.1 - 8.4.4 | ✅ Testable |
| FR-3.4.4 | Confirm Month | TC-3.7.1 - 3.7.5 | ✅ Testable |
| FR-3.5.2 | RBAC Edit Permissions | TC-5.4.2 - 5.4.3 | ⚠️ Future |

---

## 7. Defect Management

### Severity Classification

| Severity | Criteria | Example | Action |
|----------|---------|---------|--------|
| **S1 - Critical** | System unusable, data loss | Snapshot save fails, data deleted | STOP testing, fix immediately |
| **S2 - High** | Major feature broken | Cannot confirm month, WIP not calculating | Continue testing, fix before release |
| **S3 - Medium** | Feature works with issues | UI glitch, slow loading | Log, fix in sprint |
| **S4 - Low** | Cosmetic, minor UX issue | Typo, color mismatch | Log, fix when convenient |

### Defect Logging Template

```markdown
## Defect #001

**Severity**: S2 - High
**Epic**: Epic 8 - Snapshot Workflow
**Test Case**: TC 8.4.4
**Environment**: Windows 11, Chrome 131, localhost:5173

**Steps to Reproduce**:
1. Navigate to TEST_Snapshot Workflow Project
2. Enable edit mode
3. Modify WIP value
4. Click Save

**Expected Result**: Diff dialog appears showing old vs new value

**Actual Result**: Dialog shows undefined for old value

**Evidence**: [Screenshot attached]

**Workaround**: Refresh page before editing

**Assigned To**: [Developer Name]
**Status**: Open
**Fix Version**: 3.1
```

### Defect Triage Process

```
New Defect → Triage Meeting → Assign Severity → Prioritize → Assign Developer → Fix → Retest → Close
              (Daily)          (S1/S2: Immediate                (Verify in    (If pass)
                                S3/S4: Next sprint)              same env)
```

---

## 8. Success Criteria

### Exit Criteria by Phase

| Phase | Exit Criteria | Min Pass Rate |
|-------|--------------|--------------|
| Phase 1 (Smoke) | All 5 smoke tests pass | 100% |
| Phase 2 (Financial) | Core calculations accurate, snapshot workflow operational | 95% |
| Phase 3 (Data Mgmt) | CRUD operations work, no data corruption | 100% |
| Phase 4 (UX) | Navigation smooth, notifications work | 90% |
| Phase 5 (Regression) | No new defects introduced | 85% |
| Phase 6 (RBAC) | Documentation complete for future testing | 100% |

### Overall Success Criteria

✅ **PASS**: 
- 80+ test cases executed (93% coverage)
- 90%+ pass rate on P0 tests
- No S1 defects open
- Max 2 S2 defects open
- RBAC status documented

⚠️ **CONDITIONAL PASS**:
- 75-89% pass rate on P0 tests
- 1 S2 defect open with workaround
- Minor regressions in P2 tests

❌ **FAIL**:
- &lt;75% pass rate on P0 tests
- Any S1 defect open
- 3+ S2 defects open
- Data integrity issues
- Core financial calculations incorrect

### Quality Metrics

Track these throughout testing:

```
Defect Density = Total Defects Found / Total Test Cases Executed
Target: < 0.15 (expect <12 defects across 80 tests)

Pass Rate = (Total Passed / Total Executed) × 100
Target: ≥ 90%

Coverage = (Testable TCs / Total TCs) × 100
Current: 93% (80/86)

Severity Distribution Target:
- S1: 0%
- S2: <10%
- S3: 30-40%
- S4: 50-60%
```

---

## 9. Test Execution Timeline

### Recommended Schedule

**Day 1: Setup & Core Features (4 hours)**
- 09:00-09:30: Environment setup (Section 2)
- 09:30-10:00: Phase 1 - Smoke Testing
- 10:00-12:00: Phase 2 - Core Financial Workflow
- 12:00-13:00: Lunch Break
- 13:00-14:00: Phase 3 - Data Management

**Day 2: UX & Regression (4 hours)**
- 09:00-10:00: Phase 4 - User Experience Features
- 10:00-11:30: Phase 5 - Regression & Exploratory
- 11:30-12:00: Phase 6 - RBAC Documentation
- 12:00-13:00: Defect review & prioritization

**Day 3: Retest & Reporting (2 hours)**
- 09:00-10:00: Retest failed cases
- 10:00-10:30: Final metrics calculation
- 10:30-11:00: Test summary report
- 11:00-11:30: Stakeholder presentation

**Total Effort**: 10 hours (1.25 person-days)

---

## 10. Test Deliverables

### Required Artifacts

1. **Test Execution Report** (Excel/CSV)
   - Columns: TC ID, Description, Status, Tester, Date, Comments, Defect ID
   - One row per test case
   - Export from test management tool or manual tracking

2. **Defect Log** (Excel/CSV or Jira)
   - List of all defects found
   - Status tracking (Open, In Progress, Resolved, Closed)

3. **Test Summary Report** (Document)
   - Executive summary
   - Pass/Fail metrics
   - Quality assessment
   - Recommendations
   - Screenshots of key validations

4. **RBAC Future Testing Plan** (Markdown)
   - Test cases 5.3-5.6 detailed steps
   - Test data mapping (which user tests what)
   - Prerequisites for RBAC implementation

5. **Screen Recordings** (Optional but recommended)
   - Record snapshot workflow test (Suite 2.2-2.6)
   - Record edit mode toggle demo
   - Record override with diff preview
   - Store in artifacts folder

---

## 11. Tools & Resources

### Test Data Files
- **Primary**: `tools/TestData_Seed_v3.sql` (use this)
- **Legacy**: `tools/TestData_Seed.sql` (reference only)
- **Production Sample**: `ResourceManagement.Infrastructure/Persistence/Scripts/SeedData.sql`

### Verification Queries

**Check Snapshot Data**:
```sql
SELECT 
    p.Name,
    pms.Month,
    CASE pms.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Editable' WHEN 2 THEN 'Confirmed' END AS Status,
    pms.Wip,
    pms.IsOverridden
FROM ProjectMonthlySnapshot pms
JOIN Project p ON pms.ProjectId = p.Id
WHERE p.Wbs = 'TEST.003'
ORDER BY pms.Month;
```

**Check WIP Calculation**:
```sql
SELECT 
    r.FullNameEn,
    r.Level,
    ra.AllocatedDays,
    gr.NominalRate,
    p.Discount,
    (ra.AllocatedDays * gr.NominalRate * (1 - p.Discount/100)) AS CalculatedWIP
FROM ResourceAllocation ra
JOIN Roster r ON ra.RosterId = r.Id
JOIN ForecastVersion fv ON ra.ForecastVersionId = fv.Id
JOIN Project p ON fv.ProjectId = p.Id
LEFT JOIN GlobalRate gr ON r.Level = gr.Level
WHERE p.Wbs = 'TEST.002' AND ra.Month = '2026-01-01';
```

### Browser DevTools Shortcuts
- **Console**: F12 → Console tab (check for errors)
- **Network**: F12 → Network tab (monitor API calls)
- **Application**: F12 → Application → Local Storage (check theme, session)
- **Lighthouse**: F12 → Lighthouse → Run audit (performance check)

---

## 12. Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Test data script fails to execute | Low | High | Manually create minimal dataset if needed |
| S1 defect found during Phase 2 | Medium | Critical | STOP testing, emergency fix, retest from Phase 1 |
| API not responding | Low | High | Check backend process, restart if needed |
| Snapshot auto-generation doesn't trigger | Medium | Medium | Manually trigger via API or Postman |
| Browser compatibility issues | Low | Low | Test on both Chrome and Edge |
| RBAC assumption incorrect | Low | Medium | Clarify with dev team, update docs |

---

## Appendix A: Test Case Quick Reference

### P0 Critical Test Cases (Must Pass)

| ID | Test Case | Epic | Duration |
|----|-----------|------|----------|
| TC-3.2.1 | WIP Calculation Accuracy | 3 | 15 min |
| TC-8.1.x | View Snapshot Matrix | 8 | 20 min |
| TC-8.2.x | Edit Mode Toggle | 8 | 15 min |
| TC-8.4.x | Manual Override with Audit | 8 | 25 min |
| TC-3.6.x | Clear Override | 8 | 15 min |
| TC-3.7.x | Confirm/Lock Month | 8 | 20 min |
| TC-1.1.1 | View Roster | 1 | 5 min |
| TC-2.1.1 | Nominal Budget Calculation | 2 | 5 min |
| TC-3.1.x | Financial Inputs | 3 | 15 min |

**Total P0 Duration**: ~2.5 hours

---

## Appendix B: Test Data Reset Procedure

If you need to reset test data mid-testing:

```sql
-- 1. Backup current state (optional)
SELECT * INTO ProjectMonthlySnapshot_Backup FROM ProjectMonthlySnapshot WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');

-- 2. Delete all test data
DELETE FROM ProjectMonthlySnapshot WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
DELETE FROM ResourceAllocation WHERE ForecastVersionId IN (SELECT Id FROM ForecastVersion WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%'));
DELETE FROM ForecastVersion WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
DELETE FROM Billing WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
DELETE FROM Expense WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
DELETE FROM ProjectRate WHERE ProjectId IN (SELECT Id FROM Project WHERE Wbs LIKE 'TEST.%');
DELETE FROM Project WHERE Wbs LIKE 'TEST.%';
DELETE FROM Roster WHERE SapCode LIKE 'TEST_%';
DELETE FROM GlobalRate;

-- 3. Re-run TestData_Seed_v3.sql
-- Execute entire script from SSMS/Azure Data Studio

-- 4. Restart backend API to clear any caches
-- Ctrl+C in API terminal, then: dotnet run --project ResourceManagement.Api

-- 5. Hard refresh browser
-- Ctrl+Shift+R (Chrome/Edge)
```

---

## Appendix C: Sign-Off Template

```
TESTING SIGN-OFF APPROVAL

Project: Resource Management Tool v3.0
Test Phase: [Phase Name]
Test Date: [Date Range]
Tester: [Your Name]

SUMMARY:
- Total Test Cases Executed: ___
- Passed: ___
- Failed: ___
- Blocked: ___
- Pass Rate: ___%

DEFECTS:
- S1 (Critical): ___
- S2 (High): ___
- S3 (Medium): ___
- S4 (Low): ___

RECOMMENDATION:
[ ] APPROVED FOR RELEASE
[ ] APPROVED WITH CONDITIONS (list below)
[ ] NOT APPROVED (critical issues)

Conditions/Notes:
__________________________________________________

Signatures:
QA Lead: ___________________ Date: ___________
Dev Lead: __________________ Date: ___________
Product Owner: _____________ Date: ___________
```

---

**Document Version**: 3.0  
**Last Updated**: 2026-02-03  
**Next Review**: After Phase 2 completion or on finding of S1 defect  
**Status**: ✅ READY FOR EXECUTION

**End of Testing Strategy & Execution Plan**
