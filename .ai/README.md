# AI Coding Factory - Unified AI Instructions

This directory contains tool-agnostic AI instructions that can be consumed by any AI coding assistant.

## Supported Tools

| Tool | Config Location | Status |
|------|-----------------|--------|
| Claude Code | `.claude/` | ✅ Full support |
| OpenCode | `.opencode/` | ✅ Full support |
| Cursor | `.cursorrules` | ✅ Full support |
| Codex CLI | `CODEX.md` | ✅ Full support |
| Aider | `.aider.conf.yml` | ✅ Full support |
| Continue | `.continue/` | ✅ Full support |

## Directory Structure

```
.ai/
├── README.md                 # This file
├── instructions/             # Shared instructions (tool-agnostic)
│   ├── CORE.md              # Core principles and rules
│   ├── architecture.md       # Clean Architecture guidelines
│   ├── patterns.md          # Code patterns and conventions
│   ├── testing.md           # Testing standards
│   ├── security.md          # Security guidelines
│   └── governance.md        # Governance and traceability
├── templates/               # Code generation templates
│   ├── entity.cs.template
│   ├── command-handler.cs.template
│   └── ...
├── workflows/               # Step-by-step workflows
│   ├── new-feature.md
│   ├── bug-fix.md
│   └── release.md
└── config/                  # Tool-specific adapters
    ├── claude.json          # Claude Code mapping
    ├── opencode.json        # OpenCode mapping
    ├── cursor.json          # Cursor mapping
    └── codex.json           # Codex CLI mapping
```

## How It Works

1. **Shared Instructions** (`.ai/instructions/`) contain the canonical knowledge
2. **Tool Adapters** (`.ai/config/`) map shared instructions to tool-specific formats
3. **Sync Script** (`scripts/sync-ai-tools.sh`) keeps all tools aligned

## Usage

### Manual Sync
```bash
./scripts/sync-ai-tools.sh
```

### Adding a New Tool
1. Create adapter in `.ai/config/{tool}.json`
2. Add generation logic to `scripts/sync-ai-tools.sh`
3. Run sync to generate tool-specific config

## Instruction Priority

When working with any AI tool in this repository:

1. **CORPORATE_RND_POLICY.md** - Always authoritative for governance
2. **CLAUDE.md / AGENTS.md** - Tool-specific entry points
3. **.ai/instructions/CORE.md** - Shared core principles
4. **Tool-specific configs** - Implementation details

## Contributing

To update AI instructions:

1. Edit the shared instruction in `.ai/instructions/`
2. Run `./scripts/sync-ai-tools.sh`
3. Commit all generated changes together
