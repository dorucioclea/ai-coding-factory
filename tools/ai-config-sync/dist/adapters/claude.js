/**
 * Claude Code Adapter
 *
 * Adapter for Anthropic's Claude Code CLI configuration.
 * This is the primary/source system for most configurations.
 */
import { existsSync, mkdirSync, readFileSync, writeFileSync, statSync, symlinkSync, unlinkSync, readdirSync } from "node:fs";
import { join, basename, dirname, relative } from "node:path";
import { glob } from "glob";
import { generateArtifactId } from "../models/types.js";
import { BaseAdapter } from "./base.js";
export class ClaudeAdapter extends BaseAdapter {
    systemId = "claude";
    name = "Claude Code";
    capabilities = {
        skills: true,
        agents: true,
        commands: true,
        hooks: true,
        rules: true,
        mcpServers: true,
        templates: true,
        contextFiles: true,
        instructions: true,
        symlinksSupported: true,
    };
    paths = {
        root: ".claude",
        skills: ".claude/skills",
        agents: ".claude/agents",
        commands: ".claude/commands",
        hooks: ".claude/hooks",
        rules: ".claude/rules",
        mcpServers: ".claude/mcp-servers.json",
        templates: ".claude/templates",
        context: ".claude/context",
        instructions: "CLAUDE.md",
        config: ".claude/settings.json",
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
            this.paths.hooks,
            this.paths.rules,
            this.paths.templates,
            this.paths.context,
        ];
        for (const dir of dirs) {
            if (dir) {
                const fullPath = join(projectRoot, dir);
                if (!existsSync(fullPath)) {
                    mkdirSync(fullPath, { recursive: true });
                }
            }
        }
        // Create default settings.json if not exists
        const settingsPath = join(projectRoot, this.paths.config);
        if (!existsSync(settingsPath)) {
            writeFileSync(settingsPath, JSON.stringify({}, null, 2));
        }
    }
    async scanArtifacts(projectRoot, options = {}) {
        const artifacts = [];
        const types = options.types ?? ["skill", "agent", "command", "hook", "rule"];
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
                case "hook":
                    artifacts.push(...(await this.scanHooks(projectRoot)));
                    break;
                case "rule":
                    artifacts.push(...(await this.scanRules(projectRoot)));
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
                const refFiles = readdirSync(referencesDir);
                references.push(...refFiles.filter((f) => f.endsWith(".md")));
            }
            // Scan for assets
            const assetsDir = join(skillsDir, skillName, "assets");
            const assets = [];
            if (existsSync(assetsDir)) {
                const assetFiles = await glob("**/*", { cwd: assetsDir });
                assets.push(...assetFiles);
            }
            skills.push({
                id: generateArtifactId(skillName, "skill", "claude"),
                name: skillName,
                type: "skill",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "claude",
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
                id: generateArtifactId(agentName, "agent", "claude"),
                name: agentName,
                type: "agent",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "claude",
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
                id: generateArtifactId(commandName, "command", "claude"),
                name: commandName,
                type: "command",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "claude",
                sourcePath: join(this.paths.commands, commandFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                command: `/${commandName}`,
                aliases: frontmatter.aliases ?? undefined,
            });
        }
        return commands;
    }
    async scanHooks(projectRoot) {
        const hooksDir = join(projectRoot, this.paths.hooks);
        if (!existsSync(hooksDir))
            return [];
        const hookFiles = await glob("*.{sh,js}", { cwd: hooksDir });
        const hooks = [];
        for (const hookFile of hookFiles) {
            const hookPath = join(hooksDir, hookFile);
            const hookName = basename(hookFile).replace(/\.(sh|js)$/, "");
            const content = readFileSync(hookPath, "utf-8");
            const stats = statSync(hookPath);
            // Determine hook type from filename
            let hookType = "pre_tool_use";
            if (hookName.startsWith("pre-")) {
                hookType = "pre_tool_use";
            }
            else if (hookName.startsWith("post-")) {
                hookType = "post_tool_use";
            }
            else if (hookName.includes("session-start")) {
                hookType = "session_start";
            }
            else if (hookName.includes("session-end")) {
                hookType = "session_end";
            }
            hooks.push({
                id: generateArtifactId(hookName, "hook", "claude"),
                name: hookName,
                type: "hook",
                description: undefined,
                content,
                metadata: {},
                sourceSystem: "claude",
                sourcePath: join(this.paths.hooks, hookFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                hookType,
                script: content,
            });
        }
        return hooks;
    }
    async scanRules(projectRoot) {
        const rulesDir = join(projectRoot, this.paths.rules);
        if (!existsSync(rulesDir))
            return [];
        const ruleFiles = await glob("*.md", { cwd: rulesDir });
        const rules = [];
        for (const ruleFile of ruleFiles) {
            const rulePath = join(rulesDir, ruleFile);
            const ruleName = basename(ruleFile, ".md");
            const content = readFileSync(rulePath, "utf-8");
            const stats = statSync(rulePath);
            const { frontmatter } = this.parseYamlFrontmatter(content);
            rules.push({
                id: generateArtifactId(ruleName, "rule", "claude"),
                name: ruleName,
                type: "rule",
                description: frontmatter.description ?? undefined,
                content,
                metadata: frontmatter,
                sourceSystem: "claude",
                sourcePath: join(this.paths.rules, ruleFile),
                checksum: this.generateChecksum(content),
                lastModified: stats.mtime,
                globs: frontmatter.globs ?? undefined,
                alwaysApply: frontmatter.alwaysApply ?? false,
            });
        }
        return rules;
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
        else if (artifactPath.includes("/hooks/")) {
            type = "hook";
        }
        else if (artifactPath.includes("/rules/")) {
            type = "rule";
        }
        return {
            id: generateArtifactId(name, type, "claude"),
            name,
            type,
            description: frontmatter.description ?? undefined,
            content,
            metadata: frontmatter,
            sourceSystem: "claude",
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
        if (options.useSymlink && options.symlinkTarget) {
            // Create symlink
            if (existsSync(fullPath)) {
                unlinkSync(fullPath);
            }
            const relativePath = relative(targetDir, join(projectRoot, options.symlinkTarget));
            symlinkSync(relativePath, fullPath);
        }
        else {
            // Write file
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
            case "hook":
                return join(this.paths.hooks, `${artifact.name}.sh`);
            case "rule":
                return join(this.paths.rules, `${artifact.name}.md`);
            default:
                return join(this.paths.root, artifact.name);
        }
    }
    async readMcpServers(projectRoot) {
        const mcpPath = join(projectRoot, this.paths.mcpServers);
        if (!existsSync(mcpPath))
            return [];
        try {
            const content = readFileSync(mcpPath, "utf-8");
            const config = JSON.parse(content);
            const servers = [];
            for (const [name, serverConfig] of Object.entries(config.mcpServers || {})) {
                const server = serverConfig;
                servers.push({
                    id: generateArtifactId(name, "mcp_server", "claude"),
                    name,
                    type: "mcp_server",
                    content: JSON.stringify(server, null, 2),
                    metadata: {},
                    sourceSystem: "claude",
                    sourcePath: this.paths.mcpServers,
                    checksum: this.generateChecksum(JSON.stringify(server)),
                    lastModified: statSync(mcpPath).mtime,
                    command: server.command ?? [],
                    env: server.env,
                    enabled: server.enabled ?? false,
                });
            }
            return servers;
        }
        catch {
            return [];
        }
    }
    async writeMcpServers(projectRoot, servers) {
        const mcpPath = join(projectRoot, this.paths.mcpServers);
        const config = { mcpServers: {} };
        for (const server of servers) {
            config.mcpServers[server.name] = {
                command: server.command,
                env: server.env,
                enabled: server.enabled,
            };
        }
        writeFileSync(mcpPath, JSON.stringify(config, null, 2));
    }
}
//# sourceMappingURL=claude.js.map