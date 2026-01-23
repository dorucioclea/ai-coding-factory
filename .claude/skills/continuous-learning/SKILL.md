---
name: continuous-learning
description: Auto-extract patterns and learnings from sessions to build institutional knowledge and improve future development
---

# Continuous Learning Skill

This skill enables automatic extraction of patterns, insights, and learnings from development sessions to build reusable knowledge.

## When to Activate

- After completing a significant implementation
- When solving a complex debugging problem
- After discovering a new pattern or anti-pattern
- During code review sessions
- When encountering and resolving errors

## Core Concept: Learning Extraction Loop

```
┌─────────────────────────────────────────────────────────┐
│            CONTINUOUS LEARNING LOOP                     │
│                                                         │
│  ┌─────────┐   ┌───────────┐   ┌──────────┐            │
│  │ SESSION │──>│  EXTRACT  │──>│  STORE   │            │
│  └─────────┘   │ PATTERNS  │   │ KNOWLEDGE│            │
│                └───────────┘   └──────────┘            │
│                      │               │                  │
│                      v               v                  │
│               ┌───────────┐   ┌──────────┐             │
│               │ VALIDATE  │   │  APPLY   │             │
│               │  PATTERN  │   │  NEXT    │             │
│               └───────────┘   │ SESSION  │             │
│                               └──────────┘             │
└─────────────────────────────────────────────────────────┘
```

## Pattern Categories

### 1. Code Patterns
Reusable implementation patterns discovered during development.

```markdown
## Pattern: Repository with Specification

**Context**: Need to query entities with complex, composable filters.

**Solution**:
```csharp
public interface IRepository<T> where T : Entity
{
    Task<IEnumerable<T>> FindAsync(ISpecification<T> spec);
}

public class OrdersByCustomerSpec : Specification<Order>
{
    public OrdersByCustomerSpec(Guid customerId)
    {
        Query.Where(o => o.CustomerId == customerId)
             .Include(o => o.Items);
    }
}
```

**When to Use**: Complex queries that need to be reused or composed.

**Anti-Pattern**: Putting query logic in controllers or services.
```

### 2. Debugging Patterns
Solutions to common errors and debugging approaches.

```markdown
## Error Pattern: EF Core Lazy Loading Exception

**Symptom**: `InvalidOperationException: Navigation property accessed after context disposed`

**Root Cause**: Accessing navigation properties outside the DbContext scope.

**Solution**:
```csharp
// Bad: Lazy loading outside context
var order = await _context.Orders.FindAsync(id);
return order; // order.Items accessed later throws

// Good: Eager load or project
var order = await _context.Orders
    .Include(o => o.Items)
    .FirstOrDefaultAsync(o => o.Id == id);
```

**Prevention**: Always use `.Include()` for navigation properties or project to DTOs.
```

### 3. Architecture Decisions
Key decisions and their rationale.

```markdown
## Decision: Use MediatR for CQRS

**Context**: Need to separate read and write operations with clear boundaries.

**Decision**: Implement CQRS using MediatR library.

**Rationale**:
- Clean separation of commands and queries
- Pipeline behaviors for cross-cutting concerns
- Easy to test in isolation
- Well-established .NET ecosystem support

**Consequences**:
- Additional abstraction layer
- Learning curve for new team members
- Need to maintain command/query classes
```

### 4. Performance Optimizations
Discovered performance improvements.

```markdown
## Optimization: Batch Database Operations

**Before**: N+1 query problem
```csharp
foreach (var order in orders)
{
    var customer = await _context.Customers.FindAsync(order.CustomerId);
}
```

**After**: Single query with Include
```csharp
var orders = await _context.Orders
    .Include(o => o.Customer)
    .ToListAsync();
```

**Impact**: Reduced 101 queries to 1 query for 100 orders.
```

## Learning Extraction Process

### Step 1: Identify Learning Opportunity

During development, note when you:
- Solve a non-trivial problem
- Discover a better approach
- Make and correct a mistake
- Find a reusable pattern

