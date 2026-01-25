/**
 * Utility helper functions
 */
/**
 * Generate a unique ID for sync jobs
 */
export declare function generateJobId(): string;
/**
 * Format a date for display
 */
export declare function formatDate(date: Date): string;
/**
 * Format bytes for display
 */
export declare function formatBytes(bytes: number): string;
/**
 * Pluralize a word
 */
export declare function pluralize(count: number, singular: string, plural?: string): string;
/**
 * Truncate string for display
 */
export declare function truncate(str: string, maxLength: number): string;
/**
 * Pad string to fixed width
 */
export declare function padRight(str: string, width: number): string;
/**
 * Create a simple progress bar
 */
export declare function progressBar(current: number, total: number, width?: number): string;
//# sourceMappingURL=helpers.d.ts.map