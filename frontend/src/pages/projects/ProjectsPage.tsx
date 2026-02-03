import React, { useEffect, useState } from 'react';
import { projectService } from '../../api/services';
import { useAuth } from '../../context/AuthContext';
import type { Project } from '../../types';
import { Plus, Loader2, Briefcase, Calendar, ChevronRight, Download, X } from 'lucide-react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useNotification } from '../../context/NotificationContext';
import { downloadFile } from '../../utils/downloadFile';
import './ProjectsPage.css';
import { useNavigation } from '../../context/NavigationContext';

const ProjectsPage: React.FC = () => {
    const { setBreadcrumbs } = useNavigation();
    const { notify } = useNotification();
    const [projects, setProjects] = useState<Project[]>([]);
    const [loading, setLoading] = useState(true);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [newProject, setNewProject] = useState<Partial<Project>>({
        name: '',
        wbs: '',
        actualBudget: 0,
        nominalBudget: 0,
        discount: 0,
        targetMargin: 0.20,
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString().split('T')[0]
    });
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const { user } = useAuth();
    const canManage = user?.role === 'Admin' || user?.role === 'Partner';

    useEffect(() => {
        setBreadcrumbs([
            { label: 'Resource Platform', path: '/' },
            { label: 'Projects', disabled: true }
        ]);
        loadProjects();
        if (searchParams.get('action') === 'create') {
            setIsCreateModalOpen(true);
            searchParams.delete('action');
            setSearchParams(searchParams, { replace: true });
        }
    }, []);

    const loadProjects = async () => {
        setLoading(true);
        try {
            const res = await projectService.getProjects();
            setProjects(res.data);
        } catch (error) {
            console.error('Failed to load projects:', error);
            notify.error('Failed to load projects');
        } finally {
            setLoading(false);
        }
    };

    const handleCreateProject = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await projectService.createProject(newProject);
            notify.success('Project initiated successfully');
            setIsCreateModalOpen(false);
            loadProjects();
        } catch (error) {
            console.error('Create failed', error);
            notify.error('Failed to initiate project');
        }
    };

    const handleExport = async () => {
        try {
            const res = await projectService.exportProjects();

            // Extract filename from Content-Disposition header
            const contentDisposition = res.headers['content-disposition'];
            let filename = 'Projects_Export.xlsx';
            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            // Create blob
            const blob = new Blob([res.data], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });

            // Use the robust download utility
            downloadFile(blob, filename);

            notify.success('Portfolio exported successfully');
        } catch (error) {
            console.error('Export failed:', error);
            notify.error('Failed to export portfolio');
        }
    };

    return (
        <div className="projects-premium-container">
            <header className="page-header" style={{ marginBottom: '3rem' }}>
                <div>
                    <h1 style={{ marginBottom: '0.5rem' }}>Portfolio Overview</h1>
                    <p style={{ color: 'var(--text-muted)' }}>Monitor project performance, budgets, and strategic alignment.</p>
                </div>
                {
                    canManage && (
                        <div style={{ display: 'flex', gap: '1rem' }}>
                            <input
                                type="file"
                                id="projects-import"
                                style={{ display: 'none' }}
                                onChange={async (e) => {
                                    const file = e.target.files?.[0];
                                    if (file) {
                                        const formData = new FormData();
                                        formData.append('file', file);
                                        setLoading(true);
                                        try {
                                            const res = await projectService.importProjects(formData);
                                            notify.success(`Successfully processed ${res.data.count} projects`);
                                            loadProjects();
                                        } catch (err) {
                                            console.error('Import failed', err);
                                            notify.error('Import failed. Please verify the Excel format.');
                                        } finally {
                                            setLoading(false);
                                        }
                                    }
                                }}
                            />
                            <button className="btn-outline-premium" onClick={() => document.getElementById('projects-import')?.click()}>
                                <Plus size={18} />
                                Import Portfolio
                            </button>
                            <button className="btn-outline-premium" onClick={handleExport}>
                                <Download size={18} />
                                Export Portfolio
                            </button>
                            <button className="btn-premium" onClick={() => setIsCreateModalOpen(true)}>
                                <Plus size={18} />
                                Initiate Project
                            </button>
                        </div>
                    )
                }
            </header >

            {
                loading ? (
                    <div className="loading-state" style={{ height: '400px' }} >
                        <Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" />
                        <span>Synchronizing portfolio data...</span>
                    </div >
                ) : (
                    <div className="projects-grid animate-fade-in">
                        {projects.map((project: Project) => (
                            <div key={project.id} className="project-card-premium" onClick={() => navigate(`/projects/${project.id}`)}>
                                <div style={{ position: 'absolute', top: 0, right: 0, padding: '1rem', color: 'var(--deloitte-green)', opacity: 0.2 }}>
                                    <Briefcase size={80} />
                                </div>

                                <div style={{ position: 'relative', zIndex: 1 }}>
                                    <div className="project-wbs">{project.wbs}</div>
                                    <h2 className="project-title">{project.name}</h2>

                                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '2rem' }}>
                                        <div>
                                            <div className="card-metric-label">Actual Budget</div>
                                            <div className="card-metric-value">€{project.actualBudget.toLocaleString()}</div>
                                        </div>
                                        <div>
                                            <div className="card-metric-label">Target Margin</div>
                                            <div className="card-metric-value" style={{ color: 'var(--deloitte-green)' }}>{(project.targetMargin * 100).toFixed(0)}%</div>
                                        </div>
                                    </div>

                                    <div className="card-footer">
                                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                            <Calendar size={16} />
                                            <span>{new Date(project.startDate).toLocaleDateString()}</span>
                                        </div>
                                        <ChevronRight size={16} style={{ color: 'var(--text-muted)' }} />
                                        <span>{new Date(project.endDate).toLocaleDateString()}</span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

            {
                isCreateModalOpen && (
                    <div className="modal-overlay" onClick={() => setIsCreateModalOpen(false)}>
                        <div className="modal-content animate-scale-up" onClick={e => e.stopPropagation()}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                                <h2 style={{ margin: 0 }}>Initiate New Project</h2>
                                <button
                                    onClick={() => setIsCreateModalOpen(false)}
                                    className="btn-close"
                                >
                                    <X size={20} />
                                </button>
                            </div>
                            <form onSubmit={handleCreateProject}>
                                <div className="form-group">
                                    <label className="form-label">Project Name</label>
                                    <input className="form-input" required value={newProject.name} onChange={e => setNewProject({ ...newProject, name: e.target.value })} placeholder="e.g. Cloud Migration Phase 1" />
                                </div>
                                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                    <div className="form-group">
                                        <label className="form-label">WBS Code</label>
                                        <input className="form-input" required value={newProject.wbs} onChange={e => setNewProject({ ...newProject, wbs: e.target.value })} placeholder="WBS.001" />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">Actual Budget (€)</label>
                                        <input type="number" className="form-input" required value={newProject.actualBudget} onChange={e => setNewProject({ ...newProject, actualBudget: Number(e.target.value) })} />
                                    </div>
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Discount (%)</label>
                                    <input type="number" step="0.01" className="form-input" value={newProject.discount} onChange={e => setNewProject({ ...newProject, discount: Number(e.target.value) })} />
                                </div>
                                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                    <div className="form-group">
                                        <label className="form-label">Start Date</label>
                                        <input type="date" className="form-input" required value={newProject.startDate?.toString().split('T')[0]} onChange={e => setNewProject({ ...newProject, startDate: e.target.value })} />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">End Date</label>
                                        <input type="date" className="form-input" required value={newProject.endDate?.toString().split('T')[0]} onChange={e => setNewProject({ ...newProject, endDate: e.target.value })} />
                                    </div>
                                </div>
                                <div className="modal-actions">
                                    <button type="button" className="btn-secondary" onClick={() => setIsCreateModalOpen(false)}>Cancel</button>
                                    <button type="submit" className="btn-premium">Create Project</button>
                                </div>
                            </form>
                        </div>
                    </div>
                )
            }
        </div >
    );
};

export default ProjectsPage;
