import React, { useEffect, useState, useMemo } from 'react';
import { Search, UserPlus, Users, Filter, AlertTriangle } from 'lucide-react';
import Drawer from '../../components/common/Drawer';
import PremiumSelect from '../../components/common/PremiumSelect';
import { suggestionService } from '../../api/services';
import { getSeniorityBadgeClass } from '../../constants/seniorityLevels';
import { SENIORITY_LEVELS, TECHNICAL_ROLES, SENIORITY_TIERS } from '../../constants/seniorityLevels';
import type { ResourceSuggestion } from '../../types';
import './ResourceSuggestionDrawer.css';

interface ResourceSuggestionDrawerProps {
    isOpen: boolean;
    onClose: () => void;
    onAssign: (rosterId: number) => void;
    projectId: number;
    forecastVersionId: number | null;
    assignedRosterIds: number[];
}

type SortOption = 'availability' | 'cost' | 'budget';

const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('de-DE', {
        style: 'currency',
        currency: 'EUR',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
    }).format(value);
};

const formatCurrencyCompact = (value: number): string => {
    if (Math.abs(value) >= 1000) {
        return new Intl.NumberFormat('de-DE', {
            style: 'currency',
            currency: 'EUR',
            minimumFractionDigits: 0,
            maximumFractionDigits: 1,
            notation: 'compact',
        }).format(value);
    }
    return formatCurrency(value);
};

const getAvailabilityColor = (pct: number): string => {
    if (pct > 60) return 'green';
    if (pct > 20) return 'yellow';
    return 'red';
};

