/**
 * History Command
 *
 * Show sync history and job details.
 */
export interface HistoryCommandOptions {
    limit?: number;
    job?: string;
}
export declare function historyCommand(projectRoot: string, options: HistoryCommandOptions): Promise<void>;
//# sourceMappingURL=history.d.ts.map