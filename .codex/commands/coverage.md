# /coverage - Check Code Coverage

Run tests with coverage and analyze results.

## Usage
```
/coverage [scope]
```

Where scope is:
- (none) - Run tests and show coverage summary
- `report` - Generate detailed coverage report
- `check` - Validate coverage meets thresholds (>=80%)
- `uncovered <file>` - Show uncovered lines in a file

## Instructions

### Run Coverage (`/coverage` or `/coverage report`)

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Find the coverage file
COVERAGE_FILE=$(find ./coverage -name "coverage.cobertura.xml" | head -1)

# If coverage script exists, use it
if [ -f "scripts/coverage/check-coverage.py" ]; then
    python3 scripts/coverage/check-coverage.py "$COVERAGE_FILE"
fi
```

### Parse Coverage Report

When analyzing coverage.cobertura.xml or coverage.xml:

1. Extract overall coverage percentage
2. Break down by namespace/layer
3. Identify files below threshold

### Output Format

```markdown
# Code Coverage Report
Generated: <timestamp>

## Summary

| Metric | Value | Status |
|--------|-------|--------|
| Line Coverage | X% | PASS/FAIL |
| Branch Coverage | X% | PASS/FAIL |
| Threshold | 80% | - |

## Coverage by Layer

| Layer | Line % | Branch % | Status |
|-------|--------|----------|--------|
| Domain | X% | X% | PASS |
| Application | X% | X% | PASS |
| Infrastructure | X% | X% | WARN |
| API | X% | X% | N/A |

## Files Below Threshold

| File | Line % | Uncovered Lines |
|------|--------|-----------------|
| OrderService.cs | 65% | 45-52, 78-85 |
| PaymentHandler.cs | 72% | 34-40 |

## Recommendations

1. Add tests for uncovered business logic
2. Focus on Domain and Application layers
3. Consider edge cases and error paths
```

### Check Thresholds (`/coverage check`)

Validate coverage meets requirements:

```bash
# Minimum thresholds
DOMAIN_THRESHOLD=80
APPLICATION_THRESHOLD=80
OVERALL_THRESHOLD=80

# Parse coverage and compare
# Exit 1 if below threshold
```

### Show Uncovered Lines (`/coverage uncovered <file>`)

Display specific uncovered lines in a file:

```markdown
## Uncovered Lines: <filename>

Line 45-52:
```csharp
public void ProcessOrder(Order order)
{
    if (order.Status == OrderStatus.Cancelled)  // Line 45
    {
        throw new OrderCancelledException();    // Line 46-47 uncovered
    }
    // ...
}
```

**Reason**: Error path not tested
**Suggestion**: Add test for cancelled order scenario
```

## Coverage Requirements

Per Corporate R&D Policy:
- **Domain Layer**: >=80% line and branch coverage
- **Application Layer**: >=80% line and branch coverage
- **Infrastructure**: Best effort (external dependencies)
- **API**: Integration tests cover endpoints

## Tips for Improving Coverage

1. **Test edge cases**: null inputs, empty collections, boundary values
2. **Test error paths**: exceptions, validation failures
3. **Test business rules**: domain logic variations
4. **Use test data builders**: consistent test data setup
5. **Parameterized tests**: `[Theory]` with `[InlineData]`

## Example

```
User: /coverage

Claude: Running tests with coverage...

dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

## Coverage Summary

| Metric | Value | Status |
|--------|-------|--------|
| Overall | 87% | PASS |
| Domain | 92% | PASS |
| Application | 85% | PASS |
| Infrastructure | 78% | WARN |

### Files Needing Attention

1. `PaymentService.cs` (72%) - Missing error handling tests
2. `OrderValidator.cs` (68%) - Missing validation edge cases

Run `/coverage uncovered PaymentService.cs` for details.
```
