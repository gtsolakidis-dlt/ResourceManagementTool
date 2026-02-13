import React from 'react';
import type { Project } from '../../types';
import { Target, PieChart, Wallet } from 'lucide-react';

interface Props {
    project: Project;
}

const ProjectContextStrip: React.FC<Props> = ({ project }) => {
    const formatCurrency = (val: number) => {
        if (val >= 1000000) return `€${(val / 1000000).toFixed(2)}M`;
        if (val >= 1000) return `€${(val / 1000).toFixed(1)}k`;
        return `€${val.toFixed(0)}`;
    };

    const recoverability = ((1 - (project.discount > 1 ? project.discount / 100 : project.discount)) * 100).toFixed(0);
    const targetMargin = project.targetMargin > 1 ? project.targetMargin.toFixed(0) : (project.targetMargin * 100).toFixed(0);

    return (
        <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '4rem',
            padding: '0.75rem 1.5rem',
            background: 'rgba(255, 255, 255, 0.05)',
            borderRadius: '8px',
            border: '1px solid var(--border-color)',
            marginBottom: '2rem',
            fontSize: '0.875rem',
            color: 'var(--text-secondary)'
        }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <Wallet size={14} style={{ color: 'var(--deloitte-green)' }} />
                <span>Budget: <strong style={{ color: 'var(--text-primary)' }}>{formatCurrency(project.actualBudget)}</strong></span>
            </div>
            <div style={{ width: '1px', height: '16px', background: 'var(--border-color)' }}></div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <PieChart size={14} style={{ color: 'var(--deloitte-green)' }} />
                <span>Recoverability: <strong style={{ color: 'var(--text-primary)' }}>{recoverability}%</strong></span>
            </div>
            <div style={{ width: '1px', height: '16px', background: 'var(--border-color)' }}></div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <Target size={14} style={{ color: 'var(--deloitte-green)' }} />
                <span>Target Margin: <strong style={{ color: 'var(--text-primary)' }}>{targetMargin}%</strong></span>
            </div>
        </div>
    );
};

export default ProjectContextStrip;
