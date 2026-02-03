# Premium Navigation Experience Design Proposal

## 1. Overview
To elevate the user experience and provide seamless navigation through deep hierarchies (e.g., specific Project details), we propose transforming the static Sidebar and Top-bar into dynamic, context-aware components. This "Adaptive Navigation" system will guide the user without them losing their place.

![Design Mockup](nav_design_mockup_1769172551748.png)

## 2. Component: Adaptive Sidebar (The "Left")

### Current State
Static list of high-level modules (Roster, Projects).

### Proposed Design
The Sidebar will become state-aware. It will have two modes:

1.  **Global Mode (Default)**:
    *   Displays core modules: Dashboard, Roster, Projects, Analytics.
    *   Clean, high-contrast icons.

2.  **Context Mode (Drill-down)**:
    *   Trigger: When a user enters a specific entity (e.g., clicking a Project).
    *   Behavior: The parent module ("Projects") remains active. A **Nested Sub-menu** expands below it.
    *   **Sub-menu Items**:
        *   Overview
        *   Resource Forecast
        *   Financials
        *   Billings & Expenses
    *   **Visuals**: Sub-items are indented (24px), use a slightly smaller font (0.85rem), and have a border-left indicator when active. This creates a clear visual hierarchy.

### Implementation Strategy
*   **Routing**: Use `useLocation()` and `matchPath` to detect if we are in a `:id` sub-route.
*   **Animation**: `Framer Motion` or CSS Transitions for the sub-menu sliding down (Accordion effect).

## 3. Component: Hierarchical Top-bar (The "Top")

### Current State
Static text ("Resource Platform > Dashboard").

### Proposed Design
A fully functional **Dynamic Breadcrumb System**.

1.  **Auto-Generation**: 
    *   Parsed from the URL path: `/projects/123/forecast`.
    *   Maps to: `Projects > [Project Name] > Forecast`.
2.  **Hyperlinks**:
    *   Each segment (except the last) is a clickable link.
    *   Clicking `Projects` navigates back to the portfolio list.
    *   Clicking `[Project Name]` navigates to the project overview.
3.  **Visuals**:
    *   Separators: Sleek Chevron (`>`) icons.
    *   Colors: Inactive path is `text-muted`. Active (current) page is `text-primary` or `deloitte-green`.
    *   **Project Name Resolution**: Since the URL only has the ID (`123`), we will implement a `BreadcrumbContext` or simpler separate component that fetches/displays the project name, or pass it via Route state to avoid extra API calls.

## 4. User Experience Gains
*   **Wayfinding**: Users never wonder "Where am I?".
*   **Speed**: Switching between "Financials" and "Forecast" becomes a 1-click action in the sidebar (vs going back to dashboard).
*   **Premium Feel**: Smooth transitions and "App-like" hierarchy vs "Web-page-like" reloading.

## 5. Technical Stack
*   **Framework**: React Router v6 (`useMatches`, `useLocation`).
*   **Styling**: CSS Modules / Scoped CSS with CSS Variables (for Theming).
*   **Icons**: Lucide React (feather-light icons).

This design ensures the application feels like a cohesive, professional tool suitable for enterprise resource management.
