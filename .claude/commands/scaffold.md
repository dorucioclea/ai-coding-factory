# /scaffold - Create New Project from Template

Scaffold a new .NET project using the Clean Architecture template.

## Usage
```
/scaffold <project-name> [template]
```

Where:
- `project-name`: The name for the new project (PascalCase recommended)
- `template`: Optional, one of:
  - `clean-architecture` (default) - Full Clean Architecture solution
  - `microservice` - Microservice template

## Instructions

When invoked, perform the following steps:

1. **Validate project name**:
   - Must be valid C# identifier (PascalCase, no spaces/special chars)
   - Cannot conflict with existing projects in `projects/`

2. **Create project directory**:
   ```bash
   mkdir -p projects/<ProjectName>
   cd projects/<ProjectName>
   ```

3. **Copy template structure**:
   - For `clean-architecture`: Copy from `templates/clean-architecture-solution/`
   - For `microservice`: Copy from `templates/microservice-template/`

4. **Replace placeholders**:
   - Replace `{ProjectName}` with the actual project name in:
     - Directory names
     - File names
     - File contents (.csproj, .cs, .json, .yml files)
     - Solution file (.sln)

5. **Initialize project**:
   ```bash
   # Restore dependencies
   dotnet restore

   # Verify build
   dotnet build

   # Run tests
   dotnet test
   ```

6. **Create initial story**:
   - Create `artifacts/stories/ACF-001.md` for project setup
   - Include project scaffolding as the first tracked story

7. **Initialize git** (if not already in repo):
   ```bash
   git init
   git add .
   git commit -m "ACF-001 Initial project scaffold from template"
   ```

## Output Structure

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
│   ├── Dockerfile
│   └── docker-compose.yml
├── docs/
│   ├── architecture/
│   ├── modules/
│   ├── api/
│   └── operations/
├── artifacts/
│   └── stories/
│       └── ACF-001.md
├── <ProjectName>.sln
├── .gitignore
├── .editorconfig
├── Directory.Build.props
├── azure-pipelines.yml
└── README.md
```

## Post-Scaffold Checklist

After scaffolding, remind the user to:

1. Review and customize `appsettings.json`
2. Set up `.env` from `.env.example`
3. Configure database connection string
4. Review generated documentation structure
5. Create first feature story with `/new-story`

## Example

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
