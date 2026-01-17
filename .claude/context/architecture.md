# Architecture Context

Quick reference for Claude Code when working with this codebase.

## Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  Controllers, Middleware, Filters, DI Configuration         │
│  Depends on: Application, Infrastructure                    │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                      │
│  EF Core, Repositories, External Services                   │
│  Depends on: Domain, Application                            │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│  Commands, Queries, DTOs, Validators, Behaviors             │
│  Depends on: Domain only                                    │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│  Entities, Value Objects, Aggregates, Domain Events         │
│  Depends on: NOTHING (zero external dependencies)           │
└─────────────────────────────────────────────────────────────┘
```

## File Locations by Layer

### Domain Layer
```
src/{Project}.Domain/
├── Entities/           # Business objects with identity
├── ValueObjects/       # Immutable, compared by value
├── Aggregates/         # Consistency boundaries
├── Events/             # Domain events
├── Repositories/       # Interface definitions (I*Repository.cs)
├── Specifications/     # Query specifications
└── Exceptions/         # Domain-specific exceptions
```

### Application Layer
```
src/{Project}.Application/
├── Commands/           # Write operations (CQRS)
│   └── {Feature}/
│       ├── {Command}Command.cs
│       ├── {Command}Handler.cs
│       └── {Command}Validator.cs
├── Queries/            # Read operations (CQRS)
│   └── {Feature}/
│       ├── {Query}Query.cs
│       └── {Query}Handler.cs
├── DTOs/               # Data transfer objects
├── Behaviors/          # Pipeline behaviors (logging, validation)
├── Interfaces/         # External service interfaces
└── Mappings/           # AutoMapper profiles
```

### Infrastructure Layer
```
src/{Project}.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/    # EF Core entity configurations
│   └── Migrations/        # Database migrations
├── Repositories/          # Repository implementations
├── Services/              # External service implementations
└── Extensions/            # DI registration extensions
```

### API Layer
```
src/{Project}.Api/
├── Controllers/           # REST endpoints
├── Middleware/            # Custom middleware
├── Filters/               # Action/exception filters
├── Extensions/            # Startup extensions
├── Models/                # API-specific models
└── Program.cs             # Application entry point
```

## Key Design Patterns

### CQRS Pattern
- **Commands**: Modify state, return void or result
- **Queries**: Read state, never modify
- **Use MediatR** for dispatching

### Repository Pattern
- Interface in Domain: `IOrderRepository`
- Implementation in Infrastructure: `OrderRepository`
- Never expose DbContext outside Infrastructure

### Unit of Work
- Transaction management across repositories
- Commit at the end of a request

### Domain Events
- Raised by aggregates
- Handled by event handlers
- Enables loose coupling

## Dependency Rules (CRITICAL)

| Layer | Can Reference | Cannot Reference |
|-------|---------------|------------------|
| Domain | Nothing | Application, Infrastructure, API |
| Application | Domain | Infrastructure, API |
| Infrastructure | Domain, Application | API |
| API | All | - |

## Common Violations to Avoid

```csharp
// BAD: Domain referencing EF Core
using Microsoft.EntityFrameworkCore;  // Don't do this in Domain!

// BAD: Application referencing Infrastructure
using Project.Infrastructure.Repositories;  // Wrong direction!

// GOOD: Application using Domain interfaces
using Project.Domain.Repositories;  // Interface defined in Domain
```

## Namespace Conventions

```
{Company}.{Product}.{Layer}.{Feature}

Examples:
- Acme.OrderSystem.Domain.Entities
- Acme.OrderSystem.Application.Commands.Orders
- Acme.OrderSystem.Infrastructure.Persistence
- Acme.OrderSystem.Api.Controllers
```
