import React, { useEffect } from 'react';
import { NavLink, Link, useLocation } from 'react-router-dom';
import {
    LayoutDashboard,
    Users,
    Settings,
    ChevronRight,
    Search,
    Bell,
    Sun,
    Moon,
    BarChart3,
    LogOut,
    FolderKanban // Retained as it's used in the original code
} from 'lucide-react';
import './Layout.css';
import { useNavigation } from '../../context/NavigationContext';
import { useTheme } from '../../context/ThemeContext';
import NotificationCenter from '../notifications/NotificationCenter';
import { useNotification } from '../../context/NotificationContext';
import CommandPalette from '../common/CommandPalette';
import { useAuth } from '../../context/AuthContext';
import { TutorialManager } from '../tutorial/TutorialManager';

interface LayoutProps {
    children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
    const location = useLocation();
    const { breadcrumbs, activeSection, sidebarSubItems, setSidebarSubItems, setActiveSection } = useNavigation();
    const { theme, toggleTheme } = useTheme();
    const { unreadCount } = useNotification();
    const { user, logout } = useAuth();

    const [isNotificationOpen, setIsNotificationOpen] = React.useState(false);

    const [isProjectExpanded, setIsProjectExpanded] = React.useState(true);
    // Initialize settings expansion based on current path
    const [isSettingsExpanded, setIsSettingsExpanded] = React.useState(() => location.pathname.startsWith('/admin'));

    const childrenWrapperRef = React.useRef<HTMLDivElement>(null);
    const [indicatorStyle, setIndicatorStyle] = React.useState<React.CSSProperties>({});

    // Helper to determine if a main link is active
    const isLinkActive = (path: string) => {
        if (path === '/') return location.pathname === '/';
        return location.pathname.startsWith(path);
    };

    // Auto-Close Sub-Menues when navigation away from their section
    useEffect(() => {
        // Projects logic
        if (!location.pathname.startsWith('/projects')) {
            if (activeSection === 'projects') {
                setSidebarSubItems([]);
                setActiveSection('');
            }
        }

        // Settings logic: collapse when navigating away
        if (!location.pathname.startsWith('/admin')) {
            setIsSettingsExpanded(false);
        }
    }, [location.pathname, activeSection, setSidebarSubItems, setActiveSection]);

    // Traveling Indicator Logic
    useEffect(() => {
        if (!childrenWrapperRef.current) return;
        const timer = setTimeout(() => {
            const activeLink = childrenWrapperRef.current?.querySelector('a.sub-nav-link.active') as HTMLElement;
            if (activeLink) {
                // Calculate top relative to wrapper
                setIndicatorStyle({
                    top: `${activeLink.offsetTop + (activeLink.offsetHeight - 16) / 2}px`,
                    height: '16px',
                    opacity: 1
                });
            } else {
                setIndicatorStyle({ opacity: 0 });
            }
        }, 50); // Delay for render/transition
        return () => clearTimeout(timer);
    }, [location.pathname, isProjectExpanded, sidebarSubItems]);

