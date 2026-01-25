/**
 * Sync Engine
 *
 * Core synchronization logic for transferring configurations
 * between AI coding assistants.
 */
import { DatabaseManager } from "./database/index.js";
import type { SystemId, ArtifactType, SyncResult, ArtifactDiff } from "./models/types.js";
export interface SyncOptions {
    source: SystemId;
    targets: SystemId[];
    artifactTypes?: ArtifactType[];
    dryRun?: boolean;
    force?: boolean;
    useSymlinks?: boolean;
    verbose?: boolean;
    /** Delete artifacts from targets that no longer exist in source */
    syncDeletions?: boolean;
}
export interface SyncSummary {
    jobId: string;
    source: SystemId;
    targets: SystemId[];
    startedAt: Date;
    completedAt: Date;
    results: {
        total: number;
        created: number;
        updated: number;
        skipped: number;
        failed: number;
        symlinked: number;
        deleted: number;
    };
    details: SyncResult[];
}
export declare class SyncEngine {
    private db;
    private projectRoot;
    private adapters;
    constructor(projectRoot: string);
    /**
     * Get or create adapter for a system
     */
    private getSystemAdapter;
    /**
     * Execute a full sync operation
     */
    sync(options: SyncOptions): Promise<SyncSummary>;
    /**
     * Sync a single artifact
     */
    private syncArtifact;
    /**
     * Write artifact to target system
     */
    private writeArtifact;
    /**
     * Sync deletions: remove artifacts from target that no longer exist in source
     */
    private syncDeletions;
    /**
     * Get diff between source and target systems
     */
    diff(source: SystemId, target: SystemId, artifactType?: ArtifactType): Promise<ArtifactDiff[]>;
    /**
     * Get status of all systems
     */
    status(): Promise<{
        systems: Array<{
            id: SystemId;
            name: string;
            configured: boolean;
            artifactCounts: Record<string, number>;
        }>;
        lastSync?: string;
        stats: ReturnType<DatabaseManager["getStats"]>;
    }>;
    /**
     * Get recent sync history
     */
    getHistory(limit?: number): Array<{
        id: string;
        source: SystemId;
        targets: SystemId[];
        status: string;
        startedAt: string;
        completedAt?: string;
        summary?: Record<string, unknown>;
    }>;
    /**
     * Get results for a specific job
     */
    getJobResults(jobId: string): SyncResult[];
    /**
     * Close database connection
     */
    close(): void;
}
//# sourceMappingURL=sync-engine.d.ts.map