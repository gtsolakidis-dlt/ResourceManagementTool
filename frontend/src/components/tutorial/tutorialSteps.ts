import type { DriveStep } from "driver.js";

// Define role-specific steps
export const getStepsForRole = (role: string): DriveStep[] => {

    // Steps common to everyone (Introduction & Basic Nav)
    const commonSteps: DriveStep[] = [
        {
            element: '#nav-dashboard',
            popover: {
                title: 'Welcome to Resourcely',
                description: 'This is your central command center. View <strong>Utilization Rates</strong>, active <strong>Headcount</strong>, and your upcoming <strong>Allocation Alerts</strong> at a glance.',
                side: "right",
                align: 'start'
            }
        },
        {
            element: '#btn-search',
            popover: {
                title: 'Command Palette (Ctrl+K)',
                description: 'Power user tip: Press <strong>Ctrl+K</strong> anytime to open the Command Palette. Instantly jump to any Project, find a Colleague, or execute actions like "Create Project" without lifting your hands from the keyboard.',
                side: "bottom",
                align: 'end'
            }
        }
    ];

    // Roster Management
    const rosterStep: DriveStep = {
        element: '#nav-roster',
        popover: {
            title: 'Talent Pool & Roster',
            description: role === 'Admin' || role === 'Partner'
                ? 'Your single source of truth for people data. Manage <strong>Grades, Costs, and Skills</strong> here. <br/><br/><em>As an Admin, you can also assign System Roles (Manager/Employee) to users from the Edit view.</em>'
                : 'Browse the company directory. Find colleagues by <strong>Grade, Skill, or Availability</strong> to staff your next initiative.',
            side: "right",
            align: 'start'
        }
    };

    // Project Management (Context-aware)
    const projectsStep: DriveStep = {
        element: '#nav-projects',
        popover: {
            title: 'Project Portfolio',
            description: role === 'Employee'
                ? 'Access your <strong>Assigned Projects</strong>. View your booking schedule and understand exactly where you are allocated for the coming months.'
                : 'The heart of the system. <strong>Create Projects</strong>, manage WBS codes, and oversee budgets. <br/><br/>Dive into any project to access the <strong>Forecasting Matrix</strong> where you can allocate resources month-by-month.',
            side: "right",
            align: 'start'
        }
    };

    // Forecasting & Financials (Manager+)
    const analyticsStep: DriveStep = {
        element: '#nav-analytics',
        popover: {
            title: 'Financial Intelligence',
            description: 'Analyze the health of your portfolio. <br/><br/>• <strong>WIP (Work In Progress):</strong> Calculated automatically based on <em>Allocated Days × Global Rate × (1 - Discount)</em>.<br/>• <strong>Margin:</strong> Track profitability against your targets.',
            side: "right",
            align: 'start'
        }
    };

    // Global Configuration (Admin)
    const settingsStep: DriveStep = {
        element: '#nav-settings',
        popover: {
            title: 'Global Configuration',
            description: 'Define the baseline physics of the system. <br/><br/>Set <strong>Nominal Rates</strong> per Grade (e.g., Senior Consultant = €500/day). These rates drive all revenue and WIP calculations across every project automatically.',
            side: "right",
            align: 'start'
        }
    };

    const notificationStep: DriveStep = {
        element: '#btn-notifications',
        popover: {
            title: 'Stay Informed',
            description: 'Your notification center for important alerts. Receive updates on <strong>Allocation conflicts</strong>, budget overruns, or system announcements.',
            side: "bottom",
            align: 'end'
        }
    };

    // Construct flow based on role
    let steps = [...commonSteps];

    if (role === 'Employee') {
        steps.push(projectsStep);
        steps.push(rosterStep);
        steps.push(notificationStep);
    } else {
        // Managers/Partners/Admins need the full flow
        steps.push(projectsStep);
        steps.push(rosterStep);

        // Add specific Analytics/Financials step
        if (role !== 'Employee') {
            steps.push(analyticsStep);
        }

        if (role === 'Admin' || role === 'Partner') {
            steps.push(settingsStep);
        }
        steps.push(notificationStep);
    }

    // Final "Good to go" step (generic center modal)
    steps.push({
        element: 'body', // Center of screen
        popover: {
            title: 'You are all set!',
            description: 'You can restart this tour anytime by clicking the <strong>Help (?)</strong> icon in the top header. <br/><br/>Enjoy using Resourcely!',
            side: "top",
            align: 'center'
        }
    });

    return steps;
};
