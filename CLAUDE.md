# CLAUDE.md - AI Coding Factory

## What This Is

A "forge" for creating enterprise platforms - delivers project templates, automation scripts, and governance frameworks for .NET backend, React web, and React Native mobile.

## Quick Start

```bash
# Validate platform
./scripts/validate-project.sh

# Scaffold a project
./scripts/scaffold-and-verify.sh

# Check enforcement rules
python3 scripts/validate-enforcement-rules.py --phase development
```

## Project Structure

```
.claude/
├── agents/           # 41 specialized agents
├── commands/         # 47 slash commands
├── hooks/            # Automation hooks
├── rules/            # Behavioral rules (auto-loaded)
└── skills/           # 131+ reusable skills

config/
├── enforcement/rules.yaml   # Declarative governance
└── gates/*.md               # Phase transition checklists

templates/
├── clean-architecture-solution/  # .NET 8 backend
├── react-frontend-template/      # Next.js 14 web
├── react-native-template/        # Expo 52 mobile
└── infrastructure/               # Docker, PostgreSQL, Redis
```

## Non-Negotiables

1. **Story IDs**: Format `ACF-###` in commits, tests, stories
2. **Test Coverage**: ≥80% for Domain/Application layers
3. **No Secrets**: Never hardcode credentials
4. **Clean Architecture**: Domain has ZERO external dependencies
5. **Traceability**: Story → Test → Commit → Release chain

## Layer Dependencies

```
Domain ← Application ← Infrastructure ← API
  ↓           ↓              ↓            ↓
No deps    Domain only   Domain+App    All
```

## Commit Format

```
ACF-### Brief description

- Detail 1
- Detail 2

Co-Authored-By: Claude <noreply@anthropic.com>
```

## Key Files

| Purpose | Location |
|---------|----------|
| Governance | `CORPORATE_RND_POLICY.md` |
| Enforcement | `config/enforcement/rules.yaml` |
| Gates | `config/gates/*.md` |
| Stories | `artifacts/stories/ACF-###.md` |
| Agents | `.claude/agents/orchestrator.yaml` |

## Behavioral Rules

Rules in `.claude/rules/` are auto-loaded. Key rules:
- `orchestrator-activation.md` - Auto-routes tasks to agents
- `project-creation.md` - Auto-invokes spec-driven-development
- `coding-style.md` - Immutability, file organization
- `security.md` - Mandatory security checks

## See Also

- `.claude/rules/*.md` for detailed behavioral rules
- `CORPORATE_RND_POLICY.md` for governance policy
- `config/enforcement/rules.yaml` for declarative enforcement
