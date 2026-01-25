---
description: Run pre-deployment validation checks before releasing to production
---

Run comprehensive pre-deployment validation before releasing to production.

## Pre-Deployment Checklist

Execute the following checks:

### 1. Code Quality Gates

- [ ] **Build Success** - Ensure clean build with no warnings
- [ ] **Test Coverage** - Verify 80%+ coverage on Domain/Application layers
- [ ] **Code Review** - Final code review check completed
- [ ] **Architecture Tests** - Clean Architecture dependency rules pass

### 2. Security Validation

- [ ] **Secret Scanner** - No exposed secrets in codebase
- [ ] **Dependency Audit** - No high/critical vulnerabilities
- [ ] **Security Review** - Use security-reviewer agent for final check
- [ ] **OWASP Top 10** - Common vulnerabilities addressed

### 3. Configuration Validation

- [ ] **Environment Variables** - All required env vars documented
- [ ] **Connection Strings** - No hardcoded connection strings
- [ ] **API Keys** - Use secrets management (Azure Key Vault, etc.)
- [ ] **Feature Flags** - Production flags properly configured

### 4. Infrastructure Readiness

- [ ] **Docker Build** - Container builds and runs successfully
- [ ] **Health Checks** - `/health` and `/ready` endpoints work
- [ ] **Database Migrations** - All migrations applied and tested
- [ ] **Resource Limits** - CPU/memory limits configured

### 5. Traceability Validation

- [ ] **Story Linkage** - All changes linked to story IDs (ACF-###)
- [ ] **Test Coverage** - Tests tagged with story traits
- [ ] **Release Notes** - Changes documented in release notes

## Output Format

For each check:
- ✅ **PASS** - Requirement met
- ❌ **FAIL** - Blocking issue found
- ⚠️ **WARN** - Non-blocking concern

## Final Decision

Provide a **GO/NO-GO** decision for deployment:

### GO Criteria
- All FAIL items resolved
- No critical security vulnerabilities
- Test coverage >= 80%
- All migrations tested

### Report Structure

```
## Deployment Readiness Report
Date: [Date]
Version: [Version]
Decision: GO/NO-GO

### Blocking Issues (Must Fix)
- [List of FAIL items]

### Warnings (Should Fix Soon)
- [List of WARN items]

### Verification Commands Run
- `dotnet build --warningsAsErrors`
- `dotnet test --collect:"XPlat Code Coverage"`
- `./scripts/validate-project.sh`
- `python3 scripts/traceability/traceability.py validate`

### Sign-off
- [ ] Dev Lead Approval
- [ ] QA Sign-off
- [ ] Security Review Complete
```

**If any blocking issues exist, recommend NO-GO and list what must be fixed.**
