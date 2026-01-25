# Repository Assessment & Merge Plan

**Date**: 2026-01-25
**Author**: Claude Opus 4.5
**Target Repository**: `/Users/dorucioclea/Documents/GitHub/ai-coding-factory`

---

## Executive Summary

After comprehensive analysis of the AI Coding Factory repository and four external repositories, I've identified significant opportunities to enhance the platform with non-overlapping skills, agents, commands, and hooks. The current repository is **well-structured but has gaps** in several areas that the external repositories address.

---

## Current Repository Assessment

### Strengths

1. **Comprehensive Template System**: Clean Architecture (.NET), React Frontend (Next.js), React Native (Expo SDK 52), Infrastructure templates
2. **Strong Governance**: CORPORATE_RND_POLICY.md, story-test-commit traceability, validation scripts
3. **Multi-Platform AI Support**: Claude Code, Cursor, OpenCode configurations
4. **React Native Excellence**: 14+ specialized RN skills/agents with Sentry, design tokens, animations
5. **Security Focus**: Trail of Bits security skills (29+), OWASP compliance, fuzzing tools

### Critical Gaps Identified

| Gap | Impact | Source Fix |
|-----|--------|-----------|
| No autonomous QA system (continuous error fixing) | Manual intervention required | claude-code-maestro (Ralph Wiggum) |
| No long-term memory/brain persistence | Context lost between sessions | claude-code-maestro (brain.jsonl) |
| Limited git worktree isolation guidance | Ad-hoc worktree usage | claude-code-maestro |
| Missing advanced .NET skills (Akka.NET, Aspire, concurrency) | Limited distributed systems support | claude-code-dotnet |
| No BDD-style testing patterns | Testing methodology gaps | dotnet-claude-code-skills |
| Missing DevOps/productivity skills | Limited automation coverage | devkit |
| No database migration generator | Manual migration creation | devkit |
| Missing API development utilities | Limited API tooling | devkit |

### Issues Found

1. **Duplicate Configurations**: OpenCode (`.opencode/`) and Claude Code (`.claude/`) have overlapping skills
2. **Inconsistent Naming**: Some skills use `net-*` prefix, others don't
3. **Missing Hook Integration**: No session-start auto-detection of tech stack
4. **No Brain/Memory System**: Context doesn't persist across sessions
5. **Basic Planning Skills**: Planning exists but lacks "Blast Radius Mapping" and dependency forecasting

---

## External Repository Analysis

### 1. claude-code-maestro (xenitV1)

**Most Valuable Assets** (MUST MERGE):

| Asset | Value | Overlap Status |
|-------|-------|----------------|
| **Ralph Wiggum** (autonomous QA) | CRITICAL - autonomous error fixing with circuit breaker | **NO OVERLAP** |
| **Brain System** (long-term memory) | CRITICAL - context persistence across sessions | **NO OVERLAP** |
| **git-worktrees skill** | HIGH - isolation protocols, cross-platform setup | **PARTIAL** - enhance existing |
| **session-start.js** hook | HIGH - tech stack auto-detection (30+ frameworks) | **NO OVERLAP** |
| **brain-sync.js** hook | HIGH - auto-sync after every tool use | **NO OVERLAP** |
| **Grandmaster agent** | MEDIUM - unified elite architect pattern | **PARTIAL** - different approach |
| **clean-code skill** | MEDIUM - 2025 standards, supply chain security | **PARTIAL** - enhance existing |
| **planning-mastery skill** | MEDIUM - RFC-Lite format, dependency forecasting | **PARTIAL** - enhance existing |
| **frontend-design skill** | MEDIUM - Anti-AI aesthetic mandate | **PARTIAL** - different approach |
| **backend-design skill** | MEDIUM - Vertical slice architecture | **PARTIAL** - different from Clean Arch |
| **optimization-mastery skill** | LOW - 2026-grade optimization | **PARTIAL** - overlaps with performance |
| **browser-extension skill** | LOW - MV3 extension development | **NO OVERLAP** - niche |

