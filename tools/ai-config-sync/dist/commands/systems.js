/**
 * Systems Command
 *
 * List and manage supported AI coding systems.
 */
import chalk from "chalk";
import { table } from "table";
import { getAvailableAdapters, getAdapter, isAdapterAvailable } from "../adapters/index.js";
export async function systemsCommand(projectRoot, options) {
    console.log(chalk.bold("\nSupported AI Coding Systems"));
    console.log(chalk.dim("─".repeat(70)));
    const allSystems = [
        "claude",
        "opencode",
        "cursor",
        "codex",
        "windsurf",
        "aider",
        "gemini",
        "continue",
        "cody",
    ];
    const systemData = [];
    for (const systemId of allSystems) {
        const available = isAdapterAvailable(systemId);
        if (!options.all && !available) {
            continue;
        }
        let name = systemId;
        let caps = "";
        let configured = chalk.dim("N/A");
        if (available) {
            const adapter = getAdapter(systemId);
            name = adapter.name;
            configured = adapter.isConfigured(projectRoot)
                ? chalk.green("✓")
                : chalk.dim("✗");
            const capabilities = [];
            if (adapter.capabilities.skills)
                capabilities.push("skills");
            if (adapter.capabilities.agents)
                capabilities.push("agents");
            if (adapter.capabilities.commands)
                capabilities.push("commands");
            if (adapter.capabilities.hooks)
                capabilities.push("hooks");
            if (adapter.capabilities.rules)
                capabilities.push("rules");
            if (adapter.capabilities.mcpServers)
                capabilities.push("mcp");
            caps = capabilities.join(", ");
        }
        systemData.push([
            systemId,
            name,
            available ? chalk.green("✓") : chalk.yellow("○"),
            configured,
            caps || chalk.dim("limited"),
        ]);
    }
    console.log(table([
        ["ID", "Name", "Adapter", "Configured", "Capabilities"],
        ...systemData,
    ], {
        columns: [
            { width: 12 },
            { width: 20 },
            { width: 10, alignment: "center" },
            { width: 12, alignment: "center" },
            { width: 35 },
        ],
    }));
    console.log(chalk.dim("Legend:"));
    console.log(chalk.dim("  Adapter: ✓ = implemented, ○ = planned"));
    console.log(chalk.dim("  Configured: ✓ = initialized, ✗ = not initialized"));
    if (!options.all) {
        console.log(chalk.dim(`\nUse ${chalk.cyan("acs systems --all")} to show all systems`));
    }
    // Show capability matrix
    console.log(chalk.bold("\n\nCapability Matrix"));
    console.log(chalk.dim("─".repeat(70)));
    const availableSystems = getAvailableAdapters();
    const capHeaders = ["System", "Skills", "Agents", "Commands", "Hooks", "Rules", "MCP"];
    const capData = availableSystems.map((systemId) => {
        const adapter = getAdapter(systemId);
        const c = adapter.capabilities;
        return [
            systemId,
            c.skills ? chalk.green("✓") : chalk.dim("✗"),
            c.agents ? chalk.green("✓") : chalk.dim("✗"),
            c.commands ? chalk.green("✓") : chalk.dim("✗"),
            c.hooks ? chalk.green("✓") : chalk.dim("✗"),
            c.rules ? chalk.green("✓") : chalk.dim("✗"),
            c.mcpServers ? chalk.green("✓") : chalk.dim("✗"),
        ];
    });
    console.log(table([capHeaders, ...capData], {
        columns: [
            { width: 12 },
            { width: 8, alignment: "center" },
            { width: 8, alignment: "center" },
            { width: 10, alignment: "center" },
            { width: 8, alignment: "center" },
            { width: 8, alignment: "center" },
            { width: 6, alignment: "center" },
        ],
    }));
    // Show transformation support
    console.log(chalk.bold("\nTransformation Support"));
    console.log(chalk.dim("─".repeat(50)));
    console.log("  claude → opencode: " + chalk.cyan("symlink"));
    console.log("  claude → codex:    " + chalk.cyan("copy"));
    console.log("  claude → cursor:   " + chalk.cyan("skill → .mdc rule"));
    console.log("  claude → windsurf: " + chalk.cyan("skill → markdown rule"));
}
//# sourceMappingURL=systems.js.map