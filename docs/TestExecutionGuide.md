# Test Execution Guide
**Version:** 1.0
**Date:** 2026-01-26

This guide provides step-by-step instructions for executing all test cases with the prepared test data.

---

## Prerequisites

1. **Run Test Data Script**: Execute `tools/TestData_Seed.sql` against your database
2. **Start Backend**: `dotnet run --project ResourceManagement.Api`
3. **Start Frontend**: `cd frontend && npm run dev`
4. **Open Browser**: Navigate to `http://localhost:5173`

---

## Test User Credentials

| Username | Password | Role | Purpose |
|----------|----------|------|---------|
| `admin` | `admin123` | Admin | Full access testing |
| `partner` | `admin123` | Partner | Partner role testing |
| `manager` | `admin123` | Manager | Manager RBAC testing |
| `employee` | `admin123` | Employee | Employee RBAC testing |
| `john` | `admin123` | Employee | Search testing |
| `maria` | `admin123` | Employee | Additional user |

---

## Epic 1: Resource Roster Management

### Test 1.1.1: View Roster Columns
**Steps:**
1. Login as `admin`
2. Click "Resource Roster" in sidebar
3. **Verify**: Table displays columns: Name, SAP Ref, Function, Seniority, Monthly Cost
**Expected**: All test users visible with calculated monthly costs

### Test 1.1.2: Edit Resource
**Steps:**
1. On Roster page, click Edit (pencil icon) on any row
2. **Verify**: Modal opens with editable fields: Monthly Salary, Cars, Metlife, etc.
3. Change Monthly Salary, click Save
**Expected**: Success toast, value updates in table

### Test 1.2.1: Excel Import
**Steps:**
1. As `admin`, go to Roster page
2. Click Import button
3. Select `tools/RosterTemplate.xlsx` (must contain valid data)
**Expected**: Success toast, new records appear in table

### Test 1.2.2: Excel Export
**Steps:**
1. Click Export button on Roster page
**Expected**: File `Roster_[Date].xlsx` downloads

### Test 1.3.1: Search by Name
**Steps:**
1. In Roster search box, type "John"
**Expected**: Only "John Smith" appears in results
**Test Data**: User `TEST_JOHN_001` named "John Smith"

### Test 1.3.2: Filter by Level
**Steps:**
1. Click Level filter dropdown
2. Select "SC"
**Expected**: Only SC level resources shown (John Smith, Maria Garcia, Andreas Papadopoulos, Employee User)
**Test Data**: 4 users with Level = "SC"

---

## Epic 2: Project Management

### Test 2.1.1: Nominal Budget Calculation
**Steps:**
1. As `manager` or higher, click "Initiate Project"
2. Enter: Name = "Test Project", WBS = "NEW.001", Actual Budget = 100000, Discount = 20%
3. **Verify**: Nominal Budget field shows ~125,000
**Formula**: 100,000 / (1 - 0.20) = 125,000
**Note**: Existing `TEST.001` project already has this data for verification

### Test 2.1.2: Project Card Display
**Steps:**
1. After creating project, go to Projects page
**Expected**: New project card visible with budget metrics

### Test 2.2.1: Project Sub-Navigation
**Steps:**
1. Click on any project card (e.g., "TEST_Manager Assigned Project")
**Expected**: Sidebar expands showing: Overview, Billings, Expenses, Forecast links

### Test 2.2.2: Navigate to Forecast
**Steps:**
1. Click "Forecast" in sub-navigation
**Expected**: Navigates to Resource Allocation Matrix page

---

## Epic 3: Financials & Forecasting

### Test 3.1.1: Enter Billing
**Steps:**
1. Navigate to Project > Billings
2. Scroll to Jan 2026
3. Enter 5000 in the input field
4. Click Save
**Expected**: Success toast, value persists after page refresh

### Test 3.2.1: WIP Calculation
**Prerequisites**: 
- Global Rate for SC = 500 (already set)
- Project TEST.002 has 10% discount
- John (SC level) allocated 10 days in Jan 2026

**Steps:**
1. Go to Project "TEST_WIP Calculation Project"
2. Navigate to Financials/Overview
**Expected WIP**: 10 days × €500 × (1 - 0.10) = €4,500

### Test 3.3.1: Clone Scenario
**Steps:**
1. Go to any project's Forecast page
2. Click "Clone Scenario"
**Expected**: New version appears in dropdown, success toast

### Test 3.3.2: Switch Versions
**Steps:**
1. After cloning, use dropdown to switch versions
**Expected**: Allocations update to show data for selected version

