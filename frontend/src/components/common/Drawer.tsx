import React, { useEffect } from 'react';
import { X } from 'lucide-react';
import './Drawer.css';

interface DrawerProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: React.ReactNode;
    footer?: React.ReactNode;
    width?: string;
}

const Drawer: React.FC<DrawerProps> = ({ isOpen, onClose, title, children, footer, width }) => {
    // Prevent scrolling behind drawer
    useEffect(() => {
        if (isOpen) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'unset';
        }
        return () => {
            document.body.style.overflow = 'unset';
        };
    }, [isOpen]);

    return (
        <>
            <div
                className={`drawer-overlay ${isOpen ? 'open' : ''}`}
                onClick={onClose}
            />
            <div
                className={`drawer-container ${isOpen ? 'open' : ''}`}
                style={{ maxWidth: width }}
            >
                <div className="drawer-header">
                    <h2 className="drawer-title">{title}</h2>
                    <button className="drawer-close" onClick={onClose}>
                        <X size={20} />
                    </button>
                </div>

                <div className="drawer-content">
                    {children}
                </div>

                {footer && (
                    <div className="drawer-footer">
                        {footer}
                    </div>
                )}
            </div>
        </>
    );
};

export default Drawer;
