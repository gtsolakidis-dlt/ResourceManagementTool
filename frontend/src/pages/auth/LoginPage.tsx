import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Loader2 } from 'lucide-react';
import { MainLogoIcon } from '../../components/common/Logo';
import './LoginPage.css';

const LoginPage: React.FC = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setIsSubmitting(true);
        try {
            await login(username, password);
            navigate('/');
        } catch (err) {
            setError('Invalid credentials');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="login-page">
            <div className="login-card glass-panel animate-fade-in">
                <div className="login-header">
                    <MainLogoIcon className="w-20 h-16 mb-4 text-primary" />
                    <h1><strong>Resource Management</strong> <span style={{ fontWeight: 400 }}>Tool</span></h1>
                    <p>Enter your credentials to access the system</p>
                </div>

                <form onSubmit={handleSubmit} className="login-form">
                    <div className="form-group-premium">
                        <label>Username</label>
                        <input
                            type="text"
                            value={username}
                            onChange={e => setUsername(e.target.value)}
                            placeholder="e.g. jdoe"
                            required
                            autoFocus
                        />
                    </div>

                    <div className="form-group-premium">
                        <label>Password</label>
                        <input
                            type="password"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            required
                        />
                    </div>

                    {error && <div className="login-error">{error}</div>}

                    <button type="submit" className="btn-premium btn-login" disabled={isSubmitting}>
                        {isSubmitting ? <Loader2 className="animate-spin" /> : 'Sign In'}
                    </button>
                </form>

                <div className="login-footer">
                    <p>Protected System &copy; 2026</p>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;
