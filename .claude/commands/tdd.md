# /tdd - Test-Driven Development Workflow

Implement features using the RED-GREEN-REFACTOR TDD cycle.

## Usage

```
/tdd <feature-description>
/tdd ACF-042 implement order calculation
```

## Workflow

When this command is invoked:

1. **Activate TDD Skill**: Load the `tdd-workflow` skill for methodology guidance.

2. **RED Phase - Write Failing Test**:
   - Create a test file if one doesn't exist
   - Write a test that describes the expected behavior
   - Include the Story ID trait: `[Trait("Story", "ACF-XXX")]`
   - Run the test and verify it FAILS
   - Commit: `git commit -m "ACF-XXX test: Add failing test for <feature>"`

3. **GREEN Phase - Minimal Implementation**:
   - Write the minimum code to make the test pass
   - No extra features or "nice to haves"
   - Run the test and verify it PASSES
   - Commit: `git commit -m "ACF-XXX feat: Implement <feature> (green)"`

4. **REFACTOR Phase - Clean Up**:
   - Improve code quality while keeping tests green
   - Apply SOLID principles
   - Extract methods/classes if needed
   - Run tests after each refactoring step
   - Commit: `git commit -m "ACF-XXX refactor: Clean up <feature>"`

5. **Repeat**: For each new behavior, start at RED again.

## Test Naming Convention

Use: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
[Trait("Story", "ACF-042")]
public void CalculateTotal_WithValidItems_ReturnsSum()
```

## Structure

```csharp
[Fact]
public void TestMethod()
{
    // Arrange - Set up test data

    // Act - Execute the behavior

    // Assert - Verify the result
}
```

## Commands Available During TDD

- `dotnet test --filter "Category=Unit"` - Run unit tests
- `dotnet test --filter "Story=ACF-XXX"` - Run story-specific tests
- `dotnet watch test` - Watch mode for continuous feedback

## Exit Criteria

TDD session complete when:
- [ ] All required behaviors have tests
- [ ] All tests pass
- [ ] Code is refactored and clean
- [ ] Coverage >= 80%
- [ ] Story ID linked in all tests
