# Gate 3: Validation → Release

## Purpose
Validates that all quality checks pass and the system is ready for release.

## Required Artifacts

| Artifact | Location | Description |
|----------|----------|-------------|
| Test Reports | `artifacts/test-reports/` | CI test results |
| Coverage Report | `artifacts/coverage/` | Code coverage data |
| Traceability Report | `artifacts/traceability/` | Story-test-commit linkage |
| Security Scan | `artifacts/security/` | Vulnerability scan results |

## Checklist

### CI/CD (CI)
- [ ] CI-1: All CI checks pass (build, test, lint)
- [ ] CI-2: No merge conflicts
- [ ] CI-3: PR approved by required reviewers
- [ ] CI-4: Branch up to date with main

### Test Results (TEST)
- [ ] TEST-1: All unit tests pass
- [ ] TEST-2: All integration tests pass
- [ ] TEST-3: E2E tests pass (if applicable)
- [ ] TEST-4: No flaky tests
- [ ] TEST-5: Performance tests meet NFRs

### Coverage (COV)
- [ ] COV-1: Domain layer ≥ 80% coverage
- [ ] COV-2: Application layer ≥ 80% coverage
- [ ] COV-3: No critical paths uncovered
- [ ] COV-4: Coverage did not decrease from baseline

### Traceability (TRACE)
- [ ] TRACE-1: All stories have linked tests
- [ ] TRACE-2: All tests have story IDs
- [ ] TRACE-3: Traceability report generated
- [ ] TRACE-4: No orphan tests (tests without story)

### Security (SEC)
- [ ] SEC-1: Security scan completed
- [ ] SEC-2: No critical vulnerabilities
- [ ] SEC-3: No high vulnerabilities (or documented exceptions)
- [ ] SEC-4: Dependency audit passed
- [ ] SEC-5: Security review checklist completed (for auth changes)

### Documentation (DOC)
- [ ] DOC-1: Documentation build succeeds
- [ ] DOC-2: API docs match implementation
- [ ] DOC-3: CHANGELOG updated

## Validation Commands

```bash
# Full validation suite
./scripts/validate-project.sh
./scripts/validate-documentation.sh
./scripts/validate-rnd-policy.sh

# Generate traceability report
python3 scripts/traceability/traceability.py validate \
  --story-root artifacts/stories \
  --test-root tests \
  --output artifacts/traceability/

# Check coverage thresholds
python3 scripts/coverage/check-coverage.py coverage.xml \
  --threshold 80 \
  --layers Domain,Application

# Security scan
dotnet list package --vulnerable
# or use security-reviewer agent
```

## Exit Criteria

All items checked = **GATE PASSED**
Any item unchecked = **GATE BLOCKED**

## Exception Process

If a gate item cannot be met:
1. Document the exception in `artifacts/exceptions/`
2. Get approval from appropriate stakeholder
3. Set remediation timeline
4. Proceed with documented risk acceptance

## Next Phase
Upon passing, proceed to **Release** phase.
