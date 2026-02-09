import React, { useMemo } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts';
import type { Project, ProjectMonthlySnapshot } from '../../../types';

interface Props {
    projects: Project[];
    snapshotsMap: Record<number, ProjectMonthlySnapshot[]>;
}

const TrendCharts: React.FC<Props> = ({ projects, snapshotsMap }) => {

    const chartData = useMemo(() => {
        // 1. Collect all unique months
        const allMonths = new Set<string>();
        Object.values(snapshotsMap).flat().forEach(s => allMonths.add(s.month));

        const sortedMonths = Array.from(allMonths).sort((a, b) => new Date(a).getTime() - new Date(b).getTime());

        // 2. Build data points for each month
        return sortedMonths.map(month => {
            let totalNsr = 0;
            let totalCost = 0;
            let weightedMarginSum = 0;
            let revenueForMargin = 0;

            projects.forEach(p => {
                const snapshots = snapshotsMap[p.id] || [];
                // Find snapshot for this month
                const snapshot = snapshots.find(s => s.month === month);

                if (snapshot) {
                    totalNsr += snapshot.nsr || 0;
                    totalCost += snapshot.operationalCost || 0; // Using operational cost
                    if ((snapshot.nsr || 0) > 0) {
                        weightedMarginSum += (snapshot.margin * snapshot.nsr);
                        revenueForMargin += snapshot.nsr;
                    }
                } else {
                    // If no snapshot for this exact month, we might want to carry forward previous value?
                    // For now, let's strictly show data where snapshots exist to avoid projecting finished projects forever
                    // OR: Use strict temporal alignment. If a project hasn't started, it's 0.
                    // If it finished, do we count it? Yes, but its value doesn't change.
                    // For simplicity, we only sum active snapshots for that month.
                }
            });

            const avgMargin = revenueForMargin > 0 ? (weightedMarginSum / revenueForMargin) : 0;

            return {
                month: (() => {
                    const [y, m] = month.split('-').map(Number);
                    return new Date(y, m - 1, 1).toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
                })(),
                nsr: totalNsr,
                cost: totalCost,
                margin: avgMargin * 100 // Convert to percentage
            };
        });
    }, [projects, snapshotsMap]);

    if (chartData.length === 0) {
        return <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>Not enough data for trend analysis.</div>;
    }

    const formatCurrency = (val: number) => {
        if (Math.abs(val) >= 1000000) return `€${(val / 1000000).toFixed(1)}M`;
        if (Math.abs(val) >= 1000) return `€${(val / 1000).toFixed(0)}k`;
        return `€${val.toFixed(0)}`;
    };

    return (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(500px, 1fr))', gap: '1.5rem' }}>
            <div className="chart-container">
                <h3 style={{ marginBottom: '1rem', fontSize: '1rem', color: 'var(--text-secondary)' }}>Portfolio Revenue (NSR) Evolution</h3>
                <ResponsiveContainer width="100%" height="90%">
                    <AreaChart data={chartData}>
                        <defs>
                            <linearGradient id="colorNsr" x1="0" y1="0" x2="0" y2="1">
                                <stop offset="5%" stopColor="var(--deloitte-green)" stopOpacity={0.3} />
                                <stop offset="95%" stopColor="var(--deloitte-green)" stopOpacity={0} />
                            </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" vertical={false} />
                        <XAxis
                            dataKey="month"
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                        />
                        <YAxis
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                            tickFormatter={formatCurrency}
                        />
                        <Tooltip
                            contentStyle={{ backgroundColor: 'var(--card-bg)', borderColor: 'var(--border-color)', color: 'var(--text-primary)' }}
                            formatter={(value: any) => [formatCurrency(Number(value)), 'NSR']}
                        />
                        <Area type="monotone" dataKey="nsr" stroke="var(--deloitte-green)" fillOpacity={1} fill="url(#colorNsr)" strokeWidth={3} />
                    </AreaChart>
                </ResponsiveContainer>
            </div>

            <div className="chart-container">
                <h3 style={{ marginBottom: '1rem', fontSize: '1rem', color: 'var(--text-secondary)' }}>Average Margin %</h3>
                <ResponsiveContainer width="100%" height="90%">
                    <LineChart data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" vertical={false} />
                        <XAxis
                            dataKey="month"
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                        />
                        <YAxis
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                            unit="%"
                        />
                        <Tooltip
                            contentStyle={{ backgroundColor: 'var(--card-bg)', borderColor: 'var(--border-color)', color: 'var(--text-primary)' }}
                            formatter={(value: any) => [`${Number(value).toFixed(1)}%`, 'Margin']}
                        />
                        <Line type="monotone" dataKey="margin" stroke="#f59e0b" strokeWidth={3} dot={{ r: 4, fill: '#f59e0b' }} />
                    </LineChart>
                </ResponsiveContainer>
            </div>
        </div>
    );
};

export default TrendCharts;
