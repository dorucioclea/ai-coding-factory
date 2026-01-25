/**
 * Base Adapter Interface for AI Config Sync
 *
 * Defines the contract that all system adapters must implement.
 */

import type {
  SystemId,
  ArtifactType,
  Artifact,
  SystemCapabilities,
  SystemPaths,
  McpServerArtifact,
} from "../models/types.js";

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
  writeArtifact(
    projectRoot: string,
    artifact: Artifact,
    options?: WriteOptions
  ): Promise<string>;

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
  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] };

  /**
   * Read MCP server configuration
   */
  readMcpServers?(projectRoot: string): Promise<McpServerArtifact[]>;

  /**
   * Write MCP server configuration
   */
  writeMcpServers?(
    projectRoot: string,
    servers: McpServerArtifact[]
  ): Promise<void>;
}

/**
 * Abstract base class with common functionality
 */
export abstract class BaseAdapter implements SystemAdapter {
  abstract readonly systemId: SystemId;
  abstract readonly name: string;
  abstract readonly capabilities: SystemCapabilities;
  abstract readonly paths: SystemPaths;

  abstract isConfigured(projectRoot: string): boolean;
  abstract initialize(projectRoot: string): Promise<void>;
  abstract scanArtifacts(
    projectRoot: string,
    options?: ScanOptions
  ): Promise<Artifact[]>;
  abstract readArtifact(
    projectRoot: string,
    artifactPath: string
  ): Promise<Artifact | null>;
  abstract writeArtifact(
    projectRoot: string,
    artifact: Artifact,
    options?: WriteOptions
  ): Promise<string>;
  abstract deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
  abstract getArtifactPath(artifact: Artifact): string;

  /**
   * Default transformation (identity)
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    return artifact;
  }

  /**
   * Default validation
   */
  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!artifact.id) {
      errors.push("Artifact ID is required");
    }
    if (!artifact.name) {
      errors.push("Artifact name is required");
    }
    if (!artifact.content) {
      errors.push("Artifact content is required");
    }

    return { valid: errors.length === 0, errors };
  }

  /**
   * Generate checksum for content
   */
  protected generateChecksum(content: string): string {
    let hash = 0;
    for (let i = 0; i < content.length; i++) {
      const char = content.charCodeAt(i);
      hash = (hash << 5) - hash + char;
      hash = hash & hash;
    }
    return Math.abs(hash).toString(16).padStart(8, "0");
  }

  /**
   * Parse YAML frontmatter from markdown content
   */
  protected parseYamlFrontmatter(
    content: string
  ): { frontmatter: Record<string, unknown>; body: string } {
    const frontmatterRegex = /^---\n([\s\S]*?)\n---\n([\s\S]*)$/;
    const match = content.match(frontmatterRegex);

    if (!match) {
      return { frontmatter: {}, body: content };
    }

    try {
      // Simple YAML parsing for common cases
      const frontmatter: Record<string, unknown> = {};
      const lines = match[1].split("\n");

      for (const line of lines) {
        const colonIndex = line.indexOf(":");
        if (colonIndex > 0) {
          const key = line.slice(0, colonIndex).trim();
          let value = line.slice(colonIndex + 1).trim();

          // Remove quotes
          if (
            (value.startsWith('"') && value.endsWith('"')) ||
            (value.startsWith("'") && value.endsWith("'"))
          ) {
            value = value.slice(1, -1);
          }

          // Parse arrays
          if (value.startsWith("[") && value.endsWith("]")) {
            frontmatter[key] = value
              .slice(1, -1)
              .split(",")
              .map((s) => s.trim().replace(/['"]/g, ""));
          } else if (value === "true") {
            frontmatter[key] = true;
          } else if (value === "false") {
            frontmatter[key] = false;
          } else if (!isNaN(Number(value)) && value !== "") {
            frontmatter[key] = Number(value);
          } else {
            frontmatter[key] = value;
          }
        }
      }

      return { frontmatter, body: match[2] };
    } catch {
      return { frontmatter: {}, body: content };
    }
  }

  /**
   * Generate YAML frontmatter string
   */
  protected generateYamlFrontmatter(data: Record<string, unknown>): string {
    const lines: string[] = ["---"];

    for (const [key, value] of Object.entries(data)) {
      if (value === undefined || value === null) continue;

      if (Array.isArray(value)) {
        lines.push(`${key}: [${value.map((v) => `"${v}"`).join(", ")}]`);
      } else if (typeof value === "string") {
        lines.push(`${key}: "${value}"`);
      } else if (typeof value === "boolean" || typeof value === "number") {
        lines.push(`${key}: ${value}`);
      }
    }

    lines.push("---");
    return lines.join("\n");
  }
}
