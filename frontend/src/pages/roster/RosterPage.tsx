import React, { useEffect, useState, useRef } from 'react';
import { createPortal } from 'react-dom';
import { useSearchParams } from 'react-router-dom';
import { rosterService } from '../../api/services';
import type { RosterMember } from '../../types';
import { Search, Plus, Loader2, Edit2, Trash2, Download, Filter, X, Check, RotateCcw } from 'lucide-react';
import RosterModal from './RosterModal';
import { useNotification } from '../../context/NotificationContext';
import { downloadFile } from '../../utils/downloadFile';
import './RosterPage.css';
import { useNavigation } from '../../context/NavigationContext';
import { getSeniorityBadgeClass, SENIORITY_LEVELS } from '../../constants/seniorityLevels';
import PremiumSelect from '../../components/common/PremiumSelect';

const FUNCTIONS_LOV = [
    'Engineering',
    'AI & Data',
    'Operations'
];

const RosterPage: React.FC = () => {
    const { setBreadcrumbs } = useNavigation();
    const { notify } = useNotification();
    const [searchParams, setSearchParams] = useSearchParams();
    const [members, setMembers] = useState<RosterMember[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedMember, setSelectedMember] = useState<RosterMember | undefined>();
    const filterButtonRef = useRef<HTMLDivElement>(null);

    // Filters
    const [isFilterOpen, setIsFilterOpen] = useState(false);
    const [filterFunction, setFilterFunction] = useState('');
    const [filterLevel, setFilterLevel] = useState('');

    const [tempFunction, setTempFunction] = useState('');
    const [tempLevel, setTempLevel] = useState('');

    useEffect(() => {
        setBreadcrumbs([
            { label: 'Resource Platform', path: '/' },
            { label: 'Resource Roster', disabled: true }
        ]);
        if (searchParams.get('action') === 'create') {
            setSelectedMember(undefined);
            setIsModalOpen(true);
            searchParams.delete('action');
            setSearchParams(searchParams, { replace: true });
        }
    }, []);

    const loadMembers = async () => {
        setLoading(true);
        try {
            const response = await rosterService.getMembers({
                searchTerm,
                functionBusinessUnit: filterFunction,
                level: filterLevel
            });
            setMembers(response.data);
        } catch (error) {
            console.error('Failed to load roster:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const timer = setTimeout(() => {
            loadMembers();
        }, 300);
        return () => clearTimeout(timer);
    }, [searchTerm, filterFunction, filterLevel]);

    const handleSaveMember = async (data: Partial<RosterMember>) => {
        try {
            if (selectedMember) {
                // Include id in the payload for the update command
                const updatePayload = { ...data, id: selectedMember.id };
                await rosterService.updateMember(selectedMember.id, updatePayload);
                notify.success('Member updated successfully');
            } else {
                await rosterService.createMember(data);
                notify.success('Member created successfully');
            }
            setIsModalOpen(false);
            setSelectedMember(undefined);
            loadMembers();
        } catch (error: any) {
            console.error('Save failed', error);
            const errorMessage = error?.response?.data?.message || error?.message || 'Failed to save member';
            notify.error(errorMessage);
        }
    };

    const handleExport = async () => {
        try {
            const res = await rosterService.exportRoster();

            // Extract filename from Content-Disposition header
            const contentDisposition = res.headers['content-disposition'];
            let filename = 'Roster_Export.xlsx';
            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            // Create blob
            const blob = new Blob([res.data], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });

            // Use the robust download utility
            downloadFile(blob, filename);

            notify.success('Roster exported successfully');
        } catch (error) {
            console.error('Export failed:', error);
            notify.error('Failed to export roster');
        }
    };

    return (
        <div className="roster-premium-container">
            <header className="page-header" style={{ marginBottom: '3rem' }}>
                <div>
                    <h1 style={{ marginBottom: '0.5rem' }}>Resource Roster</h1>
                    <p style={{ color: 'var(--text-muted)' }}>Manage global talent, costs, and availability in real-time.</p>
                </div>
                <div style={{ display: 'flex', gap: '1rem' }}>
                    <input
                        type="file"
                        id="roster-import"
                        style={{ display: 'none' }}
                        onChange={async (e) => {
                            const file = e.target.files?.[0];
                            if (file) {
                                const formData = new FormData();
                                formData.append('file', file);
                                setLoading(true);
                                try {
                                    const res = await rosterService.importRoster(formData);
                                    notify.success(`Successfully processed ${res.data.count} roster entries`);
                                    loadMembers();
                                } catch (err) {
                                    console.error('Import failed', err);
                                    notify.error('Import failed. Please verify the Excel format.');
                                } finally {
                                    setLoading(false);
                                }
                            }
                        }}
                    />
                    <button className="btn-outline-premium" onClick={() => document.getElementById('roster-import')?.click()}>
                        <Plus size={18} />
                        Import Data
                    </button>
                    <button className="btn-outline-premium" onClick={handleExport}>
                        <Download size={18} />
                        Export Data
                    </button>
                    <button className="btn-premium" onClick={() => { setSelectedMember(undefined); setIsModalOpen(true); }}>
                        <Plus size={18} />
                        Add Member
                    </button>
                </div>
            </header>

            <div className="glass-panel" style={{ padding: '1.5rem', marginBottom: '2rem', display: 'flex', gap: '1rem', alignItems: 'center', position: 'relative', zIndex: 10 }}>
                <div className="search-box-premium" style={{ flex: 1, position: 'relative' }}>
                    <Search size={20} style={{ position: 'absolute', left: '1rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                    <input
                        type="text"
                        placeholder="Search for resources by name, SAP code, or role..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && loadMembers()}
                        style={{ background: 'var(--card-bg)', border: '1px solid var(--border-color)', borderRadius: '8px', padding: '1rem 1rem 1rem 3.5rem', width: '100%', color: 'var(--text-primary)', outline: 'none' }}
                    />
                </div>
                <div style={{ position: 'relative' }} ref={filterButtonRef}>
                    <button
                        className={`btn-outline-premium ${isFilterOpen ? 'active' : ''}`}
                        style={{ borderRadius: '8px', background: isFilterOpen ? 'var(--bg-color)' : 'transparent', borderColor: isFilterOpen ? 'var(--deloitte-green)' : 'var(--border-color)', position: 'relative', zIndex: 101 }}
                        onClick={() => {
                            if (!isFilterOpen) {
                                setTempFunction(filterFunction);
                                setTempLevel(filterLevel);
                            }
                            setIsFilterOpen(!isFilterOpen);
                        }}
                    >
                        <Filter size={18} style={{ color: (filterFunction || filterLevel) ? 'var(--deloitte-green)' : 'inherit' }} />
                    </button>

                    {isFilterOpen && createPortal(
                        <>
                            {/* Backdrop */}
                            <div
                                style={{ position: 'fixed', inset: 0, zIndex: 3000, cursor: 'default' }}
                                onClick={() => setIsFilterOpen(false)}
                            />

                            {/* Modal - Portaled to Body */}
                            <div className="glass-panel animate-fade-in" style={{
                                position: 'absolute',
                                top: (filterButtonRef.current?.getBoundingClientRect().bottom ?? 0) + window.scrollY + 8,
                                left: (filterButtonRef.current?.getBoundingClientRect().right ?? 0) + window.scrollX - 300, // Align right edge
                                width: '300px',
                                padding: '1.25rem',
                                zIndex: 3001,
                                boxShadow: '0 10px 40px -5px rgba(0,0,0,0.3)',
                                border: '1px solid var(--border-color)',
                                background: 'var(--bg-color)', // Opaque to prevent transparency issues
                                backdropFilter: 'none'
                            }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                                    <h3 style={{ margin: 0, fontSize: '1rem', fontWeight: 600 }}>Filters</h3>
                                    <button onClick={() => setIsFilterOpen(false)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)' }}>
                                        <X size={16} />
                                    </button>
                                </div>

                                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                                    <div>
                                        <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, color: 'var(--text-muted)', marginBottom: '0.4rem' }}>Function / BU</label>
                                        <PremiumSelect
                                            value={tempFunction}
                                            onChange={setTempFunction}
                                            placeholder="All Functions"
                                            options={[
                                                { value: '', label: 'All Functions' },
                                                ...FUNCTIONS_LOV.map(fn => ({ value: fn, label: fn }))
                                            ]}
                                        />
                                    </div>

                                    <div>
                                        <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, color: 'var(--text-muted)', marginBottom: '0.4rem' }}>Seniority Level</label>
                                        <PremiumSelect
                                            value={tempLevel}
                                            onChange={setTempLevel}
                                            placeholder="All Levels"
                                            options={[
                                                { value: '', label: 'All Levels' },
                                                ...SENIORITY_LEVELS.map(lvl => ({ value: lvl.code, label: lvl.displayName }))
                                            ]}
                                        />
                                    </div>

                                    <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                                        <button
                                            className="btn-outline-premium"
                                            style={{ flex: 1, justifyContent: 'center', fontSize: '0.8rem', padding: '0.5rem' }}
                                            onClick={() => {
                                                setTempFunction('');
                                                setTempLevel('');
                                            }}
                                        >
                                            <RotateCcw size={14} /> Reset
                                        </button>
                                        <button
                                            className="btn-premium"
                                            style={{ flex: 1, justifyContent: 'center', fontSize: '0.8rem', padding: '0.5rem' }}
                                            onClick={() => {
                                                setFilterFunction(tempFunction);
                                                setFilterLevel(tempLevel);
                                                setIsFilterOpen(false);
                                            }}
                                        >
                                            <Check size={14} /> Apply
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </>,
                        document.body
                    )}
                </div>
            </div>

            {loading ? (
                <div className="loading-state" style={{ height: '400px' }}>
                    <Loader2 className="animate-spin" size={48} color="var(--deloitte-green)" />
                    <span style={{ marginTop: '1rem', fontSize: '1.1rem', fontWeight: 500 }}>Aggregating resource data...</span>
                </div>
            ) : (
                <div className="table-wrapper animate-fade-in">
                    <table className="premium-table">
                        <thead>
                            <tr>
                                <th style={{ textAlign: 'left' }}>Resource / Entity</th>
                                <th style={{ textAlign: 'left' }}>SAP Reference</th>
                                <th style={{ textAlign: 'left' }}>Function / BU</th>
                                <th style={{ textAlign: 'left' }}>Seniority</th>
                                <th style={{ textAlign: 'left' }}>M. Cost (12m)</th>
                                <th style={{ textAlign: 'left' }}>M. Cost (14m)</th>
                                <th style={{ textAlign: 'left' }}>Daily Cost</th>
                                <th style={{ textAlign: 'right' }}>Management</th>
                            </tr>
                        </thead>
                        <tbody>
                            {members.map(member => (
                                <tr key={member.id}>
                                    <td>
                                        <div
                                            onClick={() => window.location.href = `/resources/${member.id}`}
                                            className="resource-name-link"
                                        >
                                            {member.fullNameEn}
                                        </div>
                                        <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>{member.legalEntity}</div>
                                    </td>
                                    <td><code style={{ background: 'rgba(134, 188, 37, 0.1)', color: 'var(--text-primary)', padding: '0.2rem 0.5rem', borderRadius: '4px', fontSize: '0.85rem' }}>{member.sapCode}</code></td>
                                    <td>{member.functionBusinessUnit}</td>
                                    <td>
                                        <div className={`level-badge ${getSeniorityBadgeClass(member.level || '')}`}>
                                            {member.level || '-'}
                                        </div>
                                    </td>
                                    <td style={{ fontWeight: 600 }}>€{member.monthlyCost_12months?.toLocaleString('de-DE', { minimumFractionDigits: 2 }) || '0.00'}</td>
                                    <td style={{ fontWeight: 600 }}>€{member.monthlyCost_14months?.toLocaleString('de-DE', { minimumFractionDigits: 2 }) || '0.00'}</td>
                                    <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                                        €{member.dailyCost?.toLocaleString('de-DE', { minimumFractionDigits: 2 }) || '0.00'}
                                    </td>
                                    <td style={{ textAlign: 'right' }}>
                                        <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                                            <button className="btn-icon-premium" onClick={() => { setSelectedMember(member); setIsModalOpen(true); }} style={{ background: 'none', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', padding: '0.5rem' }}>
                                                <Edit2 size={18} />
                                            </button>
                                            <button className="btn-icon-premium" style={{ background: 'none', border: 'none', color: '#ef4444', cursor: 'pointer', padding: '0.5rem' }}>
                                                <Trash2 size={18} />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {isModalOpen && (
                <RosterModal
                    member={selectedMember}
                    onClose={() => setIsModalOpen(false)}
                    onSave={handleSaveMember}
                />
            )}
        </div>
    );
};

export default RosterPage;
