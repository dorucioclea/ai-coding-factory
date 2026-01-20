# ADR Templates

Pre-filled Architecture Decision Record templates for common technical decisions.

## Available Templates

| Template | Use Case |
|----------|----------|
| `database-choice.md` | Selecting database technology (PostgreSQL, SQL Server, MongoDB, Cosmos DB) |
| `authentication-method.md` | Authentication strategy (JWT, OAuth, Cookies, API Keys) |
| `caching-strategy.md` | Caching approach (Memory, Redis, Hybrid, HTTP caching) |
| `message-queue.md` | Message broker selection (RabbitMQ, Kafka, Service Bus, SQS) |
| `api-versioning.md` | API versioning strategy (URL, Query, Header, Evolutionary) |
| `deployment-strategy.md` | Deployment approach (Rolling, Blue-Green, Canary, Feature Flags) |

## How to Use

### Via /adr Command
```bash
# Creates ADR using appropriate template
/adr "Use PostgreSQL for order persistence" --template database-choice
```

### Manual Usage
1. Copy the relevant template
2. Replace `{{PLACEHOLDER}}` values
3. Fill in context and options
4. Document decision and consequences
5. Save to `docs/architecture/adr/ADR-###-title.md`

## Template Placeholders

Common placeholders used across templates:

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{{NUMBER}}` | ADR sequence number | 001, 002 |
| `{{DATE}}` | Decision date | 2025-01-15 |
| `{{STORY_ID}}` | Related story | ACF-042 |
| `{{PROJECT_NAME}}` | Project name | OrderService |
| `{{SELECTED_OPTION}}` | Chosen option | PostgreSQL |
| `{{REASON_N}}` | Decision rationale | Strong ACID compliance |
| `{{POSITIVE_N}}` | Positive consequences | Reliable transactions |
| `{{NEGATIVE_N}}` | Negative consequences | Scaling complexity |

## ADR Naming Convention

```
ADR-{NUMBER}-{kebab-case-title}.md
```

Examples:
- `ADR-001-use-postgresql-for-persistence.md`
- `ADR-002-implement-jwt-authentication.md`
- `ADR-003-adopt-redis-caching.md`

## ADR Lifecycle

```
Proposed → Accepted → [Deprecated] → [Superseded by ADR-XXX]
```

1. **Proposed**: Initial draft for review
2. **Accepted**: Team agreed on decision
3. **Deprecated**: No longer recommended but still in use
4. **Superseded**: Replaced by newer ADR

## Creating New Templates

When adding a new template:

1. Create `{decision-type}.md` in this directory
2. Include standard sections:
   - Status, Date, Story
   - Context with requirements
   - 3-4 options with pros/cons
   - Decision section
   - Consequences
   - Implementation notes
   - References

3. Use placeholder convention: `{{UPPER_SNAKE_CASE}}`

4. Add to this README's Available Templates table

## Related Commands

- `/adr` - Create new ADR
- `/adr list` - List all ADRs
- `/adr status` - Show ADR status summary
