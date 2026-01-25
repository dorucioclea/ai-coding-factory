/**
 * Sourcegraph Cody Adapter
 *
 * Adapter for Sourcegraph Cody AI assistant.
 * Cody uses .cody/ directory with cody.json config and supports
 * custom commands and context files.
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  writeFileSync,
  statSync,
  unlinkSync,
  rmSync,
} from "node:fs";
import { join, basename, dirname } from "node:path";
import { glob } from "glob";
import type {
  SystemCapabilities,
  SystemPaths,
  Artifact,
  CommandArtifact,
  ContextArtifact,
  InstructionArtifact,
  SkillArtifact,
} from "../models/types.js";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";

export class CodyAdapter extends BaseAdapter {
  readonly systemId = "cody" as const;
  readonly name = "Sourcegraph Cody";

  readonly capabilities: SystemCapabilities = {
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
  };

  readonly paths: SystemPaths = {
    root: ".cody",
    commands: ".cody/commands",
    context: ".cody/context",
    instructions: ".cody/instructions.md",
    config: ".cody/cody.json",
  };

  isConfigured(projectRoot: string): boolean {
    return existsSync(join(projectRoot, ".cody"));
  }

  async initialize(projectRoot: string): Promise<void> {
    const dirs = [this.paths.root, this.paths.commands, this.paths.context];

    for (const dir of dirs) {
      if (dir) {
        const fullPath = join(projectRoot, dir);
        if (!existsSync(fullPath)) {
          mkdirSync(fullPath, { recursive: true });
        }
      }
    }

    // Create default cody.json if it doesn't exist
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) {
      const defaultConfig = {
        "$schema": "https://json.schemastore.org/cody.json",
        commands: {},
      };
      writeFileSync(configPath, JSON.stringify(defaultConfig, null, 2));
    }

    // Create default instructions.md if it doesn't exist
    const instructionsPath = join(projectRoot, this.paths.instructions!);
    if (!existsSync(instructionsPath)) {
      const defaultInstructions = `# Cody Instructions

This file provides instructions for Sourcegraph Cody when working in this repository.

## Project Overview

[Add your project description here]

## Coding Standards

[Add your coding standards here]

## Available Skills

See [.sourcegraph/SKILL_INDEX.md](./.sourcegraph/SKILL_INDEX.md) for a compact reference of all available skills.
To use a skill, reference it by name (e.g., "use the debugging skill").
`;
      writeFileSync(instructionsPath, defaultInstructions);
    } else {
      // Ensure skill index reference is in instructions
      let content = readFileSync(instructionsPath, "utf-8");
      if (!content.includes("SKILL_INDEX.md")) {
        content += `\n\n## Available Skills\n\nSee [.sourcegraph/SKILL_INDEX.md](./.sourcegraph/SKILL_INDEX.md) for a compact reference of all available skills.\nTo use a skill, reference it by name (e.g., "use the debugging skill").\n`;
        writeFileSync(instructionsPath, content);
      }
    }

    // Create .sourcegraph directory for skill index
    const sourcegraphDir = join(projectRoot, ".sourcegraph");
    if (!existsSync(sourcegraphDir)) {
      mkdirSync(sourcegraphDir, { recursive: true });
    }
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["command", "context", "instruction"];

    if (types.includes("command")) {
      artifacts.push(...(await this.scanCommands(projectRoot)));
    }

    if (types.includes("context")) {
      artifacts.push(...(await this.scanContext(projectRoot)));
    }

    if (types.includes("instruction")) {
      const instruction = await this.scanInstructions(projectRoot);
      if (instruction) {
        artifacts.push(instruction);
      }
    }

    return artifacts;
  }

  private async scanCommands(projectRoot: string): Promise<CommandArtifact[]> {
    // Cody commands are defined in cody.json
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) return [];

    try {
      const content = readFileSync(configPath, "utf-8");
      const config = JSON.parse(content);
      const commands: CommandArtifact[] = [];
      const stats = statSync(configPath);

      for (const [name, cmdConfig] of Object.entries(config.commands || {})) {
        const cmd = cmdConfig as { prompt?: string; description?: string; context?: unknown };
        commands.push({
          id: generateArtifactId(name, "command", "cody"),
          name,
          type: "command",
          description: cmd.description ?? `Cody command: ${name}`,
          content: JSON.stringify(cmd, null, 2),
          metadata: { context: cmd.context, prompt: cmd.prompt },
          sourceSystem: "cody",
          sourcePath: this.paths.config!,
          checksum: this.generateChecksum(JSON.stringify(cmd)),
          lastModified: stats.mtime,
          command: `/${name}`,
        });
      }

      return commands;
    } catch {
      return [];
    }
  }

  private async scanContext(projectRoot: string): Promise<ContextArtifact[]> {
    const contextDir = join(projectRoot, this.paths.context!);
    if (!existsSync(contextDir)) return [];

    const contextFiles = await glob("**/*.md", { cwd: contextDir });
    const contexts: ContextArtifact[] = [];

    for (const contextFile of contextFiles) {
      const contextPath = join(contextDir, contextFile);
      const contextName = basename(contextFile, ".md");
      const content = readFileSync(contextPath, "utf-8");
      const stats = statSync(contextPath);

      contexts.push({
        id: generateArtifactId(contextName, "context", "cody"),
        name: contextName,
        type: "context",
        description: `Context file: ${contextName}`,
        content,
        metadata: {},
        sourceSystem: "cody",
        sourcePath: join(this.paths.context!, contextFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        contextType: "file",
      });
    }

    return contexts;
  }

  private async scanInstructions(projectRoot: string): Promise<InstructionArtifact | null> {
    const instructionsPath = join(projectRoot, this.paths.instructions!);
    if (!existsSync(instructionsPath)) return null;

    const content = readFileSync(instructionsPath, "utf-8");
    const stats = statSync(instructionsPath);

    return {
      id: generateArtifactId("cody-instructions", "instruction", "cody"),
      name: "cody-instructions",
      type: "instruction",
      description: "Cody project instructions",
      content,
      metadata: {},
      sourceSystem: "cody",
      sourcePath: this.paths.instructions!,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      format: "markdown",
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
    const name = basename(artifactPath, ".md").replace(".json", "");

    // Determine type based on path
    let type: "command" | "context" | "instruction" = "context";
    if (artifactPath.includes("/commands/") || artifactPath.endsWith("cody.json")) {
      type = "command";
    } else if (artifactPath.includes("instructions")) {
      type = "instruction";
    }

    return {
      id: generateArtifactId(name, type, "cody"),
      name,
      type,
      content,
      metadata: {},
      sourceSystem: "cody",
      sourcePath: artifactPath,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
    } as Artifact;
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

    if (artifact.type === "command") {
      // Commands go into cody.json
      return this.writeCommand(projectRoot, artifact as CommandArtifact, options);
    }

    if (!options.overwrite && existsSync(fullPath)) {
      throw new Error(`File already exists: ${fullPath}`);
    }

    writeFileSync(fullPath, artifact.content);
    return targetPath;
  }

  private writeCommand(
    projectRoot: string,
    command: CommandArtifact,
    options: WriteOptions
  ): string {
    const configPath = join(projectRoot, this.paths.config!);

    let config: { commands: Record<string, unknown> } = { commands: {} };
    if (existsSync(configPath)) {
      try {
        config = JSON.parse(readFileSync(configPath, "utf-8"));
        if (!config.commands) {
          config.commands = {};
        }
      } catch {
        config = { commands: {} };
      }
    }

    if (!options.overwrite && config.commands[command.name]) {
      throw new Error(`Command already exists: ${command.name}`);
    }

    // Add or update the command
    config.commands[command.name] = {
      prompt: command.metadata?.prompt ?? command.content,
      description: command.description,
      context: command.metadata?.context,
    };

    writeFileSync(configPath, JSON.stringify(config, null, 2));
    return this.paths.config!;
  }

  async deleteArtifact(projectRoot: string, artifactPath: string): Promise<void> {
    const fullPath = join(projectRoot, artifactPath);
    if (existsSync(fullPath)) {
      const stats = statSync(fullPath);
      if (stats.isDirectory()) {
        rmSync(fullPath, { recursive: true, force: true });
      } else {
        unlinkSync(fullPath);
      }
    }
  }

  getArtifactPath(artifact: Artifact): string {
    switch (artifact.type) {
      case "command":
        return this.paths.config!;
      case "context":
        return join(this.paths.context!, `${artifact.name}.md`);
      case "instruction":
        return this.paths.instructions!;
      default:
        return join(this.paths.root, `${artifact.name}.md`);
    }
  }

  /**
   * Transform a skill to a Cody command or context
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    if (artifact.type === "skill") {
      // Transform skills to context files for Cody
      return this.skillToContext(artifact as SkillArtifact);
    }
    if (artifact.type === "command" && artifact.sourceSystem !== "cody") {
      return this.transformCommand(artifact as CommandArtifact);
    }
    return artifact;
  }

  private skillToContext(skill: SkillArtifact): ContextArtifact {
    const { body } = this.parseYamlFrontmatter(skill.content);

    return {
      id: generateArtifactId(skill.name, "context", "cody"),
      name: skill.name,
      type: "context",
      description: skill.description ?? `Context from skill: ${skill.name}`,
      content: body,
      metadata: { ...skill.metadata, sourceSkill: skill.name },
      sourceSystem: "cody",
      sourcePath: join(this.paths.context!, `${skill.name}.md`),
      checksum: this.generateChecksum(body),
      lastModified: new Date(),
      contextType: "file",
    };
  }

  private transformCommand(command: CommandArtifact): CommandArtifact {
    return {
      ...command,
      id: generateArtifactId(command.name, "command", "cody"),
      sourceSystem: "cody",
      sourcePath: this.paths.config!,
      checksum: this.generateChecksum(command.content),
      lastModified: new Date(),
    };
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (!["command", "context", "instruction"].includes(artifact.type)) {
      errors.push(`Cody only supports command, context, and instruction artifacts, got: ${artifact.type}`);
    }

    return { valid: errors.length === 0, errors };
  }
}
