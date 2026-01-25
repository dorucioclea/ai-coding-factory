/**
 * Sync Command
 *
 * Synchronize configurations from source to target systems.
 */
import chalk from "chalk";
import { table } from "table";
import { SyncEngine } from "../sync-engine.js";
import { SystemId, ArtifactType } from "../models/types.js";
import { getAvailableAdapters } from "../adapters/index.js";
export async function syncCommand(projectRoot, options) {
    const engine = new SyncEngine(projectRoot);
    try {
        // Parse and validate options
        const source = parseSystemId(options.source ?? "claude");
        const targets = parseTargets(options.targets);
        const artifactTypes = parseArtifactTypes(options.types);
        console.log(chalk.bold("\nAI Config Sync"));
        console.log(chalk.dim("─".repeat(50)));
        console.log(`Source:  ${chalk.cyan(source)}`);
        console.log(`Targets: ${chalk.cyan(targets.join(", "))}`);
        if (artifactTypes) {
            console.log(`Types:   ${chalk.cyan(artifactTypes.join(", "))}`);
        }
        if (options.dryRun) {
            console.log(chalk.yellow("\n[DRY RUN] No changes will be made"));
        }
        console.log();
        const syncOptions = {
            source,
            targets,
            artifactTypes,
            dryRun: options.dryRun,
            force: options.force,
            useSymlinks: !options.noSymlinks,
            verbose: options.verbose,
            syncDeletions: options.delete,
        };
        const summary = await engine.sync(syncOptions);
        printSummary(summary, options.verbose);
    }
    finally {
        engine.close();
    }
}
function parseSystemId(value) {
    const result = SystemId.safeParse(value);
    if (!result.success) {
        const available = getAvailableAdapters().join(", ");
        throw new Error(`Invalid system ID: ${value}. Available: ${available}`);
    }
    return result.data;
}
function parseTargets(targets) {
    if (!targets || targets.length === 0) {
        return ["opencode", "cursor", "codex"];
    }
    return targets.map((t) => parseSystemId(t));
}
function parseArtifactTypes(types) {
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
function printSummary(summary, verbose) {
    const { results, details } = summary;
    console.log(chalk.bold("\nSync Summary"));
    console.log(chalk.dim("─".repeat(50)));
    const summaryData = [
        ["Total", results.total.toString()],
        ["Created", chalk.green(results.created.toString())],
        ["Updated", chalk.blue(results.updated.toString())],
        ["Symlinked", chalk.cyan(results.symlinked.toString())],
        ["Deleted", results.deleted > 0 ? chalk.magenta(results.deleted.toString()) : "0"],
        ["Skipped", chalk.yellow(results.skipped.toString())],
        ["Failed", results.failed > 0 ? chalk.red(results.failed.toString()) : "0"],
    ];
    console.log(table(summaryData, {
        header: { content: "Results", alignment: "center" },
        columns: [{ width: 15 }, { width: 10, alignment: "right" }],
    }));
    if (verbose && details.length > 0) {
        console.log(chalk.bold("\nDetails"));
        console.log(chalk.dim("─".repeat(50)));
        const detailData = details.map((d) => [
            d.artifactType,
            d.artifactName,
            d.targetSystem,
            d.operation,
            d.success ? chalk.green("✓") : chalk.red("✗"),
            d.message ?? d.error ?? "",
        ]);
        console.log(table([["Type", "Name", "Target", "Operation", "Status", "Message"], ...detailData], {
            columns: [
                { width: 10 },
                { width: 25 },
                { width: 10 },
                { width: 10 },
                { width: 6, alignment: "center" },
                { width: 30 },
            ],
        }));
    }
    // Show failed items
    const failed = details.filter((d) => !d.success);
    if (failed.length > 0) {
        console.log(chalk.bold.red("\nFailed Operations"));
        console.log(chalk.dim("─".repeat(50)));
        for (const f of failed) {
            console.log(chalk.red(`  ✗ ${f.artifactName} (${f.artifactType})`));
            if (f.error) {
                console.log(chalk.dim(`    ${f.error}`));
            }
        }
    }
    const duration = (summary.completedAt.getTime() - summary.startedAt.getTime()) / 1000;
    console.log(chalk.dim(`\nCompleted in ${duration.toFixed(2)}s`));
    console.log(chalk.dim(`Job ID: ${summary.jobId}`));
}
//# sourceMappingURL=sync.js.map