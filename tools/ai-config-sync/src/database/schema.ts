/**
 * SQLite Database Schema for AI Config Sync
 *
 * Stores sync state, artifact metadata, and history for
 * configuration synchronization across AI assistants.
 */

export const SCHEMA_VERSION = 1;

/**
 * SQL statements to create the database schema
 */
export const CREATE_TABLES = `
-- Schema version tracking
CREATE TABLE IF NOT EXISTS schema_version (
  version INTEGER PRIMARY KEY,
  applied_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Registered AI systems
CREATE TABLE IF NOT EXISTS systems (
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  description TEXT,
  capabilities TEXT NOT NULL, -- JSON
  paths TEXT NOT NULL, -- JSON
  config_format TEXT NOT NULL,
  file_patterns TEXT NOT NULL, -- JSON
  enabled INTEGER NOT NULL DEFAULT 1,
  created_at TEXT NOT NULL DEFAULT (datetime('now')),
  updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Artifacts (skills, agents, commands, hooks, rules, etc.)
CREATE TABLE IF NOT EXISTS artifacts (
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  type TEXT NOT NULL,
  description TEXT,
  content_hash TEXT NOT NULL,
  source_system TEXT NOT NULL REFERENCES systems(id),
  source_path TEXT NOT NULL,
  metadata TEXT, -- JSON
  created_at TEXT NOT NULL DEFAULT (datetime('now')),
  updated_at TEXT NOT NULL DEFAULT (datetime('now')),
  UNIQUE(source_system, type, name)
);

-- Artifact sync state per target system
CREATE TABLE IF NOT EXISTS artifact_sync_state (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  artifact_id TEXT NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,
  target_system TEXT NOT NULL REFERENCES systems(id),
  target_path TEXT,
  sync_method TEXT NOT NULL, -- 'copy', 'symlink', 'transform'
  synced_hash TEXT,
  last_synced_at TEXT,
  status TEXT NOT NULL DEFAULT 'pending', -- 'pending', 'synced', 'failed', 'skipped'
  error_message TEXT,
  UNIQUE(artifact_id, target_system)
);

-- Sync jobs history
CREATE TABLE IF NOT EXISTS sync_jobs (
  id TEXT PRIMARY KEY,
  source_system TEXT NOT NULL REFERENCES systems(id),
  target_systems TEXT NOT NULL, -- JSON array
  artifact_types TEXT NOT NULL, -- JSON array
  dry_run INTEGER NOT NULL DEFAULT 0,
  force INTEGER NOT NULL DEFAULT 0,
  use_symlinks INTEGER NOT NULL DEFAULT 1,
  started_at TEXT NOT NULL DEFAULT (datetime('now')),
  completed_at TEXT,
  status TEXT NOT NULL DEFAULT 'running', -- 'running', 'completed', 'failed', 'cancelled'
  summary TEXT -- JSON
);

-- Sync results (individual artifact sync results per job)
CREATE TABLE IF NOT EXISTS sync_results (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  job_id TEXT NOT NULL REFERENCES sync_jobs(id) ON DELETE CASCADE,
  artifact_id TEXT NOT NULL,
  artifact_name TEXT NOT NULL,
  artifact_type TEXT NOT NULL,
  source_system TEXT NOT NULL,
  target_system TEXT NOT NULL,
  operation TEXT NOT NULL, -- 'create', 'update', 'delete', 'skip', 'conflict', 'symlink'
  success INTEGER NOT NULL,
  message TEXT,
  error TEXT,
  source_path TEXT NOT NULL,
  target_path TEXT,
  timestamp TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Mapping rules between systems
CREATE TABLE IF NOT EXISTS mapping_rules (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  source_system TEXT NOT NULL REFERENCES systems(id),
  target_system TEXT NOT NULL REFERENCES systems(id),
  artifact_type TEXT NOT NULL,
  source_pattern TEXT NOT NULL, -- glob pattern
  target_pattern TEXT NOT NULL, -- output path template
  transform_type TEXT, -- 'none', 'yaml_to_mdc', 'md_to_json', etc.
  use_symlink INTEGER NOT NULL DEFAULT 0,
  priority INTEGER NOT NULL DEFAULT 0,
  enabled INTEGER NOT NULL DEFAULT 1,
  UNIQUE(source_system, target_system, artifact_type, source_pattern)
);

-- Conflict resolutions
CREATE TABLE IF NOT EXISTS conflict_resolutions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  artifact_id TEXT NOT NULL,
  source_system TEXT NOT NULL,
  target_system TEXT NOT NULL,
  resolution TEXT NOT NULL, -- 'source_wins', 'target_wins', 'manual', 'merge'
  resolved_at TEXT NOT NULL DEFAULT (datetime('now')),
  resolved_by TEXT -- 'auto' or user identifier
);

-- Configuration settings
CREATE TABLE IF NOT EXISTS settings (
  key TEXT PRIMARY KEY,
  value TEXT NOT NULL,
  updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_artifacts_source ON artifacts(source_system);
CREATE INDEX IF NOT EXISTS idx_artifacts_type ON artifacts(type);
CREATE INDEX IF NOT EXISTS idx_artifact_sync_state_artifact ON artifact_sync_state(artifact_id);
CREATE INDEX IF NOT EXISTS idx_artifact_sync_state_target ON artifact_sync_state(target_system);
CREATE INDEX IF NOT EXISTS idx_sync_results_job ON sync_results(job_id);
CREATE INDEX IF NOT EXISTS idx_mapping_rules_source ON mapping_rules(source_system, artifact_type);

-- Insert schema version
INSERT OR REPLACE INTO schema_version (version) VALUES (${SCHEMA_VERSION});
`;

