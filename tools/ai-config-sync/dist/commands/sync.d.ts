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
}
export declare function syncCommand(projectRoot: string, options: SyncCommandOptions): Promise<void>;
//# sourceMappingURL=sync.d.ts.map