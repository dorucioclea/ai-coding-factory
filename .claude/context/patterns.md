# Code Patterns

Standard patterns to follow when generating code in this repository.

## Entity Pattern

```csharp
namespace Project.Domain.Entities;

public class Order : BaseEntity, IAggregateRoot
{
    // Private setters for encapsulation
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }

    // Navigation properties
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Factory method for creation
    public static Order Create(Guid customerId)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Draft
        };

        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }

    // Business methods
    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateException("Cannot add items to non-draft order");

        _items.Add(new OrderItem(product, quantity));
    }

    // Private parameterless constructor for EF
    private Order() { }
}
```

## Value Object Pattern

```csharp
namespace Project.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

## Command Pattern (CQRS)

```csharp
// Command
namespace Project.Application.Commands.Orders;

public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items
) : IRequest<Result<Guid>>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

        var order = Order.Create(request.CustomerId);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.Quantity);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}

// Validator
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");
    }
}
```

## Query Pattern (CQRS)

```csharp
// Query
namespace Project.Application.Queries.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

// Handler
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order?.ToDto();
    }
}
```

## Repository Interface (Domain)

```csharp
namespace Project.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
}
```

## Repository Implementation (Infrastructure)

```csharp
namespace Project.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders.FindAsync(new object[] { id }, ct);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}
```

## Controller Pattern

```csharp
namespace Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.CustomerId, request.Items);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);

        if (order is null)
            return NotFound();

        return Ok(order);
    }
}
```

## Unit Test Pattern

```csharp
namespace Project.UnitTests.Application.Orders;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        _handler = new CreateOrderHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    [Trait("Story", "ACF-001")]
    public async Task Handle_ValidCommand_ReturnsSuccessWithOrderId()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new(ProductId: Guid.NewGuid(), Quantity: 2)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _orderRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [Trait("Story", "ACF-001")]
    public async Task Handle_EmptyItems_ReturnsFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
```

## Integration Test Pattern

```csharp
namespace Project.IntegrationTests.Api;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    [Trait("Story", "ACF-001")]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var orderId = await response.Content.ReadFromJsonAsync<Guid>();
        orderId.Should().NotBeEmpty();
    }
}
```
