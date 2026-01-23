# /scaffold - Create New Project from Template

Scaffold a new project using enterprise templates.

## Usage
```
/scaffold <project-name> [template]
```

Where:
- `project-name`: The name for the new project (PascalCase recommended)
- `template`: One of:
  - `clean-architecture` (default) - Full .NET Clean Architecture solution
  - `microservice` - Lightweight .NET microservice
  - `react-frontend` - Next.js 14+ React frontend with auth

## Instructions

When invoked, perform the following steps:

### 1. Validate Project Name
- Must be valid identifier (PascalCase, no spaces/special chars)
- Cannot conflict with existing projects in `projects/`

### 2. Detect Template Type

**For .NET templates** (`clean-architecture`, `microservice`):
```bash
mkdir -p projects/<ProjectName>
```

**For React template** (`react-frontend`):
```bash
mkdir -p projects/<ProjectName>-frontend
```

### 3. Copy Template Structure

| Template | Source Path |
|----------|-------------|
| `clean-architecture` | `templates/clean-architecture-solution/` |
| `microservice` | `templates/microservice-template/` |
| `react-frontend` | `templates/react-frontend-template/` |

### 4. Replace Placeholders

Replace `ProjectName` / `projectname` with the actual project name in:
- Directory names
- File names
- File contents

**For .NET**: `.csproj`, `.cs`, `.json`, `.yml`, `.sln` files
**For React**: `package.json`, `.tsx`, `.ts`, `.env.example` files

### 5. Initialize Project

**For .NET templates**:
```bash
dotnet restore
dotnet build
dotnet test
```

**For React template**:
```bash
npm install
npm run lint
npm run type-check
npm run build
```

### 6. Create Initial Story
- Create `artifacts/stories/ACF-001.md` for project setup
- Include project scaffolding as the first tracked story

### 7. Initialize Git (if not in repo)
```bash
git init
git add .
git commit -m "ACF-001 Initial project scaffold from template"
```

## Output Structures

### Clean Architecture (.NET)
```
projects/<ProjectName>/
├── src/
│   ├── <ProjectName>.Api/
│   ├── <ProjectName>.Application/
│   ├── <ProjectName>.Domain/
│   └── <ProjectName>.Infrastructure/
├── tests/
│   ├── <ProjectName>.UnitTests/
│   ├── <ProjectName>.IntegrationTests/
│   └── <ProjectName>.ArchitectureTests/
├── docker/
├── docs/
├── artifacts/stories/
├── <ProjectName>.sln
└── README.md
```

### React Frontend
```
projects/<ProjectName>-frontend/
├── src/
│   ├── app/                 # Next.js pages
│   │   ├── auth/           # Login, Register, Forgot Password
│   │   └── dashboard/      # Protected pages
│   ├── components/
│   │   ├── ui/             # shadcn/ui components
│   │   ├── layout/         # Header, Sidebar
│   │   ├── forms/          # Form components
│   │   └── tables/         # Data tables
│   ├── hooks/              # useAuth, custom hooks
│   ├── lib/                # API client, auth, utils
│   ├── stores/             # Zustand stores
│   ├── types/              # TypeScript types
│   └── i18n/               # Translations
├── e2e/                    # Playwright tests
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Post-Scaffold Checklists

### For .NET Projects
1. Review and customize `appsettings.json`
2. Set up `.env` from `.env.example`
3. Configure database connection string
4. Configure JWT secret
5. Create first feature story with `/new-story`

### For React Projects
1. Copy `.env.example` to `.env.local`
2. Set `NEXT_PUBLIC_API_URL` to backend URL
3. Match `JWT_SECRET` with backend
4. Review `src/config/site.ts`
5. Create first feature story with `/new-story`

## Examples

### Scaffold .NET Backend
```
User: /scaffold OrderManagement

Claude: Creating new project "OrderManagement" using clean-architecture template...

1. Creating directory structure... done
2. Copying template files... done
3. Replacing placeholders... done
4. Restoring dependencies... done
5. Building solution... done
6. Running tests... 15 passed

Project created at: projects/OrderManagement/

Next steps:
1. cd projects/OrderManagement
2. Review appsettings.json and configure database
3. Create your first feature: /new-story <feature-name>
4. Run: dotnet run --project src/OrderManagement.Api
```

### Scaffold React Frontend
```
User: /scaffold OrderManagement react-frontend

Claude: Creating new React frontend "OrderManagement" using react-frontend template...

1. Creating directory structure... done
2. Copying template files... done
3. Replacing placeholders... done
4. Installing dependencies... done
5. Type checking... done
6. Building... done

Project created at: projects/OrderManagement-frontend/

Next steps:
1. cd projects/OrderManagement-frontend
2. Copy .env.example to .env.local
3. Set NEXT_PUBLIC_API_URL=http://localhost:5000/api
4. Run: npm run dev
5. Open http://localhost:3000

To connect with backend:
- Ensure backend is running on port 5000
- Match JWT_SECRET between frontend and backend
- Configure CORS in backend for http://localhost:3000
```

### Scaffold Full Stack
```
User: /scaffold FishSpot
     /scaffold FishSpot react-frontend

Claude: Full stack project created!

Backend: projects/FishSpot/
Frontend: projects/FishSpot-frontend/

Quick start:
# Terminal 1 - Backend
cd projects/FishSpot
dotnet run --project src/FishSpot.Api

# Terminal 2 - Frontend
cd projects/FishSpot-frontend
npm run dev

# Open http://localhost:3000
```
