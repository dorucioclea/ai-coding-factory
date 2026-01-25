/**
 * OpenCode Adapter
 *
 * Adapter for OpenCode AI CLI configuration.
 * Supports skills (symlinked from Claude), agents, and commands.
 */
import { existsSync, mkdirSync, readFileSync, writeFileSync, statSync, symlinkSync, unlinkSync, lstatSync, readlinkSync, } from "node:fs";
import { join, basename, dirname, relative } from "node:path";
import { glob } from "glob";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter } from "./base.js";
export class OpenCodeAdapter extends BaseAdapter {
    systemId = "opencode";
    name = "OpenCode";
    capabilities = {
        skills: true,
        agents: true,
        commands: true,
        hooks: false,
        rules: false,
        mcpServers: true,
        templates: true,
        contextFiles: false,
        instructions: true,
        symlinksSupported: true,
    };
    paths = {
        root: ".opencode",
        skills: ".opencode/skill",
        agents: ".opencode/agent",
        commands: ".opencode/commands",
        templates: ".opencode/templates",
        instructions: "AGENTS.md",
        config: ".opencode/opencode.json",
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
            this.paths.templates,
        ];
        for (const dir of dirs) {
            if (dir) {
                const fullPath = join(projectRoot, dir);
                if (!existsSync(fullPath)) {
                    mkdirSync(fullPath, { recursive: true });
                }
            }
        }
        // Create default opencode.json if not exists
        const configPath = join(projectRoot, this.paths.config);
        if (!existsSync(configPath)) {
            const defaultConfig = {
                model: "gpt-4",
                provider: {},
                permission: { bash: {}, edit: "ask", write: "ask" },
                agent: {},
                skill: {},
                mcp: {},
            };
            writeFileSync(configPath, JSON.stringify(defaultConfig, null, 2));
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
        const skillDirs = await glob("*/", { cwd: skillsDir });
        const skills = [];
        for (const skillDir of skillDirs) {
            const skillName = skillDir.replace(/\/$/, "");
            const skillPath = join(skillsDir, skillName);
            // Check if it's a symlink
            const isSymlink = lstatSync(skillPath).isSymbolicLink();
            let content = "";
            let sourcePath = join(this.paths.skills, skillName);
            if (isSymlink) {
                // Follow symlink to read content
                const target = readlinkSync(skillPath);
                const realPath = join(skillsDir, target, "SKILL.md");
                if (existsSync(realPath)) {
                    content = readFileSync(realPath, "utf-8");
                }
            }
            else {
                const skillFile = join(skillPath, "SKILL.md");
                if (existsSync(skillFile)) {
                    content = readFileSync(skillFile, "utf-8");
                }
            }
            if (!content)
                continue;
            const stats = statSync(skillPath);
            const { frontmatter } = this.parseYamlFrontmatter(content);
            skills.push({
                id: generateArtifactId(skillName, "skill", "opencode"),
                name: skillName,
                type: "skill",
                description: frontmatter.description ?? undefined,
                content,
                metadata: { ...frontmatter, isSymlink },
                sourceSystem: "opencode",
                sourcePath,
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                triggers: frontmatter.triggers ?? undefined,
                globs: frontmatter.globs ?? undefined,
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
                id: generateArtifactId(agentName, "agent", "opencode"),
                name: agentName,
                type: "agent",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "opencode",
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
                id: generateArtifactId(commandName, "command", "opencode"),
                name: commandName,
                type: "command",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "opencode",
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
        const stats = statSync(fullPath);
        let content;
        // Handle symlinks
        if (lstatSync(fullPath).isSymbolicLink()) {
            const target = readlinkSync(fullPath);
            const realPath = join(dirname(fullPath), target);
            if (existsSync(realPath)) {
                content = readFileSync(realPath, "utf-8");
            }
            else {
                return null;
            }
        }
        else {
            content = readFileSync(fullPath, "utf-8");
        }
        const { frontmatter } = this.parseYamlFrontmatter(content);
        // Determine artifact type from path
        let type = "skill";
        let name = basename(artifactPath, ".md");
        if (artifactPath.includes("/skill/")) {
            type = "skill";
            name = dirname(artifactPath).split("/").pop() ?? name;
        }
        else if (artifactPath.includes("/agent/")) {
            type = "agent";
        }
        else if (artifactPath.includes("/commands/")) {
            type = "command";
        }
        return {
            id: generateArtifactId(name, type, "opencode"),
            name,
            type,
            description: frontmatter.description ?? undefined,
            content,
            metadata: frontmatter,
            sourceSystem: "opencode",
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
        // Remove existing symlink or file
        if (existsSync(fullPath) || lstatSync(fullPath).isSymbolicLink()) {
            try {
                unlinkSync(fullPath);
            }
            catch {
                // Ignore errors
            }
        }
        if (options.useSymlink && options.symlinkTarget) {
            // Create symlink to source directory
            const sourcePath = join(projectRoot, options.symlinkTarget);
            const sourceDir = dirname(sourcePath);
            const relativePath = relative(targetDir, sourceDir);
            symlinkSync(relativePath, fullPath);
        }
        else {
            // For skills, create directory structure
            if (artifact.type === "skill") {
                if (!existsSync(fullPath)) {
                    mkdirSync(fullPath, { recursive: true });
                }
                writeFileSync(join(fullPath, "SKILL.md"), artifact.content);
            }
            else {
                // Write file directly
                if (!options.overwrite && existsSync(fullPath)) {
                    throw new Error(`File already exists: ${fullPath}`);
                }
                writeFileSync(fullPath, artifact.content);
            }
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
                return join(this.paths.skills, artifact.name);
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
            const config = JSON.parse(content);
            const servers = [];
            for (const [name, serverConfig] of Object.entries(config.mcp || {})) {
                const server = serverConfig;
                servers.push({
                    id: generateArtifactId(name, "mcp_server", "opencode"),
                    name,
                    type: "mcp_server",
                    content: JSON.stringify(server, null, 2),
                    metadata: {},
                    sourceSystem: "opencode",
                    sourcePath: this.paths.config,
                    checksum: this.generateChecksum(JSON.stringify(server)),
                    lastModified: statSync(configPath).mtime,
                    command: server.command ?? [],
                    env: server.env,
                    enabled: true,
                });
            }
            return servers;
        }
        catch {
            return [];
        }
    }
    async writeMcpServers(projectRoot, servers) {
        const configPath = join(projectRoot, this.paths.config);
        let config = {};
        if (existsSync(configPath)) {
            try {
                config = JSON.parse(readFileSync(configPath, "utf-8"));
            }
            catch {
                // Start fresh
            }
        }
        config.mcp = {};
        for (const server of servers) {
            config.mcp[server.name] = {
                command: server.command,
                env: server.env,
            };
        }
        writeFileSync(configPath, JSON.stringify(config, null, 2));
    }
}
//# sourceMappingURL=opencode.js.map