/**
 * Sync Engine Tests
 */

import { describe, it, expect, beforeEach, afterEach } from "vitest";
import { mkdirSync, rmSync, writeFileSync, existsSync, readdirSync } from "node:fs";
import { join } from "node:path";
import { SyncEngine } from "../src/sync-engine.js";

const TEST_DIR = join(process.cwd(), "test-project");

describe("SyncEngine", () => {
  beforeEach(() => {
    // Create test directory structure
    mkdirSync(TEST_DIR, { recursive: true });

    // Create Claude structure
    mkdirSync(join(TEST_DIR, ".claude/skills/test-skill"), { recursive: true });
    mkdirSync(join(TEST_DIR, ".claude/agents"), { recursive: true });

    // Write test skill
    writeFileSync(
      join(TEST_DIR, ".claude/skills/test-skill/SKILL.md"),
      `---
description: Test skill for testing
---

# Test Skill

This is a test skill.
`
    );

    // Write test agent
    writeFileSync(
      join(TEST_DIR, ".claude/agents/test-agent.md"),
      `---
description: Test agent
role: tester
---

# Test Agent
`
    );
  });

  afterEach(() => {
    // Cleanup
    rmSync(TEST_DIR, { recursive: true, force: true });
  });

  it("should scan claude artifacts", async () => {
    const engine = new SyncEngine(TEST_DIR);

    try {
      const status = await engine.status();
      expect(status.systems.find((s) => s.id === "claude")?.configured).toBe(true);
    } finally {
      engine.close();
    }
  });

  it("should sync skills to opencode", async () => {
    const engine = new SyncEngine(TEST_DIR);

    try {
      // Initialize opencode
      mkdirSync(join(TEST_DIR, ".opencode/skill"), { recursive: true });

      const result = await engine.sync({
        source: "claude",
        targets: ["opencode"],
        artifactTypes: ["skill"],
        useSymlinks: true,
      });

      expect(result.results.failed).toBe(0);
      expect(existsSync(join(TEST_DIR, ".opencode/skill/test-skill"))).toBe(true);
    } finally {
      engine.close();
    }
  });

  it("should sync skills to codex with copy", async () => {
    const engine = new SyncEngine(TEST_DIR);

    try {
      // Initialize codex
      mkdirSync(join(TEST_DIR, ".codex/skills"), { recursive: true });

      const result = await engine.sync({
        source: "claude",
        targets: ["codex"],
        artifactTypes: ["skill"],
        useSymlinks: false,
      });

      expect(result.results.failed).toBe(0);
      expect(existsSync(join(TEST_DIR, ".codex/skills/test-skill/SKILL.md"))).toBe(true);
    } finally {
      engine.close();
    }
  });

  it("should perform dry run without changes", async () => {
    const engine = new SyncEngine(TEST_DIR);

    try {
      mkdirSync(join(TEST_DIR, ".opencode/skill"), { recursive: true });

      const result = await engine.sync({
        source: "claude",
        targets: ["opencode"],
        artifactTypes: ["skill"],
        dryRun: true,
      });

      // Should report what would be done but not actually create
      expect(result.results.failed).toBe(0);
      // Directory should not have the skill yet (dry run)
      const contents = readdirSync(join(TEST_DIR, ".opencode/skill"));
      expect(contents.length).toBe(0);
    } finally {
      engine.close();
    }
  });
});
