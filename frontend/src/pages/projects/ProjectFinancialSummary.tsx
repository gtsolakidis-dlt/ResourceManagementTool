import React, { useState } from 'react';
import type { Project, ProjectMonthlySnapshot } from '../../types';
import { TrendingUp, TrendingDown, Lock, Flag, Calendar, ChevronDown, ChevronUp } from 'lucide-react';
import './ProjectFinancialSummary.css';

interface Props {
    project: Project;
    snapshots: ProjectMonthlySnapshot[];
}

const formatCurrency = (val: number) => {
    if (Math.abs(val) >= 1000000) return `€${(val / 1000000).toFixed(2)}M`;
    if (Math.abs(val) >= 1000) return `€${(val / 1000).toFixed(1)}k`;
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
    // Sort by date ascending
    const sorted = [...snapshots].sort((a, b) => new Date(a.month).getTime() - new Date(b.month).getTime());

    // Find last confirmed
    const ctdSnapshot = [...sorted].reverse().find(s => s.status === 'Confirmed');

    // 3. Find EAC Snapshot (Last one)
    const eacSnapshot = sorted.length > 0 ? sorted[sorted.length - 1] : null;

    const renderVariance = (actualMargin: number) => {
        const diff = actualMargin - targetMarginRatio;
        const diffPercent = (diff * 100).toFixed(1);
        const isPositive = diff >= -0.001; // tolerance
        const Icon = isPositive ? TrendingUp : TrendingDown;
        const color = isPositive ? 'var(--deloitte-green)' : '#ef4444';

        return (
            <div style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '0.75rem', color: color, marginTop: '4px' }}>
                <Icon size={12} />
                <span>{Math.abs(Number(diffPercent))}% {isPositive ? 'above' : 'below'} target</span>
            </div>
        );
    };

    const renderDetailItem = (label: string, value: number) => (
        <div>
            <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', marginBottom: '4px' }}>{label}</div>
            <div style={{ fontSize: '0.9rem', fontWeight: 600 }}>{formatCurrencyFull(value)}</div>
        </div>
    );

    return (
        <div className="financial-hero-grid animate-fade-in" style={{
            display: 'grid',
            gridTemplateColumns: '1fr 1fr',
            gap: '1.5rem',
            marginBottom: '2rem'
        }}>
            {/* CTD Card */}
            <div className="glass-panel hero-card" style={{ padding: '1.5rem', position: 'relative', overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
                <div style={{ position: 'absolute', top: 0, right: 0, padding: '1rem', opacity: 0.05, transform: 'scale(1.5)', transformOrigin: 'top right' }}>
                    <Lock size={100} />
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--text-secondary)', fontSize: '0.875rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    <Lock size={14} />
                    Contract To Date (CTD)
                    {ctdSnapshot && (
                        <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: '4px', background: 'rgba(255,255,255,0.1)', padding: '2px 8px', borderRadius: '12px', fontSize: '0.7rem' }}>
                            <Calendar size={10} />
                            {(() => {
                                const [y, m] = ctdSnapshot.month.split('-').map(Number);
                                return new Date(y, m - 1, 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
                            })()}
                        </div>
                    )}
                </div>

                {ctdSnapshot ? (
                    <>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: showDetails ? '1.5rem' : '0' }}>
                            <div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '4px' }}>Revenue (NSR)</div>
                                <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-primary)' }}>{formatCurrency(ctdSnapshot.nsr)}</div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: '4px' }}>Billings: {formatCurrency(ctdSnapshot.cumulativeBillings)}</div>
                            </div>
                            <div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '4px' }}>Margin</div>
                                <div style={{ fontSize: '1.75rem', fontWeight: 700, color: ctdSnapshot.margin < targetMarginRatio ? '#ef4444' : 'var(--deloitte-green)' }}>
                                    {formatPercent(ctdSnapshot.margin)}
                                </div>
                                {renderVariance(ctdSnapshot.margin)}
                            </div>
                        </div>

                        {showDetails && (
                            <div className="animate-fade-in" style={{ borderTop: '1px solid rgba(255,255,255,0.1)', paddingTop: '1rem', marginTop: '1rem', display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: '1rem' }}>
                                {renderDetailItem('Opening Balance', ctdSnapshot.openingBalance)}
                                {renderDetailItem('WIP', ctdSnapshot.wip)}
                                {renderDetailItem('Direct Expenses', ctdSnapshot.directExpenses)}
                                {renderDetailItem('Operational Cost', ctdSnapshot.operationalCost)}
                            </div>
                        )}
                    </>
                ) : (
                    <div style={{ color: 'var(--text-muted)', fontStyle: 'italic', padding: '1rem 0' }}>No confirmed history available yet.</div>
                )}
            </div>

            {/* EAC Card */}
            <div className="glass-panel hero-card" style={{ padding: '1.5rem', position: 'relative', overflow: 'hidden', border: '1px solid rgba(134, 188, 37, 0.3)', display: 'flex', flexDirection: 'column' }}>
                <div style={{ position: 'absolute', top: 0, right: 0, padding: '1rem', opacity: 0.05, color: 'var(--deloitte-green)', transform: 'scale(1.5)', transformOrigin: 'top right' }}>
                    <Flag size={100} />
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--deloitte-green)', fontSize: '0.875rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    <Flag size={14} />
                    Estimate At Completion (EAC)
                </div>

                {eacSnapshot ? (
                    <>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: showDetails ? '1.5rem' : '0' }}>
                            <div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '4px' }}>Projected NSR</div>
                                <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-primary)' }}>{formatCurrency(eacSnapshot.nsr)}</div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: '4px' }}>Cost: {formatCurrency(eacSnapshot.directExpenses + eacSnapshot.operationalCost)}</div>
                            </div>
                            <div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '4px' }}>Projected Margin</div>
                                <div style={{ fontSize: '1.75rem', fontWeight: 700, color: eacSnapshot.margin < targetMarginRatio ? '#ef4444' : 'var(--deloitte-green)' }}>
                                    {formatPercent(eacSnapshot.margin)}
                                </div>
                                {renderVariance(eacSnapshot.margin)}
                            </div>
                        </div>

                        {showDetails && (
                            <div className="animate-fade-in" style={{ borderTop: '1px solid rgba(255,255,255,0.1)', paddingTop: '1rem', marginTop: '1rem', display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: '1rem' }}>
                                {renderDetailItem('Target Balance', 0)}
                                {renderDetailItem('Final WIP', eacSnapshot.wip)}
                                {renderDetailItem('Total Expenses', eacSnapshot.directExpenses)}
                                {renderDetailItem('Ops Cost', eacSnapshot.operationalCost)}
                            </div>
                        )}
                    </>
                ) : (
                    <div style={{ color: 'var(--text-muted)', fontStyle: 'italic', padding: '1rem 0' }}>No forecast data available.</div>
                )}
            </div>

            {/* Central Toggle Button */}
            <div style={{ gridColumn: '1 / -1', display: 'flex', justifyContent: 'center', marginTop: '-1rem', zIndex: 10 }}>
                <button
                    onClick={() => setShowDetails(!showDetails)}
                    style={{
                        background: 'var(--bg-color)',
                        border: '1px solid var(--border-color)',
                        borderRadius: '20px',
                        padding: '4px 12px',
                        fontSize: '0.75rem',
                        color: 'var(--text-muted)',
                        cursor: 'pointer',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '6px',
                        boxShadow: '0 2px 5px rgba(0,0,0,0.1)'
                    }}
                >
                    {showDetails ? (
                        <>Hide Breakdown <ChevronUp size={12} /></>
                    ) : (
                        <>Show Breakdown <ChevronDown size={12} /></>
                    )}
                </button>
            </div>
        </div>
    );
};

export default ProjectFinancialSummary;
