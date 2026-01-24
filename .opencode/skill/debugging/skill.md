---
name: systematic-debugging
description: Four-phase root cause analysis framework. Use when facing test failures, production bugs, unexpected behavior, or performance issues. Prevents symptom-fixing.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: testing
---

# Systematic Debugging

## Core Principle

> **"ALWAYS find root cause before attempting fixes. Symptom fixes are failure."**

## When to Use This Skill

- Test failures
- Production bugs
- Unexpected behavior
- Performance problems
- Build failures
- Integration issues

**Especially critical when:**
- Under time pressure
- A "quick fix" seems obvious
- After multiple failed attempts

## Four-Phase Process

### Phase 1: Root Cause Investigation

1. **Analyze errors carefully** - Read the full stack trace
2. **Reproduce consistently** - Can you trigger it reliably?
3. **Review recent changes** - What changed since it last worked?
4. **Gather evidence** - In multi-component systems, isolate the source
5. **Trace data flow** - Follow the call stack from symptom to source

```
Symptom → Component → Function → Line → Root Cause
```

### Phase 2: Pattern Analysis

1. **Locate working examples** - Find similar code that works
2. **Compare against references** - What's different?
3. **Identify differences** - Subtle variations matter
4. **Understand dependencies** - What does this code rely on?

### Phase 3: Hypothesis and Testing

Apply the scientific method:

1. **Form specific hypothesis** - "The bug occurs because X"
2. **Make minimal changes** - One variable at a time
3. **Test the hypothesis** - Does the evidence support it?
4. **Iterate if needed** - Wrong hypothesis → new hypothesis

### Phase 4: Implementation

1. **Create a failing test** - Prove the bug exists
2. **Implement single fix** - Address root cause only
3. **Verify results** - Test passes, no regressions

## Critical Guardrails

### Red Flags (Return to Phase 1)

- ❌ Proposing fixes without investigation
- ❌ Attempting multiple changes simultaneously
- ❌ Skipping test creation

### The Rule of Three

> **If ≥ 3 fix attempts fail: STOP and question the architecture**

Don't continue patching symptoms. The problem is likely structural.

## Debugging Decision Tree

```
Bug Reported
    │
    ▼
Can you reproduce it?
    │
    ├─ NO → Gather more info, check logs
    │
    └─ YES → What changed recently?
              │
              ├─ Something changed → Review that change
              │
              └─ Nothing changed → Environment issue?
                                   Dependency update?
                                   Data change?
```

## Common Root Causes by Category

### .NET Specific
- Null reference: Check initialization order
- Async deadlock: Missing ConfigureAwait or sync-over-async
- EF Core: N+1 queries, detached entity updates
- DI: Wrong lifetime (Scoped in Singleton)

### React Specific
- Stale closure: Missing dependency in useEffect
- Infinite render: State update in render path
- Memory leak: Missing cleanup in useEffect
- Hydration mismatch: Server/client content differs

## Integration with AI Coding Factory

1. **Document findings** in `artifacts/debugging/` for future reference
2. **Link to story** if bug relates to a feature: `ACF-###`
3. **Create regression test** using templates
4. **Update error-patterns.json** if new pattern discovered
