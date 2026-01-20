# ADR-{{NUMBER}}: Message Queue Selection

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to implement asynchronous messaging for {{PROJECT_NAME}}. Requirements:

- **Use cases**: [Event-driven | Task queue | Pub/Sub | Request-Reply]
- **Message volume**: {{VOLUME}} messages/second
- **Delivery guarantee**: [At-least-once | At-most-once | Exactly-once]
- **Message ordering**: [Required | Not required]
- **Message size**: {{MESSAGE_SIZE}}

### Integration Patterns Needed
- [ ] Point-to-point messaging
- [ ] Publish-subscribe
- [ ] Request-reply
- [ ] Dead letter handling
- [ ] Delayed/scheduled messages
- [ ] Message deduplication

### Current Architecture
<!-- Describe current message flow if any -->

## Options Considered

### Option 1: RabbitMQ
**Type**: Traditional message broker

**Pros**:
- Mature and battle-tested
- Rich routing capabilities (exchanges, bindings)
- Multiple protocols (AMQP, MQTT, STOMP)
- Management UI included
- Strong .NET support (MassTransit, EasyNetQ)

**Cons**:
- Single point of failure (needs clustering)
- Not designed for very high throughput
- Operational complexity at scale

**Best for**: Traditional enterprise messaging, complex routing

**Implementation**:
```csharp
// Using MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});
```

### Option 2: Apache Kafka
**Type**: Distributed event streaming

**Pros**:
- Extremely high throughput
- Durable message storage
- Replay capability
- Horizontal scaling
- Event sourcing friendly

**Cons**:
- Operational complexity
- Overkill for simple use cases
- Higher latency than RabbitMQ
- Steep learning curve

**Best for**: Event streaming, high volume, event sourcing

**Implementation**:
```csharp
// Using Confluent.Kafka
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
    return new ProducerBuilder<string, string>(config).Build();
});
```

### Option 3: Azure Service Bus
**Type**: Cloud-managed message broker

**Pros**:
- Fully managed
- Enterprise features (sessions, dedup, scheduled)
- Dead letter queues built-in
- AMQP support
- SLA-backed

**Cons**:
- Azure lock-in
- Cost at scale
- Network latency for on-prem

**Best for**: Azure-hosted applications, enterprise features

**Implementation**:
```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(configuration.GetConnectionString("ServiceBus"));
        cfg.ConfigureEndpoints(context);
    });
});
```

### Option 4: Amazon SQS/SNS
**Type**: Cloud-managed queue/topic

**Pros**:
- Fully managed
- High availability
- Pay-per-use
- Simple API
- Integrates with AWS ecosystem

**Cons**:
- AWS lock-in
- Limited features compared to Service Bus
- 256KB message limit

**Best for**: AWS-hosted applications, simple queuing

### Option 5: In-Process (MediatR + Channels)
**Type**: In-memory, single process

**Pros**:
- No external dependencies
- Zero latency
- Simple to implement
- Good for CQRS

**Cons**:
- Not distributed
- Lost on restart
- Single process only

**Best for**: Monolith, domain events, CQRS

**Implementation**:
```csharp
// MediatR for domain events
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Channel for background processing
var channel = Channel.CreateBounded<WorkItem>(100);
builder.Services.AddSingleton(channel);
builder.Services.AddHostedService<WorkItemProcessor>();
```

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

### Message Types and Queues
| Message Type | Queue/Topic | Consumer | Retry Policy |
|--------------|-------------|----------|--------------|
| {{MESSAGE_1}} | {{QUEUE_1}} | {{CONSUMER_1}} | {{RETRY_1}} |
| {{MESSAGE_2}} | {{QUEUE_2}} | {{CONSUMER_2}} | {{RETRY_2}} |

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Operational Considerations
- Monitoring: {{MONITORING_APPROACH}}
- Alerting: Queue depth, consumer lag, DLQ count
- Scaling: {{SCALING_STRATEGY}}

## Implementation Notes

### Message Contract
```csharp
public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt);
```

### Consumer Pattern
```csharp
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        // Process message
    }
}
```

### Error Handling
- Retry policy: Exponential backoff (1s, 5s, 30s)
- Dead letter after: 3 attempts
- DLQ monitoring: Required

### Idempotency
Messages must be processed idempotently using:
- Message ID tracking
- Idempotent operations
- Deduplication window

## References
- [MassTransit Documentation](https://masstransit-project.com/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [Azure Service Bus Documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
