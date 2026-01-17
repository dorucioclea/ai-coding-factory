# /add-entity - Add Domain Entity

Add a new domain entity following DDD patterns.

## Usage
```
/add-entity <EntityName> [options]
```

Options:
- `--aggregate` - Mark as aggregate root
- `--with-events` - Include domain events
- `--with-repository` - Generate repository interface

## Instructions

When invoked:

### 1. Gather Entity Information

Ask for:
- Entity name (PascalCase)
- Key properties and their types
- Whether it's an aggregate root
- Related entities (for navigation properties)

### 2. Generate Files

**Entity Class** (`src/Project.Domain/Entities/<EntityName>.cs`):
```csharp
namespace Project.Domain.Entities;

public class <EntityName> : BaseEntity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Properties...

    public static <EntityName> Create(/* params */)
    {
        var entity = new <EntityName>
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        entity.AddDomainEvent(new <EntityName>CreatedEvent(entity.Id));
        return entity;
    }

    private <EntityName>() { }
}
```

**Domain Events** (if `--with-events`):
```csharp
public record <EntityName>CreatedEvent(Guid <EntityName>Id) : IDomainEvent;
public record <EntityName>UpdatedEvent(Guid <EntityName>Id) : IDomainEvent;
```

**Repository Interface** (if `--with-repository`):
```csharp
public interface I<EntityName>Repository : IRepository<<EntityName>>
{
    Task<<EntityName>?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(<EntityName> entity, CancellationToken ct = default);
    void Update(<EntityName> entity);
}
```

### 3. Update EF Configuration

Create or update Infrastructure configuration:
```csharp
// src/Project.Infrastructure/Persistence/Configurations/<EntityName>Configuration.cs
public class <EntityName>Configuration : IEntityTypeConfiguration<<EntityName>>
{
    public void Configure(EntityTypeBuilder<<EntityName>> builder)
    {
        builder.ToTable("<EntityName>s");
        builder.HasKey(e => e.Id);
        // Configure properties...
    }
}
```

### 4. Add to DbContext

Update `ApplicationDbContext.cs`:
```csharp
public DbSet<<EntityName>> <EntityName>s => Set<<EntityName>>();
```

### 5. Generate Tests

Create unit tests for entity creation and business methods.

## Templates Used

- `.claude/templates/entity.cs.template`
- `.claude/templates/repository.cs.template`

## Output

```markdown
## Entity Created: <EntityName>

### Files Created
- src/Project.Domain/Entities/<EntityName>.cs
- src/Project.Domain/Events/<EntityName>Events.cs
- src/Project.Domain/Repositories/I<EntityName>Repository.cs
- src/Project.Infrastructure/Persistence/Configurations/<EntityName>Configuration.cs

### Next Steps
1. Add properties to the entity
2. Implement repository in Infrastructure
3. Register in DI container
4. Create CQRS commands/queries with `/add-command` or `/add-query`
```

## Example

```
User: /add-entity Order --aggregate --with-events --with-repository

Claude: Creating Order entity as aggregate root...

I'll create the following:
1. Order entity with factory method
2. OrderCreatedEvent and OrderUpdatedEvent
3. IOrderRepository interface
4. EF Core configuration

Properties for Order:
- What properties should the Order have? (e.g., CustomerId, Status, TotalAmount)

[After gathering info, generates all files]

Entity Order created successfully.
```
