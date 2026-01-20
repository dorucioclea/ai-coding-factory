# /health - Project Health Dashboard

Display comprehensive project health status including tests, coverage, security, dependencies, and documentation.

## Usage
```
/health [options]
```

Options:
- `--full` - Run all checks (slower but complete)
- `--quick` - Run fast checks only (default)
- `--json` - Output in JSON format
- `--ci` - Exit with non-zero if unhealthy (for CI/CD)

## Instructions

When invoked:

### 1. Gather Project Information

```bash
# Find solution
SOLUTION=$(find . -name "*.sln" -type f | head -1)

# Count projects
PROJECT_COUNT=$(dotnet sln "$SOLUTION" list 2>/dev/null | tail -n +3 | wc -l)

# Get .NET version
DOTNET_VERSION=$(dotnet --version)

# Get last commit
LAST_COMMIT=$(git log -1 --format="%h %s" 2>/dev/null)
```

### 2. Run Health Checks

#### Tests Check
```bash
# Run tests and capture results
dotnet test --no-build --verbosity quiet --logger "console;verbosity=minimal" 2>&1

# Parse results
TESTS_PASSED=$(grep -oE "[0-9]+ Passed" | head -1)
TESTS_FAILED=$(grep -oE "[0-9]+ Failed" | head -1)
```

#### Coverage Check
```bash
# Check if coverage report exists
if [ -f "TestResults/coverage.cobertura.xml" ]; then
  COVERAGE=$(grep -oP 'line-rate="\K[^"]+' TestResults/coverage.cobertura.xml | head -1)
  COVERAGE_PERCENT=$(echo "$COVERAGE * 100" | bc)
fi
```

#### Security Check
```bash
# Check for vulnerable packages
dotnet list package --vulnerable --include-transitive 2>&1 | tee /tmp/vuln-check.txt

# Count vulnerabilities
VULN_COUNT=$(grep -c "has the following vulnerable packages" /tmp/vuln-check.txt || echo 0)
```

#### Dependencies Check
```bash
# Check for outdated packages
dotnet list package --outdated 2>&1 | tee /tmp/outdated-check.txt

# Count outdated
OUTDATED_COUNT=$(grep -c ">" /tmp/outdated-check.txt || echo 0)
```

#### Documentation Check
```bash
# Check required docs exist
DOCS_STATUS="✅"
for doc in README.md CLAUDE.md CORPORATE_RND_POLICY.md; do
  if [ ! -f "$doc" ]; then
    DOCS_STATUS="❌"
    MISSING_DOCS="$MISSING_DOCS $doc"
  fi
done
```

#### Build Check
```bash
# Check if build succeeds
if dotnet build --no-restore --verbosity quiet 2>&1; then
  BUILD_STATUS="✅"
else
  BUILD_STATUS="❌"
fi
```

#### Traceability Check
```bash
# Check story-test linkage
STORIES=$(find artifacts/stories -name "ACF-*.md" 2>/dev/null | wc -l)
TESTS_WITH_STORY=$(grep -r '\[Trait("Story"' tests/ --include="*.cs" 2>/dev/null | wc -l)
```

### 3. Generate Health Report

