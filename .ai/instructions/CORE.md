# Core AI Instructions

These instructions apply to ALL AI coding assistants working in this repository.

## Mission

AI Coding Factory is an enterprise .NET software delivery platform that uses AI-assisted development to automate the complete development lifecycle with strict governance, traceability, security, and quality standards.

## Non-Negotiables

1. **Privacy-First**: All inference must be local. Never require external AI services for core functionality.
2. **Enterprise .NET**: Target .NET 8+ with Clean Architecture.
3. **Container-First**: Everything must be Kubernetes-ready from day one.
4. **Traceability**: Enforce story → test → commit → release chain.
5. **Verifiable**: Every claim must be backed by scripts or CI.
6. **Security by Default**: No secrets in code, least privilege, input validation.

## Governance

**ALWAYS follow `CORPORATE_RND_POLICY.md`** as the authoritative governance document.

### Story ID Convention

- Format: `ACF-###` (e.g., `ACF-001`, `ACF-042`)
- Story IDs MUST appear in:
  - Story files: `artifacts/stories/ACF-###.md`
  - Test traits: `[Trait("Story", "ACF-###")]`
  - Commit messages: `ACF-### Description of change`
  - Release notes: Linked story references

### Definition of Done

A story is complete when:
- [ ] Acceptance criteria met and linked to `ACF-###`
- [ ] Tests exist and pass in CI
- [ ] Coverage ≥80% for Domain/Application layers
- [ ] Traceability report includes story and tests
- [ ] Documentation updated
- [ ] Security/privacy checks completed
- [ ] No high/critical vulnerabilities

## Clean Architecture

### Layer Dependencies (CRITICAL)

```
Domain ← Application ← Infrastructure ← API
  ↓           ↓              ↓            ↓
No deps    Domain only   Domain+App    All
```

**Rules**:
- Domain has ZERO external dependencies
- Application depends ONLY on Domain
- Infrastructure depends on Domain + Application
- API is the composition root

### Layer Contents

| Layer | Contains |
|-------|----------|
| Domain | Entities, Value Objects, Aggregates, Domain Events, Repository Interfaces |
| Application | Commands/Queries (CQRS), DTOs, Validators, Behaviors |
| Infrastructure | EF Core, Repository Implementations, External Services |
| API | Controllers, Middleware, Filters, DI Configuration |

## Testing Standards

### Test Pyramid
- **Unit Tests**: >80% coverage for Domain/Application
- **Integration Tests**: All critical API endpoints
- **Architecture Tests**: Validate dependency rules

### Test Naming Convention
```csharp
[Fact]
[Trait("Story", "ACF-###")]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange, Act, Assert
}
```

### Test Frameworks
- xUnit (runner)
- FluentAssertions (assertions)
- Moq (mocking)
- TestContainers (integration)
- Coverlet (coverage)
- NetArchTest (architecture)

## Security Guidelines

### Always Do
- Use parameterized queries (EF Core)
- Validate all inputs (FluentValidation)
- Use IConfiguration for settings
- Implement proper authentication (JWT)
- Log with ILogger (structured)
- Use HTTPS in production

### Never Do
- Hardcode secrets, tokens, or keys
- Use raw SQL strings
- Trust user input without validation
- Use Console.WriteLine for logging
- Commit .env files
- Disable security features

## .NET Standards

```csharp
// Target .NET 8 LTS
<TargetFramework>net8.0</TargetFramework>

// Async for all I/O
public async Task<Result> ProcessAsync(CancellationToken ct)

// Specific exceptions
throw new InvalidOperationException("State invalid");

// Structured logging
_logger.LogInformation("Processing {OrderId}", order.Id);

// Constructor injection only
public class Service(IRepository repo, ILogger<Service> logger)

// Proper resource disposal
await using var stream = File.OpenRead(path);
```

## Commit Message Format

```
ACF-### Brief description

- Detailed bullet point 1
- Detailed bullet point 2

Co-Authored-By: AI Assistant <noreply@example.com>
```

## File Locations

| Artifact | Location |
|----------|----------|
| Stories | `artifacts/stories/ACF-###.md` |
| ADRs | `docs/architecture/adr/` |
| Traceability | `artifacts/traceability/` |
| Release notes | `artifacts/traceability/release-notes.md` |

## Validation Commands

```bash
# Platform validation
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh

# Traceability
python3 scripts/traceability/traceability.py validate

# Scaffold verification
./scripts/scaffold-and-verify.sh
```

## Remember

- **Quality over speed**: Trade-offs require ADRs
- **Test-first**: Every behavior change needs tests
- **Traceability**: Story IDs everywhere
- **No hallucinations**: Don't claim without evidence
- **Smallest viable change**: Avoid unnecessary refactors
