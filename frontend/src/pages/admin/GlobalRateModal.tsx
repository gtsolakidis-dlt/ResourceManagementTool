import React, { useState, useEffect } from 'react';
import { X, DollarSign, Tag } from 'lucide-react';
import type { GlobalRate } from '../../types';
import { globalRateService } from '../../api/services';
import { SENIORITY_LEVELS } from '../../constants/seniorityLevels';
import PremiumSelect from '../../components/common/PremiumSelect';
import { useNotification } from '../../context/NotificationContext';
import './GlobalRateModal.css';

interface GlobalRateModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: () => void;
    initialData: GlobalRate | null;
}

const GlobalRateModal: React.FC<GlobalRateModalProps> = ({ isOpen, onClose, onSave, initialData }) => {
    const { notify } = useNotification();
    const [level, setLevel] = useState('');
    const [nominalRate, setNominalRate] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (initialData) {
            setLevel(initialData.level);
            setNominalRate(initialData.nominalRate.toString());
        } else {
            setLevel('');
            setNominalRate('');
        }
    }, [initialData]);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!level || !nominalRate) {
            notify.error('Please fill in all fields');
            return;
        }

        try {
            setLoading(true);
            const rateValue = parseFloat(nominalRate.replace(',', '.')); // Handle decimal comma if entered

            if (isNaN(rateValue)) {
                notify.error('Invalid rate value');
                return;
            }

            const data = {
                level,
                nominalRate: rateValue
            };

            if (initialData) {
                await globalRateService.update(initialData.id, { ...data, id: initialData.id });
                notify.success('Rate updated successfully');
            } else {
                await globalRateService.create(data);
                notify.success('Rate created successfully');
            }
            onSave();
        } catch (error: any) {
            console.error('Error saving rate:', error);
            const msg = error.response?.data?.detail || 'Failed to save rate';
            notify.error(msg);
        } finally {
            setLoading(false);
        }
    };

    // Handle backdrop click
    const handleBackdropClick = (e: React.MouseEvent) => {
        if (e.target === e.currentTarget) {
            onClose();
        }
    };

    return (
        <div className="rate-modal-overlay" onClick={handleBackdropClick}>
            <div className="rate-modal-container">
                {/* Header */}
                <div className="rate-modal-header">
                    <div className="rate-modal-header-content">
                        <div className="rate-modal-icon">
                            <DollarSign size={20} />
                        </div>
                        <h2>{initialData ? 'Edit Rate' : 'New Rate'}</h2>
                    </div>
                    <button onClick={onClose} className="rate-modal-close">
                        <X size={20} />
                    </button>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="rate-modal-form">
                    <div className="rate-form-group">
                        <label>
                            <Tag size={14} />
                            Seniority Level
                        </label>
                        <PremiumSelect
                            value={level}
                            onChange={setLevel}
                            placeholder="Select a level..."
                            options={[
                                ...SENIORITY_LEVELS.map(l => ({ value: l.code, label: l.displayName }))
                            ]}
                        />
                        <span className="form-hint">Select the seniority level for this rate</span>
                    </div>

                    <div className="rate-form-group">
                        <label>
                            <DollarSign size={14} />
                            Daily Nominal Rate (€)
                        </label>
                        <div className="rate-input-wrapper">
                            <span className="rate-prefix">€</span>
                            <input
                                type="number"
                                step="0.01"
                                min="0"
                                value={nominalRate}
                                onChange={(e) => setNominalRate(e.target.value)}
                                placeholder="0.00"
                                required
                            />
                            <span className="rate-suffix">/day</span>
                        </div>
                        <span className="form-hint">Standard billing rate per day for this level</span>
                    </div>

                    {/* Actions */}
                    <div className="rate-modal-actions">
                        <button type="button" onClick={onClose} className="btn-outline-premium">
                            Cancel
                        </button>
                        <button type="submit" disabled={loading} className="btn-premium">
                            {loading ? 'Saving...' : initialData ? 'Update Rate' : 'Create Rate'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default GlobalRateModal;
