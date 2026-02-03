import React, { useEffect, useState } from 'react';
import { useNavigation } from '../../context/NavigationContext';
import { projectService, snapshotService, rosterService, forecastService } from '../../api/services';
import type { Project, ProjectMonthlySnapshot, RosterMember, ResourceAllocation } from '../../types';
import { Loader2, TrendingUp, BarChart3, Users } from 'lucide-react';
import PortfolioSummary from './components/PortfolioSummary';
import TrendCharts from './components/TrendCharts';
import UtilizationAnalytics from './components/UtilizationAnalytics';
import './AnalyticsPage.css';

interface ForecastVersion {
    id: number;
    projectId: number;
    name: string;
    isBaseline: boolean;
}

const AnalyticsPage: React.FC = () => {
    const { setBreadcrumbs, setActiveSection, setSidebarSubItems } = useNavigation();
    const [loading, setLoading] = useState(true);
    const [projects, setProjects] = useState<Project[]>([]);
    const [allSnapshots, setAllSnapshots] = useState<Record<number, ProjectMonthlySnapshot[]>>({});
    const [roster, setRoster] = useState<RosterMember[]>([]);
    const [allocations, setAllocations] = useState<ResourceAllocation[]>([]);

    useEffect(() => {
        setActiveSection('analytics');
        setBreadcrumbs([{ label: 'Resource Platform', path: '/' }, { label: 'Analytics', disabled: true }]);
        setSidebarSubItems([]);
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);

            // 1. Fetch Projects and Roster
            const [projectsRes, rosterRes] = await Promise.all([
                projectService.getProjects(),
                rosterService.getMembers()
            ]);

            const projectsData = projectsRes.data || [];
            setProjects(projectsData);
            setRoster(rosterRes.data || []);

            // 2. First fetch versions for each project to get the correct version IDs
            const versionPromises = projectsData.slice(0, 20).map(p =>
                forecastService.getVersions(p.id)
                    .then(res => ({ projectId: p.id, versions: (res.data || []) as ForecastVersion[] }))
                    .catch(() => ({ projectId: p.id, versions: [] as ForecastVersion[] }))
            );

            const versionResults = await Promise.all(versionPromises);

            // Build a map of projectId -> first version ID (or baseline version)
            const projectVersionMap: Record<number, number> = {};
            versionResults.forEach(r => {
                if (r.versions.length > 0) {
                    // Prefer baseline version, otherwise take first
                    const baselineVersion = r.versions.find(v => v.isBaseline);
                    projectVersionMap[r.projectId] = baselineVersion ? baselineVersion.id : r.versions[0].id;
                }
            });

            // 3. Fetch Snapshots for each project using the correct version ID
            const snapshotPromises = projectsData.slice(0, 20).map(p => {
                const versionId = projectVersionMap[p.id];
                if (!versionId) {
                    return Promise.resolve({ projectId: p.id, snapshots: [] });
                }
                return snapshotService.getSnapshots(p.id, versionId)
                    .then(res => ({ projectId: p.id, snapshots: res.data || [] }))
                    .catch(() => ({ projectId: p.id, snapshots: [] }));
            });

            const snapshotsResults = await Promise.all(snapshotPromises);
            const snapshotsMap: Record<number, ProjectMonthlySnapshot[]> = {};
            snapshotsResults.forEach(r => {
                snapshotsMap[r.projectId] = r.snapshots;
            });
            setAllSnapshots(snapshotsMap);

            // 4. Fetch allocations for each project's version
            const allocationPromises = Object.entries(projectVersionMap).map(([_, versionId]) =>
                forecastService.getAllocations(versionId)
                    .then(res => res.data || [])
                    .catch(() => [])
            );

            const allocationsResults = await Promise.all(allocationPromises);
            const flatAllocations = allocationsResults.flat();
            setAllocations(flatAllocations);

        } catch (error) {
            console.error('Failed to load analytics data:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="analytics-container" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
                <div style={{ textAlign: 'center' }}>
                    <Loader2 size={48} className="animate-spin" style={{ color: 'var(--deloitte-green)', marginBottom: '1rem' }} />
                    <p style={{ color: 'var(--text-secondary)' }}>Gathering portfolio insights...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="analytics-container">
            <header className="analytics-header">
                <div>
                    <h1>Analytics & Insights</h1>
                    <p style={{ color: 'var(--text-muted)' }}>Portfolio performance, financial trends, and resource utilization.</p>
                </div>
            </header>

            <div className="analytics-section">
                <div className="section-header">
                    <TrendingUp size={20} color="var(--deloitte-green)" />
                    <h2>Portfolio Overview</h2>
                </div>
                <PortfolioSummary projects={projects} snapshotsMap={allSnapshots} />
            </div>

            <div className="analytics-section">
                <div className="section-header">
                    <BarChart3 size={20} color="var(--deloitte-green)" />
                    <h2>Financial Trends</h2>
                </div>
                <TrendCharts projects={projects} snapshotsMap={allSnapshots} />
            </div>

            <div className="analytics-section">
                <div className="section-header">
                    <Users size={20} color="var(--deloitte-green)" />
                    <h2>Resource Utilization</h2>
                </div>
                <UtilizationAnalytics roster={roster} allocations={allocations} />
            </div>
        </div>
    );
};

export default AnalyticsPage;
