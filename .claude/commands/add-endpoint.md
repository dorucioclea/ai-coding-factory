# /add-endpoint - Add API Endpoint

Add a new API endpoint with full CQRS implementation.

## Usage
```
/add-endpoint <HttpMethod> <Route> [options]
```

Examples:
- `/add-endpoint GET /orders/{id}`
- `/add-endpoint POST /orders`
- `/add-endpoint PUT /orders/{id}`
- `/add-endpoint DELETE /orders/{id}`

Options:
- `--auth` - Require authentication
- `--role <role>` - Require specific role
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Parse Route Information

Extract:
- HTTP method (GET, POST, PUT, DELETE, PATCH)
- Route template
- Route parameters
- Controller name (from route)

### 2. Determine Required Components

| HTTP Method | Components |
|-------------|------------|
| GET (single) | Query + Handler + DTO |
| GET (list) | Query + Handler + DTO |
| POST | Command + Handler + Validator + Request |
| PUT | Command + Handler + Validator + Request |
| DELETE | Command + Handler |
| PATCH | Command + Handler + Validator + Request |

### 3. Generate Files

**For GET endpoint**:
```csharp
// Query
public record Get<Entity>ByIdQuery(Guid Id) : IRequest<<Entity>Dto?>;

// Handler
public class Get<Entity>ByIdHandler : IRequestHandler<Get<Entity>ByIdQuery, <Entity>Dto?>
{
    // Implementation...
}

// Controller action
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(<Entity>Dto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
{
    var result = await _mediator.Send(new Get<Entity>ByIdQuery(id), ct);
    return result is null ? NotFound() : Ok(result);
}
```

**For POST endpoint**:
```csharp
// Command
public record Create<Entity>Command(/* properties */) : IRequest<Result<Guid>>;

// Handler
public class Create<Entity>Handler : IRequestHandler<Create<Entity>Command, Result<Guid>>
{
    // Implementation...
}

// Validator
public class Create<Entity>Validator : AbstractValidator<Create<Entity>Command>
{
    // Validation rules...
}

// Controller action
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] Create<Entity>Request request, CancellationToken ct)
{
    var command = new Create<Entity>Command(/* map request */);
    var result = await _mediator.Send(command, ct);

    return result.IsFailure
        ? BadRequest(result.Error)
        : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
}
```

### 4. Generate Tests

Create integration tests for the endpoint:
```csharp
[Fact]
[Trait("Story", "<StoryId>")]
public async Task <Method>_<Scenario>_Returns<Status>()
{
    // Test implementation
}
```

### 5. Update OpenAPI Documentation

Ensure proper XML comments and attributes:
```csharp
/// <summary>
/// Creates a new <Entity>.
/// </summary>
/// <param name="request">The creation request</param>
/// <returns>The created entity ID</returns>
/// <response code="201">Returns the created entity ID</response>
/// <response code="400">If the request is invalid</response>
```

## Templates Used

- `.claude/templates/controller.cs.template`
- `.claude/templates/command-handler.cs.template`
- `.claude/templates/query-handler.cs.template`
- `.claude/templates/integration-test.cs.template`

## Output

```markdown
## Endpoint Created: <Method> <Route>

### Files Created/Modified
- src/Project.Api/Controllers/<Controller>Controller.cs (modified)
- src/Project.Application/Commands/<Feature>/Create<Entity>Command.cs
- src/Project.Application/Commands/<Feature>/Create<Entity>Handler.cs
- src/Project.Application/Commands/<Feature>/Create<Entity>Validator.cs
- tests/Project.IntegrationTests/Api/<Controller>Tests.cs

### Endpoint Details
- Method: POST
- Route: /api/orders
- Auth: Required
- Role: None

### Test Coverage
- Create_ValidRequest_ReturnsCreated
- Create_InvalidRequest_ReturnsBadRequest
```

## Example

```
User: /add-endpoint POST /orders --auth --story ACF-042

Claude: Creating POST /orders endpoint...

This will create:
1. CreateOrderCommand + Handler + Validator
2. Controller action with [Authorize]
3. Integration tests linked to ACF-042

What properties should CreateOrderRequest have?
- CustomerId (Guid)
- Items (List<OrderItemRequest>)
- etc.

[After gathering info, generates all files]

Endpoint created: POST /api/orders
```
