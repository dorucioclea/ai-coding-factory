/**
 * AI Config Sync - Core Type Definitions
 *
 * This module defines the core types for synchronizing configurations
 * across different AI coding assistants.
 */
import { z } from "zod";
// =============================================================================
// System Types
// =============================================================================
/**
 * Supported AI coding assistant systems
 */
export const SystemId = z.enum([
    "claude",
    "opencode",
    "cursor",
    "codex",
    "windsurf",
    "aider",
    "gemini",
    "continue",
    "cody",
]);
/**
 * Configuration format types
 */
export const ConfigFormat = z.enum(["json", "yaml", "toml", "markdown", "mdc"]);
/**
 * Artifact types that can be synchronized
 */
export const ArtifactType = z.enum([
    "skill",
    "agent",
    "command",
    "hook",
    "rule",
    "mcp_server",
    "template",
    "context",
    "instruction",
]);
// =============================================================================
// Capability Schemas
// =============================================================================
/**
 * System capabilities - what each AI assistant supports
 */
export const SystemCapabilities = z.object({
    skills: z.boolean().default(false),
    agents: z.boolean().default(false),
    commands: z.boolean().default(false),
    hooks: z.boolean().default(false),
    rules: z.boolean().default(false),
    mcpServers: z.boolean().default(false),
    templates: z.boolean().default(false),
    contextFiles: z.boolean().default(false),
    instructions: z.boolean().default(false),
    symlinksSupported: z.boolean().default(true),
});
/**
 * System configuration paths
 */
export const SystemPaths = z.object({
    root: z.string(),
    skills: z.string().optional(),
    agents: z.string().optional(),
    commands: z.string().optional(),
    hooks: z.string().optional(),
    rules: z.string().optional(),
    mcpServers: z.string().optional(),
    templates: z.string().optional(),
    context: z.string().optional(),
    instructions: z.string().optional(),
    config: z.string().optional(),
});
// =============================================================================
// Artifact Schemas
// =============================================================================
/**
 * Base artifact schema
 */
export const BaseArtifact = z.object({
    id: z.string(),
    name: z.string(),
    type: ArtifactType,
    description: z.string().optional(),
    content: z.string(),
    metadata: z.record(z.unknown()).optional(),
    sourceSystem: SystemId,
    sourcePath: z.string(),
    checksum: z.string(),
    lastModified: z.date(),
});
/**
 * Skill artifact
 */
export const SkillArtifact = BaseArtifact.extend({
    type: z.literal("skill"),
    triggers: z.array(z.string()).optional(),
    globs: z.array(z.string()).optional(),
    references: z.array(z.string()).optional(),
    assets: z.array(z.string()).optional(),
});
/**
 * Agent artifact
 */
export const AgentArtifact = BaseArtifact.extend({
    type: z.literal("agent"),
    role: z.string().optional(),
    tools: z.array(z.string()).optional(),
    temperature: z.number().optional(),
});
/**
 * Command artifact
 */
export const CommandArtifact = BaseArtifact.extend({
    type: z.literal("command"),
    command: z.string(),
    aliases: z.array(z.string()).optional(),
});
/**
 * Hook artifact
 */
export const HookArtifact = BaseArtifact.extend({
    type: z.literal("hook"),
    hookType: z.enum([
        "pre_tool_use",
        "post_tool_use",
        "session_start",
        "session_end",
        "pre_compact",
        "user_prompt_submit",
        "stop",
    ]),
    toolMatch: z.string().optional(),
    script: z.string(),
});
/**
 * Rule artifact (for Cursor .mdc files)
 */
export const RuleArtifact = BaseArtifact.extend({
    type: z.literal("rule"),
    globs: z.array(z.string()).optional(),
    alwaysApply: z.boolean().default(false),
});
/**
 * MCP Server artifact
 */
