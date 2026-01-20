# /switch - Multi-Project Support

Switch between multiple projects in a monorepo or multi-project workspace.

## Usage
```
/switch [project-name] [options]
```

Options:
- `--list` - List all available projects
- `--status` - Show current project context
- `--new <name>` - Create a new project
- `--template <template>` - Template for new project

## Instructions

### Project Discovery

When invoked, scan for projects:

```bash
# Find solution files
find . -name "*.sln" -type f

# Find project directories
find ./projects -maxdepth 1 -type d

# Find docker-compose services
grep -E "^\s+\w+:" docker-compose.yml | grep -v "version\|services\|volumes\|networks"

# Check for monorepo structure
ls -la packages/ services/ apps/ 2>/dev/null
```

### Project Context File

Maintain project context in `.claude/current-project.json`:

```json
{
  "currentProject": {
    "name": "OrderService",
    "path": "services/OrderService",
    "solution": "services/OrderService/OrderService.sln",
    "type": "microservice",
    "framework": "net8.0",
    "switchedAt": "2025-01-20T10:30:00Z"
  },
  "recentProjects": [
    {
      "name": "OrderService",
      "path": "services/OrderService",
      "lastUsed": "2025-01-20T10:30:00Z"
    },
    {
      "name": "UserService",
      "path": "services/UserService",
      "lastUsed": "2025-01-19T15:45:00Z"
    }
  ],
  "projectRegistry": {
    "OrderService": {
      "path": "services/OrderService",
      "type": "microservice",
      "dependencies": ["SharedKernel", "Messaging"]
    },
    "UserService": {
      "path": "services/UserService",
      "type": "microservice",
      "dependencies": ["SharedKernel"]
    },
    "SharedKernel": {
      "path": "shared/SharedKernel",
      "type": "library",
      "dependencies": []
    },
    "Gateway": {
      "path": "services/Gateway",
      "type": "api-gateway",
      "dependencies": ["OrderService", "UserService"]
    }
  }
}
```

### List Projects (`/switch --list`)

```markdown
================================================================================
                         AVAILABLE PROJECTS
================================================================================

## Microservices
| Name | Path | Framework | Status |
|------|------|-----------|--------|
| OrderService | services/OrderService | .NET 8.0 | ✅ Healthy |
| UserService | services/UserService | .NET 8.0 | ✅ Healthy |
| PaymentService | services/PaymentService | .NET 8.0 | ⚠️ Outdated deps |
| NotificationService | services/NotificationService | .NET 8.0 | ✅ Healthy |

## Libraries
| Name | Path | Framework | Used By |
|------|------|-----------|---------|
| SharedKernel | shared/SharedKernel | .NET 8.0 | All services |
| Messaging | shared/Messaging | .NET 8.0 | Order, Notification |

## Infrastructure
| Name | Path | Type |
|------|------|------|
| Gateway | services/Gateway | API Gateway |
| docker-compose | ./ | Container orchestration |

## Current Context
📍 Active: OrderService (services/OrderService)
   Switched: 2 hours ago

## Quick Switch
  /switch UserService
  /switch SharedKernel
  /switch Gateway
```

### Switch Project (`/switch <project>`)

```
User: /switch UserService

Claude: Switching to UserService...

## Context Switch

From: OrderService (services/OrderService)
To:   UserService (services/UserService)

## UserService Overview

**Type**: Microservice
**Framework**: .NET 8.0
**Solution**: services/UserService/UserService.sln

### Project Structure
```
services/UserService/
├── src/
│   ├── UserService.Domain/
│   ├── UserService.Application/
│   ├── UserService.Infrastructure/
│   └── UserService.API/
└── tests/
    ├── UserService.UnitTests/
    └── UserService.IntegrationTests/
```

### Dependencies
- SharedKernel (shared library)

### Recent Activity
- Last commit: 3 hours ago "ACF-089 Add user preferences"
- Open stories: 2 (ACF-091, ACF-092)
- Test status: 142 passing

### Quick Commands (project-scoped)
- Build: `dotnet build services/UserService/UserService.sln`
- Test: `dotnet test services/UserService/`
- Run: `dotnet run --project services/UserService/src/UserService.API`

✅ Switched to UserService

All commands now scoped to: services/UserService/
```

### Project Status (`/switch --status`)

```
User: /switch --status

Claude: 📍 Current Project Context

## Active Project: OrderService

**Path**: services/OrderService
**Solution**: services/OrderService/OrderService.sln
**Switched**: 2025-01-20 10:30 (2 hours ago)

### Project Health
- Build: ✅ Passing
- Tests: ✅ 168/168 passing
- Coverage: 87%
- Dependencies: ⚠️ 2 outdated

### Active Work
- Current sprint: Sprint 12
- Open stories: ACF-088, ACF-090
- In progress: ACF-088 (Order validation)

### Dependencies
```
OrderService
├── SharedKernel (v1.2.0)
├── Messaging (v1.0.0)
└── External
    ├── MediatR (12.2.0)
    └── FluentValidation (11.9.0)
