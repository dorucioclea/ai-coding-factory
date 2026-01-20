# /learn - Learning Mode

Record user corrections and preferences to improve Claude Code's suggestions over time.

## Usage
```
/learn <action> [options]
```

Actions:
- `record` - Record a correction or preference
- `list` - List learned patterns
- `apply` - Apply learned patterns to current context
- `export` - Export learned patterns
- `clear` - Clear specific or all patterns

Options:
- `--category <code|style|naming|architecture>` - Filter by category
- `--project <name>` - Project-specific learning

## Instructions

### Learning Categories

| Category | Description | Examples |
|----------|-------------|----------|
| `code` | Code patterns and implementations | "Use record instead of class for DTOs" |
| `style` | Formatting and style preferences | "Prefer expression-bodied members" |
| `naming` | Naming conventions | "Prefix interfaces with I" |
| `architecture` | Structural decisions | "Use MediatR for all commands" |
| `testing` | Test patterns | "Use FluentAssertions for all tests" |
| `documentation` | Doc preferences | "Use XML comments on public members" |

### Recording a Learning (`/learn record`)

When user corrects output, capture:

```json
{
  "id": "learn-{{UUID}}",
  "timestamp": "{{ISO_DATE}}",
  "category": "{{CATEGORY}}",
  "context": {
    "task": "{{WHAT_USER_ASKED}}",
    "file": "{{AFFECTED_FILE}}",
    "language": "csharp"
  },
  "original": {
    "code": "{{CLAUDE_OUTPUT}}",
    "reasoning": "{{WHY_CLAUDE_DID_THIS}}"
  },
  "correction": {
    "code": "{{USER_CORRECTION}}",
    "explanation": "{{USER_REASONING}}"
  },
  "pattern": {
    "trigger": "{{WHEN_TO_APPLY}}",
    "action": "{{WHAT_TO_DO}}",
    "confidence": 0.8
  },
  "metadata": {
    "project": "{{PROJECT_NAME}}",
    "appliedCount": 0,
    "lastApplied": null
  }
}
```

### Storage Location

Learned patterns are stored in `.claude/learned-patterns.json`:

```json
{
  "version": "1.0",
  "project": "{{PROJECT_NAME}}",
  "lastUpdated": "{{ISO_DATE}}",
  "patterns": [
    {
      "id": "learn-001",
      "category": "code",
      "trigger": "Creating DTOs",
      "action": "Use record types instead of classes",
      "example": {
        "before": "public class OrderDto { ... }",
        "after": "public record OrderDto(...)"
      },
      "confidence": 0.9,
      "appliedCount": 15
    },
    {
      "id": "learn-002",
      "category": "naming",
      "trigger": "Naming command handlers",
      "action": "Use {Command}Handler naming convention",
      "example": {
        "before": "CreateOrderCommandHandler",
        "after": "CreateOrderHandler"
      },
      "confidence": 0.85,
      "appliedCount": 8
    }
  ]
}
```

### Interactive Recording Flow

```
User: That's not quite right. We use records for DTOs, not classes.

Claude: I'll record this preference.

📝 Recording Learning...

Category: code
Context: Creating DTOs in Application layer

Original approach:
```csharp
public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
}
```

Your preferred approach:
```csharp
public record OrderDto(Guid Id, string CustomerName);
```

Pattern identified:
- Trigger: When creating DTO classes
- Action: Use record types with primary constructors
- Scope: All Application layer DTOs

Save this learning? [y/n]

User: y

Claude: ✅ Learned! Pattern saved as learn-003.

  This will be applied when:
  - Creating new DTOs
  - Generating Application layer models
  - Using /add-endpoint command

  View all patterns: /learn list
```

### Listing Patterns (`/learn list`)

```
User: /learn list

Claude: 📚 Learned Patterns (12 total)

## Code Patterns (5)
| ID | Trigger | Action | Used |
|----|---------|--------|------|
| learn-001 | Creating DTOs | Use record types | 15x |
| learn-003 | Entity methods | Use expression bodies | 8x |
| learn-005 | Null checks | Use pattern matching | 12x |

## Naming Patterns (3)
| ID | Trigger | Action | Used |
|----|---------|--------|------|
| learn-002 | Handler names | Drop "Command" suffix | 8x |
| learn-007 | Private fields | Use _camelCase | 20x |

## Architecture Patterns (2)
| ID | Trigger | Action | Used |
|----|---------|--------|------|
| learn-004 | New features | Always use CQRS | 6x |
| learn-008 | Repositories | Generic base class | 4x |

## Style Patterns (2)
| ID | Trigger | Action | Used |
|----|---------|--------|------|
| learn-006 | Braces | Always include | 25x |
| learn-009 | Line length | Max 120 chars | 18x |

Options:
  /learn list --category code
  /learn show learn-001
  /learn clear learn-005
```

