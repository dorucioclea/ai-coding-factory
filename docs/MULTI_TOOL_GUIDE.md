# Multi-Tool AI Assistant Guide

This guide explains how to use various AI coding assistants with the AI Coding Factory repository.

## Supported Tools

| Tool | Config Location | Primary Use Case |
|------|-----------------|------------------|
| **Claude Code** | `.claude/` | Full IDE-like experience in CLI |
| **OpenCode** | `.opencode/` | Agent-based development |
| **Cursor** | `.cursorrules` | IDE with AI integration |
| **Codex CLI** | `CODEX.md` | OpenAI-powered CLI |
| **Aider** | `.aider.conf.yml` | Git-aware pair programming |
| **Continue** | `.continue/` | VS Code/JetBrains extension |

## Quick Start by Tool

### Claude Code

```bash
# Install
npm install -g @anthropic-ai/claude-code

# Run in project root
claude

# Use slash commands
/validate       # Run validation scripts
/new-story      # Create user story
/implement      # Implement story
/code-review    # Review code
/security-review # Security audit
```

**Key Files:**
- `CLAUDE.md` - Main instructions
- `.claude/commands/` - 25 slash commands
- `.claude/settings.json` - Permissions and hooks

### OpenCode

```bash
# Install
npm install -g opencode-ai

# Run in project root
opencode

# Use agents
@developer     # Implementation
@qa            # Testing
@security      # Security review
@devops        # CI/CD
```

**Key Files:**
- `AGENTS.md` - Agent overview
- `.opencode/agent/` - 14 agents
- `.opencode/skill/` - 12 skills

### Cursor

1. Open project in Cursor IDE
2. Rules are automatically loaded from `.cursorrules`
3. Use Chat (Cmd+L) or Edit (Cmd+K)

**Key Files:**
- `.cursorrules` - Comprehensive rules file

### Codex CLI

```bash
# Install
npm install -g @openai/codex

# Run in project root
codex

# Reference CODEX.md for instructions
```

**Key Files:**
- `CODEX.md` - Instructions and patterns

### Aider

```bash
# Install
pip install aider-chat

# Run with Claude
aider --model claude-3-5-sonnet-20241022

# Run with GPT-4
aider --model gpt-4o
```

**Key Files:**
- `.aider.conf.yml` - Configuration and conventions

### Continue

1. Install Continue extension in VS Code or JetBrains
2. Config is loaded from `.continue/config.json`
3. Use custom commands: `/story`, `/implement`, `/review`, `/test`

**Key Files:**
- `.continue/config.json` - Models, commands, context

## Common Operations Across Tools

### Create User Story

| Tool | Command |
|------|---------|
| Claude Code | `/new-story` |
| OpenCode | `@product-owner create story` |
| Cursor | "Create a new user story following ACF-### format" |
| Codex | "create story" |
| Aider | Reference CODEX.md for story template |
| Continue | `/story` |

### Implement Feature

| Tool | Command |
|------|---------|
| Claude Code | `/implement ACF-###` |
| OpenCode | `@developer implement ACF-###` |
| Cursor | "Implement story ACF-###" |
| Codex | "implement ACF-###" |
| Aider | "implement the story ACF-###" |
| Continue | `/implement ACF-###` |

### Run Validation

| Tool | Command |
|------|---------|
| Claude Code | `/validate` |
| OpenCode | `@qa validate` |
| Cursor | "Run validation scripts" |
| Codex | "validate" |
| Aider | `/run ./scripts/validate-project.sh` |
| Continue | `/validate` |

### Code Review

| Tool | Command |
|------|---------|
| Claude Code | `/code-review` |
| OpenCode | `@net-code-reviewer` |
| Cursor | "Review this code" |
| Codex | "code-review" |
| Aider | "review this code for quality" |
| Continue | `/review` |

## Shared Instructions

All tools share a common source of truth in `.ai/instructions/`:

```
.ai/
├── instructions/
│   ├── CORE.md           # Core principles
│   ├── architecture.md   # Clean Architecture
│   ├── testing.md        # Testing standards
│   ├── security.md       # Security guidelines
│   └── governance.md     # Traceability rules
└── config/
    └── tool-mapping.json # Tool command mapping
```

## Keeping Tools in Sync

```bash
# Run sync script to verify all tools are aligned
./scripts/sync-ai-tools.sh
```

The sync script:
1. Verifies source files exist
2. Checks tool configs are present
3. Reports tool configuration status
4. Identifies manually managed files

## Rules That Apply to ALL Tools

### Story Traceability
- Format: `ACF-###` (e.g., ACF-001, ACF-042)
- Stories: `artifacts/stories/ACF-###.md`
- Tests: `[Trait("Story", "ACF-###")]`
- Commits: `ACF-### Description`

### Clean Architecture
```
Domain ← Application ← Infrastructure ← API
```

### .NET Standards
- Target .NET 8 LTS
- Async for I/O operations
- FluentValidation for inputs
- MediatR for CQRS

### Testing
- >80% coverage for Domain/Application
- Naming: `MethodName_Scenario_ExpectedBehavior`

### Security
- Never hardcode secrets
- Use parameterized queries
- Validate all inputs

## Choosing the Right Tool

| Scenario | Recommended Tool |
|----------|------------------|
| Full project development | Claude Code |
| Quick edits in IDE | Cursor |
| Complex multi-file changes | Aider |
| VS Code users | Continue |
| OpenAI preference | Codex CLI |
| Agent orchestration | OpenCode |

## Troubleshooting

### Tool Not Following Rules

1. Verify config files exist
2. Run `./scripts/sync-ai-tools.sh`
3. Check tool-specific entry point is loaded:
   - Claude Code: `CLAUDE.md`
   - OpenCode: `AGENTS.md`
   - Cursor: `.cursorrules`
   - Codex: `CODEX.md`

### Inconsistent Behavior

The `.ai/instructions/` directory contains the canonical rules. If a tool behaves differently:

1. Check if tool config is out of sync
2. Manually reference `.ai/instructions/CORE.md`
3. Report issue to update tool config

### Missing Commands

Each tool has different command capabilities:

- **Most commands**: Claude Code (25 slash commands)
- **Most agents**: OpenCode (14 agents + 12 skills)
- **IDE integration**: Cursor, Continue
- **Git integration**: Aider

## Contributing

To add support for a new tool:

1. Create adapter config in `.ai/config/`
2. Add tool section to `tool-mapping.json`
3. Create tool-specific config file
4. Update sync script
5. Document in this guide

## Environment Variables

Some tools require API keys:

```bash
# Claude/Anthropic
export ANTHROPIC_API_KEY=your-key

# OpenAI (Codex, Continue GPT models)
export OPENAI_API_KEY=your-key

# Mistral (Continue autocomplete)
export MISTRAL_API_KEY=your-key
```

---

*Last updated: January 2025*
