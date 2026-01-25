# /validate - Run All Validation Scripts

Execute all platform validation scripts and report results.

## Usage
```
/validate [scope]
```

Where scope can be:
- `all` (default) - Run all validations
- `project` - Validate project structure only
- `docs` - Validate documentation only
- `policy` - Validate R&D policy compliance only
- `traceability` - Validate story-test-commit linkage only
- `coverage` - Check code coverage (requires coverage.xml)

## Instructions

When invoked, run the appropriate validation scripts:

### Full Validation (`/validate` or `/validate all`)

```bash
# 1. Project structure validation
./scripts/validate-project.sh

# 2. Documentation validation
./scripts/validate-documentation.sh

# 3. R&D Policy validation
./scripts/validate-rnd-policy.sh

# 4. Traceability validation
python3 scripts/traceability/traceability.py validate

# 5. If in a .NET project with coverage, check coverage
if [ -f "coverage.xml" ]; then
    python3 scripts/coverage/check-coverage.py coverage.xml
fi
```

### Scoped Validation

For each scope, run only the relevant script:

- `/validate project`: `./scripts/validate-project.sh`
- `/validate docs`: `./scripts/validate-documentation.sh`
- `/validate policy`: `./scripts/validate-rnd-policy.sh`
- `/validate traceability`: `python3 scripts/traceability/traceability.py validate`
- `/validate coverage`: `python3 scripts/coverage/check-coverage.py coverage.xml`

## Output Format

Provide a summary report:

```
## Validation Results

| Check | Status | Details |
|-------|--------|---------|
| Project Structure | PASS/FAIL | <details> |
| Documentation | PASS/FAIL | <details> |
| R&D Policy | PASS/FAIL | <details> |
| Traceability | PASS/FAIL | <details> |
| Coverage | PASS/FAIL/N/A | <details> |

### Summary
- Total checks: X
- Passed: X
- Failed: X
- Skipped: X

### Required Actions
<List any failures that need to be addressed>
```

## Exit Criteria

- All validations must pass before claiming work is complete
- If any validation fails, list specific remediation steps
- Do not proceed with commits or releases until all pass
