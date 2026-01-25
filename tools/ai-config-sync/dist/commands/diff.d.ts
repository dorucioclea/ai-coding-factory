/**
 * Diff Command
 *
 * Show differences between source and target systems.
 */
export interface DiffCommandOptions {
    source?: string;
    target?: string;
    type?: string;
}
export declare function diffCommand(projectRoot: string, options: DiffCommandOptions): Promise<void>;
//# sourceMappingURL=diff.d.ts.map