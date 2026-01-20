# ADR-{{NUMBER}}: Deployment Strategy

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to establish a deployment strategy for {{PROJECT_NAME}}. Requirements:

- **Availability target**: {{AVAILABILITY_SLA}}
- **Deployment frequency**: {{DEPLOYMENT_FREQUENCY}}
- **Rollback time**: {{ROLLBACK_TIME}}
- **Zero-downtime**: [Required | Preferred | Not required]
- **Environment count**: {{ENVIRONMENTS}}

### Current State
<!-- Describe current deployment process -->

### Constraints
- Infrastructure: {{INFRASTRUCTURE}}
- Team size: {{TEAM_SIZE}}
- Budget: {{BUDGET}}

## Options Considered

### Option 1: Rolling Deployment
**Type**: Gradual instance replacement

**How it works**:
1. Deploy to subset of instances
2. Health check passes
3. Continue to next subset
4. Complete when all instances updated

**Pros**:
- Zero downtime
- Gradual rollout
- Easy rollback (stop deployment)
- Resource efficient

**Cons**:
- Mixed versions during deployment
- Database migrations complexity
- Longer deployment time

**Best for**: Stateless applications, frequent deployments

**Kubernetes Implementation**:
```yaml
apiVersion: apps/v1
kind: Deployment
spec:
  replicas: 4
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
```

### Option 2: Blue-Green Deployment
**Type**: Full environment switch

**How it works**:
1. Deploy to inactive environment (Green)
2. Test Green environment
3. Switch traffic from Blue to Green
4. Blue becomes standby for rollback

**Pros**:
- Instant rollback (switch back)
- Full testing before go-live
- No mixed versions
- Clean separation

**Cons**:
- Double infrastructure cost
- Database state synchronization
- DNS/LB propagation time

**Best for**: Critical applications, infrequent releases

**Implementation**:
```yaml
# Blue environment
apiVersion: v1
kind: Service
metadata:
  name: app-blue
spec:
  selector:
    app: myapp
    version: blue

# Green environment
apiVersion: v1
kind: Service
metadata:
  name: app-green
spec:
  selector:
    app: myapp
    version: green

# Production service (switch selector)
apiVersion: v1
kind: Service
metadata:
  name: app-production
spec:
  selector:
    app: myapp
    version: green  # Switch between blue/green
```

### Option 3: Canary Deployment
**Type**: Gradual traffic shifting

**How it works**:
1. Deploy new version to small subset
2. Route small % of traffic to canary
3. Monitor metrics and errors
4. Gradually increase traffic
5. Complete or rollback based on metrics

**Pros**:
- Risk mitigation
- Real user testing
- Metrics-driven decisions
- Gradual exposure

**Cons**:
- Complex traffic management
- Requires good monitoring
- Longer deployment cycle
- Session affinity challenges

**Best for**: High-traffic applications, risk-averse releases

**Kubernetes with Istio**:
```yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
spec:
  http:
  - route:
    - destination:
        host: app
        subset: stable
      weight: 90
    - destination:
        host: app
        subset: canary
      weight: 10
```

### Option 4: Feature Flags
**Type**: Code-level deployment control

**How it works**:
1. Deploy code with features behind flags
2. Enable flags for specific users/groups
3. Gradually increase flag coverage
4. Remove flag when fully rolled out

**Pros**:
- Decouple deploy from release
- Instant rollback (toggle flag)
- A/B testing capability
- Targeted rollouts

**Cons**:
- Code complexity
- Technical debt (old flags)
- Testing matrix explosion
- Flag management overhead

**Best for**: Frequent releases, experimentation culture

**Implementation**:
```csharp
// Using Microsoft.FeatureManagement
builder.Services.AddFeatureManagement();

[FeatureGate("NewCheckoutFlow")]
[HttpPost("checkout")]
public async Task<IActionResult> NewCheckout()
{
    // New implementation
}
```

### Option 5: Recreate (Big Bang)
**Type**: Stop old, start new

**How it works**:
1. Stop all old instances
2. Deploy new version
3. Start all new instances

**Pros**:
- Simple
- No mixed versions
- Clean state

**Cons**:
- Downtime required
- High risk
- No gradual rollout

**Best for**: Dev/test environments, maintenance windows

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

### Strategy by Environment
| Environment | Strategy | Approval | Rollback |
|-------------|----------|----------|----------|
| Development | Recreate | Auto | Auto |
| Staging | Rolling | Auto | Manual |
| Production | {{PROD_STRATEGY}} | Manual | {{ROLLBACK}} |

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Prerequisites
- [ ] Health check endpoints
- [ ] Graceful shutdown handling
- [ ] Database migration strategy
- [ ] Monitoring and alerting
- [ ] Rollback runbook

## Implementation Notes

### Health Check Requirements
```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Always returns healthy
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Graceful Shutdown
```csharp
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Handle SIGTERM
lifetime.ApplicationStopping.Register(() =>
{
    // Complete in-flight requests
    // Close connections gracefully
});
```

### Database Migration Strategy
- **Backward compatible**: Run migrations before deployment
- **Breaking changes**: Use expand-contract pattern
  1. Expand: Add new column/table
  2. Migrate: Move data
  3. Contract: Remove old column/table (next release)

### Rollback Procedure
1. Detect issue (monitoring/alerts)
2. Decision to rollback
3. Execute rollback (strategy-specific)
4. Verify rollback success
5. Post-mortem

### Deployment Checklist
- [ ] Database migrations tested
- [ ] Health checks passing
- [ ] Monitoring configured
- [ ] Rollback tested
- [ ] Stakeholders notified

## References
- [Kubernetes Deployment Strategies](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Martin Fowler - Blue Green Deployment](https://martinfowler.com/bliki/BlueGreenDeployment.html)
- [Feature Flags Best Practices](https://launchdarkly.com/blog/feature-flag-best-practices/)
