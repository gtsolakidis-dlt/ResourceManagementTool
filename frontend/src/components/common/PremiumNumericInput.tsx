import React from 'react';
import { Plus, Minus } from 'lucide-react';
import './PremiumNumericInput.css';

interface PremiumNumericInputProps {
    value: number;
    onChange: (value: number) => void;
    step?: number;
    min?: number;
    max?: number;
    className?: string;
    style?: React.CSSProperties;
    disabled?: boolean;
    hideControls?: boolean;
    onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
    inputRef?: React.Ref<HTMLInputElement>;
}

const PremiumNumericInput: React.FC<PremiumNumericInputProps> = ({
    value,
    onChange,
    step = 1,
    min,
    max,
    className,
    style,
    disabled = false,
    hideControls = false,
    onKeyDown,
    inputRef
}) => {
    // Format helper: 1234.56 -> "1.234,56"
    const formatValue = (val: number): string => {
        if (val === undefined || val === null || isNaN(val)) return '';
        return val.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    const [inputValue, setInputValue] = React.useState(formatValue(value));
    const [isFocused, setIsFocused] = React.useState(false);

    // Sync external value changes when not focused
    React.useEffect(() => {
        if (!isFocused) {
            setInputValue(formatValue(value));
        }
    }, [value, isFocused]);

    const handleIncrement = () => {
        const newValue = value + step;
        if (max === undefined || newValue <= max) {
            onChange(Number(newValue.toFixed(2)));
        }
    };

    const handleDecrement = () => {
        const newValue = value - step;
        if (min === undefined || newValue >= min) {
            onChange(Number(newValue.toFixed(2)));
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(e.target.value);
    };

    const handleBlur = () => {
        setIsFocused(false);
        // Parse "1.234,56" or "1234.56"
        let raw = inputValue.replace(/\./g, ''); // Remove thousands dots
        raw = raw.replace(',', '.'); // Change comma to dot for parsing

        let num = parseFloat(raw);
        if (isNaN(num)) num = 0;

        // validation
        if (min !== undefined && num < min) num = min;
        if (max !== undefined && num > max) num = max;

        onChange(num);
        setInputValue(formatValue(num));
    };

    const handleFocus = () => {
        setIsFocused(true);
        // On focus, perhaps show cleaner number for editing? 
        // Or just keep the text. Keeping text is fine if user knows format.
        // Let's select all text for easier replacement
        // e.target.select() happens in onFocus prop usually, we can add logic if needed.
    };

    return (
        <div className={`premium-numeric-input-container ${className || ''}`} style={style}>
            {!hideControls && (
                <button
                    className="input-control-btn minus"
                    onClick={handleDecrement}
                    disabled={disabled || (min !== undefined && value <= min)}
                    type="button"
                    tabIndex={-1}
                >
                    <Minus size={14} />
                </button>
            )}
            <input
                ref={inputRef}
                onKeyDown={onKeyDown}
                type="text"
                value={inputValue}
                onChange={handleChange}
                onBlur={handleBlur}
                onFocus={handleFocus}
                disabled={disabled}
                className="premium-numeric-input-field"
                style={{ textAlign: 'center' }}
            />
            {!hideControls && (
                <button
                    className="input-control-btn plus"
                    onClick={handleIncrement}
                    disabled={disabled || (max !== undefined && value >= max)}
                    type="button"
                    tabIndex={-1}
                >
                    <Plus size={14} />
                </button>
            )}
        </div>
    );
};

export default PremiumNumericInput;
