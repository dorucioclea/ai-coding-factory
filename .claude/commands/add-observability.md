# /add-observability - Add Observability Stack

Add comprehensive observability with structured logging, distributed tracing, and metrics.

## Usage
```
/add-observability [options]
```

Options:
- `--logging <serilog|nlog>` - Logging framework (default: serilog)
- `--tracing <jaeger|zipkin|otlp>` - Tracing exporter (default: otlp)
- `--metrics` - Include Prometheus metrics
- `--health-checks` - Include Kubernetes health checks
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Add Required NuGet Packages

**Serilog packages:**
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Enrichers.Span" Version="3.1.0" />
```

**OpenTelemetry packages:**
```xml
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.11" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.7.0-rc.1" />
```

**Health check packages:**
```xml
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="8.0.0" />
```

### 2. Create Observability Structure

```
src/{{ProjectName}}.Infrastructure/Observability/
├── Logging/
│   ├── LoggingExtensions.cs
│   └── CorrelationIdMiddleware.cs
├── Tracing/
│   └── TracingExtensions.cs
├── Metrics/
│   ├── MetricsExtensions.cs
│   └── ApplicationMetrics.cs
└── HealthChecks/
    └── HealthCheckExtensions.cs
```

### 3. Generate Logging Configuration

**LoggingExtensions.cs**:
```csharp
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace {{Namespace}}.Infrastructure.Observability.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseStructuredLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var environment = context.HostingEnvironment.EnvironmentName;
            var applicationName = context.HostingEnvironment.ApplicationName;

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", environment);

            // Development: Human-readable console output
            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                // Production: JSON format for log aggregation
                loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
            }

            // Optional: Seq for local development
            var seqUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrEmpty(seqUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqUrl);
            }
        });
    }
}
```

**CorrelationIdMiddleware.cs**:
```csharp
using System.Diagnostics;
using Serilog.Context;

namespace {{Namespace}}.Infrastructure.Observability.Logging;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Add to log context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Add to Activity for distributed tracing
            Activity.Current?.SetTag("correlation.id", correlationId);

            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            && !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
```

### 4. Generate Tracing Configuration

**TracingExtensions.cs**:
```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace {{Namespace}}.Infrastructure.Observability.Tracing;

public static class TracingExtensions
{
    public static IServiceCollection AddDistributedTracing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "{{ProjectName}}";
        var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health") &&
                            !context.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    });

                // Configure exporter
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    tracing.AddConsoleExporter();
                }
            });

        return services;
    }
}
```

### 5. Generate Metrics Configuration

**MetricsExtensions.cs**:
```csharp
using OpenTelemetry.Metrics;

namespace {{Namespace}}.Infrastructure.Observability.Metrics;

public static class MetricsExtensions
{
    public static IServiceCollection AddApplicationMetrics(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "{{ProjectName}}";

        services.AddSingleton<ApplicationMetrics>();

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
```

**ApplicationMetrics.cs**:
```csharp
using System.Diagnostics.Metrics;

namespace {{Namespace}}.Infrastructure.Observability.Metrics;

public class ApplicationMetrics
{
    private readonly Counter<long> _requestsTotal;
    private readonly Counter<long> _errorsTotal;
    private readonly Histogram<double> _requestDuration;
    private readonly Histogram<double> _dbQueryDuration;
    private readonly ObservableGauge<int> _activeConnections;

    private int _currentActiveConnections;

    public ApplicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("{{ProjectName}}.Metrics");

        _requestsTotal = meter.CreateCounter<long>(
            "app_requests_total",
            description: "Total number of requests processed");

        _errorsTotal = meter.CreateCounter<long>(
            "app_errors_total",
            description: "Total number of errors");

        _requestDuration = meter.CreateHistogram<double>(
            "app_request_duration_seconds",
            unit: "s",
            description: "Request duration in seconds");

        _dbQueryDuration = meter.CreateHistogram<double>(
            "app_db_query_duration_seconds",
            unit: "s",
            description: "Database query duration in seconds");

        _activeConnections = meter.CreateObservableGauge(
            "app_active_connections",
            () => _currentActiveConnections,
            description: "Number of active connections");
    }

    public void RecordRequest(string endpoint, string method, int statusCode, double durationSeconds)
    {
        var tags = new TagList
        {
            { "endpoint", endpoint },
            { "method", method },
            { "status_code", statusCode.ToString() }
        };

        _requestsTotal.Add(1, tags);
        _requestDuration.Record(durationSeconds, tags);

        if (statusCode >= 500)
        {
            _errorsTotal.Add(1, new TagList { { "type", "server_error" } });
        }
        else if (statusCode >= 400)
        {
            _errorsTotal.Add(1, new TagList { { "type", "client_error" } });
        }
    }

    public void RecordDatabaseQuery(string operation, double durationSeconds)
    {
        _dbQueryDuration.Record(durationSeconds, new TagList { { "operation", operation } });
    }

    public void IncrementActiveConnections() => Interlocked.Increment(ref _currentActiveConnections);
    public void DecrementActiveConnections() => Interlocked.Decrement(ref _currentActiveConnections);
}
```

### 6. Generate Health Check Configuration

**HealthCheckExtensions.cs**:
```csharp
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace {{Namespace}}.Infrastructure.Observability.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Self check (always healthy)
        healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

