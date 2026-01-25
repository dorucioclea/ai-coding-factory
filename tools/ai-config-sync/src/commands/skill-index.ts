/**
 * Skill Index Command
 *
 * Generate and sync skill index to limited systems.
 */

import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";
import { SystemId } from "../models/types.js";

export interface SkillIndexCommandOptions {
  targets?: string[];
  dryRun?: boolean;
  verbose?: boolean;
}

export async function skillIndexCommand(
  projectRoot: string,
  options: SkillIndexCommandOptions
): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    console.log(chalk.bold("\nSkill Index Sync"));
    console.log(chalk.dim("─".repeat(50)));

    const targets = options.targets?.map((t) => {
      const result = SystemId.safeParse(t);
      if (!result.success) {
        throw new Error(`Invalid system ID: ${t}`);
      }
      return result.data;
    });

    if (targets) {
      console.log(`Targets: ${chalk.cyan(targets.join(", "))}`);
    } else {
      console.log(`Targets: ${chalk.cyan("gemini, aider, continue, cody")} (limited systems)`);
    }

    if (options.dryRun) {
      console.log(chalk.yellow("\n[DRY RUN] No changes will be made"));
    }
    console.log();

    const results = await engine.syncSkillIndex({
      targets,
      dryRun: options.dryRun,
      verbose: options.verbose,
    });

    printResults(results, options.verbose);
  } finally {
    engine.close();
  }
}

function printResults(
  results: {
    synced: number;
    skipped: number;
    failed: number;
    details: Array<{
      target: SystemId;
      status: "success" | "skipped" | "failed";
      message?: string;
    }>;
  },
  verbose?: boolean
): void {
  console.log(chalk.bold("\nSkill Index Sync Summary"));
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
      d.message ?? "-",
    ]);

    console.log(
      table([["Target", "Status", "Message"], ...detailData], {
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
