import React, { useMemo } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import type { RosterMember, ResourceAllocation } from '../../../types';

interface Props {
    roster: RosterMember[];
    allocations: ResourceAllocation[];
}

const UtilizationAnalytics: React.FC<Props> = ({ roster, allocations }) => {

    const levelData = useMemo(() => {
        const levelMap: Record<string, number> = {};

        allocations.forEach(alloc => {
            const member = roster.find(r => r.id === alloc.rosterId);
            if (member && member.level) {
                levelMap[member.level] = (levelMap[member.level] || 0) + (alloc.allocatedDays || 0);
            }
        });

        // Convert to array
        return Object.entries(levelMap)
            .map(([level, days]) => ({ level, days }))
            .sort((a, b) => b.days - a.days); // Sort desc
    }, [roster, allocations]);

    const topResources = useMemo(() => {
        const resourceMap: Record<number, number> = {};
        allocations.forEach(alloc => {
            resourceMap[alloc.rosterId] = (resourceMap[alloc.rosterId] || 0) + (alloc.allocatedDays || 0);
        });

        return Object.entries(resourceMap)
            .map(([id, days]) => {
                const member = roster.find(r => r.id === Number(id));
                return {
                    name: member ? member.fullNameEn : 'Unknown',
                    level: member ? member.level : '-',
                    days
                };
            })
            .sort((a, b) => b.days - a.days)
            .slice(0, 5); // Top 5
    }, [roster, allocations]);

    const COLORS = ['#86bc25', '#0076a8', '#62b5e5', '#f0dc00', '#000000'];

    return (
        <div className="utilization-grid">
            <div className="chart-container">
                <h3 style={{ marginBottom: '1rem', fontSize: '1rem', color: 'var(--text-secondary)' }}>Total Allocated Days by Level</h3>
                <ResponsiveContainer width="100%" height="90%">
                    <BarChart data={levelData}>
                        <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" vertical={false} />
                        <XAxis
                            dataKey="level"
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                        />
                        <YAxis
                            tick={{ fill: 'var(--text-muted)', fontSize: 12 }}
                            axisLine={false}
                            tickLine={false}
                        />
                        <Tooltip
                            cursor={{ fill: 'rgba(255,255,255,0.05)' }}
                            contentStyle={{ backgroundColor: 'var(--card-bg)', borderColor: 'var(--border-color)', color: 'var(--text-primary)' }}
                        />
                        <Bar dataKey="days" radius={[4, 4, 0, 0]}>
                            {levelData.map((_, index) => (
                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                        </Bar>
                    </BarChart>
                </ResponsiveContainer>
            </div>

            <div className="kpi-card">
                <h3 style={{ marginBottom: '1rem', fontSize: '1rem', color: 'var(--text-secondary)' }}>Most Utilized Talent</h3>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                    {topResources.map((res, idx) => (
                        <div key={idx} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingBottom: '0.75rem', borderBottom: idx < 4 ? '1px solid var(--border-color)' : 'none' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                <div style={{
                                    width: '28px', height: '28px', borderRadius: '50%', background: 'var(--bg-secondary)',
                                    display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '0.7rem', fontWeight: 600
                                }}>
                                    {res.name.substring(0, 2).toUpperCase()}
                                </div>
                                <div>
                                    <div style={{ fontSize: '0.9rem', fontWeight: 500 }}>{res.name}</div>
                                    <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{res.level}</div>
                                </div>
                            </div>
                            <div style={{ fontSize: '0.9rem', fontWeight: 600, color: 'var(--deloitte-green)' }}>
                                {res.days.toFixed(1)} <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', fontWeight: 400 }}>days</span>
                            </div>
                        </div>
                    ))}
                    {topResources.length === 0 && (
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>No allocation data found.</p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default UtilizationAnalytics;
