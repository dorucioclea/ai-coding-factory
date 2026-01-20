# /refactor - Refactoring Assistant

Safely refactor code with automatic safety checks, backups, and test validation.

## Usage
```
/refactor <type> [target] [options]
```

Types:
- `rename` - Rename symbol (class, method, variable)
- `extract` - Extract method, class, or interface
- `inline` - Inline method or variable
- `move` - Move to different file/namespace
- `simplify` - Simplify complex code
- `modernize` - Update to modern C# patterns

Options:
- `--dry-run` - Preview changes without applying
- `--no-backup` - Skip creating backup branch
- `--force` - Skip confirmation prompts
- `--scope <file|project|solution>` - Limit refactoring scope

## Instructions

### Safety Protocol

Before any refactoring:

1. **Create Backup Branch**
```bash
# Create backup branch
BACKUP_BRANCH="refactor-backup-$(date +%Y%m%d-%H%M%S)"
git checkout -b $BACKUP_BRANCH
git checkout -

# Store branch name for potential rollback
echo $BACKUP_BRANCH > .claude/refactor-backup
```

2. **Run Pre-Refactor Tests**
```bash
# Ensure tests pass before refactoring
dotnet test --no-build --verbosity quiet
if [ $? -ne 0 ]; then
  echo "❌ Tests failing - refactoring aborted"
  exit 1
fi
```

3. **Analyze Impact**
```bash
# Find all usages of target symbol
grep -rn "SymbolName" src/ tests/ --include="*.cs"
```

### Rename Refactoring (`/refactor rename`)

```
User: /refactor rename OrderService to OrderManager

Claude: 🔄 Rename Refactoring: OrderService → OrderManager

## Pre-flight Checks
✅ Tests passing (168/168)
✅ Backup branch created: refactor-backup-20250120-103000
✅ No uncommitted changes

## Impact Analysis

Files affected: 12
References found: 47

### Files to Modify
| File | Changes | Type |
|------|---------|------|
| src/Application/Services/OrderService.cs | 2 | Definition |
| src/Application/Services/IOrderService.cs | 1 | Interface |
| src/API/Controllers/OrdersController.cs | 3 | Usage |
| src/Infrastructure/DependencyInjection.cs | 2 | Registration |
| tests/UnitTests/OrderServiceTests.cs | 15 | Tests |
| ... and 7 more files | 24 | Various |

### Proposed Changes

**1. Rename class definition**
```csharp
// Before: src/Application/Services/OrderService.cs
public class OrderService : IOrderService

// After: src/Application/Services/OrderManager.cs
public class OrderManager : IOrderService
```

**2. Update interface (optional)**
```csharp
// Rename IOrderService → IOrderManager? [y/n]
```

**3. Update file name**
```
OrderService.cs → OrderManager.cs
```

**4. Update DI registration**
```csharp
// Before
services.AddScoped<IOrderService, OrderService>();

// After
services.AddScoped<IOrderService, OrderManager>();
```

**5. Update test files**
```csharp
// Before: tests/UnitTests/OrderServiceTests.cs
public class OrderServiceTests

// After: tests/UnitTests/OrderManagerTests.cs
public class OrderManagerTests
```

## Confirmation

Proceed with refactoring? [y/n/preview]
```

### Extract Method (`/refactor extract method`)

```
User: /refactor extract method --from OrderService.ProcessOrder --lines 45-62

Claude: 🔄 Extract Method Refactoring

## Source Analysis

File: src/Application/Services/OrderService.cs
Method: ProcessOrder
Lines: 45-62

### Code to Extract
```csharp
// Lines 45-62 in ProcessOrder
var discount = 0m;
if (order.Customer.IsPremium)
{
    discount = order.Total * 0.1m;
}
else if (order.Total > 100)
{
    discount = order.Total * 0.05m;
}
else if (order.Customer.OrderCount > 10)
{
    discount = order.Total * 0.03m;
}
order.ApplyDiscount(discount);
```

### Proposed Extraction

**New method signature:**
```csharp
private decimal CalculateDiscount(Order order)
{
    if (order.Customer.IsPremium)
        return order.Total * 0.1m;

    if (order.Total > 100)
        return order.Total * 0.05m;

    if (order.Customer.OrderCount > 10)
        return order.Total * 0.03m;

    return 0m;
}
```

**Updated ProcessOrder:**
```csharp
public async Task ProcessOrder(Order order)
{
    // ... previous code ...

    var discount = CalculateDiscount(order);
    order.ApplyDiscount(discount);

    // ... remaining code ...
}
```

### Suggestions
1. Consider making `CalculateDiscount` a pure function
2. This logic could be moved to Order entity (domain logic)
3. Consider using a DiscountCalculator service for complex rules

Apply extraction? [y/n]
```

