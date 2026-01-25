# /release - Prepare Release

Prepare a release with all required artifacts and validations.

## Usage
```
/release <version>
```

Example: `/release 1.2.0`

## Instructions

### Pre-Release Validation

1. **Run all validation scripts**:
   ```bash
   ./scripts/validate-project.sh
   ./scripts/validate-documentation.sh
   ./scripts/validate-rnd-policy.sh
   ```

2. **Verify test coverage**:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   python3 scripts/coverage/check-coverage.py coverage.xml
   ```

3. **Run security scan**:
   ```bash
   dotnet list package --vulnerable
   ```

4. **Generate traceability report**:
   ```bash
   python3 scripts/traceability/traceability.py report \
     --story-root artifacts/stories \
     --test-root tests \
     --output artifacts/traceability/traceability-report.md
   ```

5. **Verify all stories complete**:
   - All stories in this release have linked tests
   - All tests pass
   - Coverage thresholds met

### Release Artifacts

Create/update the following:

#### 1. Release Notes (`artifacts/traceability/release-notes-<version>.md`)

```markdown
# Release Notes - v<version>

**Release Date**: <YYYY-MM-DD>
**Type**: Major | Minor | Patch

## Summary
<Brief summary of this release>

## New Features
- **ACF-###**: <Feature description>
- **ACF-###**: <Feature description>

## Improvements
- **ACF-###**: <Improvement description>

## Bug Fixes
- **ACF-###**: <Bug fix description>

## Breaking Changes
<List any breaking changes and migration steps>

## Security Updates
- <Security fix or dependency update>

## Dependencies Updated
| Package | From | To |
|---------|------|-----|
| Example.Package | 1.0.0 | 1.1.0 |

## Known Issues
- <Known issue and workaround if applicable>

## Upgrade Guide
<Steps to upgrade from previous version>

## Contributors
- Claude Code (AI-assisted development)
- <Human reviewers/approvers>
```

#### 2. Security Review Checklist (`artifacts/traceability/security-review-<version>.md`)

```markdown
# Security Review Checklist - v<version>

**Reviewer**: <name>
**Date**: <YYYY-MM-DD>

## Code Security
- [ ] No hardcoded secrets
- [ ] All inputs validated
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS prevention (output encoding)
- [ ] CSRF protection enabled
- [ ] Authentication on protected endpoints
- [ ] Authorization checks implemented
- [ ] Error messages don't leak sensitive info

## Dependencies
- [ ] No known vulnerabilities in dependencies
- [ ] All dependencies from approved sources
- [ ] Dependency versions pinned

## Configuration
- [ ] Secrets in environment variables
- [ ] Debug mode disabled in production
- [ ] HTTPS enforced
- [ ] Security headers configured

## Data Protection
- [ ] PII properly encrypted
- [ ] Data retention policies followed
- [ ] Audit logging implemented

## Approval
| Role | Name | Date | Approved |
|------|------|------|----------|
| Security Lead | | | [ ] |
| Tech Lead | | | [ ] |
```

#### 3. Release Readiness Checklist (`artifacts/traceability/release-readiness-<version>.md`)

```markdown
# Release Readiness Checklist - v<version>

**Target Date**: <YYYY-MM-DD>

## Quality Gates
- [ ] All tests pass
- [ ] Coverage >= 80%
- [ ] No critical/high security vulnerabilities
- [ ] No P1/P2 bugs open
- [ ] Performance benchmarks met

## Documentation
- [ ] API documentation updated
- [ ] User guide updated
- [ ] Deployment guide updated
- [ ] Release notes complete

## Artifacts
- [ ] Traceability report generated
- [ ] Security review completed
- [ ] Release notes approved

## Deployment
- [ ] Docker images built and tested
- [ ] Kubernetes manifests updated
- [ ] Environment variables documented
- [ ] Rollback plan documented

## Approval
| Role | Name | Date | Approved |
|------|------|------|----------|
| Product Owner | | | [ ] |
| QA Lead | | | [ ] |
| Security Lead | | | [ ] |
| DevOps Lead | | | [ ] |
```

### Version Tagging

After all approvals:

```bash
# Update version in project files
# Directory.Build.props or .csproj files

# Commit version bump
git add .
git commit -m "chore: bump version to <version>"

# Create annotated tag
git tag -a v<version> -m "Release v<version>

<Brief summary>

Co-Authored-By: Claude <noreply@anthropic.com>"

# Push tag
git push origin v<version>
```

## Output Format

```markdown
## Release Preparation: v<version>

### Pre-Release Checks

| Check | Status | Details |
|-------|--------|---------|
| Project Validation | PASS | No issues |
| Documentation | PASS | All docs present |
| Policy Compliance | PASS | Meets R&D policy |
| Test Coverage | PASS | 87% coverage |
| Security Scan | PASS | 0 vulnerabilities |
| Traceability | PASS | 12/12 stories linked |

### Artifacts Generated
- [x] Release notes: artifacts/traceability/release-notes-<version>.md
- [x] Security review: artifacts/traceability/security-review-<version>.md
- [x] Release readiness: artifacts/traceability/release-readiness-<version>.md
- [x] Traceability report: artifacts/traceability/traceability-report.md

### Stories in This Release
| Story | Title | Tests | Status |
|-------|-------|-------|--------|
| ACF-040 | User Registration | 5 | Complete |
| ACF-041 | Password Reset | 3 | Complete |
| ACF-042 | User Login | 4 | Complete |

### Pending Approvals
- [ ] Product Owner
- [ ] Security Lead
- [ ] DevOps Lead

### Next Steps
1. Get required approvals on checklists
2. Run `/release tag v<version>` after approval
```

## Example

```
User: /release 1.2.0

Claude: Preparing release v1.2.0...

## Pre-Release Validation

Running validation scripts...
- validate-project.sh: PASS
- validate-documentation.sh: PASS
- validate-rnd-policy.sh: PASS
- test coverage: 87% (>80% required) PASS
- security scan: 0 vulnerable packages PASS

## Generating Release Artifacts

Created:
- artifacts/traceability/release-notes-1.2.0.md
- artifacts/traceability/security-review-1.2.0.md
- artifacts/traceability/release-readiness-1.2.0.md

## Release Summary

**Version**: 1.2.0
**Stories Included**: 5
**Test Coverage**: 87%
**Breaking Changes**: None

Ready for approval review.
```
