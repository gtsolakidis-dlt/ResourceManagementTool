
import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { rosterService } from '../../api/services';
import type { RosterMember } from '../../types';
import { Loader2, ArrowLeft, CreditCard, UserCheck } from 'lucide-react';
import { useNavigation } from '../../context/NavigationContext';
import { getSeniorityBadgeClass } from '../../constants/seniorityLevels';
import './ResourceProfilePage.css';

const ResourceProfilePage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { setBreadcrumbs } = useNavigation();
    const [member, setMember] = useState<RosterMember | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadMember();
    }, [id]);

    const loadMember = async () => {
        if (!id) return;
        setLoading(true);
        try {
            const res = await rosterService.getMember(parseInt(id));
            setMember(res.data);

            // Set breadcrumbs after data load
            setBreadcrumbs([
                { label: 'Resource Platform', path: '/' },
                { label: 'Resource Roster', path: '/roster' },
                { label: res.data.fullNameEn, disabled: true }
            ]);
        } catch (error) {
            console.error('Failed to load member:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="loading-state" style={{ height: '50vh' }}>
                <Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" />
            </div>
        );
    }

    if (!member) return <div>Resource not found</div>;

    return (
        <div className="profile-premium-container animate-fade-in">
            <button className="back-btn" onClick={() => navigate('/roster')}>
                <ArrowLeft size={18} />
                Back to Roster
            </button>

            <header className="profile-header">
                <div>
                    <h1 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.5rem' }}>{member.fullNameEn}</h1>
                    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
                        <span className={`level-badge ${getSeniorityBadgeClass(member.level || '')}`} style={{ fontSize: '1rem', padding: '0.25rem 0.75rem' }}>
                            {member.level || 'N/A'}
                        </span>
                        <span style={{ color: 'var(--text-muted)' }}>•</span>
                        <span style={{ color: 'var(--text-secondary)', fontWeight: 500 }}>{member.functionBusinessUnit}</span>
                    </div>
                </div>
                <div style={{ textAlign: 'right' }}>
                    <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '0.25rem' }}>SAP CODE</div>
                    <code style={{ fontSize: '1.25rem', fontWeight: 600, color: 'var(--deloitte-green)' }}>{member.sapCode}</code>
                </div>
            </header>

            <div className="stat-grid">
                <div className="profile-card stat-item">
                    <span className="stat-label">Daily Cost (Standard)</span>
                    <span className="stat-value currency">€{member.dailyCost?.toLocaleString('de-DE', { minimumFractionDigits: 2 })}</span>
                </div>
                <div className="profile-card stat-item">
                    <span className="stat-label">Monthly Cost (12m)</span>
                    <span className="stat-value currency">€{member.monthlyCost_12months?.toLocaleString('de-DE', { minimumFractionDigits: 2 })}</span>
                </div>
                <div className="profile-card stat-item">
                    <span className="stat-label">Monthly Cost (14m)</span>
                    <span className="stat-value currency">€{member.monthlyCost_14months?.toLocaleString('de-DE', { minimumFractionDigits: 2 })}</span>
                </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'minmax(400px, 2fr) 1fr', gap: '2rem' }}>
                <div className="profile-card">
                    <h3 className="section-title">
                        <UserCheck size={20} color="var(--deloitte-green)" />
                        Employment Details
                    </h3>
                    <div className="info-grid">
                        <div className="info-row">
                            <span className="info-label">Legal Entity</span>
                            <span className="info-data">{member.legalEntity || '-'}</span>
                        </div>
                        <div className="info-row">
                            <span className="info-label">Cost Center</span>
                            <span className="info-data">{member.costCenterCode || '-'}</span>
                        </div>
                        <div className="info-row">
                            <span className="info-label">Seniority Level</span>
                            <span className="info-data">{member.level || '-'}</span>
                        </div>
                        <div className="info-row">
                            <span className="info-label">System Role</span>
                            <span className="info-data">{member.role || 'User'}</span>
                        </div>
                    </div>
                </div>

                <div className="profile-card">
                    <h3 className="section-title">
                        <CreditCard size={20} color="var(--deloitte-green)" />
                        Attributes
                    </h3>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div className="info-row">
                            <span className="info-label">Company Car</span>
                            <span className="info-data">{member.cars > 0 ? `€${member.cars}` : 'No'}</span>
                        </div>
                        <div className="info-row">
                            <span className="info-label">Ticket Restaurant</span>
                            <span className="info-data">{member.ticketRestaurant > 0 ? `€${member.ticketRestaurant}` : 'No'}</span>
                        </div>
                        <div className="info-row">
                            <span className="info-label">Metlife Insurance</span>
                            <span className="info-data">{member.metlife > 0 ? `€${member.metlife}` : 'No'}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ResourceProfilePage;
