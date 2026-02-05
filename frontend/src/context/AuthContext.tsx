import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

// DYNAMIC CONFIG FOR SANDBOX VM
// If we are in production (not using Vite Proxy), point to Port 8080 on the same host
if (!import.meta.env.DEV) {
    const protocol = window.location.protocol;
    const hostname = window.location.hostname;
    axios.defaults.baseURL = `${protocol}//${hostname}:8080`;
    console.log('Production API Base URL set to:', axios.defaults.baseURL);
}

interface User {
    id: string; // RosterId
    username: string;
    role: 'Employee' | 'Manager' | 'Partner' | 'Admin';
    raw?: any; // DEBUG: Capture full response
}

interface AuthContextType {
    user: User | null;
    login: (username: string, password: string) => Promise<void>;
    logout: () => void;
    isLoading: boolean;
    isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const checkAuth = async () => {
        const storedAuth = localStorage.getItem('authToken');
        if (storedAuth) {
            // Set default header
            axios.defaults.headers.common['Authorization'] = `Basic ${storedAuth}`;
            try {
                // Verify with backend
                const res = await axios.get('/api/auth/me');
                const d = res.data;
                setUser({
                    id: d.rosterId || d.RosterId || d.id || d.Id,
                    username: d.username || d.Username,
                    role: d.role || d.Role,
                    raw: d
                });
            } catch (err) {
                console.error('Auth verification failed', err);
                logout(); // Clear invalid token
            }
        }
        setIsLoading(false);
    };

    useEffect(() => {
        checkAuth();
    }, []);

    const login = async (username: string, password: string) => {
        const token = btoa(`${username}:${password}`); // Basic Auth format
        const authHeader = `Basic ${token}`;

        // Test credentials
        try {
            const res = await axios.get('/api/auth/me', {
                headers: { Authorization: authHeader }
            });

            // If success, save and set state
            localStorage.setItem('authToken', token);
            axios.defaults.headers.common['Authorization'] = authHeader;

            const d = res.data;
            setUser({
                id: d.rosterId || d.RosterId || d.id || d.Id,
                username: d.username || d.Username,
                role: d.role || d.Role,
                raw: d
            });

            return Promise.resolve();
        } catch (error) {
            return Promise.reject(error);
        }
    };

    const logout = () => {
        localStorage.removeItem('authToken');
        delete axios.defaults.headers.common['Authorization'];
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, isLoading, isAuthenticated: !!user }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
