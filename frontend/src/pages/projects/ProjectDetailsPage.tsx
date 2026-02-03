import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { projectService, forecastService } from '../../api/services';
import type { Project } from '../../types';
import { DollarSign, Calculator, Loader2, CreditCard, Receipt, Target, ArrowUpRight } from 'lucide-react';

import { useNavigation } from '../../context/NavigationContext';

import { useNotification } from '../../context/NotificationContext';
import ProjectOverviewWithSnapshots from './ProjectOverviewWithSnapshots';

const ProjectDetailsPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const projectId = parseInt(id || '0');
    const navigate = useNavigate();
    const { setBreadcrumbs, setActiveSection, setSidebarSubItems } = useNavigation();
    const { notify } = useNotification();

    const [project, setProject] = useState<Project | null>(null);
    const [forecastVersionId, setForecastVersionId] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [editProject, setEditProject] = useState<Partial<Project>>({});

    useEffect(() => {
        if (projectId) loadProjectData();
    }, [projectId]);

    useEffect(() => {
        if (project) {
            setActiveSection('projects');
            setBreadcrumbs([
                { label: 'Resource Platform', path: '/' },
                { label: 'Projects', path: '/projects' },
                { label: project.name, disabled: true },
                { label: 'Overview' }
            ]);
            setSidebarSubItems([
                { label: project.name, path: `/projects/${projectId}`, exact: false, isHeader: true }, // Header-like item
                { label: 'Overview', path: `/projects/${projectId}` },
                { label: 'Billings', path: `/projects/${projectId}/billings` },
                { label: 'Expenses', path: `/projects/${projectId}/expenses` },
                { label: 'Forecast', path: `/projects/${projectId}/forecast` }
            ]);
        }

        // Cleanup on unmount to reset sidebar? 
        // Ideally yes, but Layout might persist or other pages overwrite.
        // For simple flow, next page will overwrite. 
        return () => {
            // Optional: setActiveSection('') or similar if leaving projects entirely
        };
    }, [project]);

    const loadProjectData = async () => {
        setLoading(true);
        try {
            // Load Project Core Data
            const projRes = await projectService.getProject(projectId);
            setProject(projRes.data);

            // Fetch Versions to get the latest version ID for snapshots
            try {
                const versionRes = await forecastService.getVersions(projectId);
                if (versionRes.data.length > 0) {
                    setForecastVersionId(versionRes.data[0].id);
                } else {
                    console.warn('No forecast versions found.');
                    setForecastVersionId(null);
                }
            } catch (versionError) {
                console.warn('Forecast versions unavailable:', versionError);
                setForecastVersionId(null);
            }
        } catch (error) {
            console.error('Project load error:', error);
            setProject(null);
        } finally {
            setLoading(false);
        }
    };

    const handleUpdateProject = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await projectService.updateProject(projectId, editProject);
            notify.success('Project updated successfully');
            setIsEditModalOpen(false);
            loadProjectData(); // Reload data
        } catch (error) {
            console.error('Update failed', error);
            notify.error('Failed to update project');
        }
    };

    if (loading) return <div className="loading-state" style={{ height: '80vh' }}><Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" /><span>Decoding financial streams...</span></div>;
    if (!project) return <div>Project not found.</div>;

    return (
        <div className="project-details-premium">
            <header className="page-header" style={{ marginBottom: '3rem' }}>
                <div>
                    <div style={{ color: 'var(--deloitte-green)', fontWeight: 700, fontSize: '0.75rem', letterSpacing: '0.1em', textTransform: 'uppercase', marginBottom: '0.5rem' }}>{project.wbs}</div>
                    <h1 style={{ marginBottom: '0.5rem' }}>{project.name}</h1>
                </div>
                <div style={{ display: 'flex', gap: '1rem' }}>
                    <button className="btn-outline-premium" onClick={() => navigate(`/projects/${projectId}/billings`)}>
                        <CreditCard size={18} />
                        Billings
                    </button>
                    <button className="btn-outline-premium" onClick={() => navigate(`/projects/${projectId}/expenses`)}>
                        <Receipt size={18} />
                        Expenses
                    </button>
                    {project.canEdit && (
                        <button className="btn-outline-premium" onClick={() => {
                            setEditProject({
                                ...project,
                                startDate: project.startDate.toString().split('T')[0],
                                endDate: project.endDate.toString().split('T')[0]
                            });
                            setIsEditModalOpen(true);
                        }}>
                            <ArrowUpRight size={18} style={{ transform: 'rotate(45deg)' }} />
                            Edit
                        </button>
                    )}
                    <button className="btn-premium" onClick={() => navigate(`/projects/${projectId}/forecast`)}>
                        <Calculator size={18} />
                        Resource Forecast
                    </button>
                </div>
            </header>

            {/* Edit Project Handler */}
            {(() => {
                // Inline handler definition or move to component body
                // Moving to component body is better, but doing inline check 
                // requires us to not use hooks inside conditional, which we aren't.
                // WE NEED TO DEFINE handleUpdateProject IN COMPONENT BODY
                return null;
            })()}

            <div className="metrics-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '2rem', marginBottom: '3rem' }}>
                <div className="glass-panel" style={{ padding: '2rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Total Actual Budget</span>
                        <DollarSign size={20} color="var(--deloitte-green)" />
                    </div>
                    <div style={{ fontSize: '2rem', fontWeight: 800 }}>€{project.actualBudget.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
                    <div style={{ marginTop: '1rem', fontSize: '0.75rem', color: 'var(--text-muted)' }}>Nominal: €{project.nominalBudget.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} (Disc: {project.discount}%)</div>
                </div>

                <div className="glass-panel" style={{ padding: '2rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Recoverability</span>
                        <Target size={20} color="var(--deloitte-green)" />
                    </div>
                    <div style={{ fontSize: '2rem', fontWeight: 800 }}>{(project.recoverability * 100).toFixed(0)}%</div>
                    <div style={{ marginTop: '1rem', fontSize: '0.75rem', color: 'var(--text-muted)' }}>Budget recovery rate</div>
                </div>

                <div className="glass-panel" style={{ padding: '2rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Target Margin</span>
                        <Target size={20} color="var(--deloitte-green)" />
                    </div>
                    <div style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--deloitte-green)' }}>
                        {(project.targetMargin * 100).toFixed(0)}%
                    </div>
                    <div style={{ marginTop: '1rem', fontSize: '0.75rem', color: 'var(--text-muted)' }}>Target Efficiency</div>
                </div>
            </div>

            {/* Monthly Snapshots with Overwrite/Confirm functionality */}
            {forecastVersionId ? (
                <ProjectOverviewWithSnapshots
                    projectId={projectId}
                    forecastVersionId={forecastVersionId}
                />
            ) : (
                <div className="glass-panel" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                    <p style={{ fontSize: '1.1rem', marginBottom: '0.5rem' }}>No forecast version available.</p>
                    <p style={{ fontSize: '0.875rem' }}>Start by adding resource allocations in the Forecast section.</p>
                </div>
            )}
            {/* Modal for Edit Project */}
            {isEditModalOpen && project && (
                <div className="modal-overlay" onClick={() => setIsEditModalOpen(false)}>
                    <div className="modal-content animate-scale-up" onClick={e => e.stopPropagation()}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                            <h2 style={{ margin: 0 }}>Edit Project</h2>
                            <button
                                onClick={() => setIsEditModalOpen(false)}
                                className="btn-close"
                            >
                                <ArrowUpRight size={20} style={{ transform: 'rotate(45deg)' }} />
                            </button>
                        </div>
                        <form onSubmit={handleUpdateProject}>
                            <div className="form-group">
                                <label className="form-label">Project Name</label>
                                <input className="form-input" required value={editProject.name} onChange={e => setEditProject({ ...editProject, name: e.target.value })} />
                            </div>
                            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                <div className="form-group">
                                    <label className="form-label">WBS Code</label>
                                    <input className="form-input" required value={editProject.wbs} onChange={e => setEditProject({ ...editProject, wbs: e.target.value })} />
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Actual Budget (€)</label>
                                    <input type="number" className="form-input" required value={editProject.actualBudget} onChange={e => setEditProject({ ...editProject, actualBudget: Number(e.target.value) })} />
                                </div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">Discount (%)</label>
                                <input type="number" step="0.01" className="form-input" value={editProject.discount} onChange={e => setEditProject({ ...editProject, discount: Number(e.target.value) })} />
                            </div>
                            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                <div className="form-group">
                                    <label className="form-label">Start Date</label>
                                    <input type="date" className="form-input" required value={editProject.startDate?.toString().split('T')[0]} onChange={e => setEditProject({ ...editProject, startDate: e.target.value })} />
                                </div>
                                <div className="form-group">
                                    <label className="form-label">End Date</label>
                                    <input type="date" className="form-input" required value={editProject.endDate?.toString().split('T')[0]} onChange={e => setEditProject({ ...editProject, endDate: e.target.value })} />
                                </div>
                            </div>
                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setIsEditModalOpen(false)}>Cancel</button>
                                <button type="submit" className="btn-premium">Save Changes</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ProjectDetailsPage;
