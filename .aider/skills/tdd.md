# Test-Driven Development (TDD)

## Core Principle

> "If you didn't watch the test fail, you don't know if it tests the right thing."

## The Iron Law

```
NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST
```

Write code before the test? **Delete it. Start over.**

- Don't keep it as "reference"
- Don't "adapt" it while writing tests
- Delete means delete

## When to Use

**Always:**
- New features
- Bug fixes
- Refactoring
- Behavior changes

**Rare Exceptions (require explicit approval):**
- Throwaway prototypes
- Generated code
- Pure configuration

## Red-Green-Refactor Cycle

### RED - Write Failing Test

Write one minimal test demonstrating desired behavior.

**Requirements:**
- One behavior per test
- Clear descriptive name
- Real code (mocks only when unavoidable)

**Run test - it MUST fail:**
```bash
# .NET
dotnet test --filter "MethodName"

# JavaScript/TypeScript
npm test path/to/test.test.ts
```

**Verify:**
- Test fails (not errors)
- Failure message is expected
- Fails because feature missing (not typos)

### GREEN - Minimal Code

Write the **simplest possible code** to make the test pass.

- Don't add extra features
- Don't over-engineer
- Don't refactor yet

**Run test - it MUST pass:**
```bash
dotnet test  # or npm test
```

**Verify:**
- Test passes
- All other tests still pass
- No warnings or errors

### REFACTOR - Clean Up

Only after green:
- Remove duplication
- Improve names
- Extract helpers
- Optimize if needed

**Keep tests green throughout refactoring.**

### Repeat

Next failing test for next behavior.

## .NET Test Pattern

```csharp
[Fact]
[Trait("Story", "ACF-042")]  // REQUIRED: Link to story
[Trait("Category", "Unit")]
public void CalculateTotal_WithValidItems_ReturnsSum()
{
    // Arrange
    var calculator = new OrderCalculator();
    var items = new[] { new OrderItem("Widget", 10.00m, 2) };

    // Act
    var result = calculator.CalculateTotal(items);

    // Assert
    result.Should().Be(20.00m);
}

[Fact]
[Trait("Story", "ACF-042")]
public void CalculateTotal_WithEmptyItems_ReturnsZero()
{
    // Arrange
    var calculator = new OrderCalculator();

    // Act
    var result = calculator.CalculateTotal(Array.Empty<OrderItem>());

    // Assert
    result.Should().Be(0m);
}

[Fact]
[Trait("Story", "ACF-042")]
public void CalculateTotal_WithNullItems_ThrowsArgumentNullException()
{
    // Arrange
    var calculator = new OrderCalculator();

    // Act
    var act = () => calculator.CalculateTotal(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
}
```

## JavaScript/TypeScript Test Pattern

### Unit Tests (Jest/Vitest)

```typescript
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from './Button'

describe('Button Component', () => {
  it('renders with correct text', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByText('Click me')).toBeInTheDocument()
  })

  it('calls onClick when clicked', () => {
    const handleClick = jest.fn()
    render(<Button onClick={handleClick}>Click</Button>)

    fireEvent.click(screen.getByRole('button'))

    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Click</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })
})
```

### API Integration Tests

```typescript
describe('GET /api/markets', () => {
  it('returns markets successfully', async () => {
    const request = new NextRequest('http://localhost/api/markets')
    const response = await GET(request)
    const data = await response.json()

    expect(response.status).toBe(200)
    expect(data.success).toBe(true)
    expect(Array.isArray(data.data)).toBe(true)
  })

  it('validates query parameters', async () => {
    const request = new NextRequest('http://localhost/api/markets?limit=invalid')
    const response = await GET(request)

    expect(response.status).toBe(400)
  })
})
```

### E2E Tests (Playwright)

```typescript
import { test, expect } from '@playwright/test'

test('user can search markets', async ({ page }) => {
  await page.goto('/markets')
  await expect(page.locator('h1')).toContainText('Markets')

  await page.fill('input[placeholder="Search"]', 'election')
  await page.waitForTimeout(600) // debounce

  const results = page.locator('[data-testid="market-card"]')
  await expect(results).toHaveCount(5, { timeout: 5000 })
})
```

## Coverage Requirements

| Layer | Minimum Coverage |
|-------|-----------------|
| Domain | 80% |
| Application | 80% |
| Infrastructure | 60% |
| API/UI | 50% |

```bash
# .NET
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# JavaScript
npm run test:coverage
```

## Common Rationalizations (All Invalid)

| Excuse | Reality |
|--------|---------|
| "Too simple to test" | Simple code breaks. Test takes 30 seconds. |
| "I'll test after" | Tests passing immediately prove nothing. |
| "Already manually tested" | Ad-hoc ≠ systematic. No record, can't re-run. |
| "Deleting X hours is wasteful" | Sunk cost fallacy. Unverified code = debt. |
| "TDD will slow me down" | TDD faster than debugging in production. |
| "Need to explore first" | Fine. Throw away exploration, start fresh with TDD. |
| "Test hard = design unclear" | Listen to test. Hard to test = hard to use. |

## Red Flags - STOP and Start Over

- Code written before test
- Test passes immediately without changes
- Can't explain why test failed
- "I already manually tested it"
- "Just this once"
- "Keep code as reference"

**All mean: Delete code. Start over with TDD.**

## Test Naming Convention

Use: `MethodName_Scenario_ExpectedBehavior`

```csharp
CalculateTotal_WithValidItems_ReturnsSum
CalculateTotal_WithEmptyItems_ReturnsZero
CalculateTotal_WithNullItems_ThrowsArgumentNullException
```

## Integration with AI Coding Factory

1. **Story Linkage**: Every test MUST include `[Trait("Story", "ACF-###")]`
2. **Traceability**: Tests link to story acceptance criteria
3. **Coverage Target**: ≥80% for Domain/Application layers
4. **Commit Format**: `ACF-### test: Add failing test for <feature>`

## Verification Checklist

Before marking work complete:

- [ ] Every new function has a test
- [ ] Watched each test fail before implementing
- [ ] Each test failed for expected reason
- [ ] Wrote minimal code to pass
- [ ] All tests pass
- [ ] Tests use real code (mocks only if unavoidable)
- [ ] Edge cases and errors covered
- [ ] Story trait included on all tests
- [ ] Coverage ≥80% for Domain/Application

Can't check all boxes? You skipped TDD. Start over.