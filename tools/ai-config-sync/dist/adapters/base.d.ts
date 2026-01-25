/**
 * Base Adapter Interface for AI Config Sync
 *
 * Defines the contract that all system adapters must implement.
 */
import type { SystemId, ArtifactType, Artifact, SystemCapabilities, SystemPaths, McpServerArtifact } from "../models/types.js";
/**
 * Scan options for discovering artifacts
 */
export interface ScanOptions {
    types?: ArtifactType[];
    includeHidden?: boolean;
    recursive?: boolean;
}
/**
 * Write options for saving artifacts
 */
export interface WriteOptions {
    overwrite?: boolean;
    createDirectories?: boolean;
    useSymlink?: boolean;
    symlinkTarget?: string;
}
/**
 * Parse result for skill files
 */
export interface ParsedSkill {
    name: string;
    description?: string;
    triggers?: string[];
    globs?: string[];
    content: string;
    references?: string[];
    assets?: string[];
    metadata: Record<string, unknown>;
}
/**
 * Transform options for converting between formats
 */
export interface TransformOptions {
    sourceFormat: string;
    targetFormat: string;
    preserveMetadata?: boolean;
}
/**
 * Base adapter interface that all system adapters must implement
 */
export interface SystemAdapter {
    /**
     * System identifier
     */
    readonly systemId: SystemId;
    /**
     * Human-readable system name
     */
    readonly name: string;
    /**
     * System capabilities
     */
    readonly capabilities: SystemCapabilities;
    /**
     * System paths configuration
     */
    readonly paths: SystemPaths;
    /**
     * Check if the system is configured in the project
     */
    isConfigured(projectRoot: string): boolean;
    /**
     * Initialize the system configuration in a project
     */
    initialize(projectRoot: string): Promise<void>;
    /**
     * Scan for artifacts of specified types
     */
    scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    /**
     * Read a specific artifact by path
     */
    readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    /**
     * Write an artifact to the system
     */
    writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    /**
     * Delete an artifact
     */
    deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    /**
     * Transform an artifact for this system
     */
    transformArtifact(artifact: Artifact, options?: TransformOptions): Artifact;
    /**
     * Get the target path for an artifact in this system
     */
    getArtifactPath(artifact: Artifact): string;
    /**
     * Validate an artifact against system requirements
     */
    validateArtifact(artifact: Artifact): {
        valid: boolean;
        errors: string[];
    };
    /**
     * Read MCP server configuration
     */
    readMcpServers?(projectRoot: string): Promise<McpServerArtifact[]>;
    /**
     * Write MCP server configuration
     */
    writeMcpServers?(projectRoot: string, servers: McpServerArtifact[]): Promise<void>;
}
/**
 * Abstract base class with common functionality
 */
export declare abstract class BaseAdapter implements SystemAdapter {
    abstract readonly systemId: SystemId;
    abstract readonly name: string;
    abstract readonly capabilities: SystemCapabilities;
    abstract readonly paths: SystemPaths;
    abstract isConfigured(projectRoot: string): boolean;
    abstract initialize(projectRoot: string): Promise<void>;
    abstract scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    abstract readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    abstract writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    abstract deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    abstract getArtifactPath(artifact: Artifact): string;
    /**
     * Default transformation (identity)
     */
    transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact;
    /**
     * Default validation
     */
    validateArtifact(artifact: Artifact): {
        valid: boolean;
        errors: string[];
    };
    /**
     * Generate checksum for content
     */
    protected generateChecksum(content: string): string;
    /**
     * Parse YAML frontmatter from markdown content
     */
    protected parseYamlFrontmatter(content: string): {
        frontmatter: Record<string, unknown>;
        body: string;
    };
    /**
     * Generate YAML frontmatter string
     */
    protected generateYamlFrontmatter(data: Record<string, unknown>): string;
}
//# sourceMappingURL=base.d.ts.map