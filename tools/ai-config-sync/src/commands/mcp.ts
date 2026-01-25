/**
 * MCP Sync Command
 *
 * Synchronize MCP server configurations between systems.
 */

import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";
import { SystemId } from "../models/types.js";
import { getAvailableAdapters } from "../adapters/index.js";

export interface McpSyncCommandOptions {
  source?: string;
  targets?: string[];
  dryRun?: boolean;
  verbose?: boolean;
}

export async function mcpSyncCommand(
  projectRoot: string,
  options: McpSyncCommandOptions
): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    // Parse and validate options
    const source = parseSystemId(options.source ?? "claude");
    const targets = parseTargets(options.targets);

    console.log(chalk.bold("\nMCP Server Sync"));
    console.log(chalk.dim("─".repeat(50)));
    console.log(`Source:  ${chalk.cyan(source)}`);
    console.log(`Targets: ${chalk.cyan(targets.join(", "))}`);
    if (options.dryRun) {
      console.log(chalk.yellow("\n[DRY RUN] No changes will be made"));
    }
    console.log();

    const results = await engine.syncMcpServers({
      source,
      targets,
      dryRun: options.dryRun,
      verbose: options.verbose,
    });

    printResults(results, options.verbose);
  } finally {
    engine.close();
  }
}

function parseSystemId(value: string): SystemId {
  const result = SystemId.safeParse(value);
  if (!result.success) {
    const available = getAvailableAdapters().join(", ");
    throw new Error(`Invalid system ID: ${value}. Available: ${available}`);
  }
  return result.data;
}

function parseTargets(targets?: string[]): SystemId[] {
  if (!targets || targets.length === 0) {
    // Default to all MCP-capable systems
    return ["opencode", "cursor", "codex", "windsurf"] as SystemId[];
  }

  return targets.map((t) => parseSystemId(t));
}

function printResults(
  results: {
    synced: number;
    skipped: number;
    failed: number;
    details: Array<{
      target: SystemId;
      servers: string[];
      status: "success" | "skipped" | "failed";
      message?: string;
    }>;
  },
  verbose?: boolean
): void {
  console.log(chalk.bold("\nMCP Sync Summary"));
  console.log(chalk.dim("─".repeat(50)));

  const summaryData = [
    ["Synced", chalk.green(results.synced.toString())],
    ["Skipped", chalk.yellow(results.skipped.toString())],
    ["Failed", results.failed > 0 ? chalk.red(results.failed.toString()) : "0"],
  ];

  console.log(
    table(summaryData, {
      header: { content: "Results", alignment: "center" },
      columns: [{ width: 15 }, { width: 10, alignment: "right" }],
    })
  );

  if (verbose || results.details.length > 0) {
    console.log(chalk.bold("\nDetails by Target"));
    console.log(chalk.dim("─".repeat(50)));

    const detailData = results.details.map((d) => [
      d.target,
      d.status === "success"
        ? chalk.green("✓")
        : d.status === "skipped"
        ? chalk.yellow("○")
        : chalk.red("✗"),
      d.servers.length > 0
        ? d.servers.slice(0, 5).join(", ") + (d.servers.length > 5 ? ` +${d.servers.length - 5} more` : "")
        : d.message ?? "-",
    ]);

    console.log(
      table([["Target", "Status", "Servers / Message"], ...detailData], {
        columns: [
          { width: 12 },
          { width: 8, alignment: "center" },
          { width: 50 },
        ],
      })
    );
  }

  // Show failed items
  const failed = results.details.filter((d) => d.status === "failed");
  if (failed.length > 0) {
    console.log(chalk.bold.red("\nFailed Targets"));
    console.log(chalk.dim("─".repeat(50)));

    for (const f of failed) {
      console.log(chalk.red(`  ✗ ${f.target}`));
      if (f.message) {
        console.log(chalk.dim(`    ${f.message}`));
      }
    }
  }
}
