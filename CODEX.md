# AI Coding Factory - Codex CLI Instructions

This file provides instructions for the OpenAI Codex CLI when working in this repository.

## Overview

AI Coding Factory is an enterprise .NET software delivery platform with strict governance, traceability, security, and quality standards. All work must follow the established patterns and policies.

## Quick Reference

### Key Files
- `CORPORATE_RND_POLICY.md` - Authoritative governance document (ALWAYS follow)
- `CLAUDE.md` - Detailed AI instructions
- `AGENTS.md` - Agent system documentation
- `.ai/instructions/` - Shared instructions for all AI tools

### Project Structure
```
.ai/                      # Unified AI instructions
.claude/                  # Claude Code configuration
.opencode/                # OpenCode configuration
artifacts/stories/        # User stories (ACF-###.md)
docs/architecture/adr/    # Architecture Decision Records
templates/                # Project boilerplates
scripts/                  # Validation scripts
```

## Core Rules

### 1. Story Traceability
Every change must be linked to a story:
- Story format: `ACF-###` (e.g., ACF-001, ACF-042)
- Stories live in: `artifacts/stories/ACF-###.md`
- Tests must have: `[Trait("Story", "ACF-###")]`
- Commits must start with: `ACF-### Description`

### 2. Clean Architecture
```
Domain ← Application ← Infrastructure ← API
```
- **Domain**: Entities, Value Objects, no external deps
- **Application**: Commands, Queries, DTOs, Validators
- **Infrastructure**: EF Core, Repositories, External Services
- **API**: Controllers, Middleware, DI Config

### 3. .NET Standards
- Target: .NET 8 LTS
- Async for all I/O operations
- FluentValidation for input validation
- MediatR for CQRS pattern
- Structured logging with ILogger

### 4. Testing Requirements
- Unit tests: >80% coverage for Domain/Application
- Integration tests: All critical endpoints
- Test naming: `MethodName_Scenario_ExpectedBehavior`

## Agents

Use these specialized contexts for different work:

| Agent | Purpose |
|-------|---------|
| developer | Implementation within story scope |
| qa | Test linkage and coverage verification |
| security | Threat modeling and security reviews |
| devops | CI/CD and deployment configuration |
| architect | Design decisions and ADRs |

## Common Tasks

### Create a Story
```bash
# Find next ID
ls artifacts/stories/ACF-*.md | tail -1

# Create story file
# Use template from .ai/instructions/governance.md
```

### Implement a Feature
1. Read story requirements from `artifacts/stories/ACF-###.md`
2. Create Domain entities if needed
3. Add Application commands/queries with validators
4. Implement Infrastructure (repositories, services)
5. Add API controllers
6. Write tests with `[Trait("Story", "ACF-###")]`
7. Commit with message: `ACF-### Description`

### Run Validations
```bash
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh
python3 scripts/traceability/traceability.py validate
```

### Create ADR
```bash
# Use templates from docs/architecture/adr/
# or .ai/instructions/governance.md
```

## Code Patterns

### Entity (Domain Layer)
```csharp
public class Order : Entity<Guid>
{
    public string OrderNumber { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public static Order Create(CustomerId customerId)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            Status = OrderStatus.Draft
        };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }

    public void AddLine(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify confirmed order");
        _lines.Add(new OrderLine(product.Id, product.Price, quantity));
    }
}
```

### Command Handler (Application Layer)
```csharp
public record CreateOrderCommand(Guid CustomerId) : IRequest<Result<Guid>>;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(CustomerId.Create(request.CustomerId));
        await orderRepository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(order.Id);
    }
}
```

### Test (with Traceability)
```csharp
[Fact]
[Trait("Story", "ACF-042")]
[Trait("Category", "Unit")]
public void Order_Create_WithValidCustomer_ReturnsOrder()
{
    // Arrange
    var customerId = CustomerId.Create(Guid.NewGuid());

    // Act
    var order = Order.Create(customerId);

    // Assert
    order.Should().NotBeNull();
    order.Status.Should().Be(OrderStatus.Draft);
    order.CustomerId.Should().Be(customerId);
}
```

## Security Guidelines

### Always
- Use EF Core parameterized queries
- Validate all inputs with FluentValidation
- Use IConfiguration for secrets
- Implement JWT authentication
- Use HTTPS in production

### Never
- Hardcode secrets, tokens, or keys
- Use raw SQL strings
- Trust user input without validation
- Commit .env files
- Log sensitive data

## Commit Message Format

```
ACF-### Brief description of change

- Detailed bullet point 1
- Detailed bullet point 2

Co-Authored-By: Codex <noreply@openai.com>
```

## Definition of Done

Before marking work complete:
- [ ] All acceptance criteria from story met
- [ ] Tests exist with `[Trait("Story", "ACF-###")]`
- [ ] All tests pass
- [ ] Coverage >= 80% for Domain/Application
- [ ] No high/critical security vulnerabilities
- [ ] Commits prefixed with `ACF-###`

## Getting Help

- Read `CLAUDE.md` for comprehensive instructions
- Check `.ai/instructions/` for detailed guides
- Review `CORPORATE_RND_POLICY.md` for governance rules
- See `.opencode/templates/` for templates

---
*Synced from .ai/instructions/ - See .ai/README.md for details*