---

## Epic 4: Global System Configuration

### Test 4.1.1: Add Global Rate
**Steps:**
1. As `admin`, go to Settings > Nominal Rates
2. Click "Add Rate"
3. Enter Level = "BA" (Business Analyst), Rate = 350
4. Save
**Expected**: New rate appears in table

### Test 4.1.2: Update Global Rate
**Steps:**
1. Click Edit on "M" (Manager) rate
2. Change from 800 to 850
3. Save
**Expected**: Rate updates, timestamp refreshes

---

## Epic 5: Authentication & Access Control

### Test 5.1.1: Valid Login
**Steps:**
1. Go to login page
2. Enter username: `admin`, password: `admin123`
3. Click Login
**Expected**: Redirect to Dashboard

### Test 5.1.2: Invalid Login
**Steps:**
1. Enter username: `admin`, password: `wrongpassword`
2. Click Login
**Expected**: Error message "Invalid credentials"

### Test 5.1.3: Session Persistence
**Steps:**
1. Login successfully
2. Refresh the page (F5)
**Expected**: Still logged in, not redirected to login

### Test 5.2.1: Logout
**Steps:**
1. While logged in, click logout icon in sidebar footer
**Expected**: Redirect to Login page

### Test 5.2.2: Protected Route
**Steps:**
1. After logout, manually navigate to `http://localhost:5173/projects`
**Expected**: Redirect to Login page

### Test 5.3.1: Employee Project Visibility
**Steps:**
1. Login as `employee` (password: admin123)
2. Go to Projects page
**Expected**: Only "TEST_Employee Assigned Project" visible
**Test Data**: Employee is allocated to Project TEST.005 only

### Test 5.3.2: Employee Unassigned Access
**Steps:**
1. As `employee`, manually navigate to `/projects/[ID of TEST.004]`
**Expected**: 403 Forbidden or redirect

### Test 5.4.1: Manager Sees All Projects
**Steps:**
1. Login as `manager`
2. Go to Projects page
**Expected**: All 5 test projects visible

### Test 5.4.2: Manager Can Edit Assigned Project
**Steps:**
1. As `manager`, go to "TEST_Manager Assigned Project" Forecast
**Expected**: "Commit Plan", "Clone Scenario", "Assign Resource" buttons visible
**Test Data**: Manager is allocated to Project TEST.003

### Test 5.4.3: Manager Cannot Edit Unassigned Project
**Steps:**
1. As `manager`, go to "TEST_Unassigned Project" Forecast
**Expected**: Edit buttons hidden/disabled, inputs disabled

### Test 5.5.1: Admin Full Access
**Steps:**
1. Login as `admin`
2. Go to any project's Forecast
**Expected**: All edit controls available

### Test 5.5.2: Partner Full Access
**Steps:**
1. Login as `partner`
2. Edit any project's forecast allocations
3. Click "Commit Plan"
**Expected**: Save succeeds

### Test 5.6.1: Role Dropdown Visible for Admin
**Steps:**
1. As `admin`, go to Roster
2. Click Edit on any user
**Expected**: "Role" dropdown visible with options: Employee, Manager, Partner, Admin

### Test 5.6.2: Change User Role
**Steps:**
1. As `admin`, edit "Employee User"
2. Change Role from Employee to Manager
3. Save
**Expected**: Success toast, role updated

### Test 5.6.3: Employee Cannot See Role Dropdown
**Steps:**
1. Login as `employee`
2. Go to Roster, click Edit on any user (if allowed)
**Expected**: Role dropdown NOT visible or entire edit button hidden

---

## Epic 6: Notifications & User Feedback

### Test 6.1.1: Success Toast
**Steps:**
1. Perform any save action (e.g., save a roster member)
**Expected**: Green success toast appears at top-center

### Test 6.1.2: Error Toast
**Steps:**
1. Try to create roster member with duplicate SAP code
**Expected**: Red error toast appears

### Test 6.1.3: Toast Auto-Dismiss
**Steps:**
1. Trigger a toast notification
2. Wait 3-4 seconds
**Expected**: Toast fades out automatically

### Test 6.2.1: Notification History
**Steps:**
1. Trigger multiple actions (save, error, etc.)
2. Click bell icon in header
**Expected**: Panel opens showing notification history

### Test 6.2.2: Unread Badge
**Steps:**
1. Trigger notifications
2. Look at bell icon
**Expected**: Green dot badge indicates unread notifications

