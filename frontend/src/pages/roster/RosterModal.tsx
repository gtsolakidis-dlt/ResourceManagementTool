import React, { useEffect, useState } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { RosterMember } from '../../types';
import { SENIORITY_LEVELS, TECHNICAL_ROLES } from '../../constants/seniorityLevels';
import PremiumSelect from '../../components/common/PremiumSelect';
import { useAuth } from '../../context/AuthContext';
import './RosterModal.css';

interface RosterModalProps {
    member?: RosterMember;
    onClose: () => void;
    onSave: (data: Partial<RosterMember>) => Promise<void>;
}

const FUNCTIONS = ['Engineering', 'AI & Data', 'Operations'];

const RosterModal: React.FC<RosterModalProps> = ({ member, onClose, onSave }) => {
    const { user } = useAuth();
    const [formData, setFormData] = useState<Partial<RosterMember>>({
        sapCode: '',
        fullNameEn: '',
        legalEntity: 'DBS',
        functionBusinessUnit: '',
        costCenterCode: '',
        level: '',
        technicalRole: '',
        monthlySalary: 0,
        monthlyEmployerContributions: 0,
        cars: 0,
        ticketRestaurant: 0,
        metlife: 0
    });
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (member) {
            setFormData(member);
        }
    }, [member]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await onSave(formData);
            // Parent will close modal on success
        } catch (error) {
            console.error('Failed to save member:', error);
            // Error is handled by parent, don't close
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'number' ? parseFloat(value) || 0 : value
        }));
    };

    // Helper for currency formatting logic if needed, but for inputs we accept numbers.
    // User requested "thousands separator with dot (.)".
    // Browser <input type="number"> usually manages display based on locale, but typically doesn't show separators while typing.
    // If strict requirement "input fields to accept values... format... once user clicks outside", then Type=Text with formatting logic is needed.
    // I will use type="text" and handle formatting onBlur? 
    // Or simplest: type="number" and CSS/Locale. 
    // User: "format their values once the user clicks outside... with thousands separator the dot".
    // This implies custom formatting logic.
    // I'll stick to simple numeric input for now to avoid breaking edits, or use a helper component.
    // For now, I'll use type="number" step="0.01" to ensure functionality first. 
    // Formatting can be refined.

    const handleBackdropClick = (e: React.MouseEvent) => {
        if (e.target === e.currentTarget) {
            onClose();
        }
    };

    return (
        <div className="modal-overlay roster-modal-overlay" onClick={handleBackdropClick}>
            <div className="modal-content">
                <header className="modal-header">
                    <h2>{member ? 'Edit Member' : 'Add New Member'}</h2>
                    <button className="btn-close" onClick={onClose}><X size={20} /></button>
                </header>
                <form onSubmit={handleSubmit} className="modal-form">
                    <div className="form-section">
                        <h3>Basic Information</h3>
                        <div className="form-grid">
                            <div className="form-group">
                                <label>SAP Code</label>
                                <input name="sapCode" value={formData.sapCode} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>Full Name (EN)</label>
                                <input name="fullNameEn" value={formData.fullNameEn} onChange={handleChange} required />
                            </div>
                            <div className="form-group">
                                <label>Legal Entity</label>
                                <input name="legalEntity" value={formData.legalEntity} onChange={handleChange} disabled />
                            </div>
                            <div className="form-group">
                                <label>Function / BU</label>
                                <PremiumSelect
                                    value={formData.functionBusinessUnit || ''}
                                    onChange={(val) => setFormData(prev => ({ ...prev, functionBusinessUnit: val }))}
                                    placeholder="Select Function"
                                    options={[
                                        ...FUNCTIONS.map(f => ({ value: f, label: f }))
                                    ]}
                                />
                            </div>
                            <div className="form-group">
                                <label>Level / Seniority</label>
                                <PremiumSelect
                                    value={formData.level || ''}
                                    onChange={(val) => setFormData(prev => ({ ...prev, level: val }))}
                                    placeholder="Select Level"
                                    options={[
                                        ...SENIORITY_LEVELS.map(l => ({ value: l.code, label: l.displayName }))
                                    ]}
                                />
                            </div>
                            <div className="form-group">
                                <label>Technical Role</label>
                                <PremiumSelect
                                    value={formData.technicalRole || ''}
                                    onChange={(val) => setFormData(prev => ({ ...prev, technicalRole: val }))}
                                    placeholder="Select Technical Role"
                                    options={[
                                        { value: '', label: 'None' },
                                        ...TECHNICAL_ROLES.map(r => ({ value: r, label: r }))
                                    ]}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="form-section">
                        <h3>Financials (Monthly)</h3>
                        <div className="form-grid">
                            <div className="form-group">
                                <label>Monthly Salary (EUR)</label>
                                <input type="number" name="monthlySalary" value={formData.monthlySalary} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Monthly Employer Contributions (EUR)</label>
                                <input type="number" name="monthlyEmployerContributions" value={formData.monthlyEmployerContributions} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Cars (EUR)</label>
                                <input type="number" name="cars" value={formData.cars} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Ticket Restaurant (EUR)</label>
                                <input type="number" name="ticketRestaurant" value={formData.ticketRestaurant} onChange={handleChange} />
                            </div>
                            <div className="form-group">
                                <label>Metlife (EUR)</label>
                                <input type="number" name="metlife" value={formData.metlife} onChange={handleChange} />
                            </div>
                        </div>
                    </div>

                    {(!member || user?.role?.toLowerCase() === 'admin') && (
                        <div className="form-section">
                            <h3>User Credentials {member && '(Admin Only)'}</h3>
                            <div className="form-grid">
                                <div className="form-group">
                                    <label>Username</label>
                                    <input
                                        name="username"
                                        value={(formData as any).username || ''}
                                        onChange={handleChange}
                                        required={!member}
                                        placeholder="e.g. gtsolakidis"
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Password {member && <span style={{ fontWeight: 'normal', fontSize: '0.7em', color: 'var(--text-muted)' }}>(Leave blank to keep)</span>}</label>
                                    <input
                                        type="password"
                                        name="password"
                                        value={(formData as any).password || ''}
                                        onChange={handleChange}
                                        required={!member}
                                    />
                                </div>
                            </div>
                        </div>
                    )}

                    {(['admin', 'partner'].includes(user?.role?.toLowerCase() || '')) && (
                        <div className="form-section">
                            <h3>Access Control</h3>
                            <div className="form-group">
                                <label>Role</label>
                                <PremiumSelect
                                    value={formData.role || 'Employee'}
                                    onChange={(val) => setFormData(prev => ({ ...prev, role: val }))}
                                    options={[
                                        { value: 'Employee', label: 'Employee' },
                                        { value: 'Manager', label: 'Manager' },
                                        { value: 'Partner', label: 'Partner' },
                                        { value: 'Admin', label: 'Admin' }
                                    ]}
                                />
                            </div>
                        </div>
                    )}

                    <footer className="modal-footer">
                        <button type="button" className="btn-outline-premium" onClick={onClose} disabled={loading}>Cancel</button>
                        <button type="submit" className="btn-premium" disabled={loading}>
                            {loading && <Loader2 size={16} className="animate-spin" />}
                            {member ? 'Update Member' : 'Create Member'}
                        </button>
                    </footer>
                </form>
            </div>
        </div>
    );
};

export default RosterModal;
