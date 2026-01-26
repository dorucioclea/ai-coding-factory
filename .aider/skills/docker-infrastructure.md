# Docker Infrastructure Management

## Overview

Guide for managing the shared Docker infrastructure that supports both the .NET backend and React frontend templates. Located at `templates/infrastructure/`.

## When to Use

- Starting development environment (database, redis)
- Running the full stack in containers
- Managing database (backup, restore, migrations)
- Troubleshooting container issues
- Setting up a new project

## Infrastructure Location

```
templates/infrastructure/
├── docker-compose.yml    # Main compose file
├── .env.example          # Environment template
├── .env                  # Your local config (gitignored)
├── init-scripts/         # Database initialization
│   └── 00-init.sql
└── README.md             # Detailed documentation
```

## Quick Commands

### Starting Services

```bash
# Navigate to infrastructure
cd templates/infrastructure

# Start database + Redis only (for local development)
docker compose up -d

# Start with backend API
docker compose --profile api up -d

# Start full stack (DB + Redis + API + Frontend)
docker compose --profile fullstack up -d

# Start with dev tools (pgAdmin, Redis Commander, MailHog)
docker compose --profile tools up -d
```

### Stopping Services

```bash
# Stop running services
docker compose down

# Stop ALL profiles
docker compose --profile "*" down

# Stop and DELETE all data (fresh start)
docker compose --profile "*" down -v
```

### Checking Status

```bash
# Show running containers
docker compose ps

# Follow logs
docker compose logs -f

# Follow specific service
docker compose logs -f api
docker compose logs -f db
```

## Development Workflow

### Option 1: Local Development (Recommended for coding)

Run only infrastructure in Docker, apps locally for hot reload:

```bash
# 1. Start infrastructure
cd templates/infrastructure
docker compose up -d

# 2. Run backend locally (Terminal 1)
cd ../clean-architecture-solution
dotnet run --project src/ProjectName.Api

# 3. Run frontend locally (Terminal 2)
cd ../react-frontend-template
npm run dev
```

**URLs:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Option 2: Full Docker (For testing production-like)

```bash
cd templates/infrastructure
docker compose --profile fullstack up -d --build
```

**URLs:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000

## Database Operations

### Connect to Database

```bash
# Via docker exec
docker compose exec db psql -U postgres -d projectdb

# Connection string for tools
postgresql://postgres:postgres@localhost:5432/projectdb
```

### Run EF Core Migrations

```bash
cd ../clean-architecture-solution

# Create migration
dotnet ef migrations add MigrationName \
  --project src/ProjectName.Infrastructure \
  --startup-project src/ProjectName.Api

# Apply migration
dotnet ef database update \
  --project src/ProjectName.Infrastructure \
  --startup-project src/ProjectName.Api
```

### Backup & Restore

```bash
cd templates/infrastructure

# Backup
docker compose exec db pg_dump -U postgres projectdb > backup_$(date +%Y%m%d).sql

# Restore
cat backup.sql | docker compose exec -T db psql -U postgres projectdb
```

### Reset Database

```bash
# Drop and recreate
docker compose exec db psql -U postgres -c "DROP DATABASE IF EXISTS projectdb;"
docker compose exec db psql -U postgres -c "CREATE DATABASE projectdb;"

# Or full reset with volume
docker compose down -v
docker compose up -d
```

## Redis Operations

### Connect to Redis

```bash
docker compose exec redis redis-cli

# Common commands
> KEYS *          # List all keys
> FLUSHALL        # Clear all data
> INFO            # Server info
```

### Clear Cache

```bash
docker compose exec redis redis-cli FLUSHALL
```

## Environment Configuration

### Setup for New Project

```bash
cd templates/infrastructure

# 1. Copy template
cp .env.example .env

# 2. Edit with your settings
# IMPORTANT: Change JWT_SECRET for production!
```

### Key Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `PROJECT_NAME` | project | Container name prefix |
| `POSTGRES_PASSWORD` | postgres | DB password |
| `JWT_SECRET` | ... | Auth secret (32+ chars) |
| `API_PORT` | 5000 | Backend port |
| `FRONTEND_PORT` | 3000 | Frontend port |

## Troubleshooting

### Port Already in Use

```bash
# Find what's using port 5432
lsof -i :5432

# Option 1: Kill the process
kill -9 <PID>

# Option 2: Change port in .env
POSTGRES_PORT=5433
```

### Container Won't Start

```bash
# Check logs
docker compose logs db

# Check health
docker compose ps

# Rebuild
docker compose build --no-cache api
```

### Database Connection Refused

```bash
# 1. Ensure DB is healthy
docker compose ps
# Should show "healthy" for db service

# 2. Wait for health check
docker compose up -d --wait

# 3. Check connection string in API logs
docker compose logs api | grep -i connection
```

### Permission Denied (Mac/Linux)

```bash
# Fix volume permissions
sudo chown -R $(whoami) ~/.docker
```

### Out of Disk Space

```bash
# Remove unused Docker data
docker system prune -a --volumes

# Check disk usage
docker system df
```

## Service Ports Reference

| Service | Default Port | Purpose |
|---------|--------------|---------|
| PostgreSQL | 5432 | Database |
| Redis | 6379 | Cache |
| API | 5000 | Backend |
| Frontend | 3000 | React app |
| pgAdmin | 5050 | DB management |
| Redis Commander | 8081 | Cache management |
| MailHog SMTP | 1025 | Email testing |
| MailHog UI | 8025 | Email viewer |

## Integration Points

### Backend → Database

Connection string set via environment:
```
ConnectionStrings__DefaultConnection=Host=db;Database=projectdb;Username=postgres;Password=postgres
```

### Backend → Redis

```
ConnectionStrings__Redis=redis:6379
```

### Frontend → Backend

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### JWT Shared Config

Both services use the same JWT settings:
```
JWT_SECRET=your-super-secret-key-at-least-32-characters
JWT_ISSUER=ProjectName
JWT_AUDIENCE=ProjectName
```