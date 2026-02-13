import React, { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { forecastService, rosterService, projectService } from '../../api/services';
import type { ResourceAllocation, RosterMember, Project } from '../../types';
import { Plus, Save, Loader2, ChevronLeft, Layers, UserPlus, Trash2 } from 'lucide-react';
import PremiumSelect from '../../components/common/PremiumSelect';
import PremiumNumericInput from '../../components/common/PremiumNumericInput';
import { getSeniorityBadgeClass } from '../../constants/seniorityLevels';
import './ForecastingPage.css';
import { useNotification } from '../../context/NotificationContext';

import { useNavigation } from '../../context/NavigationContext';

const ForecastingPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const projectId = parseInt(id || '0');
    const navigate = useNavigate();
    const { notify } = useNotification();
    const { setBreadcrumbs, setActiveSection, setSidebarSubItems } = useNavigation();

    // Grid Navigation Refs
    const gridRefs = useRef<(HTMLInputElement | null)[][]>([]);

    const handleGridKey = (e: React.KeyboardEvent, row: number, col: number) => {
        if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (row > 0) gridRefs.current[row - 1]?.[col]?.focus();
        }
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            const nextRow = gridRefs.current[row + 1];
            if (nextRow) nextRow[col]?.focus();
        }
        // Future: Add Left/Right with modifiers
    };

    const [allocations, setAllocations] = useState<ResourceAllocation[]>([]);
    const [project, setProject] = useState<Project | null>(null);
    const [roster, setRoster] = useState<RosterMember[]>([]);
    const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
    const [versions, setVersions] = useState<any[]>([]);
    const [selectedVersionId, setSelectedVersionId] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (projectId) loadInitialData();
    }, [projectId]);

    useEffect(() => {
        if (project) {
            setActiveSection('projects');
            setBreadcrumbs([
                { label: 'Resource Platform', path: '/' },
                { label: 'Projects', path: '/projects' },
                { label: project.name, path: `/projects/${projectId}` },
                { label: 'Forecast', disabled: true }
            ]);
            setSidebarSubItems([
                { label: project.name, path: `/projects/${projectId}`, exact: false, isHeader: true },
                { label: 'Overview', path: `/projects/${projectId}` },
                { label: 'Billings', path: `/projects/${projectId}/billings` },
                { label: 'Expenses', path: `/projects/${projectId}/expenses` },
                { label: 'Forecast', path: `/projects/${projectId}/forecast` }
            ]);
        }
    }, [project]);

    useEffect(() => {
        if (selectedVersionId) loadAllocations();
    }, [selectedVersionId]);

    const loadInitialData = async () => {
        setLoading(true);
        try {
            const [rosterRes, versionRes, projectRes] = await Promise.all([
                rosterService.getMembers(),
                forecastService.getVersions(projectId),
                projectService.getProject(projectId)
            ]);
            setRoster(rosterRes.data);
            setVersions(versionRes.data);
            setProject(projectRes.data);
            if (versionRes.data.length > 0) {
                setSelectedVersionId(versionRes.data[0].id);
            }
        } catch (err) {
            console.error('Initial load failed', err);
        } finally {
            setLoading(false);
        }
    };

    const loadAllocations = async () => {
        if (!selectedVersionId) return;
        setLoading(true);
        try {
            const res = await forecastService.getAllocations(selectedVersionId);
            // Normalize date strings to YYYY-MM-01 to match generated month columns.
            // Backend may return end-of-month dates (e.g. 2026-01-31); force day to 01.
            setAllocations(res.data.map(a => {
                const dateOnly = a.month.toString().split('T')[0];
                const [y, m] = dateOnly.split('-');
                return { ...a, month: `${y}-${m}-01` };
            }));
        } catch (err) {
            console.error('Allocations load failed', err);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        if (allocations.length === 0) return;
        setSaving(true);
        try {
            await forecastService.upsertAllocations(allocations);
            notify.success('Allocation matrix committed successfully');
        } catch (err) {
            console.error('Save failed', err);
            notify.error('Failed to commit plan');
        } finally {
            setSaving(false);
        }
    };

    const handleAssignResource = (rosterId: number) => {
        if (!selectedVersionId) {
            notify.error('No active scenario selected');
            return;
        }
        if (!project) {
            notify.error('Project data not loaded');
            return;
        }

        // Check if already assigned
        if (allocations.some(a => a.rosterId === rosterId)) {
            notify.error('Resource already assigned to this project');
            return;
        }

        // Generate allocations for all months in the project timeline
        const projectMonths = generateProjectMonths();
        const newAllocations: ResourceAllocation[] = projectMonths.map(month => ({
            id: 0,
            forecastVersionId: selectedVersionId,
            rosterId: rosterId,
            month: month,
            allocatedDays: 0
        }));

        setAllocations(prev => [...prev, ...newAllocations]);
        setIsAssignModalOpen(false);
        notify.success('Resource assigned to matrix');
    };

    const handleRemoveResource = async (rosterId: number) => {
        if (!selectedVersionId) return;

        if (window.confirm('Are you sure you want to remove this resource and all their allocations?')) {
            try {
                await forecastService.deleteAllocation(selectedVersionId, rosterId);
                setAllocations(prev => prev.filter(a => a.rosterId !== rosterId));
                notify.success('Resource removed from matrix');
            } catch (err) {
                console.error('Delete failed', err);
                notify.error('Failed to remove resource');
            }
        }
    };

    const generateProjectMonths = () => {
        if (!project) return [];
        const months: string[] = [];

        // Parse date strings directly to avoid timezone-induced month rollback.
        // e.g. "2024-03-01" parsed as UTC midnight shifts to Feb 28 in UTC-5.
        const [startYear, startMonth] = project.startDate.split('-').map(Number);
        const [endYear, endMonth] = project.endDate.split('-').map(Number);

        let curYear = startYear;
        let curMonth = startMonth;

        while (curYear < endYear || (curYear === endYear && curMonth <= endMonth)) {
            const mm = String(curMonth).padStart(2, '0');
            months.push(`${curYear}-${mm}-01`);
            curMonth++;
            if (curMonth > 12) {
                curMonth = 1;
                curYear++;
            }
        }

        if (months.length === 0) {
            return Array.from(new Set(allocations.map(a => a.month))).sort();
        }

        return months;
    };

    const handleClone = async () => {
        setSaving(true);
        try {
            // If no version selected, we treat it as create new (sourceId=0)
            const sourceId = selectedVersionId || 0;
            const res = await forecastService.cloneVersion(projectId, sourceId);
            await loadInitialData();
            if (res.data && res.data.id) {
                setSelectedVersionId(res.data.id);
                notify.success('New scenario created');
            }
        } catch (err) {
            console.error('Clone failed', err);
            notify.error((err as any).response?.data?.Message || 'Failed to create scenario');
        } finally {
            setSaving(false);
        }
    };

    if (loading && !selectedVersionId) return <div className="loading-state"><Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" /><span>Initializing resource allocation engine...</span></div>;

    const months = generateProjectMonths();
    const resources = Array.from(new Set(allocations.map(a => a.rosterId)));

    return (
        <div className="forecasting-premium">
            <header className="page-header" style={{ marginBottom: '3rem' }}>
                <div>
                    <h1 style={{ marginBottom: '0.5rem' }}>Resource Allocation Matrix</h1>
                    <p style={{ color: 'var(--text-muted)' }}>Strategize workforce deployment and quantify effort across project timelines.</p>
                </div>
                <div style={{ display: 'flex', gap: '1rem' }}>
                    {project?.canEdit && (
                        <>
                            <button className="btn-outline-premium" onClick={handleClone} disabled={saving}>
                                {saving ? <Loader2 className="animate-spin" size={18} /> : <Plus size={18} />}
                                {versions.length === 0 ? 'Create Scenario' : 'Clone Scenario'}
                            </button>
                            <button className="btn-premium" onClick={handleSave} disabled={saving}>
                                {saving ? <Loader2 className="animate-spin" size={18} /> : <Save size={18} />}
                                Commit Plan
                            </button>
                        </>
                    )}
                    <button className="btn-outline-premium" onClick={() => navigate(`/projects/${projectId}`)}>
                        <ChevronLeft size={18} />
                        Exit
                    </button>
                </div>
            </header>

            <div className="glass-panel" style={{ padding: '1.5rem', marginBottom: '2rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div style={{ display: 'flex', gap: '2rem', alignItems: 'center' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        <Layers size={18} color="var(--deloitte-green)" />
                        <span style={{ fontSize: '0.875rem', fontWeight: 600 }}>Active Scenario:</span>
                    </div>
                    {versions.length > 0 ? (
                        <div style={{ width: '300px' }}>
                            <PremiumSelect
                                value={String(selectedVersionId || '')}
                                onChange={(val) => setSelectedVersionId(Number(val))}
                                options={versions.map(v => ({
                                    value: String(v.id),
                                    label: `Version ${v.versionNumber} (${new Date(v.createdAt).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' })})`
                                }))}
                            />
                        </div>
                    ) : (
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem', fontStyle: 'italic' }}>No scenarios created yet</span>
                    )}
                </div>
                {project?.canEdit && (
                    <button
                        className="btn-outline-premium"
                        onClick={() => setIsAssignModalOpen(true)}
                    >
                        <UserPlus size={14} />
                        <span>Assign Resource</span>
                    </button>
                )}
            </div>

            {isAssignModalOpen && (
                <div className="modal-overlay" onClick={() => setIsAssignModalOpen(false)}>
                    <div className="modal-content animate-scale-up" onClick={e => e.stopPropagation()}>
                        <header style={{ marginBottom: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <h2 style={{ fontSize: '1.25rem', fontWeight: 600, margin: 0 }}>Select Talent</h2>
                            <button className="btn-icon-premium" onClick={() => setIsAssignModalOpen(false)}>×</button>
                        </header>
                        <div style={{ display: 'grid', gap: '0.75rem', maxHeight: '60vh', overflowY: 'auto', paddingRight: '0.5rem' }}>
                            {roster.filter(r => !resources.includes(r.id)).map(member => (
                                <div
                                    key={member.id}
                                    className="roster-item-selectable"
                                    onClick={() => handleAssignResource(member.id)}
                                >
                                    <div className="roster-info">
                                        <h4>{member.fullNameEn}</h4>
                                        <div className="roster-sub">{member.level} • {member.functionBusinessUnit}</div>
                                    </div>
                                    <Plus size={16} color="var(--deloitte-green)" />
                                </div>
                            ))}
                            {roster.filter(r => !resources.includes(r.id)).length === 0 && (
                                <div style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-muted)' }}>
                                    <p>All available talent has been assigned to this project.</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}

            <div className="forecast-matrix-wrapper animate-fade-in" style={{ overflowX: 'auto' }}>
                <table className="premium-table">
                    <thead>
                        <tr>
                            <th className="sticky-col-header">Talent / Seniority</th>
                            {months.length > 0 ? months.map(month => {
                                const [y, m] = month.split('-').map(Number);
                                const label = new Date(y, m - 1, 1).toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
                                return <th key={month} style={{ minWidth: '120px' }}>{label}</th>;
                            }) : <th style={{ textAlign: 'center' }}>No timeline data (Check Project Dates)</th>}
                            <th style={{ width: '80px', textAlign: 'right' }}></th>
                        </tr>
                    </thead>
                    <tbody>
                        {resources.map((rosterId, rowIndex) => {
                            const member = roster.find(r => r.id === rosterId);
                            if (!member) return null;

                            // Initialize row ref array
                            if (!gridRefs.current[rowIndex]) gridRefs.current[rowIndex] = [];

                            return (
                                <tr key={rosterId}>
                                    <td className="sticky-col-cell">
                                        <div
                                            onClick={() => navigate(`/resources/${member.id}`)}
                                            className="resource-name-link"
                                        >
                                            {member.fullNameEn}
                                        </div>
                                        <span className={`level-badge ${getSeniorityBadgeClass(member.level || '')}`}>
                                            {member.level}
                                        </span>
                                    </td>
                                    {months.map((month, colIndex) => {
                                        const alloc = allocations.find(a => a.rosterId === rosterId && a.month === month);
                                        return (
                                            <td key={month} style={{ textAlign: 'center' }}>
                                                <PremiumNumericInput
                                                    hideControls={true}
                                                    inputRef={(el) => {
                                                        if (el) gridRefs.current[rowIndex][colIndex] = el;
                                                    }}
                                                    onKeyDown={(e) => handleGridKey(e, rowIndex, colIndex)}
                                                    step={1}
                                                    value={alloc?.allocatedDays || 0}
                                                    onChange={(val) => {
                                                        if (!project?.canEdit) return;
                                                        if (alloc) {
                                                            setAllocations(prev => prev.map(p =>
                                                                (p.rosterId === rosterId && p.month === month) ? { ...p, allocatedDays: val } : p
                                                            ));
                                                        } else {
                                                            // Upsert: create a new allocation for this cell
                                                            setAllocations(prev => [...prev, {
                                                                id: 0,
                                                                forecastVersionId: selectedVersionId!,
                                                                rosterId,
                                                                month,
                                                                allocatedDays: val
                                                            }]);
                                                        }
                                                    }}
                                                    disabled={!project?.canEdit}
                                                    style={{ width: '120px' }}
                                                />
                                            </td>
                                        );
                                    })}
                                    <td style={{ textAlign: 'right' }}>
                                        {project?.canEdit && (
                                            <button
                                                className="btn-icon-premium"
                                                onClick={() => handleRemoveResource(rosterId)}
                                                style={{ color: '#ef4444', background: 'none', border: 'none', cursor: 'pointer' }}
                                            >
                                                <Trash2 size={16} />
                                            </button>
                                        )}
                                    </td>
                                </tr>
                            );
                        })}
                        {resources.length === 0 && (
                            <tr>
                                <td colSpan={months.length + 2} style={{ textAlign: 'center', padding: '4rem', color: 'var(--text-muted)' }}>
                                    No resources assigned to this scenario yet. Click "Assign Resource" to start.
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default ForecastingPage;