### Extract Interface (`/refactor extract interface`)

```
User: /refactor extract interface OrderService

Claude: 🔄 Extract Interface Refactoring

## Analysis

Class: OrderService
Location: src/Application/Services/OrderService.cs

### Public Members Detected
```csharp
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto);
    public async Task<Order> GetOrderAsync(Guid id);
    public async Task<IEnumerable<Order>> GetOrdersForCustomerAsync(Guid customerId);
    public async Task UpdateOrderStatusAsync(Guid id, OrderStatus status);
    public async Task CancelOrderAsync(Guid id);
}
```

### Proposed Interface
```csharp
// New file: src/Application/Services/IOrderService.cs
namespace Application.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderDto dto);
    Task<Order> GetOrderAsync(Guid id);
    Task<IEnumerable<Order>> GetOrdersForCustomerAsync(Guid customerId);
    Task UpdateOrderStatusAsync(Guid id, OrderStatus status);
    Task CancelOrderAsync(Guid id);
}
```

### Updated Class
```csharp
public class OrderService : IOrderService
{
    // Implementation unchanged
}
```

### Additional Updates
- DI registration: `services.AddScoped<IOrderService, OrderService>()`
- Update consumers to use `IOrderService`

Files to update: 5
- DependencyInjection.cs
- OrdersController.cs
- OrderServiceTests.cs (mock IOrderService)

Create interface? [y/n]
```

### Move Refactoring (`/refactor move`)

```
User: /refactor move OrderValidator to Domain layer

Claude: 🔄 Move Refactoring

## Current Location
File: src/Application/Validators/OrderValidator.cs
Namespace: Application.Validators

## Target Location
File: src/Domain/Validators/OrderValidator.cs
Namespace: Domain.Validators

## Analysis

### Dependencies Check
Current dependencies:
- FluentValidation ✅ (available in Domain)
- Application.DTOs ❌ (not available in Domain)

### Issues Detected
⚠️ OrderValidator depends on CreateOrderDto (Application layer)

### Suggested Resolution
1. Create domain model OrderValidationInput
2. Move validation logic to domain
3. Keep DTO-specific validation in Application

### Alternative Approach
Move validation rules to Order entity:
```csharp
// Domain/Entities/Order.cs
public class Order
{
    public static Result<Order> Create(string customerId, List<OrderItem> items)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(customerId))
            errors.Add("Customer ID is required");

        if (!items.Any())
            errors.Add("Order must have at least one item");

        if (errors.Any())
            return Result.Failure<Order>(errors);

        return Result.Success(new Order(customerId, items));
    }
}
```

Choose approach:
1. Move with domain model adapter
2. Move validation to entity (DDD)
3. Keep in Application layer
4. Cancel

[1/2/3/4]
```

### Simplify (`/refactor simplify`)

```
User: /refactor simplify OrderService.GetOrderStatus

Claude: 🔄 Simplify Refactoring

## Analyzing: OrderService.GetOrderStatus

### Current Code (Complexity: 8)
```csharp
public string GetOrderStatus(Order order)
{
    string status;
    if (order == null)
    {
        status = "Unknown";
    }
    else if (order.IsCancelled)
    {
        status = "Cancelled";
    }
    else if (order.IsShipped)
    {
        status = "Shipped";
    }
    else if (order.IsPaid)
    {
        status = "Processing";
    }
    else
    {
        status = "Pending";
    }
    return status;
}
```

### Simplified Code (Complexity: 3)
```csharp
public string GetOrderStatus(Order? order) => order switch
{
    null => "Unknown",
    { IsCancelled: true } => "Cancelled",
    { IsShipped: true } => "Shipped",
    { IsPaid: true } => "Processing",
    _ => "Pending"
};
```

### Improvements
- ✅ Reduced cyclomatic complexity: 8 → 3
- ✅ Removed mutable variable
- ✅ Used pattern matching
- ✅ Expression-bodied member
- ✅ Nullable annotation added

### Additional Suggestions
1. Consider using an enum for status
2. This logic could be a property on Order entity:
   ```csharp
   public OrderStatus Status => this switch { ... };
   ```

Apply simplification? [y/n]
```

