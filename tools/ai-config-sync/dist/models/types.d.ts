/**
 * AI Config Sync - Core Type Definitions
 *
 * This module defines the core types for synchronizing configurations
 * across different AI coding assistants.
 */
import { z } from "zod";
/**
 * Supported AI coding assistant systems
 */
export declare const SystemId: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
export type SystemId = z.infer<typeof SystemId>;
/**
 * Configuration format types
 */
export declare const ConfigFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
export type ConfigFormat = z.infer<typeof ConfigFormat>;
/**
 * Artifact types that can be synchronized
 */
export declare const ArtifactType: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
export type ArtifactType = z.infer<typeof ArtifactType>;
/**
 * System capabilities - what each AI assistant supports
 */
export declare const SystemCapabilities: z.ZodObject<{
    skills: z.ZodDefault<z.ZodBoolean>;
    agents: z.ZodDefault<z.ZodBoolean>;
    commands: z.ZodDefault<z.ZodBoolean>;
    hooks: z.ZodDefault<z.ZodBoolean>;
    rules: z.ZodDefault<z.ZodBoolean>;
    mcpServers: z.ZodDefault<z.ZodBoolean>;
    templates: z.ZodDefault<z.ZodBoolean>;
    contextFiles: z.ZodDefault<z.ZodBoolean>;
    instructions: z.ZodDefault<z.ZodBoolean>;
    symlinksSupported: z.ZodDefault<z.ZodBoolean>;
}, "strip", z.ZodTypeAny, {
    skills: boolean;
    agents: boolean;
    commands: boolean;
    hooks: boolean;
    rules: boolean;
    mcpServers: boolean;
    templates: boolean;
    contextFiles: boolean;
    instructions: boolean;
    symlinksSupported: boolean;
}, {
    skills?: boolean | undefined;
    agents?: boolean | undefined;
    commands?: boolean | undefined;
    hooks?: boolean | undefined;
    rules?: boolean | undefined;
    mcpServers?: boolean | undefined;
    templates?: boolean | undefined;
    contextFiles?: boolean | undefined;
    instructions?: boolean | undefined;
    symlinksSupported?: boolean | undefined;
}>;
export type SystemCapabilities = z.infer<typeof SystemCapabilities>;
/**
 * System configuration paths
 */
export declare const SystemPaths: z.ZodObject<{
    root: z.ZodString;
    skills: z.ZodOptional<z.ZodString>;
    agents: z.ZodOptional<z.ZodString>;
    commands: z.ZodOptional<z.ZodString>;
    hooks: z.ZodOptional<z.ZodString>;
    rules: z.ZodOptional<z.ZodString>;
    mcpServers: z.ZodOptional<z.ZodString>;
    templates: z.ZodOptional<z.ZodString>;
    context: z.ZodOptional<z.ZodString>;
    instructions: z.ZodOptional<z.ZodString>;
    config: z.ZodOptional<z.ZodString>;
}, "strip", z.ZodTypeAny, {
    root: string;
    context?: string | undefined;
    skills?: string | undefined;
    agents?: string | undefined;
    commands?: string | undefined;
    hooks?: string | undefined;
    rules?: string | undefined;
    mcpServers?: string | undefined;
    templates?: string | undefined;
    instructions?: string | undefined;
    config?: string | undefined;
}, {
    root: string;
    context?: string | undefined;
    skills?: string | undefined;
    agents?: string | undefined;
    commands?: string | undefined;
    hooks?: string | undefined;
    rules?: string | undefined;
    mcpServers?: string | undefined;
    templates?: string | undefined;
    instructions?: string | undefined;
    config?: string | undefined;
}>;
export type SystemPaths = z.infer<typeof SystemPaths>;
/**
 * Base artifact schema
 */
export declare const BaseArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    type: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
}, "strip", z.ZodTypeAny, {
    type: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
}, {
    type: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
}>;
export type BaseArtifact = z.infer<typeof BaseArtifact>;
/**
 * Skill artifact
 */
export declare const SkillArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"skill">;
    triggers: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
    globs: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
    references: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
    assets: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
}, "strip", z.ZodTypeAny, {
    type: "skill";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    triggers?: string[] | undefined;
    globs?: string[] | undefined;
    references?: string[] | undefined;
    assets?: string[] | undefined;
}, {
    type: "skill";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    triggers?: string[] | undefined;
    globs?: string[] | undefined;
    references?: string[] | undefined;
    assets?: string[] | undefined;
}>;
export type SkillArtifact = z.infer<typeof SkillArtifact>;
/**
 * Agent artifact
 */
