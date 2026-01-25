/**
 * Database Manager for AI Config Sync
 *
 * Manages SQLite database operations for storing and retrieving
 * sync state, artifacts, and configuration.
 */
import Database from "better-sqlite3";
import { existsSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { CREATE_TABLES, DEFAULT_MAPPING_RULES, DEFAULT_SETTINGS, DEFAULT_SYSTEMS, SCHEMA_VERSION, } from "./schema.js";
/**
 * Database Manager class
 */
export class DatabaseManager {
    db;
    dbPath;
    constructor(projectRoot, dbFileName = "ai-config-sync.db") {
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
    initialize() {
        // Check if schema exists and is up to date
        const versionTable = this.db
            .prepare("SELECT name FROM sqlite_master WHERE type='table' AND name='schema_version'")
            .get();
        if (!versionTable) {
            // Create fresh schema
            this.db.exec(CREATE_TABLES);
            this.seedDefaults();
        }
        else {
            // Check version and migrate if needed
            const version = this.db
                .prepare("SELECT version FROM schema_version ORDER BY version DESC LIMIT 1")
                .get();
            if (!version || version.version < SCHEMA_VERSION) {
                this.migrate(version?.version ?? 0);
            }
        }
    }
    /**
     * Seed default data
     */
    seedDefaults() {
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
    migrate(fromVersion) {
        // Future migrations go here
        if (fromVersion < 1) {
            this.db.exec(CREATE_TABLES);
            this.seedDefaults();
        }
    }
    /**
     * Close database connection
     */
    close() {
        this.db.close();
    }
    // ==========================================================================
    // System Operations
    // ==========================================================================
    /**
     * Get all registered systems
     */
    getSystems() {
        return this.db.prepare("SELECT * FROM systems WHERE enabled = 1").all();
    }
    /**
     * Get a specific system by ID
     */
    getSystem(id) {
        return this.db.prepare("SELECT * FROM systems WHERE id = ?").get(id);
    }
    /**
     * Update system configuration
     */
    updateSystem(id, updates) {
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
    upsertArtifact(artifact) {
        this.db
            .prepare(`
      INSERT INTO artifacts (id, name, type, description, content_hash, source_system, source_path, metadata)
      VALUES (@id, @name, @type, @description, @content_hash, @source_system, @source_path, @metadata)
      ON CONFLICT(id) DO UPDATE SET
        name = @name,
        description = @description,
        content_hash = @content_hash,
        source_path = @source_path,
        metadata = @metadata,
        updated_at = datetime('now')
    `)
            .run(artifact);
    }
    /**
     * Get all artifacts for a system
     */
    getArtifactsBySystem(systemId) {
        return this.db
            .prepare("SELECT * FROM artifacts WHERE source_system = ?")
            .all(systemId);
    }
    /**
     * Get artifacts by type
     */
    getArtifactsByType(type) {
        return this.db
            .prepare("SELECT * FROM artifacts WHERE type = ?")
            .all(type);
    }
    /**
     * Get a specific artifact
     */
    getArtifact(id) {
        return this.db.prepare("SELECT * FROM artifacts WHERE id = ?").get(id);
    }
    /**
     * Get artifact by source path
     */
    getArtifactByPath(sourcePath) {
        return this.db
            .prepare("SELECT * FROM artifacts WHERE source_path = ?")
            .get(sourcePath);
    }
    /**
     * Delete an artifact
     */
    deleteArtifact(id) {
        this.db.prepare("DELETE FROM artifacts WHERE id = ?").run(id);
    }
    // ==========================================================================
    // Sync State Operations
    // ==========================================================================
    /**
     * Get sync state for an artifact
     */
    getSyncState(artifactId, targetSystem) {
        return this.db
            .prepare("SELECT * FROM artifact_sync_state WHERE artifact_id = ? AND target_system = ?")
            .get(artifactId, targetSystem);
    }
    /**
     * Get all sync states for a target system
     */
    getSyncStatesForTarget(targetSystem) {
        return this.db
            .prepare("SELECT * FROM artifact_sync_state WHERE target_system = ?")
            .all(targetSystem);
    }
    /**
     * Update sync state
     */
    upsertSyncState(state) {
        this.db
            .prepare(`
      INSERT INTO artifact_sync_state (artifact_id, target_system, target_path, sync_method, synced_hash, last_synced_at, status, error_message)
      VALUES (@artifact_id, @target_system, @target_path, @sync_method, @synced_hash, @last_synced_at, @status, @error_message)
      ON CONFLICT(artifact_id, target_system) DO UPDATE SET
        target_path = @target_path,
        sync_method = @sync_method,
        synced_hash = @synced_hash,
        last_synced_at = @last_synced_at,
        status = @status,
        error_message = @error_message
    `)
            .run(state);
    }
    // ==========================================================================
    // Sync Job Operations
    // ==========================================================================
    /**
     * Create a new sync job
     */
    createSyncJob(job) {
        this.db
            .prepare(`
      INSERT INTO sync_jobs (id, source_system, target_systems, artifact_types, dry_run, force, use_symlinks)
      VALUES (@id, @source_system, @target_systems, @artifact_types, @dry_run, @force, @use_symlinks)
    `)
            .run(job);
    }
    /**
     * Update sync job status
     */
    updateSyncJob(id, status, summary) {
        this.db
            .prepare(`
      UPDATE sync_jobs
      SET status = ?, completed_at = datetime('now'), summary = ?
      WHERE id = ?
    `)
            .run(status, summary ? JSON.stringify(summary) : null, id);
    }
    /**
     * Get sync job by ID
     */
    getSyncJob(id) {
        return this.db.prepare("SELECT * FROM sync_jobs WHERE id = ?").get(id);
    }
    /**
     * Get recent sync jobs
     */
    getRecentSyncJobs(limit = 10) {
        return this.db
            .prepare("SELECT * FROM sync_jobs ORDER BY started_at DESC LIMIT ?")
            .all(limit);
    }
    /**
     * Add sync result
     */
    addSyncResult(result) {
        this.db
            .prepare(`
      INSERT INTO sync_results (job_id, artifact_id, artifact_name, artifact_type, source_system, target_system, operation, success, message, error, source_path, target_path)
      VALUES (@job_id, @artifactId, @artifactName, @artifactType, @sourceSystem, @targetSystem, @operation, @success, @message, @error, @sourcePath, @targetPath)
    `)
            .run({
            ...result,
            success: result.success ? 1 : 0,
        });
    }
    /**
     * Get sync results for a job
     */
    getSyncResults(jobId) {
        const results = this.db
            .prepare("SELECT * FROM sync_results WHERE job_id = ?")
            .all(jobId);
        return results.map((r) => ({
            artifactId: r.artifact_id,
            artifactName: r.artifact_name,
            artifactType: r.artifact_type,
            sourceSystem: r.source_system,
            targetSystem: r.target_system,
            operation: r.operation,
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
    getMappingRules(sourceSystem, targetSystem, artifactType) {
        let query = "SELECT * FROM mapping_rules WHERE source_system = ? AND target_system = ? AND enabled = 1";
        const params = [sourceSystem, targetSystem];
        if (artifactType) {
            query += " AND artifact_type = ?";
            params.push(artifactType);
        }
        query += " ORDER BY priority DESC";
        return this.db.prepare(query).all(...params);
    }
    /**
     * Add a custom mapping rule
     */
    addMappingRule(rule) {
        this.db
            .prepare(`
      INSERT INTO mapping_rules (source_system, target_system, artifact_type, source_pattern, target_pattern, transform_type, use_symlink, priority)
      VALUES (@source_system, @target_system, @artifact_type, @source_pattern, @target_pattern, @transform_type, @use_symlink, @priority)
    `)
            .run(rule);
    }
    // ==========================================================================
    // Settings Operations
    // ==========================================================================
    /**
     * Get a setting value
     */
    getSetting(key) {
        const row = this.db
            .prepare("SELECT value FROM settings WHERE key = ?")
            .get(key);
        return row?.value;
    }
    /**
     * Set a setting value
     */
    setSetting(key, value) {
        this.db
            .prepare(`
      INSERT INTO settings (key, value)
      VALUES (?, ?)
      ON CONFLICT(key) DO UPDATE SET value = ?, updated_at = datetime('now')
    `)
            .run(key, value, value);
    }
    // ==========================================================================
    // Diff Operations
    // ==========================================================================
    /**
     * Get artifacts that need syncing to a target
     */
    getOutOfSyncArtifacts(sourceSystem, targetSystem, artifactType) {
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
        const params = [targetSystem, targetSystem, sourceSystem];
        if (artifactType) {
            query += " AND a.type = ?";
            params.push(artifactType);
        }
        const results = this.db.prepare(query).all(...params);
        return results.map((r) => ({
            artifactId: r.artifact_id,
            artifactName: r.artifact_name,
            artifactType: r.artifact_type,
            sourceSystem: r.source_system,
            targetSystem: r.target_system,
            status: r.status,
            sourceChecksum: r.source_checksum,
            targetChecksum: r.target_checksum ?? undefined,
            sourcePath: r.source_path,
            targetPath: r.target_path ?? undefined,
        }));
    }
    /**
     * Get database path
     */
    getDatabasePath() {
        return this.dbPath;
    }
    /**
     * Get database statistics
     */
    getStats() {
        return {
            systems: this.db.prepare("SELECT COUNT(*) as c FROM systems").get().c,
            artifacts: this.db.prepare("SELECT COUNT(*) as c FROM artifacts").get().c,
            syncStates: this.db.prepare("SELECT COUNT(*) as c FROM artifact_sync_state").get().c,
            syncJobs: this.db.prepare("SELECT COUNT(*) as c FROM sync_jobs").get().c,
            mappingRules: this.db.prepare("SELECT COUNT(*) as c FROM mapping_rules").get().c,
        };
    }
}
//# sourceMappingURL=manager.js.map