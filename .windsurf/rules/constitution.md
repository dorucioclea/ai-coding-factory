<!-- Define project principles and non-negotiables before any implementation. Use at project start to establish governance, quality standards, and architectural constraints. -->


# Project Constitution

## Overview

A constitution defines the **non-negotiable principles** that govern a project. It's created once at project inception and referenced throughout development to ensure consistency.

Think of it as a contract between all contributors (human and AI) about how the project will be built.

## When to Use

- Starting a new project (before any code)
- Onboarding to an existing project without clear principles
- After significant team/scope changes
- When there's confusion about "how we do things"

## The Constitution Document

Create at `docs/constitution.md` or `memory/constitution.md`:

```markdown
# {Project Name} Constitution

**Version**: 1.0.0
**Ratified**: {date}
**Last Amended**: {date}

## Core Principles

### I. {Principle Name}

{Description of the non-negotiable rule}

**Rationale**: {Why this matters}

**Enforcement**: {How violations are detected/prevented}

### II. {Principle Name}
...

## Quality Standards

### Code Quality
- {Standard 1}
- {Standard 2}

### Testing Requirements
- {Requirement 1}
- {Requirement 2}

### Documentation Requirements
- {Requirement 1}

## Architectural Constraints

### Technology Stack
| Layer | Choice | Rationale |
|-------|--------|-----------|
| Backend | .NET 8 | {why} |
| Frontend | Next.js 14 | {why} |
| Database | PostgreSQL | {why} |

### Patterns Required
- {Pattern 1}: {when to use}
- {Pattern 2}: {when to use}

### Patterns Prohibited
- {Anti-pattern 1}: {why prohibited}

## Governance

### Amendment Process
1. Propose change with rationale
2. Document impact on existing code
3. Get approval from {stakeholders}
4. Update version (semver)

### Compliance
- All PRs must verify constitution compliance
- Violations require explicit justification and approval
- Constitution supersedes all other documentation

---
**This constitution governs all development on this project.**
```

## Standard Principles to Consider

### For Enterprise Projects

```markdown
### I. Clean Architecture (NON-NEGOTIABLE)

Domain layer has ZERO external dependencies. Application depends only on Domain.
Infrastructure implements interfaces defined in Domain/Application.

**Rationale**: Testability, maintainability, technology independence

**Enforcement**: Architecture tests fail build if violated

### II. Test-Driven Development

Tests MUST be written before implementation. Red → Green → Refactor.

**Rationale**: Ensures testability, documents behavior, prevents regression

**Enforcement**:
- Minimum 80% coverage for Domain/Application
- PR blocked without tests for new features

### III. Immutability First

Prefer immutable data structures. Mutations require explicit justification.

**Rationale**: Predictable behavior, easier debugging, thread safety

**Enforcement**: Code review checklist item

### IV. Explicit Over Implicit

No magic. Configuration over convention. Explicit dependencies.

**Rationale**: Readability, debuggability, onboarding speed

**Enforcement**: Code review, no reflection-based DI
```

### For Startup/MVP Projects

```markdown
### I. Ship Small, Ship Fast

Every feature should be deployable within 1-2 days. If larger, break it down.

**Rationale**: Fast feedback, reduced risk, maintain momentum

**Enforcement**: Sprint planning, story point limits

### II. Good Enough > Perfect

80% solution shipped beats 100% solution planned. Iterate based on feedback.

**Rationale**: Learn from users, avoid over-engineering

**Enforcement**: Time-box features, avoid scope creep

### III. Pragmatic Testing

Test critical paths and edge cases. Don't test framework code.

**Rationale**: Balance coverage with velocity

**Enforcement**: Focus on integration tests for APIs, E2E for critical flows
```

## Creating a Constitution

### Interactive Flow

When user starts a new project, ask:

**Question 1: Project Type**

| Option | Description | Implies |
|--------|-------------|---------|
| A | Enterprise/Long-term | Strict architecture, high test coverage |
| B | Startup/MVP | Pragmatic, speed-focused |
| C | Personal/Learning | Minimal constraints |

**Question 2: Team Size**

| Option | Implications |
|--------|--------------|
| Solo | Less documentation, flexible |
| Small (2-5) | Clear conventions, some docs |
| Large (5+) | Strict governance, full docs |

**Question 3: Key Principles (pick 3-5)**

| Principle | Description |
|-----------|-------------|
| Clean Architecture | Layer separation, dependency rules |
| TDD | Tests before code |
| Immutability | No mutations |
| CQRS | Command/Query separation |
| Event Sourcing | Events as source of truth |
| Microservices | Service boundaries |
| Monolith First | Start simple, extract later |

### Output

Generate constitution based on answers, then:

1. Write to `docs/constitution.md`
2. Reference in project README
3. Create architecture tests to enforce (if applicable)

## Versioning

Use semantic versioning for the constitution:

- **MAJOR**: Breaking changes (principle removed, redefined)
- **MINOR**: New principle added, guidance expanded
- **PATCH**: Clarifications, typos, non-semantic changes

## Integration with Other Skills

- **spec-driven-development**: Reference constitution when validating specs
- **planning**: Check plan against constitution principles
- **code-review**: Verify changes comply with constitution

## Example Constitution

```markdown
# FishingSpots Constitution

**Version**: 1.0.0
**Ratified**: 2024-01-23
**Last Amended**: 2024-01-23

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)

The codebase follows Clean Architecture with strict layer separation:
- Domain: Zero dependencies, pure business logic
- Application: Orchestration, depends only on Domain
- Infrastructure: External concerns, implements Domain interfaces
- API: Composition root, HTTP concerns only

**Enforcement**: NetArchTest rules in CI pipeline

### II. Test-First Development

All features require tests written BEFORE implementation:
- Unit tests for Domain logic
- Integration tests for API endpoints
- E2E tests for critical user journeys

**Enforcement**: Minimum 80% coverage, PR requires test plan

### III. API-First Design

Backend APIs designed and documented before frontend implementation.
OpenAPI spec is the contract.

**Enforcement**: API spec required before implementation starts

## Quality Standards

### Code Quality
- No warnings in build
- All public APIs documented
- No TODO comments in main branch

### Testing
- Domain/Application: 80% minimum
- Critical paths: 100%
- E2E: Happy path + top 3 error cases

## Technology Stack

| Layer | Choice | Locked |
|-------|--------|--------|
| Backend | .NET 8 | Yes |
| Frontend | Next.js 14 | Yes |
| Database | PostgreSQL | Yes |
| Cache | Redis | No |
| Auth | JWT | Yes |

## Governance

Changes to this constitution require:
1. Written proposal with rationale
2. Impact assessment on existing code
3. Approval from project owner
4. Migration plan if breaking

---
This constitution governs all development on FishingSpots.
```

## Commands

```bash
# Create constitution interactively
/constitution create

# Validate code against constitution
/constitution check

# Propose amendment
/constitution amend "Add principle about logging"
```
