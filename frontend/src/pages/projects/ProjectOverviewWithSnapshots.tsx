import React, { useState, useEffect } from 'react';
import { snapshotService, projectService } from '../../api/services';
import type { ProjectMonthlySnapshot, Project } from '../../types';
import { Lock, Check, Loader2, Save, RotateCcw, Edit2, X } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { useNotification } from '../../context/NotificationContext';
import PremiumNumericInput from '../../components/common/PremiumNumericInput';
import './ProjectOverviewWithSnapshots.css';

interface Props {
    projectId: number;
    forecastVersionId: number;
}

const formatCurrency = (value: number): string => {
    return value.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

const formatPercent = (value: number): string => {
    return (value * 100).toFixed(1) + '%';
};

const ProjectOverviewWithSnapshots: React.FC<Props> = ({ projectId, forecastVersionId }) => {
    const { user } = useAuth();
    const { notify } = useNotification();
    const [snapshots, setSnapshots] = useState<ProjectMonthlySnapshot[]>([]);
    const [project, setProject] = useState<Project | null>(null);
    const [editableMonth, setEditableMonth] = useState<ProjectMonthlySnapshot | null>(null);
    const [editedValues, setEditedValues] = useState<Partial<ProjectMonthlySnapshot>>({});
    const [loading, setLoading] = useState(true);
    const [isEditMode, setIsEditMode] = useState(false);
    const [confirming, setConfirming] = useState(false);
    const [saving, setSaving] = useState(false);
    const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
    const [lastSavedAt, setLastSavedAt] = useState<Date | null>(null);
    const [showConfirmDialog, setShowConfirmDialog] = useState(false);
    const [showDiffDialog, setShowDiffDialog] = useState(false);
    const [showResetDialog, setShowResetDialog] = useState(false);
    const [hoveredTooltip, setHoveredTooltip] = useState<{ x: number; y: number; content: React.ReactNode } | null>(null);

    useEffect(() => {
        loadSnapshots();
    }, [projectId, forecastVersionId]);

    const loadSnapshots = async () => {
        try {
            setLoading(true);
            setIsEditMode(false);
            const [snapshotsRes, projectRes] = await Promise.all([
                snapshotService.getSnapshots(projectId, forecastVersionId),
                projectService.getProject(projectId)
            ]);

            // Sort by date to ensure correct order
            const sorted = (snapshotsRes.data || []).sort((a, b) => new Date(a.month).getTime() - new Date(b.month).getTime());
            setSnapshots(sorted);
            setProject(projectRes.data);

            const editable = sorted.find(s => s.status === 'Editable') || null;
            setEditableMonth(editable);
            if (editable) {
                setEditedValues({
                    openingBalance: editable.openingBalance,
                    wip: editable.wip,
                    directExpenses: editable.directExpenses,
                    operationalCost: editable.operationalCost,
                    cumulativeBillings: editable.cumulativeBillings
                });
            }
            setHasUnsavedChanges(false);
        } catch (error) {
            notify.error('Failed to load project data');
        } finally {
            setLoading(false);
        }
    };

    const handleConfirmClick = () => {
        if (hasUnsavedChanges) {
            notify.error('Please save your changes before confirming the month');
            return;
        }
        setShowConfirmDialog(true);
    };

    const handleConfirm = async () => {
        if (!editableMonth) return;

        try {
            setConfirming(true);
            setShowConfirmDialog(false);
            await snapshotService.confirmMonth({
                projectId,
                forecastVersionId,
                month: editableMonth.month,
                confirmedBy: user?.username || 'Unknown'
            });
            notify.success('Month confirmed and locked successfully');
            loadSnapshots();
        } catch (error: any) {
            notify.error(error.response?.data?.message || 'Failed to confirm month');
        } finally {
            setConfirming(false);
        }
    };

    const getChangedFields = () => {
        if (!editableMonth) return [];
        const changes: Array<{ field: string; oldValue: number; newValue: number }> = [];

        const fields: Array<keyof ProjectMonthlySnapshot> = [
            'openingBalance', 'wip', 'directExpenses', 'operationalCost', 'cumulativeBillings'
        ];

        fields.forEach(field => {
            const newVal = editedValues[field];
            const oldVal = editableMonth[field];
            if (newVal !== undefined && newVal !== oldVal) {
                changes.push({
                    field: field.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase()),
                    oldValue: oldVal as number,
                    newValue: newVal as number
                });
            }
        });

        return changes;
    };

    const handleSaveClick = () => {
        const changes = getChangedFields();
        if (changes.length === 0) {
            notify.error('No changes to save');
            return;
        }
        setShowDiffDialog(true);
    };

    const handleSave = async () => {
        if (!editableMonth) return;
        setSaving(true);
        setShowDiffDialog(false);
        try {
            await snapshotService.overwriteSnapshot({
                projectId,
                forecastVersionId,
                month: editableMonth.month,
                overriddenBy: user?.username || 'Unknown',
                // Only send editable fields - exclude NSR and Margin
                openingBalance: editedValues.openingBalance ?? editableMonth.openingBalance,
                wip: editedValues.wip ?? editableMonth.wip,
                directExpenses: editedValues.directExpenses ?? editableMonth.directExpenses,
                operationalCost: editedValues.operationalCost ?? editableMonth.operationalCost,
                cumulativeBillings: editedValues.cumulativeBillings ?? editableMonth.cumulativeBillings
            });
            notify.success('Changes saved successfully');
            setLastSavedAt(new Date());
            setHasUnsavedChanges(false);
            loadSnapshots();
        } catch (error: any) {
            notify.error(error.response?.data?.message || 'Failed to save changes');
        } finally {
            setSaving(false);
        }
    };

    const handleReset = async () => {
        if (!editableMonth) return;

        // Case 1: If there are unsaved changes, just discard them
        if (hasUnsavedChanges) {
            setEditedValues({
                openingBalance: editableMonth.openingBalance,
                wip: editableMonth.wip,
                directExpenses: editableMonth.directExpenses,
                operationalCost: editableMonth.operationalCost,
                cumulativeBillings: editableMonth.cumulativeBillings
            });
            setHasUnsavedChanges(false);
            notify.success('Unsaved changes discarded');
            return;
        }

        // Case 2: If the month is overridden (saved), show confirmation dialog
        if (editableMonth.isOverridden) {
            setShowResetDialog(true);
        } else {
            notify.success('No changes to reset');
        }
    };

    const confirmResetOverrides = async () => {
        if (!editableMonth) return;
        setShowResetDialog(false);
        try {
            setSaving(true);
            await snapshotService.clearOverride(
                projectId,
                forecastVersionId,
                editableMonth.month
            );

            notify.success('Overrides cleared - values restored to calculated state');
            await loadSnapshots();
        } catch (error: any) {
            notify.error(error.response?.data?.message || 'Failed to clear overrides');
        } finally {
            setSaving(false);
        }
    };

    const cancelResetOverrides = () => {
        setShowResetDialog(false);
    };

    const handleValueChange = (key: keyof ProjectMonthlySnapshot, value: number) => {
        setEditedValues(prev => {
            const newValues = { ...prev, [key]: value };

            // Check if the new values differ from the original snapshot
            const changed = editableMonth && (
                (newValues.openingBalance !== undefined && newValues.openingBalance !== editableMonth.openingBalance) ||
                (newValues.wip !== undefined && newValues.wip !== editableMonth.wip) ||
                (newValues.directExpenses !== undefined && newValues.directExpenses !== editableMonth.directExpenses) ||
                (newValues.operationalCost !== undefined && newValues.operationalCost !== editableMonth.operationalCost) ||
                (newValues.cumulativeBillings !== undefined && newValues.cumulativeBillings !== editableMonth.cumulativeBillings)
            );

            setHasUnsavedChanges(!!changed);
            return newValues;
        });
    };

    const getColumnClass = (snapshot: ProjectMonthlySnapshot) => {
        if (snapshot.status === 'Editable') return 'editable-col';
        if (snapshot.status === 'Pending') return 'pending-col';
        return '';
    };

    if (loading) {
        return (
            <div className="snapshot-loading">
                <Loader2 size={24} className="animate-spin" />
                <span>Loading monthly overview...</span>
            </div>
        );
    }

    if (snapshots.length === 0) {
        return (
            <div className="snapshot-empty">
                <p>No monthly snapshots available for this project.</p>
            </div>
        );
    }

    // Metrics configuration for the rows
    const metrics = [
        { label: 'Opening Balance', key: 'openingBalance', originalKey: 'originalOpeningBalance', format: formatCurrency, editable: true },
        { label: 'Work in Progress (WIP)', key: 'wip', originalKey: 'originalWip', format: formatCurrency, editable: true },
        { label: 'Cumulative Billings', key: 'cumulativeBillings', originalKey: 'originalCumulativeBillings', format: formatCurrency, editable: true },
        { label: 'Direct Expenses', key: 'directExpenses', originalKey: 'originalDirectExpenses', format: formatCurrency, editable: true },
        { label: 'Operational Cost', key: 'operationalCost', originalKey: 'originalOperationalCost', format: formatCurrency, editable: true },
        { label: 'NET SERVICE REVENUE (NSR)', key: 'nsr', format: formatCurrency, highlight: true, editable: false, isSummary: true },
        { label: 'MARGIN PERFORMANCE %', key: 'margin', format: formatPercent, highlight: true, editable: false, step: 0.01, isSummary: true },
    ];

    return (
        <div className="snapshot-overview">
            <div className="snapshot-header">
                <h2>Monthly Financial Overview</h2>
            </div>

            <div className="snapshot-table-container">
                <table className="premium-table snapshot-table">
                    <thead>
                        <tr>
                            <th style={{ minWidth: '180px' }}>FINANCIAL KPIS</th>
                            {snapshots.map(snapshot => (
                                <th key={snapshot.id} className={snapshot.status === 'Editable' ? 'editable-col' : ''} style={{ textAlign: 'right', minWidth: '140px' }}>
                                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: '0.5rem' }}>
                                        {snapshot.status === 'Confirmed' && <Lock size={12} />}
                                        {snapshot.status === 'Editable' && (
                                            <button
                                                onClick={() => setIsEditMode(!isEditMode)}
                                                className="edit-toggle-btn"
                                                style={{ border: 'none', background: 'transparent', cursor: 'pointer', color: 'var(--deloitte-green)', padding: '2px' }}
                                                title={isEditMode ? "Cancel Editing" : "Edit Month"}
                                            >
                                                {isEditMode ? <X size={14} /> : <Edit2 size={14} />}
                                            </button>
                                        )}
                                        {new Date(snapshot.month).toLocaleDateString('en-US', { month: 'short', year: '2-digit' }).toUpperCase()}
                                        {snapshot.status === 'Editable' && <span style={{ fontSize: '0.65rem', fontWeight: 'normal' }}>(EDITABLE)</span>}
                                    </div>
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {metrics.map((metric) => {
                            const metricKey = metric.key as keyof ProjectMonthlySnapshot;
                            // @ts-ignore
                            const isSummary = metric.isSummary;

                            return (
                                <tr key={metric.key} className={isSummary ? 'summary-row' : ''}>
                                    <td className={metric.highlight ? 'highlight' : ''} style={{ fontWeight: 500 }}>{metric.label}</td>
                                    {snapshots.map(snapshot => {
                                        const isEditableCell = snapshot.status === 'Editable' && metric.editable && isEditMode;

                                        // Specific change detection
                                        // @ts-ignore
                                        const originalKey = metric.originalKey as keyof ProjectMonthlySnapshot | undefined;
                                        let hasChanged = false;

                                        if (snapshot.isOverridden && originalKey) {
                                            const orig = snapshot[originalKey] as number | undefined;
                                            const curr = snapshot[metricKey] as number;
                                            // Only show if we have an original value and it differs
                                            if (orig !== undefined && orig !== null && Math.abs(curr - orig) > 0.001) {
                                                hasChanged = true;
                                            }
                                        }

                                        // Conditional formatting
                                        let cellClass = `numeric ${getColumnClass(snapshot)}`;

                                        // Apply pending-col class for summary rows in pending state
                                        if (isSummary && snapshot.status === 'Pending') {
                                            cellClass += ' pending-col';
                                        }
                                        if (metricKey === 'margin' && (snapshot[metricKey] as number) < 0) {
                                            cellClass += ' negative-margin';
                                        }
                                        if (isSummary) {
                                            cellClass += ' highlight';
                                        }

                                        if (metric.key === 'margin' && project) {
                                            const marginVal = snapshot.margin;
                                            const targetVal = project.targetMargin > 1 ? project.targetMargin / 100 : project.targetMargin;
                                            if (marginVal < targetVal) {
                                                cellClass += ' negative-margin';
                                            }
                                        } else if (metric.key === 'nsr') {
                                            // Check if negative
                                            if ((snapshot.nsr || 0) < 0) {
                                                cellClass += ' negative-margin';
                                            }
                                        }

                                        return (
                                            <td
                                                key={`${metric.key}-${snapshot.id}`}
                                                className={cellClass}
                                                style={{ padding: isEditableCell ? '4px 8px' : undefined, position: 'relative' }}
                                            >
                                                {isEditableCell ? (
                                                    <PremiumNumericInput
                                                        value={(editedValues[metricKey] as number) ?? 0}
                                                        onChange={(val) => handleValueChange(metricKey, val)}
                                                        step={metric.step || 1}
                                                        hideControls={true}
                                                        style={{ width: '100%', fontSize: '0.85rem' }}
                                                    />
                                                ) : (
                                                    <>
                                                        {/* @ts-ignore dynamic access */}
                                                        {metric.format ? metric.format(snapshot[metricKey]) : snapshot[metricKey]}
                                                        {hasChanged && metric.editable && (
                                                            <div
                                                                className="override-indicator"
                                                                onMouseEnter={(e) => {
                                                                    const rect = e.currentTarget.getBoundingClientRect();
                                                                    setHoveredTooltip({
                                                                        x: rect.left + rect.width / 2,
                                                                        y: rect.top,
                                                                        content: (
                                                                            <div className="tooltip-content">
                                                                                <div className="tooltip-header">Manually Overwritten</div>
                                                                                <div className="tooltip-detail">
                                                                                    By <span className="tooltip-highlight">{snapshot.overriddenBy || 'User'}</span>
                                                                                </div>
                                                                                <div className="tooltip-date">
                                                                                    {snapshot.overriddenAt ? new Date(snapshot.overriddenAt).toLocaleDateString() : 'Unknown Date'}
                                                                                </div>
                                                                                <div className="tooltip-detail" style={{ marginTop: '5px', fontSize: '0.75rem' }}>
                                                                                    Original: {formatCurrency(snapshot[originalKey!] as number)}
                                                                                </div>
                                                                            </div>
                                                                        )
                                                                    });
                                                                }}
                                                                onMouseLeave={() => setHoveredTooltip(null)}
                                                            />
                                                        )}
                                                    </>
                                                )}
                                            </td>
                                        );
                                    })}
                                </tr>
                            );
                        })}

                        {/* Actions Row - Only render when edit mode is active */}
                        {isEditMode && (
                            <tr className="actions-row">
                                <td></td>
                                {snapshots.map(snapshot => (
                                    <td key={`action-${snapshot.id}`} className={snapshot.status === 'Editable' ? 'editable-col' : ''} style={{ textAlign: 'right', verticalAlign: 'top' }}>
                                        {snapshot.status === 'Editable' && (
                                            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', alignItems: 'flex-end', paddingTop: '0.5rem' }}>
                                                {lastSavedAt && (
                                                    <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', fontStyle: 'italic' }}>
                                                        Last saved: {lastSavedAt.toLocaleTimeString()}
                                                    </div>
                                                )}
                                                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', width: '100%' }}>
                                                    <button
                                                        className="btn-outline-premium btn-sm"
                                                        onClick={handleSaveClick}
                                                        disabled={saving || !hasUnsavedChanges}
                                                        style={{ width: '100%', justifyContent: 'center' }}
                                                    >
                                                        {saving ? <Loader2 size={14} className="animate-spin" /> : <Save size={14} />}
                                                        Save
                                                    </button>
                                                    <button
                                                        className="btn-reset btn-sm"
                                                        onClick={handleReset}
                                                        disabled={saving || (!hasUnsavedChanges && !snapshot.isOverridden)}
                                                        style={{ width: '100%', justifyContent: 'center' }}
                                                        title={hasUnsavedChanges ? 'Discard unsaved changes' : (snapshot.isOverridden ? 'Clear overrides and restore calculated values' : 'No changes to reset')}
                                                    >
                                                        <RotateCcw size={14} />
                                                        Reset
                                                    </button>
                                                    <button
                                                        className="btn-premium btn-sm"
                                                        onClick={handleConfirmClick}
                                                        disabled={confirming || saving || hasUnsavedChanges}
                                                        style={{ width: '100%', justifyContent: 'center' }}
                                                        title={hasUnsavedChanges ? 'Save changes before confirming' : 'Lock this month permanently'}
                                                    >
                                                        {confirming ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
                                                        Confirm
                                                    </button>
                                                </div>
                                            </div>
                                        )}
                                    </td>
                                ))}
                            </tr>
                        )}

                    </tbody>
                </table>
            </div>

            {/* Diff Dialog */}
            {showDiffDialog && (
                <div className="modal-overlay" onClick={() => setShowDiffDialog(false)}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '500px' }}>
                        <h3 style={{ marginBottom: '1rem' }}>Confirm Changes</h3>
                        <p style={{ marginBottom: '1rem', color: 'var(--text-secondary)' }}>
                            You are about to save the following changes:
                        </p>
                        <div style={{ background: 'var(--surface-secondary)', padding: '1rem', borderRadius: '8px', marginBottom: '1.5rem' }}>
                            {getChangedFields().map((change, idx) => (
                                <div key={idx} style={{ marginBottom: '0.75rem', fontSize: '0.9rem' }}>
                                    <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{change.field}</div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-secondary)' }}>
                                        <span style={{ textDecoration: 'line-through' }}>{formatCurrency(change.oldValue)}</span>
                                        <span>→</span>
                                        <span style={{ color: 'var(--deloitte-green)', fontWeight: 600 }}>{formatCurrency(change.newValue)}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <div className="modal-actions">
                            <button className="btn-secondary" onClick={() => setShowDiffDialog(false)}>
                                Cancel
                            </button>
                            <button className="btn-premium" onClick={handleSave} disabled={saving}>
                                {saving ? <Loader2 size={16} className="animate-spin" /> : 'Confirm Save'}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Confirm Month Dialog */}
            {showConfirmDialog && editableMonth && (
                <div className="modal-overlay" onClick={() => setShowConfirmDialog(false)}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '500px' }}>
                        <h3 style={{ marginBottom: '1rem', color: 'var(--text-primary)' }}>Lock Month Permanently?</h3>
                        <p style={{ marginBottom: '1rem', color: 'var(--text-secondary)' }}>
                            Confirming <strong>{new Date(editableMonth.month).toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}</strong> will:
                        </p>
                        <ul style={{ marginBottom: '1.5rem', paddingLeft: '1.5rem', color: 'var(--text-secondary)' }}>
                            <li style={{ marginBottom: '0.5rem' }}>Lock all values permanently</li>
                            <li style={{ marginBottom: '0.5rem' }}>Unlock the next month for editing</li>
                            <li style={{ marginBottom: '0.5rem', color: '#ef4444', fontWeight: 600 }}>This action CANNOT be undone</li>
                        </ul>
                        <div className="modal-actions">
                            <button className="btn-secondary" onClick={() => setShowConfirmDialog(false)}>
                                Cancel
                            </button>
                            <button className="btn-premium" onClick={handleConfirm} disabled={confirming} style={{ background: '#ef4444' }}>
                                {confirming ? <Loader2 size={16} className="animate-spin" /> : 'Yes, Lock Month'}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Reset Confirmation Dialog */}
            {showResetDialog && editableMonth && (
                <div className="modal-overlay" onClick={cancelResetOverrides}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '500px' }}>
                        <h3 style={{ marginBottom: '1rem', color: 'var(--text-primary)' }}>⚠️ Clear Manual Overrides?</h3>
                        <p style={{ marginBottom: '1rem', color: 'var(--text-secondary)' }}>
                            This will clear all manual overrides for <strong>{new Date(editableMonth.month).toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}</strong> and restore calculated values.
                        </p>
                        <ul style={{ marginBottom: '1.5rem', paddingLeft: '1.5rem', color: 'var(--text-secondary)' }}>
                            <li style={{ marginBottom: '0.5rem' }}>All manually entered values will be removed</li>
                            <li style={{ marginBottom: '0.5rem' }}>Values will be recalculated from source data</li>
                            <li style={{ marginBottom: '0.5rem', color: '#f59e0b', fontWeight: 600 }}>You can re-enter values after this action</li>
                        </ul>
                        <div className="modal-actions">
                            <button className="btn-secondary" onClick={cancelResetOverrides}>
                                Cancel
                            </button>
                            <button className="btn-reset" onClick={confirmResetOverrides} disabled={saving} style={{ padding: '0.5rem 1rem' }}>
                                {saving ? <Loader2 size={16} className="animate-spin" /> : <>< RotateCcw size={16} /> Clear Overrides</>}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="snapshot-legend">
                <span className="legend-item">
                    <span className="legend-color pending"></span> Pending
                </span>
                <span className="legend-item">
                    <span className="legend-color editable"></span> Editable
                </span>
                <span className="legend-item">
                    <span className="legend-color confirmed"></span> Confirmed
                </span>
                <span className="legend-item">
                    <span style={{ color: 'var(--deloitte-green)', fontSize: '1.2rem', lineHeight: 1 }}>●</span> Manually Overwritten
                </span>
            </div>

            {/* Premium Tooltip Portal */}
            {hoveredTooltip && (
                <div
                    className="premium-tooltip-portal animate-fade-in"
                    style={{
                        position: 'fixed',
                        top: hoveredTooltip.y,
                        left: hoveredTooltip.x,
                        transform: 'translate(-50%, -100%) translateY(-12px)',
                        zIndex: 9999,
                        pointerEvents: 'none'
                    }}
                >
                    {hoveredTooltip.content}
                </div>
            )}
        </div>
    );
};

export default ProjectOverviewWithSnapshots;
