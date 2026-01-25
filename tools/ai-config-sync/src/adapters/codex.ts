/**
 * OpenAI Codex CLI Adapter
 *
 * Adapter for OpenAI Codex CLI configuration.
 * Supports skills, agents, and commands (similar to OpenCode).
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  writeFileSync,
  statSync,
  unlinkSync,
  symlinkSync,
  cpSync,
} from "node:fs";
import { join, basename, dirname } from "node:path";
import { glob } from "glob";
import * as toml from "toml";
import type {
  SystemCapabilities,
  SystemPaths,
  Artifact,
  SkillArtifact,
  AgentArtifact,
  CommandArtifact,
  McpServerArtifact,
  ArtifactType,
} from "../models/types.js";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter, type ScanOptions, type WriteOptions } from "./base.js";

export class CodexAdapter extends BaseAdapter {
  readonly systemId = "codex" as const;
  readonly name = "OpenAI Codex CLI";

  readonly capabilities: SystemCapabilities = {
    skills: true,
    agents: true,
    commands: true,
    hooks: false,
    rules: false,
    mcpServers: true,
    templates: false,
    contextFiles: false,
    instructions: true,
    symlinksSupported: true,
  };

  readonly paths: SystemPaths = {
    root: ".codex",
    skills: ".codex/skills",
    agents: ".codex/agents",
    commands: ".codex/commands",
    instructions: "AGENTS.md",
    config: ".codex/config.toml",
  };

  isConfigured(projectRoot: string): boolean {
    return existsSync(join(projectRoot, this.paths.root));
  }

  async initialize(projectRoot: string): Promise<void> {
    const dirs = [
      this.paths.root,
      this.paths.skills,
      this.paths.agents,
      this.paths.commands,
    ];

    for (const dir of dirs) {
      if (dir) {
        const fullPath = join(projectRoot, dir);
        if (!existsSync(fullPath)) {
          mkdirSync(fullPath, { recursive: true });
        }
      }
    }

    // Create default config.toml if not exists
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) {
      const defaultConfig = `# OpenAI Codex CLI Configuration
# Documentation: https://developers.openai.com/codex

[features]
shell_snapshot = true
web_search_request = true
autonomous_mode = false

[model]
default = "gpt-4"

[context]
project_doc_fallback_filenames = ["AGENTS.md", "CLAUDE.md", "README.md"]

[notifications]
notify_on_complete = true
notify_on_error = true

[tui]
notifications = true
`;
      writeFileSync(configPath, defaultConfig);
    }

    // Create AGENTS.md symlink to CLAUDE.md if needed
    const agentsPath = join(projectRoot, this.paths.instructions!);
    const claudePath = join(projectRoot, "CLAUDE.md");
    if (!existsSync(agentsPath) && existsSync(claudePath)) {
      symlinkSync("CLAUDE.md", agentsPath);
    }
  }

  async scanArtifacts(
    projectRoot: string,
    options: ScanOptions = {}
  ): Promise<Artifact[]> {
    const artifacts: Artifact[] = [];
    const types = options.types ?? ["skill", "agent", "command"];

    for (const type of types) {
      switch (type) {
        case "skill":
          artifacts.push(...(await this.scanSkills(projectRoot)));
          break;
        case "agent":
          artifacts.push(...(await this.scanAgents(projectRoot)));
          break;
        case "command":
          artifacts.push(...(await this.scanCommands(projectRoot)));
          break;
      }
    }

    return artifacts;
  }

  private async scanSkills(projectRoot: string): Promise<SkillArtifact[]> {
    const skillsDir = join(projectRoot, this.paths.skills!);
    if (!existsSync(skillsDir)) return [];

    const skillFiles = await glob("*/SKILL.md", { cwd: skillsDir });
    const skills: SkillArtifact[] = [];

    for (const skillFile of skillFiles) {
      const skillPath = join(skillsDir, skillFile);
      const skillName = dirname(skillFile);
      const content = readFileSync(skillPath, "utf-8");
      const stats = statSync(skillPath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      // Scan for references
      const referencesDir = join(skillsDir, skillName, "references");
      const references: string[] = [];
      if (existsSync(referencesDir)) {
        const refFiles = await glob("*.md", { cwd: referencesDir });
        references.push(...refFiles);
      }

      // Scan for assets
      const assetsDir = join(skillsDir, skillName, "assets");
      const assets: string[] = [];
      if (existsSync(assetsDir)) {
        const assetFiles = await glob("**/*", { cwd: assetsDir });
        assets.push(...assetFiles);
      }

      skills.push({
        id: generateArtifactId(skillName, "skill", "codex"),
        name: skillName,
        type: "skill",
        description: (frontmatter.description as string) ?? undefined,
        content,
        metadata: frontmatter,
        sourceSystem: "codex",
        sourcePath: join(this.paths.skills!, skillFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        triggers: (frontmatter.triggers as string[]) ?? undefined,
        globs: (frontmatter.globs as string[]) ?? undefined,
        references: references.length > 0 ? references : undefined,
        assets: assets.length > 0 ? assets : undefined,
      });
    }

    return skills;
  }

  private async scanAgents(projectRoot: string): Promise<AgentArtifact[]> {
    const agentsDir = join(projectRoot, this.paths.agents!);
    if (!existsSync(agentsDir)) return [];

    const agentFiles = await glob("*.md", { cwd: agentsDir });
    const agents: AgentArtifact[] = [];

    for (const agentFile of agentFiles) {
      const agentPath = join(agentsDir, agentFile);
      const agentName = basename(agentFile, ".md");
      const content = readFileSync(agentPath, "utf-8");
      const stats = statSync(agentPath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      agents.push({
        id: generateArtifactId(agentName, "agent", "codex"),
        name: agentName,
        type: "agent",
        description: (frontmatter.description as string) ?? undefined,
        content,
        metadata: frontmatter,
        sourceSystem: "codex",
        sourcePath: join(this.paths.agents!, agentFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        role: (frontmatter.role as string) ?? undefined,
        tools: (frontmatter.tools as string[]) ?? undefined,
        temperature: (frontmatter.temperature as number) ?? undefined,
      });
    }

    return agents;
  }

  private async scanCommands(projectRoot: string): Promise<CommandArtifact[]> {
    const commandsDir = join(projectRoot, this.paths.commands!);
    if (!existsSync(commandsDir)) return [];

    const commandFiles = await glob("**/*.md", { cwd: commandsDir });
    const commands: CommandArtifact[] = [];

    for (const commandFile of commandFiles) {
      const commandPath = join(commandsDir, commandFile);
      const commandName = basename(commandFile, ".md");
      const content = readFileSync(commandPath, "utf-8");
      const stats = statSync(commandPath);

      const { frontmatter } = this.parseYamlFrontmatter(content);

      commands.push({
        id: generateArtifactId(commandName, "command", "codex"),
        name: commandName,
        type: "command",
        description: (frontmatter.description as string) ?? undefined,
        content,
        metadata: frontmatter,
        sourceSystem: "codex",
        sourcePath: join(this.paths.commands!, commandFile),
        checksum: this.generateChecksum(content),
        lastModified: stats.mtime,
        command: `/${commandName}`,
        aliases: (frontmatter.aliases as string[]) ?? undefined,
      });
    }

    return commands;
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

    // Determine artifact type from path
    let type: ArtifactType = "skill";
    let name = basename(artifactPath, ".md");

    if (artifactPath.includes("/skills/")) {
      type = "skill";
      name = dirname(artifactPath).split("/").pop() ?? name;
    } else if (artifactPath.includes("/agents/")) {
      type = "agent";
    } else if (artifactPath.includes("/commands/")) {
      type = "command";
    }

    return {
      id: generateArtifactId(name, type, "codex"),
      name,
      type,
      description: (frontmatter.description as string) ?? undefined,
      content,
      metadata: frontmatter,
      sourceSystem: "codex",
      sourcePath: artifactPath,
      checksum: this.generateChecksum(content),
      lastModified: stats.mtime,
    };
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

    if (artifact.type === "skill") {
      // For skills, copy the entire directory including references and assets
      if (options.symlinkTarget) {
        const sourceSkillDir = dirname(join(projectRoot, options.symlinkTarget));
        const targetSkillDir = join(projectRoot, this.paths.skills!, artifact.name);

        if (!existsSync(targetSkillDir)) {
          mkdirSync(targetSkillDir, { recursive: true });
        }

        // Copy SKILL.md
        writeFileSync(join(targetSkillDir, "SKILL.md"), artifact.content);

        // Copy references if they exist
        const sourceRefs = join(sourceSkillDir, "references");
        const targetRefs = join(targetSkillDir, "references");
        if (existsSync(sourceRefs)) {
          cpSync(sourceRefs, targetRefs, { recursive: true });
        }

        // Copy assets if they exist
        const sourceAssets = join(sourceSkillDir, "assets");
        const targetAssets = join(targetSkillDir, "assets");
        if (existsSync(sourceAssets)) {
          cpSync(sourceAssets, targetAssets, { recursive: true });
        }
      } else {
        // Just write the SKILL.md
        const skillDir = join(projectRoot, this.paths.skills!, artifact.name);
        if (!existsSync(skillDir)) {
          mkdirSync(skillDir, { recursive: true });
        }
        writeFileSync(join(skillDir, "SKILL.md"), artifact.content);
      }
    } else {
      // Write file directly
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
    switch (artifact.type) {
      case "skill":
        return join(this.paths.skills!, artifact.name, "SKILL.md");
      case "agent":
        return join(this.paths.agents!, `${artifact.name}.md`);
      case "command":
        return join(this.paths.commands!, `${artifact.name}.md`);
      default:
        return join(this.paths.root, artifact.name);
    }
  }

  async readMcpServers(projectRoot: string): Promise<McpServerArtifact[]> {
    const configPath = join(projectRoot, this.paths.config!);
    if (!existsSync(configPath)) return [];

    try {
      const content = readFileSync(configPath, "utf-8");
      const config = toml.parse(content);
      const servers: McpServerArtifact[] = [];

      // TOML sections starting with [mcp.xxx]
      for (const [key, value] of Object.entries(config)) {
        if (key === "mcp" && typeof value === "object" && value !== null) {
          for (const [serverName, serverConfig] of Object.entries(
            value as Record<string, unknown>
          )) {
            const server = serverConfig as {
              command?: string[];
              env?: Record<string, string>;
            };

            servers.push({
              id: generateArtifactId(serverName, "mcp_server", "codex"),
              name: serverName,
              type: "mcp_server",
              content: JSON.stringify(server, null, 2),
              metadata: {},
              sourceSystem: "codex",
              sourcePath: this.paths.config!,
              checksum: this.generateChecksum(JSON.stringify(server)),
              lastModified: statSync(configPath).mtime,
              command: server.command ?? [],
              env: server.env,
              enabled: true,
            });
          }
        }
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
    const configPath = join(projectRoot, this.paths.config!);
    let content = "";

    if (existsSync(configPath)) {
      content = readFileSync(configPath, "utf-8");

      // Remove existing [mcp.*] sections
      content = content.replace(/\[mcp\.\w+\][\s\S]*?(?=\[|$)/g, "");
    }

    // Append MCP server sections
    for (const server of servers) {
      content += `\n[mcp.${server.name}]\n`;
      content += `command = ${JSON.stringify(server.command)}\n`;
      if (server.env && Object.keys(server.env).length > 0) {
        content += `env = { `;
        content += Object.entries(server.env)
          .map(([k, v]) => `${k} = "${v}"`)
          .join(", ");
        content += ` }\n`;
      }
    }

    writeFileSync(configPath, content);
  }
}
