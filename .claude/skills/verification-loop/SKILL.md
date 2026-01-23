---
name: verification-loop
description: Continuous verification pattern for maintaining code quality through checkpoints and automated validation loops
---

# Verification Loop Skill

This skill implements continuous verification patterns to ensure code quality throughout development, using checkpoints and automated validation.

## When to Activate

- During complex multi-step implementations
- When making changes that could break existing functionality
- Before committing or creating pull requests
- When working on critical business logic
- After refactoring sessions

## Core Concept: Continuous Verification

```
┌─────────────────────────────────────────────────────────┐
│              VERIFICATION LOOP                          │
│                                                         │
│    ┌──────────┐    ┌──────────┐    ┌──────────┐        │
│    │  CHANGE  │───>│  VERIFY  │───>│ PROCEED  │        │
│    └──────────┘    └──────────┘    └──────────┘        │
│         ^               │                │              │
│         │               v                │              │
│         │         ┌──────────┐           │              │
│         └─────────│   FIX    │<──────────┘              │
│                   └──────────┘   (if failed)            │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Verification Types

### 1. Checkpoint Verification (Manual)

Use checkpoints at key milestones to save state and verify progress.

**When to checkpoint:**
- After completing a logical unit of work
- Before starting a risky change
- After tests pass for a new feature
- Before refactoring

**Checkpoint command:**
```bash
# Save current state
git stash push -m "checkpoint: before refactoring OrderService"

# Or create a WIP commit
git commit -m "WIP: checkpoint before API changes"
```

### 2. Continuous Verification (Automated)

Run verification automatically after each change.

**Verification pipeline:**
```bash
#!/bin/bash
# verify.sh - Run full verification

set -e

echo "=== Running Verification Loop ==="

# Step 1: Build
echo "[1/5] Building..."
dotnet build --no-incremental

# Step 2: Unit Tests
echo "[2/5] Running unit tests..."
dotnet test --filter "Category=Unit" --no-build

# Step 3: Integration Tests
echo "[3/5] Running integration tests..."
dotnet test --filter "Category=Integration" --no-build

# Step 4: Architecture Tests
echo "[4/5] Running architecture tests..."
dotnet test --filter "Category=Architecture" --no-build

# Step 5: Coverage Check
echo "[5/5] Checking coverage..."
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

echo "=== Verification Complete ==="
```

## Verification Levels

| Level | When | What | Speed |
|-------|------|------|-------|
| **Quick** | After each file save | Syntax, lint | < 1s |
| **Unit** | After logical changes | Unit tests | < 30s |
| **Integration** | After feature complete | Integration tests | < 2min |
| **Full** | Before commit/PR | All tests + coverage | < 5min |

## Verification Checklist

### Pre-Change Verification
- [ ] Current tests pass
- [ ] No uncommitted changes (or stashed)
- [ ] On correct branch
- [ ] Dependencies up to date

### Post-Change Verification
- [ ] Build succeeds
- [ ] All tests pass
- [ ] No new warnings
- [ ] Coverage maintained (>= 80%)
- [ ] No security vulnerabilities introduced

### Pre-Commit Verification
- [ ] Full test suite passes
- [ ] Traceability complete (Story ID linked)
- [ ] Documentation updated
- [ ] No TODO/FIXME in new code
- [ ] Commit message follows convention

## Grader Patterns

### Pass@K Metrics

Track how many attempts it takes to get a passing result:

```
Pass@1: First attempt success rate
Pass@3: Success within 3 attempts
Pass@5: Success within 5 attempts
```

**Healthy project targets:**
- Pass@1 >= 90% for unit tests
- Pass@1 >= 80% for integration tests
- Pass@3 = 100% (all tests eventually pass)

### Grader Types

| Grader | Purpose | Example |
|--------|---------|---------|
| **Binary** | Pass/Fail | Build succeeds |
| **Threshold** | Above minimum | Coverage >= 80% |
| **Comparison** | Better than baseline | Performance not regressed |
| **Pattern** | Matches expected | Output format correct |

## Implementation: Hooks Integration

### Pre-Build Hook
```bash
#!/bin/bash
# .claude/hooks/pre-build-check.sh

# Verify no uncommitted changes to critical files
if git diff --name-only | grep -q "appsettings.json"; then
    echo "Warning: appsettings.json has uncommitted changes"
fi

# Check for TODO markers
todo_count=$(grep -r "TODO" src/ --include="*.cs" | wc -l)
if [ "$todo_count" -gt 10 ]; then
    echo "Warning: $todo_count TODO markers found"
fi
```

### Post-Test Hook
```bash
#!/bin/bash
# .claude/hooks/post-test-traceability.sh

# Verify all new tests have Story traits
new_tests=$(git diff --name-only HEAD~1 | grep "Test.*\.cs$")
for test_file in $new_tests; do
    if ! grep -q 'Trait("Story"' "$test_file"; then
        echo "Error: $test_file missing Story trait"
        exit 1
    fi
done
```

## Verification Failure Response

When verification fails:

1. **Identify the failure** - Which check failed?
2. **Analyze the cause** - Why did it fail?
3. **Fix minimally** - Only fix what's broken
4. **Re-verify** - Run the full loop again
5. **Document** - If pattern, add to anti-patterns

```
FAILURE → ANALYZE → FIX → VERIFY → (repeat until pass)
```

## State Management

### Checkpoint State
```json
{
  "checkpoint_id": "chk-20240115-1430",
  "branch": "feature/ACF-042-user-auth",
  "last_passing_commit": "abc123",
  "coverage": 85.2,
  "test_results": {
    "passed": 142,
    "failed": 0,
    "skipped": 3
  },
  "timestamp": "2024-01-15T14:30:00Z"
}
```

### Recovery from Checkpoint
```bash
# Restore to last known good state
git reset --hard $LAST_PASSING_COMMIT

# Or restore stashed checkpoint
git stash pop
```

## Integration with CI/CD

```yaml
# .github/workflows/verify.yml
name: Verification Loop

on: [push, pull_request]

jobs:
  verify:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Quick Verify
        run: dotnet build

      - name: Unit Tests
        run: dotnet test --filter "Category=Unit"

      - name: Integration Tests
        run: dotnet test --filter "Category=Integration"

      - name: Coverage Check
        run: |
          dotnet test /p:CollectCoverage=true
          python3 scripts/coverage/check-coverage.py coverage.xml

      - name: Traceability Check
        run: python3 scripts/traceability/traceability.py validate
```

## Success Criteria

A verification loop is healthy when:
- [ ] All verification levels can run independently
- [ ] Failures are detected within the appropriate level
- [ ] Recovery from failures is straightforward
- [ ] Checkpoints are saved at logical milestones
- [ ] CI/CD runs full verification on every PR
