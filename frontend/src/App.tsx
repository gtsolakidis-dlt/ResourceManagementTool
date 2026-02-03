import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/layout/Layout';
import RosterPage from './pages/roster/RosterPage';
import ResourceProfilePage from './pages/roster/ResourceProfilePage';
import ProjectsPage from './pages/projects/ProjectsPage';
import ProjectDetailsPage from './pages/projects/ProjectDetailsPage';
import ForecastingPage from './pages/forecasting/ForecastingPage';
import FinancialInputPage from './pages/projects/FinancialInputPage';
import GlobalRatesPage from './pages/admin/GlobalRatesPage';
import { NavigationProvider } from './context/NavigationContext';
import { ThemeProvider } from './context/ThemeContext';
import { NotificationProvider } from './context/NotificationContext';
import DashboardPage from './pages/dashboard/DashboardPage';
import AnalyticsPage from './pages/analytics/AnalyticsPage';

import { AuthProvider, useAuth } from './context/AuthContext';
import LoginPage from './pages/auth/LoginPage';

// Guard Component
const RequireAuth: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, isLoading } = useAuth();
  if (isLoading) {
    return (
      <div style={{ height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--bg-color)', color: 'var(--deloitte-green)' }}>
        Loading...
      </div>
    );
  }
  if (!user) return <Navigate to="/login" replace />;
  return <>{children}</>;
};

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route path="*" element={
        <RequireAuth>
          <Layout>
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/analytics" element={<AnalyticsPage />} />
              <Route path="/roster" element={<RosterPage />} />
              <Route path="/resources/:id" element={<ResourceProfilePage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/projects/:id" element={<ProjectDetailsPage />} />
              <Route path="/projects/:id/forecast" element={<ForecastingPage />} />
              <Route path="/projects/:id/billings" element={<FinancialInputPage type="billing" />} />
              <Route path="/projects/:id/expenses" element={<FinancialInputPage type="expense" />} />
              <Route path="/admin/rates" element={<GlobalRatesPage />} />
              <Route path="*" element={<div>Page Not Found</div>} />
            </Routes>
          </Layout>
        </RequireAuth>
      } />
    </Routes>
  );
};

const App: React.FC = () => {
  return (
    <Router>
      <AuthProvider>
        <NavigationProvider>
          <ThemeProvider>
            <NotificationProvider>
              <AppRoutes />
            </NotificationProvider>
          </ThemeProvider>
        </NavigationProvider>
      </AuthProvider>
    </Router>
  );
};

export default App;