```markdown
================================================================================
                         PROJECT HEALTH DASHBOARD
================================================================================

Project:     {{PROJECT_NAME}}
Solution:    {{SOLUTION_FILE}}
Framework:   .NET {{DOTNET_VERSION}}
Last Commit: {{LAST_COMMIT}}
Generated:   {{TIMESTAMP}}

================================================================================
                              HEALTH SUMMARY
================================================================================

┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│   Overall Health:  ✅ HEALTHY  |  ⚠️ DEGRADED  |  ❌ UNHEALTHY              │
│                                                                             │
│   Score: {{SCORE}}/100                                                      │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

================================================================================
                              DETAILED CHECKS
================================================================================

BUILD STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{BUILD_STATUS}} {{BUILD_MESSAGE}}
Projects:    {{PROJECT_COUNT}} projects in solution
Warnings:    {{WARNING_COUNT}} compiler warnings
Errors:      {{ERROR_COUNT}} compiler errors

TEST STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{TEST_STATUS}}
Total:       {{TOTAL_TESTS}} tests
Passed:      {{PASSED_TESTS}} ✅
Failed:      {{FAILED_TESTS}} {{FAILED_ICON}}
Skipped:     {{SKIPPED_TESTS}}
Duration:    {{TEST_DURATION}}

COVERAGE STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{COVERAGE_STATUS}}
Overall:     {{COVERAGE_PERCENT}}% (target: 80%)

By Layer:
  Domain:        {{DOMAIN_COVERAGE}}%  {{DOMAIN_ICON}}
  Application:   {{APP_COVERAGE}}%  {{APP_ICON}}
  Infrastructure: {{INFRA_COVERAGE}}%  {{INFRA_ICON}}
  API:           {{API_COVERAGE}}%  {{API_ICON}}

Coverage Bar:
[████████████████████░░░░] {{COVERAGE_PERCENT}}%

SECURITY STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{SECURITY_STATUS}}
Vulnerabilities:
  Critical:  {{CRITICAL_VULNS}}
  High:      {{HIGH_VULNS}}
  Medium:    {{MEDIUM_VULNS}}
  Low:       {{LOW_VULNS}}

{{#if VULNS_FOUND}}
Vulnerable Packages:
{{#each VULNERABLE_PACKAGES}}
  ⚠️ {{package}} ({{severity}}): {{advisory}}
{{/each}}
{{/if}}

DEPENDENCY STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{DEPENDENCY_STATUS}}
Total:       {{TOTAL_PACKAGES}} packages
Outdated:    {{OUTDATED_COUNT}} packages need updates
Deprecated:  {{DEPRECATED_COUNT}} packages deprecated

{{#if OUTDATED_PACKAGES}}
Outdated Packages:
{{#each OUTDATED_PACKAGES}}
  📦 {{package}}: {{current}} → {{latest}}
{{/each}}
{{/if}}

DOCUMENTATION STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{DOCS_STATUS}}

Required Files:
  README.md              {{README_ICON}}
  CLAUDE.md              {{CLAUDE_ICON}}
  CORPORATE_RND_POLICY.md {{POLICY_ICON}}
  ARCHITECTURE.md        {{ARCH_ICON}}

Optional Files:
  CONTRIBUTING.md        {{CONTRIB_ICON}}
  CHANGELOG.md           {{CHANGELOG_ICON}}
  LICENSE                {{LICENSE_ICON}}

TRACEABILITY STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{TRACEABILITY_STATUS}}
Stories:     {{STORY_COUNT}} defined
Tests:       {{TESTS_WITH_STORY}} linked to stories
Coverage:    {{STORY_TEST_COVERAGE}}%

Unlinked Stories:
{{#each UNLINKED_STORIES}}
  ⚠️ {{story_id}}: No tests found
{{/each}}

ARCHITECTURE STATUS
────────────────────────────────────────────────────────────────────────────────
Status:      {{ARCHITECTURE_STATUS}}
Clean Architecture: {{CLEAN_ARCH_STATUS}}

Layer Dependencies:
  Domain → (none)           {{DOMAIN_DEP_ICON}}
  Application → Domain      {{APP_DEP_ICON}}
  Infrastructure → App,Dom  {{INFRA_DEP_ICON}}
  API → All                 {{API_DEP_ICON}}

{{#if ARCH_VIOLATIONS}}
Violations:
{{#each ARCH_VIOLATIONS}}
  ❌ {{violation}}
{{/each}}
{{/if}}

================================================================================
                              RECOMMENDATIONS
================================================================================

{{#each RECOMMENDATIONS}}
{{priority}}. {{recommendation}}
   Action: {{action}}
{{/each}}

================================================================================
                              TREND (Last 5 Builds)
================================================================================

Coverage:  ████████░░ 82% → ████████░░ 84% → █████████░ 86% → █████████░ 87%
Tests:     142 → 148 → 155 → 162 → 168 (+26)
Vulns:     2 → 1 → 0 → 0 → 0 (fixed)

================================================================================
                                  ACTIONS
================================================================================

Quick Fixes:
{{#each QUICK_FIXES}}
  • {{fix}}
{{/each}}

Commands to Run:
  /coverage          - Detailed coverage report
  /security-review   - Full security audit
  /dependencies      - Dependency analysis
  /traceability      - Story-test linkage report
```

### 4. Calculate Health Score

```
Score Calculation (100 points total):

Build:          15 points
  - Compiles:   10 points
  - No warnings: 5 points

Tests:          25 points
  - All pass:   20 points
  - >100 tests:  5 points

Coverage:       20 points
  - ≥80%:       20 points
  - ≥70%:       15 points
  - ≥60%:       10 points
  - <60%:        0 points

Security:       20 points
  - No critical: 10 points
  - No high:     5 points
  - No medium:   5 points

Dependencies:   10 points
  - All current: 10 points
  - <5 outdated:  5 points

Traceability:   10 points
  - 100% linked: 10 points
  - ≥80% linked:  5 points

Total: {{SCORE}}/100
```