    return (
        <div className="layout-root">
            {/* Sidebar */}
            <aside className="sidebar-premium">
                <div className="sidebar-header">
                    <div className="brand-dot"></div>
                    <span className="brand-name">Resourcely</span>
                </div>

                <div className="nav-section">
                    <div className="nav-label">Core Platform</div>

                    <NavLink to="/" className={({ isActive }) => `nav-link-premium ${isActive ? 'active' : ''}`} id="nav-dashboard">
                        <LayoutDashboard size={20} />
                        <span>Dashboard</span>
                    </NavLink>

                    <NavLink to="/roster" className={() => `nav-link-premium ${isLinkActive('/roster') ? 'active' : ''}`} id="nav-roster">
                        <Users size={20} />
                        <span>Resource Roster</span>
                    </NavLink>

                    <div style={{ position: 'relative' }}>
                        <NavLink to="/projects" className={() => `nav-link-premium ${isLinkActive('/projects') ? 'active' : ''}`} id="nav-projects">
                            <FolderKanban size={20} />
                            <span>Projects</span>
                        </NavLink>

                        {/* Dynamic Sub-items with refined hierarchy */}
                        {activeSection === 'projects' && sidebarSubItems.length > 0 && (
                            <div className="sub-nav-container">

                                {sidebarSubItems.filter(i => i.isHeader).map((item, index) => (
                                    <div key={`head-${index}`} onClick={() => setIsProjectExpanded(!isProjectExpanded)} style={{ cursor: 'pointer' }}>
                                        <NavLink
                                            to={item.path}
                                            className={({ isActive }) => `sub-nav-header ${isActive ? 'active' : ''}`}
                                            end={item.exact ?? true}
                                            style={{ display: 'flex', alignItems: 'center', paddingLeft: '3rem' }}
                                        >
                                            <span style={{ flex: 1 }}>{item.label}</span>
                                            <div style={{ marginRight: '1rem', transition: 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)', transform: isProjectExpanded ? 'rotate(90deg)' : 'rotate(0deg)', display: 'flex', alignItems: 'center' }}>
                                                <ChevronRight size={16} color="var(--text-muted)" />
                                            </div>
                                        </NavLink>
                                    </div>
                                ))}

                                <div className={`sub-nav-children-wrapper ${isProjectExpanded ? 'expanded' : ''}`} ref={childrenWrapperRef}>
                                    <div className="traveling-indicator" style={indicatorStyle} />
                                    {sidebarSubItems.filter(i => !i.isHeader).map((item, index) => (
                                        <NavLink
                                            key={index}
                                            to={item.path}
                                            className={({ isActive }) => `sub-nav-link ${isActive ? 'active' : ''}`}
                                            end={item.exact ?? true}
                                        >
                                            <span>{item.label}</span>
                                        </NavLink>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <NavLink to="/analytics" className={({ isActive }) => `nav-link-premium ${isActive ? 'active' : ''}`} id="nav-analytics">
                        <BarChart3 size={20} />
                        <span>Analytics</span>
                    </NavLink>
                </div>

                <div className="nav-section" style={{ marginTop: 'auto', flex: 0 }}>
                    <div className="nav-label">System</div>
                    <div style={{ position: 'relative' }}>
                        <div
                            onClick={() => setIsSettingsExpanded(!isSettingsExpanded)}
                            className={`nav-link-premium ${isLinkActive('/admin') ? 'active' : ''}`}
                            style={{ cursor: 'pointer' }}
                            id="nav-settings"
                        >
                            <Settings size={20} />
                            <span style={{ flex: 1 }}>Settings</span>
                            <div className="nav-chevron" style={{
                                transition: 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                                transform: isSettingsExpanded ? 'rotate(90deg)' : 'rotate(0deg)',
                                display: 'flex',
                                alignItems: 'center'
                            }}>
                                <ChevronRight size={16} color="var(--text-muted)" />
                            </div>
                        </div>

                        {/* Settings Sub-items */}
                        <div
                            style={{
                                display: 'flex',
                                flexDirection: 'column',
                                overflow: 'hidden',
                                maxHeight: isSettingsExpanded ? '200px' : '0',
                                transition: 'max-height 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
                            }}>
                            <NavLink
                                to="/admin/rates"
                                className={({ isActive }) => `sub-nav-link ${isActive ? 'active' : ''}`}
                                style={{ paddingLeft: '3.5rem' }}
                            >
                                <span>Nominal Rates</span>
                            </NavLink>
                        </div>
                    </div>
                </div>

                <div className="sidebar-footer">
                    <div className="sidebar-footer-content">
                        <div style={{ width: '32px', height: '32px', minWidth: '32px', minHeight: '32px', borderRadius: '50%', background: 'var(--deloitte-green)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '0.75rem', fontWeight: 700, color: 'black' }}>
                            {user?.username?.substring(0, 2).toUpperCase() || 'GT'}
                        </div>
                        <div className="sidebar-footer-text" style={{ flex: 1, overflow: 'hidden' }}>
                            <div style={{ fontSize: '0.85rem', fontWeight: 600, color: 'white', whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>{user?.username || 'User'}</div>
                            <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>{user?.role || 'Guest'}</div>
                        </div>
                        <button onClick={logout} className="btn-icon-premium" style={{ color: 'white', marginLeft: '0.5rem' }} title="Logout">
                            <LogOut size={16} />
                        </button>
                    </div>
                </div>
            </aside>

            {/* Main Content */}
            <main className="main-premium">
                <div className="top-bar-premium">
                    <div className="breadcrumbs-container">
                        {breadcrumbs.length > 0 ? (
                            <div className="breadcrumbs">
                                {breadcrumbs.map((item, index) => (
                                    <React.Fragment key={index}>
                                        {index > 0 && (
                                            <ChevronRight size={14} style={{ color: 'var(--border-color)', margin: '0 0.5rem' }} />
                                        )}
                                        {item.path && !item.disabled ? (
                                            <Link to={item.path} className="breadcrumb-segment link">
                                                {item.label}
                                            </Link>
                                        ) : (
                                            <span className={`breadcrumb-segment ${index === breadcrumbs.length - 1 ? 'active' : ''}`}>
                                                {item.label}
                                            </span>
                                        )}
                                    </React.Fragment>
                                ))}
                            </div>
                        ) : (
                            <div className="breadcrumbs">
                                <span className="breadcrumb-segment active">Resource Platform</span>
                            </div>
                        )}
                    </div>

                    <div style={{ display: 'flex', gap: '1rem' }}>
                        <TutorialManager />
                        <button className="btn-icon-premium" onClick={toggleTheme} id="btn-theme-toggle">
                            {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
                        </button>
                        <button className="btn-icon-premium" onClick={() => window.dispatchEvent(new CustomEvent('open-command-palette'))} id="btn-search">
                            <Search size={20} />
                        </button>
                        <div style={{ position: 'relative' }}>
                            <button
                                className={`btn-icon-premium ${isNotificationOpen ? 'active' : ''}`}
                                onClick={() => setIsNotificationOpen(!isNotificationOpen)}
                                id="btn-notifications"
                            >
                                <Bell size={20} />
                                {unreadCount > 0 && (
                                    <span style={{
                                        position: 'absolute',
                                        top: '6px',
                                        right: '6px',
                                        width: '8px',
                                        height: '8px',
                                        background: 'var(--deloitte-green)',
                                        borderRadius: '50%',
                                        boxShadow: '0 0 0 1.5px var(--bg-color)'
                                    }} />
                                )}
                            </button>
                            {isNotificationOpen && <NotificationCenter onClose={() => setIsNotificationOpen(false)} />}
                        </div>
                    </div>
                </div>

                <div className="page-transition">
                    {children}
                </div>
            </main>
            <CommandPalette />
        </div>
    );
};

export default Layout;
