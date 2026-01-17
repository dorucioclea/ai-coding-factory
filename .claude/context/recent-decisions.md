# Recent Architecture Decisions

Summary of recent ADRs and significant decisions for quick reference.

> **Note**: This file should be updated when new ADRs are created or significant architecture changes occur.

## Active Decisions

### ADR-001: Clean Architecture as Default
**Date**: Project inception
**Status**: Active
**Summary**: All generated projects follow Clean Architecture with strict layer dependencies.

**Key Points**:
- Domain layer has zero external dependencies
- Application layer depends only on Domain
- Infrastructure implements Domain interfaces
- API is the composition root

**Impact**: All code generation follows these patterns. Never introduce cross-layer dependencies.

---

### ADR-002: CQRS with MediatR
**Date**: Project inception
**Status**: Active
**Summary**: Use CQRS pattern with MediatR for command/query handling.

**Key Points**:
- Commands for write operations
- Queries for read operations
- MediatR for decoupling
- Pipeline behaviors for cross-cutting concerns

**Impact**: All use cases implemented as Command/Query handlers.

---

### ADR-003: Story ID Traceability
**Date**: Project inception
**Status**: Active
**Summary**: All work items use `ACF-###` format for end-to-end traceability.

**Key Points**:
- Story IDs in: stories, tests, commits, release notes
- Test trait format: `[Trait("Story", "ACF-###")]`
- Commit format: `ACF-### Description`
- Automated validation via traceability scripts

**Impact**: Every change must reference a story ID. No exceptions without waiver.

---

### ADR-004: Claude Code as Primary AI Assistant
**Date**: January 2025
**Status**: Active
**Summary**: Optimized platform for Claude Code with OpenCode legacy support.

**Key Points**:
- Primary configuration in `.claude/` directory
- Slash commands for common operations
- Hooks for automated validation
- Context files for pattern consistency

**Impact**: New development should use Claude Code commands and patterns.

---

## Pending Decisions

*No pending decisions at this time.*

---

## Superseded Decisions

*No superseded decisions yet.*

---

## Decision Process

When making a new architectural decision:

1. **Document with ADR**: Create `docs/architecture/adr/ADR-####-<title>.md`
2. **Update this file**: Add summary to Recent Decisions
3. **Get approval**: Product Owner + Security + Architecture owner
4. **Link to stories**: Reference related `ACF-###` IDs
5. **Communicate**: Update relevant documentation

## Templates

Use `/adr` command to create new Architecture Decision Records.

---

*Last updated: January 2025*