### 5. Health Status Thresholds

| Score | Status | Action |
|-------|--------|--------|
| 90-100 | ✅ HEALTHY | Maintain current practices |
| 70-89 | ⚠️ DEGRADED | Address warnings soon |
| <70 | ❌ UNHEALTHY | Immediate attention needed |

### 6. JSON Output (--json)

```json
{
  "project": "{{PROJECT_NAME}}",
  "timestamp": "{{TIMESTAMP}}",
  "score": {{SCORE}},
  "status": "healthy|degraded|unhealthy",
  "checks": {
    "build": {
      "status": "pass|fail",
      "projects": {{PROJECT_COUNT}},
      "warnings": {{WARNING_COUNT}},
      "errors": {{ERROR_COUNT}}
    },
    "tests": {
      "status": "pass|fail",
      "total": {{TOTAL_TESTS}},
      "passed": {{PASSED_TESTS}},
      "failed": {{FAILED_TESTS}},
      "skipped": {{SKIPPED_TESTS}}
    },
    "coverage": {
      "status": "pass|warn|fail",
      "overall": {{COVERAGE_PERCENT}},
      "byLayer": {
        "domain": {{DOMAIN_COVERAGE}},
        "application": {{APP_COVERAGE}},
        "infrastructure": {{INFRA_COVERAGE}},
        "api": {{API_COVERAGE}}
      }
    },
    "security": {
      "status": "pass|warn|fail",
      "vulnerabilities": {
        "critical": {{CRITICAL_VULNS}},
        "high": {{HIGH_VULNS}},
        "medium": {{MEDIUM_VULNS}},
        "low": {{LOW_VULNS}}
      }
    },
    "dependencies": {
      "status": "pass|warn",
      "total": {{TOTAL_PACKAGES}},
      "outdated": {{OUTDATED_COUNT}}
    },
    "traceability": {
      "status": "pass|warn|fail",
      "stories": {{STORY_COUNT}},
      "linkedTests": {{TESTS_WITH_STORY}},
      "coverage": {{STORY_TEST_COVERAGE}}
    }
  },
  "recommendations": [
    {"priority": 1, "message": "{{RECOMMENDATION}}"}
  ]
}
```

## Example

```
User: /health

Claude: Running project health checks...

================================================================================
                         PROJECT HEALTH DASHBOARD
================================================================================

Project:     OrderService
Framework:   .NET 8.0.1
Last Commit: abc123f Add order validation

================================================================================
                              HEALTH SUMMARY
================================================================================

   Overall Health:  ✅ HEALTHY
   Score: 92/100

================================================================================
                              DETAILED CHECKS
================================================================================

BUILD STATUS
  Status:      ✅ Build succeeded
  Projects:    7 projects
  Warnings:    3 compiler warnings

TEST STATUS
  Status:      ✅ All tests passing
  Total:       168 tests
  Passed:      168 ✅
  Failed:      0

COVERAGE STATUS
  Status:      ✅ Above threshold
  Overall:     87% (target: 80%)
  Domain:      92% ✅
  Application: 89% ✅

SECURITY STATUS
  Status:      ✅ No vulnerabilities
  Critical:    0
  High:        0

DEPENDENCY STATUS
  Status:      ⚠️ 2 packages outdated
  📦 MediatR: 12.2.0 → 12.3.0
  📦 Serilog: 3.1.0 → 3.1.1

TRACEABILITY STATUS
  Status:      ✅ 100% coverage
  Stories:     12 defined
  Tests:       45 linked

================================================================================
                              RECOMMENDATIONS
================================================================================

1. Update 2 outdated packages
   Run: dotnet outdated --upgrade

2. Fix 3 compiler warnings
   Run: dotnet build and review warnings

Health check complete. Project is healthy.
```

```
User: /health --ci

Claude: Running health checks for CI...

Score: 92/100
Status: HEALTHY

Exit code: 0 (healthy)
```

```
User: /health --quick

Claude: Quick health check...

✅ Build:      Passing
✅ Tests:      168 passing
✅ Coverage:   87%
✅ Security:   No vulnerabilities
⚠️ Deps:       2 outdated

Score: 92/100 - HEALTHY
```
