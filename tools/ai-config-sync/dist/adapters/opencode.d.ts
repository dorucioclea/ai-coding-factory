/**
 * OpenCode Adapter
 *
 * Adapter for OpenCode AI CLI configuration.
 * Supports skills (symlinked from Claude), agents, and commands.
 */
import type { SystemCapabilities, SystemPaths, Artifact, McpServerArtifact } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions } from "./base.js";
export declare class OpenCodeAdapter extends BaseAdapter {
    readonly systemId: "opencode";
    readonly name = "OpenCode";
    readonly capabilities: SystemCapabilities;
    readonly paths: SystemPaths;
    isConfigured(projectRoot: string): boolean;
    initialize(projectRoot: string): Promise<void>;
    scanArtifacts(projectRoot: string, options?: ScanOptions): Promise<Artifact[]>;
    private scanSkills;
    private scanAgents;
    private scanCommands;
    readArtifact(projectRoot: string, artifactPath: string): Promise<Artifact | null>;
    writeArtifact(projectRoot: string, artifact: Artifact, options?: WriteOptions): Promise<string>;
    deleteArtifact(projectRoot: string, artifactPath: string): Promise<void>;
    getArtifactPath(artifact: Artifact): string;
    readMcpServers(projectRoot: string): Promise<McpServerArtifact[]>;
    writeMcpServers(projectRoot: string, servers: McpServerArtifact[]): Promise<void>;
}
//# sourceMappingURL=opencode.d.ts.map