```

### Recent Commands
- /implement ACF-088 (30 min ago)
- /add-test OrderValidator (1 hour ago)
- /coverage (2 hours ago)

### Dependent Services
- Gateway (routes to this service)
- NotificationService (receives events)
```

### Create New Project (`/switch --new`)

```
User: /switch --new InventoryService --template microservice

Claude: Creating new project: InventoryService

## Project Setup

Using template: microservice
Location: services/InventoryService

### Creating Structure
```
services/InventoryService/
├── src/
│   ├── InventoryService.Domain/         ✅ Created
│   ├── InventoryService.Application/    ✅ Created
│   ├── InventoryService.Infrastructure/ ✅ Created
│   └── InventoryService.API/            ✅ Created
├── tests/
│   ├── InventoryService.UnitTests/      ✅ Created
│   └── InventoryService.IntegrationTests/ ✅ Created
├── InventoryService.sln                  ✅ Created
├── Dockerfile                            ✅ Created
└── README.md                             ✅ Created
```

### Dependencies Added
- SharedKernel (project reference)
- Standard NuGet packages

### Registration
- Added to docker-compose.yml
- Added to project registry
- Added to Gateway routes (if applicable)

### Configuration
- appsettings.json created
- appsettings.Development.json created
- Connection strings templated

✅ InventoryService created and registered

Switched to: InventoryService

Next steps:
1. Define domain entities: /add-entity
2. Create first story: /new-story
3. Run initial build: dotnet build
```

### Cross-Project Commands

Some commands can target specific projects:

```bash
# Run tests for specific project
/coverage --project UserService

# Implement story in specific project
/implement ACF-091 --project UserService

# Health check specific project
/health --project OrderService

# View dependencies across projects
/dependencies --all
```

### Project-Aware Paths

When a project is selected, paths are relative to that project:

```
# Without project context (from repo root)
src/OrderService.Domain/Entities/Order.cs

# With OrderService context
src/Domain/Entities/Order.cs   → resolves to → services/OrderService/src/OrderService.Domain/Entities/Order.cs
```

### Monorepo Detection

Automatically detect monorepo patterns:

```bash
# Check for common monorepo structures
if [ -d "packages" ] || [ -d "services" ] || [ -d "apps" ]; then
  REPO_TYPE="monorepo"
fi

# Check for workspace files
if [ -f "pnpm-workspace.yaml" ] || [ -f "lerna.json" ] || [ -f "rush.json" ]; then
  REPO_TYPE="monorepo"
fi

# Check for multiple solutions
SOLUTION_COUNT=$(find . -name "*.sln" -type f | wc -l)
if [ $SOLUTION_COUNT -gt 1 ]; then
  REPO_TYPE="multi-solution"
fi
```

### Workspace Configuration

Optional `.claude/workspace.json` for custom project configuration:

```json
{
  "workspace": {
    "name": "E-Commerce Platform",
    "type": "monorepo"
  },
  "projects": {
    "OrderService": {
      "path": "services/OrderService",
      "type": "microservice",
      "port": 5001,
      "database": "orders_db",
      "team": "orders-team"
    },
    "UserService": {
      "path": "services/UserService",
      "type": "microservice",
      "port": 5002,
      "database": "users_db",
      "team": "identity-team"
    }
  },
  "shared": {
    "SharedKernel": {
      "path": "shared/SharedKernel",
      "type": "library"
    }
  },
  "infrastructure": {
    "Gateway": {
      "path": "services/Gateway",
      "type": "api-gateway",
      "port": 5000
    }
  },
  "defaultProject": "OrderService"
}
```

## Example Session

```
User: /switch --list

Claude: Available projects:

  MICROSERVICES
  1. OrderService (current) - services/OrderService
  2. UserService - services/UserService
  3. PaymentService - services/PaymentService

  LIBRARIES
  4. SharedKernel - shared/SharedKernel

  INFRASTRUCTURE
  5. Gateway - services/Gateway

User: /switch UserService

Claude: Switching to UserService...

✅ Switched to UserService

  Path: services/UserService
  Type: Microservice
  Stories: 2 open (ACF-091, ACF-092)

  All commands now scoped to UserService.

User: /add-entity UserPreference

Claude: Creating entity in UserService...

  File: services/UserService/src/UserService.Domain/Entities/UserPreference.cs

  [Creates entity in UserService context]
```

```
User: /switch --status

Claude: 📍 Current: UserService (services/UserService)

  Health: ✅ Healthy
  Tests: 142 passing
  Open stories: ACF-091, ACF-092

  Related projects:
  - SharedKernel (dependency)
  - Gateway (consumer)
```
