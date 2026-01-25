/**
 * Windsurf IDE Adapter
 *
 * Adapter for Codeium's Windsurf AI IDE configuration.
 * Supports rules (markdown files) and MCP servers.
 */
import type { SystemCapabilities, SystemPaths, Artifact, McpServerArtifact } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";
export declare class WindsurfAdapter extends BaseAdapter {
    readonly systemId: "windsurf";
    readonly name = "Windsurf IDE";
    readonly capabilities: SystemCapabilities;
    readonly paths: SystemPaths;
    isConfigured(projectRoot: string): boolean;
    initialize(projectRoot: string): Promise<void>;
    scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    private scanRules;
    /**
     * Extract description from first heading or paragraph
     */
    private extractDescription;
    readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    getArtifactPath(artifact: Artifact): string;
    /**
     * Transform a skill artifact to Windsurf markdown rule
     */
    transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact;
    /**
     * Convert a skill to a Windsurf markdown rule
     */
    private skillToRule;
    /**
     * Normalize a rule from another system
     */
    private normalizeRule;
    validateArtifact(artifact: Artifact): {
        valid: boolean;
        errors: string[];
    };
    readMcpServers(projectRoot: string): Promise<McpServerArtifact[]>;
    writeMcpServers(projectRoot: string, servers: McpServerArtifact[]): Promise<void>;
}
//# sourceMappingURL=windsurf.d.ts.map