export declare const AgentArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"agent">;
    role: z.ZodOptional<z.ZodString>;
    tools: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
    temperature: z.ZodOptional<z.ZodNumber>;
}, "strip", z.ZodTypeAny, {
    type: "agent";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    role?: string | undefined;
    tools?: string[] | undefined;
    temperature?: number | undefined;
}, {
    type: "agent";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    role?: string | undefined;
    tools?: string[] | undefined;
    temperature?: number | undefined;
}>;
export type AgentArtifact = z.infer<typeof AgentArtifact>;
/**
 * Command artifact
 */
export declare const CommandArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"command">;
    command: z.ZodString;
    aliases: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
}, "strip", z.ZodTypeAny, {
    type: "command";
    command: string;
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    aliases?: string[] | undefined;
}, {
    type: "command";
    command: string;
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    aliases?: string[] | undefined;
}>;
export type CommandArtifact = z.infer<typeof CommandArtifact>;
/**
 * Hook artifact
 */
export declare const HookArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"hook">;
    hookType: z.ZodEnum<["pre_tool_use", "post_tool_use", "session_start", "session_end", "pre_compact", "user_prompt_submit", "stop"]>;
    toolMatch: z.ZodOptional<z.ZodString>;
    script: z.ZodString;
}, "strip", z.ZodTypeAny, {
    type: "hook";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    hookType: "pre_tool_use" | "post_tool_use" | "session_start" | "session_end" | "pre_compact" | "user_prompt_submit" | "stop";
    script: string;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    toolMatch?: string | undefined;
}, {
    type: "hook";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    hookType: "pre_tool_use" | "post_tool_use" | "session_start" | "session_end" | "pre_compact" | "user_prompt_submit" | "stop";
    script: string;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    toolMatch?: string | undefined;
}>;
export type HookArtifact = z.infer<typeof HookArtifact>;
/**
 * Rule artifact (for Cursor .mdc files)
 */
export declare const RuleArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"rule">;
    globs: z.ZodOptional<z.ZodArray<z.ZodString, "many">>;
    alwaysApply: z.ZodDefault<z.ZodBoolean>;
}, "strip", z.ZodTypeAny, {
    type: "rule";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    alwaysApply: boolean;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    globs?: string[] | undefined;
}, {
    type: "rule";
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    globs?: string[] | undefined;
    alwaysApply?: boolean | undefined;
}>;
export type RuleArtifact = z.infer<typeof RuleArtifact>;
/**
 * MCP Server artifact
 */
export declare const McpServerArtifact: z.ZodObject<{
    id: z.ZodString;
    name: z.ZodString;
    description: z.ZodOptional<z.ZodString>;
    content: z.ZodString;
    metadata: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodUnknown>>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    sourcePath: z.ZodString;
    checksum: z.ZodString;
    lastModified: z.ZodDate;
} & {
    type: z.ZodLiteral<"mcp_server">;
    command: z.ZodArray<z.ZodString, "many">;
    env: z.ZodOptional<z.ZodRecord<z.ZodString, z.ZodString>>;
    enabled: z.ZodDefault<z.ZodBoolean>;
}, "strip", z.ZodTypeAny, {
    type: "mcp_server";
    command: string[];
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    enabled: boolean;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    env?: Record<string, string> | undefined;
}, {
    type: "mcp_server";
    command: string[];
    id: string;
    name: string;
    content: string;
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    checksum: string;
    lastModified: Date;
    description?: string | undefined;
    metadata?: Record<string, unknown> | undefined;
    env?: Record<string, string> | undefined;
    enabled?: boolean | undefined;
}>;
export type McpServerArtifact = z.infer<typeof McpServerArtifact>;
/**
 * Union of all artifact types
 */
export type Artifact = SkillArtifact | AgentArtifact | CommandArtifact | HookArtifact | RuleArtifact | McpServerArtifact | BaseArtifact;
/**
 * Sync operation types
 */
export declare const SyncOperation: z.ZodEnum<["create", "update", "delete", "skip", "conflict", "symlink"]>;
export type SyncOperation = z.infer<typeof SyncOperation>;
/**
 * Sync result for a single artifact
 */
