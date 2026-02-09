import api from './client';
import type {
    RosterMember,
    Project,
    ResourceAllocation,
    MonthlyFinancial,
    ProjectMonthlySnapshot,
    ConfirmMonthRequest,
    OverwriteSnapshotRequest,
    AuditLog,
    ResourceSuggestion
} from '../types';

export const rosterService = {
    getMembers: (params?: any) => api.get<RosterMember[]>('/roster', { params }),
    getMember: (id: number) => api.get<RosterMember>(`/roster/${id}`),
    createMember: (data: Partial<RosterMember>) => api.post<number>('/roster', data),
    updateMember: (id: number, data: Partial<RosterMember>) => api.put(`/roster/${id}`, data),
    deleteMember: (id: number) => api.delete(`/roster/${id}`),
    exportRoster: () => api.get('/roster/export', { responseType: 'arraybuffer' }),
    importRoster: (formData: FormData) => api.post('/roster/import', formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
};

export const projectService = {
    getProjects: () => api.get<Project[]>('/projects'),
    getProject: (id: number) => api.get<Project>(`/projects/${id}`),
    createProject: (data: Partial<Project>) => api.post<number>('/projects', data),
    updateProject: (id: number, data: Partial<Project>) => api.put(`/projects/${id}`, data),
    exportProjects: () => api.get('/projects/export', { responseType: 'arraybuffer' }),
    importProjects: (formData: FormData) => api.post('/projects/import', formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
};

export const forecastService = {
    getAllocations: (versionId: number) => api.get<ResourceAllocation[]>(`/forecasts/allocations/${versionId}`),
    upsertAllocations: (allocations: ResourceAllocation[]) => api.post('/forecasts/allocations', allocations),
    getVersions: (projectId: number) => api.get<any[]>(`/forecasts/${projectId}/versions`),
    cloneVersion: (projectId: number, sourceVersionId: number) => api.post('/forecasts/clone', { projectId, sourceVersionId }),
    deleteAllocation: (versionId: number, rosterId: number) => api.delete(`/forecasts/allocations/${versionId}/${rosterId}`),
};

export const financialService = {
    getFinancials: (projectId: number, forecastVersionId: number) =>
        api.get<MonthlyFinancial[]>(`/financials/${projectId}/calculate/${forecastVersionId}`),
    upsertBilling: (data: any) => api.post('/financials/billing', data),
    upsertExpense: (data: any) => api.post('/financials/expense', data),
    upsertOverride: (data: any) => api.post('/financials/override', data),
};

export const snapshotService = {
    getSnapshots: (projectId: number, forecastVersionId: number) =>
        api.get<ProjectMonthlySnapshot[]>(`/financials/${projectId}/snapshots/${forecastVersionId}`),
    confirmMonth: (data: ConfirmMonthRequest) =>
        api.post<boolean>('/financials/snapshots/confirm', data),
    overwriteSnapshot: (data: OverwriteSnapshotRequest) =>
        api.post<boolean>('/financials/snapshots/overwrite', data),
    clearOverride: (projectId: number, forecastVersionId: number, month: string) =>
        api.post<boolean>('/financials/snapshots/clear-override', { projectId, forecastVersionId, month }),
};

export const globalRateService = {
    getAll: () => api.get<any[]>('/globalrates'),
    create: (data: any) => api.post<number>('/globalrates', data),
    update: (id: number, data: any) => api.put(`/globalrates/${id}`, data),
};

export const auditService = {
    getRecent: (count: number = 10) => api.get<AuditLog[]>('/audits/recent', { params: { count } }),
};

export const suggestionService = {
    getResourceSuggestions: (projectId: number, forecastVersionId: number) =>
        api.get<ResourceSuggestion[]>('/suggestions/resources', {
            params: { projectId, forecastVersionId }
        }),
};
