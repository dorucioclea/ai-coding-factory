/**
 * Cursor IDE Adapter
 *
 * Adapter for Cursor IDE configuration.
 * Supports rules (.mdc files with YAML frontmatter).
 */
import type { SystemCapabilities, SystemPaths, Artifact, McpServerArtifact } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";
export declare class CursorAdapter extends BaseAdapter {
    readonly systemId: "cursor";
    readonly name = "Cursor IDE";
    readonly capabilities: SystemCapabilities;
    readonly paths: SystemPaths;
    isConfigured(projectRoot: string): boolean;
    initialize(projectRoot: string): Promise<void>;
    scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    private scanRules;
    readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    getArtifactPath(artifact: Artifact): string;
    /**
     * Transform a skill artifact to Cursor .mdc rule format
     */
    transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact;
    /**
     * Convert a skill to a Cursor .mdc rule
     */
    private skillToRule;
    /**
     * Convert a rule from another system to Cursor .mdc format
     */
    private ruleToMdc;
    validateArtifact(artifact: Artifact): {
        valid: boolean;
        errors: string[];
    };
    readMcpServers(projectRoot: string): Promise<McpServerArtifact[]>;
    writeMcpServers(projectRoot: string, servers: McpServerArtifact[]): Promise<void>;
}
//# sourceMappingURL=cursor.d.ts.map