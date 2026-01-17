# Anti-Patterns to Avoid

Common mistakes that Claude Code should never introduce.

## Architecture Violations

### ❌ Domain Layer with External Dependencies
```csharp
// WRONG: Domain entity using EF Core
using Microsoft.EntityFrameworkCore;

namespace Project.Domain.Entities;

public class Order
{
    [Key]  // Don't use data annotations in Domain
    public Guid Id { get; set; }
}
```

```csharp
// CORRECT: Pure domain entity
namespace Project.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    private Order() { } // For EF, but no EF reference
}
```

### ❌ Application Layer Referencing Infrastructure
```csharp
// WRONG: Handler using concrete repository
using Project.Infrastructure.Repositories;

public class CreateOrderHandler
{
    private readonly OrderRepository _repo;  // Concrete type!
}
```

```csharp
// CORRECT: Handler using interface
using Project.Domain.Repositories;

public class CreateOrderHandler
{
    private readonly IOrderRepository _repo;  // Interface from Domain
}
```

---

## Security Violations

### ❌ Hardcoded Secrets
```csharp
// NEVER DO THIS
public class PaymentService
{
    private const string ApiKey = "sk_live_abc123";  // WRONG!
}
```

```csharp
// CORRECT: Use configuration
public class PaymentService
{
    private readonly string _apiKey;

    public PaymentService(IConfiguration config)
    {
        _apiKey = config["Payment:ApiKey"]
            ?? throw new InvalidOperationException("Payment API key not configured");
    }
}
```

### ❌ SQL Injection
```csharp
// NEVER DO THIS
public async Task<User?> GetUser(string email)
{
    var sql = $"SELECT * FROM Users WHERE Email = '{email}'";  // WRONG!
    return await _context.Users.FromSqlRaw(sql).FirstOrDefaultAsync();
}
```

```csharp
// CORRECT: Parameterized query
public async Task<User?> GetUser(string email)
{
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
}
```

### ❌ Missing Authorization
```csharp
// WRONG: No authorization on sensitive endpoint
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)  // Anyone can delete!
{
    await _service.DeleteAsync(id);
    return NoContent();
}
```

```csharp
// CORRECT: Authorization required
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(Guid id)
{
    await _service.DeleteAsync(id);
    return NoContent();
}
```

---

## Code Quality Violations

### ❌ Sync over Async
```csharp
// WRONG: Blocking call
public Order GetOrder(Guid id)
{
    return _repository.GetByIdAsync(id).Result;  // Deadlock risk!
}
```

```csharp
// CORRECT: Proper async
public async Task<Order> GetOrderAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}
```

### ❌ Console.WriteLine for Logging
```csharp
// WRONG: Using Console
public void ProcessOrder(Order order)
{
    Console.WriteLine($"Processing order {order.Id}");  // Not structured!
}
```

```csharp
// CORRECT: Structured logging
public void ProcessOrder(Order order)
{
    _logger.LogInformation("Processing order {OrderId}", order.Id);
}
```

### ❌ Generic Exception Handling
```csharp
// WRONG: Catching and throwing generic Exception
try
{
    await ProcessAsync();
}
catch (Exception ex)
{
    throw new Exception("Processing failed");  // Lost context!
}
```

```csharp
// CORRECT: Specific exceptions
try
{
    await ProcessAsync();
}
catch (HttpRequestException ex)
{
    throw new ExternalServiceException("Payment service unavailable", ex);
}
catch (ValidationException ex)
{
    throw;  // Let validation exceptions bubble up
}
```

### ❌ Service Locator Pattern
```csharp
// WRONG: Service locator
public class OrderService
{
    public void Process()
    {
        var repo = ServiceLocator.Get<IOrderRepository>();  // Anti-pattern!
    }
}
```

```csharp
// CORRECT: Constructor injection
public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
}
```

---

## Testing Violations

### ❌ Tests Without Story IDs
```csharp
// WRONG: Missing traceability
[Fact]
public void CreateOrder_ValidInput_Succeeds()
{
    // No story ID!
}
```

```csharp
// CORRECT: Story ID included
[Fact]
[Trait("Story", "ACF-042")]
public void CreateOrder_ValidInput_Succeeds()
{
}
```

### ❌ Test Method Naming
```csharp
// WRONG: Unclear name
[Fact]
public void Test1()
{
}

// WRONG: Too vague
[Fact]
public void OrderTest()
{
}
```

```csharp
// CORRECT: Method_Scenario_ExpectedBehavior
[Fact]
public void CreateOrder_WithEmptyItems_ThrowsValidationException()
{
}
```

### ❌ No Assertions
```csharp
// WRONG: Test without assertions
[Fact]
public void CreateOrder_ValidInput_Works()
{
    var order = Order.Create(Guid.NewGuid());
    // No assertion!
}
```

```csharp
// CORRECT: Clear assertions
[Fact]
public void CreateOrder_ValidInput_ReturnsOrderWithId()
{
    var order = Order.Create(Guid.NewGuid());

    order.Should().NotBeNull();
    order.Id.Should().NotBeEmpty();
}
```

---

## Traceability Violations

### ❌ Commits Without Story IDs
```bash
# WRONG
git commit -m "Fixed bug"
git commit -m "Updated code"
```

```bash
# CORRECT
git commit -m "ACF-042 Fix order validation bug"
git commit -m "ACF-043 Add customer lookup endpoint"
```

### ❌ Missing Documentation Updates
```csharp
// WRONG: Changed behavior but didn't update docs
public class OrderService
{
    /// <summary>
    /// Creates an order.
    /// </summary>
    public Order Create(CreateOrderRequest request)
    {
        // Implementation now requires authentication
        // but docs don't mention it!
    }
}
```

---

## API Design Violations

### ❌ Leaking Internal Models
```csharp
// WRONG: Exposing domain entities directly
[HttpGet("{id}")]
public async Task<Order> Get(Guid id)  // Domain entity exposed!
{
    return await _repository.GetByIdAsync(id);
}
```

```csharp
// CORRECT: Use DTOs
[HttpGet("{id}")]
public async Task<OrderDto> Get(Guid id)
{
    var order = await _repository.GetByIdAsync(id);
    return order.ToDto();
}
```

### ❌ Ignoring Cancellation Tokens
```csharp
// WRONG: No cancellation support
public async Task<Order> GetOrderAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);  // No CT!
}
```

```csharp
// CORRECT: Propagate cancellation
public async Task<Order> GetOrderAsync(Guid id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}
```

---

## Resource Management Violations

### ❌ Not Disposing Resources
```csharp
// WRONG: Resource leak
public void ProcessFile(string path)
{
    var stream = File.OpenRead(path);
    // Stream never disposed!
}
```

```csharp
// CORRECT: Using statement
public void ProcessFile(string path)
{
    using var stream = File.OpenRead(path);
    // Automatically disposed
}

// Or async version
public async Task ProcessFileAsync(string path)
{
    await using var stream = File.OpenRead(path);
}
```

---

## Remember

1. **Architecture**: Domain has NO external dependencies
2. **Security**: Never hardcode secrets, always validate input
3. **Async**: Use async/await properly, never block
4. **Logging**: Use ILogger with structured logging
5. **Testing**: Include story IDs, use clear naming
6. **Traceability**: Story IDs in commits, tests, and code
7. **APIs**: Use DTOs, never expose domain models