**Hook Scripts to Merge**:
- `session-start.js` - Auto-detect tech stack, inject brain memory
- `brain-sync.js` - Sync context after every tool use
- `stop.js` - Ralph Wiggum controller for exit blocking
- `lib/utils.js` - Utility functions
- `lib/brain.js` - Brain/memory operations
- `lib/ralph.js` - Ralph state management

### 2. claude-code-dotnet (Aaronontheweb)

**Most Valuable Assets** (MUST MERGE):

| Asset | Value | Overlap Status |
|-------|-------|----------------|
| **Akka.NET Specialist agent** | HIGH - actor systems, distributed computing | **NO OVERLAP** |
| **DocFX Specialist agent** | MEDIUM - documentation system | **NO OVERLAP** |
| **.NET Concurrency Specialist agent** | HIGH - threading, race condition analysis | **NO OVERLAP** |
| **.NET Performance Analyst agent** | HIGH - profiling, benchmarking analysis | **NO OVERLAP** |
| **.NET Benchmark Designer agent** | MEDIUM - BenchmarkDotNet expertise | **NO OVERLAP** |
| **modern-csharp-coding-standards skill** | HIGH - records, Span<T>, async patterns | **PARTIAL** - enhances existing |
| **akka-net-testing-patterns skill** | HIGH - Akka.Hosting.TestKit patterns | **NO OVERLAP** |
| **akka-net-aspire-configuration skill** | HIGH - .NET Aspire integration | **NO OVERLAP** |
| **aspire-integration-testing skill** | HIGH - Aspire testing facilities | **NO OVERLAP** |
| **testcontainers-integration-tests skill** | HIGH - TestContainers patterns | **PARTIAL** - existing has basics |
| **playwright-blazor-testing skill** | MEDIUM - Blazor UI testing | **NO OVERLAP** |

### 3. dotnet-claude-code-skills (nesbo)

**Most Valuable Assets** (MUST MERGE):

| Asset | Value | Overlap Status |
|-------|-------|----------------|
| **ddd-dotnet skill** | HIGH - Hexagonal/Ports-Adapters, Paramore.Brighter | **PARTIAL** - different CQRS approach |
| **data-dotnet skill** | HIGH - EF Core patterns, auto-DI registration | **PARTIAL** - enhances existing |
| **bdd-dotnet skill** | HIGH - BDD-style testing, TestDataBuilder | **NO OVERLAP** |
| **add-serena skill** | MEDIUM - Serena MCP integration for LSP | **NO OVERLAP** |

### 4. devkit (CuriousLearner)

**Most Valuable Assets** (SELECTIVE MERGE - 25 of 52 skills):

| Category | Skills to Merge | Overlap Status |
|----------|----------------|----------------|
| **API Development** | api-tester, openapi-generator, mock-server, webhook-tester, api-documentation | **NO OVERLAP** |
| **Database** | query-builder, schema-visualizer, migration-generator, seed-data-generator, query-optimizer | **NO OVERLAP** |
| **DevOps** | log-analyzer, resource-monitor | **NO OVERLAP** (docker-helper overlaps) |
| **Productivity** | snippet-manager, search-enhancer, conflict-resolver, changelog-generator | **NO OVERLAP** |
| **Data/Analytics** | csv-processor, json-transformer, chart-generator, data-validator | **NO OVERLAP** |
| **Language-Specific** | go-mod-helper, rust-cargo-assistant, java-maven-helper | **NO OVERLAP** |
| **Collaboration** | pr-template-generator, code-explainer, onboarding-helper, meeting-notes, architecture-documenter | **PARTIAL** |

**Skills to SKIP** (Overlap with existing):
- code-formatter (existing)
- test-generator (existing net-testing, tdd-guide)
- code-reviewer (existing)
- complexity-analyzer (partial overlap)
- dead-code-detector (existing refactor-cleaner)
- dependency-auditor (existing tob-* security skills)
- secret-scanner (existing tob-semgrep)
- security-headers (existing security skill)
- deployment-checker (existing devops agent)
- env-manager (existing infrastructure templates)
- docker-helper (existing net-docker)
- project-scaffolder (existing scaffold command)
- python-venv-manager (not .NET focused)
- npm-helper (partial overlap)

