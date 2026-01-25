/**
 * Sync Command
 *
 * Synchronize configurations from source to target systems.
 */
export interface SyncCommandOptions {
    source?: string;
    targets?: string[];
    types?: string[];
    dryRun?: boolean;
    force?: boolean;
    noSymlinks?: boolean;
    verbose?: boolean;
    /** Delete artifacts from targets that no longer exist in source */
    delete?: boolean;
}
export declare function syncCommand(projectRoot: string, options: SyncCommandOptions): Promise<void>;
//# sourceMappingURL=sync.d.ts.map