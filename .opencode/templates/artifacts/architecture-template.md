# Architecture Document: {PROJECT_NAME}

**Version**: 1.0
**Date**: {DATE}
**Author**: {Author}
**Status**: Draft | Review | Approved

---

## Overview

### Purpose

{1-2 sentences: What does this system do?}

### Scope

**In Scope**:
- {Capability 1}
- {Capability 2}

**Out of Scope**:
- {Excluded capability}

### Key Architectural Decisions

| Decision | Choice | Rationale | ADR Link |
|----------|--------|-----------|----------|
| Architecture Style | Clean Architecture | Testability, maintainability | ADR-001 |
| API Style | REST | Simplicity, tooling | ADR-002 |
| Database | PostgreSQL | ACID, JSON support | ADR-003 |
| Frontend Framework | Next.js | SSR, React ecosystem | ADR-004 |

---

## System Context

### Context Diagram

```
                    ┌─────────────────┐
                    │     Users       │
                    │  (Web Browser)  │
                    └────────┬────────┘
                             │ HTTPS
                             ▼
┌────────────────────────────────────────────────────────┐
│                    {PROJECT_NAME}                       │
│  ┌──────────────┐    ┌──────────────┐                  │
│  │   Frontend   │───▶│   Backend    │                  │
│  │  (Next.js)   │    │  (.NET API)  │                  │
│  └──────────────┘    └──────┬───────┘                  │
│                             │                          │
│         ┌───────────────────┼───────────────────┐      │
│         ▼                   ▼                   ▼      │
│  ┌────────────┐     ┌────────────┐     ┌────────────┐ │
│  │ PostgreSQL │     │   Redis    │     │   Files    │ │
│  │  (Primary) │     │  (Cache)   │     │ (Storage)  │ │
│  └────────────┘     └────────────┘     └────────────┘ │
└────────────────────────────────────────────────────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
     ┌────────────────┐           ┌────────────────┐
     │ Email Service  │           │ External API   │
     │   (SendGrid)   │           │   (Optional)   │
     └────────────────┘           └────────────────┘
```

### External Systems

| System | Purpose | Protocol | Owner |
|--------|---------|----------|-------|
| {System 1} | {What it does for us} | REST/HTTPS | {Who maintains} |
| {System 2} | {What it does for us} | SMTP | {Who maintains} |

---

## Container Architecture

### Backend (.NET API)

```
┌─────────────────────────────────────────────────────────┐
│                     API Layer                            │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Controllers / Endpoints              │    │
│  │  • AuthController    • {Entity}Controller        │    │
│  │  • HealthController  • ...                       │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │              Application Layer                   │    │
│  │  ┌─────────────┐  ┌─────────────┐               │    │
│  │  │  Commands   │  │   Queries   │  (MediatR)    │    │
│  │  │  Handlers   │  │  Handlers   │               │    │
│  │  └─────────────┘  └─────────────┘               │    │
│  │  ┌─────────────────────────────────┐            │    │
│  │  │  Validators (FluentValidation)  │            │    │
│  │  └─────────────────────────────────┘            │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │                Domain Layer                      │    │
│  │  ┌───────────┐ ┌───────────┐ ┌───────────┐      │    │
│  │  │ Entities  │ │  Value    │ │  Domain   │      │    │
│  │  │           │ │  Objects  │ │  Events   │      │    │
│  │  └───────────┘ └───────────┘ └───────────┘      │    │
│  │  ┌───────────────────────────────────────┐      │    │
│  │  │     Repository Interfaces             │      │    │
│  │  └───────────────────────────────────────┘      │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │             Infrastructure Layer                 │    │
│  │  ┌───────────────────────────────────────┐      │    │
│  │  │    Repository Implementations          │      │    │
│  │  │    (EF Core, Redis, External APIs)    │      │    │
│  │  └───────────────────────────────────────┘      │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### Frontend (Next.js)

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │                   Pages (App Router)             │    │
│  │  • /              • /auth/login                  │    │
│  │  • /dashboard     • /dashboard/{entity}          │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │               Components                         │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐        │    │
│  │  │    UI    │ │ Features │ │  Layout  │        │    │
│  │  │(shadcn)  │ │          │ │          │        │    │
│  │  └──────────┘ └──────────┘ └──────────┘        │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │                 Hooks & State                    │    │
│  │  ┌───────────────┐  ┌───────────────┐           │    │
│  │  │ TanStack Query│  │    Zustand    │           │    │
│  │  │  (Server)     │  │   (Client)    │           │    │
│  │  └───────────────┘  └───────────────┘           │    │
│  └─────────────────────┬───────────────────────────┘    │
│                        │                                 │
│  ┌─────────────────────▼───────────────────────────┐    │
│  │                  API Client                      │    │
│  │            (Axios + Interceptors)               │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Data Architecture

### Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐
│      User       │       │    {Entity}     │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ Email           │──1:N──│ CreatedById(FK) │
│ PasswordHash    │       │ Name            │
│ CreatedAt       │       │ ...             │
│ IsActive        │       │ CreatedAt       │
└─────────────────┘       └─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐
│  RefreshToken   │
├─────────────────┤
│ Id (PK)         │
│ UserId (FK)     │
│ Token           │
│ ExpiresAt       │
└─────────────────┘
```

### Data Flow

```
User Action → Frontend → API → Command Handler → Domain → Repository → Database
                                    │
                                    ├── Domain Events → Event Handlers
                                    │
                                    └── Validation → FluentValidation
```

---

## API Design

### Authentication