**Commands to Merge**:
- `/devkit:quality-check` - Adapt as `/quality-check`
- `/devkit:pre-deploy` - Adapt as `/pre-deploy`

---

## Merge Plan

### Phase 1: Critical Infrastructure (Priority: HIGHEST)

**From claude-code-maestro**:

1. **Ralph Wiggum System**
   - Copy: `skills/ralph-wiggum/SKILL.md` → `.claude/skills/ralph-wiggum/`
   - Copy: `scripts/js/ralph-*.js` → `.claude/scripts/`
   - Copy: `hooks/lib/ralph.js` → `.claude/hooks/lib/`
   - Add `.maestro/` directory support for state files

2. **Brain/Long-Term Memory System**
   - Copy: `hooks/lib/brain.js` → `.claude/hooks/lib/`
   - Copy: `hooks/brain-sync.js` → `.claude/hooks/`
   - Add brain.jsonl support to hooks.json
   - Create `.maestro/` directory in project template

3. **Session Start Enhancement**
   - Merge: `hooks/session-start.js` → enhance `.claude/hooks/`
   - Add tech stack auto-detection (30+ frameworks)
   - Add brain injection on session start

4. **Git Worktrees Enhancement**
   - Merge: `skills/git-worktrees/SKILL.md` → enhance existing `.claude/skills/using-git-worktrees/`
   - Add: Isolation Integrity Mandate, cross-platform setup scripts

### Phase 2: .NET Excellence (Priority: HIGH)

**From claude-code-dotnet**:

1. **New Agents**
   - Copy: `agents/akka-net-specialist.md` → `.claude/agents/`
   - Copy: `agents/docfx-specialist.md` → `.claude/agents/`
   - Copy: `agents/dotnet-concurrency-specialist.md` → `.claude/agents/`
   - Copy: `agents/dotnet-performance-analyst.md` → `.claude/agents/`
   - Copy: `agents/dotnet-benchmark-designer.md` → `.claude/agents/`

2. **New Skills**
   - Copy: `skills/akka-net-testing-patterns.md` → `.claude/skills/`
   - Copy: `skills/akka-net-aspire-configuration.md` → `.claude/skills/`
   - Copy: `skills/aspire-integration-testing.md` → `.claude/skills/`
   - Copy: `skills/playwright-blazor-testing.md` → `.claude/skills/`
   - Merge: `skills/modern-csharp-coding-standards.md` → enhance existing
   - Merge: `skills/testcontainers-integration-tests.md` → enhance existing

**From dotnet-claude-code-skills**:

1. **New Skills**
   - Copy: `ddd-dotnet/SKILL.md` → `.claude/skills/ddd-hexagonal/`
   - Copy: `data-dotnet/SKILL.md` → `.claude/skills/data-persistence-patterns/`
   - Copy: `bdd-dotnet/SKILL.md` → `.claude/skills/bdd-testing/`
   - Copy: `add-serena/SKILL.md` → `.claude/skills/serena-mcp/`

### Phase 3: DevOps & Productivity (Priority: MEDIUM)

**From devkit**:

1. **API Development Skills** (5 skills)
   - Copy: `skills/api-tester/SKILL.md` → `.claude/skills/`
   - Copy: `skills/openapi-generator/SKILL.md` → `.claude/skills/`
   - Copy: `skills/mock-server/SKILL.md` → `.claude/skills/`
   - Copy: `skills/webhook-tester/SKILL.md` → `.claude/skills/`
   - Copy: `skills/api-documentation/SKILL.md` → `.claude/skills/`

