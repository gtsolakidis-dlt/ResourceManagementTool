import React, { useState } from 'react';
import { X, Save, Loader2 } from 'lucide-react';
import type { ProjectMonthlySnapshot } from '../../types';
import './OverwriteModal.css';

interface Props {
    snapshot: ProjectMonthlySnapshot;
    onClose: () => void;
    onSave: (data: Partial<ProjectMonthlySnapshot>) => Promise<void>;
}

const OverwriteModal: React.FC<Props> = ({ snapshot, onClose, onSave }) => {
    const [formData, setFormData] = useState({
        openingBalance: snapshot.openingBalance,
        wip: snapshot.wip,
        directExpenses: snapshot.directExpenses,
        operationalCost: snapshot.operationalCost,
        nsr: snapshot.nsr,
        margin: snapshot.margin * 100 // Convert to percentage for display
    });
    const [saving, setSaving] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true);
        try {
            await onSave({
                openingBalance: formData.openingBalance,
                wip: formData.wip,
                directExpenses: formData.directExpenses,
                operationalCost: formData.operationalCost,
                nsr: formData.nsr,
                margin: formData.margin / 100 // Convert back to decimal
            });
        } finally {
            setSaving(false);
        }
    };

    const handleChange = (field: string, value: string) => {
        setFormData(prev => ({
            ...prev,
            [field]: parseFloat(value) || 0
        }));
    };

    const handleBackdropClick = (e: React.MouseEvent) => {
        if (e.target === e.currentTarget) {
            onClose();
        }
    };

    const formatMonth = (dateStr: string) => {
        const [y, m] = dateStr.split('-').map(Number);
        return new Date(y, m - 1, 1).toLocaleDateString('en-US', {
            month: 'long',
            year: 'numeric'
        });
    };

    return (
        <div className="modal-overlay overwrite-modal-overlay" onClick={handleBackdropClick}>
            <div className="modal-content overwrite-modal-content">
                <header className="modal-header">
                    <div>
                        <h2>Overwrite Financial Values</h2>
                        <span className="modal-subtitle">{formatMonth(snapshot.month)}</span>
                    </div>
                    <button className="btn-close" onClick={onClose}><X size={20} /></button>
                </header>

                <form onSubmit={handleSubmit}>
                    <div className="form-section">
                        <p className="form-description">
                            Override the calculated values for this month. These values will be preserved
                            even when data changes trigger recalculations.
                        </p>

                        <div className="form-grid overwrite-grid">
                            <div className="form-group">
                                <label>Opening Balance (OB)</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={formData.openingBalance}
                                    onChange={(e) => handleChange('openingBalance', e.target.value)}
                                />
                            </div>

                            <div className="form-group">
                                <label>Work in Progress (WIP)</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={formData.wip}
                                    onChange={(e) => handleChange('wip', e.target.value)}
                                />
                            </div>

                            <div className="form-group">
                                <label>Direct Expenses (DE)</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={formData.directExpenses}
                                    onChange={(e) => handleChange('directExpenses', e.target.value)}
                                />
                            </div>

                            <div className="form-group">
                                <label>Operational Cost (OC)</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={formData.operationalCost}
                                    onChange={(e) => handleChange('operationalCost', e.target.value)}
                                />
                            </div>

                            <div className="form-group">
                                <label>Net Service Revenue (NSR)</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={formData.nsr}
                                    onChange={(e) => handleChange('nsr', e.target.value)}
                                />
                            </div>

                            <div className="form-group">
                                <label>Margin (%)</label>
                                <input
                                    type="number"
                                    step="0.1"
                                    value={formData.margin}
                                    onChange={(e) => handleChange('margin', e.target.value)}
                                />
                            </div>
                        </div>
                    </div>

                    <footer className="modal-footer">
                        <button
                            type="button"
                            className="btn-outline-premium"
                            onClick={onClose}
                            disabled={saving}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="btn-premium"
                            disabled={saving}
                        >
                            {saving ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
                            Save Overwrite
                        </button>
                    </footer>
                </form>
            </div>
        </div>
    );
};

export default OverwriteModal;
