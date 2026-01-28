# Unit Test Quality Standards

## Core Principle

> "A test that can't fail is worthless. A test that tests nothing meaningful is worse than worthless - it's misleading."

## The Quality Hierarchy

```
BEHAVIOR > IMPLEMENTATION > EXISTENCE
```

Tests should verify **what** code does, not **how** it does it.

---

## Anti-Patterns: Tests to DELETE

### 1. Mock Verification Theater

**BAD: Testing that you called a mock correctly**
```csharp
[Fact]
public async Task Handle_WithFilter_ShouldPassFilterToRepository()
{
    // Setup mock to return data
    _repoMock.Setup(x => x.GetAsync(It.IsAny<Filter>()))
        .ReturnsAsync(new List<Item>());

    // Call handler
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert... that result exists? USELESS
    result.Should().NotBeNull();  // <-- WORTHLESS ASSERTION
}
```

**Problem:** You're testing that Moq works, not that your code works. The mock always returns data, so `NotBeNull()` always passes.

**GOOD: Test actual behavior or use integration tests**
```csharp
[Fact]
public async Task Handle_WithFilter_ReturnsOnlyMatchingItems()
{
    // Use real repository with in-memory database
    var context = CreateInMemoryContext();
    await SeedTestData(context);
    var repository = new Repository(context);
    var handler = new Handler(repository);

    var result = await handler.Handle(filterQuery, CancellationToken.None);

    // Assert on ACTUAL filtering behavior
    result.Items.Should().HaveCount(3);
    result.Items.Should().OnlyContain(x => x.Status == "Active");
}
```

### 2. Parameter Passing Tests (Redundant)

**BAD: Multiple tests that all verify parameter passing**
```csharp
[Fact] Handle_WithNicheFilter_ShouldPassNichesToRepository() { ... }
[Fact] Handle_WithPlatformFilter_ShouldPassPlatformsToRepository() { ... }
[Fact] Handle_WithSearchFilter_ShouldPassSearchToRepository() { ... }
[Fact] Handle_WithCollabFilter_ShouldPassCollabToRepository() { ... }
// All 4 tests do the same thing: verify a parameter is passed
```

**GOOD: One parameterized test OR test the actual filtering**
```csharp
[Theory]
[InlineData("gaming", 2)]      // Filter by niche
[InlineData("tech", 1)]        // Different niche
public async Task DiscoverCreators_FiltersByNiche_ReturnsMatchingCount(
    string niche, int expectedCount)
{
    // Arrange with real data
    await SeedCreatorsWithNiches();

    // Act
    var result = await _repository.DiscoverAsync(niches: new[] { niche });

    // Assert on actual filtered results
    result.Should().HaveCount(expectedCount);
}
```

### 3. Default Value Tests

**BAD: Testing that nulls stay null**
```csharp
[Fact]
public async Task Handle_WithNullAudienceSize_ShouldPassNullFollowerRange()
{
    var query = new Query(AudienceSize: null);
    await _handler.Handle(query, CancellationToken.None);
    // Verifies null was passed as null... so what?
}
```

**DELETE** - This tests nothing. If null in, null out, that's not behavior.

### 4. Trivial Echo Tests

**BAD: Testing that values are echoed back**
```csharp
[Fact]
public void Handle_ShouldSetPageSizeInResponse()
{
    var query = new Query(PageSize: 35);
    var result = await _handler.Handle(query);
    result.PageSize.Should().Be(35);  // Just echoing the input
}
```

**Only valuable if:** The value is transformed, validated, or could be changed.

### 5. Duplicate Edge Case Tests

**BAD: Testing the same edge case twice**
```csharp
[Fact] Handle_WithInvalidCursor_ShouldPassNull() { ... }
[Fact] Handle_WithEmptyCursor_ShouldPassNull() { ... }
[Fact] Handle_WithWhitespaceCursor_ShouldPassNull() { ... }
// All test: invalid input → null
```

**GOOD: One parameterized test**
```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
[InlineData("not-a-guid")]
public async Task Handle_WithInvalidCursor_TreatsAsNoCursor(string? cursor)
{
    var result = await _handler.Handle(new Query(Cursor: cursor));
    result.Items.Should().HaveCount(TotalItems); // Returns from beginning
}
```

---

## What Makes a GOOD Test

### 1. Tests Behavior, Not Implementation

| Bad (Implementation) | Good (Behavior) |
|---------------------|-----------------|
| "Calls repository with X" | "Returns items matching X" |
| "Passes filter to service" | "Filters out non-matching items" |
| "Sets cache key to Y" | "Returns cached result on second call" |

### 2. Has Meaningful Assertions

```csharp
// BAD - Weak assertions
result.Should().NotBeNull();
result.Should().NotBeEmpty();

// GOOD - Specific assertions
result.Items.Should().HaveCount(3);
result.Items.Should().OnlyContain(x => x.Status == "Active");
result.Items.Select(x => x.Id).Should().BeInDescendingOrder();
result.TotalCount.Should().Be(10);
result.HasMore.Should().BeTrue();
```

### 3. Tests One Thing Well

```csharp
// BAD - Testing multiple behaviors
[Fact]
public async Task Handle_ShouldFilterAndPaginateAndCacheResults()
{
    // Tests 3 things - which one broke if it fails?
}

// GOOD - One behavior per test
[Fact] Handle_WithNicheFilter_ReturnsOnlyMatchingNiches() { }
[Fact] Handle_WithPageSize_LimitsResultCount() { }
[Fact] Handle_OnSecondCall_ReturnsCachedResult() { }
```