2. **Database Skills** (5 skills)
   - Copy: `skills/query-builder/SKILL.md` → `.claude/skills/`
   - Copy: `skills/schema-visualizer/SKILL.md` → `.claude/skills/`
   - Copy: `skills/migration-generator/SKILL.md` → `.claude/skills/`
   - Copy: `skills/seed-data-generator/SKILL.md` → `.claude/skills/`
   - Copy: `skills/query-optimizer/SKILL.md` → `.claude/skills/`

3. **Productivity Skills** (4 skills)
   - Copy: `skills/snippet-manager/SKILL.md` → `.claude/skills/`
   - Copy: `skills/search-enhancer/SKILL.md` → `.claude/skills/`
   - Copy: `skills/conflict-resolver/SKILL.md` → `.claude/skills/`
   - Copy: `skills/changelog-generator/SKILL.md` → `.claude/skills/`

4. **Data/Analytics Skills** (4 skills)
   - Copy: `skills/csv-processor/SKILL.md` → `.claude/skills/`
   - Copy: `skills/json-transformer/SKILL.md` → `.claude/skills/`
   - Copy: `skills/chart-generator/SKILL.md` → `.claude/skills/`
   - Copy: `skills/data-validator/SKILL.md` → `.claude/skills/`

5. **DevOps Skills** (2 skills)
   - Copy: `skills/log-analyzer/SKILL.md` → `.claude/skills/`
   - Copy: `skills/resource-monitor/SKILL.md` → `.claude/skills/`

6. **Language-Specific Skills** (3 skills)
   - Copy: `skills/go-mod-helper/SKILL.md` → `.claude/skills/`
   - Copy: `skills/rust-cargo-assistant/SKILL.md` → `.claude/skills/`
   - Copy: `skills/java-maven-helper/SKILL.md` → `.claude/skills/`

7. **Collaboration Skills** (5 skills)
   - Copy: `skills/pr-template-generator/SKILL.md` → `.claude/skills/`
   - Copy: `skills/code-explainer/SKILL.md` → `.claude/skills/`
   - Copy: `skills/onboarding-helper/SKILL.md` → `.claude/skills/`
   - Copy: `skills/meeting-notes/SKILL.md` → `.claude/skills/`
   - Copy: `skills/architecture-documenter/SKILL.md` → `.claude/skills/`

8. **New Commands**
   - Adapt: `commands/quality-check.md` → `.claude/commands/quality-check.md`
   - Adapt: `commands/pre-deploy.md` → `.claude/commands/pre-deploy.md`

### Phase 4: Enhancement & Integration (Priority: MEDIUM)

1. **Enhance Planning Skills**
   - Merge RFC-Lite format from planning-mastery
   - Add Blast Radius Mapping concept
   - Add Dependency Forecasting Mandate

2. **Enhance Clean Code Skills**
   - Merge 2025 standards from clean-code skill
   - Add supply chain security protocols
   - Add AI-era considerations

3. **Update CLAUDE.md**
   - Document new skills and agents
   - Add Ralph Wiggum usage instructions
   - Add brain system documentation

4. **Sync to OpenCode and Cursor**
   - Run `scripts/sync-ai-configs.sh` after merge
   - Update `.opencode/skill/` with new skills
   - Update `.cursor/skills/` with new skills

---

## Merge Statistics

| Category | Current Count | Adding | New Total |
|----------|--------------|--------|-----------|
| **Agents** | 18 | 5 | 23 |
| **Skills** | 90 | 35 | 125 |
| **Commands** | 45 | 2 | 47 |
| **Hooks** | 7 types | 3 scripts | Enhanced |

### New Asset Summary

**5 New Agents**:
1. akka-net-specialist
2. docfx-specialist
3. dotnet-concurrency-specialist
4. dotnet-performance-analyst
5. dotnet-benchmark-designer

