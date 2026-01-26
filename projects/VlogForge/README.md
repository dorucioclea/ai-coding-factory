# VlogForge

A comprehensive platform for active vloggers to manage their content, track analytics, collaborate with teams, and connect with other creators.

## Quick Start

```bash
# Start infrastructure (PostgreSQL + Redis)
cd infrastructure
docker compose up -d

# Start backend API
cd ../backend
dotnet restore
dotnet run --project src/Api

# Start frontend (in another terminal)
cd ../frontend
npm install
npm run dev
```

## Project Structure

```
VlogForge/
├── backend/                    # .NET 8 Clean Architecture API
│   ├── src/
│   │   ├── Api/               # REST API, controllers, middleware
│   │   ├── Application/       # Use cases, CQRS handlers, DTOs
│   │   ├── Domain/            # Entities, value objects, domain events
│   │   └── Infrastructure/    # EF Core, external services, repositories
│   └── tests/
│       ├── VlogForge.UnitTests/
│       ├── VlogForge.IntegrationTests/
│       └── VlogForge.ArchitectureTests/
├── frontend/                   # Next.js 14 + React + shadcn/ui
│   ├── src/
│   │   ├── app/               # App Router pages
│   │   ├── components/        # React components
│   │   ├── lib/               # Utilities, API client
│   │   └── hooks/             # Custom React hooks
│   └── e2e/                   # Playwright E2E tests
├── infrastructure/             # Docker Compose setup
│   ├── docker-compose.yml     # PostgreSQL, Redis, optional services
│   └── init-scripts/          # Database initialization
└── artifacts/
    └── stories/               # User stories (ACF-001 through ACF-014)
```

## Key Features

### MVP (Phase 1-6)

| Feature | Stories | Status |
|---------|---------|--------|
| **Authentication** | ACF-001 | Ready |
| **Creator Profiles** | ACF-002 | Ready |
| **Platform Integrations** | ACF-003 | Ready |
| **Analytics Dashboard** | ACF-004 | Ready |
| **Content Planning** | ACF-005, ACF-006 | Ready |
| **Team Management** | ACF-007, ACF-008, ACF-009, ACF-014 | Ready |
| **Creator Discovery** | ACF-010 | Ready |
| **Collaboration** | ACF-011, ACF-012, ACF-013 | Ready |

### Platforms Supported

- YouTube (Data API v3)
- Instagram (Graph API)
- TikTok (Research API)

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 8, ASP.NET Core, EF Core 8 |
| Frontend | Next.js 14, React 18, TypeScript, Tailwind CSS |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Real-time | SignalR |
| Auth | JWT + Refresh Tokens |
| Testing | xUnit, Playwright, Vitest |

## Development

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose

### Running Tests

```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm test

# E2E tests
npm run test:e2e
```

### Docker Profiles

```bash
# Infrastructure only
docker compose up -d

# Full stack
docker compose --profile api --profile frontend up -d

# With dev tools (pgAdmin, Redis Commander)
docker compose --profile tools up -d
```

## Architecture

### Clean Architecture Layers

```
Domain ← Application ← Infrastructure ← API
  ↓           ↓              ↓            ↓
No deps    Domain only   Domain+App    All
```

### Key Principles

- **Domain-Driven Design**: Rich domain models with business logic
- **CQRS Pattern**: Separate read/write operations via MediatR
- **Repository Pattern**: Abstract data access
- **Dependency Injection**: Loose coupling throughout

## Contributing

1. Pick a story from `artifacts/stories/`
2. Create a feature branch: `git checkout -b feature/ACF-XXX-description`
3. Implement with TDD (tests first!)
4. Ensure 80%+ code coverage
5. Create PR with story reference

## License

Proprietary - Internal Use Only
