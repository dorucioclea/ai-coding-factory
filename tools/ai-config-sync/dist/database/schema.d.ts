/**
 * SQLite Database Schema for AI Config Sync
 *
 * Stores sync state, artifact metadata, and history for
 * configuration synchronization across AI assistants.
 */
export declare const SCHEMA_VERSION = 1;
/**
 * SQL statements to create the database schema
 */
export declare const CREATE_TABLES = "\n-- Schema version tracking\nCREATE TABLE IF NOT EXISTS schema_version (\n  version INTEGER PRIMARY KEY,\n  applied_at TEXT NOT NULL DEFAULT (datetime('now'))\n);\n\n-- Registered AI systems\nCREATE TABLE IF NOT EXISTS systems (\n  id TEXT PRIMARY KEY,\n  name TEXT NOT NULL,\n  description TEXT,\n  capabilities TEXT NOT NULL, -- JSON\n  paths TEXT NOT NULL, -- JSON\n  config_format TEXT NOT NULL,\n  file_patterns TEXT NOT NULL, -- JSON\n  enabled INTEGER NOT NULL DEFAULT 1,\n  created_at TEXT NOT NULL DEFAULT (datetime('now')),\n  updated_at TEXT NOT NULL DEFAULT (datetime('now'))\n);\n\n-- Artifacts (skills, agents, commands, hooks, rules, etc.)\nCREATE TABLE IF NOT EXISTS artifacts (\n  id TEXT PRIMARY KEY,\n  name TEXT NOT NULL,\n  type TEXT NOT NULL,\n  description TEXT,\n  content_hash TEXT NOT NULL,\n  source_system TEXT NOT NULL REFERENCES systems(id),\n  source_path TEXT NOT NULL,\n  metadata TEXT, -- JSON\n  created_at TEXT NOT NULL DEFAULT (datetime('now')),\n  updated_at TEXT NOT NULL DEFAULT (datetime('now')),\n  UNIQUE(source_system, type, name)\n);\n\n-- Artifact sync state per target system\nCREATE TABLE IF NOT EXISTS artifact_sync_state (\n  id INTEGER PRIMARY KEY AUTOINCREMENT,\n  artifact_id TEXT NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,\n  target_system TEXT NOT NULL REFERENCES systems(id),\n  target_path TEXT,\n  sync_method TEXT NOT NULL, -- 'copy', 'symlink', 'transform'\n  synced_hash TEXT,\n  last_synced_at TEXT,\n  status TEXT NOT NULL DEFAULT 'pending', -- 'pending', 'synced', 'failed', 'skipped'\n  error_message TEXT,\n  UNIQUE(artifact_id, target_system)\n);\n\n-- Sync jobs history\nCREATE TABLE IF NOT EXISTS sync_jobs (\n  id TEXT PRIMARY KEY,\n  source_system TEXT NOT NULL REFERENCES systems(id),\n  target_systems TEXT NOT NULL, -- JSON array\n  artifact_types TEXT NOT NULL, -- JSON array\n  dry_run INTEGER NOT NULL DEFAULT 0,\n  force INTEGER NOT NULL DEFAULT 0,\n  use_symlinks INTEGER NOT NULL DEFAULT 1,\n  started_at TEXT NOT NULL DEFAULT (datetime('now')),\n  completed_at TEXT,\n  status TEXT NOT NULL DEFAULT 'running', -- 'running', 'completed', 'failed', 'cancelled'\n  summary TEXT -- JSON\n);\n\n-- Sync results (individual artifact sync results per job)\nCREATE TABLE IF NOT EXISTS sync_results (\n  id INTEGER PRIMARY KEY AUTOINCREMENT,\n  job_id TEXT NOT NULL REFERENCES sync_jobs(id) ON DELETE CASCADE,\n  artifact_id TEXT NOT NULL,\n  artifact_name TEXT NOT NULL,\n  artifact_type TEXT NOT NULL,\n  source_system TEXT NOT NULL,\n  target_system TEXT NOT NULL,\n  operation TEXT NOT NULL, -- 'create', 'update', 'delete', 'skip', 'conflict', 'symlink'\n  success INTEGER NOT NULL,\n  message TEXT,\n  error TEXT,\n  source_path TEXT NOT NULL,\n  target_path TEXT,\n  timestamp TEXT NOT NULL DEFAULT (datetime('now'))\n);\n\n-- Mapping rules between systems\nCREATE TABLE IF NOT EXISTS mapping_rules (\n  id INTEGER PRIMARY KEY AUTOINCREMENT,\n  source_system TEXT NOT NULL REFERENCES systems(id),\n  target_system TEXT NOT NULL REFERENCES systems(id),\n  artifact_type TEXT NOT NULL,\n  source_pattern TEXT NOT NULL, -- glob pattern\n  target_pattern TEXT NOT NULL, -- output path template\n  transform_type TEXT, -- 'none', 'yaml_to_mdc', 'md_to_json', etc.\n  use_symlink INTEGER NOT NULL DEFAULT 0,\n  priority INTEGER NOT NULL DEFAULT 0,\n  enabled INTEGER NOT NULL DEFAULT 1,\n  UNIQUE(source_system, target_system, artifact_type, source_pattern)\n);\n\n-- Conflict resolutions\nCREATE TABLE IF NOT EXISTS conflict_resolutions (\n  id INTEGER PRIMARY KEY AUTOINCREMENT,\n  artifact_id TEXT NOT NULL,\n  source_system TEXT NOT NULL,\n  target_system TEXT NOT NULL,\n  resolution TEXT NOT NULL, -- 'source_wins', 'target_wins', 'manual', 'merge'\n  resolved_at TEXT NOT NULL DEFAULT (datetime('now')),\n  resolved_by TEXT -- 'auto' or user identifier\n);\n\n-- Configuration settings\nCREATE TABLE IF NOT EXISTS settings (\n  key TEXT PRIMARY KEY,\n  value TEXT NOT NULL,\n  updated_at TEXT NOT NULL DEFAULT (datetime('now'))\n);\n\n-- Create indexes for performance\nCREATE INDEX IF NOT EXISTS idx_artifacts_source ON artifacts(source_system);\nCREATE INDEX IF NOT EXISTS idx_artifacts_type ON artifacts(type);\nCREATE INDEX IF NOT EXISTS idx_artifact_sync_state_artifact ON artifact_sync_state(artifact_id);\nCREATE INDEX IF NOT EXISTS idx_artifact_sync_state_target ON artifact_sync_state(target_system);\nCREATE INDEX IF NOT EXISTS idx_sync_results_job ON sync_results(job_id);\nCREATE INDEX IF NOT EXISTS idx_mapping_rules_source ON mapping_rules(source_system, artifact_type);\n\n-- Insert schema version\nINSERT OR REPLACE INTO schema_version (version) VALUES (1);\n";
/**
 * Default system definitions to seed the database
 */
export declare const DEFAULT_SYSTEMS: {
    id: string;
    name: string;
    description: string;
    capabilities: string;
    paths: string;
    config_format: string;
    file_patterns: string;
}[];
/**
 * Default mapping rules
 */
export declare const DEFAULT_MAPPING_RULES: {
    source_system: string;
    target_system: string;
    artifact_type: string;
    source_pattern: string;
    target_pattern: string;
    transform_type: string;
    use_symlink: number;
    priority: number;
}[];
/**
 * Default settings
 */
export declare const DEFAULT_SETTINGS: {
    key: string;
    value: string;
}[];
//# sourceMappingURL=schema.d.ts.map