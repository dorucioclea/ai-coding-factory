/**
 * Database Manager for AI Config Sync
 *
 * Manages SQLite database operations for storing and retrieving
 * sync state, artifacts, and configuration.
 */
import type { SystemId, ArtifactType, SyncResult, ArtifactDiff } from "../models/types.js";
export interface ArtifactRecord {
    id: string;
    name: string;
    type: string;
    description: string | null;
    content_hash: string;
    source_system: string;
    source_path: string;
    metadata: string | null;
    created_at: string;
    updated_at: string;
}
export interface SyncStateRecord {
    id: number;
    artifact_id: string;
    target_system: string;
    target_path: string | null;
    sync_method: string;
    synced_hash: string | null;
    last_synced_at: string | null;
    status: string;
    error_message: string | null;
}
export interface SystemRecord {
    id: string;
    name: string;
    description: string | null;
    capabilities: string;
    paths: string;
    config_format: string;
    file_patterns: string;
    enabled: number;
}
export interface MappingRuleRecord {
    id: number;
    source_system: string;
    target_system: string;
    artifact_type: string;
    source_pattern: string;
    target_pattern: string;
    transform_type: string | null;
    use_symlink: number;
    priority: number;
    enabled: number;
}
export interface SyncJobRecord {
    id: string;
    source_system: string;
    target_systems: string;
    artifact_types: string;
    dry_run: number;
    force: number;
    use_symlinks: number;
    started_at: string;
    completed_at: string | null;
    status: string;
    summary: string | null;
}
/**
 * Database Manager class
 */
export declare class DatabaseManager {
    private db;
    private dbPath;
    constructor(projectRoot: string, dbFileName?: string);
    /**
     * Initialize database schema
     */
    private initialize;
    /**
     * Seed default data
     */
    private seedDefaults;
    /**
     * Migrate database schema
     */
    private migrate;
    /**
     * Close database connection
     */
    close(): void;
    /**
     * Get all registered systems
     */
    getSystems(): SystemRecord[];
    /**
     * Get a specific system by ID
     */
    getSystem(id: SystemId): SystemRecord | undefined;
    /**
     * Update system configuration
     */
    updateSystem(id: SystemId, updates: Partial<Omit<SystemRecord, "id">>): void;
    /**
     * Upsert an artifact
     */
    upsertArtifact(artifact: Omit<ArtifactRecord, "created_at" | "updated_at">): void;
    /**
     * Get all artifacts for a system
     */
    getArtifactsBySystem(systemId: SystemId): ArtifactRecord[];
    /**
     * Get artifacts by type
     */
    getArtifactsByType(type: ArtifactType): ArtifactRecord[];
    /**
     * Get a specific artifact
     */
    getArtifact(id: string): ArtifactRecord | undefined;
    /**
     * Get artifact by source path
     */
    getArtifactByPath(sourcePath: string): ArtifactRecord | undefined;
    /**
     * Delete an artifact
     */
    deleteArtifact(id: string): void;
    /**
     * Get sync state for an artifact
     */
    getSyncState(artifactId: string, targetSystem: SystemId): SyncStateRecord | undefined;
    /**
     * Get all sync states for a target system
     */
    getSyncStatesForTarget(targetSystem: SystemId): SyncStateRecord[];
    /**
     * Update sync state
     */
    upsertSyncState(state: Omit<SyncStateRecord, "id">): void;
    /**
     * Create a new sync job
     */
    createSyncJob(job: Omit<SyncJobRecord, "started_at" | "completed_at" | "status">): void;
    /**
     * Update sync job status
     */
    updateSyncJob(id: string, status: string, summary?: Record<string, unknown>): void;
    /**
     * Get sync job by ID
     */
    getSyncJob(id: string): SyncJobRecord | undefined;
    /**
     * Get recent sync jobs
     */
    getRecentSyncJobs(limit?: number): SyncJobRecord[];
    /**
     * Add sync result
     */
    addSyncResult(result: Omit<SyncResult, "timestamp"> & {
        job_id: string;
    }): void;
    /**
     * Get sync results for a job
     */
    getSyncResults(jobId: string): SyncResult[];
    /**
     * Get mapping rules for a source-target pair
     */
    getMappingRules(sourceSystem: SystemId, targetSystem: SystemId, artifactType?: ArtifactType): MappingRuleRecord[];
    /**
     * Add a custom mapping rule
     */
    addMappingRule(rule: Omit<MappingRuleRecord, "id" | "enabled">): void;
    /**
     * Get a setting value
     */
    getSetting(key: string): string | undefined;
    /**
     * Set a setting value
     */
    setSetting(key: string, value: string): void;
    /**
     * Get artifacts that need syncing to a target
     */
    getOutOfSyncArtifacts(sourceSystem: SystemId, targetSystem: SystemId, artifactType?: ArtifactType): ArtifactDiff[];
    /**
     * Get database path
     */
    getDatabasePath(): string;
    /**
     * Get database statistics
     */
    getStats(): {
        systems: number;
        artifacts: number;
        syncStates: number;
        syncJobs: number;
        mappingRules: number;
    };
}
//# sourceMappingURL=manager.d.ts.map