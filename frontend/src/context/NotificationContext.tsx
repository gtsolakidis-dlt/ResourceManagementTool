import React, { createContext, useContext, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import StatusCapsule from '../components/notifications/StatusCapsule';

type NotificationType = 'success' | 'error' | 'info' | 'warning';

export interface NotificationItem {
    id: string;
    type: NotificationType;
    message: string;
    description?: string;
    timestamp: number;
    read: boolean;
}

interface NotificationContextProps {
    notifications: NotificationItem[];
    unreadCount: number;
    notify: {
        success: (msg: string, desc?: string) => void;
        error: (msg: string, desc?: string) => void;
        info: (msg: string, desc?: string) => void;
        warning: (msg: string, desc?: string) => void;
        loading: (msg: string) => string; // Returns id
        dismiss: (id: string) => void;
    };
    markAsRead: (id: string) => void;
    markAllAsRead: () => void;
    clearAll: () => void;
}

const NotificationContext = createContext<NotificationContextProps | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [notifications, setNotifications] = useState<NotificationItem[]>([]);

    const addHistory = (type: NotificationType, message: string, description?: string) => {
        const newItem: NotificationItem = {
            id: Date.now().toString() + Math.random(),
            type,
            message,
            description,
            timestamp: Date.now(),
            read: false
        };
        setNotifications(prev => [newItem, ...prev].slice(0, 50));
    };

    const notify = {
        success: (message: string, description?: string) => {
            addHistory('success', message, description);
            toast.custom((t) => <StatusCapsule type="success" message={message} description={description} visible={t.visible} />, { duration: 3000, position: 'top-center' });
        },
        error: (message: string, description?: string) => {
            addHistory('error', message, description);
            toast.custom((t) => <StatusCapsule type="error" message={message} description={description} visible={t.visible} />, { duration: 4000, position: 'top-center' });
        },
        info: (message: string, description?: string) => {
            addHistory('info', message, description);
            toast.custom((t) => <StatusCapsule type="info" message={message} description={description} visible={t.visible} />, { duration: 3000, position: 'top-center' });
        },
        warning: (message: string, description?: string) => {
            addHistory('warning', message, description);
            toast.custom((t) => <StatusCapsule type="warning" message={message} description={description} visible={t.visible} />, { duration: 4000, position: 'top-center' });
        },
        loading: (message: string) => {
            // Loading usually doesn't need history until it resolves
            return toast.custom((t) => <StatusCapsule type="loading" message={message} visible={t.visible} />, { duration: Infinity, position: 'top-center' });
        },
        dismiss: (id: string) => toast.dismiss(id)
    };

    const markAsRead = (id: string) => {
        setNotifications(prev => prev.map(n => n.id === id ? { ...n, read: true } : n));
    };
    const markAllAsRead = () => {
        setNotifications(prev => prev.map(n => ({ ...n, read: true })));
    };
    const clearAll = () => setNotifications([]);

    const unreadCount = notifications.filter(n => !n.read).length;

    return (
        <NotificationContext.Provider value={{ notifications, unreadCount, notify, markAsRead, markAllAsRead, clearAll }}>
            {children}
            <Toaster
                containerStyle={{
                    top: 20
                }}
            />
        </NotificationContext.Provider>
    );
};

export const useNotification = () => {
    const context = useContext(NotificationContext);
    if (!context) throw new Error('useNotification must be used within NotificationProvider');
    return context;
};
