---
name: brain-memory
description: Long-term project memory system. Persists context across sessions using brain.jsonl for tech stack, decisions, goals, errors, and compact summaries.
allowed-tools: Read, Write, Edit, Bash
---

# BRAIN MEMORY: LONG-TERM PROJECT CONTEXT

> **Philosophy:** Context is king. Never lose important project decisions, errors, or progress across sessions.

The Brain Memory system provides **cross-session persistence** for project context. It stores and retrieves:

- Tech stack information (frameworks, dependencies, dev tools)
- Architecture patterns and key directories
- Project goals and decisions
- Completed tasks and errors
- Compact session summaries

---

## HOW IT WORKS

### Storage Location

All memory is stored in `.maestro/brain.jsonl` in the project root.

### Entry Types

| Type | Purpose |
|------|---------|
| `tech_stack` | Project name, frameworks, key dependencies, dev tools |
| `architecture` | Project type, patterns, key directories, entry points |
| `scripts` | Available npm/dotnet scripts |
| `compact` | Session summaries (auto-captured on context compaction) |
| `goal` | Project goals and objectives |
| `decision` | Key architectural and design decisions |
| `completed` | Completed tasks |
| `error` | Known issues and errors |

### JSONL Format

Each line is a JSON object:

```jsonl
{"type":"tech_stack","ts":"2026-01-25 10:00:00","project_name":"my-app","frameworks":["next.js","react"]}
{"type":"decision","ts":"2026-01-25 10:05:00","content":"Using Redux Toolkit for state management"}
{"type":"compact","ts":"2026-01-25 11:00:00","summary":"Implemented auth flow with JWT..."}
```

---

## AUTOMATIC FEATURES

### Session Start (via session-start.js hook)

1. **Tech Stack Detection:** Analyzes `package.json` or `*.csproj` files
2. **Structure Analysis:** Identifies architecture patterns and key directories
3. **Brain Injection:** Loads existing brain.jsonl into session context
4. **Compact Recovery:** Captures summaries from previous sessions

### After Tool Use (via brain-sync.js hook)

1. **Error Capture:** Extracts errors from tool results
2. **Decision Tracking:** Identifies key decisions in assistant messages
3. **Task Tracking:** Captures TodoWrite updates
4. **File Change Logging:** Tracks file edits and creations

### On Session Stop (via stop.js hook)

1. **Final Sync:** Ensures all context is persisted
2. **Ralph Integration:** Manages iteration state if Ralph Wiggum is active

---

## READING BRAIN CONTEXT

The brain is automatically loaded on session start. The formatted output includes:

```markdown
### Tech Stack
**Project:** my-app
**Frameworks:** next.js, react
**Key Dependencies:** State (Redux Toolkit), Styling (Tailwind)
**Dev Tools:** TypeScript, ESLint, Prettier
**Package Manager:** npm

### Architecture
**Patterns:** App Router (Next.js 13+), Clean Architecture
**Key Dirs:** app/, src/components/, src/hooks/
**Entry Points:** app/page.tsx

### Recent project history (Compacted)
**LATEST:** [2026-01-25 11:00:00] Implemented auth flow with JWT tokens...

### Key Decisions
- Using Redux Toolkit for state management because...
- Chose PostgreSQL for database due to...

### Completed
- Set up project structure
- Implemented authentication

### Known Issues/Errors
- [Bash] Build failed: Module not found...
```

---

## MANUAL INTERACTION

### View Brain Contents

```bash
cat .maestro/brain.jsonl | jq .
```

### Add a Decision

```bash
echo '{"type":"decision","ts":"'$(date -Iseconds)'","content":"Decided to use Prisma for ORM"}' >> .maestro/brain.jsonl
```

### Add a Goal

```bash
echo '{"type":"goal","ts":"'$(date -Iseconds)'","content":"Build MVP by end of sprint"}' >> .maestro/brain.jsonl
```

### Clear Brain (Start Fresh)

```bash
rm .maestro/brain.jsonl
```

---

## STALE CONTEXT GUARD

If a project directory is empty (only has `.git`, `.maestro`, `.claude`), the brain is automatically **purged** to prevent stale context from confusing new projects.

The session start hook will:
1. Detect empty project
2. Rename `brain.jsonl` to `brain.jsonl.stale.<timestamp>`
3. Inject "BLACK SLATE" context message

---

## INTEGRATION WITH OTHER SYSTEMS

### Ralph Wiggum

Brain memory stores:
- Iteration count and status
- Errors encountered during fixes
- Decisions made during debugging

### Planning Skills

Brain memory stores:
- Active plans and their status
- Implementation decisions
- Completed plan items

### Verification Loop

Brain memory tracks:
- Test results and coverage
- Build status
- Verification checkpoints

---

## BEST PRACTICES

### Do's

- Let the hooks manage brain.jsonl automatically
- Review brain context at session start
- Add important decisions manually if not auto-captured
- Keep goals updated as project evolves

### Don'ts

- Don't manually edit brain.jsonl unless necessary
- Don't commit brain.jsonl (add to .gitignore)
- Don't rely on brain for code - it's context only
- Don't store sensitive data in brain

---

## DEBUGGING

### Enable Debug Logging

```bash
export MAESTRO_DEBUG=1
```

This enables verbose logging to stderr for all hook operations.

### Common Issues

| Issue | Solution |
|-------|----------|
| Brain not loading | Check `.maestro/brain.jsonl` exists |
| Tech stack wrong | Delete `.maestro/.tech_hash` to force re-analysis |
| Stale context | Delete `brain.jsonl` and restart session |
| Sync not working | Check `MAESTRO_DEBUG=1` logs for errors |

---

## FILE STRUCTURE

```
.maestro/
├── brain.jsonl          # Long-term memory (JSONL format)
├── .tech_hash           # Hash of package.json for change detection
├── sync.state           # Brain sync offset tracking
├── brain-sync.state     # Last sync timestamp
├── ralph.state          # Ralph Wiggum iteration state
├── ralph.active         # Ralph active sentinel
└── ralph.complete       # Manual completion signal
```

---

## GITIGNORE RECOMMENDATION

Add to your `.gitignore`:

```gitignore
# AI Coding Factory memory
.maestro/
```

This ensures brain memory stays local and isn't committed to the repository.