export declare const SyncResult: z.ZodObject<{
    artifactId: z.ZodString;
    artifactName: z.ZodString;
    artifactType: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    targetSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    operation: z.ZodEnum<["create", "update", "delete", "skip", "conflict", "symlink"]>;
    success: z.ZodBoolean;
    message: z.ZodOptional<z.ZodString>;
    error: z.ZodOptional<z.ZodString>;
    sourcePath: z.ZodString;
    targetPath: z.ZodOptional<z.ZodString>;
    timestamp: z.ZodDate;
}, "strip", z.ZodTypeAny, {
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    artifactId: string;
    artifactName: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
    success: boolean;
    timestamp: Date;
    message?: string | undefined;
    error?: string | undefined;
    targetPath?: string | undefined;
}, {
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    artifactId: string;
    artifactName: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
    success: boolean;
    timestamp: Date;
    message?: string | undefined;
    error?: string | undefined;
    targetPath?: string | undefined;
}>;
export type SyncResult = z.infer<typeof SyncResult>;
/**
 * Sync job configuration
 */
export declare const SyncJob: z.ZodObject<{
    id: z.ZodString;
    source: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    targets: z.ZodArray<z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>, "many">;
    artifactTypes: z.ZodArray<z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>, "many">;
    dryRun: z.ZodDefault<z.ZodBoolean>;
    force: z.ZodDefault<z.ZodBoolean>;
    useSymlinks: z.ZodDefault<z.ZodBoolean>;
    startedAt: z.ZodDate;
    completedAt: z.ZodOptional<z.ZodDate>;
    results: z.ZodDefault<z.ZodArray<z.ZodObject<{
        artifactId: z.ZodString;
        artifactName: z.ZodString;
        artifactType: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
        sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
        targetSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
        operation: z.ZodEnum<["create", "update", "delete", "skip", "conflict", "symlink"]>;
        success: z.ZodBoolean;
        message: z.ZodOptional<z.ZodString>;
        error: z.ZodOptional<z.ZodString>;
        sourcePath: z.ZodString;
        targetPath: z.ZodOptional<z.ZodString>;
        timestamp: z.ZodDate;
    }, "strip", z.ZodTypeAny, {
        sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        sourcePath: string;
        artifactId: string;
        artifactName: string;
        artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
        targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
        success: boolean;
        timestamp: Date;
        message?: string | undefined;
        error?: string | undefined;
        targetPath?: string | undefined;
    }, {
        sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        sourcePath: string;
        artifactId: string;
        artifactName: string;
        artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
        targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
        success: boolean;
        timestamp: Date;
        message?: string | undefined;
        error?: string | undefined;
        targetPath?: string | undefined;
    }>, "many">>;
}, "strip", z.ZodTypeAny, {
    id: string;
    source: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    targets: ("claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody")[];
    artifactTypes: ("skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction")[];
    dryRun: boolean;
    force: boolean;
    useSymlinks: boolean;
    startedAt: Date;
    results: {
        sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        sourcePath: string;
        artifactId: string;
        artifactName: string;
        artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
        targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
        success: boolean;
        timestamp: Date;
        message?: string | undefined;
        error?: string | undefined;
        targetPath?: string | undefined;
    }[];
    completedAt?: Date | undefined;
}, {
    id: string;
    source: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    targets: ("claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody")[];
    artifactTypes: ("skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction")[];
    startedAt: Date;
    dryRun?: boolean | undefined;
    force?: boolean | undefined;
    useSymlinks?: boolean | undefined;
    completedAt?: Date | undefined;
    results?: {
        sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        sourcePath: string;
        artifactId: string;
        artifactName: string;
        artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
        targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
        operation: "create" | "update" | "delete" | "skip" | "conflict" | "symlink";
        success: boolean;
        timestamp: Date;
        message?: string | undefined;
        error?: string | undefined;
        targetPath?: string | undefined;
    }[] | undefined;
}>;
export type SyncJob = z.infer<typeof SyncJob>;
/**
 * Transformation rule for mapping between systems
 */
export declare const TransformRule: z.ZodObject<{
    sourceFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
    targetFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
    transform: z.ZodOptional<z.ZodFunction<z.ZodTuple<[z.ZodUnknown], z.ZodUnknown>, z.ZodUnknown>>;
}, "strip", z.ZodTypeAny, {
    sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
}, {
    sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
}>;
export type TransformRule = z.infer<typeof TransformRule>;
/**
 * System mapping configuration
 */
