/**
 * Claude Code Adapter
 *
 * Adapter for Anthropic's Claude Code CLI configuration.
 * This is the primary/source system for most configurations.
 */
import type { SystemCapabilities, SystemPaths, Artifact, McpServerArtifact } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions } from "./base.js";
export declare class ClaudeAdapter extends BaseAdapter {
    readonly systemId: "claude";
    readonly name = "Claude Code";
    readonly capabilities: SystemCapabilities;
    readonly paths: SystemPaths;
    isConfigured(projectRoot: string): boolean;
    initialize(projectRoot: string): Promise<void>;
    scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    private scanSkills;
    private scanAgents;
    private scanCommands;
    private scanHooks;
    private scanRules;
    readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    getArtifactPath(artifact: Artifact): string;
    readMcpServers(projectRoot: string): Promise<McpServerArtifact[]>;
    writeMcpServers(projectRoot: string, servers: McpServerArtifact[]): Promise<void>;
}
//# sourceMappingURL=claude.d.ts.map