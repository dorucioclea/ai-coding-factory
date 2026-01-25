/**
 * Sync Engine
 *
 * Core synchronization logic for transferring configurations
 * between AI coding assistants.
 */
import { existsSync, cpSync, mkdirSync, symlinkSync, unlinkSync, lstatSync, rmSync, readlinkSync } from "node:fs";
import { join, dirname, relative } from "node:path";
import ora from "ora";
import { DatabaseManager } from "./database/index.js";
import { getAdapter, getAvailableAdapters, } from "./adapters/index.js";
import { generateJobId } from "./utils/helpers.js";
/**
 * Check if a path is a symlink pointing to a source within .claude/
 */
function isClaudeSymlink(projectRoot, targetPath) {
    try {
        const fullPath = join(projectRoot, targetPath);
        const stats = lstatSync(fullPath);
        if (!stats.isSymbolicLink())
            return false;
        const linkTarget = readlinkSync(fullPath);
        // Check if the symlink points to something in .claude/
        return linkTarget.includes(".claude") || linkTarget.includes("claude");
    }
    catch {
        return false;
    }
}
/**
 * Check if a directory was created by this sync tool (has sync state in DB)
 */
function isSyncedDirectory(db, targetSystem, targetPath) {
    const syncStates = db.getSyncStatesForTarget(targetSystem);
    return syncStates.some(state => state.target_path === targetPath ||
        state.target_path?.startsWith(targetPath + "/"));
}
export class SyncEngine {
    db;
    projectRoot;
    adapters = new Map();
    constructor(projectRoot) {
        this.projectRoot = projectRoot;
        this.db = new DatabaseManager(projectRoot);
    }
    /**
     * Get or create adapter for a system
     */
    getSystemAdapter(systemId) {
        if (!this.adapters.has(systemId)) {
            this.adapters.set(systemId, getAdapter(systemId));
        }
        return this.adapters.get(systemId);
    }
    /**
     * Execute a full sync operation
     */
    async sync(options) {
        const jobId = generateJobId();
        const startedAt = new Date();
        const results = [];
        const summary = {
            total: 0,
            created: 0,
            updated: 0,
            skipped: 0,
            failed: 0,
            symlinked: 0,
            deleted: 0,
        };
        // Create sync job record
        this.db.createSyncJob({
            id: jobId,
            source_system: options.source,
            target_systems: JSON.stringify(options.targets),
            artifact_types: JSON.stringify(options.artifactTypes ?? ["skill", "agent", "command", "rule"]),
            dry_run: options.dryRun ? 1 : 0,
            force: options.force ? 1 : 0,
            use_symlinks: options.useSymlinks !== false ? 1 : 0,
            summary: null,
        });
        const spinner = ora({
            text: `Scanning ${options.source} for artifacts...`,
            color: "cyan",
        }).start();
        try {
            // Get source adapter and scan artifacts
            const sourceAdapter = this.getSystemAdapter(options.source);
            if (!sourceAdapter.isConfigured(this.projectRoot)) {
                spinner.fail(`Source system ${options.source} is not configured`);
                throw new Error(`Source system ${options.source} is not configured`);
            }
            const artifacts = await sourceAdapter.scanArtifacts(this.projectRoot, {
                types: options.artifactTypes,
            });
            spinner.succeed(`Found ${artifacts.length} artifacts in ${options.source}`);
            // Update artifacts in database
            for (const artifact of artifacts) {
                this.db.upsertArtifact({
                    id: artifact.id,
                    name: artifact.name,
                    type: artifact.type,
                    description: artifact.description ?? null,
                    content_hash: artifact.checksum,
                    source_system: artifact.sourceSystem,
                    source_path: artifact.sourcePath,
                    metadata: artifact.metadata ? JSON.stringify(artifact.metadata) : null,
                });
            }
            // Sync to each target
            for (const target of options.targets) {
                const targetSpinner = ora({
                    text: `Syncing to ${target}...`,
                    color: "blue",
                }).start();
                try {
                    const targetAdapter = this.getSystemAdapter(target);
                    // Initialize target if needed
                    if (!targetAdapter.isConfigured(this.projectRoot)) {
                        await targetAdapter.initialize(this.projectRoot);
                    }
                    // Get mapping rules for this source-target pair
                    const mappingRules = this.db.getMappingRules(options.source, target);
                    // Sync each artifact
                    for (const artifact of artifacts) {
                        summary.total++;
                        const result = await this.syncArtifact(artifact, sourceAdapter, targetAdapter, mappingRules, options);
                        results.push(result);
                        // Update summary
                        switch (result.operation) {
                            case "create":
                                summary.created++;
                                break;
                            case "update":
                                summary.updated++;
                                break;
                            case "skip":
                                summary.skipped++;
                                break;
                            case "symlink":
                                summary.symlinked++;
                                break;
                            default:
                                if (!result.success) {
                                    summary.failed++;
                                }
                        }
                        // Record result in database
                        if (!options.dryRun) {
                            this.db.addSyncResult({
                                job_id: jobId,
                                artifactId: result.artifactId,
                                artifactName: result.artifactName,
                                artifactType: result.artifactType,
                                sourceSystem: result.sourceSystem,
                                targetSystem: result.targetSystem,
                                operation: result.operation,
                                success: result.success,
                                message: result.message,
                                error: result.error,
                                sourcePath: result.sourcePath,
                                targetPath: result.targetPath,
                            });
                        }
                    }
                    // Handle deletions: remove artifacts from target that no longer exist in source
                    if (options.syncDeletions) {
                        const deletionResults = await this.syncDeletions(options.source, target, artifacts, targetAdapter, options);
                        for (const result of deletionResults) {
                            results.push(result);
                            summary.total++;
                            if (result.operation === "delete" && result.success) {
                                summary.deleted++;
                            }
                            else if (!result.success) {
                                summary.failed++;
                            }
                            // Record deletion in database
                            if (!options.dryRun) {
                                this.db.addSyncResult({
                                    job_id: jobId,
                                    artifactId: result.artifactId,
                                    artifactName: result.artifactName,
                                    artifactType: result.artifactType,
                                    sourceSystem: result.sourceSystem,
                                    targetSystem: result.targetSystem,
                                    operation: result.operation,
                                    success: result.success,
                                    message: result.message,
                                    error: result.error,
                                    sourcePath: result.sourcePath,
                                    targetPath: result.targetPath,
                                });
                            }
                        }
                    }
                    targetSpinner.succeed(`Synced ${artifacts.length} artifacts to ${target}${summary.deleted > 0 ? ` (${summary.deleted} deleted)` : ""}`);
                }
                catch (error) {
                    targetSpinner.fail(`Failed to sync to ${target}: ${error}`);
                    summary.failed += artifacts.length;
                }
            }
            const completedAt = new Date();
            // Update job status
            if (!options.dryRun) {
                this.db.updateSyncJob(jobId, "completed", summary);
                this.db.setSetting("last_sync", completedAt.toISOString());
            }
            return {
                jobId,
                source: options.source,
                targets: options.targets,
                startedAt,
                completedAt,
                results: summary,
                details: results,
            };
        }
        catch (error) {
            spinner.fail(`Sync failed: ${error}`);
            this.db.updateSyncJob(jobId, "failed", { error: String(error) });
            throw error;
        }
    }
    /**
     * Sync a single artifact
     */
    async syncArtifact(artifact, _sourceAdapter, targetAdapter, mappingRules, options) {
        const targetSystem = targetAdapter.systemId;
        const baseResult = {
            artifactId: artifact.id,
            artifactName: artifact.name,
            artifactType: artifact.type,
            sourceSystem: options.source,
            targetSystem,
            sourcePath: artifact.sourcePath,
            timestamp: new Date(),
        };
        try {
            // Check if target supports this artifact type
            const capabilities = targetAdapter.capabilities;
            const typeCapability = `${artifact.type}s`;
            if (!capabilities[typeCapability]) {
                // Need to transform the artifact
                if ((artifact.type === "skill" || artifact.type === "agent") && capabilities.rules) {
                    // Transform skill or agent to rule
                    const transformedArtifact = targetAdapter.transformArtifact(artifact);
                    return await this.writeArtifact(transformedArtifact, targetAdapter, mappingRules, options, baseResult, undefined, // no symlink for transformed artifacts
                    artifact.id // pass original artifact ID for sync state tracking
                    );
                }
                else if (artifact.type === "command" && capabilities.rules) {
                    // Transform command to rule (as documentation)
                    const transformedArtifact = targetAdapter.transformArtifact(artifact);
                    if (transformedArtifact.type === "rule") {
                        return await this.writeArtifact(transformedArtifact, targetAdapter, mappingRules, options, baseResult, undefined, // no symlink for transformed artifacts
                        artifact.id // pass original artifact ID for sync state tracking
                        );
                    }
                    // If no transformation available, skip
                    return {
                        ...baseResult,
                        operation: "skip",
                        success: true,
                        message: `${targetSystem} does not support ${artifact.type} artifacts`,
                    };
                }
                else {
                    return {
                        ...baseResult,
                        operation: "skip",
                        success: true,
                        message: `${targetSystem} does not support ${artifact.type} artifacts`,
                    };
                }
            }
            // Find applicable mapping rule
            const rule = mappingRules.find((r) => r.artifact_type === artifact.type);
            const useSymlink = rule?.use_symlink === 1 && options.useSymlinks !== false;
            // Check existing sync state
            const syncState = this.db.getSyncState(artifact.id, targetSystem);
            if (syncState && !options.force) {
                // Check if artifact has changed
                if (syncState.synced_hash === artifact.checksum) {
                    return {
                        ...baseResult,
                        operation: "skip",
                        success: true,
                        message: "Already up to date",
                        targetPath: syncState.target_path ?? undefined,
                    };
                }
            }
            // Transform artifact if needed
            let finalArtifact = artifact;
            const wasTransformed = rule?.transform_type && rule.transform_type !== "none";
            if (wasTransformed && rule?.transform_type) {
                finalArtifact = targetAdapter.transformArtifact(artifact, {
                    sourceFormat: "markdown",
                    targetFormat: rule.transform_type,
                });
            }
            return await this.writeArtifact(finalArtifact, targetAdapter, mappingRules, options, baseResult, useSymlink ? artifact.sourcePath : undefined, wasTransformed ? artifact.id : undefined // pass original ID if transformed
            );
        }
        catch (error) {
            return {
                ...baseResult,
                operation: "skip",
                success: false,
                error: String(error),
            };
        }
    }
    /**
     * Write artifact to target system
     * @param artifact - The artifact to write (may be transformed)
     * @param targetAdapter - The target system adapter
     * @param _mappingRules - Mapping rules (unused but kept for API consistency)
     * @param options - Sync options
     * @param baseResult - Base result containing original artifact info
     * @param symlinkSource - Source path for symlink (optional)
     * @param originalArtifactId - Original artifact ID for sync state tracking (uses artifact.id if not provided)
     */
    async writeArtifact(artifact, targetAdapter, _mappingRules, options, baseResult, symlinkSource, originalArtifactId) {
        // Use original artifact ID for sync state tracking (important for transformed artifacts)
        const syncStateArtifactId = originalArtifactId ?? artifact.id;
        const targetPath = targetAdapter.getArtifactPath(artifact);
        if (options.dryRun) {
            return {
                ...baseResult,
                operation: symlinkSource ? "symlink" : "create",
                success: true,
                message: `Would ${symlinkSource ? "symlink" : "write"} to ${targetPath}`,
                targetPath,
            };
        }
        try {
            if (symlinkSource && options.useSymlinks !== false) {
                // Handle symlink creation for skills
                const fullTargetPath = join(this.projectRoot, targetPath);
                const targetDir = dirname(fullTargetPath);
                if (!existsSync(targetDir)) {
                    mkdirSync(targetDir, { recursive: true });
                }
                // Check if target already exists
                try {
                    const stats = lstatSync(fullTargetPath);
                    if (stats.isSymbolicLink()) {
                        // Safe to remove symlinks - they're just pointers
                        unlinkSync(fullTargetPath);
                    }
                    else if (stats.isFile()) {
                        // Safe to remove single files
                        unlinkSync(fullTargetPath);
                    }
                    else if (stats.isDirectory()) {
                        // Check if this directory was created by previous sync (safe to replace)
                        // or if it's native content (needs --force to replace)
                        const wasSynced = isSyncedDirectory(this.db, targetAdapter.systemId, targetPath);
                        const isSymlinked = isClaudeSymlink(this.projectRoot, targetPath);
                        if (isSymlinked || wasSynced || options.force) {
                            // Safe to replace: it's a symlink, was synced before, or user requested --force
                            rmSync(fullTargetPath, { recursive: true, force: true });
                        }
                        else {
                            // This is native content - don't delete without --force
                            return {
                                ...baseResult,
                                operation: "skip",
                                success: false,
                                error: `Target directory contains native content: ${fullTargetPath}. Use --force to overwrite.`,
                                targetPath,
                            };
                        }
                    }
                }
                catch {
                    // File doesn't exist, which is fine
                }
                // For skills, symlink the directory
                if (artifact.type === "skill") {
                    const sourceSkillDir = dirname(join(this.projectRoot, symlinkSource));
                    const relativePath = relative(targetDir, sourceSkillDir);
                    symlinkSync(relativePath, fullTargetPath);
                }
                else {
                    const sourcePath = join(this.projectRoot, symlinkSource);
                    const relativePath = relative(targetDir, sourcePath);
                    symlinkSync(relativePath, fullTargetPath);
                }
                // Update sync state using original artifact ID
                this.db.upsertSyncState({
                    artifact_id: syncStateArtifactId,
                    target_system: targetAdapter.systemId,
                    target_path: targetPath,
                    sync_method: "symlink",
                    synced_hash: artifact.checksum,
                    last_synced_at: new Date().toISOString(),
                    status: "synced",
                    error_message: null,
                });
                return {
                    ...baseResult,
                    operation: "symlink",
                    success: true,
                    message: `Symlinked to ${targetPath}`,
                    targetPath,
                };
            }
            else {
                // Copy with full directory structure for skills
                if (artifact.type === "skill" && symlinkSource) {
                    const sourceSkillDir = dirname(join(this.projectRoot, symlinkSource));
                    const targetSkillDir = dirname(join(this.projectRoot, targetPath));
                    // Copy entire skill directory
                    if (existsSync(sourceSkillDir)) {
                        cpSync(sourceSkillDir, targetSkillDir, { recursive: true });
                    }
                }
                else {
                    await targetAdapter.writeArtifact(this.projectRoot, artifact, {
                        overwrite: true,
                        createDirectories: true,
                        symlinkTarget: symlinkSource,
                    });
                }
                // Update sync state using original artifact ID
                this.db.upsertSyncState({
                    artifact_id: syncStateArtifactId,
                    target_system: targetAdapter.systemId,
                    target_path: targetPath,
                    sync_method: "copy",
                    synced_hash: artifact.checksum,
                    last_synced_at: new Date().toISOString(),
                    status: "synced",
                    error_message: null,
                });
                const syncState = this.db.getSyncState(syncStateArtifactId, targetAdapter.systemId);
                const isUpdate = syncState && syncState.synced_hash;
                return {
                    ...baseResult,
                    operation: isUpdate ? "update" : "create",
                    success: true,
                    message: `${isUpdate ? "Updated" : "Created"} ${targetPath}`,
                    targetPath,
                };
            }
        }
        catch (error) {
            // Update sync state with error using original artifact ID
            this.db.upsertSyncState({
                artifact_id: syncStateArtifactId,
                target_system: targetAdapter.systemId,
                target_path: targetPath,
                sync_method: symlinkSource ? "symlink" : "copy",
                synced_hash: null,
                last_synced_at: new Date().toISOString(),
                status: "failed",
                error_message: String(error),
            });
            return {
                ...baseResult,
                operation: "skip",
                success: false,
                error: String(error),
                targetPath,
            };
        }
    }
    /**
     * Sync deletions: remove artifacts from target that no longer exist in source
     */
    async syncDeletions(sourceSystem, targetSystem, currentSourceArtifacts, targetAdapter, options) {
        const results = [];
        // Get all previously synced artifacts for this source→target pair
        const syncStates = this.db.getSyncStatesForTarget(targetSystem);
        const currentSourceIds = new Set(currentSourceArtifacts.map(a => a.id));
        for (const syncState of syncStates) {
            // Check if this artifact still exists in source
            const artifact = this.db.getArtifact(syncState.artifact_id);
            if (!artifact)
                continue;
            // Only process artifacts from our source system
            if (artifact.source_system !== sourceSystem)
                continue;
            // Check if artifact type matches filter
            if (options.artifactTypes && !options.artifactTypes.includes(artifact.type)) {
                continue;
            }
            // If artifact no longer exists in current source scan, it was deleted
            if (!currentSourceIds.has(syncState.artifact_id)) {
                const baseResult = {
                    artifactId: syncState.artifact_id,
                    artifactName: artifact.name,
                    artifactType: artifact.type,
                    sourceSystem,
                    targetSystem,
                    sourcePath: artifact.source_path,
                    timestamp: new Date(),
                };
                if (options.dryRun) {
                    results.push({
                        ...baseResult,
                        operation: "delete",
                        success: true,
                        message: `Would delete ${syncState.target_path}`,
                        targetPath: syncState.target_path ?? undefined,
                    });
                }
                else {
                    try {
                        // Delete from target
                        if (syncState.target_path) {
                            await targetAdapter.deleteArtifact(this.projectRoot, syncState.target_path);
                        }
                        // Remove sync state
                        this.db.deleteSyncState(syncState.artifact_id, targetSystem);
                        results.push({
                            ...baseResult,
                            operation: "delete",
                            success: true,
                            message: `Deleted ${syncState.target_path}`,
                            targetPath: syncState.target_path ?? undefined,
                        });
                    }
                    catch (error) {
                        results.push({
                            ...baseResult,
                            operation: "delete",
                            success: false,
                            error: String(error),
                            targetPath: syncState.target_path ?? undefined,
                        });
                    }
                }
            }
        }
        return results;
    }
    /**
     * Get diff between source and target systems
     */
    async diff(source, target, artifactType) {
        const sourceAdapter = this.getSystemAdapter(source);
        // Ensure target adapter is initialized (validates system exists)
        this.getSystemAdapter(target);
        // Scan source artifacts
        const sourceArtifacts = await sourceAdapter.scanArtifacts(this.projectRoot, {
            types: artifactType ? [artifactType] : undefined,
        });
        // Update database with current source state
        for (const artifact of sourceArtifacts) {
            this.db.upsertArtifact({
                id: artifact.id,
                name: artifact.name,
                type: artifact.type,
                description: artifact.description ?? null,
                content_hash: artifact.checksum,
                source_system: artifact.sourceSystem,
                source_path: artifact.sourcePath,
                metadata: artifact.metadata ? JSON.stringify(artifact.metadata) : null,
            });
        }
        // Get diffs from database
        return this.db.getOutOfSyncArtifacts(source, target, artifactType);
    }
    /**
     * Get status of all systems
     */
    async status() {
        const availableSystems = getAvailableAdapters();
        const systems = [];
        for (const systemId of availableSystems) {
            const adapter = this.getSystemAdapter(systemId);
            const configured = adapter.isConfigured(this.projectRoot);
            const artifactCounts = {};
            if (configured) {
                const artifacts = await adapter.scanArtifacts(this.projectRoot);
                for (const artifact of artifacts) {
                    artifactCounts[artifact.type] = (artifactCounts[artifact.type] ?? 0) + 1;
                }
            }
            systems.push({
                id: systemId,
                name: adapter.name,
                configured,
                artifactCounts,
            });
        }
        return {
            systems,
            lastSync: this.db.getSetting("last_sync"),
            stats: this.db.getStats(),
        };
    }
    /**
     * Get recent sync history
     */
    getHistory(limit = 10) {
        const jobs = this.db.getRecentSyncJobs(limit);
        return jobs.map((job) => ({
            id: job.id,
            source: job.source_system,
            targets: JSON.parse(job.target_systems),
            status: job.status,
            startedAt: job.started_at,
            completedAt: job.completed_at ?? undefined,
            summary: job.summary ? JSON.parse(job.summary) : undefined,
        }));
    }
    /**
     * Get results for a specific job
     */
    getJobResults(jobId) {
        return this.db.getSyncResults(jobId);
    }
    /**
     * Sync MCP server configurations between systems
     */
    async syncMcpServers(options) {
        const results = {
            synced: 0,
            skipped: 0,
            failed: 0,
            details: [],
        };
        const spinner = ora({
            text: `Reading MCP servers from ${options.source}...`,
            color: "cyan",
        }).start();
        try {
            // Get source adapter and read MCP servers
            const sourceAdapter = this.getSystemAdapter(options.source);
            if (!sourceAdapter.capabilities.mcpServers) {
                spinner.fail(`Source system ${options.source} does not support MCP servers`);
                return results;
            }
            // Check if the adapter has MCP server methods
            if (!("readMcpServers" in sourceAdapter)) {
                spinner.fail(`Source adapter ${options.source} does not implement MCP server reading`);
                return results;
            }
            const servers = await sourceAdapter.readMcpServers(this.projectRoot);
            if (servers.length === 0) {
                spinner.warn(`No MCP servers found in ${options.source}`);
                return results;
            }
            spinner.succeed(`Found ${servers.length} MCP servers in ${options.source}`);
            // Sync to each target
            for (const target of options.targets) {
                const targetSpinner = ora({
                    text: `Syncing MCP servers to ${target}...`,
                    color: "blue",
                }).start();
                try {
                    const targetAdapter = this.getSystemAdapter(target);
                    // Check if target supports MCP servers
                    if (!targetAdapter.capabilities.mcpServers) {
                        targetSpinner.warn(`${target} does not support MCP servers, skipping`);
                        results.skipped++;
                        results.details.push({
                            target,
                            servers: [],
                            status: "skipped",
                            message: "MCP servers not supported",
                        });
                        continue;
                    }
                    // Initialize target if needed
                    if (!targetAdapter.isConfigured(this.projectRoot)) {
                        await targetAdapter.initialize(this.projectRoot);
                    }
                    // Check if the adapter has MCP server write methods
                    if (!("writeMcpServers" in targetAdapter)) {
                        targetSpinner.warn(`${target} adapter does not implement MCP server writing`);
                        results.skipped++;
                        results.details.push({
                            target,
                            servers: [],
                            status: "skipped",
                            message: "MCP server writing not implemented",
                        });
                        continue;
                    }
                    if (options.dryRun) {
                        targetSpinner.succeed(`Would sync ${servers.length} MCP servers to ${target}`);
                        results.synced++;
                        results.details.push({
                            target,
                            servers: servers.map((s) => s.name),
                            status: "success",
                            message: "Dry run - no changes made",
                        });
                        continue;
                    }
                    // Transform servers for the target system
                    const transformedServers = servers.map((server) => ({
                        ...server,
                        id: server.id.replace(options.source, target),
                        sourceSystem: target,
                    }));
                    // Write MCP servers to target
                    await targetAdapter.writeMcpServers(this.projectRoot, transformedServers);
                    targetSpinner.succeed(`Synced ${servers.length} MCP servers to ${target}`);
                    results.synced++;
                    results.details.push({
                        target,
                        servers: servers.map((s) => s.name),
                        status: "success",
                    });
                }
                catch (error) {
                    targetSpinner.fail(`Failed to sync MCP servers to ${target}: ${error}`);
                    results.failed++;
                    results.details.push({
                        target,
                        servers: [],
                        status: "failed",
                        message: String(error),
                    });
                }
            }
            return results;
        }
        catch (error) {
            spinner.fail(`Failed to read MCP servers: ${error}`);
            results.failed++;
            return results;
        }
    }
    /**
     * Generate a compact skill index from Claude skills
     * Returns a markdown document with skill names, descriptions, and categories
     */
    async generateSkillIndex(options = {}) {
        const spinner = ora({
            text: "Scanning skills for index generation...",
            color: "cyan",
        }).start();
        try {
            const claudeAdapter = this.getSystemAdapter("claude");
            const artifacts = await claudeAdapter.scanArtifacts(this.projectRoot, {
                types: ["skill"],
            });
            const skills = artifacts.filter((a) => a.type === "skill");
            if (skills.length === 0) {
                spinner.warn("No skills found");
                return { content: "", skillCount: 0, categories: {} };
            }
            // Categorize skills by extracting category from name or path
            const categories = {
                "Development": [],
                "Testing": [],
                "Security": [],
                "Documentation": [],
                "Planning": [],
                "Mobile": [],
                "Backend": [],
                "Frontend": [],
                "DevOps": [],
                "General": [],
            };
            for (const skill of skills) {
                const name = skill.name;
                const description = skill.description ?? this.extractDescription(skill.content);
                // Categorize based on name patterns
                const category = this.categorizeSkill(name, description);
                if (!categories[category]) {
                    categories[category] = [];
                }
                categories[category].push({ name, description });
            }
            // Generate compact markdown index
            let content = `# Skill Index

This document provides a compact reference to all available Claude skills.
**To use a skill:** Simply reference it by name (e.g., "use the debugging skill").

---

## Quick Reference

| Skill | Description |
|-------|-------------|
`;
            // Sort skills alphabetically
            const allSkills = skills
                .map((s) => ({
                name: s.name,
                description: s.description ?? this.extractDescription(s.content),
            }))
                .sort((a, b) => a.name.localeCompare(b.name));
            for (const skill of allSkills) {
                const shortDesc = skill.description.length > 80
                    ? skill.description.slice(0, 77) + "..."
                    : skill.description;
                content += `| \`${skill.name}\` | ${shortDesc} |\n`;
            }
            content += `\n---\n\n## Skills by Category\n\n`;
            // Add categorized sections (only non-empty categories)
            const categoryMap = {};
            for (const [category, skillList] of Object.entries(categories)) {
                if (skillList.length === 0)
                    continue;
                categoryMap[category] = skillList.map((s) => s.name);
                content += `### ${category}\n\n`;
                for (const skill of skillList.sort((a, b) => a.name.localeCompare(b.name))) {
                    content += `- **${skill.name}**: ${skill.description}\n`;
                }
                content += "\n";
            }
            content += `---\n\n*Generated by ai-config-sync • ${skills.length} skills indexed*\n`;
            spinner.succeed(`Generated skill index with ${skills.length} skills`);
            if (options.verbose) {
                console.log(`  Categories: ${Object.keys(categoryMap).join(", ")}`);
            }
            return {
                content,
                skillCount: skills.length,
                categories: categoryMap,
            };
        }
        catch (error) {
            spinner.fail(`Failed to generate skill index: ${error}`);
            throw error;
        }
    }
    /**
     * Extract description from skill content (first paragraph after frontmatter)
     */
    extractDescription(content) {
        // Remove YAML frontmatter
        const withoutFrontmatter = content.replace(/^---[\s\S]*?---\n*/, "");
        // Get first meaningful paragraph (skip headers)
        const lines = withoutFrontmatter.split("\n");
        for (const line of lines) {
            const trimmed = line.trim();
            if (trimmed && !trimmed.startsWith("#") && !trimmed.startsWith(">") && !trimmed.startsWith("-")) {
                return trimmed.length > 100 ? trimmed.slice(0, 97) + "..." : trimmed;
            }
        }
        return "No description available";
    }
    /**
     * Categorize a skill based on its name and description
     */
    categorizeSkill(name, description) {
        const combined = `${name} ${description}`.toLowerCase();
        if (combined.includes("test") || combined.includes("tdd") || combined.includes("e2e") || combined.includes("jest") || combined.includes("playwright")) {
            return "Testing";
        }
        if (combined.includes("security") || combined.includes("audit") || combined.includes("vulnerability") || combined.includes("tob-")) {
            return "Security";
        }
        if (combined.includes("doc") || combined.includes("readme") || combined.includes("changelog")) {
            return "Documentation";
        }
        if (combined.includes("plan") || combined.includes("brainstorm") || combined.includes("epic") || combined.includes("spec")) {
            return "Planning";
        }
        if (combined.includes("rn-") || combined.includes("react-native") || combined.includes("mobile") || combined.includes("expo")) {
            return "Mobile";
        }
        if (combined.includes("dotnet") || combined.includes("csharp") || combined.includes("backend") || combined.includes("api") || combined.includes("akka")) {
            return "Backend";
        }
        if (combined.includes("react") || combined.includes("frontend") || combined.includes("ui") || combined.includes("design")) {
            return "Frontend";
        }
        if (combined.includes("docker") || combined.includes("deploy") || combined.includes("ci") || combined.includes("kubernetes") || combined.includes("git")) {
            return "DevOps";
        }
        if (combined.includes("debug") || combined.includes("refactor") || combined.includes("code-review") || combined.includes("build")) {
            return "Development";
        }
        return "General";
    }
    /**
     * Sync skill index to limited systems (Gemini, Aider, Continue, Cody)
     */
    async syncSkillIndex(options = {}) {
        // Default targets are the limited systems
        const limitedSystems = ["gemini", "aider", "continue", "cody"];
        const targets = options.targets ?? limitedSystems;
        const results = {
            synced: 0,
            skipped: 0,
            failed: 0,
            details: [],
        };
        // Generate the skill index
        const { content, skillCount } = await this.generateSkillIndex({
            verbose: options.verbose,
        });
        if (skillCount === 0) {
            console.log("No skills to index");
            return results;
        }
        const spinner = ora({
            text: "Syncing skill index to limited systems...",
            color: "cyan",
        }).start();
        for (const target of targets) {
            // Only sync to limited systems
            if (!limitedSystems.includes(target)) {
                results.skipped++;
                results.details.push({
                    target,
                    status: "skipped",
                    message: "Not a limited system (has native skill support)",
                });
                continue;
            }
            try {
                const adapter = this.getSystemAdapter(target);
                // Initialize if needed
                if (!adapter.isConfigured(this.projectRoot)) {
                    await adapter.initialize(this.projectRoot);
                }
                if (options.dryRun) {
                    results.synced++;
                    results.details.push({
                        target,
                        status: "success",
                        message: `Would write skill index (${skillCount} skills)`,
                    });
                    continue;
                }
                // Write the skill index file
                await this.writeSkillIndex(target, content);
                results.synced++;
                results.details.push({
                    target,
                    status: "success",
                    message: `Wrote skill index with ${skillCount} skills`,
                });
            }
            catch (error) {
                results.failed++;
                results.details.push({
                    target,
                    status: "failed",
                    message: String(error),
                });
            }
        }
        if (results.synced > 0) {
            spinner.succeed(`Synced skill index to ${results.synced} system(s)`);
        }
        else if (results.failed > 0) {
            spinner.fail("Failed to sync skill index");
        }
        else {
            spinner.warn("No systems to sync skill index to");
        }
        return results;
    }
    /**
     * Write skill index to a specific system
     */
    async writeSkillIndex(target, content) {
        const { writeFileSync, mkdirSync, existsSync } = await import("node:fs");
        const { join, dirname } = await import("node:path");
        let targetPath;
        switch (target) {
            case "gemini":
                // Append to GEMINI.md or create SKILL_INDEX.md
                targetPath = join(this.projectRoot, "SKILL_INDEX.md");
                break;
            case "aider":
                // Write to .aider/SKILL_INDEX.md and reference in config
                targetPath = join(this.projectRoot, ".aider", "SKILL_INDEX.md");
                break;
            case "continue":
                // Write to .continue/SKILL_INDEX.md
                targetPath = join(this.projectRoot, ".continue", "SKILL_INDEX.md");
                break;
            case "cody":
                // Write to .sourcegraph/SKILL_INDEX.md
                targetPath = join(this.projectRoot, ".sourcegraph", "SKILL_INDEX.md");
                break;
            default:
                throw new Error(`Unsupported target for skill index: ${target}`);
        }
        const targetDir = dirname(targetPath);
        if (!existsSync(targetDir)) {
            mkdirSync(targetDir, { recursive: true });
        }
        writeFileSync(targetPath, content);
        // Update system config to reference the skill index
        await this.updateSystemConfigForSkillIndex(target);
    }
    /**
     * Update system configuration to reference the skill index
     */
    async updateSystemConfigForSkillIndex(target) {
        const { readFileSync, writeFileSync, existsSync } = await import("node:fs");
        const { join } = await import("node:path");
        switch (target) {
            case "aider": {
                // Update .aider.conf.yml to include SKILL_INDEX.md in read files
                const configPath = join(this.projectRoot, ".aider.conf.yml");
                if (existsSync(configPath)) {
                    let config = readFileSync(configPath, "utf-8");
                    if (!config.includes("SKILL_INDEX.md")) {
                        // Add to read section
                        if (config.includes("read:")) {
                            config = config.replace(/read:\n/, "read:\n  - .aider/SKILL_INDEX.md\n");
                        }
                        else {
                            config += "\nread:\n  - .aider/SKILL_INDEX.md\n";
                        }
                        writeFileSync(configPath, config);
                    }
                }
                break;
            }
            case "continue": {
                // Update .continue/config.json to include skill index in context
                const configPath = join(this.projectRoot, ".continue", "config.json");
                if (existsSync(configPath)) {
                    try {
                        const config = JSON.parse(readFileSync(configPath, "utf-8"));
                        if (!config.contextProviders) {
                            config.contextProviders = [];
                        }
                        // Check if file context provider exists for skill index
                        const hasSkillIndex = config.contextProviders.some((p) => p.name === "file" && p.params?.files?.includes(".continue/SKILL_INDEX.md"));
                        if (!hasSkillIndex) {
                            config.contextProviders.push({
                                name: "file",
                                params: {
                                    files: [".continue/SKILL_INDEX.md"],
                                },
                            });
                            writeFileSync(configPath, JSON.stringify(config, null, 2));
                        }
                    }
                    catch {
                        // Config might be malformed, skip
                    }
                }
                break;
            }
            // Gemini and Cody don't need config updates - they read files directly
        }
    }
    /**
     * Close database connection
     */
    close() {
        this.db.close();
    }
}
//# sourceMappingURL=sync-engine.js.map