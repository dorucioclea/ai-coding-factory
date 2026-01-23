# Clean Architecture Guidelines

## Overview

This repository enforces Clean Architecture for all .NET projects. The architecture consists of four layers with strict dependency rules.

## Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                           │
│              Controllers, Middleware, DI Config             │
└─────────────────────────────┬───────────────────────────────┘
                              │ depends on
┌─────────────────────────────▼───────────────────────────────┐
│                   Infrastructure Layer                       │
│         EF Core, Repository Impl, External Services          │
└─────────────────────────────┬───────────────────────────────┘
                              │ depends on
┌─────────────────────────────▼───────────────────────────────┐
│                    Application Layer                         │
│          Commands, Queries, DTOs, Validators                 │
└─────────────────────────────┬───────────────────────────────┘
                              │ depends on
┌─────────────────────────────▼───────────────────────────────┐
│                      Domain Layer                            │
│     Entities, Value Objects, Aggregates, Domain Events       │
└─────────────────────────────────────────────────────────────┘
```

## Dependency Rules

| Layer | Can Depend On | Cannot Depend On |
|-------|---------------|------------------|
| Domain | Nothing | Application, Infrastructure, API |
| Application | Domain | Infrastructure, API |
| Infrastructure | Domain, Application | API |
| API | All layers | - |

### Violation Detection

```csharp
// Architecture test example
[Fact]
public void Domain_Should_Not_Reference_Application()
{
    var result = Types.InAssembly(typeof(Order).Assembly)
        .Should()
        .NotHaveDependencyOn("Application")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

## Domain Layer

### Entities
```csharp
public class Order : Entity<Guid>
{
    public string OrderNumber { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    // Factory method for creation
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

    // Business methods
    public void AddLine(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify confirmed order");

        _lines.Add(new OrderLine(product.Id, product.Price, quantity));
    }
}
```

### Value Objects
```csharp
public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Currency mismatch");
        return this with { Amount = Amount + other.Amount };
    }
}
```

### Repository Interfaces
```csharp
// In Domain layer - interface only
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
}
```

## Application Layer

### Commands (CQRS)
```csharp
public record CreateOrderCommand(Guid CustomerId) : IRequest<Result<Guid>>;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request,
        CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure<Guid>("Customer not found");

        var order = Order.Create(customer.Id);
        await orderRepository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(order.Id);
    }
}
```

### Queries (CQRS)
```csharp
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;

public class GetOrderHandler(IDbContext dbContext)
    : IRequestHandler<GetOrderQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        return await dbContext.Orders
            .Where(o => o.Id == request.OrderId)
            .Select(o => new OrderDto(o.Id, o.OrderNumber, o.Status.ToString()))
            .FirstOrDefaultAsync(ct);
    }
}
```

### Validators
```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");
    }
}
```

## Infrastructure Layer

### Repository Implementation
```csharp
public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await context.Orders.AddAsync(order, ct);
    }

    public Task UpdateAsync(Order order, CancellationToken ct)
    {
        context.Orders.Update(order);
        return Task.CompletedTask;
    }
}
```

### DbContext Configuration
```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

## API Layer

### Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(request.CustomerId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetOrderQuery(id);
        var result = await mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }
}
```

### Dependency Injection
```csharp
// Program.cs
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderValidator).Assembly);
```

## Project Structure

```
src/
├── ProjectName.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Aggregates/
│   ├── Events/
│   ├── Exceptions/
│   └── Interfaces/
├── ProjectName.Application/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Validators/
│   ├── Behaviors/
│   └── Interfaces/
├── ProjectName.Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Services/
│   └── Configuration/
└── ProjectName.API/
    ├── Controllers/
    ├── Middleware/
    ├── Filters/
    └── Extensions/
tests/
├── ProjectName.UnitTests/
├── ProjectName.IntegrationTests/
└── ProjectName.ArchitectureTests/
```

## Common Patterns

### Result Pattern
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

### Specification Pattern
```csharp
public class OrderByCustomerSpec : Specification<Order>
{
    public OrderByCustomerSpec(Guid customerId)
    {
        Query.Where(o => o.CustomerId == customerId)
             .Include(o => o.Lines)
             .OrderByDescending(o => o.CreatedAt);
    }
}
```

### Domain Events
```csharp
public record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

public class OrderCreatedHandler(INotificationService notifications)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await notifications.SendAsync($"Order {notification.OrderId} created", ct);
    }
}
```
