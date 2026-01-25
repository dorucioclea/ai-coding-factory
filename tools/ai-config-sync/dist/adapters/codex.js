/**
 * OpenAI Codex CLI Adapter
 *
 * Adapter for OpenAI Codex CLI configuration.
 * Supports skills, agents, and commands (similar to OpenCode).
 */
import { existsSync, mkdirSync, readFileSync, writeFileSync, statSync, unlinkSync, symlinkSync, cpSync, } from "node:fs";
import { join, basename, dirname } from "node:path";
import { glob } from "glob";
import * as toml from "toml";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter } from "./base.js";
export class CodexAdapter extends BaseAdapter {
    systemId = "codex";
    name = "OpenAI Codex CLI";
    capabilities = {
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
    paths = {
        root: ".codex",
        skills: ".codex/skills",
        agents: ".codex/agents",
        commands: ".codex/commands",
        instructions: "AGENTS.md",
        config: ".codex/config.toml",
    };
    isConfigured(projectRoot) {
        return existsSync(join(projectRoot, this.paths.root));
    }
    async initialize(projectRoot) {
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
        const configPath = join(projectRoot, this.paths.config);
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
        const agentsPath = join(projectRoot, this.paths.instructions);
        const claudePath = join(projectRoot, "CLAUDE.md");
        if (!existsSync(agentsPath) && existsSync(claudePath)) {
            symlinkSync("CLAUDE.md", agentsPath);
        }
    }
    async scanArtifacts(projectRoot, options = {}) {
        const artifacts = [];
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
    async scanSkills(projectRoot) {
        const skillsDir = join(projectRoot, this.paths.skills);
        if (!existsSync(skillsDir))
            return [];
        const skillFiles = await glob("*/SKILL.md", { cwd: skillsDir });
        const skills = [];
        for (const skillFile of skillFiles) {
            const skillPath = join(skillsDir, skillFile);
            const skillName = dirname(skillFile);
            const content = readFileSync(skillPath, "utf-8");
            const stats = statSync(skillPath);
            const { frontmatter } = this.parseYamlFrontmatter(content);
            // Scan for references
            const referencesDir = join(skillsDir, skillName, "references");
            const references = [];
            if (existsSync(referencesDir)) {
                const refFiles = await glob("*.md", { cwd: referencesDir });
                references.push(...refFiles);
            }
            // Scan for assets
            const assetsDir = join(skillsDir, skillName, "assets");
            const assets = [];
            if (existsSync(assetsDir)) {
                const assetFiles = await glob("**/*", { cwd: assetsDir });
                assets.push(...assetFiles);
            }
            skills.push({
                id: generateArtifactId(skillName, "skill", "codex"),
                name: skillName,
                type: "skill",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "codex",
                sourcePath: join(this.paths.skills, skillFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                triggers: frontmatter.triggers ?? undefined,
                globs: frontmatter.globs ?? undefined,
                references: references.length > 0 ? references : undefined,
                assets: assets.length > 0 ? assets : undefined,
            });
        }
        return skills;
    }
    async scanAgents(projectRoot) {
        const agentsDir = join(projectRoot, this.paths.agents);
        if (!existsSync(agentsDir))
            return [];
        const agentFiles = await glob("*.md", { cwd: agentsDir });
        const agents = [];
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
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "codex",
                sourcePath: join(this.paths.agents, agentFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                role: frontmatter.role ?? undefined,
                tools: frontmatter.tools ?? undefined,
                temperature: frontmatter.temperature ?? undefined,
            });
        }
        return agents;
    }
    async scanCommands(projectRoot) {
        const commandsDir = join(projectRoot, this.paths.commands);
        if (!existsSync(commandsDir))
            return [];
        const commandFiles = await glob("**/*.md", { cwd: commandsDir });
        const commands = [];
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
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "codex",
                sourcePath: join(this.paths.commands, commandFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                command: `/${commandName}`,
                aliases: frontmatter.aliases ?? undefined,
            });
        }
        return commands;
    }
    async readArtifact(projectRoot, artifactPath) {
        const fullPath = join(projectRoot, artifactPath);
        if (!existsSync(fullPath))
            return null;
        const content = readFileSync(fullPath, "utf-8");
        const stats = statSync(fullPath);
        const { frontmatter } = this.parseYamlFrontmatter(content);
        // Determine artifact type from path
        let type = "skill";
        let name = basename(artifactPath, ".md");
        if (artifactPath.includes("/skills/")) {
            type = "skill";
            name = dirname(artifactPath).split("/").pop() ?? name;
        }
        else if (artifactPath.includes("/agents/")) {
            type = "agent";
        }
        else if (artifactPath.includes("/commands/")) {
            type = "command";
        }
        return {
            id: generateArtifactId(name, type, "codex"),
            name,
            type,
            description: frontmatter.description ?? undefined,
            content,
            metadata: frontmatter,
            sourceSystem: "codex",
            sourcePath: artifactPath,
            checksum: this.generateChecksum(content),
            lastModified: stats.mtime,
        };
    }
    async writeArtifact(projectRoot, artifact, options = {}) {
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
                const targetSkillDir = join(projectRoot, this.paths.skills, artifact.name);
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
            }
            else {
                // Just write the SKILL.md
                const skillDir = join(projectRoot, this.paths.skills, artifact.name);
                if (!existsSync(skillDir)) {
                    mkdirSync(skillDir, { recursive: true });
                }
                writeFileSync(join(skillDir, "SKILL.md"), artifact.content);
            }
        }
        else {
            // Write file directly
            if (!options.overwrite && existsSync(fullPath)) {
                throw new Error(`File already exists: ${fullPath}`);
            }
            writeFileSync(fullPath, artifact.content);
        }
        return targetPath;
    }
    async deleteArtifact(projectRoot, artifactPath) {
        const fullPath = join(projectRoot, artifactPath);
        if (existsSync(fullPath)) {
            unlinkSync(fullPath);
        }
    }
    getArtifactPath(artifact) {
        switch (artifact.type) {
            case "skill":
                return join(this.paths.skills, artifact.name, "SKILL.md");
            case "agent":
                return join(this.paths.agents, `${artifact.name}.md`);
            case "command":
                return join(this.paths.commands, `${artifact.name}.md`);
            default:
                return join(this.paths.root, artifact.name);
        }
    }
    async readMcpServers(projectRoot) {
        const configPath = join(projectRoot, this.paths.config);
        if (!existsSync(configPath))
            return [];
        try {
            const content = readFileSync(configPath, "utf-8");
            const config = toml.parse(content);
            const servers = [];
            // TOML sections starting with [mcp.xxx]
            for (const [key, value] of Object.entries(config)) {
                if (key === "mcp" && typeof value === "object" && value !== null) {
                    for (const [serverName, serverConfig] of Object.entries(value)) {
                        const server = serverConfig;
                        servers.push({
                            id: generateArtifactId(serverName, "mcp_server", "codex"),
                            name: serverName,
                            type: "mcp_server",
                            content: JSON.stringify(server, null, 2),
                            metadata: {},
                            sourceSystem: "codex",
                            sourcePath: this.paths.config,
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
        }
        catch {
            return [];
        }
    }
    async writeMcpServers(projectRoot, servers) {
        const configPath = join(projectRoot, this.paths.config);
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
//# sourceMappingURL=codex.js.map