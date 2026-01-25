/**
 * Windsurf IDE Adapter
 *
 * Adapter for Codeium's Windsurf AI IDE configuration.
 * Supports rules (markdown files) and MCP servers.
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  writeFileSync,
  statSync,
  unlinkSync,
  symlinkSync,
} from "node:fs";
import { join, basename, dirname, relative } from "node:path";
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

export class WindsurfAdapter extends BaseAdapter {
  readonly systemId = "windsurf" as const;
  readonly name = "Windsurf IDE";

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
    root: ".windsurf",
    rules: ".windsurf/rules",
    mcpServers: ".windsurf/mcp.json",
    instructions: ".windsurfrules",
    config: ".windsurf/settings.json",
  };

  isConfigured(projectRoot: string): boolean {
    return (
      existsSync(join(projectRoot, this.paths.root)) ||
      existsSync(join(projectRoot, ".windsurfrules"))
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

    // Create .windsurfrules from CLAUDE.md if available
    const windsurfRules = join(projectRoot, this.paths.instructions!);
    const claudeMd = join(projectRoot, "CLAUDE.md");
    if (!existsSync(windsurfRules) && existsSync(claudeMd)) {
      symlinkSync("CLAUDE.md", windsurfRules);
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

    const ruleFiles = await glob("*.md", { cwd: rulesDir });
    const rules: RuleArtifact[] = [];

    for (const ruleFile of ruleFiles) {
      const rulePath = join(rulesDir, ruleFile);
      const ruleName = basename(ruleFile, ".md");
      const content = readFileSync(rulePath, "utf-8");
      const stats = statSync(rulePath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      rules.push({
        id: generateArtifactId(ruleName, "rule", "windsurf"),
        name: ruleName,
        type: "rule",
        description: (frontmatter.description as string) ?? this.extractDescription(content),
        content,
        metadata: frontmatter,
        sourceSystem: "windsurf",
        sourcePath: join(this.paths.rules!, ruleFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        globs: (frontmatter.globs as string[]) ?? undefined,
        alwaysApply: (frontmatter.alwaysApply as boolean) ?? false,
      });
    }

    return rules;
  }

  /**
   * Extract description from first heading or paragraph
   */
  private extractDescription(content: string): string | undefined {
    // Try to get first heading
    const headingMatch = content.match(/^#\s+(.+)$/m);
    if (headingMatch) {
      return headingMatch[1];
    }

    // Try first non-empty line
    const lines = content.split("\n").filter((l) => l.trim());
    if (lines.length > 0) {
      return lines[0].slice(0, 100);
    }

    return undefined;
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

    const name = basename(artifactPath, ".md");

    return {
      id: generateArtifactId(name, "rule", "windsurf"),
      name,
      type: "rule",
      description: (frontmatter.description as string) ?? this.extractDescription(content),
      content,
      metadata: frontmatter,
      sourceSystem: "windsurf",
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

    if (options.useSymlink && options.symlinkTarget) {
      // Create symlink
      if (existsSync(fullPath)) {
        unlinkSync(fullPath);
      }
      const relativePath = relative(targetDir, join(projectRoot, options.symlinkTarget));
      symlinkSync(relativePath, fullPath);
    } else {
      if (!options.overwrite && existsSync(fullPath)) {
        throw new Error(`File already exists: ${fullPath}`);
      }
      writeFileSync(fullPath, artifact.content);
    }

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
      return join(this.paths.rules!, `${artifact.name}.md`);
    }
    return join(this.paths.root, `${artifact.name}.md`);
  }

  /**
   * Transform a skill artifact to Windsurf markdown rule
   */
  transformArtifact(artifact: Artifact, _options?: TransformOptions): Artifact {
    if (artifact.type === "skill") {
      return this.skillToRule(artifact as SkillArtifact);
    }
    if (artifact.type === "rule" && artifact.sourceSystem !== "windsurf") {
      return this.normalizeRule(artifact as RuleArtifact);
    }
    return artifact;
  }

  /**
   * Convert a skill to a Windsurf markdown rule
   */
  private skillToRule(skill: SkillArtifact): RuleArtifact {
    const { body } = this.parseYamlFrontmatter(skill.content);

    // Windsurf uses plain markdown, optionally with frontmatter
    let content = body;

    // Add description as a comment at the top if available
    if (skill.description) {
      content = `<!-- ${skill.description} -->\n\n${content}`;
    }

    return {
      id: generateArtifactId(skill.name, "rule", "windsurf"),
      name: skill.name,
      type: "rule",
      description: skill.description,
      content,
      metadata: { ...skill.metadata, sourceSkill: skill.name },
      sourceSystem: "windsurf",
      sourcePath: join(this.paths.rules!, `${skill.name}.md`),
      checksum: this.generateChecksum(content),
      lastModified: new Date(),
      globs: skill.globs,
      alwaysApply: false,
    };
  }

  /**
   * Normalize a rule from another system
   */
  private normalizeRule(rule: RuleArtifact): RuleArtifact {
    const { body } = this.parseYamlFrontmatter(rule.content);

    return {
      ...rule,
      id: generateArtifactId(rule.name, "rule", "windsurf"),
      sourceSystem: "windsurf",
      sourcePath: join(this.paths.rules!, `${rule.name}.md`),
      content: body, // Remove frontmatter for Windsurf
      checksum: this.generateChecksum(body),
      lastModified: new Date(),
    };
  }

  validateArtifact(artifact: Artifact): { valid: boolean; errors: string[] } {
    const baseResult = super.validateArtifact(artifact);
    const errors = [...baseResult.errors];

    if (artifact.type !== "rule") {
      errors.push(`Windsurf only supports rule artifacts, got: ${artifact.type}`);
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
          id: generateArtifactId(name, "mcp_server", "windsurf"),
          name,
          type: "mcp_server",
          content: JSON.stringify(server, null, 2),
          metadata: {},
          sourceSystem: "windsurf",
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