        // Database check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "database",
                tags: new[] { "ready", "db" });
        }

        // Redis check
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecksBuilder.AddRedis(
                redisConnection,
                name: "redis",
                tags: new[] { "ready", "cache" });
        }

        return services;
    }

    public static IApplicationBuilder UseApplicationHealthChecks(this IApplicationBuilder app)
    {
        // Kubernetes liveness probe
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Kubernetes readiness probe
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Full health status
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
```

### 7. Update Program.cs

Add to Program.cs:
```csharp
using {{Namespace}}.Infrastructure.Observability.Logging;
using {{Namespace}}.Infrastructure.Observability.Tracing;
using {{Namespace}}.Infrastructure.Observability.Metrics;
using {{Namespace}}.Infrastructure.Observability.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure structured logging
builder.Host.UseStructuredLogging();

// Add observability
builder.Services.AddDistributedTracing(builder.Configuration);
builder.Services.AddApplicationMetrics(builder.Configuration);
builder.Services.AddApplicationHealthChecks(builder.Configuration);

var app = builder.Build();

// Use observability middleware
app.UseCorrelationId();
app.UseSerilogRequestLogging();

// Health checks
app.UseApplicationHealthChecks();

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();
```

### 8. Update appsettings.json

```json
{
  "OpenTelemetry": {
    "ServiceName": "{{ProjectName}}",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://otel-collector:4317"
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Output

```markdown
## Observability Stack Added

### Files Created
**Infrastructure Layer:**
- Observability/Logging/LoggingExtensions.cs
- Observability/Logging/CorrelationIdMiddleware.cs
- Observability/Tracing/TracingExtensions.cs
- Observability/Metrics/MetricsExtensions.cs
- Observability/Metrics/ApplicationMetrics.cs
- Observability/HealthChecks/HealthCheckExtensions.cs

### NuGet Packages Added
- Serilog.AspNetCore
- OpenTelemetry.*
- AspNetCore.HealthChecks.*

### Endpoints
| Endpoint | Purpose |
|----------|---------|
| /health | Full health status |
| /health/live | Kubernetes liveness probe |
| /health/ready | Kubernetes readiness probe |
| /metrics | Prometheus metrics |

### Configuration Required
Update appsettings.json with OpenTelemetry and Serilog settings.

### Observability Stack
- **Logging**: Serilog with structured JSON output
- **Tracing**: OpenTelemetry with OTLP export
- **Metrics**: Prometheus-compatible metrics
- **Health**: Kubernetes-ready health checks

### Local Development
```bash
# Start observability stack
docker-compose -f docker-compose.observability.yml up -d

# Services:
# - Jaeger: http://localhost:16686
# - Prometheus: http://localhost:9090
# - Grafana: http://localhost:3000
# - Seq: http://localhost:5341
```
```

## Example

```
User: /add-observability --logging serilog --tracing otlp --metrics --health-checks --story ACF-070

Claude: Adding comprehensive observability stack...

Creating logging infrastructure:
1. Serilog with structured logging
2. Correlation ID middleware
3. Request logging enrichment

Creating tracing:
4. OpenTelemetry with OTLP exporter
5. ASP.NET Core instrumentation
6. EF Core instrumentation

Creating metrics:
7. Prometheus exporter
8. Custom application metrics
9. Runtime metrics

Creating health checks:
10. Liveness and readiness probes
11. Database health check
12. Redis health check

Observability stack added successfully.

Endpoints:
- /health/live - Liveness probe
- /health/ready - Readiness probe
- /metrics - Prometheus metrics
```