### Applying Patterns (`/learn apply`)

Before generating code, check relevant patterns:

```
User: Create a DTO for Product

Claude: Creating ProductDto...

📝 Applying learned patterns:
  ✅ learn-001: Using record type (15x used)
  ✅ learn-009: Max 120 char lines (18x used)

Generated with your preferences:

```csharp
public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Category,
    bool IsAvailable);
```

Patterns applied: 2
```

### Pattern Confidence System

Patterns have confidence scores that affect when they're applied:

| Confidence | Behavior |
|------------|----------|
| 0.9 - 1.0 | Always apply automatically |
| 0.7 - 0.89 | Apply with notification |
| 0.5 - 0.69 | Suggest but ask confirmation |
| < 0.5 | Only apply when explicitly requested |

Confidence increases when:
- Pattern is applied and user doesn't modify
- User explicitly confirms pattern

Confidence decreases when:
- User modifies output after pattern applied
- User rejects pattern suggestion

### Exporting Patterns (`/learn export`)

```
User: /learn export

Claude: Exporting learned patterns...

Exported to: .claude/learned-patterns-export-2025-01-20.json

Contents:
- 12 patterns total
- 5 code patterns
- 3 naming patterns
- 2 architecture patterns
- 2 style patterns

Share with team: Copy to shared .claude/ folder
Import in new project: /learn import <file>
```

### Pattern Categories Reference

#### Code Patterns
```json
{
  "category": "code",
  "examples": [
    "Use records for DTOs",
    "Prefer pattern matching for null checks",
    "Use expression-bodied members for simple methods",
    "Use collection expressions []",
    "Prefer LINQ over loops"
  ]
}
```

#### Style Patterns
```json
{
  "category": "style",
  "examples": [
    "Always include braces",
    "Max line length 120",
    "Prefer var for obvious types",
    "Blank line before return statements",
    "Group usings by namespace"
  ]
}
```

#### Naming Patterns
```json
{
  "category": "naming",
  "examples": [
    "_camelCase for private fields",
    "PascalCase for public members",
    "I prefix for interfaces",
    "Async suffix for async methods",
    "Handler suffix for CQRS handlers"
  ]
}
```

#### Architecture Patterns
```json
{
  "category": "architecture",
  "examples": [
    "All commands via MediatR",
    "Repository pattern for data access",
    "Result<T> for operation outcomes",
    "Domain events for cross-aggregate",
    "Specification pattern for queries"
  ]
}
```

### Integration with Other Commands

Learned patterns are automatically considered by:

| Command | How Patterns Apply |
|---------|-------------------|
| `/add-entity` | Naming, code style |
| `/add-endpoint` | Architecture, code patterns |
| `/add-test` | Testing patterns, naming |
| `/implement` | All applicable patterns |
| `/code-review` | Check against patterns |

### Example: Full Learning Session

```
User: Generate a command handler for creating products

Claude: Creating CreateProductCommandHandler...

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Price);
        await _repository.AddAsync(product, ct);
        return Result.Success(product.Id);
    }
}
```

User: We use primary constructors and simpler handler names

Claude: I'll update and learn this pattern.

Updated code:
```csharp
public class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Price);
        await repository.AddAsync(product, ct);
        return Result.Success(product.Id);
    }
}
```

📝 Patterns learned:

1. **Primary Constructors** (learn-010)
   - Trigger: Creating classes with DI
   - Action: Use primary constructor syntax
   - Category: code

2. **Handler Naming** (learn-011)
   - Trigger: Naming command/query handlers
   - Action: Use {Action}{Entity}Handler
   - Category: naming

Patterns saved. Will apply to future handlers.
```

## Storage File Structure

```
.claude/
├── learned-patterns.json      # Main patterns file
├── learned-patterns.backup    # Auto-backup before changes
└── learning-history/          # Historical corrections
    ├── 2025-01-20.json
    └── 2025-01-19.json
```

## Example

```
User: /learn record

Claude: What would you like me to learn?

Describe the correction or preference:

User: Always use sealed classes unless inheritance is needed

Claude: 📝 Recording pattern...

Pattern details:
- Category: code
- Trigger: Creating new classes
- Action: Make classes sealed by default
- Scope: All layers

Example:
```csharp
// Before (default)
public class OrderService { }

// After (learned)
public sealed class OrderService { }
```

Save this learning? [y/n]

User: y

Claude: ✅ Learned! Pattern saved as learn-012.

Future class generations will be sealed by default.
Confidence: 0.8 (will increase with successful applications)
```
