using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace TestCaseGenerator
{
    class Program
    {
        static void Main()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            
            // Sheet 1: Test Cases
            var ws1 = package.Workbook.Worksheets.Add("Test Cases");
            CreateTestCasesSheet(ws1);
            
            // Sheet 2: Test Data
            var ws2 = package.Workbook.Worksheets.Add("Test Data - Users");
            CreateTestUsersSheet(ws2);
            
            // Sheet 3: Test Data - Projects
            var ws3 = package.Workbook.Worksheets.Add("Test Data - Projects");
            CreateTestProjectsSheet(ws3);
            
            // Sheet 4: Global Rates
            var ws4 = package.Workbook.Worksheets.Add("Test Data - Rates");
            CreateGlobalRatesSheet(ws4);
            
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestCases_ResourceManagement.xlsx");
            package.SaveAs(new FileInfo(filePath));
            
            Console.WriteLine($"Excel file created: {filePath}");
        }
        
        static void CreateTestCasesSheet(ExcelWorksheet ws)
        {
            // Headers
            string[] headers = { "Test ID", "Epic", "User Story", "Test Case Name", "Priority", "Preconditions", "Test Steps", "Test Data", "Expected Result", "Acceptance Criteria", "Status", "Notes" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
            }
            
            // Style headers
            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                range.Style.Font.Color.SetColor(Color.White);
            }
            
            var testCases = GetTestCases();
            int row = 2;
            foreach (var tc in testCases)
            {
                ws.Cells[row, 1].Value = tc.Id;
                ws.Cells[row, 2].Value = tc.Epic;
                ws.Cells[row, 3].Value = tc.UserStory;
                ws.Cells[row, 4].Value = tc.Name;
                ws.Cells[row, 5].Value = tc.Priority;
                ws.Cells[row, 6].Value = tc.Preconditions;
                ws.Cells[row, 7].Value = tc.Steps;
                ws.Cells[row, 8].Value = tc.TestData;
                ws.Cells[row, 9].Value = tc.ExpectedResult;
                ws.Cells[row, 10].Value = tc.AcceptanceCriteria;
                ws.Cells[row, 11].Value = "";
                ws.Cells[row, 12].Value = "";
                
                // Alternate row colors
                if (row % 2 == 0)
                {
                    using var range = ws.Cells[row, 1, row, headers.Length];
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                }
                row++;
            }
            
            // Auto-fit columns
            ws.Cells.AutoFitColumns();
            // Set max width for text columns
            ws.Column(6).Width = 40;
            ws.Column(7).Width = 60;
            ws.Column(8).Width = 40;
            ws.Column(9).Width = 40;
            ws.Column(10).Width = 40;
            
            // Enable text wrap
            ws.Cells[2, 6, row, 10].Style.WrapText = true;
            
            // Freeze header row
            ws.View.FreezePanes(2, 1);
        }
        
        static void CreateTestUsersSheet(ExcelWorksheet ws)
        {
            string[] headers = { "Username", "Password", "Role", "Full Name", "Level", "SAP Code", "Can View Projects", "Can Edit Projects", "Purpose" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
            }
            
            StyleHeader(ws, headers.Length);
            
            var users = new[]
            {
                ("admin", "admin123", "Admin", "Admin User", "D", "TEST_ADMIN_001", "All", "All", "Full access testing, role assignment"),
                ("partner", "admin123", "Partner", "Partner User", "P", "TEST_PARTNER_001", "All", "All", "Partner role testing"),
                ("manager", "admin123", "Manager", "Manager User", "M", "TEST_MANAGER_001", "All", "Only TEST.003", "Manager RBAC - can edit assigned projects only"),
                ("employee", "admin123", "Employee", "Employee User", "SC", "TEST_EMP_001", "Only TEST.005", "None", "Employee RBAC - view assigned only, no edit"),
                ("john", "admin123", "Employee", "John Smith", "SC", "TEST_JOHN_001", "Only TEST.002", "None", "Search testing (name contains 'John')"),
                ("maria", "admin123", "Employee", "Maria Garcia", "SC", "TEST_SC_001", "Only TEST.005", "None", "Additional SC user for filter testing")
            };
            
            int row = 2;
            foreach (var u in users)
            {
                ws.Cells[row, 1].Value = u.Item1;
                ws.Cells[row, 2].Value = u.Item2;
                ws.Cells[row, 3].Value = u.Item3;
                ws.Cells[row, 4].Value = u.Item4;
                ws.Cells[row, 5].Value = u.Item5;
                ws.Cells[row, 6].Value = u.Item6;
                ws.Cells[row, 7].Value = u.Item7;
                ws.Cells[row, 8].Value = u.Item8;
                ws.Cells[row, 9].Value = u.Item9;
                row++;
            }
            
            ws.Cells.AutoFitColumns();
            ws.View.FreezePanes(2, 1);
        }
        
        static void CreateTestProjectsSheet(ExcelWorksheet ws)
        {
            string[] headers = { "WBS", "Project Name", "Actual Budget", "Discount %", "Nominal Budget", "Assigned Users", "Purpose" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
            }
            
            StyleHeader(ws, headers.Length);
            
            var projects = new[]
            {
                ("TEST.001", "Budget Calculation Project", "€100,000", "20%", "€125,000", "None", "Test nominal budget calculation: 100k / (1-0.20) = 125k"),
                ("TEST.002", "WIP Calculation Project", "€50,000", "10%", "€55,556", "John (10 days)", "WIP test: 10 days × €500 (SC rate) × 0.9 = €4,500"),
                ("TEST.003", "Manager Assigned Project", "€200,000", "10%", "€222,222", "Manager (15 days)", "Manager CAN edit this project"),
                ("TEST.004", "Unassigned Project", "€75,000", "5%", "€78,947", "None", "Manager CANNOT edit (not assigned)"),
                ("TEST.005", "Employee Assigned Project", "€120,000", "5%", "€126,316", "Employee, Maria", "Employee visibility test (can view, not edit)")
            };
            
            int row = 2;
            foreach (var p in projects)
            {
                ws.Cells[row, 1].Value = p.Item1;
                ws.Cells[row, 2].Value = p.Item2;
                ws.Cells[row, 3].Value = p.Item3;
                ws.Cells[row, 4].Value = p.Item4;
                ws.Cells[row, 5].Value = p.Item5;
                ws.Cells[row, 6].Value = p.Item6;
                ws.Cells[row, 7].Value = p.Item7;
                row++;
            }
            
            ws.Cells.AutoFitColumns();
            ws.Column(7).Width = 50;
            ws.View.FreezePanes(2, 1);
        }
        
        static void CreateGlobalRatesSheet(ExcelWorksheet ws)
        {
            string[] headers = { "Level", "Description", "Nominal Rate (€/day)", "Used In Tests" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
            }
            
            StyleHeader(ws, headers.Length);
            
            var rates = new[]
            {
                ("A", "Associate", "€300", ""),
                ("C", "Consultant", "€400", ""),
                ("SC", "Senior Consultant", "€500", "WIP Calculation Test (3.2.1)"),
                ("M", "Manager", "€800", "Global Rates Tests (4.1.1, 4.1.2)"),
                ("SM", "Senior Manager", "€900", ""),
                ("D", "Director", "€1,200", ""),
                ("P", "Partner", "€1,500", "")
            };
            
            int row = 2;
            foreach (var r in rates)
            {
                ws.Cells[row, 1].Value = r.Item1;
                ws.Cells[row, 2].Value = r.Item2;
                ws.Cells[row, 3].Value = r.Item3;
                ws.Cells[row, 4].Value = r.Item4;
                row++;
            }
            
            ws.Cells.AutoFitColumns();
            ws.View.FreezePanes(2, 1);
        }
        
        static void StyleHeader(ExcelWorksheet ws, int colCount)
        {
            using var range = ws.Cells[1, 1, 1, colCount];
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
            range.Style.Font.Color.SetColor(Color.White);
        }
        
        static List<TestCase> GetTestCases()
        {
            return new List<TestCase>
            {
                // Epic 1: Resource Roster Management
                new("1.1.1", "Resource Roster", "View Roster", "View Roster Columns", "High",
                    "User is logged in as admin",
                    "1. Navigate to Resource Roster from sidebar\n2. Observe the table columns",
                    "Login: admin / admin123",
                    "Table displays with columns: Name, SAP Ref, Function, Seniority, Monthly Cost",
                    "All required columns are visible and contain data"),
                    
                new("1.1.2", "Resource Roster", "View Roster", "Edit Resource Modal", "High",
                    "User is logged in as admin\nRoster page is open",
                    "1. Click Edit (pencil icon) on any row\n2. Observe the modal fields\n3. Modify Monthly Salary value\n4. Click Save",
                    "Any roster member",
                    "Modal opens with editable fields: Monthly Salary, Cars, Metlife, etc.\nSuccess toast on save",
                    "Fields are editable and changes persist"),
                    
                new("1.2.1", "Resource Roster", "Excel Import/Export", "Excel Import", "Medium",
                    "User is logged in as admin\nValid .xlsx file prepared",
                    "1. Click Import button\n2. Select valid .xlsx file\n3. Confirm import",
                    "Excel file with roster data",
                    "Success toast appears\nNew records visible in table",
                    "Data imports without errors"),
                    
                new("1.2.2", "Resource Roster", "Excel Import/Export", "Excel Export", "Medium",
                    "User is logged in as admin\nRoster page is open",
                    "1. Click Export button\n2. Wait for download",
                    "N/A",
                    "File Roster_[Date].xlsx downloads",
                    "File contains accurate roster data"),
                    
                new("1.3.1", "Resource Roster", "Search & Filter", "Search by Name", "High",
                    "User is logged in\nRoster page is open",
                    "1. Type 'John' in search box\n2. Observe results",
                    "User 'John Smith' (TEST_JOHN_001) exists",
                    "Only 'John Smith' appears in filtered results",
                    "Search is case-insensitive and filters correctly"),
                    
                new("1.3.2", "Resource Roster", "Search & Filter", "Filter by Level", "High",
                    "User is logged in\nRoster page is open",
                    "1. Click Level filter dropdown\n2. Select 'SC'\n3. Observe results",
                    "Multiple SC-level users exist",
                    "Only SC level resources shown (John Smith, Maria Garcia, Andreas, Employee User)",
                    "Filter correctly shows only selected level"),
                
                // Epic 2: Project Management
                new("2.1.1", "Project Management", "Project Creation", "Nominal Budget Calculation", "Critical",
                    "User is logged in as manager or higher",
                    "1. Click 'Initiate Project' button\n2. Enter Name: 'Test Project'\n3. Enter WBS: 'NEW.001'\n4. Enter Actual Budget: 100000\n5. Enter Discount: 20\n6. Observe Nominal Budget field",
                    "N/A",
                    "Nominal Budget shows approximately €125,000\nFormula: 100,000 / (1 - 0.20) = 125,000",
                    "Nominal budget is calculated correctly in real-time"),
                    
                new("2.1.2", "Project Management", "Project Creation", "Project Card Display", "High",
                    "Project created in 2.1.1",
                    "1. Navigate to Projects page\n2. Find the newly created project card",
                    "Newly created project",
                    "Project card visible with correct budget metrics",
                    "Card displays accurate project information"),
                    
                new("2.2.1", "Project Management", "Project Navigation", "Project Sub-Navigation", "High",
                    "User is logged in\nProjects exist",
                    "1. Click on any project card (e.g., TEST_Manager Assigned Project)\n2. Observe sidebar",
                    "Project TEST.003",
                    "Sidebar expands showing: Overview, Billings, Expenses, Forecast links",
                    "Sub-navigation appears for project drill-down"),
                    
                new("2.2.2", "Project Management", "Project Navigation", "Navigate to Forecast", "High",
                    "Inside a project detail page",
                    "1. Click 'Forecast' in sub-navigation",
                    "Any project with forecast data",
                    "Navigates to Resource Allocation Matrix page",
                    "Forecast page loads correctly"),
                
                // Epic 3: Financials & Forecasting
                new("3.1.1", "Financials", "Financial Inputs", "Enter Billing", "High",
                    "User is logged in as manager+\nProject selected",
                    "1. Navigate to Project > Billings\n2. Scroll to Jan 2026 column\n3. Enter 5000 in input field\n4. Click Save",
                    "Project TEST.003",
                    "Success toast appears\nValue 5000 persists after refresh",
                    "Billing data saves correctly"),
                    
                new("3.2.1", "Financials", "WIP Calculation", "Automated WIP Calculation", "Critical",
                    "Global Rate for SC = €500\nProject TEST.002 has 10% discount\nJohn (SC) allocated 10 days in Jan",
                    "1. Navigate to Project TEST.002\n2. Go to Financials/Overview\n3. Check WIP for Jan 2026",
                    "John allocated 10 days @ SC rate (€500), Project discount 10%",
                    "WIP = 10 × 500 × 0.9 = €4,500",
                    "WIP calculation matches formula exactly"),
                    
                new("3.3.1", "Financials", "Forecast Versioning", "Clone Scenario", "Medium",
                    "User is logged in as manager+\nProject with forecast exists",
                    "1. Go to any project's Forecast page\n2. Click 'Clone Scenario' button",
                    "Any project",
                    "New version appears in dropdown\nSuccess toast shown",
                    "New scenario created with copied allocations"),
                    
                new("3.3.2", "Financials", "Forecast Versioning", "Switch Versions", "Medium",
                    "Multiple forecast versions exist",
                    "1. Use version dropdown to switch\n2. Observe allocation data",
                    "Project with multiple versions",
                    "Allocations update to show selected version's data",
                    "Version switching works seamlessly"),
                
                // Epic 4: Global Configuration
                new("4.1.1", "System Config", "Global Rates", "Add Global Rate", "Medium",
                    "User is logged in as admin",
                    "1. Go to Settings > Nominal Rates\n2. Click 'Add Rate'\n3. Enter Level: 'BA', Rate: 350\n4. Save",
                    "New level 'BA' not in system",
                    "New rate appears in table",
                    "Rate is added successfully"),
                    
                new("4.1.2", "System Config", "Global Rates", "Update Global Rate", "Medium",
                    "User is logged in as admin",
                    "1. Click Edit on 'M' (Manager) rate\n2. Change 800 to 850\n3. Save",
                    "Existing M rate = €800",
                    "Rate updates to €850\nTimestamp refreshes",
                    "Rate update persists correctly"),
                
                // Epic 5: Authentication & Access Control
                new("5.1.1", "Authentication", "User Login", "Valid Login", "Critical",
                    "User is on login page",
                    "1. Enter username: admin\n2. Enter password: admin123\n3. Click Login",
                    "Username: admin, Password: admin123",
                    "Redirect to Dashboard",
                    "User is authenticated and redirected"),
                    
                new("5.1.2", "Authentication", "User Login", "Invalid Login", "Critical",
                    "User is on login page",
                    "1. Enter username: admin\n2. Enter password: wrongpassword\n3. Click Login",
                    "Username: admin, Password: wrongpassword",
                    "Error message 'Invalid credentials'",
                    "Invalid credentials are rejected with message"),
                    
                new("5.1.3", "Authentication", "User Login", "Session Persistence", "High",
                    "User logged in successfully",
                    "1. Refresh the page (F5)",
                    "Active login session",
                    "Still logged in, not redirected to login",
                    "Session persists across page refresh"),
                    
                new("5.2.1", "Authentication", "User Logout", "Logout", "High",
                    "User is logged in",
                    "1. Click logout icon in sidebar footer",
                    "Active session",
                    "Redirect to Login page",
                    "Session is terminated correctly"),
                    
                new("5.2.2", "Authentication", "User Logout", "Protected Route After Logout", "High",
                    "User has logged out",
                    "1. Manually navigate to /projects",
                    "No active session",
                    "Redirect to Login page",
                    "Protected routes require authentication"),
                    
                new("5.3.1", "Authorization", "Employee RBAC", "Employee Project Visibility", "Critical",
                    "User is on login page",
                    "1. Login as employee (admin123)\n2. Navigate to Projects",
                    "Username: employee, Password: admin123\nEmployee allocated to TEST.005 only",
                    "Only 'TEST_Employee Assigned Project' visible",
                    "Employees only see assigned projects"),
                    
                new("5.3.2", "Authorization", "Employee RBAC", "Employee Unassigned Access", "Critical",
                    "Logged in as employee",
                    "1. Manually navigate to /projects/[ID of TEST.004]",
                    "TEST.004 ID (employee not assigned)",
                    "403 Forbidden or redirect to projects",
                    "Unassigned projects are inaccessible"),
                    
                new("5.4.1", "Authorization", "Manager RBAC", "Manager Sees All Projects", "Critical",
                    "User is on login page",
                    "1. Login as manager (admin123)\n2. Navigate to Projects",
                    "Username: manager, Password: admin123",
                    "All 5 test projects visible",
                    "Managers can view all projects"),
                    
                new("5.4.2", "Authorization", "Manager RBAC", "Manager Can Edit Assigned", "Critical",
                    "Logged in as manager",
                    "1. Go to TEST_Manager Assigned Project\n2. Navigate to Forecast",
                    "Manager allocated to TEST.003",
                    "'Commit Plan', 'Clone Scenario', 'Assign Resource' buttons visible",
                    "Edit controls available for assigned project"),
                    
                new("5.4.3", "Authorization", "Manager RBAC", "Manager Cannot Edit Unassigned", "Critical",
                    "Logged in as manager",
                    "1. Go to TEST_Unassigned Project\n2. Navigate to Forecast",
                    "Manager NOT allocated to TEST.004",
                    "Edit buttons hidden/disabled, inputs disabled",
                    "Edit controls hidden for unassigned project"),
                    
                new("5.5.1", "Authorization", "Admin/Partner RBAC", "Admin Full Access", "Critical",
                    "User is on login page",
                    "1. Login as admin (admin123)\n2. Go to any project's Forecast",
                    "Username: admin, Password: admin123",
                    "All edit controls available",
                    "Admin has full edit access"),
                    
                new("5.5.2", "Authorization", "Admin/Partner RBAC", "Partner Full Access", "Critical",
                    "User is on login page",
                    "1. Login as partner (admin123)\n2. Edit any project's forecast\n3. Click 'Commit Plan'",
                    "Username: partner, Password: admin123",
                    "Save succeeds with success toast",
                    "Partner has full edit access"),
                    
                new("5.6.1", "Authorization", "Role Assignment", "Role Dropdown Visible for Admin", "High",
                    "Logged in as admin",
                    "1. Go to Roster\n2. Click Edit on any user",
                    "Admin account",
                    "'Role' dropdown visible with: Employee, Manager, Partner, Admin",
                    "Admins can view role assignment option"),
                    
                new("5.6.2", "Authorization", "Role Assignment", "Change User Role", "High",
                    "Logged in as admin, editing a user",
                    "1. Change Role dropdown from Employee to Manager\n2. Save",
                    "Any Employee user",
                    "Success toast, role updated in database",
                    "Role change persists"),
                    
                new("5.6.3", "Authorization", "Role Assignment", "Employee Cannot See Role Dropdown", "High",
                    "Logged in as employee",
                    "1. Go to Roster\n2. Click Edit on any user (if allowed)",
                    "Username: employee",
                    "Role dropdown NOT visible or edit button hidden",
                    "Employees cannot assign roles"),
                
                // Epic 6: Notifications
                new("6.1.1", "Notifications", "Toast Notifications", "Success Toast", "Medium",
                    "User is logged in",
                    "1. Perform any save action (e.g., save roster member)",
                    "Any saveable form",
                    "Green success toast appears at top-center",
                    "Success actions show green toast"),
                    
                new("6.1.2", "Notifications", "Toast Notifications", "Error Toast", "Medium",
                    "User is logged in",
                    "1. Try to create roster with duplicate SAP code",
                    "Existing SAP code",
                    "Red error toast appears",
                    "Errors show red toast"),
                    
                new("6.1.3", "Notifications", "Toast Notifications", "Toast Auto-Dismiss", "Low",
                    "Toast notification visible",
                    "1. Trigger a toast\n2. Wait 3-4 seconds",
                    "Any action that triggers toast",
                    "Toast fades out automatically",
                    "Toasts auto-dismiss"),
                    
                new("6.2.1", "Notifications", "Notification History", "View Notification History", "Medium",
                    "Multiple actions performed",
                    "1. Trigger multiple save/error actions\n2. Click bell icon in header",
                    "Multiple previous notifications",
                    "Panel opens showing notification history",
                    "History panel shows past notifications"),
                    
                new("6.2.2", "Notifications", "Notification History", "Unread Badge", "Medium",
                    "New notifications exist",
                    "1. Trigger notifications\n2. Look at bell icon",
                    "Unread notifications",
                    "Green dot badge on bell icon",
                    "Unread count is displayed"),
                    
                new("6.2.3", "Notifications", "Notification History", "Mark All Read", "Low",
                    "Unread notifications exist",
                    "1. Open notification panel\n2. Click checkmark button",
                    "Unread notifications",
                    "Badge disappears, all items marked read",
                    "Mark all read works"),
                    
                new("6.2.4", "Notifications", "Notification History", "Clear All", "Low",
                    "Notifications in history",
                    "1. Open notification panel\n2. Click trash button",
                    "Notification history",
                    "List empties",
                    "Clear all removes history"),
                
                // Epic 7: Navigation
                new("7.1.1", "Navigation", "Command Palette", "Open with Keyboard", "High",
                    "User is logged in",
                    "1. Press Ctrl+K",
                    "N/A",
                    "Command palette modal opens",
                    "Keyboard shortcut works"),
                    
                new("7.1.2", "Navigation", "Command Palette", "Search Commands", "High",
                    "Command palette open",
                    "1. Type 'roster' in search",
                    "N/A",
                    "'Go to Roster' appears in filtered results",
                    "Search filters commands"),
                    
                new("7.1.3", "Navigation", "Command Palette", "Execute Command", "High",
                    "Command palette open with results",
                    "1. Press Enter on 'Go to Roster'",
                    "N/A",
                    "Navigates to Roster page, palette closes",
                    "Command execution works"),
                    
                new("7.1.4", "Navigation", "Command Palette", "Open with Click", "Medium",
                    "User is logged in",
                    "1. Click search icon (magnifying glass) in header",
                    "N/A",
                    "Command palette opens",
                    "Click trigger works"),
                    
                new("7.2.1", "Navigation", "Breadcrumbs", "Breadcrumb Display", "Medium",
                    "User is in project drill-down",
                    "1. Navigate to Project > Forecast",
                    "Any project",
                    "Breadcrumb shows: Resource Platform > Projects > [Name] > Forecast",
                    "Breadcrumbs reflect navigation path"),
                    
                new("7.2.2", "Navigation", "Breadcrumbs", "Breadcrumb Navigation", "Medium",
                    "Breadcrumbs visible",
                    "1. Click 'Projects' in breadcrumb",
                    "Deep navigation path",
                    "Navigates back to Projects list",
                    "Breadcrumb links work"),
                    
                new("7.3.1", "Navigation", "Theme Toggle", "Toggle Theme", "Low",
                    "User is logged in",
                    "1. Click sun/moon icon in header",
                    "N/A",
                    "Theme changes between dark and light",
                    "Theme toggle works"),
                    
                new("7.3.2", "Navigation", "Theme Toggle", "Theme Persistence", "Low",
                    "Theme changed",
                    "1. Change to Light theme\n2. Refresh page",
                    "Theme preference",
                    "Light theme still active",
                    "Theme preference persists")
            };
        }
    }
    
    record TestCase(
        string Id,
        string Epic,
        string UserStory,
        string Name,
        string Priority,
        string Preconditions,
        string Steps,
        string TestData,
        string ExpectedResult,
        string AcceptanceCriteria
    );
}