/**
 * Default system definitions to seed the database
 */
export const DEFAULT_SYSTEMS = [
  {
    id: "claude",
    name: "Claude Code",
    description: "Anthropic Claude Code CLI",
    capabilities: JSON.stringify({
      skills: true,
      agents: true,
      commands: true,
      hooks: true,
      rules: true,
      mcpServers: true,
      templates: true,
      contextFiles: true,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".claude",
      skills: ".claude/skills",
      agents: ".claude/agents",
      commands: ".claude/commands",
      hooks: ".claude/hooks",
      rules: ".claude/rules",
      mcpServers: ".claude/mcp-servers.json",
      templates: ".claude/templates",
      context: ".claude/context",
      instructions: "CLAUDE.md",
      config: ".claude/settings.json",
    }),
    config_format: "json",
    file_patterns: JSON.stringify({
      skill: "**/SKILL.md",
      agent: "**/*.md",
      command: "**/*.md",
      hook: "**/*.sh",
      rule: "**/*.md",
    }),
  },
  {
    id: "opencode",
    name: "OpenCode",
    description: "OpenCode AI CLI",
    capabilities: JSON.stringify({
      skills: true,
      agents: true,
      commands: true,
      hooks: false,
      rules: false,
      mcpServers: true,
      templates: true,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".opencode",
      skills: ".opencode/skill",
      agents: ".opencode/agent",
      commands: ".opencode/commands",
      templates: ".opencode/templates",
      instructions: "AGENTS.md",
      config: ".opencode/opencode.json",
    }),
    config_format: "json",
    file_patterns: JSON.stringify({
      skill: "**/SKILL.md",
      agent: "**/*.md",
      command: "**/*.md",
    }),
  },
  {
    id: "cursor",
    name: "Cursor IDE",
    description: "Cursor AI-powered IDE",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: false,
      hooks: false,
      rules: true,
      mcpServers: true,
      templates: false,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".cursor",
      rules: ".cursor/rules",
      mcpServers: ".cursor/mcp.json",
      instructions: ".cursorrules",
      config: ".cursor/settings.json",
    }),
    config_format: "mdc",
    file_patterns: JSON.stringify({
      rule: "**/*.mdc",
    }),
  },
  {
    id: "codex",
    name: "OpenAI Codex CLI",
    description: "OpenAI Codex CLI",
    capabilities: JSON.stringify({
      skills: true,
      agents: true,
      commands: true,
      hooks: false,
      rules: false,
      mcpServers: true,
      templates: false,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".codex",
      skills: ".codex/skills",
      agents: ".codex/agents",
      commands: ".codex/commands",
      instructions: "AGENTS.md",
      config: ".codex/config.toml",
    }),
    config_format: "toml",
    file_patterns: JSON.stringify({
      skill: "**/SKILL.md",
      agent: "**/*.md",
      command: "**/*.md",
    }),
  },
  {
    id: "windsurf",
    name: "Windsurf IDE",
    description: "Codeium Windsurf AI IDE",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: false,
      hooks: false,
      rules: true,
      mcpServers: true,
      templates: false,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".windsurf",
      rules: ".windsurf/rules",
      mcpServers: ".windsurf/mcp.json",
      instructions: ".windsurfrules",
      config: ".windsurf/settings.json",
    }),
    config_format: "markdown",
    file_patterns: JSON.stringify({
      rule: "**/*.md",
    }),
  },
  {
    id: "aider",
    name: "Aider",
    description: "Aider AI pair programming",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: false,
      hooks: false,
      rules: false,
      mcpServers: false,
      templates: false,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".",
      instructions: ".aider.conf.yml",
      config: ".aider.conf.yml",
    }),
    config_format: "yaml",
    file_patterns: JSON.stringify({}),
  },
  {
    id: "gemini",
    name: "Google Gemini",
    description: "Google Gemini Code Assist",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: false,
      hooks: false,
      rules: false,
      mcpServers: false,
      templates: false,
      contextFiles: false,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".",
      instructions: "GEMINI.md",
    }),
    config_format: "markdown",
    file_patterns: JSON.stringify({}),
  },
  {
    id: "continue",
    name: "Continue.dev",
    description: "Continue AI coding assistant",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: false,
      hooks: false,
      rules: true,
      mcpServers: false,
      templates: false,
      contextFiles: true,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".continue",
      rules: ".continue/rules",
      context: ".continue/context",
      instructions: ".continuerc.json",
      config: ".continue/config.json",
    }),
    config_format: "json",
    file_patterns: JSON.stringify({
      rule: "**/*.md",
    }),
  },
  {
    id: "cody",
    name: "Sourcegraph Cody",
    description: "Sourcegraph Cody AI assistant",
    capabilities: JSON.stringify({
      skills: false,
      agents: false,
      commands: true,
      hooks: false,
      rules: false,
      mcpServers: false,
      templates: false,
      contextFiles: true,
      instructions: true,
      symlinksSupported: true,
    }),
    paths: JSON.stringify({
      root: ".cody",
      commands: ".cody/commands",
      context: ".cody/context",
      instructions: ".cody/instructions.md",
      config: ".cody/cody.json",
    }),
    config_format: "json",
    file_patterns: JSON.stringify({
      command: "**/*.json",
    }),
  },
];

