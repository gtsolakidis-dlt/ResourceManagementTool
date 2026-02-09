import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { projectService, financialService } from '../../api/services';
import type { Project, MonthlyFinancial } from '../../types';
import { Save, Loader2, ChevronLeft } from 'lucide-react';
import PremiumNumericInput from '../../components/common/PremiumNumericInput';
import './FinancialInputPage.css';

interface Props {
    type: 'billing' | 'expense';
}

import { useNavigation } from '../../context/NavigationContext';
import { useNotification } from '../../context/NotificationContext';

const FinancialInputPage: React.FC<Props> = ({ type }) => {
    const { id } = useParams<{ id: string }>();
    const projectId = parseInt(id || '0');
    const navigate = useNavigate();
    const { setBreadcrumbs, setActiveSection, setSidebarSubItems } = useNavigation();
    const { notify } = useNotification();

    const [project, setProject] = useState<Project | null>(null);
    const [financials, setFinancials] = useState<MonthlyFinancial[]>([]);

    const [values, setValues] = useState<Record<string, number>>({});
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (projectId) loadData();
    }, [projectId]);

    useEffect(() => {
        if (project) {
            setActiveSection('projects');
            const label = type === 'billing' ? 'Billings' : 'Expenses';
            setBreadcrumbs([
                { label: 'Resource Platform', path: '/' },
                { label: 'Projects', path: '/projects' },
                { label: project.name, path: `/projects/${projectId}` },
                { label: label, disabled: true }
            ]);
            setSidebarSubItems([
                { label: project.name, path: `/projects/${projectId}`, exact: false, isHeader: true },
                { label: 'Overview', path: `/projects/${projectId}` },
                { label: 'Billings', path: `/projects/${projectId}/billings` },
                { label: 'Expenses', path: `/projects/${projectId}/expenses` },
                { label: 'Forecast', path: `/projects/${projectId}/forecast` }
            ]);
        }
    }, [project, type]);

    const loadData = async () => {
        setLoading(true);
        try {
            // Load Project Core Data
            const projRes = await projectService.getProject(projectId);
            setProject(projRes.data);

            // Attempt to load Financials
            try {
                const finRes = await financialService.getFinancials(projectId, 1);
                setFinancials(finRes.data);

                const initialValues: Record<string, number> = {};
                finRes.data.forEach(f => {
                    initialValues[f.month] = type === 'billing' ? f.monthlyBillings : f.monthlyExpenses;
                });
                setValues(initialValues);
            } catch (finError) {
                console.warn('Financials unavailable (possibly new project):', finError);
                setFinancials([]);
                setValues({});
            }
        } catch (error) {
            console.error('Load error:', error);
            // If project load fails, we can't do much
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        setSaving(true);
        try {
            for (const month of Object.keys(values)) {
                const payload = { projectId, month, amount: values[month] };
                if (type === 'billing') await financialService.upsertBilling(payload);
                else await financialService.upsertExpense(payload);
            }
            notify.success('Financial records updated successfully');
            navigate(`/projects/${projectId}`);
        } catch (error) {
            console.error('Save error:', error);
            notify.error('Failed to update financial records');
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div className="loading-state" style={{ height: '80vh' }}><Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" /><span>Synchronizing financial records...</span></div>;

    return (
        <div className="financial-premium-container">
            <header className="page-header" style={{ marginBottom: '3rem' }}>
                <div>
                    <h1 style={{ marginBottom: '0.5rem' }}>{project?.name || 'Project'}</h1>
                    <p style={{ color: 'var(--text-muted)' }}>Adjust monthly {type} targets for financial precision.</p>
                </div>
                <div style={{ display: 'flex', gap: '1rem' }}>
                    <button className="btn-outline-premium" onClick={() => navigate(`/projects/${projectId}`)}>
                        <ChevronLeft size={18} />
                        Discard
                    </button>
                    <button className="btn-premium" onClick={handleSave} disabled={saving}>
                        {saving ? <Loader2 className="animate-spin" size={18} /> : <Save size={18} />}
                        Confirm Changes
                    </button>
                </div>
            </header>

            {financials.length === 0 ? (
                <div className="glass-panel" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                    <p style={{ fontSize: '1.1rem', marginBottom: '0.5rem' }}>No financial periods available.</p>
                    <p style={{ fontSize: '0.875rem' }}>Financial data will appear after resource allocations are made in the Forecast section.</p>
                </div>
            ) : (
                <div className="table-wrapper animate-fade-in" style={{ overflowX: 'auto', paddingBottom: '1rem' }}>
                    <table className="premium-table horizontal-financials">
                        <thead>
                            <tr>
                                <th style={{ minWidth: '150px', position: 'sticky', left: 0, zIndex: 10, background: 'var(--card-bg)' }}>Measure</th>
                                {financials.map(f => {
                                    const [y, m] = f.month.split('-').map(Number);
                                    const label = new Date(y, m - 1, 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
                                    return (
                                        <th key={f.month} style={{ minWidth: '140px', textAlign: 'center' }}>
                                            {label}
                                        </th>
                                    );
                                })}
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td style={{ fontWeight: 600, position: 'sticky', left: 0, zIndex: 10, background: 'var(--card-bg)', borderRight: '1px solid var(--border-color)' }}>
                                    {type === 'billing' ? 'Billings (EUR)' : 'Expenses (EUR)'}
                                </td>
                                {financials.map(f => (
                                    <td key={f.month} style={{ padding: '0.75rem' }}>
                                        <PremiumNumericInput
                                            value={values[f.month] || 0}
                                            onChange={(val) => setValues({ ...values, [f.month]: val })}
                                            className="financial-input-horizontal"
                                            hideControls={true}
                                        />
                                    </td>
                                ))}
                            </tr>
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default FinancialInputPage;

