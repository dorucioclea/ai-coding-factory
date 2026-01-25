# /add-docker - Add Docker Configuration

Add Docker configuration for containerizing .NET applications.

## Usage
```
/add-docker [options]
```

Options:
- `--compose` - Include docker-compose for local development
- `--db <postgres|sqlserver|mysql>` - Include database service
- `--cache` - Include Redis cache service
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Analyze Project Structure

Determine:
- Solution name and project structure
- API project location (src/*.Api/)
- Dependencies (database, cache, etc.)
- Target .NET version (8.0)

### 2. Generate Dockerfile

Create multi-stage Dockerfile:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/{{ProjectName}}.Api/{{ProjectName}}.Api.csproj", "{{ProjectName}}.Api/"]
COPY ["src/{{ProjectName}}.Application/{{ProjectName}}.Application.csproj", "{{ProjectName}}.Application/"]
COPY ["src/{{ProjectName}}.Domain/{{ProjectName}}.Domain.csproj", "{{ProjectName}}.Domain/"]
COPY ["src/{{ProjectName}}.Infrastructure/{{ProjectName}}.Infrastructure.csproj", "{{ProjectName}}.Infrastructure/"]
RUN dotnet restore "{{ProjectName}}.Api/{{ProjectName}}.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/{{ProjectName}}.Api"
RUN dotnet build "{{ProjectName}}.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "{{ProjectName}}.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

EXPOSE 8080
COPY --from=publish /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "{{ProjectName}}.Api.dll"]

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

### 3. Generate Docker Compose (if --compose)

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION}
      - JwtSettings__Secret=${JWT_SECRET}
    depends_on:
      db:
        condition: service_healthy
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  db:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB={{ProjectName}}
      - POSTGRES_USER=app
      - POSTGRES_PASSWORD=app_password
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U app"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - app-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - app-network

volumes:
  postgres-data:
  redis-data:

networks:
  app-network:
    driver: bridge
```

### 4. Generate .dockerignore

```
**/bin/
**/obj/
**/out/
**/.vs/
**/TestResults/
**/*.user
**/*.suo
**/*.cache
**/*.log
**/.vscode/
.git/
.gitignore
*.md
docker-compose*.yml
Dockerfile*
.env*
```

### 5. Generate Environment File Template

Create `.env.template`:
```bash
# Database
DATABASE_CONNECTION=Host=db;Database={{ProjectName}};Username=app;Password=app_password

# JWT
JWT_SECRET=your-256-bit-secret-key-here

# Application
ASPNETCORE_ENVIRONMENT=Development
```

## Security Checklist

Before generating:
- [ ] Non-root user in container
- [ ] No secrets in Dockerfile
- [ ] Minimal base image (Alpine where possible)
- [ ] Health checks configured
- [ ] Resource limits recommended
- [ ] .dockerignore excludes sensitive files

## Output

```markdown
## Docker Configuration Created

### Files Created
- Dockerfile
- docker-compose.yml (if --compose)
- docker-compose.override.yml (for dev settings)
- .dockerignore
- .env.template

### Services Configured
| Service | Image | Port |
|---------|-------|------|
| api | Custom build | 5000:8080 |
| db | postgres:16-alpine | 5432:5432 |
| redis | redis:7-alpine | 6379:6379 |

### Quick Start
```bash
# Build and run
docker-compose up --build

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop
docker-compose down
```

### Production Notes
1. Use secrets management (not env vars)
2. Set resource limits
3. Use image scanning
4. Enable container security features
```

## Example

```
User: /add-docker --compose --db postgres --cache --story ACF-050

Claude: Adding Docker configuration for the project...

Detected project: OrderService
- API: src/OrderService.Api
- Using .NET 8.0
- PostgreSQL database required
- Redis cache required

Creating files:
1. Dockerfile (multi-stage build)
2. docker-compose.yml (api, postgres, redis)
3. docker-compose.override.yml (dev settings)
4. .dockerignore
5. .env.template

Docker configuration created successfully.

To start: docker-compose up --build
```
