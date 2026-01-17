# /code-review - Perform Code Quality Review

Conduct a comprehensive code review following .NET and Clean Architecture best practices.

## Usage
```
/code-review [scope]
```

Where scope is:
- `staged` (default) - Review staged git changes
- `recent` - Review last commit
- `file <path>` - Review specific file
- `pr` - Review all changes in current branch vs main

## Instructions

### Code Review Checklist

#### 1. Clean Architecture Compliance
- [ ] Domain layer has no external dependencies
- [ ] Application layer only depends on Domain
- [ ] Infrastructure depends only on Domain + Application
- [ ] No circular dependencies

```csharp
// Check for violations
// Domain should NOT have:
using Microsoft.EntityFrameworkCore;  // Infrastructure concern
using Microsoft.AspNetCore.*;          // API concern

// Application should NOT have:
using ProjectName.Infrastructure;      // Wrong direction
```

#### 2. SOLID Principles
- **S**ingle Responsibility: Each class has one reason to change
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Subtypes substitutable for base types
- **I**nterface Segregation: No fat interfaces
- **D**ependency Inversion: Depend on abstractions

#### 3. Code Quality Checks

**Naming Conventions**:
```csharp
// Classes, methods, properties: PascalCase
public class OrderService { }
public void ProcessOrder() { }
public string OrderId { get; set; }

// Private fields: _camelCase
private readonly ILogger _logger;

// Local variables, parameters: camelCase
var orderTotal = CalculateTotal(orderId);
```

**Async/Await**:
```csharp
// GOOD: Async for I/O operations
public async Task<Order> GetOrderAsync(int id, CancellationToken ct)
{
    return await _repository.GetByIdAsync(id, ct);
}

// BAD: Sync over async
public Order GetOrder(int id)
{
    return _repository.GetByIdAsync(id).Result; // Don't do this!
}
```

**Exception Handling**:
```csharp
// GOOD: Specific exceptions
throw new OrderNotFoundException(orderId);
throw new InvalidOperationException("Order cannot be modified");

// BAD: Generic exceptions
throw new Exception("Something went wrong");
```

**Logging**:
```csharp
// GOOD: Structured logging with ILogger
_logger.LogInformation("Processing order {OrderId} for {CustomerId}", order.Id, customer.Id);

// BAD: Console or string interpolation in log
Console.WriteLine($"Processing order {order.Id}");
_logger.LogInformation($"Processing order {order.Id}"); // No structured params
```

#### 4. Testing Coverage
- Unit tests for business logic
- Tests reference story IDs
- Meaningful test names
- Arrange-Act-Assert pattern

```csharp
[Fact]
[Trait("Story", "ACF-042")]
public async Task CreateOrder_ValidInput_ReturnsOrderId()
{
    // Arrange
    var command = new CreateOrderCommand { /* ... */ };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.OrderId.Should().BeGreaterThan(0);
}
```

#### 5. Documentation
- XML comments on public APIs
- Complex logic explained
- No obvious comments

```csharp
/// <summary>
/// Calculates the total order amount including discounts and taxes.
/// </summary>
/// <param name="order">The order to calculate.</param>
/// <returns>The total amount in the order's currency.</returns>
/// <exception cref="InvalidOrderException">Thrown when order is invalid.</exception>
public decimal CalculateTotal(Order order)
```

### Output Format

```markdown
# Code Review Report
Date: <timestamp>
Scope: <scope>
Files Reviewed: <count>

## Summary
| Category | Issues |
|----------|--------|
| Architecture | X |
| Code Quality | X |
| Security | X |
| Testing | X |
| Documentation | X |

## Findings

### Architecture Issues

#### [ARCH-001] Infrastructure Dependency in Domain
**File**: src/Domain/Entities/Order.cs:15
**Issue**: Domain entity has EF Core dependency
**Suggestion**: Move data annotations to Infrastructure configuration

---

### Code Quality Issues

#### [CODE-001] Missing Async Suffix
**File**: src/Application/Handlers/OrderHandler.cs:42
**Issue**: Async method should be named `GetOrderAsync`
**Current**: `GetOrder`
**Suggestion**: Rename to follow async naming convention

---

## Recommendations

1. **Must Fix**: Architecture and Security issues
2. **Should Fix**: Code quality issues
3. **Consider**: Documentation improvements

## Approval Status
- [ ] All must-fix issues addressed
- [ ] Code follows team standards
- [ ] Tests pass and cover new code
- [ ] Documentation updated
```

## Example

```
User: /code-review staged

Claude: Reviewing staged changes...

## Code Review Summary

Files reviewed: 4
- src/Application/Commands/CreateOrderCommand.cs
- src/Application/Handlers/CreateOrderHandler.cs
- src/Domain/Entities/Order.cs
- tests/UnitTests/CreateOrderTests.cs

| Category | Issues |
|----------|--------|
| Architecture | 0 |
| Code Quality | 2 |
| Security | 0 |
| Testing | 1 |
| Documentation | 1 |

### Issues Found

#### [CODE-001] Missing null check
**File**: CreateOrderHandler.cs:28
```csharp
// Current
var customer = await _repo.GetCustomerAsync(command.CustomerId);
customer.AddOrder(order); // Could throw NullReferenceException

// Suggested
var customer = await _repo.GetCustomerAsync(command.CustomerId)
    ?? throw new CustomerNotFoundException(command.CustomerId);
```

#### [TEST-001] Missing Story trait
**File**: CreateOrderTests.cs:15
Add `[Trait("Story", "ACF-###")]` to link test to story.

---

**Verdict**: Address 2 code quality issues before merge.
```
