# /review-tests - Unit Test Quality Review

Analyze unit tests for quality issues: redundancy, weak assertions, mock theater, and missing coverage.

## Usage
```
/review-tests <target> [options]
```

Target:
- File path to test file
- Directory path to scan all tests
- Class name to find and review
- `--all` to review entire test suite

Options:
- `--fix` - Automatically fix/consolidate redundant tests
- `--report` - Generate detailed markdown report
- `--strict` - Fail on any quality issues found

## Instructions

### Step 1: Load Quality Standards

Read the skill file for test quality guidelines:
```
skills/unit-test-quality.md
```

### Step 2: Scan Test Files

For each test file, analyze:

1. **Redundancy Detection**
   - Tests with similar names doing the same thing
   - Multiple tests verifying parameter passing
   - Duplicate edge case coverage

2. **Weak Assertion Detection**
   - `result.Should().NotBeNull()` as only assertion
   - `result.Should().NotBeEmpty()` without specifics
   - No assertions on actual values

3. **Mock Theater Detection**
   - Tests that only verify mock was called
   - No assertions on return values
   - Overly complex mock setups

4. **Missing Coverage**
   - No error path tests
   - No edge case tests
   - No integration tests for DB-specific features

### Step 3: Generate Report

```markdown
## Test Quality Report for <Target>

### Summary
| Metric | Count |
|--------|-------|
| Total Tests | XX |
| Quality Tests | XX |
| Redundant Tests | XX |
| Weak Tests | XX |
| Missing Tests | XX |

### Quality Score: X/10

### Issues Found

#### REDUNDANT (Delete or Consolidate)
| Test | Issue | Recommendation |
|------|-------|----------------|
| Handle_WithNicheFilter_ShouldPass... | Same as 3 other tests | Consolidate into parameterized test |

#### WEAK (Strengthen Assertions)
| Test | Issue | Fix |
|------|-------|-----|
| Handle_WithFilter_ShouldWork | Only `NotBeNull()` assertion | Assert on filtered count/values |

#### MOCK THEATER (Rewrite or Delete)
| Test | Issue | Fix |
|------|-------|-----|
| Handle_ShouldCallRepository | Tests mock setup, not behavior | Use integration test or delete |

#### MISSING (Add)
| Scenario | Why Important |
|----------|---------------|
| Error handling when repository throws | Verifies fault tolerance |
| Empty results edge case | Boundary condition |

### Recommended Actions

1. **DELETE** these tests (provide no value):
   - Test1
   - Test2

2. **CONSOLIDATE** these into parameterized tests:
   - Group: [Test3, Test4, Test5] → Theory with 3 cases

3. **STRENGTHEN** these tests:
   - Test6: Add assertion on `result.Items.Count`

4. **ADD** these missing tests:
   - ErrorHandling_WhenRepositoryThrows_ReturnsError
   - Handle_WithEmptyResult_ReturnsEmptyList
```

### Step 4: Fix Mode (`--fix`)

When `--fix` is specified:

1. **Delete redundant tests** - Remove duplicates
2. **Consolidate similar tests** - Convert to `[Theory]` with `[InlineData]`
3. **Strengthen weak assertions** - Add specific value assertions
4. **Add TODO comments** - For tests that need manual rewrite

```csharp
// BEFORE: 4 separate tests
[Fact] Handle_WithNicheFilter_ShouldPass() { }
[Fact] Handle_WithPlatformFilter_ShouldPass() { }
[Fact] Handle_WithSearchFilter_ShouldPass() { }
[Fact] Handle_WithCollabFilter_ShouldPass() { }

// AFTER: 1 parameterized test
[Theory]
[InlineData("niche", 2)]
[InlineData("platform", 3)]
[InlineData("search", 1)]
public void Handle_WithFilter_ReturnsMatchingCount(string filterType, int expected) { }
```

## Quality Checklist

For each test, verify:

- [ ] Tests behavior, not implementation
- [ ] Has meaningful assertions (not just `NotBeNull`)
- [ ] Not a duplicate of another test
- [ ] Covers a unique scenario
- [ ] Would fail if the code broke
- [ ] Name describes what it tests

## Anti-Patterns to Flag

| Pattern | Example | Why Bad |
|---------|---------|---------|
| Mock Verification Only | `_mock.Verify(x => x.Called())` | Tests mock, not code |
| NotBeNull Assertion | `result.Should().NotBeNull()` | Weak, almost always passes |
| Default Value Test | Test that null in = null out | No behavior tested |
| Echo Test | Test that input = output | Trivial |
| Duplicate Test | Same scenario, different name | Redundant |

## Example

```
User: /review-tests tests/VlogForge.UnitTests/Application/Discovery/

Claude: Scanning 3 test files in Discovery...

## Test Quality Report

### Summary
| Metric | Count |
|--------|-------|
| Total Tests | 78 |
| Quality Tests | 34 (44%) |
| Redundant Tests | 28 (36%) |
| Weak Tests | 12 (15%) |
| Missing Tests | 4 |

### Quality Score: 4/10

### Issues Found

#### REDUNDANT (28 tests → 8 after consolidation)
- Handler tests: 15 "ShouldPass*ToRepository" → 1 parameterized test
- Validator tests: 6 duplicate boundary tests → 2 parameterized tests
- Repository tests: 7 similar filter tests → 3 tests

#### WEAK (12 tests)
- 8 tests with only `NotBeNull()` assertions
- 4 tests with no verification of filtered results

#### MISSING
- No error path tests (what if cache fails?)
- No concurrent access tests
- Search tests need PostgreSQL (move to integration)

### Recommended Actions

Run `/review-tests tests/VlogForge.UnitTests/Application/Discovery/ --fix` to:
1. Delete 20 redundant tests
2. Consolidate 15 tests into 4 parameterized tests
3. Strengthen 8 weak assertions
4. Add TODO markers for 4 missing tests

Estimated impact: 78 tests → 46 quality tests
```

## Integration with TDD

This command complements `/tdd`:
- `/tdd` - Enforces write-test-first workflow
- `/review-tests` - Ensures tests are high quality

Use both for comprehensive test discipline.
