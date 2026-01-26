# Infrastructure

Shared Docker infrastructure for fullstack development with the Clean Architecture backend and React frontend templates.

## Quick Start

```bash
# 1. Copy environment file
cp .env.example .env

# 2. Edit .env with your settings (especially JWT_SECRET for production)

# 3. Start infrastructure (database + redis)
docker compose up -d

# 4. Verify services are running
docker compose ps
```

## Usage Modes

### Development Mode (Database Only)

Start just the database and Redis for local development:

```bash
docker compose up -d
```

This starts:
- PostgreSQL on port 5432
- Redis on port 6379

Run your backend and frontend locally:
```bash
# Terminal 1: Backend
cd ../clean-architecture-solution
dotnet run --project src/ProjectName.Api

# Terminal 2: Frontend
cd ../react-frontend-template
npm run dev
```

### Full Stack Mode (Docker)

Run everything in containers:

```bash
docker compose --profile fullstack up -d
```

This starts:
- PostgreSQL (5432)
- Redis (6379)
- .NET API (5000)
- React Frontend (3000)

### With Development Tools

Add database management and email testing tools:

```bash
docker compose --profile tools up -d
```

Additional services:
- pgAdmin: http://localhost:5050 (database UI)
- Redis Commander: http://localhost:8081 (cache UI)
- MailHog: http://localhost:8025 (email testing UI)

### Selective Services

```bash
# Just the API (no frontend)
docker compose --profile api up -d

# Just the frontend (requires running API)
docker compose --profile frontend up -d
```

## Commands Reference

| Command | Description |
|---------|-------------|
| `docker compose up -d` | Start DB + Redis |
| `docker compose --profile api up -d` | Start DB + Redis + API |
| `docker compose --profile frontend up -d` | Start DB + Redis + Frontend |
| `docker compose --profile fullstack up -d` | Start everything |
| `docker compose --profile tools up -d` | Start DB tools |
| `docker compose ps` | Show running services |
| `docker compose logs -f [service]` | Follow logs |
| `docker compose down` | Stop services |
| `docker compose --profile "*" down` | Stop ALL services |
| `docker compose --profile "*" down -v` | Stop and delete volumes |

## Environment Variables

Copy `.env.example` to `.env` and customize:

### Required for Production
- `JWT_SECRET` - Must be 32+ characters, keep secret
- `POSTGRES_PASSWORD` - Database password

### Port Configuration
All ports can be customized if defaults conflict:
- `POSTGRES_PORT` (default: 5432)
- `REDIS_PORT` (default: 6379)
- `API_PORT` (default: 5000)
- `FRONTEND_PORT` (default: 3000)

## Database Management

### Access PostgreSQL directly

```bash
# Using psql
docker compose exec db psql -U postgres -d projectdb

# Using connection string
psql "postgresql://postgres:postgres@localhost:5432/projectdb"
```

### Run migrations

```bash
cd ../clean-architecture-solution
dotnet ef database update --project src/ProjectName.Infrastructure
```

### Backup database

```bash
docker compose exec db pg_dump -U postgres projectdb > backup.sql
```

### Restore database

```bash
cat backup.sql | docker compose exec -T db psql -U postgres projectdb
```

## Troubleshooting

### Port already in use

```bash
# Find what's using the port
lsof -i :5432

# Use different port in .env
POSTGRES_PORT=5433
```

### Database connection refused

```bash
# Check if DB is healthy
docker compose ps

# View DB logs
docker compose logs db

# Wait for health check
docker compose up -d --wait
```

### Reset everything

```bash
# Stop and remove volumes
docker compose --profile "*" down -v

# Rebuild images
docker compose --profile fullstack build --no-cache

# Start fresh
docker compose --profile fullstack up -d
```

## Integration with Templates

This infrastructure is designed to work with:

1. **clean-architecture-solution/** - .NET 8 backend
2. **react-frontend-template/** - Next.js 14 frontend

Both templates have Dockerfiles that this compose file references. The JWT configuration is shared between them via environment variables.
