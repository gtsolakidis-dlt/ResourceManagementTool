import React from 'react';
import { CheckCircle2, XCircle, Info, AlertTriangle, Loader2 } from 'lucide-react';
// import { Toast } from 'react-hot-toast'; // Not strictly needed for types if we pass generic props but good for animation control
import './StatusCapsule.css';

interface StatusCapsuleProps {
    type: 'success' | 'error' | 'info' | 'warning' | 'loading';
    message: string;
    description?: string;
    visible?: boolean;
}

const StatusCapsule: React.FC<StatusCapsuleProps> = ({ type, message, description, visible = true }) => {
    const getIcon = () => {
        switch (type) {
            case 'success': return <CheckCircle2 size={20} className="status-capsule-icon success" />;
            case 'error': return <XCircle size={20} className="status-capsule-icon error" />;
            case 'info': return <Info size={20} className="status-capsule-icon info" />;
            case 'warning': return <AlertTriangle size={18} className="status-capsule-icon warning" />;
            case 'loading': return <Loader2 size={18} className="status-capsule-icon info animate-spin" />;
        }
    };

    return (
        <div className={`status-capsule ${visible ? 'animate-enter' : 'animate-leave'}`}>
            {getIcon()}
            <div className="status-capsule-content">
                <span className="status-capsule-title">{message}</span>
                {description && <span className="status-capsule-message">{description}</span>}
            </div>
        </div>
    );
};

export default StatusCapsule;
