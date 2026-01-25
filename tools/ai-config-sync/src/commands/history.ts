/**
 * History Command
 *
 * Show sync history and job details.
 */

import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";

export interface HistoryCommandOptions {
  limit?: number;
  job?: string;
}

export async function historyCommand(
  projectRoot: string,
  options: HistoryCommandOptions
): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    if (options.job) {
      // Show details for specific job
      const results = engine.getJobResults(options.job);

      if (results.length === 0) {
        console.log(chalk.yellow(`No results found for job: ${options.job}`));
        return;
      }

      console.log(chalk.bold(`\nJob Details: ${options.job}`));
      console.log(chalk.dim("─".repeat(70)));

      const resultData = results.map((r) => [
        r.artifactType,
        r.artifactName,
        r.targetSystem,
        r.operation,
        r.success ? chalk.green("✓") : chalk.red("✗"),
        (r.message ?? r.error ?? "").slice(0, 30),
      ]);

      console.log(
        table(
          [
            ["Type", "Name", "Target", "Operation", "Status", "Message"],
            ...resultData,
          ],
          {
            columns: [
              { width: 10 },
              { width: 20 },
              { width: 10 },
              { width: 10 },
              { width: 8, alignment: "center" },
              { width: 32 },
            ],
          }
        )
      );

      // Summary
      const total = results.length;
      const succeeded = results.filter((r) => r.success).length;
      const failed = total - succeeded;

      console.log(chalk.dim("─".repeat(40)));
      console.log(`Total: ${total} | Succeeded: ${chalk.green(succeeded)} | Failed: ${chalk.red(failed)}`);
    } else {
      // Show job history
      const limit = options.limit ?? 20;
      const history = engine.getHistory(limit);

      if (history.length === 0) {
        console.log(chalk.yellow("\nNo sync history found"));
        return;
      }

      console.log(chalk.bold("\nSync History"));
      console.log(chalk.dim("─".repeat(80)));

      const historyData = history.map((h) => {
        const summary = h.summary as Record<string, number> | undefined;
        const stats = summary
          ? `${summary.created ?? 0}+ ${summary.updated ?? 0}~ ${summary.failed ?? 0}!`
          : "";

        return [
          h.id,
          h.source,
          h.targets.join(", "),
          h.status === "completed"
            ? chalk.green(h.status)
            : h.status === "failed"
              ? chalk.red(h.status)
              : chalk.yellow(h.status),
          stats,
          h.startedAt.slice(0, 19),
        ];
      });

      console.log(
        table(
          [
            ["Job ID", "Source", "Targets", "Status", "Results", "Started"],
            ...historyData,
          ],
          {
            columns: [
              { width: 22 },
              { width: 10 },
              { width: 20 },
              { width: 12 },
              { width: 15 },
              { width: 20 },
            ],
          }
        )
      );

      console.log(
        chalk.dim(
          `\nUse ${chalk.cyan("acs history --job <job-id>")} to see job details`
        )
      );
    }
  } finally {
    engine.close();
  }
}