**35 New Skills**:
1. ralph-wiggum (autonomous QA)
2. brain-memory (long-term memory)
3. akka-net-testing-patterns
4. akka-net-aspire-configuration
5. aspire-integration-testing
6. playwright-blazor-testing
7. ddd-hexagonal
8. data-persistence-patterns
9. bdd-testing
10. serena-mcp
11. api-tester
12. openapi-generator
13. mock-server
14. webhook-tester
15. api-documentation
16. query-builder
17. schema-visualizer
18. migration-generator
19. seed-data-generator
20. query-optimizer
21. snippet-manager
22. search-enhancer
23. conflict-resolver
24. changelog-generator
25. csv-processor
26. json-transformer
27. chart-generator
28. data-validator
29. log-analyzer
30. resource-monitor
31. go-mod-helper
32. rust-cargo-assistant
33. java-maven-helper
34. pr-template-generator
35. code-explainer
36. onboarding-helper
37. meeting-notes
38. architecture-documenter

**2 New Commands**:
1. /quality-check
2. /pre-deploy

**3 New Hook Scripts**:
1. session-start.js (tech stack detection)
2. brain-sync.js (memory persistence)
3. stop.js (Ralph Wiggum controller)

---

## Implementation Order

```
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: Critical Infrastructure (Ralph Wiggum + Brain)     │
│ ─────────────────────────────────────────────────────────── │
│ ✓ Copy Ralph Wiggum skill and scripts                       │
│ ✓ Copy Brain system hooks and libraries                     │
│ ✓ Enhance session-start hook                                │
│ ✓ Enhance git-worktrees skill                               │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: .NET Excellence (Akka.NET, Aspire, BDD)            │
│ ─────────────────────────────────────────────────────────── │
│ ✓ Copy 5 new .NET agents                                    │
│ ✓ Copy 6 new .NET skills                                    │
│ ✓ Copy 4 DDD/BDD skills from nesbo                          │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 3: DevOps & Productivity (devkit)                     │
│ ─────────────────────────────────────────────────────────── │
│ ✓ Copy 25 selected devkit skills                            │
│ ✓ Adapt 2 new commands                                      │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 4: Enhancement & Integration                          │
│ ─────────────────────────────────────────────────────────── │
│ ✓ Merge enhancements to existing skills                     │
│ ✓ Update CLAUDE.md documentation                            │
│ ✓ Sync to OpenCode and Cursor                               │
│ ✓ Run validation scripts                                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Hook conflicts | Test in isolated worktree first |
| Skill naming collisions | Use consistent `acf-` prefix for factory-specific skills |
| Performance impact from brain sync | Implement 30-second throttling (from maestro) |
| Breaking existing workflows | Maintain backward compatibility |

---

## Validation Checklist

After merge, run:
```bash
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/verify-ai-configs.sh
```

---

## Notes

1. **Ralph Wiggum** is the most valuable addition - provides autonomous QA with circuit breaker
2. **Brain System** enables cross-session context persistence
3. **.NET Aspire** skills fill a critical gap for modern .NET development
4. **BDD Testing** provides alternative testing methodology to complement TDD
5. **DevKit skills** add significant DevOps automation capability

---

**Approved by**: User (2026-01-25)
**Implementation Status**: ✅ COMPLETED (2026-01-25)

---

## Implementation Results

### Phase 1: Critical Infrastructure ✅
- Ralph Wiggum autonomous QA system - MERGED
- Brain/Long-term memory system - MERGED
- Session start enhancement hooks - MERGED
- Git worktrees enhancement skill - MERGED

### Phase 2: .NET Excellence ✅
- 5 new .NET agents - MERGED
- 10 .NET skills (6 from claude-code-dotnet, 4 from nesbo) - MERGED

### Phase 3: DevOps & Productivity ✅
- 28 DevKit skills - MERGED
- 2 new commands (quality-check, pre-deploy) - MERGED

### Phase 4: Enhancement & Integration ✅
- AI config sync completed
- OpenCode skills synced
- Cursor rules generated

### Final Statistics

| Category | Before | After | Change |
|----------|--------|-------|--------|
| Skills | ~90 | 131 | +41 |
| Agents | 18 | 23 | +5 |
| Commands | 45 | 47 | +2 |
| Hooks | 7 | Enhanced | +3 scripts |
