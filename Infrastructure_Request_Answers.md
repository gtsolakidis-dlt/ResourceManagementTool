# Infrastructure Request Form - Answers

## Based on: Resource Management Tool Project

---

## 1. Brief Title for Your Request
**Resource Management Tool - Enterprise Production Deployment**

*Alternative titles you can use:*
- Resource Management Platform - Azure Infrastructure Setup
- Financial Forecasting & Resource Management System Deployment
- Enterprise Resource & Project Management Platform

---

## 2. Brief Description of Your Request (Maximum 200 characters)

### Recommended Answer (195 characters):
**"Web-based resource management platform for tracking consultant allocations, project portfolios, and financial forecasting. Replaces Excel-based workflows with automated calculations and RBAC."**

### Alternative (Slightly Shorter - 187 characters):
**"Enterprise platform for managing consulting resources and project financials. Features automated WIP/NSR calculations, role-based access control, and real-time forecasting capabilities."**

---

## 3. Details of the Business Benefits

### Recommended Answer:

**Business Benefits:**

1. **Operational Efficiency**
   - Eliminates manual Excel-based processes for resource allocation and financial tracking
   - Reduces time spent on monthly financial reporting by automating WIP/NSR calculations
   - Enables real-time updates across the organization, eliminating version control issues

2. **Financial Visibility & Control**
   - Real-time project profitability monitoring with automated margin calculations
   - Accurate revenue forecasting based on resource allocations and global rate cards
   - Cumulative financial tracking (Billings, Expenses, Cost, WIP) for better budget control

3. **Improved Decision Making**
   - Project Managers can instantly assess resource costs vs. budget
   - Partners/Admins gain portfolio-level visibility across all engagements
   - Historical tracking enables data-driven capacity planning

4. **Enhanced Collaboration**
   - Centralized platform accessible to 2000+ employees and contractors
   - Role-based access ensures appropriate visibility (Employee, Manager, Partner, Admin)
   - Notification system keeps stakeholders informed of critical changes

5. **Risk Mitigation**
   - Secure authentication and authorization replaces shared Excel files
   - Audit trail through versioned forecasts and timestamp tracking
   - Reduced error rates through automated calculations and validation

6. **Scalability & Future Growth**
   - Foundation for advanced analytics and reporting capabilities
   - Integration-ready architecture for future Azure Entra ID SSO
   - Supports enterprise-scale operations (2000 roster entries, 24-month forecasts)

**Quantifiable Impact:**
- Estimated time savings: 20-30 hours/month per Project Manager
- Improved forecast accuracy: 15-25% through automated calculations
- Enhanced data security and compliance readiness

---

## Infrastructure Requirements Summary

Based on the project architecture, here are the infrastructure needs:

### **Hosting Platform:** Azure (Recommended)

### **Required Services:**
1. **Azure App Service** (for .NET 9 Web API backend)
   - Standard tier or higher (for production SLA)
   - Windows-based hosting

2. **Azure App Service / Static Web App** (for React frontend)
   - Can be hosted separately or as static files from the API

3. **Azure SQL Database**
   - Standard tier (S2 or higher)
   - Estimated size: 10-50 GB initial
   - Supports up to 2000 roster records, multiple projects with historical data

4. **Application Insights** (Monitoring)
   - Track API performance (200ms calculation SLA)
   - Error logging and diagnostics

### **Optional Enhancements:**
- Azure Key Vault (for storing connection strings securely)
- Azure Entra ID (for future SSO integration)
- Azure DevOps / GitHub Actions (CI/CD pipeline)

### **Network Requirements:**
- HTTPS only (SSL certificate via Azure-managed or custom)
- Internal network access (if required for on-premise integration)

### **Estimated Monthly Cost:**
- **Development Environment:** $100-150/month
- **Production Environment:** $300-500/month
  (Based on App Service S1 + SQL Database S2)

---

## Additional Context for Infrastructure Team

### **Performance Requirements:**
- API Response Time: < 200ms for financial calculations (24 months × 50 resources)
- Concurrent Users: Support for 100-200 simultaneous users
- Browser Support: Chrome/Edge (latest versions)

### **Security Requirements:**
- BCrypt password hashing
- All API endpoints require authentication
- Role-based authorization (4 levels: Employee, Manager, Partner, Admin)

### **Backup & Recovery:**
- Daily automated SQL backups required
- Point-in-time restore capability recommended

### **Support & Maintenance:**
- Planned Azure Entra ID integration (future phase)
- Regular security updates for .NET and React dependencies

---

## Technical Stack Reference

**Frontend:** React 18, TypeScript, Vite  
**Backend:** .NET 9 Web API, Dapper ORM, MediatR (CQRS)  
**Database:** SQL Server  
**Authentication:** Basic Auth (current), Azure Entra ID (future)

---

*Document Generated: 2026-02-03*  
*Project Version: 2.1*  
*Based on: Functional_Technical_Requirements.md & HighLevelDesign.md*
