/**
 * Aider Adapter
 *
 * Adapter for Aider AI pair programming tool.
 * Aider primarily uses .aider.conf.yml for configuration and supports
 * instructions via the config file.
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  writeFileSync,
  statSync,
  unlinkSync,
} from "node:fs";
import { join, dirname } from "node:path";
import type {
  SystemCapabilities,
  SystemPaths,
  Artifact,
  InstructionArtifact,
} from "../models/types.js";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions } from "./base.js";

export class AiderAdapter extends BaseAdapter {
  readonly systemId = "aider" as const;
  readonly name = "Aider";

  readonly capabilities: SystemCapabilities = {
    skills: false,
    agents: false,
    commands: false,
    hooks: false,
    rules: false,
    mcpServers: false,
    templates: false,
    contextFiles: true,
    instructions: true,
    symlinksSupported: true,
  };

  readonly paths: SystemPaths = {
    root: ".",
    instructions: ".aider.conf.yml",
    context: ".aider",
    config: ".aider.conf.yml",
  };

  isConfigured(projectRoot: string): boolean {
    return (
      existsSync(join(projectRoot, ".aider.conf.yml")) ||
      existsSync(join(projectRoot, ".aider"))
    );
  }

  async initialize(projectRoot: string): Promise<void> {
    const aiderDir = join(projectRoot, ".aider");
    if (!existsSync(aiderDir)) {
      mkdirSync(aiderDir, { recursive: true });
    }

    // Create default .aider.conf.yml if it doesn't exist
    const configPath = join(projectRoot, ".aider.conf.yml");
    if (!existsSync(configPath)) {
      const defaultConfig = `# Aider Configuration
# See: https://aider.chat/docs/config.html

# Model settings
# model: gpt-4

# Auto-commit settings
auto-commits: true

# Read files (context)
# Note: SKILL_INDEX.md provides a compact reference to all available skills
read:
  - CLAUDE.md
  - README.md
  - .aider/SKILL_INDEX.md
`;
      writeFileSync(configPath, defaultConfig);
    } else {
      // Ensure SKILL_INDEX.md is in read files
      let config = readFileSync(configPath, "utf-8");
      if (!config.includes("SKILL_INDEX.md")) {
        if (config.includes("read:")) {
          config = config.replace(
            /read:\n/,
            "read:\n  - .aider/SKILL_INDEX.md\n"
          );
        } else {
          config += "\n# Read files (context)\nread:\n  - .aider/SKILL_INDEX.md\n";
        }
        writeFileSync(configPath, config);
      }
    }
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["instruction", "context"];

    if (types.includes("instruction")) {
      const instruction = await this.scanInstructions(projectRoot);
      if (instruction) {
        artifacts.push(instruction);
      }
    }

    return artifacts;
  }

  private async scanInstructions(projectRoot: string): Promise<InstructionArtifact | null> {
    const configPath = join(projectRoot, this.paths.instructions!);
    if (!existsSync(configPath)) return null;

    const content = readFileSync(configPath, "utf-8");
    const stats = statSync(configPath);

    return {
      id: generateArtifactId("aider-config", "instruction", "aider"),
      name: "aider-config",
      type: "instruction",
      description: "Aider configuration file",
      content,
      metadata: {},
      sourceSystem: "aider",
      sourcePath: this.paths.instructions!,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      format: "yaml",
    };
  }

  async readArtifact(
    projectRoot: string,
    artifactPath: string
  ): Promise<Artifact | null> {
    const fullPath = join(projectRoot, artifactPath);
    if (!existsSync(fullPath)) return null;

    const content = readFileSync(fullPath, "utf-8");
    const stats = statSync(fullPath);

    return {
      id: generateArtifactId("aider-config", "instruction", "aider"),
      name: "aider-config",
      type: "instruction",
      description: "Aider configuration",
      content,
      metadata: {},
      sourceSystem: "aider",
      sourcePath: artifactPath,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      format: "yaml",
    } as InstructionArtifact;
  }

  async writeArtifact(
    projectRoot: string,
    artifact: Artifact,
    options: WriteOptions = {}
  ): Promise<string> {
    const targetPath = this.getArtifactPath(artifact);
    const fullPath = join(projectRoot, targetPath);
    const targetDir = dirname(fullPath);

    if (options.createDirectories !== false && !existsSync(targetDir)) {
      mkdirSync(targetDir, { recursive: true });
    }

    if (!options.overwrite && existsSync(fullPath)) {
      throw new Error(`File already exists: ${fullPath}`);
    }

    writeFileSync(fullPath, artifact.content);
    return targetPath;
  }

  async deleteArtifact(projectRoot: string, artifactPath: string): Promise<void> {
    const fullPath = join(projectRoot, artifactPath);
    if (existsSync(fullPath)) {
      unlinkSync(fullPath);
    }
  }

  getArtifactPath(artifact: Artifact): string {
    if (artifact.type === "instruction") {
      return this.paths.instructions!;
    }
    return join(".aider", `${artifact.name}.md`);
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (artifact.type !== "instruction" && artifact.type !== "context") {
      errors.push(`Aider only supports instruction and context artifacts, got: ${artifact.type}`);
    }

    return { valid: errors.length === 0, errors };
  }
}
