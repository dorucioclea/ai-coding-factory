# ADR-{{NUMBER}}: API Versioning Strategy

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to establish an API versioning strategy for {{PROJECT_NAME}}. Considerations:

- **API consumers**: [Internal | External | Both]
- **Breaking change frequency**: [Rare | Occasional | Frequent]
- **Backward compatibility requirement**: {{COMPATIBILITY_PERIOD}}
- **Number of concurrent versions**: {{MAX_VERSIONS}}

### Current API Surface
<!-- Describe current API state -->
- Endpoints: {{ENDPOINT_COUNT}}
- External consumers: {{CONSUMER_COUNT}}

### Versioning Triggers
Breaking changes that require new version:
- Removing or renaming fields
- Changing field types
- Removing endpoints
- Changing authentication
- Modifying response structure

## Options Considered

### Option 1: URL Path Versioning
**Format**: `/api/v1/products`, `/api/v2/products`

**Pros**:
- Highly visible and explicit
- Easy to understand
- Simple to implement
- Easy to route/cache
- Works with all HTTP clients

**Cons**:
- Changes URL structure
- Can lead to URL proliferation
- Harder to sunset gracefully

**Implementation**:
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetV1() => Ok(new { version = "1.0" });
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetV2() => Ok(new { version = "2.0", enhanced = true });
}
```

### Option 2: Query String Versioning
**Format**: `/api/products?api-version=1.0`

**Pros**:
- Doesn't change URL path
- Easy to add to existing APIs
- Optional parameter

**Cons**:
- Less visible
- Can be overlooked
- Caching complexity
- Query string clutter

**Implementation**:
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
});
```

### Option 3: Header Versioning
**Format**: `X-Api-Version: 1.0` or `Accept: application/vnd.api.v1+json`

**Pros**:
- Clean URLs
- Follows HTTP semantics
- Good for content negotiation

**Cons**:
- Not visible in URL
- Harder to test in browser
- Requires header manipulation
- Documentation challenges

**Implementation**:
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
});

// Or media type versioning
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new MediaTypeApiVersionReader("v");
});
```

### Option 4: No Versioning (Evolutionary)
**Strategy**: Additive changes only, no breaking changes

**Pros**:
- Simplest approach
- Single codebase
- No version management

**Cons**:
- Constrains API evolution
- Technical debt accumulation
- May not work for all scenarios

**Rules**:
- Only add new fields (never remove)
- New endpoints only
- Optional parameters only
- Deprecation via documentation

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

### Versioning Policy
- **Major version**: Breaking changes (v1 → v2)
- **Minor version**: Non-breaking additions (v1.0 → v1.1)
- **Support window**: {{SUPPORT_WINDOW}} for each major version
- **Deprecation notice**: {{DEPRECATION_NOTICE}} before sunset

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Migration Strategy
When releasing new major version:
1. Announce deprecation of old version
2. Provide migration guide
3. Maintain both versions for {{OVERLAP_PERIOD}}
4. Sunset old version with notice

## Implementation Notes

### Package Dependencies
```xml
<PackageReference Include="Asp.Versioning.Mvc" Version="8.0.0" />
<PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.0.0" />
```

### Controller Organization
```
src/API/Controllers/
├── V1/
│   ├── ProductsController.cs
│   └── OrdersController.cs
└── V2/
    ├── ProductsController.cs
    └── OrdersController.cs
```

### Swagger Configuration
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "API", Version = "v2" });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
});
```

### Version Response Header
```csharp
// Clients see supported versions in response
api-supported-versions: 1.0, 2.0
api-deprecated-versions: 1.0
```

### Deprecation Attributes
```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
```

## References
- [Microsoft API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [REST API Versioning Best Practices](https://www.freecodecamp.org/news/how-to-version-a-rest-api/)
- [API Changelog Best Practices](https://nordicapis.com/api-changelog-best-practices/)
