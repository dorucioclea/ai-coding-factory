/**
 * Cursor IDE Adapter
 *
 * Adapter for Cursor IDE configuration.
 * Supports rules (.mdc files with YAML frontmatter).
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  writeFileSync,
  statSync,
  unlinkSync,
} from "node:fs";
import { join, basename, dirname } from "node:path";
import { glob } from "glob";
import type {
  SystemCapabilities,
  SystemPaths,
  Artifact,
  RuleArtifact,
  McpServerArtifact,
  SkillArtifact,
} from "../models/types.js";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions, type TransformOptions } from "./base.js";

export class CursorAdapter extends BaseAdapter {
  readonly systemId = "cursor" as const;
  readonly name = "Cursor IDE";

  readonly capabilities: SystemCapabilities = {
    skills: false,
    agents: false,
    commands: false,
    hooks: false,
    rules: true,
    mcpServers: true,
    templates: false,
    contextFiles: false,
    instructions: true,
    symlinksSupported: true,
  };

  readonly paths: SystemPaths = {
    root: ".cursor",
    rules: ".cursor/rules",
    mcpServers: ".cursor/mcp.json",
    instructions: ".cursorrules",
    config: ".cursor/settings.json",
  };

  isConfigured(projectRoot: string): boolean {
    return (
      existsSync(join(projectRoot, this.paths.root)) ||
      existsSync(join(projectRoot, ".cursorrules"))
    );
  }

  async initialize(projectRoot: string): Promise<void> {
    const dirs = [this.paths.root, this.paths.rules];

    for (const dir of dirs) {
      if (dir) {
        const fullPath = join(projectRoot, dir);
        if (!existsSync(fullPath)) {
          mkdirSync(fullPath, { recursive: true });
        }
      }
    }
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["rule"];

    if (types.includes("rule")) {
      artifacts.push(...(await this.scanRules(projectRoot)));
    }

    return artifacts;
  }

  private async scanRules(projectRoot: string): Promise<RuleArtifact[]> {
    const rulesDir = join(projectRoot, this.paths.rules!);
    if (!existsSync(rulesDir)) return [];

    const ruleFiles = await glob("*.mdc", { cwd: rulesDir });
    const rules: RuleArtifact[] = [];

    for (const ruleFile of ruleFiles) {
      const rulePath = join(rulesDir, ruleFile);
      const ruleName = basename(ruleFile, ".mdc");
      const content = readFileSync(rulePath, "utf-8");
      const stats = statSync(rulePath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      rules.push({
        id: generateArtifactId(ruleName, "rule", "cursor"),
        name: ruleName,
        type: "rule",
        description: (frontmatter.description as string) ?? undefined,
        content,
        metadata: frontmatter,
        sourceSystem: "cursor",
        sourcePath: join(this.paths.rules!, ruleFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        globs: (frontmatter.globs as string[]) ?? undefined,
        alwaysApply: (frontmatter.alwaysApply as boolean) ?? false,
      });
    }

    return rules;
  }

  async readArtifact(
    projectRoot: string,
    artifactPath: string
  ): Promise<Artifact | null> {
    const fullPath = join(projectRoot, artifactPath);
    if (!existsSync(fullPath)) return null;

    const content = readFileSync(fullPath, "utf-8");
    const stats = statSync(fullPath);
    const { frontmatter } = this.parseYamlFrontmatter(content);

    const name = basename(artifactPath, ".mdc");

    return {
      id: generateArtifactId(name, "rule", "cursor"),
      name,
      type: "rule",
      description: (frontmatter.description as string) ?? undefined,
      content,
      metadata: frontmatter,
      sourceSystem: "cursor",
      sourcePath: artifactPath,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
      globs: (frontmatter.globs as string[]) ?? undefined,
      alwaysApply: (frontmatter.alwaysApply as boolean) ?? false,
    } as RuleArtifact;
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
    if (artifact.type === "rule") {
      return join(this.paths.rules!, `${artifact.name}.mdc`);
    }
    return join(this.paths.root, `${artifact.name}.mdc`);
  }

  /**
   * Transform a skill artifact to Cursor .mdc rule format
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    if (artifact.type === "skill") {
      return this.skillToRule(artifact as SkillArtifact);
    }
    if (artifact.type === "rule" && artifact.sourceSystem !== "cursor") {
      return this.ruleToMdc(artifact as RuleArtifact);
    }
    return artifact;
  }

  /**
   * Convert a skill to a Cursor .mdc rule
   */
  private skillToRule(skill: SkillArtifact): RuleArtifact {
    // Determine globs based on skill name
    let globs: string[] = ["**/*"];

    const name = skill.name.toLowerCase();
    if (name.includes("react") || name.includes("frontend") || name.includes("rn-")) {
      globs = ["**/*.tsx", "**/*.jsx", "**/*.ts", "**/*.js"];
    } else if (
      name.includes("net") ||
      name.includes("dotnet") ||
      name.includes("csharp")
    ) {
      globs = ["**/*.cs", "**/*.csproj", "**/*.sln"];
    } else if (name.includes("test")) {
      globs = ["**/*Test*.cs", "**/*Tests*.cs", "**/*.test.ts", "**/*.spec.ts"];
    } else if (name.includes("docker")) {
      globs = ["**/Dockerfile*", "**/docker-compose*.yml"];
    } else if (name.includes("kubernetes") || name.includes("k8s")) {
      globs = ["**/*.yaml", "**/*.yml"];
    } else if (name.includes("python")) {
      globs = ["**/*.py"];
    } else if (name.includes("rust") || name.includes("cargo")) {
      globs = ["**/*.rs", "**/Cargo.toml"];
    } else if (name.includes("go")) {
      globs = ["**/*.go", "**/go.mod"];
    }

    // Use skill globs if available
    if (skill.globs && skill.globs.length > 0) {
      globs = skill.globs;
    }

    const description = skill.description ?? `${skill.name} rules`;

    // Generate .mdc content
    const { body } = this.parseYamlFrontmatter(skill.content);
    const frontmatter = this.generateYamlFrontmatter({
      description,
      globs,
      alwaysApply: false,
    });

    const content = `${frontmatter}\n\n${body}`;

    return {
      id: generateArtifactId(skill.name, "rule", "cursor"),
      name: skill.name,
      type: "rule",
      description,
      content,
      metadata: { ...skill.metadata, sourceSkill: skill.name },
      sourceSystem: "cursor",
      sourcePath: join(this.paths.rules!, `${skill.name}.mdc`),
      checksum: this.generateChecksum(content),
      lastModified: new Date(),
      globs,
      alwaysApply: false,
    };
  }

  /**
   * Convert a rule from another system to Cursor .mdc format
   */
  private ruleToMdc(rule: RuleArtifact): RuleArtifact {
    const { body } = this.parseYamlFrontmatter(rule.content);

    const frontmatter = this.generateYamlFrontmatter({
      description: rule.description ?? `${rule.name} rules`,
      globs: rule.globs ?? ["**/*"],
      alwaysApply: rule.alwaysApply ?? false,
    });

    const content = `${frontmatter}\n\n${body}`;

    return {
      ...rule,
      id: generateArtifactId(rule.name, "rule", "cursor"),
      sourceSystem: "cursor",
      sourcePath: join(this.paths.rules!, `${rule.name}.mdc`),
      content,
      checksum: this.generateChecksum(content),
      lastModified: new Date(),
    };
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (artifact.type !== "rule") {
      errors.push(`Cursor only supports rule artifacts, got: ${artifact.type}`);
    }

    // Validate .mdc format
    if (artifact.content && !artifact.content.startsWith("---")) {
      errors.push("Cursor .mdc files must start with YAML frontmatter (---)");
    }

    return { valid: errors.length === 0, errors };
  }

  async readMcpServers(projectRoot: string): Promise<McpServerArtifact[]> {
    const mcpPath = join(projectRoot, this.paths.mcpServers!);
    if (!existsSync(mcpPath)) return [];

    try {
      const content = readFileSync(mcpPath, "utf-8");
      const config = JSON.parse(content);
      const servers: McpServerArtifact[] = [];

      for (const [name, serverConfig] of Object.entries(config.mcpServers || config)) {
        const server = serverConfig as {
          command?: string[];
          env?: Record<string, string>;
        };

        servers.push({
          id: generateArtifactId(name, "mcp_server", "cursor"),
          name,
          type: "mcp_server",
          content: JSON.stringify(server, null, 2),
          metadata: {},
          sourceSystem: "cursor",
          sourcePath: this.paths.mcpServers!,
          checksum: this.generateChecksum(JSON.stringify(server)),
          lastModified: statSync(mcpPath).mtime,
          command: server.command ?? [],
          env: server.env,
          enabled: true,
        });
      }

      return servers;
    } catch {
      return [];
    }
  }

  async writeMcpServers(
    projectRoot: string,
    servers: McpServerArtifact[]
  ): Promise<void> {
    const mcpPath = join(projectRoot, this.paths.mcpServers!);
    const targetDir = dirname(mcpPath);

    if (!existsSync(targetDir)) {
      mkdirSync(targetDir, { recursive: true });
    }

    const config: { mcpServers: Record<string, unknown> } = { mcpServers: {} };

    for (const server of servers) {
      config.mcpServers[server.name] = {
        command: server.command,
        env: server.env,
      };
    }

    writeFileSync(mcpPath, JSON.stringify(config, null, 2));
  }
}
