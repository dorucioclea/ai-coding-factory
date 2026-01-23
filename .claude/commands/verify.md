# /verify - Run Verification Loop

Run comprehensive verification checks on the current codebase.

## Usage

```
/verify              # Run full verification
/verify quick        # Quick build + unit tests only
/verify unit         # Unit tests only
/verify integration  # Integration tests only
/verify coverage     # Coverage analysis
/verify all          # Everything including traceability
```

## Verification Levels

### Quick (< 30s)
- Build check
- Unit tests only
- No coverage

### Standard (< 2min)
- Build check
- Unit tests
- Integration tests
- Basic coverage

### Full (< 5min)
- All tests
- Coverage >= 80% check
- Architecture tests
- Traceability validation
- Security scan

## Verification Steps

```bash
# Step 1: Build
dotnet build --no-incremental

# Step 2: Unit Tests
dotnet test --filter "Category=Unit" --no-build

# Step 3: Integration Tests
dotnet test --filter "Category=Integration" --no-build

# Step 4: Architecture Tests
dotnet test --filter "Category=Architecture" --no-build

# Step 5: Coverage Check
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
python3 scripts/coverage/check-coverage.py coverage.xml

# Step 6: Traceability
python3 scripts/traceability/traceability.py validate

# Step 7: Security Scan (if available)
dotnet list package --vulnerable
```

## Output

```
=== Verification Loop ===

[1/6] Building...                    ✓ PASS
[2/6] Unit Tests (142 tests)...      ✓ PASS
[3/6] Integration Tests (28 tests).. ✓ PASS
[4/6] Architecture Tests...          ✓ PASS
[5/6] Coverage (85.2%)...            ✓ PASS (>= 80%)
[6/6] Traceability...                ✓ PASS

=== Verification Complete ===
All checks passed! Safe to commit.
```

## Failure Handling

When verification fails:

1. **Identify failure** - Note which step failed
2. **Analyze cause** - Review error messages
3. **Fix minimally** - Only fix what's broken
4. **Re-verify** - Run full loop again

```
[3/6] Integration Tests...  ✗ FAIL

FAILED: OrderServiceTests.CreateOrder_WithInvalidCustomer_ThrowsException
  Expected: InvalidOperationException
  Actual: NullReferenceException

Action: Fix null check in OrderService.CreateOrder()
Then: Run /verify again
```

## Integration with Hooks

The verification loop integrates with existing hooks:

- `pre-test-setup.sh` - Runs before tests
- `post-test-traceability.sh` - Runs after tests
- `post-build-analyze.sh` - Runs after build

## Continuous Mode

Run verification in watch mode:

```bash
dotnet watch test --project tests/UnitTests
```

## Pre-Commit Verification

Always run before committing:

```bash
/verify && git commit -m "ACF-XXX feat: Description"
```

## Related Commands

- `/checkpoint` - Save state after passing verification
- `/tdd` - TDD workflow includes verification
- `/coverage` - Detailed coverage analysis
- `/traceability` - Detailed traceability report
