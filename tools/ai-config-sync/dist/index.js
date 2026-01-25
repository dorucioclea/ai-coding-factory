#!/usr/bin/env node
/**
 * AI Config Sync CLI
 *
 * Synchronize AI coding assistant configurations across multiple systems.
 *
 * Usage:
 *   acs sync [options]         Sync configurations between systems
 *   acs status                 Show status of all systems
 *   acs diff [options]         Show differences between systems
 *   acs init [options]         Initialize AI config sync
 *   acs history [options]      Show sync history
 *   acs systems [options]      List supported systems
 */
import { Command } from "commander";
import chalk from "chalk";
import { resolve } from "node:path";
import { existsSync } from "node:fs";
import { syncCommand, statusCommand, diffCommand, initCommand, historyCommand, systemsCommand, } from "./commands/index.js";
const program = new Command();
program
    .name("ai-config-sync")
    .alias("acs")
    .description("Synchronize AI coding assistant configurations across Claude, OpenCode, Cursor, Codex, and Windsurf")
    .version("1.0.0")
    .option("-p, --project <path>", "Project root directory", process.cwd());
// Sync command
program
    .command("sync")
    .description("Sync configurations from source to target systems")
    .option("-s, --source <system>", "Source system (default: claude)")
    .option("-t, --targets <systems...>", "Target systems (default: opencode, cursor, codex)")
    .option("--types <types...>", "Artifact types to sync (skill, agent, command, hook, rule)")
    .option("-n, --dry-run", "Show what would be synced without making changes")
    .option("-f, --force", "Force sync even if already up to date")
    .option("-d, --delete", "Delete artifacts from targets that no longer exist in source")
    .option("--no-symlinks", "Copy files instead of creating symlinks")
    .option("-v, --verbose", "Show detailed output")
    .action(async (options) => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await syncCommand(projectRoot, options);
});
// Status command
program
    .command("status")
    .description("Show status of all configured AI systems")
    .action(async () => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await statusCommand(projectRoot);
});
// Diff command
program
    .command("diff")
    .description("Show differences between source and target systems")
    .option("-s, --source <system>", "Source system (default: claude)")
    .option("-t, --target <system>", "Target system (default: codex)")
    .option("--type <type>", "Artifact type to compare")
    .action(async (options) => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await diffCommand(projectRoot, options);
});
// Init command
program
    .command("init")
    .description("Initialize AI config sync for a project")
    .option("--systems <systems...>", "Systems to initialize (default: all available)")
    .option("-f, --force", "Force reinitialization")
    .action(async (options) => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await initCommand(projectRoot, options);
});
// History command
program
    .command("history")
    .description("Show sync history")
    .option("-l, --limit <number>", "Number of jobs to show", "20")
    .option("-j, --job <id>", "Show details for specific job")
    .action(async (options) => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await historyCommand(projectRoot, {
        limit: parseInt(options.limit, 10),
        job: options.job,
    });
});
// Systems command
program
    .command("systems")
    .description("List supported AI coding systems")
    .option("-a, --all", "Show all systems including unimplemented")
    .action(async (options) => {
    const projectRoot = resolveProjectRoot(program.opts().project);
    await systemsCommand(projectRoot, options);
});
// Parse and execute
program.parse();
/**
 * Resolve and validate project root
 */
function resolveProjectRoot(path) {
    const resolved = resolve(path);
    // Check for common AI config directories to validate it's a valid project
    const indicators = [
        ".claude",
        ".opencode",
        ".cursor",
        ".codex",
        "CLAUDE.md",
        "AGENTS.md",
        ".git",
    ];
    const hasIndicator = indicators.some((i) => existsSync(resolve(resolved, i)));
    if (!hasIndicator) {
        console.log(chalk.yellow(`\nWarning: ${resolved} does not appear to be an AI-configured project.`));
        console.log(chalk.dim(`Consider running ${chalk.cyan("acs init")} to initialize the project.\n`));
    }
    return resolved;
}
//# sourceMappingURL=index.js.map