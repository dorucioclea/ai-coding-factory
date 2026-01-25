/**
 * Status Command
 *
 * Show status of all configured AI coding assistants.
 */

import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";

export async function statusCommand(projectRoot: string): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    const status = await engine.status();

    console.log(chalk.bold("\nAI Config Sync Status"));
    console.log(chalk.dim("─".repeat(60)));
    console.log(`Project: ${chalk.cyan(projectRoot)}`);
    if (status.lastSync) {
      console.log(`Last sync: ${chalk.cyan(status.lastSync)}`);
    }
    console.log();

    // Systems table
    const systemData = status.systems.map((s) => {
      const counts = Object.entries(s.artifactCounts)
        .map(([type, count]) => `${type}: ${count}`)
        .join(", ");

      return [
        s.id,
        s.name,
        s.configured ? chalk.green("✓") : chalk.red("✗"),
        counts || chalk.dim("none"),
      ];
    });

    console.log(chalk.bold("Configured Systems"));
    console.log(
      table([["ID", "Name", "Status", "Artifacts"], ...systemData], {
        columns: [
          { width: 12 },
          { width: 18 },
          { width: 8, alignment: "center" },
          { width: 35 },
        ],
      })
    );

    // Database stats
    console.log(chalk.bold("Database Statistics"));
    console.log(chalk.dim("─".repeat(40)));
    console.log(`  Systems:       ${status.stats.systems}`);
    console.log(`  Artifacts:     ${status.stats.artifacts}`);
    console.log(`  Sync States:   ${status.stats.syncStates}`);
    console.log(`  Sync Jobs:     ${status.stats.syncJobs}`);
    console.log(`  Mapping Rules: ${status.stats.mappingRules}`);

    // Recent history
    const history = engine.getHistory(5);
    if (history.length > 0) {
      console.log(chalk.bold("\nRecent Sync History"));
      console.log(chalk.dim("─".repeat(60)));

      const historyData = history.map((h) => [
        h.id.slice(0, 12),
        h.source,
        h.targets.join(", "),
        h.status === "completed"
          ? chalk.green(h.status)
          : h.status === "failed"
            ? chalk.red(h.status)
            : chalk.yellow(h.status),
        h.startedAt.slice(0, 19),
      ]);

      console.log(
        table([["Job ID", "Source", "Targets", "Status", "Started"], ...historyData], {
          columns: [
            { width: 14 },
            { width: 10 },
            { width: 20 },
            { width: 12 },
            { width: 20 },
          ],
        })
      );
    }
  } finally {
    engine.close();
  }
}