```
POST /api/auth/register    → Create account
POST /api/auth/login       → Get tokens
POST /api/auth/refresh     → Refresh access token
POST /api/auth/logout      → Invalidate tokens
GET  /api/auth/me          → Current user info
```

### Resource Endpoints

```
GET    /api/{entities}           → List (paginated)
GET    /api/{entities}/{id}      → Get by ID
POST   /api/{entities}           → Create
PUT    /api/{entities}/{id}      → Update
DELETE /api/{entities}/{id}      → Delete
```

### Request/Response Format

```json
// Success Response
{
  "data": { ... },
  "meta": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  }
}

// Error Response
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      { "field": "name", "message": "Name is required" }
    ]
  }
}
```

---

## Security Architecture

### Authentication Flow

```
┌────────┐     ┌──────────┐     ┌─────────┐     ┌──────────┐
│ Client │     │ Frontend │     │   API   │     │ Database │
└───┬────┘     └────┬─────┘     └────┬────┘     └────┬─────┘
    │               │                │               │
    │──Login Form──▶│                │               │
    │               │──POST /login──▶│               │
    │               │                │──Verify──────▶│
    │               │                │◀──User Data───│
    │               │                │               │
    │               │◀──JWT Tokens───│               │
    │               │                │               │
    │◀──Set Cookie──│                │               │
    │               │                │               │
    │──API Request─▶│                │               │
    │  (w/ Cookie)  │──Bearer Token─▶│               │
    │               │                │──Validate JWT │
    │               │◀──Response─────│               │
    │◀──Data────────│                │               │
```

### Authorization Model

| Role | Permissions |
|------|-------------|
| Anonymous | View public content |
| User | Create own content, edit own content |
| Admin | All user permissions + manage users |

### Security Controls

- [x] HTTPS everywhere
- [x] JWT with short expiry (15 min)
- [x] Refresh token rotation
- [x] Password hashing (bcrypt/Argon2)
- [x] Rate limiting
- [x] Input validation
- [x] SQL injection prevention (EF Core)
- [x] XSS prevention (React escaping)
- [x] CORS configuration

---

## Deployment Architecture

### Development

```
┌─────────────────────────────────────────────┐
│              Developer Machine               │
│                                             │
│  ┌─────────────┐    ┌─────────────────────┐ │
│  │   Docker    │    │     localhost       │ │
│  │  Compose    │    │                     │ │
│  │             │    │  Frontend :3000     │ │
│  │  DB :5432   │    │  Backend  :5000     │ │
│  │  Redis:6379 │    │                     │ │
│  └─────────────┘    └─────────────────────┘ │
└─────────────────────────────────────────────┘
```

### Production (Kubernetes)

```
                    ┌─────────────────┐
                    │   Ingress       │
                    │   (nginx)       │
                    └────────┬────────┘
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
          ▼                  ▼                  ▼
   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
   │  Frontend   │   │   Backend   │   │   Backend   │
   │   Pod (2)   │   │   Pod (3)   │   │   Pod (3)   │
   └─────────────┘   └──────┬──────┘   └──────┬──────┘
                            │                  │
          ┌─────────────────┴──────────────────┘
          │
          ▼
   ┌─────────────────────────────────────┐
   │        Managed Services             │
   │                                     │
   │  ┌───────────┐   ┌───────────┐      │
   │  │  RDS      │   │  Redis    │      │
   │  │(Postgres) │   │ (ElastiC) │      │
   │  └───────────┘   └───────────┘      │
   └─────────────────────────────────────┘
```

---

## Quality Attributes

### Performance

| Metric | Target | Measurement |
|--------|--------|-------------|
| API Response (p95) | <200ms | APM monitoring |
| Page Load (FCP) | <1.5s | Lighthouse |
| Database Query | <50ms | Query analyzer |

### Scalability

| Component | Scaling Strategy |
|-----------|-----------------|
| API | Horizontal (k8s HPA) |
| Database | Vertical + Read replicas |
| Cache | Redis Cluster |
| Frontend | CDN + Edge caching |

### Reliability

| Metric | Target |
|--------|--------|
| Availability | 99.5% |
| RTO | 4 hours |
| RPO | 1 hour |

---

## Monitoring & Observability

### Logging

- **Structured logging** with Serilog
- **Log levels**: Debug → Info → Warning → Error → Fatal
- **Correlation IDs** for request tracing

### Metrics

- Request rate, error rate, duration (RED)
- Database connection pool
- Cache hit/miss ratio
- Custom business metrics

### Alerting

| Alert | Condition | Action |
|-------|-----------|--------|
| High Error Rate | >5% 5xx in 5min | Page on-call |
| Slow Response | p95 >500ms | Slack notification |
| DB Connection | Pool exhausted | Auto-scale + page |

---

## Technology Stack Summary

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | Next.js | 14.x |
| UI Components | shadcn/ui | Latest |
| State Management | TanStack Query + Zustand | 5.x / 4.x |
| Backend | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7 |
| Message Queue | (Future) RabbitMQ | - |
| Container | Docker | Latest |
| Orchestration | Kubernetes | 1.28+ |

---

## Architecture Decision Records

| ADR | Title | Status |
|-----|-------|--------|
| ADR-001 | Use Clean Architecture | Accepted |
| ADR-002 | REST over GraphQL | Accepted |
| ADR-003 | PostgreSQL as primary database | Accepted |
| ADR-004 | Next.js for frontend | Accepted |
| ADR-005 | JWT authentication | Accepted |

---

## Appendix

### Glossary

| Term | Definition |
|------|------------|
| {Term 1} | {Definition} |
| {Term 2} | {Definition} |

### References

- Clean Architecture by Robert C. Martin
- .NET Documentation
- Next.js Documentation
