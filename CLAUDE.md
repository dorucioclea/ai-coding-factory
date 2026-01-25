# CLAUDE.md - AI Coding Factory Instructions for Claude Code

This file provides instructions and context for Claude Code when working in this repository.

## Repository Overview

AI Coding Factory is an enterprise software delivery platform that uses AI-assisted development to automate the complete development lifecycle. It supports **full-stack development** with .NET backend, React web frontend, and React Native mobile applications. It enforces strict governance, traceability, security, and quality standards while keeping all code and inference local.

**Key Principle**: This is a "forge" for creating platforms - not a traditional application codebase. The deliverables are project templates, automation scripts, and governance frameworks that generate production-ready applications across all platforms (backend, web, mobile).

## Quick Reference

### Project Structure
```
.claude/                    # Claude Code configuration
├── settings.json           # Claude Code settings
├── commands/               # Custom slash commands
└── hooks/                  # Pre/post execution hooks

.opencode/                  # OpenCode configuration (legacy, being migrated)
├── agent/                  # Agent definitions (14 agents)
├── skill/                  # Reusable skills (12 skills)
├── plugin/                 # TypeScript plugins (3 plugins)
├── templates/              # Agile/Scrum templates
└── prompts/                # Shared instructions

templates/                  # Project boilerplates
├── clean-architecture-solution/  # .NET 8 Clean Architecture backend
├── react-frontend-template/      # Next.js 14 web frontend
├── react-native-template/        # Expo SDK 52 mobile app
├── microservice-template/        # Lightweight .NET microservice
└── infrastructure/               # Docker, PostgreSQL, Redis

scripts/                    # Automation and validation
├── validate-project.sh
├── validate-documentation.sh
├── validate-rnd-policy.sh
├── scaffold-and-verify.sh
├── traceability/           # Story-test-commit linkage
└── coverage/               # Code coverage validation

docs/                       # Governance and technical docs
artifacts/                  # Traceability outputs
projects/                   # User workspace (gitignored)
```

### Essential Commands
```bash
# Validate the platform repository
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh

# Validate traceability
python3 scripts/traceability/traceability.py validate

# Scaffold and verify template
./scripts/scaffold-and-verify.sh

# .NET project commands (in generated projects)
dotnet restore && dotnet build && dotnet test
```

## Mission Non-Negotiables

1. **Privacy-First**: All inference is local. Never require external AI services.
2. **Enterprise .NET**: Target .NET 8+ with Clean Architecture.
3. **Container-First**: Everything Kubernetes-ready from day one.
4. **Traceability**: Enforce story → test → commit → release chain.
5. **Verifiable**: Every claim must be backed by scripts/CI.
6. **Security by Default**: No secrets in code, least privilege, input validation.

## Governance Policy

**IMPORTANT**: Always follow `CORPORATE_RND_POLICY.md` as the authoritative governance document.

### Required Before Any Work
- Read this file and `CORPORATE_RND_POLICY.md`
- Understand the stage you're working in (Ideation/Development/Validation/Release/Maintenance)
- Know your exit criteria before starting

### Story ID Convention
- Format: `ACF-###` (e.g., `ACF-001`, `ACF-042`)
- Story IDs MUST appear in:
  - Story files: `artifacts/stories/ACF-###.md`
  - Test traits: `[Trait("Story", "ACF-###")]`
  - Commit messages: `ACF-### Description of change`
  - Release notes: Linked story references

### Definition of Done (DoD)
A story is complete when:
- [ ] Acceptance criteria met and linked to `ACF-###`
- [ ] Tests exist and pass in CI
- [ ] Coverage >=80% for Domain/Application layers
- [ ] Traceability report includes story and tests
- [ ] Documentation updated
- [ ] Security/privacy checks completed
- [ ] No high/critical vulnerabilities

## Agent System (Task-based)

Use Claude Code's Task tool to spawn specialized agents for different work:

### Stage Agents
| Agent | Use For | Spawn Command |
|-------|---------|---------------|
| `ideation` | Requirements, epics, architecture | Research and planning tasks |
| `prototype` | Rapid MVP development | Quick implementation without tests |
| `poc` | Proof of concept with security | Security-focused implementation |
| `pilot` | Production-ready with tests/CI | Full implementation with quality |
| `product` | Monitoring, scaling, maintenance | Production operations |

### Scrum Team Agents
| Agent | Responsibility |
|-------|----------------|
| `product-owner` | Story assignment, acceptance criteria, backlog |
| `scrum-master` | DoR/DoD enforcement, traceability compliance |
| `developer` | Implementation within story scope |
| `qa` | Test linkage verification, coverage enforcement |
| `security` | Threat modeling, security reviews |
| `devops` | CI/CD, release readiness, deployment |

