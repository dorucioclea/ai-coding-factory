# AI Config Sync

A CLI tool to synchronize AI coding assistant configurations across multiple systems including Claude Code, OpenCode, Cursor, Codex, and Windsurf.

## Features

- **Multi-system support**: Claude, OpenCode, Cursor, Codex, Windsurf (with extensible architecture for future systems)
- **Artifact synchronization**: Skills, agents, commands, hooks, rules, MCP servers
- **Smart transformations**: Automatically converts between formats (e.g., skills → Cursor .mdc rules)
- **Symlink support**: Efficient syncing with symbolic links where supported
- **SQLite state tracking**: Tracks sync state for incremental updates
- **Dry-run mode**: Preview changes before applying
- **Diff detection**: See what's out of sync before syncing

## Installation

```bash
cd tools/ai-config-sync
npm install
npm run build

# Link globally (optional)
npm link
```

## Usage

### Initialize a project

```bash
# Initialize all available systems
acs init

# Initialize specific systems
acs init --systems claude opencode cursor
```

### Check status

```bash
# Show status of all configured systems
acs status
```

### Show supported systems

```bash
# List available systems and their capabilities
acs systems

# Show all systems including unimplemented
acs systems --all
```

### Sync configurations

```bash
# Default: sync from Claude to OpenCode, Cursor, and Codex
acs sync

# Sync to specific targets
acs sync --source claude --targets opencode codex

# Sync specific artifact types
acs sync --types skill agent

# Dry run (preview changes)
acs sync --dry-run

# Force sync (ignore existing state)
acs sync --force

# Copy instead of symlink
acs sync --no-symlinks

# Verbose output
acs sync --verbose
```

### Compare systems (diff)

```bash
# Compare Claude to Codex
acs diff --source claude --target codex

# Compare specific artifact type
acs diff --source claude --target cursor --type skill
```

### View history

```bash
# Show recent sync jobs
acs history

# Show specific job details
acs history --job <job-id>

# Limit number of jobs
acs history --limit 5
```

### Specify project directory

```bash
# Use different project root
acs sync --project /path/to/project
```

## Supported Systems

| System | Skills | Agents | Commands | Hooks | Rules | MCP |
|--------|--------|--------|----------|-------|-------|-----|
| Claude | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| OpenCode | ✓ | ✓ | ✓ | ✗ | ✗ | ✓ |
| Codex | ✓ | ✓ | ✓ | ✗ | ✗ | ✓ |
| Cursor | ✗ | ✗ | ✗ | ✗ | ✓ | ✓ |
| Windsurf | ✗ | ✗ | ✗ | ✗ | ✓ | ✓ |

## Architecture

```
tools/ai-config-sync/
├── src/
│   ├── index.ts              # CLI entry point
│   ├── sync-engine.ts        # Core sync logic
│   ├── adapters/             # System adapters
│   │   ├── base.ts           # Base adapter interface
│   │   ├── claude.ts         # Claude Code adapter
│   │   ├── opencode.ts       # OpenCode adapter
│   │   ├── cursor.ts         # Cursor adapter
│   │   ├── codex.ts          # Codex adapter
│   │   └── windsurf.ts       # Windsurf adapter
│   ├── commands/             # CLI commands
│   │   ├── sync.ts
│   │   ├── status.ts
│   │   ├── diff.ts
│   │   ├── init.ts
│   │   ├── history.ts
│   │   └── systems.ts
│   ├── database/             # SQLite storage
│   │   ├── schema.ts
│   │   └── manager.ts
│   ├── models/               # Type definitions
│   │   └── types.ts
│   └── utils/                # Helper functions
│       └── helpers.ts
└── tests/                    # Test files
```

## Adding a New System

1. Create a new adapter in `src/adapters/`:

```typescript
import { BaseAdapter } from "./base.js";

export class NewSystemAdapter extends BaseAdapter {
  readonly systemId = "newsystem" as const;
  readonly name = "New System";
  readonly capabilities = { /* ... */ };
  readonly paths = { /* ... */ };

  // Implement required methods
}
```

2. Register in `src/adapters/index.ts`
3. Add system definition to `src/database/schema.ts`
4. Add mapping rules if needed

## Transformation Rules

When syncing between systems with different capabilities:

- **Claude skills → Cursor rules**: Converts SKILL.md to .mdc format with YAML frontmatter
- **Claude skills → Windsurf rules**: Converts to plain markdown rules
- **Claude skills → OpenCode**: Symlinks skill directories
- **Claude skills → Codex**: Copies skill directories with references/assets

## Database

Sync state is stored in `.ai-config-sync/ai-config-sync.db` (SQLite):

- **systems**: Registered AI systems
- **artifacts**: Tracked configuration artifacts
- **artifact_sync_state**: Sync state per artifact/target
- **sync_jobs**: Sync operation history
- **sync_results**: Individual sync results
- **mapping_rules**: Transformation rules
- **settings**: CLI configuration

## Development

```bash
# Run in development mode
npm run dev

# Build
npm run build

# Run tests
npm test

# Lint
npm run lint

# Format
npm run format
```

## License

MIT
