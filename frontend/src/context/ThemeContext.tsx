import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';

type Theme = 'light' | 'dark';

interface ThemeContextType {
    theme: Theme;
    toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider = ({ children }: { children: ReactNode }) => {
    const [theme, setTheme] = useState<Theme>(() => {
        const saved = localStorage.getItem('theme');
        return (saved as Theme) || 'dark';
    });

    useEffect(() => {
        const root = document.documentElement;
        // Assuming your global CSS uses .theme-dark / .theme-light classes
        // OR standard approach is to modify root class.
        // Let's check if user's CSS depends on body class or just variables. 
        // Index.css usually has :root {} or .theme-light {}. 
        // I'll toggle classes on documentElement (<html>).
        root.classList.remove('theme-light', 'theme-dark');
        root.classList.add(`theme-${theme}`);
    }, [theme]);

    const toggleTheme = () => {
        setTheme(prev => prev === 'light' ? 'dark' : 'light');
    };

    return (
        <ThemeContext.Provider value={{ theme, toggleTheme }}>
            {children}
        </ThemeContext.Provider>
    );
};

export const useTheme = () => {
    const context = useContext(ThemeContext);
    if (!context) throw new Error('useTheme must be used within ThemeProvider');
    return context;
};
