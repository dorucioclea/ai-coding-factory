/**
 * Sync Engine
 *
 * Core synchronization logic for transferring configurations
 * between AI coding assistants.
 */

import { existsSync, cpSync, mkdirSync, symlinkSync, unlinkSync, lstatSync, rmSync, readlinkSync } from "node:fs";
import { join, dirname, relative } from "node:path";
import ora from "ora";
import { DatabaseManager, type MappingRuleRecord } from "./database/index.js";
import {
  getAdapter,
  getAvailableAdapters,
  type SystemAdapter,
} from "./adapters/index.js";
import type {
  SystemId,
  ArtifactType,
  Artifact,
  SyncResult,
  ArtifactDiff,
} from "./models/types.js";
import { generateJobId } from "./utils/helpers.js";

/**
 * Check if a path is a symlink pointing to a source within .claude/
 */
function isClaudeSymlink(projectRoot: string, targetPath: string): boolean {
  try {
    const fullPath = join(projectRoot, targetPath);
    const stats = lstatSync(fullPath);
    if (!stats.isSymbolicLink()) return false;

    const linkTarget = readlinkSync(fullPath);
    // Check if the symlink points to something in .claude/
    return linkTarget.includes(".claude") || linkTarget.includes("claude");
  } catch {
    return false;
  }
}

/**
 * Check if a directory was created by this sync tool (has sync state in DB)
 */
function isSyncedDirectory(db: DatabaseManager, targetSystem: SystemId, targetPath: string): boolean {
  const syncStates = db.getSyncStatesForTarget(targetSystem);
  return syncStates.some(state =>
    state.target_path === targetPath ||
    state.target_path?.startsWith(targetPath + "/")
  );
}

export interface SyncOptions {
  source: SystemId;
  targets: SystemId[];
  artifactTypes?: ArtifactType[];
  dryRun?: boolean;
  force?: boolean;
  useSymlinks?: boolean;
  verbose?: boolean;
  /** Delete artifacts from targets that no longer exist in source */
  syncDeletions?: boolean;
}

export interface SyncSummary {
  jobId: string;
  source: SystemId;
  targets: SystemId[];
  startedAt: Date;
  completedAt: Date;
  results: {
    total: number;
    created: number;
    updated: number;
    skipped: number;
    failed: number;
    symlinked: number;
    deleted: number;
  };
  details: SyncResult[];
}

export class SyncEngine {
  private db: DatabaseManager;
  private projectRoot: string;
  private adapters: Map<SystemId, SystemAdapter> = new Map();

  constructor(projectRoot: string) {
    this.projectRoot = projectRoot;
    this.db = new DatabaseManager(projectRoot);
  }

  /**
   * Get or create adapter for a system
   */
  private getSystemAdapter(systemId: SystemId): SystemAdapter {
    if (!this.adapters.has(systemId)) {
      this.adapters.set(systemId, getAdapter(systemId));
    }
    return this.adapters.get(systemId)!;
  }

