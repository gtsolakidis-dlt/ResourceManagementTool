import { createContext, useContext, useState, type ReactNode } from 'react';

export interface BreadcrumbItem {
    label: string;
    path?: string;
    disabled?: boolean;
}

export interface SidebarSubItem {
    label: string;
    path: string;
    isActive?: boolean;
    exact?: boolean;
    isHeader?: boolean;
}

interface NavigationContextType {
    breadcrumbs: BreadcrumbItem[];
    setBreadcrumbs: (items: BreadcrumbItem[]) => void;

    // For the Sidebar dynamic part
    activeSection: string; // e.g., 'projects'
    setActiveSection: (section: string) => void;

    sidebarSubItems: SidebarSubItem[];
    setSidebarSubItems: (items: SidebarSubItem[]) => void;
}

const NavigationContext = createContext<NavigationContextType | undefined>(undefined);

export const NavigationProvider = ({ children }: { children: ReactNode }) => {
    const [breadcrumbs, setBreadcrumbs] = useState<BreadcrumbItem[]>([]);
    const [activeSection, setActiveSection] = useState<string>('dashboard');
    const [sidebarSubItems, setSidebarSubItems] = useState<SidebarSubItem[]>([]);

    return (
        <NavigationContext.Provider value={{
            breadcrumbs, setBreadcrumbs,
            activeSection, setActiveSection,
            sidebarSubItems, setSidebarSubItems
        }}>
            {children}
        </NavigationContext.Provider>
    );
};

export const useNavigation = () => {
    const context = useContext(NavigationContext);
    if (!context) throw new Error('useNavigation must be used within NavigationProvider');
    return context;
};