export const McpServerArtifact = BaseArtifact.extend({
    type: z.literal("mcp_server"),
    command: z.array(z.string()),
    env: z.record(z.string()).optional(),
    enabled: z.boolean().default(false),
});
// =============================================================================
// Sync Schemas
// =============================================================================
/**
 * Sync operation types
 */
export const SyncOperation = z.enum([
    "create",
    "update",
    "delete",
    "skip",
    "conflict",
    "symlink",
]);
/**
 * Sync result for a single artifact
 */
export const SyncResult = z.object({
    artifactId: z.string(),
    artifactName: z.string(),
    artifactType: ArtifactType,
    sourceSystem: SystemId,
    targetSystem: SystemId,
    operation: SyncOperation,
    success: z.boolean(),
    message: z.string().optional(),
    error: z.string().optional(),
    sourcePath: z.string(),
    targetPath: z.string().optional(),
    timestamp: z.date(),
});
/**
 * Sync job configuration
 */
export const SyncJob = z.object({
    id: z.string(),
    source: SystemId,
    targets: z.array(SystemId),
    artifactTypes: z.array(ArtifactType),
    dryRun: z.boolean().default(false),
    force: z.boolean().default(false),
    useSymlinks: z.boolean().default(true),
    startedAt: z.date(),
    completedAt: z.date().optional(),
    results: z.array(SyncResult).default([]),
});
// =============================================================================
// Mapping Schemas
// =============================================================================
/**
 * Transformation rule for mapping between systems
 */
export const TransformRule = z.object({
    sourceFormat: ConfigFormat,
    targetFormat: ConfigFormat,
    transform: z.function().args(z.unknown()).returns(z.unknown()).optional(),
});
/**
 * System mapping configuration
 */
export const SystemMapping = z.object({
    sourceSystem: SystemId,
    targetSystem: SystemId,
    artifactType: ArtifactType,
    sourcePath: z.string(),
    targetPath: z.string(),
    transformRule: TransformRule.optional(),
    useSymlink: z.boolean().default(false),
});
// =============================================================================
// Configuration Schemas
// =============================================================================
/**
 * CLI configuration
 */
export const CliConfig = z.object({
    projectRoot: z.string(),
    databasePath: z.string(),
    defaultSource: SystemId.default("claude"),
    defaultTargets: z.array(SystemId).default(["opencode", "cursor", "codex"]),
    useSymlinks: z.boolean().default(true),
    verbose: z.boolean().default(false),
});
/**
 * System definition for the registry
 */
export const SystemDefinition = z.object({
    id: SystemId,
    name: z.string(),
    description: z.string(),
    capabilities: SystemCapabilities,
    paths: SystemPaths,
    configFormat: ConfigFormat,
    filePatterns: z.record(z.string()),
});
// =============================================================================
// Diff Types
// =============================================================================
/**
 * Difference between source and target
 */
export const ArtifactDiff = z.object({
    artifactId: z.string(),
    artifactName: z.string(),
    artifactType: ArtifactType,
    sourceSystem: SystemId,
    targetSystem: SystemId,
    status: z.enum(["added", "modified", "deleted", "unchanged", "missing"]),
    sourceChecksum: z.string().optional(),
    targetChecksum: z.string().optional(),
    sourcePath: z.string().optional(),
    targetPath: z.string().optional(),
});
// =============================================================================
// Helper Functions
// =============================================================================
/**
 * Generate artifact ID from name and type
 */
export function generateArtifactId(name, type, system) {
    return `${system}:${type}:${name.toLowerCase().replace(/[^a-z0-9-]/g, "-")}`;
}
/**
 * Parse artifact ID
 */
export function parseArtifactId(id) {
    const parts = id.split(":");
    if (parts.length !== 3)
        return null;
    const [system, type, name] = parts;
    try {
        return {
            system: SystemId.parse(system),
            type: ArtifactType.parse(type),
            name,
        };
    }
    catch {
        return null;
    }
}
//# sourceMappingURL=types.js.map