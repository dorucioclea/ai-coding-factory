---
name: test-driven-development
description: Enforce RED-GREEN-REFACTOR TDD cycle. Use when writing new features, fixing bugs, or modifying behavior. Ensures tests validate actual functionality.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: testing
---

# Test-Driven Development (TDD)

## Core Principle

> "If you didn't watch the test fail, you don't know if it tests the right thing."

This discipline ensures tests genuinely validate behavior rather than passing trivially.

## The Mandatory Cycle: Red-Green-Refactor

### RED Phase
1. Author a single failing test demonstrating desired functionality
2. Write the test BEFORE any implementation exists
3. Execute tests to confirm failure occurs for the correct reason
4. The failure should be because the feature doesn't exist, NOT due to syntax errors

### GREEN Phase
1. Write the **simplest possible code** satisfying the test requirements
2. Resist over-engineering or adding unneeded features
3. Confirm the test passes
4. Verify no existing tests break

### REFACTOR Phase
1. Only after passing tests, improve code quality
2. Eliminate duplication
3. Clarify naming
4. Extract utilities
5. Maintain test success throughout

## The Iron Law

> **NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST**

This admits **zero exceptions** without explicit human partner approval.

Code written before tests must be deleted entirely and reimplemented following TDD discipline.

## When TDD Applies

- ✅ New features
- ✅ Bug fixes
- ✅ Refactoring
- ✅ Behavior modifications

### Rare Exceptions (Require Authorization)
- Throwaway prototypes
- Generated code
- Pure configuration

## Addressing Common Rationalizations

| Excuse | Reality |
|--------|---------|
| "I'll write tests after" | Tests-after provide no proof since they pass immediately |
| "I tested it manually" | Manual testing lacks systematic rigor |
| "I already wrote the code" | Sunk costs shouldn't prevent rewriting |
| "Being pragmatic" | Test-first IS pragmatic—prevents production debugging |

## Verification Checklist

Before marking any task complete:

- [ ] Every function has a corresponding test
- [ ] Each test was watched failing first
- [ ] Test failed for expected reasons (missing feature, not syntax)
- [ ] Minimal code was written to pass
- [ ] All tests pass with clean output
- [ ] Real code tested (mocks only when unavoidable)
- [ ] Edge cases covered

## Integration with AI Coding Factory

When using TDD in this codebase:

1. **Story Linkage**: Every test must include `[Trait("Story", "ACF-###")]`
2. **Template Usage**: Use `.claude/templates/unit-test.cs.template`
3. **Coverage Target**: Maintain ≥80% for Domain/Application layers
4. **Traceability**: Tests must link back to story acceptance criteria
