# Gate 2: Development → Validation

## Purpose
Validates that implementation is complete and ready for validation testing.

## Required Artifacts

| Artifact | Location | Description |
|----------|----------|-------------|
| Story Files | `artifacts/stories/ACF-###.md` | All planned stories |
| ADRs | `docs/architecture/adr/` | For significant decisions |
| Implementation | `src/` or `templates/` | Working code |
| Tests | `tests/` | Unit and integration tests |
| Documentation | `docs/` | Updated documentation |

## Checklist

### Implementation (IMPL)
- [ ] IMPL-1: All stories in scope implemented
- [ ] IMPL-2: Code follows Clean Architecture boundaries
- [ ] IMPL-3: No TODO/FIXME markers in production code
- [ ] IMPL-4: Code passes linting
- [ ] IMPL-5: No compiler warnings

### Testing (TEST)
- [ ] TEST-1: Unit tests written for new behavior
- [ ] TEST-2: Integration tests for API endpoints
- [ ] TEST-3: Tests include story ID traits `[Trait("Story", "ACF-###")]`
- [ ] TEST-4: All tests pass locally
- [ ] TEST-5: Test coverage meets threshold (≥80% for Domain/Application)

### Documentation (DOC)
- [ ] DOC-1: Code documentation (XML comments for public APIs)
- [ ] DOC-2: README updated if public API changed
- [ ] DOC-3: ADRs created for major decisions
- [ ] DOC-4: API documentation updated

### Traceability (TRACE)
- [ ] TRACE-1: All commits reference story ID (ACF-###)
- [ ] TRACE-2: Stories have linked tests
- [ ] TRACE-3: Traceability report generated

### Security (SEC)
- [ ] SEC-1: No hardcoded secrets
- [ ] SEC-2: Input validation implemented
- [ ] SEC-3: Authentication/authorization correct
- [ ] SEC-4: No high/critical vulnerabilities introduced

## Validation Commands

```bash
# Run all development gate validations
./scripts/validate-project.sh

# Check tests pass
dotnet test --no-build --logger "console;verbosity=minimal"

# Check coverage
dotnet test --collect:"XPlat Code Coverage"
python3 scripts/coverage/check-coverage.py coverage.xml

# Check traceability
python3 scripts/traceability/traceability.py validate

# Check for TODOs
grep -r "TODO\|FIXME" src/ --include="*.cs" | grep -v test && echo "WARNING: TODOs found"
```

## Exit Criteria

All items checked = **GATE PASSED**
Any item unchecked = **GATE BLOCKED**

## Blocking Issues Resolution

| Issue | Resolution |
|-------|------------|
| Tests failing | Fix implementation or update tests |
| Coverage below threshold | Add missing tests |
| Traceability gaps | Add story ID to commits/tests |
| Security issues | Fix vulnerabilities before proceeding |

## Next Phase
Upon passing, proceed to **Validation** phase.
