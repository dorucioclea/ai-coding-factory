---
name: tdd-workflow
description: Test-Driven Development methodology enforcing the RED-GREEN-REFACTOR cycle for systematic, high-quality code development
---

# TDD Workflow Skill

This skill enforces Test-Driven Development (TDD) methodology using the classic RED-GREEN-REFACTOR cycle.

## When to Activate

- Before implementing any new feature or functionality
- When fixing bugs (write failing test first)
- When adding new behavior to existing code
- When refactoring with safety nets

## Core Principle: RED-GREEN-REFACTOR

```
┌─────────────────────────────────────────────────────────┐
│                    TDD CYCLE                            │
│                                                         │
│    ┌─────┐     ┌───────┐     ┌──────────┐              │
│    │ RED │ ──> │ GREEN │ ──> │ REFACTOR │ ──┐          │
│    └─────┘     └───────┘     └──────────┘   │          │
│       ^                                      │          │
│       └──────────────────────────────────────┘          │
│                                                         │
│  RED:      Write a failing test first                   │
│  GREEN:    Write minimal code to make it pass           │
│  REFACTOR: Improve code while keeping tests green       │
└─────────────────────────────────────────────────────────┘
```

## The Three Laws of TDD

1. **You may not write production code** until you have written a failing unit test
2. **You may not write more of a unit test** than is sufficient to fail
3. **You may not write more production code** than is sufficient to pass the test

## Workflow Steps

### Phase 1: RED - Write a Failing Test

1. **Understand the requirement** - What behavior are we implementing?
2. **Write the test first** - Before any implementation
3. **Run the test** - Verify it fails for the right reason
4. **Commit the failing test** - Document the expected behavior

```csharp
// Example: Testing an OrderCalculator
[Fact]
[Trait("Story", "ACF-042")]
public void CalculateTotal_WithValidItems_ReturnsCorrectSum()
{
    // Arrange
    var calculator = new OrderCalculator();
    var items = new[] { new OrderItem("Widget", 10.00m, 2) };

    // Act
    var total = calculator.CalculateTotal(items);

    // Assert
    total.Should().Be(20.00m);
}
```

### Phase 2: GREEN - Make It Pass

1. **Write minimal code** - Only enough to pass the test
2. **Don't over-engineer** - Resist the urge to add extra features
3. **Run the test** - Verify it passes
4. **Commit the passing implementation**

```csharp
// Minimal implementation to pass the test
public class OrderCalculator
{
    public decimal CalculateTotal(OrderItem[] items)
    {
        return items.Sum(i => i.Price * i.Quantity);
    }
}
```

### Phase 3: REFACTOR - Improve the Code

1. **Review the code** - Look for improvements
2. **Apply clean code principles** - But keep tests green
3. **Run tests after each change** - Ensure nothing breaks
4. **Commit the refactored code**

```csharp
// Refactored with better naming and structure
public class OrderCalculator
{
    public decimal CalculateTotal(IEnumerable<OrderItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Sum(CalculateItemTotal);
    }

    private static decimal CalculateItemTotal(OrderItem item)
        => item.Price * item.Quantity;
}
```

## Test Naming Convention

Use the pattern: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
public void CalculateTotal_WithEmptyCart_ReturnsZero()
[Fact]
public void CalculateTotal_WithNullItems_ThrowsArgumentNullException()
[Fact]
public void CalculateTotal_WithNegativeQuantity_ThrowsInvalidOperationException()
```

## Test Structure: Arrange-Act-Assert

```csharp
[Fact]
public void ExampleTest()
{
    // Arrange - Set up the test scenario
    var sut = new SystemUnderTest();
    var input = CreateTestInput();

    // Act - Execute the behavior being tested
    var result = sut.DoSomething(input);

    // Assert - Verify the expected outcome
    result.Should().BeEquivalentTo(expectedOutput);
}
```

## Edge Cases Checklist

Always consider tests for:
- [ ] Null inputs
- [ ] Empty collections
- [ ] Boundary values (0, -1, max, min)
- [ ] Invalid states
- [ ] Concurrent access (if applicable)
- [ ] Exception scenarios

## Integration with Story Traceability

Every test must include the Story ID trait:

```csharp
[Fact]
[Trait("Story", "ACF-XXX")]  // Link to user story
[Trait("Category", "Unit")]   // Test category
public void Feature_Scenario_Expectation()
```

## TDD Anti-Patterns to Avoid

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Writing tests after code | Loses design benefits | Always test first |
| Skipping RED phase | Tests may not verify behavior | Run test, see it fail |
| Over-engineering in GREEN | Wastes time, adds complexity | Minimal code only |
| Skipping REFACTOR | Technical debt accumulates | Always clean up |
| Testing implementation details | Brittle tests | Test behavior, not internals |
| Large test methods | Hard to understand failures | One assertion per test |

## Verification Commands

```bash
# Run tests in watch mode (continuous feedback)
dotnet watch test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test category
dotnet test --filter "Category=Unit"

# Run tests for a specific story
dotnet test --filter "Story=ACF-042"
```

## Success Criteria

A TDD session is complete when:
- [ ] All new behavior has tests written FIRST
- [ ] Tests clearly document expected behavior
- [ ] Code passes all tests
- [ ] Code is refactored and clean
- [ ] Coverage >= 80% for Domain/Application layers
- [ ] All tests include Story ID traits
