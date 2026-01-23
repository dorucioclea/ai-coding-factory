# Tech Stack Document: {PROJECT_NAME}

**Version**: 1.0
**Date**: {DATE}
**Author**: {Author}
**Status**: Draft | Approved

---

## Overview

{1-2 sentences: High-level description of the technology choices}

---

## Stack Summary

| Layer | Technology | Version |
|-------|------------|---------|
| **Frontend** | Next.js (React) | 14.x |
| **UI Components** | shadcn/ui + Tailwind | Latest |
| **Backend** | ASP.NET Core | 8.0 |
| **API Style** | REST | - |
| **Database** | PostgreSQL | 16 |
| **Cache** | Redis | 7 |
| **Auth** | JWT + Refresh Tokens | - |
| **Container** | Docker | Latest |

---

## Frontend Stack

### Framework: Next.js 14

**Why**: Server-side rendering, file-based routing, React 18 features, excellent DX

| Feature | Technology | Rationale |
|---------|------------|-----------|
| Routing | App Router | Latest Next.js standard |
| Data Fetching | TanStack Query | Caching, mutations, optimistic updates |
| Forms | react-hook-form + zod | Type-safe validation |
| State (Client) | Zustand | Simple, minimal boilerplate |
| Styling | Tailwind CSS | Utility-first, design system |
| Components | shadcn/ui | Accessible, customizable, not a dependency |
| Icons | Lucide React | Consistent, tree-shakeable |
| Date handling | date-fns | Immutable, tree-shakeable |

### Package Versions

```json
{
  "next": "14.2.x",
  "react": "18.3.x",
  "@tanstack/react-query": "5.x",
  "react-hook-form": "7.x",
  "zod": "3.x",
  "zustand": "4.x",
  "tailwindcss": "3.4.x"
}
```

### Frontend Architecture

```
src/
├── app/                    # Next.js App Router pages
│   ├── (auth)/            # Auth-related routes (grouped)
│   ├── (dashboard)/       # Authenticated routes
│   └── api/               # API routes (if needed)
├── components/
│   ├── ui/                # shadcn/ui primitives
│   ├── features/          # Feature-specific components
│   └── layout/            # Layout components
├── hooks/                 # Custom React hooks
├── lib/                   # Utilities, API client
├── stores/                # Zustand stores
└── types/                 # TypeScript types
```

---

## Backend Stack

### Framework: ASP.NET Core 8.0

**Why**: Enterprise-grade, excellent performance, strong typing, Clean Architecture support

| Feature | Technology | Rationale |
|---------|------------|-----------|
| Architecture | Clean Architecture | Testability, maintainability |
| CQRS | MediatR | Separation of reads/writes |
| Validation | FluentValidation | Declarative, testable |
| ORM | Entity Framework Core 8 | LINQ, migrations, ecosystem |
| API Docs | Swagger/OpenAPI | Auto-generated docs |
| Logging | Serilog | Structured logging |
| Auth | ASP.NET Core Identity + JWT | Built-in, extensible |

### Package Versions

```xml
<PackageReference Include="MediatR" Version="12.x" />
<PackageReference Include="FluentValidation" Version="11.x" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.x" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.x" />
<PackageReference Include="Serilog.AspNetCore" Version="8.x" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.x" />
```

### Backend Architecture

```
src/
├── {Project}.Domain/           # Entities, Value Objects, Interfaces
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   └── Repositories/           # Interfaces only
├── {Project}.Application/      # Use cases, CQRS
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   └── Behaviors/              # Pipeline behaviors
├── {Project}.Infrastructure/   # External concerns
│   ├── Persistence/            # EF Core, DbContext
│   ├── Repositories/           # Implementations
│   └── Services/               # External APIs
└── {Project}.Api/              # HTTP layer
    ├── Controllers/
    ├── Middleware/
    └── Extensions/             # DI registration
```

---

## Database

### PostgreSQL 16

**Why**: ACID compliance, excellent JSON support, mature ecosystem, free

| Feature | Configuration |
|---------|--------------|
| Connection pooling | Npgsql built-in |
| Migrations | EF Core Migrations |
| Full-text search | PostgreSQL native |
| JSON storage | JSONB columns where needed |

### Schema Design Principles

- Use `Guid` for primary keys (distributed-friendly)
- Soft delete via `IsDeleted` flag
- Audit columns: `CreatedAt`, `UpdatedAt`, `CreatedBy`
- Use database constraints (FK, unique, check)

---

## Cache

### Redis 7

**Why**: Fast, versatile (cache + sessions + pub/sub), mature

| Use Case | Pattern |
|----------|---------|
| API response cache | Cache-aside |
| Session storage | Direct storage |
| Rate limiting | Token bucket |
| Real-time (future) | Pub/Sub |

### Cache Strategy

```
Request → Check Cache → Hit? Return cached
                    ↓
                    Miss → Query DB → Store in cache → Return
```

**TTL Guidelines**:
- User sessions: 24 hours
- API responses: 5-15 minutes
- Static data: 1 hour

---

## Authentication & Authorization

### JWT + Refresh Tokens

**Flow**:
```
Login → Access Token (15 min) + Refresh Token (7 days)
     → Access Token expires
     → Use Refresh Token to get new Access Token
     → Refresh Token rotated on use
```