const ResourceSuggestionDrawer: React.FC<ResourceSuggestionDrawerProps> = ({
    isOpen,
    onClose,
    onAssign,
    projectId,
    forecastVersionId,
    assignedRosterIds,
}) => {
    const [suggestions, setSuggestions] = useState<ResourceSuggestion[]>([]);
    const [loading, setLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterLevel, setFilterLevel] = useState('');
    const [filterTechnicalRole, setFilterTechnicalRole] = useState('');
    const [filterSeniorityTier, setFilterSeniorityTier] = useState('');
    const [availableOnly, setAvailableOnly] = useState(false);
    const [sortBy, setSortBy] = useState<SortOption>('availability');

    useEffect(() => {
        if (isOpen && forecastVersionId) {
            loadSuggestions();
        }
        if (!isOpen) {
            setSearchTerm('');
        }
    }, [isOpen, forecastVersionId]);

    const loadSuggestions = async () => {
        if (!forecastVersionId) return;
        setLoading(true);
        try {
            const res = await suggestionService.getResourceSuggestions(projectId, forecastVersionId);
            setSuggestions(res.data);
        } catch (err) {
            console.error('Failed to load suggestions', err);
            setSuggestions([]);
        } finally {
            setLoading(false);
        }
    };

    const filtered = useMemo(() => {
        let result = suggestions.filter(s => !assignedRosterIds.includes(s.rosterId));

        if (searchTerm) {
            const term = searchTerm.toLowerCase();
            result = result.filter(s =>
                s.fullNameEn.toLowerCase().includes(term) ||
                (s.functionBusinessUnit?.toLowerCase().includes(term)) ||
                (s.technicalRole?.toLowerCase().includes(term))
            );
        }

        if (filterLevel) {
            result = result.filter(s => s.level === filterLevel);
        }

        if (filterTechnicalRole) {
            result = result.filter(s => s.technicalRole === filterTechnicalRole);
        }

        if (filterSeniorityTier) {
            result = result.filter(s => s.seniorityTier === filterSeniorityTier);
        }

        if (availableOnly) {
            result = result.filter(s => s.availabilityPercentage > 0);
        }

        switch (sortBy) {
            case 'availability':
                result.sort((a, b) => b.availabilityPercentage - a.availabilityPercentage || a.dailyCost - b.dailyCost);
                break;
            case 'cost':
                result.sort((a, b) => a.dailyCost - b.dailyCost);
                break;
            case 'budget':
                result.sort((a, b) => {
                    const order = { within: 0, tight: 1, over: 2 };
                    return (order[a.budgetFit] ?? 2) - (order[b.budgetFit] ?? 2) || a.dailyCost - b.dailyCost;
                });
                break;
        }

        return result;
    }, [suggestions, assignedRosterIds, searchTerm, filterLevel, filterTechnicalRole, filterSeniorityTier, availableOnly, sortBy]);

    const levelOptions = [
        { value: '', label: 'All Levels' },
        ...SENIORITY_LEVELS.map(l => ({ value: l.code, label: l.displayName })),
    ];

    const technicalRoleOptions = [
        { value: '', label: 'All Roles' },
        ...TECHNICAL_ROLES.map(r => ({ value: r, label: r })),
    ];

    const seniorityTierOptions = [
        { value: '', label: 'All Tiers' },
        ...SENIORITY_TIERS.map(t => ({ value: t, label: t })),
    ];

    const sortOptions = [
        { value: 'availability', label: 'Best Availability' },
        { value: 'cost', label: 'Lowest Cost' },
        { value: 'budget', label: 'Best Budget Fit' },
    ];

    const handleAssign = (rosterId: number) => {
        onAssign(rosterId);
    };

    return (
        <Drawer isOpen={isOpen} onClose={onClose} title="Assign Resource" width="520px">
            {/* Search */}
            <div className="suggestion-search-bar">
                <Search size={15} className="suggestion-search-icon" />
                <input
                    type="text"
                    placeholder="Search by name or unit..."
                    value={searchTerm}
                    onChange={e => setSearchTerm(e.target.value)}
                    autoFocus
                />
            </div>

            {/* Filters */}
            <div className="suggestion-filters">
                <div className="suggestion-filter-select">
                    <PremiumSelect
                        value={filterLevel}
                        onChange={setFilterLevel}
                        options={levelOptions}
                        placeholder="Level"
                    />
                </div>
                <div className="suggestion-filter-select">
                    <PremiumSelect
                        value={filterTechnicalRole}
                        onChange={setFilterTechnicalRole}
                        options={technicalRoleOptions}
                        placeholder="Role"
                    />
                </div>
                <div className="suggestion-filter-select">
                    <PremiumSelect
                        value={filterSeniorityTier}
                        onChange={setFilterSeniorityTier}
                        options={seniorityTierOptions}
                        placeholder="Tier"
                    />
                </div>
                <div className="suggestion-filter-select">
                    <PremiumSelect
                        value={sortBy}
                        onChange={(val) => setSortBy(val as SortOption)}
                        options={sortOptions}
                    />
                </div>
                <button
                    className={`suggestion-toggle ${availableOnly ? 'active' : ''}`}
                    onClick={() => setAvailableOnly(!availableOnly)}
                >
                    <Filter size={12} />
                    Available
                </button>
            </div>

            {/* Results */}
            {loading ? (
                <div className="suggestion-skeleton">
                    {[1, 2, 3, 4].map(i => (
                        <div key={i} className="suggestion-skeleton-card">
                            <div className="suggestion-skeleton-line medium" />
                            <div className="suggestion-skeleton-line short" />
                            <div className="suggestion-skeleton-line long" />
                            <div className="suggestion-skeleton-line medium" />
                        </div>
                    ))}
                </div>
            ) : filtered.length === 0 ? (
                <div className="suggestion-empty">
                    <Users size={40} className="suggestion-empty-icon" />
                    <h3>No matching resources</h3>
                    <p>
                        {suggestions.length === 0
                            ? 'All resources have been assigned to this project.'
                            : 'Try adjusting your search or filters.'}
                    </p>
                </div>
            ) : (
                <>
                    <div className="suggestion-results-count">
                        {filtered.length} resource{filtered.length !== 1 ? 's' : ''} found
                    </div>

                    {filtered.map(suggestion => {
                        const availColor = getAvailabilityColor(suggestion.availabilityPercentage);

                        return (
                            <div key={suggestion.rosterId} className="suggestion-card">
                                {/* Header: Name + Level Badge */}
                                <div className="suggestion-card-header">
                                    <div>
                                        <h4 className="suggestion-card-name">{suggestion.fullNameEn}</h4>
                                        <div className="suggestion-card-meta">
                                            {suggestion.functionBusinessUnit || 'Unassigned Unit'}
                                        </div>
                                        {suggestion.technicalRole && (
                                            <span className="suggestion-card-role">{suggestion.technicalRole}</span>
                                        )}
                                    </div>
                                    <div className="suggestion-card-badges">
                                        <span className={`level-badge ${getSeniorityBadgeClass(suggestion.level || '')}`}>
                                            {suggestion.level || '—'}
                                        </span>
                                        {suggestion.seniorityTier && suggestion.seniorityTier !== 'Unknown' && (
                                            <span className={`seniority-tier-badge tier-${suggestion.seniorityTier.toLowerCase()}`}>
                                                {suggestion.seniorityTier}
                                            </span>
                                        )}
                                    </div>
                                </div>

                                {/* Monthly Availability Mini Chart */}
                                {suggestion.monthlyAvailability.length > 0 && (
                                    <div className="suggestion-monthly-chart">
                                        {suggestion.monthlyAvailability.map(m => {
                                            const pct = m.capacityDays > 0
                                                ? (m.availableDays / m.capacityDays) * 100
                                                : 0;
                                            const barColor = getAvailabilityColor(pct);
                                            const monthLabel = new Date(m.month).toLocaleDateString('en-US', {
                                                month: 'short',
                                                year: '2-digit',
                                            });
                                            return (
                                                <div
                                                    key={m.month}
                                                    className={`suggestion-monthly-bar ${barColor}`}
                                                    style={{ height: `${Math.max(pct, 4)}%` }}
                                                    title={`${monthLabel}: ${m.availableDays}/${m.capacityDays} days available`}
                                                />
                                            );
                                        })}
                                    </div>
                                )}

                                {/* Availability Bar */}
                                <div className="suggestion-availability-row">
                                    <span className="suggestion-availability-label">Availability</span>
                                    <div className="suggestion-availability-bar-track">
                                        <div
                                            className={`suggestion-availability-bar-fill ${availColor}`}
                                            style={{ width: `${Math.min(suggestion.availabilityPercentage, 100)}%` }}
                                        />
                                    </div>
                                    <span className={`suggestion-availability-pct ${availColor}`}>
                                        {suggestion.availabilityPercentage}%
                                    </span>
                                </div>

                                {/* Cost & Budget */}
                                <div className="suggestion-cost-row">
                                    <div className="suggestion-cost-item">
                                        <span className="suggestion-cost-label">Daily Cost</span>
                                        <span className="suggestion-cost-value">{formatCurrency(suggestion.dailyCost)}</span>
                                    </div>
                                    <div className="suggestion-cost-item">
                                        <span className="suggestion-cost-label">Projected</span>
                                        <span className="suggestion-cost-value">{formatCurrencyCompact(suggestion.projectedCost)}</span>
                                    </div>
                                    <div className="suggestion-cost-item" style={{ alignItems: 'flex-end' }}>
                                        <span className={`suggestion-budget-badge ${suggestion.budgetFit}`}>
                                            {suggestion.budgetFit === 'over' && <AlertTriangle size={10} />}
                                            {suggestion.budgetFit === 'within' && 'Within Budget'}
                                            {suggestion.budgetFit === 'tight' && 'Tight'}
                                            {suggestion.budgetFit === 'over' && 'Over Budget'}
                                        </span>
                                    </div>
                                </div>

                                {/* Assign Button */}
                                <div className="suggestion-card-footer">
                                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                                        {suggestion.totalAvailableDays} of {suggestion.totalCapacityDays} days available
                                    </span>
                                    <button
                                        className="suggestion-assign-btn"
                                        onClick={() => handleAssign(suggestion.rosterId)}
                                    >
                                        <UserPlus size={14} />
                                        Assign
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </>
            )}
        </Drawer>
    );
};

export default ResourceSuggestionDrawer;
