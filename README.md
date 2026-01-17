<div align="center">

# AI Coding Factory

Enterprise-grade internal software delivery platform for private, traceable, and governed .NET development. Optimized for **Claude Code** with full support for OpenCode. Open source and GitHub-ready, with optional Azure DevOps support.

Repository: https://github.com/mitkox/ai-coding-factory

[Features](#features) | [Quick Start](#quick-start) | [Claude Code](#claude-code-integration) | [Governance](#governance-and-traceability) | [How To Verify](#how-to-verify)

</div>

---

## Overview

AI Coding Factory is an **AI coding forge** that transforms AI-assisted development into an **internal delivery platform** with auditable, automated, and governed software delivery. The platform enforces **quality, security, traceability, and governance by default**.

### Supported AI Coding Assistants

| Assistant | Status | Configuration |
|-----------|--------|---------------|
| **Claude Code** | Primary | `.claude/` directory, `CLAUDE.md` |
| OpenCode | Supported | `.opencode/` directory |

## Features

### Privacy-First AI Delivery
- Local inference with vLLM or LM Studio (OpenAI-compatible API)
- `.env.example` for configuration; no secrets in repo
- Air-gapped deployment guidance and least-privilege agent permissions

### Enterprise Governance and Traceability
- Definition of Done/Ready templates with explicit traceability rules
- Story -> Test -> Commit -> Release enforcement
- Automated traceability reports and release notes
- Governance policy covering branching, reviews, ownership, and risk

### Enterprise DevOps Choice (GitHub or Azure DevOps)
- GitHub Issues/Projects or Azure Boards for backlog and story tracking
- GitHub or Azure Repos for PRs and code review
- GitHub Actions or Azure Pipelines for CI quality gates and release readiness

### .NET 8+ Clean Architecture Templates
- Clean Architecture solution template with DDD/CQRS ready structure
- Microservice template with Kubernetes manifests
- Documentation and testing requirements baked in

### Agile-Native Delivery with AI Scrum Teams
- Scrum Team as Code: PO, Scrum Master, Dev, QA, Security, DevOps agents
- Sprint planning, execution, review, and retrospective workflows
- Traceability and quality gates enforced by agents

### Agents and Skills

**Stage agents**: `ideation`, `prototype`, `poc`, `pilot`, `product`  
**Scrum team agents**: `product-owner`, `scrum-master`, `developer`, `qa`, `security`, `devops`  
**Skills**: 12 reusable .NET skills in `.opencode/skill`

## Repository Map

```
ai-coding-factory/
├── .claude/                        # Claude Code configuration (primary)
│   ├── settings.json               # Permissions, hooks, environment
│   ├── commands/                   # Slash commands (/validate, /implement, etc.)
│   └── hooks/                      # Pre/post execution hooks
├── .opencode/                      # OpenCode configuration (legacy support)
│   ├── agent/                      # Agent definitions
│   ├── skill/                      # Reusable .NET skills
│   ├── plugin/                     # TypeScript plugins
│   └── templates/                  # Agile/Scrum templates
├── docs/                           # Governance, traceability, testing, Scrum
├── templates/                      # Clean Architecture + microservice templates
├── scripts/                        # Validation, traceability, scaffold verification
├── artifacts/                      # Traceability outputs (sample + generated)
├── CLAUDE.md                       # Claude Code instructions
├── AGENTS.md                       # Agent guidance (both tools)
├── azure-pipelines.yml             # Azure DevOps CI pipeline
├── .github/workflows/quality-gates.yml # GitHub Actions CI pipeline
└── README.md
```

## Claude Code Integration

Claude Code is the primary AI assistant for this platform. Configuration is in `.claude/` directory.

### Available Slash Commands

| Command | Description |
|---------|-------------|
| `/validate [scope]` | Run validation scripts (all, project, docs, policy, traceability) |
| `/new-story <title>` | Create INVEST-compliant user story |
| `/scaffold <name>` | Create new .NET project from template |
| `/implement <ACF-###>` | Full implementation workflow for a story |
| `/traceability` | Generate story-test-commit linkage report |
| `/security-review` | Perform security audit |
| `/code-review` | Perform code quality review |
| `/adr <title>` | Create Architecture Decision Record |
| `/release <version>` | Prepare release with all artifacts |
| `/sprint <action>` | Sprint management (plan, daily, review, retro) |

### Hooks

| Hook | Trigger | Purpose |
|------|---------|---------|
| `pre-write-validate.sh` | Before file writes | Block secrets, validate story IDs in tests |
| `post-test-traceability.sh` | After `dotnet test` | Validate story-test linkage |
| `post-commit-validate.sh` | After `git commit` | Verify story ID in commit message |

### Quick Start with Claude Code

```bash
# 1. Start Claude Code in the repository
claude

# 2. Read the instructions
# Claude Code automatically reads CLAUDE.md

# 3. Create your first story
/new-story User Authentication

# 4. Scaffold a new project
/scaffold AuthService

# 5. Implement the story
/implement ACF-001

# 6. Validate everything
/validate
```

## Governance and Traceability

- Corporate R&D Development Policy (authoritative): `CORPORATE_RND_POLICY.md`
- Governance policy: `docs/governance/GOVERNANCE.md`
- Traceability model: `docs/traceability/TRACEABILITY.md`
- Scrum Team as Code: `docs/agile/SCRUM-TEAM-AS-CODE.md`
- Testing strategy: `docs/testing/TESTING-STRATEGY.md`
- Documentation requirements: `docs/documentation/DOCUMENTATION-REQUIREMENTS.md`

## Quick Start

### Prerequisites

- .NET 8 SDK
- Python 3
- Docker (for container verification)
- Local inference server (vLLM or LM Studio)
- GitHub or Azure DevOps project (Issues/Boards, Repos, Actions/Pipelines)

### 0) Clone the Repository (GitHub)

```bash
git clone https://github.com/mitkox/ai-coding-factory.git
cd ai-coding-factory
```

### 1) Configure Local Inference

Update `.opencode/opencode.json` or set values in `.env.example`:

```json
{
  "provider": {
    "local-inference": {
      "type": "openai-compatible",
      "baseUrl": "http://localhost:8000/v1",
      "apiKey": "your-api-key"
    }
  }
}
```

### 2) Start Inference Server

**vLLM:**
```bash
vllm serve GLM-4.7 --dtype auto --api-key your-api-key
```

**LM Studio:**
- Load a model
- Enable OpenAI-compatible server

### 3) Run Claude Code (Recommended)

```bash
# Navigate to repository
cd ai-coding-factory

# Start Claude Code
claude

# Use built-in slash commands
/validate              # Run all validation scripts
/new-story Login Page  # Create a new user story
/scaffold MyProject    # Create new .NET project
/implement ACF-001     # Implement a story
/traceability          # Generate traceability report
/security-review       # Perform security review
/release 1.0.0         # Prepare release
```

### 3-Alt) Run OpenCode (Alternative)

```bash
opencode
/agent product-owner
```

### 4) Connect to GitHub or Azure DevOps

- Create a GitHub or Azure DevOps project
- Use GitHub Issues/Projects or Azure Boards for story tracking
- Configure GitHub Actions or Azure Pipelines to run CI checks

## How To Verify

Run the full lifecycle verification (build, test, coverage, traceability, container):

```bash
chmod +x scripts/scaffold-and-verify.sh
./scripts/scaffold-and-verify.sh
```

Additional checks:

```bash
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh
python3 scripts/traceability/traceability.py validate --commit-range origin/main..HEAD
```

If you use Azure DevOps, the same checks run in `azure-pipelines.yml`. For GitHub, use `.github/workflows/quality-gates.yml`.

## Security and Offline Guidance

- No secrets committed; use `.env` locally
- Inference endpoints should bind to localhost or a protected subnet
- Use least-privilege permissions in `.opencode/opencode.json`
- Air-gapped use: mirror dependencies and disable external MCP integrations

## License

MIT License. See `LICENSE`.

## Made with ❤️

Made with ❤️ by Mitko X.
