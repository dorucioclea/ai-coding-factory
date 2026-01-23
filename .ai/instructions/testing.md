# Testing Standards

## Test Pyramid

```
        /\
       /  \      E2E Tests (few)
      /────\
     /      \    Integration Tests (some)
    /────────\
   /          \  Unit Tests (many)
  /────────────\
```

### Coverage Requirements

| Layer | Minimum Coverage | Target |
|-------|------------------|--------|
| Domain | 80% | 90% |
| Application | 80% | 85% |
| Infrastructure | 60% | 70% |
| API | 50% | 60% |

## Test Project Structure

```
tests/
├── ProjectName.UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   ├── Application/
│   │   ├── Commands/
│   │   └── Queries/
│   └── _Setup/
│       ├── Fixtures.cs
│       └── Builders/
├── ProjectName.IntegrationTests/
│   ├── Api/
│   │   ├── OrdersControllerTests.cs
│   │   └── CustomersControllerTests.cs
│   ├── Repositories/
│   └── _Setup/
│       ├── TestWebApplicationFactory.cs
│       └── DatabaseFixture.cs
└── ProjectName.ArchitectureTests/
    ├── LayerDependencyTests.cs
    └── NamingConventionTests.cs
```

## Naming Conventions

### Test Method Names
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public void Create_WithValidData_ReturnsSuccess()

[Fact]
public void Create_WithNullCustomerId_ThrowsArgumentException()

[Fact]
public async Task Handle_WhenOrderNotFound_ReturnsNotFound()
```

### Test Class Names
```csharp
// Unit tests: {ClassName}Tests
public class OrderTests { }
public class CreateOrderHandlerTests { }

// Integration tests: {ClassName}IntegrationTests
public class OrdersControllerIntegrationTests { }
```

## Test Attributes

### Story Traceability
```csharp
[Fact]
[Trait("Story", "ACF-042")]  // Links to user story
[Trait("Category", "Unit")]   // Test category
public void Order_AddLine_IncreasesTotalAmount()
{
    // ...
}
```

### Theory with Data
```csharp
[Theory]
[Trait("Story", "ACF-042")]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void Order_AddLine_WithInvalidQuantity_ThrowsException(int quantity)
{
    // ...
}
```

## Unit Test Examples

### Entity Tests
```csharp
public class OrderTests
{
    [Fact]
    [Trait("Story", "ACF-042")]
    public void Create_WithValidCustomerId_CreatesOrderInDraftStatus()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid());

        // Act
        var order = Order.Create(customerId);

        // Assert
        order.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Draft);
        order.CustomerId.Should().Be(customerId);
        order.Lines.Should().BeEmpty();
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    public void AddLine_WhenOrderIsConfirmed_ThrowsDomainException()
    {
        // Arrange
        var order = OrderBuilder.CreateConfirmed();
        var product = ProductBuilder.Create();

        // Act
        var act = () => order.AddLine(product, 1);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Cannot modify confirmed order");
    }
}
```

### Command Handler Tests
```csharp
public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateOrderHandler(
            _orderRepoMock.Object,
            _customerRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    public async Task Handle_WithExistingCustomer_CreatesOrderSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CustomerBuilder.Create(customerId);
        _customerRepoMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new CreateOrderCommand(customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _orderRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    public async Task Handle_WithNonExistentCustomer_ReturnsFailure()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _customerRepoMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateOrderCommand(customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Customer not found");

        _orderRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

### Validator Tests
```csharp
public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator;

    public CreateOrderValidatorTests()
    {
        _validator = new CreateOrderValidator();
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    public void Validate_WithEmptyCustomerId_ReturnsError()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
              .WithErrorMessage("Customer ID is required");
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    public void Validate_WithValidCustomerId_PassesValidation()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

## Integration Test Examples

### API Integration Tests
```csharp
public class OrdersControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public OrdersControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    [Trait("Category", "Integration")]
    public async Task CreateOrder_WithValidData_Returns201Created()
    {
        // Arrange
        var customerId = await _factory.SeedCustomerAsync();
        var request = new CreateOrderRequest(customerId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var orderId = await response.Content.ReadFromJsonAsync<Guid>();
        orderId.Should().NotBeEmpty();

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    [Trait("Story", "ACF-042")]
    [Trait("Category", "Integration")]
    public async Task GetOrder_WithExistingId_ReturnsOrder()
    {
        // Arrange
        var orderId = await _factory.SeedOrderAsync();

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(orderId);
    }
}
```

### Test Web Application Factory
```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test DbContext with TestContainers
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task<Guid> SeedCustomerAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = Customer.Create("Test Customer", "test@example.com");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer.Id;
    }
}
```

## Architecture Tests

```csharp
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Order).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateOrderCommand).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain should not reference Application. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

## Test Builders

```csharp
public static class OrderBuilder
{
    public static Order Create()
    {
        return Order.Create(CustomerId.Create(Guid.NewGuid()));
    }

    public static Order CreateConfirmed()
    {
        var order = Create();
        order.AddLine(ProductBuilder.Create(), 1);
        order.Confirm();
        return order;
    }

    public static Order WithCustomer(Guid customerId)
    {
        return Order.Create(CustomerId.Create(customerId));
    }
}

public static class ProductBuilder
{
    public static Product Create()
    {
        return new Product(
            Guid.NewGuid(),
            "Test Product",
            Money.Create(9.99m, "USD"));
    }
}
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "Category=Unit"

# Run for specific story
dotnet test --filter "Story=ACF-042"

# Run with detailed output
dotnet test -v detailed

# Generate coverage report
reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html
```
