import React, { useState } from 'react';
import type { Project, ProjectMonthlySnapshot } from '../../types';
import { TrendingUp, TrendingDown, ChevronDown, ChevronUp, Lock, Flag } from 'lucide-react';
import './ProjectFinancialSummary.css';

interface Props {
    project: Project;
    snapshots: ProjectMonthlySnapshot[];
}

const formatCurrency = (val: number) => {
    // Compact format for summary header
    if (val >= 1000000) return `€${(val / 1000000).toFixed(2)}M`;
    if (val >= 1000) return `€${(val / 1000).toFixed(1)}k`;
    return `€${val.toFixed(0)}`;
};

const formatCurrencyFull = (val: number) => {
    return val.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

const formatPercent = (val: number) => (val * 100).toFixed(1) + '%';

const ProjectFinancialSummary: React.FC<Props> = ({ project, snapshots }) => {
    const [showDetails, setShowDetails] = useState(false);

    // 1. Calculate Target Margin Ratio
    const targetMarginRatio = project.targetMargin > 1 ? project.targetMargin / 100 : project.targetMargin;

    // 2. Find CTD Snapshot (Latest Confirmed)
    // Sort by date ascending to be sure
    const sorted = [...snapshots].sort((a, b) => new Date(a.month).getTime() - new Date(b.month).getTime());

    // Find last confirmed
    const ctdSnapshot = [...sorted].reverse().find(s => s.status === 'Confirmed');

    // 3. Find EAC Snapshot (Last one)
    const eacSnapshot = sorted.length > 0 ? sorted[sorted.length - 1] : null;

    const renderVariance = (actualMargin: number) => {
        const diff = actualMargin - targetMarginRatio;
        const diffPercent = (diff * 100).toFixed(1);
        const isPositive = diff >= 0;
        const Icon = isPositive ? TrendingUp : TrendingDown;
        const className = Math.abs(diff) < 0.001 ? 'neutral' : (diff > 0 ? 'positive' : 'negative');

        return (
            <span className={`variance-badge ${className}`}>
                <Icon size={12} />
                {Math.abs(Number(diffPercent))}%
            </span>
        );
    };

    const renderDetailItem = (label: string, value: number) => (
        <div className="detail-item">
            <div className="label">{label}</div>
            <div className="value">{formatCurrencyFull(value)}</div>
        </div>
    );

    return (
        <div className="glass-panel financial-summary-strip animate-fade-in">
            {/* CTD Section */}
            <div className="summary-section">
                <div className="summary-header">
                    <Lock size={14} />
                    Contract To Date (CTD)
                    {ctdSnapshot && (
                        <span className="summary-header-badge">
                            {(() => {
                                const [y, m] = ctdSnapshot.month.split('-').map(Number);
                                return new Date(y, m - 1, 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
                            })()}
                        </span>
                    )}
                </div>

                {ctdSnapshot ? (
                    <>
                        <div className="summary-main-metrics">
                            <div className="metric-block">
                                <span className="metric-label">NSR</span>
                                <div className="metric-value-sub">{formatCurrency(ctdSnapshot.nsr)}</div>
                            </div>
                            <div className="metric-block">
                                <span className="metric-label">Margin %</span>
                                <div style={{ display: 'flex', alignItems: 'center' }}>
                                    <div className={`metric-value-lg ${ctdSnapshot.margin < targetMarginRatio ? 'text-red' : ''}`} style={{ color: ctdSnapshot.margin < targetMarginRatio ? '#ef4444' : undefined }}>
                                        {formatPercent(ctdSnapshot.margin)}
                                    </div>
                                    {renderVariance(ctdSnapshot.margin)}
                                </div>
                            </div>
                        </div>
                        {showDetails && (
                            <div className="summary-details animate-fade-in">
                                {renderDetailItem('Opening Balance', ctdSnapshot.openingBalance)}
                                {renderDetailItem('Total Billings', ctdSnapshot.cumulativeBillings)}
                                {renderDetailItem('WIP', ctdSnapshot.wip)}
                                {renderDetailItem('Direct Expenses', ctdSnapshot.directExpenses)}
                            </div>
                        )}
                    </>
                ) : (
                    <div className="empty-state">No confirmed months yet.</div>
                )}
            </div>

            <div className="summary-divider"></div>

            {/* EAC Section */}
            <div className="summary-section">
                <div className="summary-header" style={{ color: 'var(--deloitte-green)' }}>
                    <Flag size={14} />
                    Estimated At Completion (EAC)
                </div>

                {eacSnapshot ? (
                    <>
                        <div className="summary-main-metrics">
                            <div className="metric-block">
                                <span className="metric-label">Forecast NSR</span>
                                <div className="metric-value-sub">{formatCurrency(eacSnapshot.nsr)}</div>
                            </div>
                            <div className="metric-block">
                                <span className="metric-label">Forecast Margin %</span>
                                <div style={{ display: 'flex', alignItems: 'center' }}>
                                    <div className={`metric-value-lg ${eacSnapshot.margin < targetMarginRatio ? 'text-red' : ''}`} style={{ color: eacSnapshot.margin < targetMarginRatio ? '#ef4444' : undefined }}>
                                        {formatPercent(eacSnapshot.margin)}
                                    </div>
                                    {renderVariance(eacSnapshot.margin)}
                                </div>
                            </div>
                        </div>
                        {showDetails && (
                            <div className="summary-details animate-fade-in">
                                {renderDetailItem('Target Balance', 0)}
                                {renderDetailItem('Total Billings', eacSnapshot.cumulativeBillings)}
                                {renderDetailItem('Final WIP', eacSnapshot.wip)}
                                {renderDetailItem('Total Expenses', eacSnapshot.directExpenses)}
                            </div>
                        )}
                    </>
                ) : (
                    <div className="empty-state">No forecast data available.</div>
                )}
            </div>

            {/* Expander Toggle */}
            <button
                onClick={() => setShowDetails(!showDetails)}
                style={{
                    position: 'absolute',
                    bottom: '-12px',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    background: 'var(--bg-color)',
                    border: '1px solid var(--border-color)',
                    borderRadius: '12px',
                    padding: '2px 8px',
                    fontSize: '0.7rem',
                    color: 'var(--text-muted)',
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '4px',
                    zIndex: 10
                }}
            >
                {showDetails ? (
                    <>Hide Breakdown <ChevronUp size={10} /></>
                ) : (
                    <>Show Breakdown <ChevronDown size={10} /></>
                )}
            </button>
        </div>
    );
};

export default ProjectFinancialSummary;
