# /add-cicd - Add CI/CD Pipeline

Add GitHub Actions CI/CD workflows for .NET applications.

## Usage
```
/add-cicd [options]
```

Options:
- `--provider <github|azure>` - CI/CD provider (default: github)
- `--deploy <azure|aws|k8s>` - Deployment target
- `--environments <list>` - Environments (e.g., staging,production)
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Analyze Project Requirements

Determine:
- Project structure and solution file location
- Test projects (Unit, Integration, Architecture)
- Docker configuration presence
- Target deployment platform

### 2. Generate Build and Test Workflow

Create `.github/workflows/build.yml`:

```yaml
name: Build and Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run unit tests
        run: |
          dotnet test tests/*.UnitTests \
            --configuration Release \
            --no-build \
            --verbosity normal \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults

      - name: Run integration tests
        run: |
          dotnet test tests/*.IntegrationTests \
            --configuration Release \
            --no-build \
            --verbosity normal

      - name: Run architecture tests
        run: |
          dotnet test tests/*.ArchitectureTests \
            --configuration Release \
            --no-build \
            --verbosity normal

      - name: Generate coverage report
        uses: danielpalme/ReportGenerator-GitHub-Action@5.2.4
        with:
          reports: '**/coverage.cobertura.xml'
          targetdir: coveragereport
          reporttypes: 'HtmlInline;Cobertura'

      - name: Upload coverage artifact
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: coveragereport/

      - name: Check coverage threshold
        run: |
          # Extract coverage percentage
          coverage=$(grep -oP 'line-rate="\K[^"]+' coveragereport/Cobertura.xml | head -1)
          coverage_percent=$(echo "$coverage * 100" | bc)
          echo "Coverage: $coverage_percent%"

          if (( $(echo "$coverage_percent < 80" | bc -l) )); then
            echo "::error::Coverage $coverage_percent% is below 80% threshold"
            exit 1
          fi

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: TestResults/
```

### 3. Generate Security Scan Workflow

Create `.github/workflows/security.yml`:

```yaml
name: Security Scan

on:
  push:
    branches: [main]
  schedule:
    - cron: '0 0 * * 0' # Weekly on Sunday

jobs:
  security:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Check for vulnerable packages
        run: |
          dotnet list package --vulnerable --include-transitive 2>&1 | tee vulnerability-report.txt
          if grep -q "has the following vulnerable packages" vulnerability-report.txt; then
            echo "::warning::Vulnerable packages detected"
          fi

      - name: Run OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: '{{ProjectName}}'
          path: '.'
          format: 'HTML'
          out: 'reports'

      - name: Upload security report
        uses: actions/upload-artifact@v4
        with:
          name: security-report
          path: reports/

      - name: CodeQL Analysis
        uses: github/codeql-action/init@v3
        with:
          languages: csharp

      - name: Build for CodeQL
        run: dotnet build

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
```

### 4. Generate Docker Build Workflow

Create `.github/workflows/docker.yml`:

```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [main]
    tags:
      - 'v*'

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=sha

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

### 5. Generate Deployment Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy

on:
  workflow_run:
    workflows: ["Build and Push Docker Image"]
    types: [completed]
    branches: [main]

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    environment:
      name: staging
      url: https://staging.example.com

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to staging
        run: |
          echo "Deploying to staging..."
          # Add deployment commands here

      - name: Health check
        run: |
          for i in {1..10}; do
            if curl -sf https://staging.example.com/health; then
              echo "Health check passed"
              exit 0
            fi
            echo "Retry $i/10..."
            sleep 10
          done
          echo "Health check failed"
          exit 1

      - name: Notify on failure
        if: failure()
        uses: 8398a7/action-slack@v3
        with:
          status: ${{ job.status }}
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}

  deploy-production:
    runs-on: ubuntu-latest
    needs: deploy-staging
    if: startsWith(github.ref, 'refs/tags/v')
    environment:
      name: production
      url: https://example.com

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to production
        run: |
          echo "Deploying to production..."
          # Add deployment commands here

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          generate_release_notes: true
```

## Required Secrets

Document required GitHub secrets:
```markdown
## GitHub Secrets Required

| Secret | Description | Required For |
|--------|-------------|--------------|
| `GITHUB_TOKEN` | Auto-provided | Package registry |
| `AZURE_CREDENTIALS` | Azure service principal | Azure deployment |
| `AWS_ACCESS_KEY_ID` | AWS credentials | AWS deployment |
| `AWS_SECRET_ACCESS_KEY` | AWS credentials | AWS deployment |
| `SLACK_WEBHOOK` | Slack webhook URL | Notifications |
| `SONAR_TOKEN` | SonarQube token | Code quality |
```

## Output

```markdown
## CI/CD Pipelines Created

### Workflows Created
| Workflow | Trigger | Purpose |
|----------|---------|---------|
| build.yml | Push/PR | Build, test, coverage |
| security.yml | Push/Weekly | Security scanning |
| docker.yml | Push to main/tags | Docker image build |
| deploy.yml | After docker build | Deploy to environments |

### Branch Protection Recommended
- Require status checks: build, security
- Require PR reviews before merge
- Require up-to-date branches

### Next Steps
1. Add required secrets to repository settings
2. Configure branch protection rules
3. Set up environments (staging, production)
4. Configure deployment targets
```

## Example

```
User: /add-cicd --provider github --deploy azure --environments staging,production --story ACF-055

Claude: Creating GitHub Actions CI/CD workflows...

Detected project structure:
- Solution: OrderService.sln
- Test projects: UnitTests, IntegrationTests, ArchitectureTests
- Docker: Dockerfile present
- Target: Azure App Service

Creating workflows:
1. .github/workflows/build.yml (build, test, coverage)
2. .github/workflows/security.yml (vulnerability scanning)
3. .github/workflows/docker.yml (container build)
4. .github/workflows/deploy.yml (staging → production)

Required secrets:
- AZURE_WEBAPP_PUBLISH_PROFILE_STAGING
- AZURE_WEBAPP_PUBLISH_PROFILE_PRODUCTION
- SLACK_WEBHOOK (optional)

CI/CD pipelines created successfully.

Next: Add secrets in GitHub repository settings → Settings → Secrets
```
