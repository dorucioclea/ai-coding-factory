# ADR-{{NUMBER}}: Caching Strategy

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to implement caching for {{PROJECT_NAME}} to improve performance. Analysis shows:

- **Hot data**: {{HOT_DATA_DESCRIPTION}}
- **Cache hit ratio target**: {{TARGET_HIT_RATIO}}%
- **Latency requirement**: {{LATENCY_REQUIREMENT}}
- **Data freshness tolerance**: {{FRESHNESS_TOLERANCE}}

### Performance Bottlenecks
<!-- List current performance issues that caching would address -->
1. {{BOTTLENECK_1}}
2. {{BOTTLENECK_2}}

### Data Characteristics
- Read/Write ratio: {{READ_WRITE_RATIO}}
- Data size: {{DATA_SIZE}}
- Update frequency: {{UPDATE_FREQUENCY}}

## Options Considered

### Option 1: In-Memory Cache (IMemoryCache)
**Type**: Local, in-process

**Pros**:
- Zero latency (in-process)
- No external dependencies
- Simple to implement
- Built into ASP.NET Core

**Cons**:
- Not shared across instances
- Lost on app restart
- Consumes application memory
- Cache inconsistency in distributed deployment

**Best for**: Single instance, small datasets, computed values

**Implementation**:
```csharp
builder.Services.AddMemoryCache();

// Usage
public class ProductService(IMemoryCache cache)
{
    public async Task<Product> GetProductAsync(int id)
    {
        return await cache.GetOrCreateAsync($"product:{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);
            return await _repository.GetByIdAsync(id);
        });
    }
}
```

### Option 2: Distributed Cache (Redis)
**Type**: External, shared

**Pros**:
- Shared across all instances
- Survives app restarts
- Rich data structures
- Pub/sub for invalidation
- High performance

**Cons**:
- Network latency
- Additional infrastructure
- Serialization overhead
- Operational complexity

**Best for**: Multi-instance, large datasets, session state

**Implementation**:
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "app:";
});

// Usage with IDistributedCache
public class ProductService(IDistributedCache cache)
{
    public async Task<Product> GetProductAsync(int id)
    {
        var key = $"product:{id}";
        var cached = await cache.GetStringAsync(key);

        if (cached != null)
            return JsonSerializer.Deserialize<Product>(cached);

        var product = await _repository.GetByIdAsync(id);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

        return product;
    }
}
```

### Option 3: Hybrid (L1 + L2)
**Type**: Two-tier caching

**Pros**:
- Best of both worlds
- L1 for hot data (in-memory)
- L2 for warm data (Redis)
- Reduced network calls

**Cons**:
- More complex invalidation
- Consistency challenges
- More code to maintain

**Implementation**:
```csharp
// Using FusionCache or custom implementation
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(10),
        MemoryDuration = TimeSpan.FromMinutes(1)
    })
    .WithDistributedCache(
        new RedisCache(new RedisCacheOptions { Configuration = redisConn }));
```

### Option 4: Response Caching / CDN
**Type**: HTTP-level caching

**Pros**:
- Reduces server load
- Standard HTTP caching
- CDN edge caching
- Browser caching

**Cons**:
- Only for GET requests
- Limited to HTTP responses
- Cache-Control complexity
- Not for personalized content

**Best for**: Static content, public APIs, assets

**Implementation**:
```csharp
builder.Services.AddResponseCaching();

// Controller
[HttpGet("{id}")]
[ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "id" })]
public async Task<ActionResult<Product>> Get(int id)
```

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

### Cache Strategy by Data Type
| Data Type | Cache Level | TTL | Invalidation |
|-----------|-------------|-----|--------------|
| {{DATA_1}} | {{LEVEL_1}} | {{TTL_1}} | {{INVALIDATION_1}} |
| {{DATA_2}} | {{LEVEL_2}} | {{TTL_2}} | {{INVALIDATION_2}} |

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Risks
- **Stale data**: Mitigation - {{STALE_DATA_MITIGATION}}
- **Cache stampede**: Mitigation - Use cache locks or request coalescing
- **Memory pressure**: Mitigation - Set size limits and eviction policies

## Implementation Notes

### Cache Key Convention
```
{entity}:{id}:{version}
product:123:v1
user:456:profile
```

### Invalidation Strategy
- **Time-based**: TTL expiration
- **Event-based**: Invalidate on entity update
- **Manual**: Admin purge capability

### Monitoring
- Cache hit/miss ratio
- Eviction rate
- Memory usage
- Latency percentiles

## References
- [Caching Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Redis Documentation](https://redis.io/documentation)
- [FusionCache](https://github.com/ZiggyCreatures/FusionCache)