### Test 6.2.3: Mark All Read
**Steps:**
1. Open notification panel
2. Click checkmark button (Mark all read)
**Expected**: Badge disappears, all items marked as read

### Test 6.2.4: Clear All
**Steps:**
1. Open notification panel
2. Click trash button (Clear all)
**Expected**: Notification list becomes empty

---

## Epic 7: Global Navigation & Search

### Test 7.1.1: Open Command Palette (Keyboard)
**Steps:**
1. Press `Ctrl+K`
**Expected**: Command palette modal opens

### Test 7.1.2: Search in Palette
**Steps:**
1. In palette, type "roster"
**Expected**: "Go to Roster" appears in filtered results

### Test 7.1.3: Execute Command
**Steps:**
1. Press Enter on "Go to Roster"
**Expected**: Navigates to Roster page, palette closes

### Test 7.1.4: Open Command Palette (Click)
**Steps:**
1. Click search icon in header (magnifying glass)
**Expected**: Command palette opens

### Test 7.2.1: Breadcrumb Display
**Steps:**
1. Navigate to Project > Forecast
**Expected**: Breadcrumb shows: Resource Platform > Projects > [Project Name] > Forecast

### Test 7.2.2: Breadcrumb Navigation
**Steps:**
1. Click "Projects" in breadcrumb
**Expected**: Navigates back to Projects list

### Test 7.3.1: Theme Toggle
**Steps:**
1. Click sun/moon icon in header
**Expected**: Theme changes between dark and light mode

### Test 7.3.2: Theme Persistence
**Steps:**
1. Change theme to Light
2. Refresh page
**Expected**: Light theme is still active

---

## Test Execution Checklist

| Test ID | Description | Status |
|---------|-------------|--------|
| 1.1.1 | View Roster Columns | ☐ |
| 1.1.2 | Edit Resource | ☐ |
| 1.2.1 | Excel Import | ☐ |
| 1.2.2 | Excel Export | ☐ |
| 1.3.1 | Search by Name | ☐ |
| 1.3.2 | Filter by Level | ☐ |
| 2.1.1 | Nominal Budget Calculation | ☐ |
| 2.1.2 | Project Card Display | ☐ |
| 2.2.1 | Project Sub-Navigation | ☐ |
| 2.2.2 | Navigate to Forecast | ☐ |
| 3.1.1 | Enter Billing | ☐ |
| 3.2.1 | WIP Calculation | ☐ |
| 3.3.1 | Clone Scenario | ☐ |
| 3.3.2 | Switch Versions | ☐ |
| 4.1.1 | Add Global Rate | ☐ |
| 4.1.2 | Update Global Rate | ☐ |
| 5.1.1 | Valid Login | ☐ |
| 5.1.2 | Invalid Login | ☐ |
| 5.1.3 | Session Persistence | ☐ |
| 5.2.1 | Logout | ☐ |
| 5.2.2 | Protected Route | ☐ |
| 5.3.1 | Employee Project Visibility | ☐ |
| 5.3.2 | Employee Unassigned Access | ☐ |
| 5.4.1 | Manager Sees All Projects | ☐ |
| 5.4.2 | Manager Edit Assigned | ☐ |
| 5.4.3 | Manager Cannot Edit Unassigned | ☐ |
| 5.5.1 | Admin Full Access | ☐ |
| 5.5.2 | Partner Full Access | ☐ |
| 5.6.1 | Role Dropdown for Admin | ☐ |
| 5.6.2 | Change User Role | ☐ |
| 5.6.3 | Employee No Role Dropdown | ☐ |
| 6.1.1 | Success Toast | ☐ |
| 6.1.2 | Error Toast | ☐ |
| 6.1.3 | Toast Auto-Dismiss | ☐ |
| 6.2.1 | Notification History | ☐ |
| 6.2.2 | Unread Badge | ☐ |
| 6.2.3 | Mark All Read | ☐ |
| 6.2.4 | Clear All | ☐ |
| 7.1.1 | Command Palette Keyboard | ☐ |
| 7.1.2 | Search in Palette | ☐ |
| 7.1.3 | Execute Command | ☐ |
| 7.1.4 | Command Palette Click | ☐ |
| 7.2.1 | Breadcrumb Display | ☐ |
| 7.2.2 | Breadcrumb Navigation | ☐ |
| 7.3.1 | Theme Toggle | ☐ |
| 7.3.2 | Theme Persistence | ☐ |

---

**Total Test Cases: 44**

---
**End of Document**
