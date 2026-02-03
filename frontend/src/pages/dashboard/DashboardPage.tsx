import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useNavigation } from '../../context/NavigationContext';
import { Users, Wallet, Target, TrendingUp, Clock, Activity, Zap } from 'lucide-react';
import { projectService, rosterService, auditService } from '../../api/services';
import type { Project, AuditLog } from '../../types';
import './DashboardPage.css';

const DashboardPage: React.FC = () => {
    const { setBreadcrumbs, setActiveSection, setSidebarSubItems } = useNavigation();
    const navigate = useNavigate();
    const [stats, setStats] = useState({
        totalProjects: 0,
        totalBudget: 0,
        activeResources: 0,
        avgMargin: 0
    });
    const [activities, setActivities] = useState<AuditLog[]>([]);

    useEffect(() => {
        setActiveSection('dashboard');
        setBreadcrumbs([{ label: 'Resource Platform', path: '/' }, { label: 'Mission Control', disabled: true }]);
        setSidebarSubItems([]);
        loadData();
    }, []);

    const loadData = async () => {
        try {
            // Fetch data independently so one failure doesn't block the others
            const projectsPromise = projectService.getProjects().catch(err => {
                console.error('Failed to load projects:', err);
                return { data: [] };
            });

            const rosterPromise = rosterService.getMembers().catch(err => {
                console.error('Failed to load roster:', err);
                return { data: [] };
            });

            const auditsPromise = auditService.getRecent(5).catch(err => {
                console.error('Failed to load audits:', err);
                return { data: [] };
            });

            const [projRes, rosterRes, auditRes] = await Promise.all([projectsPromise, rosterPromise, auditsPromise]);

            const projects = projRes.data || [];
            const roster = rosterRes.data || [];
            const recentAudits = auditRes.data || [];

            const totalBudget = projects.reduce((sum: number, p: Project) => sum + (p.actualBudget || 0), 0);
            const avgMargin = projects.length > 0
                ? projects.reduce((sum: number, p: Project) => sum + (p.targetMargin || 0), 0) / projects.length
                : 0;

            setStats({
                totalProjects: projects.length,
                totalBudget,
                activeResources: roster.length,
                avgMargin
            });
            setActivities(recentAudits);
        } catch (error) {
            console.error('Unexpected error in dashboard load', error);
        }
    };

    const formatCurrency = (val: number) => {
        if (val >= 1000000) return `€${(val / 1000000).toFixed(1)}M`;
        if (val >= 1000) return `€${(val / 1000).toFixed(1)}k`;
        return `€${val.toFixed(0)}`;
    };

    const getRelativeTime = (dateStr: string) => {
        const date = new Date(dateStr);
        const now = new Date();
        const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (diffInSeconds < 60) return 'Just now';
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} mins ago`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
        return `${Math.floor(diffInSeconds / 86400)} days ago`;
    };

    const getInitials = (name: string) => {
        return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    };

    const formatActivityMessage = (log: AuditLog) => {
        const userDisplay = log.changedBy === 'AuthenticatedUser' ? 'System' : log.changedBy;
        const lowerAction = log.action.toLowerCase();
        const lowerEntity = log.entityName.toLowerCase();
        let actionText = '';
        let details = '';

        // Extract potential details from newValues if it's JSON
        let values: any = {};
        try {
            if (log.newValues) values = JSON.parse(log.newValues);
        } catch (e) { /* ignore */ }

        if (lowerEntity.includes('createproject')) {
            actionText = 'created a new project';
            if (values.Name) details = values.Name;
        } else if (lowerEntity.includes('updateproject')) {
            actionText = 'updated project';
            if (values.Name) details = values.Name;
        } else if (lowerEntity.includes('upsertallocations')) {
            actionText = 'updated resource allocations';
        } else if (lowerEntity.includes('confirmmonth')) {
            actionText = 'confirmed monthly forecast';
            if (values.Month) details = new Date(values.Month).toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
        } else if (lowerEntity.includes('overwritesnapshot')) {
            actionText = 'overwrote financial snapshot';
             if (values.Month) details = new Date(values.Month).toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
        } else if (lowerEntity.includes('clearoverride')) {
            actionText = 'cleared financial override';
        } else if (lowerEntity.includes('cloneversion')) {
            actionText = 'cloned forecast version';
        } else if (lowerEntity.includes('createmember')) {
            actionText = 'added new talent';
            if (values.FullNameEn) details = values.FullNameEn;
        } else {
             // Fallback
             actionText = `${lowerAction} ${log.entityName}`;
        }

        return (
            <div className="activity-message">
                <strong>{userDisplay}</strong> {actionText} {details && <strong>{details}</strong>}
            </div>
        );
    };

    return (
        <div className="dashboard-container">
            <header className="dashboard-header">
                <h1>Mission Control</h1>
                <p style={{ color: 'var(--text-muted)' }}>Welcome back. Here is your portfolio performance overview.</p>
            </header>

            <div className="dashboard-grid">
                <div className="dashboard-widget">
                    <div className="widget-icon-wrapper" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' }}>
                        <Wallet size={24} />
                    </div>
                    <div className="widget-trend trend-up">
                        <TrendingUp size={14} />
                        <span>+12%</span>
                    </div>
                    <div className="widget-value">{formatCurrency(stats.totalBudget)}</div>
                    <div className="widget-label">Total Portfolio Budget</div>
                </div>

                <div className="dashboard-widget">
                    <div className="widget-icon-wrapper" style={{ background: 'rgba(134, 188, 37, 0.1)', color: 'var(--deloitte-green)' }}>
                        <Users size={24} />
                    </div>
                    <div className="widget-value">{stats.activeResources}</div>
                    <div className="widget-label">Active Talent Pool</div>
                </div>

                <div className="dashboard-widget">
                    <div className="widget-icon-wrapper" style={{ background: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' }}>
                        <Target size={24} />
                    </div>
                    <div className="widget-value">{(stats.avgMargin * 100).toFixed(1)}%</div>
                    <div className="widget-label">Avg. Target Margin</div>
                </div>

                <div className="dashboard-widget">
                    <div className="widget-icon-wrapper" style={{ background: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6' }}>
                        <Activity size={24} />
                    </div>
                    <div className="widget-value">{stats.totalProjects}</div>
                    <div className="widget-label">Active Projects</div>
                </div>
            </div>

            <div className="dashboard-grid" style={{ gridTemplateColumns: '2fr 1fr' }}>
                <div className="dashboard-widget">
                    <div className="section-title">
                        <Clock size={20} />
                        <span>Recent Activity</span>
                    </div>
                    <div className="activity-feed">
                        {activities.length === 0 ? (
                            <div className="activity-item">
                                <div className="activity-content">
                                    <div className="activity-message">No recent activity found.</div>
                                </div>
                            </div>
                        ) : (
                            activities.map((log) => (
                                <div className="activity-item" key={log.id}>
                                    <div className="activity-avatar" style={{ background: '#dbeafe', color: '#1e40af' }}>
                                        {getInitials(log.changedBy === 'AuthenticatedUser' ? 'System' : log.changedBy)}
                                    </div>
                                    <div className="activity-content">
                                        {formatActivityMessage(log)}
                                        <div className="activity-time">{getRelativeTime(log.changedAt)}</div>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                <div className="dashboard-widget" style={{ background: 'linear-gradient(145deg, var(--card-bg), rgba(134, 188, 37, 0.05))' }}>
                    <div className="section-title">
                        <Zap size={20} />
                        <span>Quick Actions</span>
                    </div>
                    <div style={{ display: 'grid', gap: '1rem' }}>
                        <button className="btn-premium" style={{ width: '100%', justifyContent: 'center' }} onClick={() => navigate('/projects?action=create')}>Create Project</button>
                        <button className="btn-outline-premium" style={{ width: '100%', justifyContent: 'center' }} onClick={() => navigate('/roster?action=create')}>Add Resource</button>
                        <button className="btn-outline-premium" style={{ width: '100%', justifyContent: 'center' }} onClick={() => navigate('/analytics')}>View Reports</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DashboardPage;
