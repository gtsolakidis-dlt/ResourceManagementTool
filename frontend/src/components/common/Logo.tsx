import React, { useId } from 'react';

export const MainLogoIcon: React.FC<{ className?: string }> = ({
    className = "w-20 h-16"
}) => {
    const maskId = useId();
    const clipId = useId();
    return (
        <svg fill="none" viewBox="0 0 60 48" className={className} xmlns="http://www.w3.org/2000/svg">
            <defs>
                <clipPath id={clipId}>
                    <rect x="0" y="0" width="60" height="44" />
                </clipPath>
                <mask id={maskId} maskUnits="userSpaceOnUse">
                    <rect x="0" y="0" width="60" height="48" fill="white" />
                    <circle cx="52" cy="10" r="9" fill="black" />
                </mask>
            </defs>

            <g mask={`url(#${maskId})`} clipPath={`url(#${clipId})`}>
                {/* R Shape */}
                <path d="M4 48V4H22C29.5 4 32.5 10 32.5 16C32.5 22 29.5 25 22 25H4" stroke="currentColor" strokeLinecap="square" strokeLinejoin="round" strokeWidth="5"></path>

                {/* M Shape: Widened slightly (x=52 vs 50) and thinner stroke (5 vs 6) creates more internal space */}
                <path d="M22 25L37 48L52 18V48" stroke="currentColor" strokeLinecap="butt" strokeLinejoin="miter" strokeMiterlimit="10" strokeWidth="5"></path>
            </g>

            {/* Green Dot: Shifted to x=52 to align with widened M */}
            <circle style={{ fill: 'var(--deloitte-green)' }} cx="52" cy="10" r="6"></circle>
        </svg>
    );
};

export const NavLogo: React.FC<{ className?: string }> = ({ className = "h-8 w-10" }) => {
    const maskId = useId();
    const clipId = useId();
    return (
        <svg className={`text-text-light dark:text-white ${className}`} fill="none" viewBox="0 0 40 32" xmlns="http://www.w3.org/2000/svg">
            <defs>
                <clipPath id={clipId}>
                    <rect x="0" y="0" width="40" height="30" />
                </clipPath>
                <mask id={maskId} maskUnits="userSpaceOnUse">
                    <rect x="0" y="0" width="40" height="32" fill="white" />
                    <circle cx="34" cy="7" r="6" fill="black" />
                </mask>
            </defs>

            <g mask={`url(#${maskId})`} clipPath={`url(#${clipId})`}>
                <path d="M2 32V2H14C19 2 21 6 21 10C21 14 19 16 14 16H2" stroke="currentColor" strokeLinecap="square" strokeLinejoin="round" strokeWidth="3"></path>
                <path d="M14 16L24 32L34 12V32" stroke="currentColor" strokeLinecap="butt" strokeLinejoin="miter" strokeMiterlimit="10" strokeWidth="3"></path>
            </g>
            <circle style={{ fill: 'var(--deloitte-green)' }} cx="34" cy="7" r="4"></circle>
        </svg>
    );
};

export const SmallIcon: React.FC<{ strokeClass?: string }> = ({
    strokeClass = "stroke-gray-900 dark:stroke-white"
}) => {
    const maskId = useId();
    const clipId = useId();
    // For small icon, keeping strokeClass prop to avoid breaking usages where className isn't passed,
    // but defaulting to currentColor behavior is safer if possible. Here keeping as is but updated paths.
    return (
        <svg fill="none" height="16" viewBox="0 0 60 48" width="20" xmlns="http://www.w3.org/2000/svg" className={strokeClass ? "" : "text-current"}>
            <defs>
                <clipPath id={clipId}>
                    <rect x="0" y="0" width="60" height="44" />
                </clipPath>
                <mask id={maskId} maskUnits="userSpaceOnUse">
                    <rect x="0" y="0" width="60" height="48" fill="white" />
                    <circle cx="52" cy="10" r="10" fill="black" />
                </mask>
            </defs>
            <g mask={`url(#${maskId})`} clipPath={`url(#${clipId})`}>
                <path stroke="currentColor" className={strokeClass} d="M4 48V4H22C29.5 4 32.5 10 32.5 16C32.5 22 29.5 25 22 25H4" strokeLinecap="square" strokeLinejoin="round" strokeWidth="6"></path>
                <path stroke="currentColor" className={strokeClass} d="M22 25L37 48L52 18V48" strokeLinecap="butt" strokeLinejoin="miter" strokeMiterlimit="10" strokeWidth="6"></path>
            </g>
            <circle style={{ fill: 'var(--deloitte-green)' }} cx="52" cy="10" r="8"></circle>
        </svg>
    );
};
