/**
 * Init Command
 *
 * Initialize AI config sync for a project.
 */

import chalk from "chalk";
import ora from "ora";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { DatabaseManager } from "../database/index.js";
import { getAdapter, getAvailableAdapters } from "../adapters/index.js";
import type { SystemId } from "../models/types.js";

export interface InitCommandOptions {
  systems?: string[];
  force?: boolean;
}

export async function initCommand(
  projectRoot: string,
  options: InitCommandOptions
): Promise<void> {
  console.log(chalk.bold("\nInitializing AI Config Sync"));
  console.log(chalk.dim("─".repeat(50)));
  console.log(`Project: ${chalk.cyan(projectRoot)}`);
  console.log();

  // Check if already initialized
  const dbPath = join(projectRoot, ".ai-config-sync");
  if (existsSync(dbPath) && !options.force) {
    console.log(
      chalk.yellow("Project already initialized. Use --force to reinitialize.")
    );
    return;
  }

  // Initialize database
  const dbSpinner = ora("Creating database...").start();
  const db = new DatabaseManager(projectRoot);
  dbSpinner.succeed("Database created");

  // Determine which systems to initialize
  const systemsToInit: SystemId[] = options.systems
    ? (options.systems as SystemId[])
    : getAvailableAdapters();

  // Initialize each system
  for (const systemId of systemsToInit) {
    const spinner = ora(`Initializing ${systemId}...`).start();

    try {
      const adapter = getAdapter(systemId);

      if (adapter.isConfigured(projectRoot)) {
        spinner.succeed(`${adapter.name} already configured`);
      } else {
        await adapter.initialize(projectRoot);
        spinner.succeed(`${adapter.name} initialized`);
      }
    } catch (error) {
      spinner.fail(`Failed to initialize ${systemId}: ${error}`);
    }
  }

  // Show summary
  console.log(chalk.bold("\nInitialization Complete"));
  console.log(chalk.dim("─".repeat(50)));
  console.log(`Database: ${chalk.cyan(db.getDatabasePath())}`);
  console.log();
  console.log("Next steps:");
  console.log(`  1. Run ${chalk.cyan("acs status")} to see configured systems`);
  console.log(`  2. Run ${chalk.cyan("acs sync")} to sync configurations`);
  console.log(`  3. Run ${chalk.cyan("acs diff")} to see differences`);

  db.close();
}