### Specialized Subagents
| Agent | Purpose |
|-------|---------|
| `net-code-reviewer` | Code quality review |
| `net-test-generator` | Test case generation |
| `net-security-auditor` | Security vulnerability scanning |

### React Native Agents
| Agent | Purpose |
|-------|---------|
| `rn-developer` | Screen/component implementation |
| `rn-navigator` | Navigation architecture, deep linking |
| `rn-state-architect` | Redux/TanStack Query state design |
| `rn-performance-guardian` | Performance optimization |
| `rn-observability-integrator` | Sentry instrumentation |
| `rn-design-token-guardian` | Design system compliance |
| `rn-a11y-enforcer` | WCAG accessibility validation |
| `rn-test-generator` | Jest, Detox, Maestro tests |
| `rn-grand-architect` | Complex feature orchestration (opus)

## Skills Reference

### .NET Development Skills
- `net-web-api` - ASP.NET Core Web API scaffolding
- `net-domain-model` - DDD entities, value objects, aggregates
- `net-repository-pattern` - Repository and Unit of Work
- `net-jwt-auth` - JWT authentication/authorization
- `net-cqrs` - CQRS with MediatR
- `net-testing` - xUnit, Moq, TestContainers, Coverlet
- `net-docker` - Dockerfile and docker-compose
- `net-github-actions` - GitHub Actions CI/CD
- `net-kubernetes` - Kubernetes deployment manifests
- `net-observability` - Serilog, tracing, metrics

### Agile/Scrum Skills
- `net-agile` - Agile practices and artifacts
- `net-scrum` - Scrum ceremonies and roles

### React Native Skills
- `rn-fundamentals` - Core RN components, styling, Expo
- `rn-navigation` - Expo Router, deep linking, protected routes
- `rn-state-management` - Redux Toolkit, TanStack Query, persistence
- `rn-api-integration` - Axios, TanStack Query hooks, error handling
- `rn-auth-integration` - JWT auth, secure storage, token refresh
- `rn-observability-setup` - Sentry SDK, source maps, EAS integration
- `rn-crash-instrumentation` - Error boundaries, native crash capture
- `rn-performance-monitoring` - Screen load tracking, network tracing
- `rn-design-system-foundation` - Design tokens, theming, dark mode
- `rn-design-preset-system` - Minimalist Modern, Glass, Brutalist presets
- `rn-animations` - Reanimated 3, gesture handling
- `rn-testing` - Jest, RTL, Detox, Maestro E2E
- `rn-deployment` - EAS Build, App Store/Play Store, OTA updates
- `rn-native-modules` - JSI, Turbo Modules, native bridging

## Clean Architecture Rules

### Layer Dependencies (CRITICAL)
```
Domain ← Application ← Infrastructure ← API
  ↓           ↓              ↓            ↓
No deps    Domain only   Domain+App    All
```

**Rules**:
- Domain has ZERO external dependencies
- Application depends ONLY on Domain
- Infrastructure depends on Domain + Application
- API is the composition root

### Layer Contents
1. **Domain**: Entities, Value Objects, Aggregates, Domain Events, Repository Interfaces
2. **Application**: Commands/Queries (CQRS), DTOs, Validators, Behaviors
3. **Infrastructure**: EF Core, Repository Implementations, External Services
4. **API**: Controllers, Middleware, Filters, DI Configuration

## Testing Standards

### Test Pyramid
- **Unit Tests**: >80% coverage for Domain/Application layers
- **Integration Tests**: All critical API endpoints
- **Architecture Tests**: Validate dependency rules

### Test Naming Convention
```csharp
[Fact]
[Trait("Story", "ACF-042")]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange, Act, Assert
}
```

### Test Frameworks
- xUnit (test runner)
- FluentAssertions (readable assertions)
- Moq (mocking)
- TestContainers (real database for integration)
- Coverlet (coverage reporting)
- NetArchTest (architecture validation)

## Security Guidelines

### Always Do
- Use parameterized queries (EF Core only)
- Validate all inputs (FluentValidation)
- Use IConfiguration for settings
- Implement proper authentication (JWT)
- Log with ILogger (structured logging)
- Use HTTPS in production

### Never Do
- Hardcode secrets, tokens, or keys
- Use raw SQL strings
- Trust user input without validation
- Use Console.WriteLine for logging
- Commit .env files (use .env.example)
- Disable security features

## .NET Standards

```csharp
// Target .NET 8 LTS
<TargetFramework>net8.0</TargetFramework>

// Async for all I/O
public async Task<Result> ProcessAsync(CancellationToken ct)

// Specific exceptions
throw new InvalidOperationException("State invalid");

// Structured logging
_logger.LogInformation("Processing {OrderId}", order.Id);

// Constructor injection only
public class Service(IRepository repo, ILogger<Service> logger)

// Proper resource disposal
await using var stream = File.OpenRead(path);

// String interpolation
var message = $"User {userId} logged in at {timestamp}";
```

