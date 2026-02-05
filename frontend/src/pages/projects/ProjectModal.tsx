import React, { useEffect, useState } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { Project } from '../../types';
import { useNotification } from '../../context/NotificationContext';
import './ProjectModal.css';

interface ProjectModalProps {
    project?: Project;
    onClose: () => void;
    onSave: (data: Partial<Project>) => Promise<void>;
}

const ProjectModal: React.FC<ProjectModalProps> = ({ project, onClose, onSave }) => {
    const { notify } = useNotification();
    const [formData, setFormData] = useState<Partial<Project>>({
        name: '',
        wbs: '',
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString().split('T')[0],
        actualBudget: 0,
        discount: 0,
        recoverability: 0.95,
        targetMargin: 25, // Percentage 0-100
    });
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (project) {
            setFormData({
                ...project,
                startDate: project.startDate.split('T')[0],
                endDate: project.endDate.split('T')[0],
            });
        }
    }, [project]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await onSave(formData);
            onClose();
        } catch (error) {
            console.error('Failed to save project:', error);
            notify.error('Error saving project. Please check if WBS is unique.');
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'number' ? parseFloat(value) || 0 : value
        }));
    };

    // Auto-calculate nominal to show user (optional but good UX)
    const nominalDisplay = formData.actualBudget && (formData.discount || 0) < 100
        ? formData.actualBudget / (1 - ((formData.discount || 0) / 100))
        : 0;

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <header className="modal-header">
                    <h2>{project ? 'Edit Project' : 'Create New Project'}</h2>
                    <button className="btn-close" onClick={onClose}><X size={20} /></button>
                </header>
                <form onSubmit={handleSubmit} className="modal-form">
                    <div className="form-section">
                        <h3>Project Details</h3>
                        <div className="form-grid">
                            <div className="form-group full-width">
                                <label>Project Name</label>
                                <input name="name" value={formData.name} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>WBS / Project Code</label>
                                <input name="wbs" value={formData.wbs} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>Actual Budget (EUR)</label>
                                <input type="number" name="actualBudget" value={formData.actualBudget} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Discount (%)</label>
                                <input type="number" name="discount" value={formData.discount} onChange={handleChange} min="0" max="100" />
                            </div>
                            <div className="form-group">
                                <label>Nominal Budget (Est.)</label>
                                <input disabled value={nominalDisplay.toFixed(2)} style={{ background: 'var(--bg-secondary)', opacity: 0.7 }} />
                            </div>
                        </div>
                    </div>

                    <div className="form-section">
                        <h3>Schedule & Targets</h3>
                        <div className="form-grid">
                            <div className="form-group">
                                <label>Start Date</label>
                                <input type="date" name="startDate" value={formData.startDate} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>End Date</label>
                                <input type="date" name="endDate" value={formData.endDate} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>Recoverability (%)</label>
                                <input type="number" step="0.01" name="recoverability" value={formData.recoverability} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Target Margin (%)</label>
                                <input type="number" step="0.01" name="targetMargin" value={formData.targetMargin} onChange={handleChange} />
                            </div>
                        </div>
                    </div>

                    <footer className="modal-footer">
                        <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>Cancel</button>
                        <button type="submit" className="btn btn-primary" disabled={loading}>
                            {loading && <Loader2 size={16} className="animate-spin" />}
                            {project ? 'Update Project' : 'Create Project'}
                        </button>
                    </footer>
                </form>
            </div>
        </div>
    );
};

export default ProjectModal;