  /**
   * Execute a full sync operation
   */
  async sync(options: SyncOptions): Promise<SyncSummary> {
    const jobId = generateJobId();
    const startedAt = new Date();

    const results: SyncResult[] = [];
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

            const result = await this.syncArtifact(
              artifact,
              sourceAdapter,
              targetAdapter,
              mappingRules,
              options
            );

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
            const deletionResults = await this.syncDeletions(
              options.source,
              target,
              artifacts,
              targetAdapter,
              options
            );

            for (const result of deletionResults) {
              results.push(result);
              summary.total++;
              if (result.operation === "delete" && result.success) {
                summary.deleted++;
              } else if (!result.success) {
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

          targetSpinner.succeed(
            `Synced ${artifacts.length} artifacts to ${target}${summary.deleted > 0 ? ` (${summary.deleted} deleted)` : ""}`
          );
        } catch (error) {
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
    } catch (error) {
      spinner.fail(`Sync failed: ${error}`);
      this.db.updateSyncJob(jobId, "failed", { error: String(error) });
      throw error;
    }
  }

  /**
   * Sync a single artifact
   */
  private async syncArtifact(
    artifact: Artifact,
    _sourceAdapter: SystemAdapter,
    targetAdapter: SystemAdapter,
    mappingRules: MappingRuleRecord[],
    options: SyncOptions
  ): Promise<SyncResult> {
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
      const typeCapability = `${artifact.type}s` as keyof typeof capabilities;

      if (!capabilities[typeCapability]) {
        // Need to transform the artifact
        if (artifact.type === "skill" && capabilities.rules) {
          // Transform skill to rule
          const transformedArtifact = targetAdapter.transformArtifact(artifact);

          return await this.writeArtifact(
            transformedArtifact,
            targetAdapter,
            mappingRules,
            options,
            baseResult
          );
        } else {
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
      if (rule?.transform_type && rule.transform_type !== "none") {
        finalArtifact = targetAdapter.transformArtifact(artifact, {
          sourceFormat: "markdown",
          targetFormat: rule.transform_type,
        });
      }

      return await this.writeArtifact(
        finalArtifact,
        targetAdapter,
        mappingRules,
        options,
        baseResult,
        useSymlink ? artifact.sourcePath : undefined
      );
    } catch (error) {
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
   */
  private async writeArtifact(
    artifact: Artifact,
    targetAdapter: SystemAdapter,
    _mappingRules: MappingRuleRecord[],
    options: SyncOptions,
    baseResult: Omit<SyncResult, "operation" | "success" | "targetPath">,
    symlinkSource?: string
  ): Promise<SyncResult> {
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
          } else if (stats.isFile()) {
            // Safe to remove single files
            unlinkSync(fullTargetPath);
          } else if (stats.isDirectory()) {
            // Check if this directory was created by previous sync (safe to replace)
            // or if it's native content (needs --force to replace)
            const wasSynced = isSyncedDirectory(this.db, targetAdapter.systemId, targetPath);
            const isSymlinked = isClaudeSymlink(this.projectRoot, targetPath);

            if (isSymlinked || wasSynced || options.force) {
              // Safe to replace: it's a symlink, was synced before, or user requested --force
              rmSync(fullTargetPath, { recursive: true, force: true });
            } else {
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
        } catch {
          // File doesn't exist, which is fine
        }

        // For skills, symlink the directory
        if (artifact.type === "skill") {
          const sourceSkillDir = dirname(join(this.projectRoot, symlinkSource));
          const relativePath = relative(targetDir, sourceSkillDir);
          symlinkSync(relativePath, fullTargetPath);
        } else {
          const sourcePath = join(this.projectRoot, symlinkSource);
          const relativePath = relative(targetDir, sourcePath);
          symlinkSync(relativePath, fullTargetPath);
        }

        // Update sync state
        this.db.upsertSyncState({
          artifact_id: artifact.id,
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
      } else {
        // Copy with full directory structure for skills
        if (artifact.type === "skill" && symlinkSource) {
          const sourceSkillDir = dirname(join(this.projectRoot, symlinkSource));
          const targetSkillDir = dirname(join(this.projectRoot, targetPath));

          // Copy entire skill directory
          if (existsSync(sourceSkillDir)) {
            cpSync(sourceSkillDir, targetSkillDir, { recursive: true });
          }
        } else {
          await targetAdapter.writeArtifact(this.projectRoot, artifact, {
            overwrite: true,
            createDirectories: true,
            symlinkTarget: symlinkSource,
          });
        }

        // Update sync state
        this.db.upsertSyncState({
          artifact_id: artifact.id,
          target_system: targetAdapter.systemId,
          target_path: targetPath,
          sync_method: "copy",
          synced_hash: artifact.checksum,
          last_synced_at: new Date().toISOString(),
          status: "synced",
          error_message: null,
        });

        const syncState = this.db.getSyncState(artifact.id, targetAdapter.systemId);
        const isUpdate = syncState && syncState.synced_hash;

        return {
          ...baseResult,
          operation: isUpdate ? "update" : "create",
          success: true,
          message: `${isUpdate ? "Updated" : "Created"} ${targetPath}`,
          targetPath,
        };
      }
    } catch (error) {
      // Update sync state with error
      this.db.upsertSyncState({
        artifact_id: artifact.id,
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
  private async syncDeletions(
    sourceSystem: SystemId,
    targetSystem: SystemId,
    currentSourceArtifacts: Artifact[],
    targetAdapter: SystemAdapter,
    options: SyncOptions
  ): Promise<SyncResult[]> {
    const results: SyncResult[] = [];

    // Get all previously synced artifacts for this source→target pair
    const syncStates = this.db.getSyncStatesForTarget(targetSystem);
    const currentSourceIds = new Set(currentSourceArtifacts.map(a => a.id));

    for (const syncState of syncStates) {
      // Check if this artifact still exists in source
      const artifact = this.db.getArtifact(syncState.artifact_id);
      if (!artifact) continue;

      // Only process artifacts from our source system
      if (artifact.source_system !== sourceSystem) continue;

      // Check if artifact type matches filter
      if (options.artifactTypes && !options.artifactTypes.includes(artifact.type as ArtifactType)) {
        continue;
      }

      // If artifact no longer exists in current source scan, it was deleted
      if (!currentSourceIds.has(syncState.artifact_id)) {
        const baseResult = {
          artifactId: syncState.artifact_id,
          artifactName: artifact.name,
          artifactType: artifact.type as ArtifactType,
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
        } else {
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
          } catch (error) {
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
  async diff(
    source: SystemId,
    target: SystemId,
    artifactType?: ArtifactType
  ): Promise<ArtifactDiff[]> {
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
  async status(): Promise<{
    systems: Array<{
      id: SystemId;
      name: string;
      configured: boolean;
      artifactCounts: Record<string, number>;
    }>;
    lastSync?: string;
    stats: ReturnType<DatabaseManager["getStats"]>;
  }> {
    const availableSystems = getAvailableAdapters();
    const systems: Array<{
      id: SystemId;
      name: string;
      configured: boolean;
      artifactCounts: Record<string, number>;
    }> = [];

    for (const systemId of availableSystems) {
      const adapter = this.getSystemAdapter(systemId);
      const configured = adapter.isConfigured(this.projectRoot);
      const artifactCounts: Record<string, number> = {};

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
  getHistory(limit = 10): Array<{
    id: string;
    source: SystemId;
    targets: SystemId[];
    status: string;
    startedAt: string;
    completedAt?: string;
    summary?: Record<string, unknown>;
  }> {
    const jobs = this.db.getRecentSyncJobs(limit);

    return jobs.map((job) => ({
      id: job.id,
      source: job.source_system as SystemId,
      targets: JSON.parse(job.target_systems) as SystemId[],
      status: job.status,
      startedAt: job.started_at,
      completedAt: job.completed_at ?? undefined,
      summary: job.summary ? JSON.parse(job.summary) : undefined,
    }));
  }

  /**
   * Get results for a specific job
   */
  getJobResults(jobId: string): SyncResult[] {
    return this.db.getSyncResults(jobId);
  }

  /**
   * Close database connection
   */
  close(): void {
    this.db.close();
  }
}
