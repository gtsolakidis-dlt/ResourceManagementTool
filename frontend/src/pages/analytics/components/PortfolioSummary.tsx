import React, { useMemo } from 'react';
import type { Project, ProjectMonthlySnapshot } from '../../../types';
import { Wallet, Target, Activity, PieChart } from 'lucide-react';

interface Props {
    projects: Project[];
    snapshotsMap: Record<number, ProjectMonthlySnapshot[]>;
}

const PortfolioSummary: React.FC<Props> = ({ projects, snapshotsMap }) => {

    const stats = useMemo(() => {
        let totalBudget = 0;
        let totalNsr = 0;
        let weightedMarginSum = 0;
        let totalRevenueForMargin = 0;

        projects.forEach(p => {
            totalBudget += p.actualBudget || 0;

            const snapshots = snapshotsMap[p.id] || [];
            if (snapshots.length > 0) {
                // Sort by date ascending and take the last (most recent) snapshot
                const sorted = [...snapshots].sort((a, b) =>
                    new Date(a.month).getTime() - new Date(b.month).getTime()
                );
                const lastSnapshot = sorted[sorted.length - 1];

                if (lastSnapshot) {
                    totalNsr += lastSnapshot.nsr || 0;
                    // snapshot.margin is stored as a decimal (e.g., 0.20 for 20%)
                    if ((lastSnapshot.nsr || 0) > 0) {
                        weightedMarginSum += (lastSnapshot.margin * lastSnapshot.nsr);
                        totalRevenueForMargin += lastSnapshot.nsr;
                    }
                }
            }
        });

        // avgMargin from snapshots is already in decimal form (0.20 = 20%)
        const avgMargin = totalRevenueForMargin > 0 ? (weightedMarginSum / totalRevenueForMargin) : 0;

        // Fallback: average target margin from project definitions
        // Note: Project.targetMargin is stored as a percentage number (e.g., 20 for 20%)
        // We need to convert it to decimal for consistency
        const avgTargetMarginDecimal = projects.length > 0
            ? projects.reduce((sum, p) => sum + ((p.targetMargin || 0) / 100), 0) / projects.length
            : 0;

        return {
            totalBudget,
            totalNsr,
            // If we have actual margin data, use it; otherwise fall back to target margin
            avgMargin: totalRevenueForMargin > 0 ? avgMargin : avgTargetMarginDecimal,
            projectCount: projects.length
        };
    }, [projects, snapshotsMap]);

    const formatCurrency = (val: number) => {
        if (Math.abs(val) >= 1000000) return `€${(val / 1000000).toFixed(2)}M`;
        if (Math.abs(val) >= 1000) return `€${(val / 1000).toFixed(1)}k`;
        return `€${val.toFixed(0)}`;
    };

    return (
        <div className="analytics-grid">
            <div className="kpi-card">
                <div className="kpi-icon-wrapper" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' }}>
                    <Wallet size={24} />
                </div>
                <div className="kpi-value">{formatCurrency(stats.totalBudget)}</div>
                <div className="kpi-label">Total Contract Value</div>
            </div>

            <div className="kpi-card">
                <div className="kpi-icon-wrapper" style={{ background: 'rgba(16, 185, 129, 0.1)', color: '#10b981' }}>
                    <PieChart size={24} />
                </div>
                <div className="kpi-value">{formatCurrency(stats.totalNsr)}</div>
                <div className="kpi-label">Recognized Revenue (NSR)</div>
            </div>

            <div className="kpi-card">
                <div className="kpi-icon-wrapper" style={{ background: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' }}>
                    <Target size={24} />
                </div>
                {/* avgMargin is in decimal (0.20 = 20%), so multiply by 100 for display */}
                <div className="kpi-value">{(stats.avgMargin * 100).toFixed(1)}%</div>
                <div className="kpi-label">Portfolio Margin</div>
            </div>

            <div className="kpi-card">
                <div className="kpi-icon-wrapper" style={{ background: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6' }}>
                    <Activity size={24} />
                </div>
                <div className="kpi-value">{stats.projectCount}</div>
                <div className="kpi-label">Active Projects</div>
            </div>
        </div>
    );
};

export default PortfolioSummary;
