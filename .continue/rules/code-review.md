---
description: "Systematic code review framework for requesting and providing reviews. Use before merging PRs or when reviewing others' code. Ensures quality and knowledge sharing."
globs: ["**/*"]
---


# Code Review

## Core Principles

1. **Reviews are collaborative** - Not adversarial
2. **Focus on the code** - Not the person
3. **Be specific** - Vague feedback isn't actionable
4. **Prioritize** - Not all issues are equal

## When to Request Review

- Before merging any PR
- After significant refactoring
- When uncertain about approach
- For security-sensitive changes
- When learning new patterns

## Review Request Checklist

Before requesting review, ensure:

- [ ] All tests pass
- [ ] Code compiles without warnings
- [ ] Self-review completed (read your own diff)
- [ ] PR description explains the "why"
- [ ] Related story ID linked: `ACF-###`
- [ ] No debug code or console.logs
- [ ] No commented-out code

## Providing Reviews

### Severity Levels

| Level | Symbol | Meaning |
|-------|--------|---------|
| Blocker | 🚫 | Must fix before merge |
| Major | ⚠️ | Should fix, discuss if disagree |
| Minor | 💡 | Nice to have, optional |
| Nitpick | 📝 | Style preference, take or leave |
| Question | ❓ | Need clarification |
| Praise | ✨ | Good work, worth highlighting |

### Review Categories

#### 1. Correctness
- Does it do what it claims?
- Edge cases handled?
- Error handling appropriate?

#### 2. Security
- Input validation present?
- No SQL injection risks?
- Secrets properly managed?
- Authentication/authorization correct?

#### 3. Performance
- N+1 query risks?
- Unnecessary allocations?
- Missing caching opportunities?

#### 4. Maintainability
- Clear naming?
- Appropriate abstraction level?
- Single responsibility?
- Tests adequate?

#### 5. Architecture
- Follows Clean Architecture?
- Layer boundaries respected?
- Dependencies flow correctly?

## Comment Templates

### Blocker
```
🚫 **Blocker**: [Issue description]

This will cause [problem] because [reason].

Suggested fix:
```code
// corrected code here
```
```

### Major Issue
```
⚠️ **Major**: [Issue description]

[Explanation of concern]

Consider: [alternative approach]
```

### Question
```
❓ **Question**: [Your question]

I'm wondering about [specific concern]. Could you explain the reasoning?
```

### Praise
```
✨ Nice use of [pattern/technique] here! This makes [benefit] much cleaner.
```

## .NET Specific Checks

- [ ] Async methods properly awaited
- [ ] CancellationToken propagated
- [ ] IDisposable properly disposed
- [ ] EF Core queries optimized
- [ ] Null checks with pattern matching
- [ ] Records used for DTOs
- [ ] Primary constructors where appropriate

## React Specific Checks

- [ ] Hooks rules followed
- [ ] Dependencies arrays complete
- [ ] Memoization appropriate (not premature)
- [ ] Error boundaries in place
- [ ] Accessibility attributes present
- [ ] Keys unique and stable
- [ ] No prop drilling (use context if deep)

## Integration with AI Coding Factory

### Automated Checks
The following are validated by hooks:
- Story ID in commits: `post-commit-validate.sh`
- Test coverage: `pre-release-checklist.sh`
- Architecture: `post-build-analyze.sh`

### Review Artifacts
Store significant review discussions in:
- `docs/architecture/adr/` for decisions
- `artifacts/reviews/` for audit trail

### Traceability
Every reviewed PR should link:
- Story: `ACF-###`
- Tests: `[Trait("Story", "ACF-###")]`
- Commit: `ACF-### Description`
