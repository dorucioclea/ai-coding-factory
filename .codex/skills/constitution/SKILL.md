---
name: constitution
description: Define project principles and non-negotiables before any implementation. Use at project start to establish governance, quality standards, and architectural constraints.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  phase: project-start
---

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

Tests MUST be written before implementation. Red -> Green -> Refactor.

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