### 4. Covers Edge Cases That Matter

**Worth testing:**
- Empty inputs
- Boundary values (0, max, max+1)
- Null vs empty collection
- Concurrent access
- Error conditions

**Not worth testing:**
- Null stays null
- Empty stays empty
- Default values are default

### 5. Uses Real Objects When Possible

```csharp
// BAD - Over-mocked
var repoMock = new Mock<IRepository>();
var cacheMock = new Mock<ICache>();
var loggerMock = new Mock<ILogger>();
var mapperMock = new Mock<IMapper>();
// Test becomes: "do mocks return what I told them to?"

// GOOD - Real objects with in-memory infrastructure
var context = new AppDbContext(InMemoryOptions);
var repository = new Repository(context);
var cache = new MemoryCache(new MemoryCacheOptions());
var handler = new Handler(repository, cache, NullLogger.Instance);
// Test becomes: "does the handler actually work?"
```

---

## Test Structure Template

```csharp
[Fact]
[Trait("Story", "ACF-XXX")]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up preconditions
    // Keep minimal, use helper methods for complex setup
    var entity = CreateTestEntity();
    await _repository.AddAsync(entity);

    // Act - Execute the behavior being tested
    // Should be ONE line (or very few)
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert - Verify the outcome
    // Be specific about what changed
    result.Status.Should().Be(ExpectedStatus);
    result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}
```

---

## Test Categories

### Unit Tests (Fast, Isolated)
- Test single class/method
- Mock external dependencies
- Run in milliseconds
- No database, network, file system

### Integration Tests (Real Dependencies)
- Test multiple components together
- Use real database (TestContainers or in-memory)
- Test actual queries, not mocked ones
- Run in seconds

### When to Use Each

| Scenario | Test Type |
|----------|-----------|
| Pure business logic | Unit |
| Complex calculations | Unit |
| Repository queries | Integration |
| Database-specific features (ILIKE, JSON) | Integration |
| API endpoints | Integration |
| Multi-service workflows | Integration |

---

## Red Flags Checklist

Before committing tests, check for:

- [ ] **No `result.Should().NotBeNull()` as the only assertion**
- [ ] **No duplicate tests** (same behavior tested multiple ways)
- [ ] **No testing default values** (null → null, empty → empty)
- [ ] **No testing mock setup** (verifying mock was called correctly)
- [ ] **Assertions are specific** (exact values, counts, conditions)
- [ ] **One behavior per test** (single reason to fail)
- [ ] **Test names describe the behavior** (not the implementation)
- [ ] **Edge cases that matter** (not theoretical edge cases)

---

## Coverage vs Quality

```
100% coverage with bad tests = FALSE CONFIDENCE
80% coverage with good tests = ACTUAL SAFETY
```

**Quality metrics:**
- Mutation testing score (do tests catch bugs?)
- Assertion density (assertions per test)
- Test isolation (can run in any order?)
- Failure clarity (is failure message helpful?)

---

## Refactoring Existing Tests

### Step 1: Identify Redundancy
```bash
# Find tests with similar names
grep -r "ShouldPass.*ToRepository" tests/
```

### Step 2: Consolidate
- Merge similar tests into Theory/parameterized tests
- Delete pure mock-verification tests
- Replace weak assertions with specific ones

### Step 3: Add Missing Coverage
- Error paths (what if it fails?)
- Edge cases (empty, null, boundary)
- Integration tests for DB-specific features

---

## Example: Before and After

### BEFORE (15 weak tests)
```csharp
Handle_WithNicheFilter_ShouldPassNichesToRepository()
Handle_WithPlatformFilter_ShouldPassPlatformsToRepository()
Handle_WithSearchTerm_ShouldPassSearchToRepository()
Handle_WithOpenToCollab_ShouldPassFilterToRepository()
Handle_WithExcludeUserId_ShouldPassIdToRepository()
Handle_WithCursor_ShouldPassCursorToRepository()
Handle_WithInvalidCursor_ShouldPassNull()
Handle_WithEmptyCursor_ShouldPassNull()
Handle_WithPageSize_ShouldPassPageSizeToRepository()
Handle_ShouldSetPageSizeInResponse()
Handle_WithNullAudienceSize_ShouldPassNullRange()
Handle_WhenNotCached_ShouldFetchFromRepository()  // GOOD
Handle_WhenCached_ShouldReturnFromCache()         // GOOD
Handle_ShouldMapProfileToDto()                    // GOOD
Handle_ShouldCalculateTotalFollowers()            // GOOD
```

### AFTER (6 quality tests)
```csharp
// Caching behavior
Handle_WhenCached_ReturnsCachedResult()
Handle_WhenNotCached_QueriesAndCachesResult()

// Filtering (use real repository)
DiscoverCreators_WithFilters_ReturnsMatchingProfiles()  // Integration test

// Data mapping
Handle_MapsProfileToDto_WithAllFields()
Handle_CalculatesTotalFollowers_FromAllPlatforms()

// Pagination
Handle_WithMoreResults_SetsCursorForNextPage()
```

**Result:** 6 meaningful tests > 15 useless tests

---

## Validation Questions

Ask before writing each test:

1. **"What behavior am I testing?"** (Not: what method am I calling)
2. **"How would this test fail?"** (If you can't answer, the test is weak)
3. **"Is this testing MY code or the framework/mocks?"**
4. **"Does another test already cover this?"**
5. **"Would I notice if I deleted this test?"** (If no, delete it)