/**
 * Default mapping rules
 */
export const DEFAULT_MAPPING_RULES = [
  // Claude -> OpenCode (symlink skills)
  {
    source_system: "claude",
    target_system: "opencode",
    artifact_type: "skill",
    source_pattern: ".claude/skills/*/SKILL.md",
    target_pattern: ".opencode/skill/{name}",
    transform_type: "none",
    use_symlink: 1,
    priority: 100,
  },
  // Claude -> OpenCode (symlink agents)
  {
    source_system: "claude",
    target_system: "opencode",
    artifact_type: "agent",
    source_pattern: ".claude/agents/*.md",
    target_pattern: ".opencode/agent/{name}.md",
    transform_type: "none",
    use_symlink: 1,
    priority: 100,
  },
  // Claude -> Codex (copy skills)
  {
    source_system: "claude",
    target_system: "codex",
    artifact_type: "skill",
    source_pattern: ".claude/skills/*/SKILL.md",
    target_pattern: ".codex/skills/{name}",
    transform_type: "none",
    use_symlink: 0,
    priority: 100,
  },
  // Claude -> Codex (copy agents)
  {
    source_system: "claude",
    target_system: "codex",
    artifact_type: "agent",
    source_pattern: ".claude/agents/*.md",
    target_pattern: ".codex/agents/{name}.md",
    transform_type: "none",
    use_symlink: 0,
    priority: 100,
  },
  // Claude -> Cursor (transform to .mdc)
  {
    source_system: "claude",
    target_system: "cursor",
    artifact_type: "skill",
    source_pattern: ".claude/skills/*/SKILL.md",
    target_pattern: ".cursor/rules/{name}.mdc",
    transform_type: "skill_to_mdc",
    use_symlink: 0,
    priority: 100,
  },
  // Claude -> Windsurf (transform to markdown rules)
  {
    source_system: "claude",
    target_system: "windsurf",
    artifact_type: "skill",
    source_pattern: ".claude/skills/*/SKILL.md",
    target_pattern: ".windsurf/rules/{name}.md",
    transform_type: "skill_to_windsurf",
    use_symlink: 0,
    priority: 100,
  },
  // Claude rules -> Cursor rules
  {
    source_system: "claude",
    target_system: "cursor",
    artifact_type: "rule",
    source_pattern: ".claude/rules/*.md",
    target_pattern: ".cursor/rules/{name}.mdc",
    transform_type: "md_to_mdc",
    use_symlink: 0,
    priority: 90,
  },
  // Claude rules -> Windsurf rules
  {
    source_system: "claude",
    target_system: "windsurf",
    artifact_type: "rule",
    source_pattern: ".claude/rules/*.md",
    target_pattern: ".windsurf/rules/{name}.md",
    transform_type: "none",
    use_symlink: 1,
    priority: 90,
  },
];

/**
 * Default settings
 */
export const DEFAULT_SETTINGS = [
  { key: "default_source", value: "claude" },
  { key: "default_targets", value: JSON.stringify(["opencode", "cursor", "codex"]) },
  { key: "use_symlinks", value: "true" },
  { key: "verbose", value: "false" },
  { key: "last_sync", value: "" },
];