export declare const SystemMapping: z.ZodObject<{
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    targetSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    artifactType: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
    sourcePath: z.ZodString;
    targetPath: z.ZodString;
    transformRule: z.ZodOptional<z.ZodObject<{
        sourceFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
        targetFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
        transform: z.ZodOptional<z.ZodFunction<z.ZodTuple<[z.ZodUnknown], z.ZodUnknown>, z.ZodUnknown>>;
    }, "strip", z.ZodTypeAny, {
        sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
    }, {
        sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
    }>>;
    useSymlink: z.ZodDefault<z.ZodBoolean>;
}, "strip", z.ZodTypeAny, {
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    targetPath: string;
    useSymlink: boolean;
    transformRule?: {
        sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
    } | undefined;
}, {
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    targetPath: string;
    transformRule?: {
        sourceFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        targetFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
        transform?: ((args_0: unknown, ...args: unknown[]) => unknown) | undefined;
    } | undefined;
    useSymlink?: boolean | undefined;
}>;
export type SystemMapping = z.infer<typeof SystemMapping>;
/**
 * CLI configuration
 */
export declare const CliConfig: z.ZodObject<{
    projectRoot: z.ZodString;
    databasePath: z.ZodString;
    defaultSource: z.ZodDefault<z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>>;
    defaultTargets: z.ZodDefault<z.ZodArray<z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>, "many">>;
    useSymlinks: z.ZodDefault<z.ZodBoolean>;
    verbose: z.ZodDefault<z.ZodBoolean>;
}, "strip", z.ZodTypeAny, {
    verbose: boolean;
    useSymlinks: boolean;
    projectRoot: string;
    databasePath: string;
    defaultSource: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    defaultTargets: ("claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody")[];
}, {
    projectRoot: string;
    databasePath: string;
    verbose?: boolean | undefined;
    useSymlinks?: boolean | undefined;
    defaultSource?: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody" | undefined;
    defaultTargets?: ("claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody")[] | undefined;
}>;
export type CliConfig = z.infer<typeof CliConfig>;
/**
 * System definition for the registry
 */
export declare const SystemDefinition: z.ZodObject<{
    id: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    name: z.ZodString;
    description: z.ZodString;
    capabilities: z.ZodObject<{
        skills: z.ZodDefault<z.ZodBoolean>;
        agents: z.ZodDefault<z.ZodBoolean>;
        commands: z.ZodDefault<z.ZodBoolean>;
        hooks: z.ZodDefault<z.ZodBoolean>;
        rules: z.ZodDefault<z.ZodBoolean>;
        mcpServers: z.ZodDefault<z.ZodBoolean>;
        templates: z.ZodDefault<z.ZodBoolean>;
        contextFiles: z.ZodDefault<z.ZodBoolean>;
        instructions: z.ZodDefault<z.ZodBoolean>;
        symlinksSupported: z.ZodDefault<z.ZodBoolean>;
    }, "strip", z.ZodTypeAny, {
        skills: boolean;
        agents: boolean;
        commands: boolean;
        hooks: boolean;
        rules: boolean;
        mcpServers: boolean;
        templates: boolean;
        contextFiles: boolean;
        instructions: boolean;
        symlinksSupported: boolean;
    }, {
        skills?: boolean | undefined;
        agents?: boolean | undefined;
        commands?: boolean | undefined;
        hooks?: boolean | undefined;
        rules?: boolean | undefined;
        mcpServers?: boolean | undefined;
        templates?: boolean | undefined;
        contextFiles?: boolean | undefined;
        instructions?: boolean | undefined;
        symlinksSupported?: boolean | undefined;
    }>;
    paths: z.ZodObject<{
        root: z.ZodString;
        skills: z.ZodOptional<z.ZodString>;
        agents: z.ZodOptional<z.ZodString>;
        commands: z.ZodOptional<z.ZodString>;
        hooks: z.ZodOptional<z.ZodString>;
        rules: z.ZodOptional<z.ZodString>;
        mcpServers: z.ZodOptional<z.ZodString>;
        templates: z.ZodOptional<z.ZodString>;
        context: z.ZodOptional<z.ZodString>;
        instructions: z.ZodOptional<z.ZodString>;
        config: z.ZodOptional<z.ZodString>;
    }, "strip", z.ZodTypeAny, {
        root: string;
        context?: string | undefined;
        skills?: string | undefined;
        agents?: string | undefined;
        commands?: string | undefined;
        hooks?: string | undefined;
        rules?: string | undefined;
        mcpServers?: string | undefined;
        templates?: string | undefined;
        instructions?: string | undefined;
        config?: string | undefined;
    }, {
        root: string;
        context?: string | undefined;
        skills?: string | undefined;
        agents?: string | undefined;
        commands?: string | undefined;
        hooks?: string | undefined;
        rules?: string | undefined;
        mcpServers?: string | undefined;
        templates?: string | undefined;
        instructions?: string | undefined;
        config?: string | undefined;
    }>;
    configFormat: z.ZodEnum<["json", "yaml", "toml", "markdown", "mdc"]>;
    filePatterns: z.ZodRecord<z.ZodString, z.ZodString>;
}, "strip", z.ZodTypeAny, {
    id: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    name: string;
    description: string;
    capabilities: {
        skills: boolean;
        agents: boolean;
        commands: boolean;
        hooks: boolean;
        rules: boolean;
        mcpServers: boolean;
        templates: boolean;
        contextFiles: boolean;
        instructions: boolean;
        symlinksSupported: boolean;
    };
    paths: {
        root: string;
        context?: string | undefined;
        skills?: string | undefined;
        agents?: string | undefined;
        commands?: string | undefined;
        hooks?: string | undefined;
        rules?: string | undefined;
        mcpServers?: string | undefined;
        templates?: string | undefined;
        instructions?: string | undefined;
        config?: string | undefined;
    };
    configFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    filePatterns: Record<string, string>;
}, {
    id: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    name: string;
    description: string;
    capabilities: {
        skills?: boolean | undefined;
        agents?: boolean | undefined;
        commands?: boolean | undefined;
        hooks?: boolean | undefined;
        rules?: boolean | undefined;
        mcpServers?: boolean | undefined;
        templates?: boolean | undefined;
        contextFiles?: boolean | undefined;
        instructions?: boolean | undefined;
        symlinksSupported?: boolean | undefined;
    };
    paths: {
        root: string;
        context?: string | undefined;
        skills?: string | undefined;
        agents?: string | undefined;
        commands?: string | undefined;
        hooks?: string | undefined;
        rules?: string | undefined;
        mcpServers?: string | undefined;
        templates?: string | undefined;
        instructions?: string | undefined;
        config?: string | undefined;
    };
    configFormat: "json" | "mdc" | "toml" | "markdown" | "yaml";
    filePatterns: Record<string, string>;
}>;
export type SystemDefinition = z.infer<typeof SystemDefinition>;
/**
 * Difference between source and target
 */
