/**
 * Database Manager for AI Config Sync
 *
 * Manages SQLite database operations for storing and retrieving
 * sync state, artifacts, and configuration.
 */

import Database from "better-sqlite3";
import { existsSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import {
  CREATE_TABLES,
  DEFAULT_MAPPING_RULES,
  DEFAULT_SETTINGS,
  DEFAULT_SYSTEMS,
  SCHEMA_VERSION,
} from "./schema.js";
import type {
  SystemId,
  ArtifactType,
  SyncResult,
  ArtifactDiff,
} from "../models/types.js";

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
export class DatabaseManager {
  private db: Database.Database;
  private dbPath: string;

  constructor(projectRoot: string, dbFileName = "ai-config-sync.db") {
    this.dbPath = join(projectRoot, ".ai-config-sync", dbFileName);

    // Ensure directory exists
    const dbDir = dirname(this.dbPath);
    if (!existsSync(dbDir)) {
      mkdirSync(dbDir, { recursive: true });
    }

    this.db = new Database(this.dbPath);
    this.db.pragma("journal_mode = WAL");
    this.db.pragma("foreign_keys = ON");

    this.initialize();
  }

  /**
   * Initialize database schema
   */
  private initialize(): void {
    // Check if schema exists and is up to date
    const versionTable = this.db
      .prepare(
        "SELECT name FROM sqlite_master WHERE type='table' AND name='schema_version'"
      )
      .get();

    if (!versionTable) {
      // Create fresh schema
      this.db.exec(CREATE_TABLES);
      this.seedDefaults();
    } else {
      // Check version and migrate if needed
      const version = this.db
        .prepare("SELECT version FROM schema_version ORDER BY version DESC LIMIT 1")
        .get() as { version: number } | undefined;

      if (!version || version.version < SCHEMA_VERSION) {
        this.migrate(version?.version ?? 0);
      }
    }
  }

  /**
   * Seed default data
   */
  private seedDefaults(): void {
    const insertSystem = this.db.prepare(`
      INSERT OR IGNORE INTO systems (id, name, description, capabilities, paths, config_format, file_patterns)
      VALUES (@id, @name, @description, @capabilities, @paths, @config_format, @file_patterns)
    `);

    const insertMapping = this.db.prepare(`
      INSERT OR IGNORE INTO mapping_rules (source_system, target_system, artifact_type, source_pattern, target_pattern, transform_type, use_symlink, priority)
      VALUES (@source_system, @target_system, @artifact_type, @source_pattern, @target_pattern, @transform_type, @use_symlink, @priority)
    `);

    const insertSetting = this.db.prepare(`
      INSERT OR IGNORE INTO settings (key, value)
      VALUES (@key, @value)
    `);

    this.db.transaction(() => {
      for (const system of DEFAULT_SYSTEMS) {
        insertSystem.run(system);
      }
      for (const mapping of DEFAULT_MAPPING_RULES) {
        insertMapping.run(mapping);
      }
      for (const setting of DEFAULT_SETTINGS) {
        insertSetting.run(setting);
      }
    })();
  }

  /**
   * Migrate database schema
   */
  private migrate(fromVersion: number): void {
    // Future migrations go here
    if (fromVersion < 1) {
      this.db.exec(CREATE_TABLES);
      this.seedDefaults();
    }
  }

  /**
   * Close database connection
   */
  close(): void {
    this.db.close();
  }

  // ==========================================================================
  // System Operations
  // ==========================================================================

  /**
   * Get all registered systems
   */
  getSystems(): SystemRecord[] {
    return this.db.prepare("SELECT * FROM systems WHERE enabled = 1").all() as SystemRecord[];
  }

  /**
   * Get a specific system by ID
   */
  getSystem(id: SystemId): SystemRecord | undefined {
    return this.db.prepare("SELECT * FROM systems WHERE id = ?").get(id) as
      | SystemRecord
      | undefined;
  }

  /**
   * Update system configuration
   */
  updateSystem(
    id: SystemId,
    updates: Partial<Omit<SystemRecord, "id">>
  ): void {
    const fields = Object.keys(updates)
      .map((key) => `${key} = @${key}`)
      .join(", ");

    this.db
      .prepare(`UPDATE systems SET ${fields}, updated_at = datetime('now') WHERE id = @id`)
      .run({ id, ...updates });
  }

  // ==========================================================================
  // Artifact Operations
  // ==========================================================================

  /**
   * Upsert an artifact
   */
  upsertArtifact(artifact: Omit<ArtifactRecord, "created_at" | "updated_at">): void {
    this.db
      .prepare(
        `
      INSERT INTO artifacts (id, name, type, description, content_hash, source_system, source_path, metadata)
      VALUES (@id, @name, @type, @description, @content_hash, @source_system, @source_path, @metadata)
      ON CONFLICT(id) DO UPDATE SET
        name = @name,
        description = @description,
        content_hash = @content_hash,
        source_path = @source_path,
        metadata = @metadata,
        updated_at = datetime('now')
    `
      )
      .run(artifact);
  }

  /**
   * Get all artifacts for a system
   */
  getArtifactsBySystem(systemId: SystemId): ArtifactRecord[] {
    return this.db
      .prepare("SELECT * FROM artifacts WHERE source_system = ?")
      .all(systemId) as ArtifactRecord[];
  }

  /**
   * Get artifacts by type
   */
  getArtifactsByType(type: ArtifactType): ArtifactRecord[] {
    return this.db
      .prepare("SELECT * FROM artifacts WHERE type = ?")
      .all(type) as ArtifactRecord[];
  }

  /**
   * Get a specific artifact
   */
  getArtifact(id: string): ArtifactRecord | undefined {
    return this.db.prepare("SELECT * FROM artifacts WHERE id = ?").get(id) as
      | ArtifactRecord
      | undefined;
  }

  /**
   * Get artifact by source path
   */
  getArtifactByPath(sourcePath: string): ArtifactRecord | undefined {
    return this.db
      .prepare("SELECT * FROM artifacts WHERE source_path = ?")
      .get(sourcePath) as ArtifactRecord | undefined;
  }

  /**
   * Delete an artifact
   */
  deleteArtifact(id: string): void {
    this.db.prepare("DELETE FROM artifacts WHERE id = ?").run(id);
  }

  // ==========================================================================
  // Sync State Operations
  // ==========================================================================

  /**
   * Get sync state for an artifact
   */
  getSyncState(artifactId: string, targetSystem: SystemId): SyncStateRecord | undefined {
    return this.db
      .prepare(
        "SELECT * FROM artifact_sync_state WHERE artifact_id = ? AND target_system = ?"
      )
      .get(artifactId, targetSystem) as SyncStateRecord | undefined;
  }

  /**
   * Get all sync states for a target system
   */
  getSyncStatesForTarget(targetSystem: SystemId): SyncStateRecord[] {
    return this.db
      .prepare("SELECT * FROM artifact_sync_state WHERE target_system = ?")
      .all(targetSystem) as SyncStateRecord[];
  }

  /**
   * Update sync state
   */
  upsertSyncState(state: Omit<SyncStateRecord, "id">): void {
    this.db
      .prepare(
        `
      INSERT INTO artifact_sync_state (artifact_id, target_system, target_path, sync_method, synced_hash, last_synced_at, status, error_message)
      VALUES (@artifact_id, @target_system, @target_path, @sync_method, @synced_hash, @last_synced_at, @status, @error_message)
      ON CONFLICT(artifact_id, target_system) DO UPDATE SET
        target_path = @target_path,
        sync_method = @sync_method,
        synced_hash = @synced_hash,
        last_synced_at = @last_synced_at,
        status = @status,
        error_message = @error_message
    `
      )
      .run(state);
  }

  /**
   * Delete sync state for an artifact-target pair
   */
  deleteSyncState(artifactId: string, targetSystem: SystemId): void {
    this.db
      .prepare("DELETE FROM artifact_sync_state WHERE artifact_id = ? AND target_system = ?")
      .run(artifactId, targetSystem);
  }

  // ==========================================================================
  // Sync Job Operations
  // ==========================================================================

  /**
   * Create a new sync job
   */
  createSyncJob(job: Omit<SyncJobRecord, "started_at" | "completed_at" | "status">): void {
    this.db
      .prepare(
        `
      INSERT INTO sync_jobs (id, source_system, target_systems, artifact_types, dry_run, force, use_symlinks)
      VALUES (@id, @source_system, @target_systems, @artifact_types, @dry_run, @force, @use_symlinks)
    `
      )
      .run(job);
  }

  /**
   * Update sync job status
   */
  updateSyncJob(id: string, status: string, summary?: Record<string, unknown>): void {
    this.db
      .prepare(
        `
      UPDATE sync_jobs
      SET status = ?, completed_at = datetime('now'), summary = ?
      WHERE id = ?
    `
      )
      .run(status, summary ? JSON.stringify(summary) : null, id);
  }

  /**
   * Get sync job by ID
   */
  getSyncJob(id: string): SyncJobRecord | undefined {
    return this.db.prepare("SELECT * FROM sync_jobs WHERE id = ?").get(id) as
      | SyncJobRecord
      | undefined;
  }

  /**
   * Get recent sync jobs
   */
  getRecentSyncJobs(limit = 10): SyncJobRecord[] {
    return this.db
      .prepare("SELECT * FROM sync_jobs ORDER BY started_at DESC LIMIT ?")
      .all(limit) as SyncJobRecord[];
  }

  /**
   * Add sync result
   */
  addSyncResult(result: Omit<SyncResult, "timestamp"> & { job_id: string }): void {
    this.db
      .prepare(
        `
      INSERT INTO sync_results (job_id, artifact_id, artifact_name, artifact_type, source_system, target_system, operation, success, message, error, source_path, target_path)
      VALUES (@job_id, @artifactId, @artifactName, @artifactType, @sourceSystem, @targetSystem, @operation, @success, @message, @error, @sourcePath, @targetPath)
    `
      )
      .run({
        ...result,
        success: result.success ? 1 : 0,
      });
  }

  /**
   * Get sync results for a job
   */
  getSyncResults(jobId: string): SyncResult[] {
    const results = this.db
      .prepare("SELECT * FROM sync_results WHERE job_id = ?")
      .all(jobId) as Array<{
      artifact_id: string;
      artifact_name: string;
      artifact_type: string;
      source_system: string;
      target_system: string;
      operation: string;
      success: number;
      message: string | null;
      error: string | null;
      source_path: string;
      target_path: string | null;
      timestamp: string;
    }>;

    return results.map((r) => ({
      artifactId: r.artifact_id,
      artifactName: r.artifact_name,
      artifactType: r.artifact_type as ArtifactType,
      sourceSystem: r.source_system as SystemId,
      targetSystem: r.target_system as SystemId,
      operation: r.operation as SyncResult["operation"],
      success: r.success === 1,
      message: r.message ?? undefined,
      error: r.error ?? undefined,
      sourcePath: r.source_path,
      targetPath: r.target_path ?? undefined,
      timestamp: new Date(r.timestamp),
    }));
  }

  // ==========================================================================
  // Mapping Rule Operations
  // ==========================================================================

  /**
   * Get mapping rules for a source-target pair
   */
  getMappingRules(
    sourceSystem: SystemId,
    targetSystem: SystemId,
    artifactType?: ArtifactType
  ): MappingRuleRecord[] {
    let query = "SELECT * FROM mapping_rules WHERE source_system = ? AND target_system = ? AND enabled = 1";
    const params: (string | undefined)[] = [sourceSystem, targetSystem];

    if (artifactType) {
      query += " AND artifact_type = ?";
      params.push(artifactType);
    }

    query += " ORDER BY priority DESC";

    return this.db.prepare(query).all(...params) as MappingRuleRecord[];
  }

  /**
   * Add a custom mapping rule
   */
  addMappingRule(rule: Omit<MappingRuleRecord, "id" | "enabled">): void {
    this.db
      .prepare(
        `
      INSERT INTO mapping_rules (source_system, target_system, artifact_type, source_pattern, target_pattern, transform_type, use_symlink, priority)
      VALUES (@source_system, @target_system, @artifact_type, @source_pattern, @target_pattern, @transform_type, @use_symlink, @priority)
    `
      )
      .run(rule);
  }

  // ==========================================================================
  // Settings Operations
  // ==========================================================================

  /**
   * Get a setting value
   */
  getSetting(key: string): string | undefined {
    const row = this.db
      .prepare("SELECT value FROM settings WHERE key = ?")
      .get(key) as { value: string } | undefined;
    return row?.value;
  }

  /**
   * Set a setting value
   */
  setSetting(key: string, value: string): void {
    this.db
      .prepare(
        `
      INSERT INTO settings (key, value)
      VALUES (?, ?)
      ON CONFLICT(key) DO UPDATE SET value = ?, updated_at = datetime('now')
    `
      )
      .run(key, value, value);
  }

  // ==========================================================================
  // Diff Operations
  // ==========================================================================

  /**
   * Get artifacts that need syncing to a target
   */
  getOutOfSyncArtifacts(
    sourceSystem: SystemId,
    targetSystem: SystemId,
    artifactType?: ArtifactType
  ): ArtifactDiff[] {
    let query = `
      SELECT
        a.id as artifact_id,
        a.name as artifact_name,
        a.type as artifact_type,
        a.source_system,
        ? as target_system,
        a.content_hash as source_checksum,
        ss.synced_hash as target_checksum,
        a.source_path,
        ss.target_path,
        CASE
          WHEN ss.id IS NULL THEN 'missing'
          WHEN ss.synced_hash != a.content_hash THEN 'modified'
          ELSE 'unchanged'
        END as status
      FROM artifacts a
      LEFT JOIN artifact_sync_state ss ON a.id = ss.artifact_id AND ss.target_system = ?
      WHERE a.source_system = ?
    `;
    const params: (string | undefined)[] = [targetSystem, targetSystem, sourceSystem];

    if (artifactType) {
      query += " AND a.type = ?";
      params.push(artifactType);
    }

    const results = this.db.prepare(query).all(...params) as Array<{
      artifact_id: string;
      artifact_name: string;
      artifact_type: string;
      source_system: string;
      target_system: string;
      source_checksum: string;
      target_checksum: string | null;
      source_path: string;
      target_path: string | null;
      status: string;
    }>;

    return results.map((r) => ({
      artifactId: r.artifact_id,
      artifactName: r.artifact_name,
      artifactType: r.artifact_type as ArtifactType,
      sourceSystem: r.source_system as SystemId,
      targetSystem: r.target_system as SystemId,
      status: r.status as ArtifactDiff["status"],
      sourceChecksum: r.source_checksum,
      targetChecksum: r.target_checksum ?? undefined,
      sourcePath: r.source_path,
      targetPath: r.target_path ?? undefined,
    }));
  }

  /**
   * Get database path
   */
  getDatabasePath(): string {
    return this.dbPath;
  }

  /**
   * Get database statistics
   */
  getStats(): {
    systems: number;
    artifacts: number;
    syncStates: number;
    syncJobs: number;
    mappingRules: number;
  } {
    return {
      systems: (this.db.prepare("SELECT COUNT(*) as c FROM systems").get() as { c: number }).c,
      artifacts: (this.db.prepare("SELECT COUNT(*) as c FROM artifacts").get() as { c: number }).c,
      syncStates: (
        this.db.prepare("SELECT COUNT(*) as c FROM artifact_sync_state").get() as { c: number }
      ).c,
      syncJobs: (this.db.prepare("SELECT COUNT(*) as c FROM sync_jobs").get() as { c: number }).c,
      mappingRules: (
        this.db.prepare("SELECT COUNT(*) as c FROM mapping_rules").get() as { c: number }
      ).c,
    };
  }
}