### Modernize (`/refactor modernize`)

```
User: /refactor modernize OrderService

Claude: 🔄 Modernize Refactoring

## Analyzing: OrderService

Checking for modernization opportunities...

### Found 8 Modernization Opportunities

**1. Primary Constructor (C# 12)**
```csharp
// Before
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// After
public class OrderService(IOrderRepository repository, ILogger<OrderService> logger)
{
    // Fields auto-generated
}
```

**2. Collection Expressions (C# 12)**
```csharp
// Before
var items = new List<OrderItem>();

// After
List<OrderItem> items = [];
```

**3. Pattern Matching (C# 9+)**
```csharp
// Before
if (order != null && order.Status == OrderStatus.Pending)

// After
if (order is { Status: OrderStatus.Pending })
```

**4. Null-Coalescing Assignment (C# 8)**
```csharp
// Before
if (_cache == null)
    _cache = new Dictionary<Guid, Order>();

// After
_cache ??= new Dictionary<Guid, Order>();
```

**5. File-Scoped Namespace (C# 10)**
```csharp
// Before
namespace Application.Services
{
    public class OrderService { }
}

// After
namespace Application.Services;

public class OrderService { }
```

**6. Target-Typed New (C# 9)**
```csharp
// Before
Order order = new Order();

// After
Order order = new();
```

**7. Raw String Literals (C# 11)**
```csharp
// Before
var query = "SELECT * FROM Orders WHERE Status = 'Pending'";

// After
var query = """
    SELECT * FROM Orders
    WHERE Status = 'Pending'
    """;
```

**8. Required Members (C# 11)**
```csharp
// Before (relies on constructor)
public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
}

// After
public class OrderDto
{
    public required Guid Id { get; init; }
    public required string CustomerName { get; init; }
}
```

### Apply Modernizations

Select which to apply:
- [x] Primary constructors
- [x] Collection expressions
- [x] Pattern matching
- [x] Null-coalescing assignment
- [x] File-scoped namespaces
- [x] Target-typed new
- [ ] Raw string literals (manual review needed)
- [ ] Required members (breaking change)

Apply selected modernizations? [y/n]
```

### Post-Refactor Validation

After any refactoring:

```
## Post-Refactor Validation

Running tests...
✅ 168/168 tests passing

Running build...
✅ Build succeeded (0 warnings)

Checking for breaking changes...
✅ No public API changes detected

## Refactoring Complete

Changes made:
- 12 files modified
- 47 references updated
- 0 errors introduced

Backup branch: refactor-backup-20250120-103000

To rollback:
  git checkout refactor-backup-20250120-103000

To clean up:
  git branch -d refactor-backup-20250120-103000
```

### Rollback Procedure

```
User: /refactor --rollback

Claude: 🔄 Rollback Refactoring

Last refactoring: OrderService → OrderManager
Backup branch: refactor-backup-20250120-103000
Time: 15 minutes ago

Files to restore: 12

⚠️ Warning: This will discard all changes since the refactoring.

Proceed with rollback? [y/n]

User: y

Claude: Rolling back...

✅ Changes reverted
✅ Backup branch still available for reference

Rollback complete. Code restored to pre-refactoring state.
```

## Example

```
User: /refactor rename CustomerRepository to CustomerDataStore --dry-run

Claude: 🔄 Rename Refactoring (DRY RUN)

CustomerRepository → CustomerDataStore

## Impact Preview

Files: 8
References: 23

Changes that WOULD be made:
1. src/Infrastructure/Repositories/CustomerRepository.cs
   - Rename file to CustomerDataStore.cs
   - Rename class

2. src/Domain/Repositories/ICustomerRepository.cs
   - Rename to ICustomerDataStore.cs
   - Rename interface

3. src/Infrastructure/DependencyInjection.cs
   - Update registration

4. src/Application/Handlers/GetCustomerHandler.cs
   - Update constructor parameter

[... 4 more files ...]

This is a dry run. No changes were made.

To apply: /refactor rename CustomerRepository to CustomerDataStore
```
