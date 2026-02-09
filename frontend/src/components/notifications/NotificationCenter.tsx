import React, { useRef, useEffect } from 'react';
import { Check, Trash2, Bell, XCircle, CheckCircle2, Info, AlertTriangle } from 'lucide-react';
import { useNotification } from '../../context/NotificationContext';
import './NotificationCenter.css';

interface NotificationCenterProps {
    onClose: () => void;
}

const NotificationCenter: React.FC<NotificationCenterProps> = ({ onClose }) => {
    const { notifications, markAllAsRead, clearAll, markAsRead } = useNotification();
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (panelRef.current && !panelRef.current.contains(event.target as Node)) {
                // Ensure the click wasn't on the bell trigger itself (handled by parent usually, but safe to close)
                onClose();
            }
        };
        // Small delay to prevent immediate close if the click that opened it bubbles
        const timer = setTimeout(() => document.addEventListener('mousedown', handleClickOutside), 10);
        return () => {
            clearTimeout(timer);
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [onClose]);

    const formatTime = (timestamp: number) => {
        const diff = Date.now() - timestamp;
        if (diff < 60000) return 'Just now';
        if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`;
        if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`;
        return new Date(timestamp).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    const getIcon = (type: string) => {
        switch (type) {
            case 'success': return <CheckCircle2 size={16} className="nc-icon success" />;
            case 'error': return <XCircle size={16} className="nc-icon error" />;
            case 'warning': return <AlertTriangle size={16} className="nc-icon warning" />;
            case 'info': return <Info size={16} className="nc-icon info" />;
            default: return <Info size={16} className="nc-icon info" />;
        }
    };

    return (
        <div className="notification-center-panel" ref={panelRef}>
            <header className="nc-header">
                <h3>Notifications</h3>
                <div className="nc-actions">
                    <button className="nc-action-btn" onClick={markAllAsRead} title="Mark all read">
                        <Check size={16} />
                    </button>
                    <button className="nc-action-btn" onClick={clearAll} title="Clear all">
                        <Trash2 size={16} />
                    </button>
                </div>
            </header>

            {notifications.length === 0 ? (
                <div className="nc-empty">
                    <Bell size={32} style={{ marginBottom: '1rem', opacity: 0.2 }} />
                    <p>No notifications yet</p>
                </div>
            ) : (
                <ul className="nc-list">
                    {notifications.map(item => (
                        <li
                            key={item.id}
                            className={`nc-item ${!item.read ? 'unread' : ''}`}
                            onClick={() => markAsRead(item.id)}
                        >
                            {getIcon(item.type)}
                            <div className="nc-content">
                                <span className="nc-message">{item.message}</span>
                                {item.description && <span className="nc-description">{item.description}</span>}
                                <span className="nc-time">{formatTime(item.timestamp)}</span>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default NotificationCenter;
