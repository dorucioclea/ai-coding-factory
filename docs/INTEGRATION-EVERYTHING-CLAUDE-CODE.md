# Integration Guide: Everything Claude Code → AI Coding Factory

This document outlines the valuable resources found in [affaan-m/everything-claude-code](https://github.com/affaan-m/everything-claude-code) and provides recommendations for integrating them into our enterprise .NET development workflow.

## Executive Summary

The everything-claude-code repository contains battle-tested configurations from an Anthropic hackathon winner. After thorough analysis, I've identified **high-value resources** that complement our existing setup while avoiding redundancy.

**Key Findings:**
- We already have extensive coverage for .NET-specific skills, commands, and hooks
- The repository offers valuable **general-purpose patterns** we can adopt
- Several **MCP server configurations** can enhance our workflow
- **Cross-client compatibility** is achievable for Cursor, Codex, Gemini, and OpenCode

---

## Part 1: What to Bring In

### 1.1 Agents (HIGH VALUE)

Our current setup has .NET-focused agents in `.opencode/agent/`. The following general-purpose agents from everything-claude-code would complement them:

| Agent | Purpose | Why Add It |
|-------|---------|------------|
| **architect.md** | System design decisions | Fills gap for architecture planning before implementation |
| **tdd-guide.md** | TDD methodology expert | Complements our existing testing skills with methodology guidance |
| **build-error-resolver.md** | Fix compilation issues | Useful for troubleshooting complex build failures |
| **refactor-cleaner.md** | Dead code cleanup | Aligns with our code quality focus |
| **planner.md** | Implementation planning | Systematic feature planning before coding |

**Skip (Already Covered):**
- `code-reviewer.md` - We have `net-code-reviewer.md`
- `security-reviewer.md` - We have `net-security-auditor.md`
- `doc-updater.md` - Lower priority, can add later

### 1.2 Commands (MEDIUM VALUE - Selective)

We have 25 commands. Valuable additions from everything-claude-code:

| Command | Purpose | Priority |
|---------|---------|----------|
| **/tdd** | TDD workflow (RED-GREEN-REFACTOR) | HIGH - Missing methodology command |
| **/checkpoint** | Save verification state | HIGH - Session state management |
| **/verify** | Run verification loop | HIGH - Continuous verification |
| **/orchestrate** | Subagent orchestration | MEDIUM - Multi-agent coordination |
| **/eval** | Evaluation framework | MEDIUM - Quality metrics |

**Skip (Already Have Equivalents):**
- `/code-review` - Already have
- `/plan` - Have `/implement` which covers planning
- `/build-fix` - Can add to `/validate`
- `/learn` - Have `/learn`
- `/update-docs`, `/update-deps` - Have `/generate-docs`, `/dependencies`

### 1.3 Skills (HIGH VALUE - Selective)

**Add These:**

| Skill | Purpose | Rationale |
|-------|---------|-----------|
| **tdd-workflow/** | TDD methodology | Systematic RED-GREEN-REFACTOR cycle |
| **security-review/** | Security checklist | Complement our security auditor |
| **verification-loop/** | Continuous verification | Checkpoint vs continuous evals |
| **continuous-learning/** | Auto-extract patterns | Learn from session history |
| **strategic-compact/** | Token optimization | Manage context efficiently |
| **backend-patterns/** | Backend best practices | Node.js/Express patterns (useful for polyglot projects) |

**Skip (Already Have or Not Applicable):**
- `frontend-patterns/` - Have `react-best-practices/`
- `coding-standards/` - Covered in our CLAUDE.md
- `clickhouse-io/` - Not relevant to our .NET focus
- `eval-harness/` - Lower priority

### 1.4 Rules (HIGH VALUE)

Everything-claude-code has modular rules we can adopt:

| Rule | Purpose | Action |
|------|---------|--------|
| **security.md** | Security checks | Merge with existing security guidelines |
| **testing.md** | TDD/80% coverage | Align with our testing standards |
| **git-workflow.md** | Commit/PR process | Add to our commit workflow |
| **performance.md** | Model selection, context | Token optimization |
| **patterns.md** | Design patterns | Reference patterns |

### 1.5 Hooks (HIGH VALUE)

**Add These:**

| Hook | Event | Purpose |
|------|-------|---------|
| **memory-persistence/** | Session start/end | Persist learning across sessions |
| **strategic-compact/** | Pre-compact | Save state before compaction |
| **suggest-compact.js** | Token threshold | Suggest compaction when context is large |

**Our Current Hooks (Keep):**
- Pre/post build, test, commit hooks are .NET-specific - keep them
- The memory persistence hooks complement rather than replace

### 1.6 MCP Servers (HIGH VALUE)

Expand our MCP configuration with these servers:

| Server | Purpose | Priority |
|--------|---------|----------|
| **azure** | Azure cloud operations | HIGH - Enterprise .NET |
| **docker** | Container operations | HIGH - Container-first |
| **postgres** | PostgreSQL access | HIGH - Database operations |
| **gitlab** | GitLab operations | MEDIUM - If using GitLab |
| **slack** | Notifications | MEDIUM - Team communication |
| **stripe** | Payment ops | LOW - If needed |
| **firebase** | Firebase ops | LOW - If needed |
| **supabase** | Supabase ops | LOW - If needed |
| **firecrawl** | Web scraping | LOW - If needed |

### 1.7 Contexts (MEDIUM VALUE)

Add these context modes:

| Context | When to Use |
|---------|-------------|
| **dev.md** | Active development mode |
| **review.md** | Code review mode |
| **research.md** | Research/exploration mode |

---

## Part 2: Cross-Client Compatibility

### Comparison Matrix

| Feature | Claude Code | Cursor | Codex CLI | Gemini | OpenCode |
|---------|-------------|--------|-----------|--------|----------|
| **Config Dir** | `.claude/` | `.cursor/` | `~/.codex/` | `~/.gemini/` | `.opencode/` |
| **Instructions** | CLAUDE.md | .cursorrules/.mdc | AGENTS.md | GEMINI.md | AGENTS.md (fallback) |
| **Commands** | commands/*.md | N/A | commands/ | N/A | agent/*.md |
| **Skills** | skills/*/SKILL.md | rules/*.mdc | skills/*/SKILL.md | IDE settings | skill/*/SKILL.md |
| **Hooks** | Yes (JSON) | No | Yes (TOML) | No | Via plugins |
| **MCP** | Yes | No | Yes | Yes (Oct 2025+) | Yes |

### Recommended Strategy: Multi-Format Generation

Create a unified source that generates tool-specific configs:

```
shared/
├── instructions.md          # Universal instructions
├── rules/                   # Modular rules (security, testing, etc.)
├── skills/                  # SKILL.md format (portable)
└── sync-configs.sh          # Generates all tool-specific configs
```

### 2.1 Cursor Integration

**Create `.cursor/rules/` from our content:**

```markdown
# .cursor/rules/dotnet-clean-arch.mdc
---
description: Clean Architecture rules for .NET projects
globs: ["**/*.cs", "**/*.csproj"]
alwaysApply: true
---

# Clean Architecture Rules

- Domain layer has ZERO external dependencies
- Application depends ONLY on Domain
- Infrastructure depends on Domain + Application
- API is the composition root
```

**Generator Script Pattern:**
```bash
#!/bin/bash
# sync-to-cursor.sh

# Convert SKILL.md files to .mdc format
for skill in .claude/skills/*/SKILL.md; do
  skill_name=$(dirname "$skill" | xargs basename)
  # Extract and transform to .mdc format
  yq '.description' "$skill" > ".cursor/rules/${skill_name}.mdc"
done
```

### 2.2 Codex CLI Integration

**Create `AGENTS.md` from CLAUDE.md:**

```bash
# symlink or copy
ln -s CLAUDE.md AGENTS.md
```

Skills are already compatible - use symlinks:
```bash
mkdir -p ~/.codex/skills
ln -s $(pwd)/.claude/skills/* ~/.codex/skills/
```

### 2.3 Gemini Integration

**Create `GEMINI.md`:**
- Copy/symlink from CLAUDE.md or AGENTS.md
- Create `.aiexclude` from .gitignore patterns

### 2.4 OpenCode Integration

Already compatible - we have `.opencode/` directory. Sync skills:
```bash
# Ensure skills are in sync
ln -sf ../.claude/skills/* .opencode/skill/
```

### 2.5 Sync Script

Create a sync script that keeps all configs in sync:

```bash
#!/bin/bash
# scripts/sync-ai-configs.sh

set -e

echo "Syncing AI coding assistant configurations..."

# 1. Ensure AGENTS.md exists (for Codex/OpenCode)
if [ ! -f AGENTS.md ]; then
  echo "Creating AGENTS.md symlink..."
  ln -s CLAUDE.md AGENTS.md
fi

# 2. Sync skills to OpenCode
echo "Syncing skills to .opencode/skill/..."
for skill_dir in .claude/skills/*/; do
  skill_name=$(basename "$skill_dir")
  if [ ! -L ".opencode/skill/$skill_name" ]; then
    ln -sf "../../.claude/skills/$skill_name" ".opencode/skill/$skill_name"
  fi
done

# 3. Generate Cursor rules from skills
echo "Generating Cursor rules..."
mkdir -p .cursor/rules
for skill in .claude/skills/*/SKILL.md; do
  skill_name=$(basename "$(dirname "$skill")")
  # Simple extraction - would need yq for proper YAML parsing
  head -20 "$skill" | grep -v "^---" > ".cursor/rules/${skill_name}.mdc"
done

# 4. Create GEMINI.md
if [ ! -f GEMINI.md ]; then
  echo "Creating GEMINI.md symlink..."
  ln -s CLAUDE.md GEMINI.md
fi

echo "Sync complete!"
```

---

## Part 3: Implementation Plan

### Phase 1: Immediate Additions (Low Risk)

1. **Add TDD Skill**
   - Copy `tdd-workflow/` to `.claude/skills/`
   - Adapt for .NET/xUnit patterns

2. **Add Verification Loop Skill**
   - Copy `verification-loop/` to `.claude/skills/`
   - Configure for our validation scripts

3. **Add Memory Persistence Hooks**
   - Add session start/end hooks for context persistence
   - Integrate with our project structure

4. **Expand MCP Servers**
   - Enable Azure, Docker, PostgreSQL servers
   - Configure with environment variables

### Phase 2: Commands and Agents

1. **Add /tdd Command**
   - Implement TDD workflow command
   - Integrate with existing test generation

2. **Add /checkpoint and /verify Commands**
   - Session state management
   - Verification loop integration

3. **Add Architect Agent**
   - System design decisions
   - ADR generation integration

### Phase 3: Cross-Client Sync

1. **Create sync script**
   - Automate config generation for all clients
   - Add to CI/CD for consistency

2. **Create AGENTS.md**
   - Vendor-neutral instruction file
   - Maintain alongside CLAUDE.md

3. **Test with Cursor**
   - Generate .mdc rules
   - Validate in Cursor IDE

---

## Part 4: File Locations Summary

### Files to Create

```
.claude/
├── skills/
│   ├── tdd-workflow/SKILL.md           # NEW: TDD methodology
│   ├── verification-loop/SKILL.md      # NEW: Continuous verification
│   ├── continuous-learning/SKILL.md    # NEW: Pattern extraction
│   └── strategic-compact/SKILL.md      # NEW: Token optimization
├── commands/
│   ├── tdd.md                          # NEW: TDD command
│   ├── checkpoint.md                   # NEW: Save state
│   └── verify.md                       # NEW: Verification loop
├── hooks/
│   ├── session-start.js                # NEW: Load context
│   ├── session-end.js                  # NEW: Save state
│   └── suggest-compact.js              # NEW: Compaction suggestion
├── context/
│   ├── dev.md                          # NEW: Development mode
│   └── review.md                       # NEW: Review mode
└── agents/
    ├── architect.md                    # NEW: System design
    ├── tdd-guide.md                    # NEW: TDD expert
    └── planner.md                      # NEW: Implementation planning

# Cross-client compatibility
AGENTS.md                               # NEW: Symlink to CLAUDE.md
GEMINI.md                               # NEW: Symlink to CLAUDE.md
.cursor/rules/*.mdc                     # NEW: Generated from skills
scripts/sync-ai-configs.sh              # NEW: Config sync script
```

### MCP Server Additions

Update `.claude/mcp-servers.json`:

```json
{
  "azure": {
    "command": "npx",
    "args": ["-y", "@modelcontextprotocol/server-azure"],
    "env": {
      "AZURE_SUBSCRIPTION_ID": "${AZURE_SUBSCRIPTION_ID}"
    },
    "enabled": true
  },
  "docker": {
    "command": "npx",
    "args": ["-y", "@modelcontextprotocol/server-docker"],
    "enabled": true
  },
  "postgres": {
    "command": "npx",
    "args": ["-y", "@modelcontextprotocol/server-postgres"],
    "env": {
      "DATABASE_URL": "${DATABASE_URL}"
    },
    "enabled": false
  }
}
```

---

## Part 5: What NOT to Bring In

### Already Covered
- Code review agents/commands (have `net-code-reviewer`)
- Security review (have `net-security-auditor`)
- Most React/frontend patterns (have `react-best-practices`)
- Documentation generation (have `/generate-docs`)
- Dependency management (have `/dependencies`)

### Not Relevant to .NET Focus
- `clickhouse-io/` - ClickHouse specific
- Language-specific skills for Python, Go, Rust
- Node.js package manager detection (we use NuGet)

### Lower Priority (Add Later If Needed)
- E2E testing with Playwright (can add when needed)
- Doc updater agent (manual updates work fine)
- Stripe, Firebase, Supabase MCPs (add when needed)

---

## Conclusion

The everything-claude-code repository provides valuable additions to our enterprise .NET workflow, particularly:

1. **TDD Methodology** - Systematic test-driven development
2. **Verification Loops** - Continuous quality checking
3. **Memory Persistence** - Context across sessions
4. **Token Optimization** - Efficient context management
5. **Cross-Client Compatibility** - Work with Cursor, Codex, Gemini

The implementation can be done incrementally with low risk, starting with skills and expanding to commands and hooks.

---

## References

- [everything-claude-code Repository](https://github.com/affaan-m/everything-claude-code)
- [AGENTS.md Standard](https://agents.md/)
- [Cursor Rules Documentation](https://cursor.com/docs/context/rules)
- [OpenAI Codex CLI](https://developers.openai.com/codex)
- [Google Gemini Code Assist](https://developers.google.com/gemini-code-assist)
