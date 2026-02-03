/**
 * Seniority Levels Constants
 * 
 * This is the single source of truth for seniority level mappings
 * used throughout the application (Roster, Global Rates, etc.)
 */

export interface SeniorityLevel {
    /** Short abbreviation stored in database */
    code: string;
    /** Full display name */
    fullName: string;
    /** Display format: "Full Name (Code)" */
    displayName: string;
    /** Numeric order for sorting (lower = more junior) */
    order: number;
}

/**
 * Standard Deloitte seniority levels from entry to partner
 */
export const SENIORITY_LEVELS: SeniorityLevel[] = [
    { code: 'BA', fullName: 'Business Analyst', displayName: 'Business Analyst (BA)', order: 1 },
    { code: 'C', fullName: 'Consultant', displayName: 'Consultant (C)', order: 2 },
    { code: 'SC', fullName: 'Senior Consultant', displayName: 'Senior Consultant (SC)', order: 3 },
    { code: 'AM', fullName: 'Assistant Manager', displayName: 'Assistant Manager (AM)', order: 4 },
    { code: 'M', fullName: 'Manager', displayName: 'Manager (M)', order: 5 },
    { code: 'SM', fullName: 'Senior Manager', displayName: 'Senior Manager (SM)', order: 6 },
    { code: 'D', fullName: 'Principal/Director', displayName: 'Principal/Director (D)', order: 7 },
    { code: 'P', fullName: 'Partner', displayName: 'Partner (P)', order: 8 },
];

/**
 * Lookup map for quick access by code
 */
export const SENIORITY_LEVEL_MAP: Record<string, SeniorityLevel> = SENIORITY_LEVELS.reduce(
    (acc, level) => {
        acc[level.code] = level;
        return acc;
    },
    {} as Record<string, SeniorityLevel>
);

/**
 * Get the full display name for a seniority code
 * @param code - The abbreviation (e.g., 'SC')
 * @returns The display name (e.g., 'Senior Consultant (SC)') or the code if not found
 */
export function getSeniorityDisplayName(code: string): string {
    const level = SENIORITY_LEVEL_MAP[code?.toUpperCase()];
    return level ? level.displayName : code;
}

/**
 * Get the full name for a seniority code
 * @param code - The abbreviation (e.g., 'SC')
 * @returns The full name (e.g., 'Senior Consultant') or the code if not found
 */
export function getSeniorityFullName(code: string): string {
    const level = SENIORITY_LEVEL_MAP[code?.toUpperCase()];
    return level ? level.fullName : code;
}

/**
 * Get the CSS class suffix for styling based on seniority code
 */
export function getSeniorityBadgeClass(code: string): string {
    const upperCode = code?.toUpperCase();
    switch (upperCode) {
        case 'BA': return 'ba';
        case 'C': return 'consultant';
        case 'SC': return 'senior';
        case 'AM': return 'am';
        case 'M': return 'manager';
        case 'SM': return 'sm';
        case 'D': return 'director';
        case 'P': return 'partner';
        default: return 'consultant';
    }
}
