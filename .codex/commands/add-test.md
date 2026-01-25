# /add-test - Generate Test Cases

Generate unit or integration tests for existing code.

## Usage
```
/add-test <type> <target> [options]
```

Types:
- `unit` - Unit tests for classes/methods
- `integration` - Integration tests for endpoints
- `architecture` - Architecture tests for layer validation

Target:
- Class name, method name, or file path

Options:
- `--story <ACF-###>` - Link to story ID (required for traceability)
- `--coverage` - Generate tests to improve coverage

## Instructions

### Unit Tests (`/add-test unit <ClassName>`)

1. **Analyze the class**:
   - Identify public methods
   - Identify dependencies (for mocking)
   - Identify edge cases and error paths

2. **Generate test class**:
```csharp
public class <ClassName>Tests
{
    private readonly Mock<IDependency> _dependencyMock;
    private readonly <ClassName> _sut;

    public <ClassName>Tests()
    {
        _dependencyMock = new Mock<IDependency>();
        _sut = new <ClassName>(_dependencyMock.Object);
    }
}
```

3. **Generate test methods** for each scenario:
   - Happy path (valid input → expected output)
   - Null input → ArgumentNullException
   - Invalid input → ValidationException
   - Error from dependency → proper error handling

### Integration Tests (`/add-test integration <Endpoint>`)

1. **Analyze the endpoint**:
   - HTTP method
   - Request/response types
   - Authentication requirements

2. **Generate test class**:
```csharp
public class <Controller>Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    // Setup...
}
```

3. **Generate tests** for each scenario:
   - Valid request → expected status code
   - Invalid request → 400 Bad Request
   - Not found → 404 Not Found
   - Unauthorized → 401 Unauthorized

### Architecture Tests (`/add-test architecture`)

Generate NetArchTest tests to validate Clean Architecture rules:

```csharp
public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(DomainAssembly).Assembly)
            .Should()
            .NotHaveDependencyOn("Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

## Test Templates

Uses templates from `.claude/templates/`:
- `unit-test.cs.template`
- `integration-test.cs.template`

## Naming Convention

```
MethodName_Scenario_ExpectedBehavior

Examples:
- CreateOrder_ValidInput_ReturnsOrderId
- CreateOrder_NullCustomerId_ThrowsArgumentNullException
- CreateOrder_DuplicateOrder_ThrowsInvalidOperationException
```

## Required Traits

All tests MUST include story trait:
```csharp
[Fact]
[Trait("Story", "ACF-###")]
[Trait("Category", "Unit")]  // or "Integration"
```

## Output

```markdown
## Tests Generated for <Target>

### Files Created
- tests/Project.UnitTests/<ClassName>Tests.cs

### Test Cases (5)
| Test | Scenario | Linked Story |
|------|----------|--------------|
| CreateOrder_ValidInput_ReturnsOrderId | Happy path | ACF-042 |
| CreateOrder_NullCustomerId_ThrowsArgumentNullException | Null guard | ACF-042 |
| CreateOrder_EmptyItems_ThrowsValidationException | Validation | ACF-042 |
| CreateOrder_CustomerNotFound_ReturnsFailure | Error handling | ACF-042 |
| CreateOrder_ConcurrentAccess_HandlesCorrectly | Concurrency | ACF-042 |

### Next Steps
1. Review generated tests
2. Run: dotnet test
3. Check coverage: /coverage
```

## Example

```
User: /add-test unit OrderService --story ACF-042

Claude: Analyzing OrderService for test generation...

Found methods to test:
1. CreateOrderAsync(CreateOrderCommand)
2. GetOrderByIdAsync(Guid)
3. UpdateOrderStatusAsync(Guid, OrderStatus)

Generating tests with story trait ACF-042...

Created: tests/Project.UnitTests/Services/OrderServiceTests.cs

Test cases generated:
- CreateOrderAsync_ValidCommand_ReturnsOrderId
- CreateOrderAsync_NullCommand_ThrowsArgumentNullException
- CreateOrderAsync_InvalidCustomer_ReturnsFailure
- GetOrderByIdAsync_ExistingOrder_ReturnsOrder
- GetOrderByIdAsync_NonExistentOrder_ReturnsNull
- UpdateOrderStatusAsync_ValidTransition_UpdatesStatus
- UpdateOrderStatusAsync_InvalidTransition_ThrowsInvalidOperationException

Run tests: dotnet test --filter "FullyQualifiedName~OrderServiceTests"
```
