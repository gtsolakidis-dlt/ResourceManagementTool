import React, { useEffect, useState } from 'react';
import { Plus, Edit, DollarSign, TrendingUp, Users, Layers, Loader2, Settings } from 'lucide-react';
import type { GlobalRate } from '../../types';
import { globalRateService } from '../../api/services';
import { getSeniorityFullName, getSeniorityBadgeClass } from '../../constants/seniorityLevels';
import { useNotification } from '../../context/NotificationContext';
import GlobalRateModal from './GlobalRateModal';
import { useNavigation } from '../../context/NavigationContext';
import './GlobalRatesPage.css';

const GlobalRatesPage: React.FC = () => {
    const { setBreadcrumbs } = useNavigation();
    const { notify } = useNotification();
    const [rates, setRates] = useState<GlobalRate[]>([]);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedRate, setSelectedRate] = useState<GlobalRate | null>(null);

    // Set breadcrumbs on mount
    useEffect(() => {
        setBreadcrumbs([
            { label: 'Resource Platform', path: '/' },
            { label: 'Settings', path: '/admin' },
            { label: 'Nominal Rates', disabled: true }
        ]);
    }, [setBreadcrumbs]);

    const fetchRates = async () => {
        try {
            setLoading(true);
            const response = await globalRateService.getAll();
            setRates(response.data);
        } catch (error) {
            console.error('Error fetching global rates:', error);
            notify.error('Failed to load global rates');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRates();
    }, []);

    const handleCreate = () => {
        setSelectedRate(null);
        setIsModalOpen(true);
    };

    const handleEdit = (rate: GlobalRate) => {
        setSelectedRate(rate);
        setIsModalOpen(true);
    };

    const handleSave = async () => {
        await fetchRates();
        setIsModalOpen(false);
    };

    // Calculate stats
    const avgRate = rates.length > 0
        ? rates.reduce((sum, r) => sum + r.nominalRate, 0) / rates.length
        : 0;
    const maxRate = rates.length > 0
        ? Math.max(...rates.map(r => r.nominalRate))
        : 0;
    const minRate = rates.length > 0
        ? Math.min(...rates.map(r => r.nominalRate))
        : 0;

    return (
        <div className="globalrates-premium-container">
            {/* Header */}
            <header className="globalrates-header">
                <div className="globalrates-header-content">
                    <h1>
                        <Settings className="header-icon" size={28} />
                        Nominal Rates
                    </h1>
                    <p>Configure standard daily billing rates by seniority level for project WIP calculations.</p>
                </div>
                <button onClick={handleCreate} className="btn-add-rate">
                    <Plus size={18} />
                    Add Rate
                </button>
            </header>

            {/* Stats Cards */}
            {rates.length > 0 && (
                <div className="stats-cards-row">
                    <div className="stat-card">
                        <div className="stat-icon">
                            <Layers size={24} />
                        </div>
                        <div className="stat-content">
                            <div className="stat-value">{rates.length}</div>
                            <div className="stat-label">Total Levels</div>
                        </div>
                    </div>
                    <div className="stat-card">
                        <div className="stat-icon">
                            <TrendingUp size={24} />
                        </div>
                        <div className="stat-content">
                            <div className="stat-value">€{avgRate.toLocaleString('de-DE', { maximumFractionDigits: 0 })}</div>
                            <div className="stat-label">Average Rate</div>
                        </div>
                    </div>
                    <div className="stat-card">
                        <div className="stat-icon">
                            <DollarSign size={24} />
                        </div>
                        <div className="stat-content">
                            <div className="stat-value">€{minRate.toLocaleString('de-DE', { maximumFractionDigits: 0 })} - €{maxRate.toLocaleString('de-DE', { maximumFractionDigits: 0 })}</div>
                            <div className="stat-label">Rate Range</div>
                        </div>
                    </div>
                </div>
            )}

            {/* Table Container */}
            <div className="rates-table-container">
                <div className="rates-table-header">
                    <h3>
                        <Users size={18} />
                        Rate Configuration
                    </h3>
                </div>

                {loading ? (
                    <div className="loading-state">
                        <Loader2 size={32} className="loading-spinner" />
                        <span>Loading rates...</span>
                    </div>
                ) : rates.length === 0 ? (
                    <div className="empty-state">
                        <div className="empty-state-icon">
                            <DollarSign size={36} />
                        </div>
                        <h3>No Rates Configured</h3>
                        <p>Get started by adding your first nominal rate for a seniority level.</p>
                        <button onClick={handleCreate} className="btn-add-rate">
                            <Plus size={18} />
                            Add First Rate
                        </button>
                    </div>
                ) : (
                    <table className="rates-premium-table">
                        <thead>
                            <tr>
                                <th>Seniority Level</th>
                                <th>Daily Nominal Rate</th>
                                <th>Last Updated</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {[...rates].sort((a, b) => a.nominalRate - b.nominalRate).map((rate) => (
                                <tr key={rate.id}>
                                    <td>
                                        <div className="level-cell">
                                            <span className={`level-badge ${getSeniorityBadgeClass(rate.level)}`}>
                                                {rate.level}
                                            </span>
                                            <span className="level-full-name">
                                                {getSeniorityFullName(rate.level)}
                                            </span>
                                        </div>
                                    </td>
                                    <td>
                                        <div className="rate-value">
                                            <span className="rate-currency">€</span>
                                            {rate.nominalRate.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                            <span className="rate-per-day">/day</span>
                                        </div>
                                    </td>
                                    <td>
                                        <span className="updated-date">
                                            {new Date(rate.updatedAt).toLocaleDateString('en-GB', {
                                                day: '2-digit',
                                                month: 'short',
                                                year: 'numeric'
                                            })}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="action-buttons">
                                            <button
                                                onClick={() => handleEdit(rate)}
                                                className="btn-action"
                                                title="Edit Rate"
                                            >
                                                <Edit size={16} />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            {isModalOpen && (
                <GlobalRateModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    onSave={handleSave}
                    initialData={selectedRate}
                />
            )}
        </div>
    );
};

export default GlobalRatesPage;
