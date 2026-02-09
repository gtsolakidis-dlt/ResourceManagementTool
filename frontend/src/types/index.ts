export interface RosterMember {
    id: number;
    sapCode: string;
    fullNameEn: string;
    legalEntity?: string;
    functionBusinessUnit?: string;
    costCenterCode?: string;
    level?: string;
    technicalRole?: string;
    monthlySalary: number;
    monthlyEmployerContributions: number; // Renamed from employerContributions
    cars: number;
    ticketRestaurant: number;
    metlife: number;

    // Calculated (Greek Labor Laws - 14 salaries)
    monthlyCost_12months: number;
    monthlyCost_14months: number;
    dailyCost: number;
    projectedRevenue: number;
    role?: string;
    username?: string;
}

export interface Project {
    id: number;
    name: string;
    wbs: string;
    wbsCode?: string;
    startDate: string;
    endDate: string;
    actualBudget: number;
    nominalBudget: number;
    discount: number;
    recoverability: number;
    targetMargin: number;
    canEdit?: boolean; // RBAC: indicates if current user can edit this project
}

export interface ResourceAllocation {
    id: number;
    forecastVersionId: number;
    rosterId: number;
    month: string;
    allocatedDays: number;
}

export interface MonthlyFinancial {
    month: string;
    openingBalance: number;
    monthlyBillings: number;
    monthlyExpenses: number;
    billings: number;
    wip: number;
    expenses: number;
    cost: number;
    nsr: number;
    margin: number;
    isOverridden: boolean;
}

export interface GlobalRate {
    id: number;
    level: string;
    nominalRate: number;
    updatedAt: string;
}

// Part 2: Project-specific rates
export interface ProjectRate {
    id: number;
    projectId: number;
    level: string;
    nominalRate: number;
    actualDailyRate: number; // NominalRate * (1 - Discount/100)
    createdAt: string;
    updatedAt: string;
}

// Part 3: Monthly Snapshot Status
export type SnapshotStatus = 'Pending' | 'Editable' | 'Confirmed';

// Part 3: Project Monthly Snapshot (persisted financial state)
export interface ProjectMonthlySnapshot {
    id: number;
    projectId: number;
    forecastVersionId: number;
    month: string;
    status: SnapshotStatus;

    // Financial Values (OB, CB, WIP, DE, OC)
    openingBalance: number;
    cumulativeBillings: number;
    wip: number;
    directExpenses: number;
    operationalCost: number;

    // Additional metrics
    monthlyBillings: number;
    monthlyExpenses: number;
    cumulativeExpenses: number;
    nsr: number;
    margin: number;


    // Original values (prior to override)
    originalOpeningBalance?: number;
    originalCumulativeBillings?: number;
    originalWip?: number;
    originalDirectExpenses?: number;
    originalOperationalCost?: number;


    // Override tracking
    isOverridden: boolean;
    overriddenAt?: string;
    overriddenBy?: string;

    // Confirmation tracking
    confirmedAt?: string;
    confirmedBy?: string;
}

// Part 3: Override request
export interface OverwriteSnapshotRequest {
    projectId: number;
    forecastVersionId: number;
    month: string;
    overriddenBy: string;
    openingBalance?: number;
    wip?: number;
    directExpenses?: number;
    operationalCost?: number;
    nsr?: number;
    margin?: number;
    cumulativeBillings?: number;
}

// Part 3: Confirm request
export interface ConfirmMonthRequest {
    projectId: number;
    forecastVersionId: number;
    month: string;
    confirmedBy: string;
}

export interface AuditLog {
    id: number;
    entityName: string;
    entityId: string;
    action: string;
    oldValues?: string;
    newValues?: string;
    changedBy: string;
    changedAt: string;
}

// Resource Suggestion System
export interface ResourceSuggestion {
    rosterId: number;
    fullNameEn: string;
    level?: string;
    functionBusinessUnit?: string;
    technicalRole?: string;
    seniorityTier?: string;
    dailyCost: number;
    totalAvailableDays: number;
    totalCapacityDays: number;
    availabilityPercentage: number;
    monthlyAvailability: MonthlyAvailability[];
    projectedCost: number;
    remainingBudget: number;
    budgetImpactPercentage: number;
    budgetFit: 'within' | 'tight' | 'over';
}

export interface MonthlyAvailability {
    month: string;
    allocatedDays: number;
    availableDays: number;
    capacityDays: number;
}
