/**
 * Diff Command
 *
 * Show differences between source and target systems.
 */

import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";
import { SystemId, ArtifactType } from "../models/types.js";
import { getAvailableAdapters } from "../adapters/index.js";

export interface DiffCommandOptions {
  source?: string;
  target?: string;
  type?: string;
}

export async function diffCommand(
  projectRoot: string,
  options: DiffCommandOptions
): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    const source = parseSystemId(options.source ?? "claude");
    const target = parseSystemId(options.target ?? "codex");
    const artifactType = options.type ? parseArtifactType(options.type) : undefined;

    console.log(chalk.bold("\nConfiguration Diff"));
    console.log(chalk.dim("─".repeat(60)));
    console.log(`Source: ${chalk.cyan(source)}`);
    console.log(`Target: ${chalk.cyan(target)}`);
    if (artifactType) {
      console.log(`Type:   ${chalk.cyan(artifactType)}`);
    }
    console.log();

    const diffs = await engine.diff(source, target, artifactType);

    if (diffs.length === 0) {
      console.log(chalk.green("✓ Systems are in sync"));
      return;
    }

    // Group by status
    const added = diffs.filter((d) => d.status === "missing");
    const modified = diffs.filter((d) => d.status === "modified");
    const unchanged = diffs.filter((d) => d.status === "unchanged");

    // Summary
    console.log(chalk.bold("Summary"));
    console.log(chalk.dim("─".repeat(40)));
    console.log(`  ${chalk.green(`+ ${added.length} new`)}`);
    console.log(`  ${chalk.yellow(`~ ${modified.length} modified`)}`);
    console.log(`  ${chalk.dim(`= ${unchanged.length} unchanged`)}`);
    console.log();

    // Details for new/modified
    const changes = [...added, ...modified];
    if (changes.length > 0) {
      console.log(chalk.bold("Changes to Apply"));

      const changeData = changes.map((d) => [
        d.status === "missing" ? chalk.green("+") : chalk.yellow("~"),
        d.artifactType,
        d.artifactName,
        d.status === "missing" ? chalk.green("new") : chalk.yellow("modified"),
        d.sourcePath?.slice(0, 40) ?? "",
      ]);

      console.log(
        table(
          [["", "Type", "Name", "Status", "Source Path"], ...changeData],
          {
            columns: [
              { width: 3 },
              { width: 10 },
              { width: 25 },
              { width: 10 },
              { width: 40 },
            ],
          }
        )
      );

      console.log(
        chalk.dim(
          `\nRun ${chalk.cyan(`acs sync --source ${source} --target ${target}`)} to apply changes`
        )
      );
    }
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

function parseArtifactType(value: string): ArtifactType {
  const result = ArtifactType.safeParse(value);
  if (!result.success) {
    throw new Error(`Invalid artifact type: ${value}`);
  }
  return result.data;
}
