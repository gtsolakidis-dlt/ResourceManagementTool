import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Home, Users, Briefcase, Plus, Settings } from 'lucide-react';
import './CommandPalette.css';
import { projectService, rosterService } from '../../api/services';
import type { Project, RosterMember } from '../../types';

interface CommandItem {
    id: string;
    label: string;
    icon: React.ReactNode;
    action: () => void;
    group?: string;
}

const CommandPalette: React.FC = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [query, setQuery] = useState('');
    const [selectedIndex, setSelectedIndex] = useState(0);
    const [dynamicCommands, setDynamicCommands] = useState<CommandItem[]>([]);
    const navigate = useNavigate();
    const inputRef = useRef<HTMLInputElement>(null);

    const staticCommands: CommandItem[] = [
        { id: 'home', label: 'Go to Dashboard', icon: <Home size={18} />, action: () => navigate('/'), group: 'Navigation' },
        { id: 'projects', label: 'Go to Projects', icon: <Briefcase size={18} />, action: () => navigate('/projects'), group: 'Navigation' },
        { id: 'roster', label: 'Go to Roster', icon: <Users size={18} />, action: () => navigate('/roster'), group: 'Navigation' },
        { id: 'rates', label: 'Go to Settings / Global Rates', icon: <Settings size={18} />, action: () => navigate('/admin/rates'), group: 'Navigation' },
        { id: 'create-project', label: 'Create New Project', icon: <Plus size={18} />, action: () => navigate('/projects?action=create'), group: 'Actions' },
        { id: 'add-member', label: 'Add New Talent', icon: <Plus size={18} />, action: () => navigate('/roster?action=create'), group: 'Actions' },
    ];

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                e.preventDefault();
                setIsOpen(prev => !prev);
            }
            if (e.key === 'Escape') setIsOpen(false);
        };

        const onOpenEvent = () => {
            setIsOpen(true);
        };

        document.addEventListener('keydown', onKeyDown);
        window.addEventListener('open-command-palette', onOpenEvent);

        return () => {
            document.removeEventListener('keydown', onKeyDown);
            window.removeEventListener('open-command-palette', onOpenEvent);
        };
    }, []);

    useEffect(() => {
        if (isOpen) {
            setTimeout(() => inputRef.current?.focus(), 100);
            setQuery('');
            setSelectedIndex(0);
            fetchData();
        }
    }, [isOpen]);

    const fetchData = async () => {
        try {
            const [projRes, rosterRes] = await Promise.all([
                projectService.getProjects(),
                rosterService.getMembers()
            ]);

            const newCommands: CommandItem[] = [];

            if (projRes.data) {
                projRes.data.forEach((p: Project) => {
                    newCommands.push({
                        id: `proj-${p.id}`,
                        label: `Project: ${p.name}`,
                        icon: <Briefcase size={18} color="var(--primary-color)" />,
                        action: () => navigate(`/projects/${p.id}`), // Assuming you have a detail view or just filtering list
                        group: 'Projects'
                    });
                });
            }

            if (rosterRes.data) {
                rosterRes.data.forEach((m: RosterMember) => {
                    newCommands.push({
                        id: `member-${m.id}`,
                        label: `Resource: ${m.fullNameEn}`,
                        icon: <Users size={18} color="var(--deloitte-green)" />,
                        action: () => navigate(`/roster?search=${encodeURIComponent(m.fullNameEn)}`), // Roster detail might technically be just a filtered list view
                        group: 'People'
                    });
                });
            }
            setDynamicCommands(newCommands);

        } catch (error) {
            console.error("Failed to load search data", error);
        }
    };

    const allCommands = [...staticCommands, ...dynamicCommands];

    const filteredCommands = allCommands.filter(c =>
        c.label.toLowerCase().includes(query.toLowerCase())
    );

    // Limit results for performance if query is empty, but show all if query exists (or limit to top 20)
    const displayCommands = filteredCommands.slice(0, 50);

    useEffect(() => {
        setSelectedIndex(0);
    }, [query]);

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setSelectedIndex(prev => (prev + 1) % displayCommands.length);
        }
        if (e.key === 'ArrowUp') {
            e.preventDefault();
            setSelectedIndex(prev => (prev - 1 + displayCommands.length) % displayCommands.length);
        }
        if (e.key === 'Enter') {
            e.preventDefault();
            if (displayCommands[selectedIndex]) {
                const cmd = displayCommands[selectedIndex];
                cmd.action();
                setIsOpen(false);
            }
        }
    };

    if (!isOpen) return null;

    return (
        <div className="cmd-overlay" onClick={() => setIsOpen(false)}>
            <div className="cmd-container" onClick={e => e.stopPropagation()}>
                <div className="cmd-header">
                    <Search className="cmd-icon" size={20} />
                    <input
                        ref={inputRef}
                        className="cmd-input"
                        placeholder="Type a command, project, or resource..."
                        value={query}
                        onChange={e => setQuery(e.target.value)}
                        onKeyDown={handleKeyDown}
                    />
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <span className="cmd-shortcut">ESC</span>
                    </div>
                </div>
                <div className="cmd-list">
                    {displayCommands.length > 0 ? (
                        displayCommands.map((cmd, index) => (
                            <div
                                key={cmd.id}
                                className={`cmd-item ${index === selectedIndex ? 'selected' : ''}`}
                                onClick={() => { cmd.action(); setIsOpen(false); }}
                                onMouseEnter={() => setSelectedIndex(index)}
                            >
                                <div className="cmd-icon">{cmd.icon}</div>
                                <div style={{ display: 'flex', flexDirection: 'column' }}>
                                    <span>{cmd.label}</span>
                                    {cmd.group && <span style={{ fontSize: '0.7em', color: 'var(--text-muted)' }}>{cmd.group}</span>}
                                </div>
                                {index === selectedIndex && <span className="cmd-shortcut">↵</span>}
                            </div>
                        ))
                    ) : (
                        <div className="cmd-empty">No results found.</div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default CommandPalette;