### Step 2: Document the Pattern

Use this template:

```markdown
## [Pattern/Error/Decision Name]

**Category**: [Code Pattern | Debugging | Architecture | Performance | Security]

**Context**: What situation led to this?

**Problem/Challenge**: What needed to be solved?

**Solution**: How was it resolved?

**Example**:
```code
// Code example showing the solution
```

**When to Apply**: In what situations is this useful?

**Anti-Pattern**: What NOT to do?

**Related**: Links to other patterns or documentation
```

### Step 3: Validate and Store

1. **Validate**: Does the pattern hold in other contexts?
2. **Generalize**: Remove project-specific details
3. **Store**: Add to appropriate location:
   - `.claude/context/patterns.md` for code patterns
   - `.claude/context/anti-patterns.md` for anti-patterns
   - `docs/architecture/adr/` for architecture decisions

## Session Learning Examples

### Example 1: Error Resolution Learning

**Session Context**: Debugging authentication failure

**Learning Extracted**:
```markdown
## JWT Token Validation Failure

**Symptom**: `SecurityTokenExpiredException` even with valid tokens

**Root Cause**: Server time drift of 5+ minutes from token issuer

**Solution**:
```csharp
services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.FromMinutes(5) // Allow 5-minute drift
    };
});
```

**Prevention**: Always configure ClockSkew for JWT validation.
```

### Example 2: Pattern Discovery

**Session Context**: Implementing domain events

**Learning Extracted**:
```markdown
## Domain Events with Outbox Pattern

**Context**: Need reliable domain event publishing

**Pattern**:
1. Store events in Outbox table within same transaction
2. Background worker publishes events
3. Mark events as published after confirmation

**Benefit**: Guaranteed delivery without distributed transactions
```

## Integration with Project

### Adding to Context Files

```bash
# Add a new pattern to context
echo "## New Pattern Name" >> .claude/context/patterns.md
```

### Creating an ADR

```bash
# Create new ADR for architecture decision
cp docs/architecture/adr/template.md docs/architecture/adr/ADR-NNN-decision-name.md
```

### Updating Anti-Patterns

```bash
# Document an anti-pattern discovered
cat >> .claude/context/anti-patterns.md << 'EOF'
## Anti-Pattern: Direct Database Calls in Controllers

**Problem**: Bypasses business logic and validation
**Solution**: Always go through Application layer services
EOF
```

## Automated Learning Hooks

### Session End Hook

```javascript
// .claude/hooks/session-end.js
// Prompts for learning extraction at session end

const learnings = extractLearnings(sessionHistory);
if (learnings.length > 0) {
    await promptUser("Document these learnings?", learnings);
}
```

### Pattern Detection

```javascript
// Detect potential patterns during session
const patterns = [
    /fixed.*error/i,
    /better approach/i,
    /should have/i,
    /next time/i,
    /learned that/i
];

if (patterns.some(p => message.match(p))) {
    suggestLearningCapture(message);
}
```

## Knowledge Organization

```
.claude/
├── context/
│   ├── patterns.md          # Reusable code patterns
│   ├── anti-patterns.md     # What NOT to do
│   ├── debugging-guide.md   # Common error resolutions
│   └── performance-tips.md  # Optimization patterns
│
├── skills/
│   └── [domain-specific]/   # Domain-specific learnings
│
└── learned/                 # Session-extracted learnings
    ├── 2024-01-15-jwt-auth.md
    └── 2024-01-16-ef-core-performance.md

docs/
└── architecture/
    └── adr/                 # Architecture Decision Records
```

## Success Criteria

Continuous learning is effective when:
- [ ] Patterns are documented as discovered
- [ ] Learnings are generalized and reusable
- [ ] Knowledge is organized and searchable
- [ ] New sessions benefit from past learnings
- [ ] Anti-patterns prevent repeated mistakes
- [ ] Team knowledge is captured, not just individual
