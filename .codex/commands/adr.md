# /adr - Create Architecture Decision Record

Create a new ADR (Architecture Decision Record) for documenting significant architectural decisions.

## Usage
```
/adr <title>
```

## When to Create an ADR

Per Corporate R&D Policy, ADRs are **mandatory** when:
- Introducing new services, databases, or external dependencies
- Modifying Clean Architecture boundaries or core domain models
- Changing security model, authentication, or authorization
- Creating breaking changes or incompatible schema migrations
- Adding new infrastructure components (message queues, caches, etc.)
- Changing deployment architecture

## Instructions

When invoked:

1. **Determine ADR number**:
   - List existing ADRs in `docs/architecture/adr/`
   - Find highest numbered ADR and increment
   - Format: `ADR-####` (4 digits, zero-padded)

2. **Create ADR file** at `docs/architecture/adr/ADR-####-<slug>.md`:

```markdown
# ADR-#### - <Title>

## Status
Proposed | Accepted | Deprecated | Superseded by ADR-####

## Date
<YYYY-MM-DD>

## Context
<What is the issue that we're seeing that is motivating this decision or change?>

## Decision
<What is the change that we're proposing and/or doing?>

## Consequences

### Positive
- <benefit 1>
- <benefit 2>

### Negative
- <trade-off 1>
- <trade-off 2>

### Neutral
- <neutral observation>

## Alternatives Considered

### Alternative 1: <Name>
**Description**: <brief description>
**Pros**: <advantages>
**Cons**: <disadvantages>
**Why not chosen**: <reasoning>

### Alternative 2: <Name>
...

## Implementation

### Required Changes
- [ ] <change 1>
- [ ] <change 2>

### Affected Components
- <component 1>
- <component 2>

### Migration Plan
<If applicable, how do we transition from current to new state?>

## Security Considerations
<Security implications of this decision>

## Related
- Stories: ACF-###, ACF-###
- Previous ADRs: ADR-####
- External docs: <links>

## Approval

| Role | Name | Date | Decision |
|------|------|------|----------|
| Product Owner | | | Pending |
| Security | | | Pending |
| Architecture | | | Pending |
```

3. **Prompt for required information**:
   - What problem are we solving?
   - What alternatives were considered?
   - What are the trade-offs?
   - What story IDs are related?

4. **Remind about approval workflow**:
   - ADRs require approval from: Product Owner + Security + Architecture owner
   - Status should be "Proposed" until approved
   - Link to related stories

## ADR Status Lifecycle

```
Proposed → Accepted → [Superseded | Deprecated]
```

- **Proposed**: Initial draft, under review
- **Accepted**: Approved by required stakeholders
- **Superseded**: Replaced by a newer ADR (reference it)
- **Deprecated**: No longer applicable

## Example

```
User: /adr Add Redis Caching Layer

Claude: Creating ADR for Redis caching...

I need some information:

1. **Context**: What performance or scalability issue are we addressing?
2. **Alternatives**: What other caching solutions were considered? (e.g., in-memory, distributed cache, CDN)
3. **Related stories**: Which ACF-### stories does this support?

[After gathering info]

Created: docs/architecture/adr/ADR-0012-redis-caching-layer.md

ADR-0012: Add Redis Caching Layer
Status: Proposed

Next steps:
1. Review the ADR for completeness
2. Get approval from Product Owner, Security, Architecture
3. Update status to "Accepted" once approved
4. Link to implementation stories
```

## ADR Directory Structure

```
docs/architecture/adr/
├── README.md                           # Index of all ADRs
├── ADR-0001-clean-architecture.md      # Initial architecture
├── ADR-0002-jwt-authentication.md      # Auth decision
├── ADR-0003-postgresql-database.md     # Database choice
└── ADR-####-<title-slug>.md            # New ADRs
```

## Index File

Keep `docs/architecture/adr/README.md` updated:

```markdown
# Architecture Decision Records

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-0001](ADR-0001-clean-architecture.md) | Clean Architecture | Accepted | 2024-01-15 |
| [ADR-0002](ADR-0002-jwt-authentication.md) | JWT Authentication | Accepted | 2024-01-20 |
```
