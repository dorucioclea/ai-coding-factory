# Governance & Traceability

## Traceability Chain

```
User Story (ACF-###)
    │
    ├── Acceptance Criteria
    │       │
    │       └── Tests [Trait("Story", "ACF-###")]
    │               │
    │               └── Commits (ACF-### message)
    │                       │
    │                       └── Release Notes (Story links)
```

Every deliverable must be traceable back to a user story.

## Story Management

### Story ID Format
- Pattern: `ACF-###` (e.g., ACF-001, ACF-042, ACF-100)
- Sequentially assigned
- Never reused

### Story File Location
```
artifacts/stories/ACF-###.md
```

### Story Template
```markdown
---
id: ACF-###
title: Short descriptive title
status: draft | ready | in-progress | review | done
created: YYYY-MM-DD
sprint: Sprint N | backlog
points: 1 | 2 | 3 | 5 | 8 | 13
priority: low | medium | high | critical
---

# ACF-### - Story Title

## User Story

As a **[persona]**,
I want to **[action]**,
So that **[benefit]**.

## Acceptance Criteria

- [ ] Given [context], when [action], then [expected result]
- [ ] Given [context], when [action], then [expected result]

## Technical Notes

- Affected components: [list]
- Dependencies: [list]
- API changes: [if any]

## Test Plan

| ID | Type | Description |
|----|------|-------------|
| TC-01 | Unit | Test description |
| TC-02 | Integration | Test description |

## Definition of Done

- [ ] All acceptance criteria met
- [ ] Tests passing with [Trait("Story", "ACF-###")]
- [ ] Coverage >= 80% for Domain/Application
- [ ] Code reviewed
- [ ] Documentation updated
- [ ] Security checklist completed
```

## Definition of Ready (DoR)

A story is ready for development when:

- [ ] User story follows "As a... I want... So that..." format
- [ ] Acceptance criteria are clear and testable
- [ ] Story is estimated (story points assigned)
- [ ] Dependencies identified and resolved
- [ ] Technical approach agreed
- [ ] No blocking questions remain

## Definition of Done (DoD)

A story is complete when:

- [ ] All acceptance criteria met
- [ ] Code implements the acceptance criteria
- [ ] Tests exist and are tagged with `[Trait("Story", "ACF-###")]`
- [ ] All tests pass in CI
- [ ] Code coverage >= 80% for Domain/Application layers
- [ ] Code reviewed and approved
- [ ] No critical or high security vulnerabilities
- [ ] Documentation updated if needed
- [ ] Traceability report includes story
- [ ] Commits prefixed with `ACF-###`

## Commit Message Format

```
ACF-### Brief description of change

- Detailed point 1
- Detailed point 2
- Detailed point 3

Co-Authored-By: AI Assistant <noreply@example.com>
```

### Examples

```
ACF-042 Implement order creation endpoint

- Add CreateOrderCommand and handler
- Add CreateOrderValidator for input validation
- Add integration tests for orders controller
- Update API documentation

Co-Authored-By: Claude <noreply@anthropic.com>
```

```
ACF-042 Add order creation tests

- Add unit tests for Order entity
- Add handler tests with mocked dependencies
- Tag all tests with [Trait("Story", "ACF-042")]

Co-Authored-By: Claude <noreply@anthropic.com>
```

## Test Tagging

All tests must be tagged with the story they verify:

```csharp
[Fact]
[Trait("Story", "ACF-042")]
[Trait("Category", "Unit")]
public void Order_Create_WithValidCustomer_Succeeds()
{
    // Test implementation
}
```

### Trait Categories
- `Story` - Required: Links to user story
- `Category` - Required: Unit, Integration, Architecture, E2E
- `Priority` - Optional: Critical, High, Medium, Low

## Traceability Validation

### Automated Checks

```bash
# Validate traceability
python3 scripts/traceability/traceability.py validate \
  --story-root artifacts/stories \
  --test-root tests

# Generate traceability report
python3 scripts/traceability/traceability.py report \
  --output artifacts/traceability/report.md
```

### Validation Rules

1. **Every story must have tests**
   - At least one test tagged with the story ID
   - Tests must pass

2. **Every commit must reference a story**
   - Commit message must start with `ACF-###`
   - Story must exist in `artifacts/stories/`

3. **Coverage requirements must be met**
   - Domain layer: >= 80%
   - Application layer: >= 80%

## Architecture Decision Records (ADRs)

### When to Create ADR

- Selecting a database, cache, or message queue
- Choosing authentication/authorization strategy
- Defining API versioning approach
- Making deployment decisions
- Any significant technical decision

### ADR Location
```
docs/architecture/adr/
├── ADR-001-use-postgresql.md
├── ADR-002-jwt-authentication.md
└── ADR-003-cqrs-pattern.md
```

### ADR Template

```markdown
# ADR-### Title

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
YYYY-MM-DD

## Context
What is the issue that we're seeing that is motivating this decision?

## Decision
What is the change that we're proposing and/or doing?

## Options Considered
1. Option A - Description
2. Option B - Description
3. Option C - Description

## Consequences
### Positive
- Benefit 1
- Benefit 2

### Negative
- Drawback 1
- Drawback 2

### Neutral
- Observation 1
```

## Release Process

### Pre-Release Checklist

- [ ] All stories in release are Done
- [ ] All tests pass in CI
- [ ] Coverage thresholds met
- [ ] Security scan completed
- [ ] Traceability report generated
- [ ] Release notes prepared
- [ ] Stakeholders notified

### Release Notes Format

```markdown
# Release vX.Y.Z

## Release Date
YYYY-MM-DD

## Summary
Brief description of what this release contains.

## Stories Included

### Features
- ACF-040: User registration
- ACF-041: Email verification
- ACF-042: Order creation

### Bug Fixes
- ACF-043: Fix login timeout issue

### Technical Debt
- ACF-044: Upgrade to .NET 8

## Breaking Changes
- List any breaking changes

## Migration Steps
1. Step 1
2. Step 2

## Known Issues
- Issue 1 (will be addressed in vX.Y.Z+1)
```

## Sprint Ceremonies

### Sprint Planning
- Review ready stories from backlog
- Team commits to sprint goal
- Stories assigned to team members

### Daily Standup
- What did I complete yesterday?
- What will I work on today?
- Any blockers?

### Sprint Review
- Demo completed stories
- Stakeholder feedback
- Update backlog

### Sprint Retrospective
- What went well?
- What could improve?
- Action items for next sprint

## Compliance Validation

```bash
# Run all validations
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh

# Check traceability
python3 scripts/traceability/traceability.py validate

# Verify coverage
python3 scripts/coverage/check-coverage.py coverage.xml
```