| Token | Storage | Lifetime |
|-------|---------|----------|
| Access Token | Memory (frontend) | 15 minutes |
| Refresh Token | HTTP-only cookie | 7 days |

### Authorization

- Role-based (Admin, User)
- Resource-based where needed (owner checks)
- Claims-based for fine-grained permissions

---

## DevOps & Infrastructure

### Containerization

| Tool | Purpose |
|------|---------|
| Docker | Container runtime |
| Docker Compose | Local development |
| Kubernetes | Production (optional) |

### CI/CD

| Stage | Tool | Actions |
|-------|------|---------|
| Build | GitHub Actions | Compile, lint |
| Test | GitHub Actions | Unit, integration tests |
| Security | GitHub Actions | Dependency scan |
| Deploy | GitHub Actions | Docker build + push |

### Environments

| Environment | Purpose | Infrastructure |
|-------------|---------|---------------|
| Local | Development | Docker Compose |
| Staging | Testing | Cloud (minimal) |
| Production | Live | Cloud (scaled) |

---

## Testing Strategy

### Frontend Testing

| Type | Tool | Target |
|------|------|--------|
| Unit | Vitest | Hooks, utils |
| Component | Testing Library | UI components |
| E2E | Playwright | Critical flows |

### Backend Testing

| Type | Tool | Target |
|------|------|--------|
| Unit | xUnit + Moq | Domain, handlers |
| Integration | TestContainers | API endpoints |
| Architecture | NetArchTest | Layer dependencies |

### Coverage Targets

| Layer | Target |
|-------|--------|
| Domain | 90% |
| Application | 80% |
| Infrastructure | 60% |
| Frontend hooks | 80% |

---

## Observability

### Logging

- **Format**: Structured JSON
- **Library**: Serilog (backend), console (frontend)
- **Levels**: Debug → Info → Warning → Error → Fatal

### Metrics (Future)

- Request rate, error rate, duration
- Database query performance
- Cache hit/miss ratio

### Tracing (Future)

- Distributed tracing with correlation IDs
- OpenTelemetry compatible

---

## Security Considerations

### OWASP Top 10 Coverage

| Vulnerability | Mitigation |
|---------------|------------|
| Injection | Parameterized queries (EF Core) |
| Broken Auth | JWT + secure cookies |
| Sensitive Data | HTTPS, encryption at rest |
| XXE | .NET default protections |
| Access Control | Role-based + resource-based |
| Misconfig | Environment-based config |
| XSS | React auto-escaping |
| Deserialization | Type-safe serialization |
| Components | Dependabot alerts |
| Logging | Structured logging, no secrets |

### Security Headers

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000
Content-Security-Policy: default-src 'self'
```

---

## Performance Targets

| Metric | Target | Tool |
|--------|--------|------|
| API p95 response | <200ms | APM |
| Page load (FCP) | <1.5s | Lighthouse |
| Time to Interactive | <3s | Lighthouse |
| Database query | <50ms | Query analyzer |
| Bundle size | <200KB (initial) | Webpack analyzer |

---

## Decision Log

| Decision | Choice | Alternatives Considered | Rationale |
|----------|--------|------------------------|-----------|
| Frontend framework | Next.js | Remix, Vite+React | SSR, ecosystem, team familiarity |
| Backend framework | .NET 8 | Node.js, Go | Enterprise requirements, typing |
| Database | PostgreSQL | MySQL, SQL Server | JSON support, cost, ecosystem |
| Cache | Redis | Memcached | Versatility, pub/sub future use |
| API style | REST | GraphQL | Simplicity, tooling, caching |

---

## Upgrade Path

### Version Support

| Technology | Current | LTS Until | Upgrade Path |
|------------|---------|-----------|--------------|
| .NET | 8.0 | Nov 2026 | .NET 9 (Nov 2024) |
| Next.js | 14.x | N/A | 15.x when stable |
| PostgreSQL | 16 | Nov 2028 | 17 when released |
| Node.js | 20.x LTS | Apr 2026 | 22 LTS (Oct 2024) |

---

## Local Development Setup

```bash
# Prerequisites
- Docker Desktop
- .NET 8 SDK
- Node.js 20 LTS
- pnpm (recommended) or npm

# Start infrastructure
cd infrastructure
docker compose up -d

# Start backend
cd {project}-api
dotnet run --project src/{Project}.Api

# Start frontend
cd {project}-frontend
pnpm install
pnpm dev

# Access
Frontend: http://localhost:3000
Backend:  http://localhost:5000
Swagger:  http://localhost:5000/swagger
pgAdmin:  http://localhost:5050 (with --profile tools)
```

---

## Appendix

### Useful Commands

```bash
# Backend
dotnet ef migrations add {Name}      # Create migration
dotnet ef database update            # Apply migrations
dotnet test                          # Run tests

# Frontend
pnpm dev                             # Start dev server
pnpm build                           # Production build
pnpm test                            # Run tests
pnpm e2e                             # Run E2E tests

# Docker
docker compose up -d                 # Start services
docker compose logs -f api           # View logs
docker compose down -v               # Stop and clean
```

### Reference Documentation

- [Next.js Docs](https://nextjs.org/docs)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
- [shadcn/ui](https://ui.shadcn.com/)
- [TanStack Query](https://tanstack.com/query)
