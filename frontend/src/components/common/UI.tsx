import React from 'react';

export const PageHeader: React.FC<{ title: string; children?: React.ReactNode }> = ({ title, children }) => (
    <div className="page-header">
        <h1>{title}</h1>
        <div className="page-actions">
            {children}
        </div>
    </div>
);

export const Card: React.FC<{ title?: string; children: React.ReactNode; className?: string }> = ({ title, children, className }) => (
    <div className={`card ${className || ''}`}>
        {title && <h3 className="card-title">{title}</h3>}
        {children}
    </div>
);
