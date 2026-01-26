/**
 * Merge Command
 *
 * Multi-source to multi-target synchronization.
 * Scans multiple sources for artifacts, identifies unique ones,
 * and optionally consolidates to a canonical source before syncing to targets.
 */

import chalk from "chalk";
import { table } from "table";
import { existsSync, mkdirSync, cpSync } from "node:fs";
import { join, dirname } from "node:path";
import { SyncEngine } from "../sync-engine.js";
import { SystemId, ArtifactType, type Artifact } from "../models/types.js";
import { getAdapter, getAvailableAdapters } from "../adapters/index.js";

export interface MergeCommandOptions {
  sources?: string[];
  targets?: string[];
  canonical?: string;
  types?: string[];
  dryRun?: boolean;
  force?: boolean;
  noSymlinks?: boolean;
  verbose?: boolean;
}

interface ArtifactWithSource {
  artifact: Artifact;
  source: SystemId;
}

export async function mergeCommand(
  projectRoot: string,
  options: MergeCommandOptions
): Promise<void> {
  const engine = new SyncEngine(projectRoot);

  try {
    // Parse options
    const sources = parseSources(options.sources);
    const targets = parseTargets(options.targets, sources);
    const canonical = options.canonical ? parseSystemId(options.canonical) : "claude";
    const artifactTypes = parseArtifactTypes(options.types);

    console.log(chalk.bold("\nAI Config Merge"));
    console.log(chalk.dim("─".repeat(60)));
    console.log(`Sources:   ${chalk.cyan(sources.join(", "))}`);
    console.log(`Canonical: ${chalk.green(canonical)}`);
    console.log(`Targets:   ${chalk.cyan(targets.join(", "))}`);
    if (artifactTypes) {
      console.log(`Types:     ${chalk.cyan(artifactTypes.join(", "))}`);
    }
    if (options.dryRun) {
      console.log(chalk.yellow("\n[DRY RUN] No changes will be made"));
    }
    console.log();

    // Step 1: Scan all sources for artifacts
    const artifactsByName = new Map<string, ArtifactWithSource[]>();
    let totalScanned = 0;

    for (const source of sources) {
      const adapter = getAdapter(source);
      if (!adapter.isConfigured(projectRoot)) {
        console.log(chalk.dim(`  Skipping ${source} (not configured)`));
        continue;
      }

      const artifacts = await adapter.scanArtifacts(projectRoot, {
        types: artifactTypes,
      });

      for (const artifact of artifacts) {
        const key = `${artifact.type}:${artifact.name}`;
        if (!artifactsByName.has(key)) {
          artifactsByName.set(key, []);
        }
        artifactsByName.get(key)!.push({ artifact, source });
        totalScanned++;
      }

      console.log(chalk.dim(`  Scanned ${source}: ${artifacts.length} artifacts`));
    }

    console.log(chalk.bold(`\nTotal: ${totalScanned} artifacts, ${artifactsByName.size} unique\n`));

    // Step 2: Identify unique artifacts and conflicts
    const uniqueToSource = new Map<SystemId, Artifact[]>();
    const conflicts: Array<{ name: string; type: ArtifactType; sources: SystemId[] }> = [];
    const allUnique: Artifact[] = [];

    for (const [_key, entries] of artifactsByName) {
      if (entries.length === 1) {
        // Unique to one source
        const { artifact, source } = entries[0];
        if (!uniqueToSource.has(source)) {
          uniqueToSource.set(source, []);
        }
        uniqueToSource.get(source)!.push(artifact);
        allUnique.push(artifact);
      } else {
        // Present in multiple sources - check if they're identical
        const checksums = new Set(entries.map((e) => e.artifact.checksum));
        if (checksums.size > 1) {
          // Different content - this is a conflict
          conflicts.push({
            name: entries[0].artifact.name,
            type: entries[0].artifact.type,
            sources: entries.map((e) => e.source),
          });
        }
        // If identical, no action needed - already synced
      }
    }

    // Report unique artifacts
    if (uniqueToSource.size > 0) {
      console.log(chalk.bold("Unique artifacts by source:"));
      const tableData: string[][] = [];

      for (const [source, artifacts] of uniqueToSource) {
        for (const artifact of artifacts) {
          const inCanonical = source === canonical ? chalk.green("✓") : chalk.yellow("→");
          tableData.push([
            inCanonical,
            source,
            artifact.type,
            artifact.name,
            artifact.description?.slice(0, 40) || "",
          ]);
        }
      }

      console.log(
        table([["", "Source", "Type", "Name", "Description"], ...tableData], {
          columns: [
            { width: 2 },
            { width: 10 },
            { width: 8 },
            { width: 25 },
            { width: 40 },
          ],
        })
      );
    }

    // Report conflicts
    if (conflicts.length > 0) {
      console.log(chalk.bold.yellow("\nConflicts (different content in multiple sources):"));
      for (const conflict of conflicts) {
        console.log(
          chalk.yellow(`  ⚠ ${conflict.type}:${conflict.name} differs in: ${conflict.sources.join(", ")}`)
        );
      }
      console.log(chalk.dim("\n  Conflicts require manual resolution. Use --force to take from first source."));
    }

    // Step 3: Copy unique artifacts to canonical source
    const canonicalAdapter = getAdapter(canonical);
    if (!canonicalAdapter.isConfigured(projectRoot)) {
      await canonicalAdapter.initialize(projectRoot);
    }

    let copiedToCanonical = 0;
    const artifactsToCopy = Array.from(uniqueToSource.entries())
      .filter(([source]) => source !== canonical)
      .flatMap(([source, artifacts]) => artifacts.map((a) => ({ artifact: a, source })));

    if (artifactsToCopy.length > 0) {
      console.log(chalk.bold(`\nCopying ${artifactsToCopy.length} unique artifacts to ${canonical}...`));

      for (const { artifact, source } of artifactsToCopy) {
        const targetPath = canonicalAdapter.getArtifactPath(artifact);
        const fullTargetPath = join(projectRoot, targetPath);

        if (options.dryRun) {
          console.log(chalk.dim(`  Would copy: ${artifact.name} (${artifact.type}) from ${source}`));
          copiedToCanonical++;
          continue;
        }

        try {
          // Create target directory
          const targetDir = dirname(fullTargetPath);
          if (!existsSync(targetDir)) {
            mkdirSync(targetDir, { recursive: true });
          }

          // Get source path
          const sourcePath = join(projectRoot, artifact.sourcePath);

          // Handle different artifact types
          if (artifact.type === "skill") {
            // For skills, copy the entire directory
            const sourceSkillDir = dirname(sourcePath);
            const targetSkillDir = dirname(fullTargetPath);
            if (existsSync(sourceSkillDir)) {
              cpSync(sourceSkillDir, targetSkillDir, { recursive: true });
            }
          } else {
            // For other artifacts, copy the file
            cpSync(sourcePath, fullTargetPath);
          }

          copiedToCanonical++;
          if (options.verbose) {
            console.log(chalk.green(`  ✓ Copied: ${artifact.name} (${artifact.type}) from ${source}`));
          }
        } catch (error) {
          console.log(chalk.red(`  ✗ Failed: ${artifact.name} - ${error}`));
        }
      }

      console.log(chalk.green(`\nCopied ${copiedToCanonical} artifacts to ${canonical}`));
    }

    // Step 4: Sync from canonical to all targets
    const syncTargets = targets.filter((t) => t !== canonical);

    if (syncTargets.length > 0 && !options.dryRun) {
      console.log(chalk.bold(`\nSyncing from ${canonical} to targets: ${syncTargets.join(", ")}...`));

      const summary = await engine.sync({
        source: canonical,
        targets: syncTargets,
        artifactTypes,
        dryRun: options.dryRun,
        force: options.force,
        useSymlinks: !options.noSymlinks,
        verbose: options.verbose,
      });

      console.log(chalk.bold("\nSync Summary"));
      console.log(chalk.dim("─".repeat(40)));
      console.log(`  Created:   ${chalk.green(summary.results.created.toString())}`);
      console.log(`  Updated:   ${chalk.blue(summary.results.updated.toString())}`);
      console.log(`  Symlinked: ${chalk.cyan(summary.results.symlinked.toString())}`);
      console.log(`  Skipped:   ${chalk.yellow(summary.results.skipped.toString())}`);
      console.log(`  Failed:    ${summary.results.failed > 0 ? chalk.red(summary.results.failed.toString()) : "0"}`);
    }

    // Final summary
    console.log(chalk.bold("\n" + "═".repeat(60)));
    console.log(chalk.bold("Merge Complete"));
    console.log(chalk.dim("─".repeat(60)));
    console.log(`  Scanned:     ${totalScanned} artifacts from ${sources.length} sources`);
    console.log(`  Unique:      ${allUnique.length} artifacts`);
    console.log(`  Conflicts:   ${conflicts.length}`);
    console.log(`  To canonical: ${copiedToCanonical}`);
    console.log(`  Targets:     ${syncTargets.length} systems`);
    console.log();

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

function parseSources(sources?: string[]): SystemId[] {
  if (!sources || sources.length === 0) {
    // Default: scan claude, opencode, and codex
    return ["claude", "opencode", "codex"] as SystemId[];
  }
  return sources.map((s) => parseSystemId(s));
}

function parseTargets(targets?: string[], sources?: SystemId[]): SystemId[] {
  if (!targets || targets.length === 0) {
    // Default: all sources plus common targets
    const defaultTargets = new Set<SystemId>([
      ...(sources || []),
      "opencode" as SystemId,
      "cursor" as SystemId,
      "codex" as SystemId,
    ]);
    return Array.from(defaultTargets);
  }
  return targets.map((t) => parseSystemId(t));
}

function parseArtifactTypes(types?: string[]): ArtifactType[] | undefined {
  if (!types || types.length === 0) {
    return undefined;
  }

  return types.map((t) => {
    const result = ArtifactType.safeParse(t);
    if (!result.success) {
      throw new Error(`Invalid artifact type: ${t}`);
    }
    return result.data;
  });
}
