import React, { useState, useRef, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { ChevronDown } from 'lucide-react';
import './PremiumSelect.css';

interface Option {
    value: string;
    label: string;
}

interface PremiumSelectProps {
    options: Option[];
    value: string;
    onChange: (value: string) => void;
    placeholder?: string;
    className?: string;
}

const PremiumSelect: React.FC<PremiumSelectProps> = ({ options, value, onChange, placeholder = 'Select...', className }) => {
    const [isOpen, setIsOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);
    const [coords, setCoords] = useState({ top: 0, left: 0, width: 0 });

    const updateCoords = () => {
        if (containerRef.current) {
            const rect = containerRef.current.getBoundingClientRect();
            setCoords({
                top: rect.bottom + window.scrollY + 6,
                left: rect.left + window.scrollX,
                width: rect.width
            });
        }
    };

    // Close on click outside and update coords on scroll/resize
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                // Also check if click is inside the portal dropdown (which is outside this component in DOM)
                const portal = document.getElementById('premium-select-portal');
                if (portal && portal.contains(event.target as Node)) return;

                setIsOpen(false);
            }
        };

        if (isOpen) {
            updateCoords();
            window.addEventListener('scroll', updateCoords, true);
            window.addEventListener('resize', updateCoords);
            document.addEventListener('mousedown', handleClickOutside);
        }

        return () => {
            window.removeEventListener('scroll', updateCoords, true);
            window.removeEventListener('resize', updateCoords);
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen]);

    const selectedOption = options.find(o => o.value === value);

    const dropdown = (
        <div
            id="premium-select-portal"
            className="premium-select-dropdown"
            style={{
                position: 'absolute',
                top: coords.top,
                left: coords.left,
                width: coords.width,
            }}
        >
            {options.map(option => (
                <div
                    key={option.value}
                    className={`premium-select-option ${option.value === value ? 'selected' : ''}`}
                    onClick={() => {
                        onChange(option.value);
                        setIsOpen(false);
                    }}
                >
                    {option.label}
                </div>
            ))}
            {options.length === 0 && (
                <div style={{ padding: '0.8rem', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
                    No options
                </div>
            )}
        </div>
    );

    return (
        <div className={`premium-select-container ${className || ''}`} ref={containerRef}>
            <div
                className="premium-select-trigger"
                onClick={() => {
                    if (!isOpen) updateCoords();
                    setIsOpen(!isOpen);
                }}
            >
                <span style={{ color: selectedOption ? 'var(--text-primary)' : 'var(--text-muted)' }}>
                    {selectedOption ? selectedOption.label : placeholder}
                </span>
                <ChevronDown
                    size={18}
                    color="var(--text-muted)"
                    style={{
                        transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                        transition: 'transform 0.3s cubic-bezier(0.16, 1, 0.3, 1)'
                    }}
                />
            </div>

            {isOpen && createPortal(dropdown, document.body)}
        </div>
    );
};

export default PremiumSelect;
