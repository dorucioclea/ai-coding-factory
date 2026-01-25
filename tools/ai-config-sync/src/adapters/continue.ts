/**
 * Continue.dev Adapter
 *
 * Adapter for Continue.dev AI coding assistant.
 * Continue uses .continue/ directory with config.json and supports
 * rules and context files.
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
  RuleArtifact,
  ContextArtifact,
  InstructionArtifact,
  SkillArtifact,
} from "../models/types.js";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";

export class ContinueAdapter extends BaseAdapter {
  readonly systemId = "continue" as const;
  readonly name = "Continue.dev";

  readonly capabilities: SystemCapabilities = {
    skills: false,
    agents: false,
    commands: false,
    hooks: false,
    rules: true,
    mcpServers: false,
    templates: false,
    contextFiles: true,
    instructions: true,
    symlinksSupported: true,
  };

  readonly paths: SystemPaths = {
    root: ".continue",
    rules: ".continue/rules",
    context: ".continue/context",
    instructions: ".continuerc.json",
    config: ".continue/config.json",
  };

  isConfigured(projectRoot: string): boolean {
    return (
      existsSync(join(projectRoot, ".continue")) ||
      existsSync(join(projectRoot, ".continuerc.json"))
    );
  }

  async initialize(projectRoot: string): Promise<void> {
    const dirs = [this.paths.root, this.paths.rules, this.paths.context];

    for (const dir of dirs) {
      if (dir) {
        const fullPath = join(projectRoot, dir);
        if (!existsSync(fullPath)) {
          mkdirSync(fullPath, { recursive: true });
        }
      }
    }

    // Create default config.json if it doesn't exist
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) {
      const defaultConfig = {
        models: [],
        customCommands: [],
        contextProviders: [
          {
            name: "file",
            params: {
              files: [".continue/SKILL_INDEX.md"],
            },
          },
        ],
        slashCommands: [],
      };
      writeFileSync(configPath, JSON.stringify(defaultConfig, null, 2));
    } else {
      // Ensure SKILL_INDEX.md is in context providers
      try {
        const config = JSON.parse(readFileSync(configPath, "utf-8"));
        if (!config.contextProviders) {
          config.contextProviders = [];
        }
        const hasSkillIndex = config.contextProviders.some(
          (p: { name?: string; params?: { files?: string[] } }) =>
            p.name === "file" && p.params?.files?.includes(".continue/SKILL_INDEX.md")
        );
        if (!hasSkillIndex) {
          config.contextProviders.push({
            name: "file",
            params: {
              files: [".continue/SKILL_INDEX.md"],
            },
          });
          writeFileSync(configPath, JSON.stringify(config, null, 2));
        }
      } catch {
        // Config might be malformed, skip
      }
    }
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["rule", "context", "instruction"];

    if (types.includes("rule")) {
      artifacts.push(...(await this.scanRules(projectRoot)));
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

  private async scanRules(projectRoot: string): Promise<RuleArtifact[]> {
    const rulesDir = join(projectRoot, this.paths.rules!);
    if (!existsSync(rulesDir)) return [];

    const ruleFiles = await glob("**/*.md", { cwd: rulesDir });
    const rules: RuleArtifact[] = [];

    for (const ruleFile of ruleFiles) {
      const rulePath = join(rulesDir, ruleFile);
      const ruleName = basename(ruleFile, ".md");
      const content = readFileSync(rulePath, "utf-8");
      const stats = statSync(rulePath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      rules.push({
        id: generateArtifactId(ruleName, "rule", "continue"),
        name: ruleName,
        type: "rule",
        description: (frontmatter.description as string) ?? undefined,
        content,
        metadata: frontmatter,
        sourceSystem: "continue",
        sourcePath: join(this.paths.rules!, ruleFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        globs: (frontmatter.globs as string[]) ?? undefined,
        alwaysApply: (frontmatter.alwaysApply as boolean) ?? false,
      });
    }

    return rules;
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
        id: generateArtifactId(contextName, "context", "continue"),
        name: contextName,
        type: "context",
        description: `Context file: ${contextName}`,
        content,
        metadata: {},
        sourceSystem: "continue",
        sourcePath: join(this.paths.context!, contextFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        contextType: "file",
      });
    }

    return contexts;
  }

  private async scanInstructions(projectRoot: string): Promise<InstructionArtifact | null> {
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) return null;

    const content = readFileSync(configPath, "utf-8");
    const stats = statSync(configPath);

    return {
      id: generateArtifactId("continue-config", "instruction", "continue"),
      name: "continue-config",
      type: "instruction",
      description: "Continue.dev configuration",
      content,
      metadata: {},
      sourceSystem: "continue",
      sourcePath: this.paths.config!,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      format: "json",
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
    const name = basename(artifactPath, ".md");

    // Determine type based on path
    let type: "rule" | "context" | "instruction" = "rule";
    if (artifactPath.includes("/context/")) {
      type = "context";
    } else if (artifactPath.endsWith(".json")) {
      type = "instruction";
    }

    return {
      id: generateArtifactId(name, type, "continue"),
      name,
      type,
      content,
      metadata: {},
      sourceSystem: "continue",
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

    if (!options.overwrite && existsSync(fullPath)) {
      throw new Error(`File already exists: ${fullPath}`);
    }

    writeFileSync(fullPath, artifact.content);
    return targetPath;
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
      case "rule":
        return join(this.paths.rules!, `${artifact.name}.md`);
      case "context":
        return join(this.paths.context!, `${artifact.name}.md`);
      case "instruction":
        return this.paths.config!;
      default:
        return join(this.paths.root, `${artifact.name}.md`);
    }
  }

  /**
   * Transform a skill to a Continue rule
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    if (artifact.type === "skill") {
      return this.skillToRule(artifact as SkillArtifact);
    }
    return artifact;
  }

  private skillToRule(skill: SkillArtifact): RuleArtifact {
    const { body } = this.parseYamlFrontmatter(skill.content);

    // Generate frontmatter for Continue
    const frontmatter = this.generateYamlFrontmatter({
      description: skill.description ?? `${skill.name} rules`,
      globs: skill.globs ?? ["**/*"],
    });

    const content = `${frontmatter}\n\n${body}`;

    return {
      id: generateArtifactId(skill.name, "rule", "continue"),
      name: skill.name,
      type: "rule",
      description: skill.description ?? `${skill.name} rules`,
      content,
      metadata: { ...skill.metadata, sourceSkill: skill.name },
      sourceSystem: "continue",
      sourcePath: join(this.paths.rules!, `${skill.name}.md`),
      checksum: this.generateChecksum(content),
      lastModified: new Date(),
      globs: skill.globs ?? ["**/*"],
      alwaysApply: false,
    };
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (!["rule", "context", "instruction"].includes(artifact.type)) {
      errors.push(`Continue only supports rule, context, and instruction artifacts, got: ${artifact.type}`);
    }

    return { valid: errors.length === 0, errors };
  }
}
