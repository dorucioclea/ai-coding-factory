# Quick Actions Reference

Fast reference for common Claude Code operations in AI Coding Factory.

---

## Development Workflows

### Start New Feature
```bash
# 1. Create story
/new-story

# 2. Implement with full workflow
/implement ACF-###
```

### Add Domain Entity
```bash
# Simple entity
/add-entity Order

# Entity with repository and events
/add-entity Order --with-repository --with-events
```

### Add API Endpoint
```bash
# CRUD endpoint
/add-endpoint Orders create

# With full CQRS
/add-endpoint Orders create --cqrs
```

### Generate Tests
```bash
# Unit tests for a class
/add-test OrderService --type unit

# Integration tests
/add-test OrdersController --type integration

# Architecture tests
/add-test --type architecture
```

---

## Quality Checks

### Run All Validations
```bash
/validate
```

### Check Code Coverage
```bash
# Full coverage report
/coverage

# Specific project
/coverage OrderService.Domain
```

### Security Review
```bash
/security-review
```

### Code Quality Review
```bash
/code-review
```

### Traceability Report
```bash
/traceability
```

---

## Infrastructure

### Add Docker Support
```bash
/add-docker
```

### Add CI/CD Pipeline
```bash
/add-cicd --provider github --deploy azure
```

### Add Kubernetes Config
```bash
/add-k8s --helm --hpa --ingress nginx
```

### Add Observability
```bash
/add-observability --logging serilog --tracing otlp --metrics
```

### Add Authentication
```bash
/add-auth --type jwt --refresh-tokens
```

---

## Architecture

### Create ADR
```bash
/adr "Use PostgreSQL for persistence"
```

### View Dependencies
```bash
/dependencies
```

### Scaffold New Project
```bash
/scaffold OrderService --template clean-architecture
```

---

## Sprint Management

### Sprint Planning
```bash
/sprint plan
```

### Daily Standup
```bash
/sprint daily
```

### Sprint Review
```bash
/sprint review
```

### Retrospective
```bash
/sprint retro
```

---

## Release

### Prepare Release
```bash
/release v1.0.0
```

---

## Common .NET Commands

### Build & Test
```bash
dotnet restore
dotnet build
dotnet test
```

### Run with Watch
```bash
dotnet watch run --project src/ProjectName.API
```

### Add Package
```bash
dotnet add package PackageName
```

### EF Core Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName -p src/ProjectName.Infrastructure -s src/ProjectName.API

# Update database
dotnet ef database update -p src/ProjectName.Infrastructure -s src/ProjectName.API
```

### Generate Coverage
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

---

## Git Operations

### Feature Branch
```bash
git checkout -b feature/ACF-###-description
```

### Commit with Story ID
```bash
git commit -m "ACF-### Brief description"
```

### Rebase on Main
```bash
git fetch origin
git rebase origin/main
```

---

## File Locations Quick Reference

| What | Where |
|------|-------|
| Stories | `artifacts/stories/ACF-###.md` |
| ADRs | `docs/architecture/adr/` |
| Templates | `templates/` |
| Scripts | `scripts/` |
| Traceability | `artifacts/traceability/` |
| Test Templates | `.claude/templates/` |
| Context Files | `.claude/context/` |

---

## Story ID Format

Always use `ACF-###` format:
- In story files: `artifacts/stories/ACF-042.md`
- In tests: `[Trait("Story", "ACF-042")]`
- In commits: `ACF-042 Add order validation`
- In branches: `feature/ACF-042-order-validation`

---

## Coverage Targets

| Layer | Minimum |
|-------|---------|
| Domain | 80% |
| Application | 80% |
| Infrastructure | 60% |
| API | 50% |

---

## Keyboard Shortcuts (Claude Code)

| Action | Shortcut |
|--------|----------|
| Accept suggestion | `Tab` |
| Reject suggestion | `Esc` |
| Open command palette | `Cmd/Ctrl + Shift + P` |
| Toggle sidebar | `Cmd/Ctrl + B` |

---

*Tip: Run `/validate` before every commit to catch issues early.*