export declare const ArtifactDiff: z.ZodObject<{
    artifactId: z.ZodString;
    artifactName: z.ZodString;
    artifactType: z.ZodEnum<["skill", "agent", "command", "hook", "rule", "mcp_server", "template", "context", "instruction"]>;
    sourceSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    targetSystem: z.ZodEnum<["claude", "opencode", "cursor", "codex", "windsurf", "aider", "gemini", "continue", "cody"]>;
    status: z.ZodEnum<["added", "modified", "deleted", "unchanged", "missing"]>;
    sourceChecksum: z.ZodOptional<z.ZodString>;
    targetChecksum: z.ZodOptional<z.ZodString>;
    sourcePath: z.ZodOptional<z.ZodString>;
    targetPath: z.ZodOptional<z.ZodString>;
}, "strip", z.ZodTypeAny, {
    status: "added" | "modified" | "deleted" | "unchanged" | "missing";
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    artifactId: string;
    artifactName: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath?: string | undefined;
    targetPath?: string | undefined;
    sourceChecksum?: string | undefined;
    targetChecksum?: string | undefined;
}, {
    status: "added" | "modified" | "deleted" | "unchanged" | "missing";
    sourceSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    artifactId: string;
    artifactName: string;
    artifactType: "skill" | "agent" | "rule" | "command" | "hook" | "mcp_server" | "template" | "context" | "instruction";
    targetSystem: "claude" | "opencode" | "cursor" | "codex" | "windsurf" | "aider" | "gemini" | "continue" | "cody";
    sourcePath?: string | undefined;
    targetPath?: string | undefined;
    sourceChecksum?: string | undefined;
    targetChecksum?: string | undefined;
}>;
export type ArtifactDiff = z.infer<typeof ArtifactDiff>;
/**
 * Generate artifact ID from name and type
 */
export declare function generateArtifactId(name: string, type: ArtifactType, system: SystemId): string;
/**
 * Parse artifact ID
 */
export declare function parseArtifactId(id: string): {
    system: SystemId;
    type: ArtifactType;
    name: string;
} | null;
//# sourceMappingURL=types.d.ts.map