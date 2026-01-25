/**
 * Google Gemini Adapter
 *
 * Adapter for Google Gemini Code Assist.
 * Gemini uses GEMINI.md for project instructions similar to CLAUDE.md.
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
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";

export class GeminiAdapter extends BaseAdapter {
  readonly systemId = "gemini" as const;
  readonly name = "Google Gemini";

  readonly capabilities: SystemCapabilities = {
    skills: false,
    agents: false,
    commands: false,
    hooks: false,
    rules: false,
    mcpServers: false,
    templates: false,
    contextFiles: false,
    instructions: true,
    symlinksSupported: true,
  };

  readonly paths: SystemPaths = {
    root: ".",
    instructions: "GEMINI.md",
  };

  isConfigured(projectRoot: string): boolean {
    return existsSync(join(projectRoot, "GEMINI.md"));
  }

  async initialize(projectRoot: string): Promise<void> {
    const geminiPath = join(projectRoot, "GEMINI.md");
    if (!existsSync(geminiPath)) {
      // Create a basic GEMINI.md by transforming CLAUDE.md if it exists
      const claudePath = join(projectRoot, "CLAUDE.md");
      if (existsSync(claudePath)) {
        const claudeContent = readFileSync(claudePath, "utf-8");
        const geminiContent = this.transformFromClaude(claudeContent);
        writeFileSync(geminiPath, geminiContent);
      } else {
        const defaultContent = `# GEMINI.md - Project Instructions for Google Gemini

This file provides instructions for Google Gemini Code Assist when working in this repository.

## Project Overview

[Add your project description here]

## Coding Standards

[Add your coding standards here]

## Important Files

[List important files and their purposes]
`;
        writeFileSync(geminiPath, defaultContent);
      }
    }
  }

  /**
   * Transform CLAUDE.md content to GEMINI.md format
   */
  private transformFromClaude(claudeContent: string): string {
    // Replace Claude-specific references with Gemini
    let content = claudeContent
      .replace(/CLAUDE\.md/g, "GEMINI.md")
      .replace(/Claude Code/g, "Google Gemini")
      .replace(/Claude/g, "Gemini")
      .replace(/\.claude\//g, "");

    // Add Gemini-specific header
    content = content.replace(
      /^# .*/m,
      "# GEMINI.md - Project Instructions for Google Gemini"
    );

    // Add skill index reference if not present
    if (!content.includes("SKILL_INDEX.md")) {
      content += `\n\n## Available Skills\n\nSee [SKILL_INDEX.md](./SKILL_INDEX.md) for a compact reference of all available skills.\nTo use a skill, reference it by name (e.g., "use the debugging skill").\n`;
    }

    return content;
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["instruction"];

    if (types.includes("instruction")) {
      const instruction = await this.scanInstructions(projectRoot);
      if (instruction) {
        artifacts.push(instruction);
      }
    }

    return artifacts;
  }

  private async scanInstructions(projectRoot: string): Promise<InstructionArtifact | null> {
    const instructionPath = join(projectRoot, this.paths.instructions!);
    if (!existsSync(instructionPath)) return null;

    const content = readFileSync(instructionPath, "utf-8");
    const stats = statSync(instructionPath);

    return {
      id: generateArtifactId("gemini-instructions", "instruction", "gemini"),
      name: "gemini-instructions",
      type: "instruction",
      description: "Gemini project instructions",
      content,
      metadata: {},
      sourceSystem: "gemini",
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

    return {
      id: generateArtifactId("gemini-instructions", "instruction", "gemini"),
      name: "gemini-instructions",
      type: "instruction",
      description: "Gemini project instructions",
      content,
      metadata: {},
      sourceSystem: "gemini",
      sourcePath: artifactPath,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      format: "markdown",
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
    return `${artifact.name}.md`;
  }

  /**
   * Transform instructions from another system to Gemini format
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    if (artifact.type === "instruction" && artifact.sourceSystem !== "gemini") {
      const content = this.transformFromClaude(artifact.content);
      return {
        ...artifact,
        id: generateArtifactId("gemini-instructions", "instruction", "gemini"),
        content,
        sourceSystem: "gemini",
        sourcePath: this.paths.instructions!,
        checksum: this.generateChecksum(content),
        lastModified: new Date(),
      };
    }
    return artifact;
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (artifact.type !== "instruction") {
      errors.push(`Gemini only supports instruction artifacts, got: ${artifact.type}`);
    }

    return { valid: errors.length === 0, errors };
  }
}