## Validation Checklist

Before completing any significant work, run:

```bash
# 1. Validate platform structure
./scripts/validate-project.sh

# 2. Validate documentation
./scripts/validate-documentation.sh

# 3. Validate R&D policy compliance
./scripts/validate-rnd-policy.sh

# 4. Validate traceability (if stories/tests changed)
python3 scripts/traceability/traceability.py validate \
  --story-root artifacts/stories \
  --test-root tests

# 5. Check coverage (in generated projects)
python3 scripts/coverage/check-coverage.py coverage.xml
```

## Commit Message Format

```
ACF-### Brief description of change

- Detailed bullet point 1
- Detailed bullet point 2

Co-Authored-By: Claude <noreply@anthropic.com>
```

## When Working on This Repository

### Adding New Agents
1. Create agent file in `.claude/commands/` (for Claude Code) or document in agent instructions
2. Define: purpose, allowed tools, temperature guidance, required outputs
3. Update this file and ARCHITECTURE.md

### Adding New Skills
1. Create skill documentation with usage examples
2. Include: what it does, when to use, what it creates
3. Test skill outputs manually

### Modifying Templates
1. Update template files in `templates/`
2. Run `./scripts/scaffold-and-verify.sh` to validate
3. Ensure build, test, coverage, traceability all pass

### Changing Governance
1. Update `CORPORATE_RND_POLICY.md`
2. Update validation scripts if enforcement changes
3. Update this file for quick reference
4. Ensure all agents/skills reference updated policy

## File Location Conventions

| Artifact | Location |
|----------|----------|
| Stories | `artifacts/stories/ACF-###.md` |
| ADRs | `docs/architecture/adr/` |
| Traceability reports | `artifacts/traceability/` |
| Release notes | `artifacts/traceability/release-notes.md` |
| Security checklists | `.opencode/templates/artifacts/` |
| Agile templates | `.opencode/templates/agile/` |
| Role playbooks | `.opencode/templates/roles/` |

## Troubleshooting

### Validation Failures
```bash
# Check JSON syntax
cat .claude/settings.json | python3 -m json.tool

# List all Claude commands
ls -la .claude/commands/

# Verify scripts are executable
chmod +x scripts/*.sh
```

### Generated Project Issues
```bash
# Clean and rebuild
dotnet clean && dotnet restore && dotnet build

# Check .NET SDK version (requires 8.0+)
dotnet --version

# Run specific test project
dotnet test tests/ProjectName.UnitTests -v detailed
```

## Mobile Development (React Native)

### Template Location
```
templates/react-native-template/
```

### Mobile Commands
| Command | Purpose |
|---------|---------|
| `/add-mobile-screen` | Scaffold new Expo Router screen |
| `/add-mobile-service` | Create API service with TanStack Query |
| `/add-mobile-auth` | Add JWT authentication |
| `/add-mobile-observability` | Configure Sentry instrumentation |
| `/mobile-e2e` | Generate Maestro/Detox E2E tests |
| `/mobile-deploy` | Build and deploy with EAS |

### Mobile Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                    Full Stack Platform                       │
├─────────────────────────────────────────────────────────────┤
│  .NET Backend        │  Web Frontend      │  Mobile         │
│  (Clean Arch)        │  (Next.js 14)      │  (Expo 52)      │
│  ─────────────       │  ─────────────     │  ─────────────  │
│  MediatR CQRS        │  TanStack Query    │  TanStack Query │
│  JWT Auth            │  Zustand           │  Redux Toolkit  │
│  Serilog             │  shadcn/ui         │  Sentry SDK     │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              │       Shared Patterns         │
              │  - JWT Auth Flow              │
              │  - API Client (Axios)         │
              │  - TypeScript Types           │
              │  - Error Handling             │
              └───────────────────────────────┘
```

### Mobile Key Decisions
- **Auth**: Unified JWT with expo-secure-store
- **Design**: Minimalist Modern preset
- **Observability**: Full Sentry integration
- **API**: TanStack Query + Axios (matches web)

### Context7 Integration
When uncertain about React Native patterns, use Context7 MCP:
```typescript
// Resolve library
mcp__plugin_context7_context7__resolve-library-id({ libraryName: "expo-router" })

// Query documentation
mcp__plugin_context7_context7__query-docs({ libraryId: "/expo/expo-router", query: "protected routes" })
```

## Remember

- **Quality over speed**: Trade-offs require ADRs
- **Test-first**: Every behavior change needs tests
- **Traceability**: Story IDs everywhere
- **No hallucinations**: Don't claim without evidence
- **Smallest viable change**: Avoid unnecessary refactors
- **Enterprise audit**: If it can't be verified, it doesn't ship
- **Cross-platform consistency**: Use shared patterns across backend, web, and mobile
