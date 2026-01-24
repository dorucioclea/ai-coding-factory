---
name: planning-with-files
description: 3-file persistent planning pattern using markdown files. Use at start of complex tasks to maintain state across sessions. Treats filesystem as working memory.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: planning
---

# Planning with Files

## Core Concept

Use the filesystem as **persistent working memory** rather than relying solely on volatile context windows. This pattern ensures continuity across sessions and provides audit trail.

## The 3-File Pattern

### 1. task_plan.md
**Purpose:** Tracks project phases and progress

```markdown
# Task Plan: [Task Name]
Story: ACF-###

## Objective
[Clear statement of what we're trying to achieve]

## Phases

### Phase 1: Research
- [ ] Understand existing code
- [ ] Identify dependencies
- [ ] Document findings

### Phase 2: Design
- [ ] Create approach
- [ ] Identify risks
- [ ] Get approval

### Phase 3: Implementation
- [ ] Write failing tests
- [ ] Implement feature
- [ ] Pass all tests

### Phase 4: Validation
- [ ] Code review
- [ ] Integration tests
- [ ] Documentation

## Current Status
Phase: [1/2/3/4]
Blocker: [None / Description]
Next Action: [Specific next step]
```

### 2. findings.md
**Purpose:** Stores research results and discoveries

```markdown
# Findings: [Task Name]

## Code Analysis

### Relevant Files
- `src/Domain/Entities/Order.cs` - Main entity
- `src/Application/Commands/CreateOrderCommand.cs` - Handler

### Patterns Observed
- Factory methods used for entity creation
- Domain events raised on state changes

### Dependencies
- MediatR for CQRS
- FluentValidation for input validation

## Technical Notes
[Discoveries, gotchas, important context]

## Open Questions
- [ ] Question 1?
- [x] Question 2? → Answer found: [answer]
```

### 3. progress.md
**Purpose:** Session log and test results

```markdown
# Progress Log: [Task Name]

## Session: 2025-01-23

### Completed
- [x] Read existing Order entity
- [x] Identified validation requirements
- [x] Created failing test for new field

### Test Results
```
dotnet test
Passed: 45
Failed: 1 (Expected - TDD red phase)
```

### Blockers Encountered
- None this session

### Next Session
- Implement OrderStatus enum
- Add status transition logic

---

## Session: 2025-01-22
[Previous session notes...]
```

## When to Use

- Complex multi-step tasks
- Tasks spanning multiple sessions
- When you need to hand off to another developer
- Features requiring research before implementation
- Debugging complex issues

## Workflow

### Starting a Task
1. Create the 3 files in `artifacts/planning/[task-name]/`
2. Fill in task_plan.md with phases
3. Begin research, document in findings.md

### During Work
1. Update progress.md at start and end of each session
2. Check off items in task_plan.md as completed
3. Add new discoveries to findings.md

### Before Tool Use (Mental Check)
- Have I re-read the plan?
- Am I still on track?
- Should I update findings?

### After File Writes
- Update progress.md
- Mark items complete in task_plan.md

### Before Stopping
- Is the task complete? If not:
  - Document current state
  - Note next actions
  - Update blockers

## Directory Structure

```
artifacts/
└── planning/
    └── acf-042-add-order-status/
        ├── task_plan.md
        ├── findings.md
        └── progress.md
```

## Integration with AI Coding Factory

### Story Linkage
- Always include Story ID in task_plan.md header
- Link to story file: `artifacts/stories/ACF-###.md`

### Traceability
- Planning files become part of audit trail
- Reference in PR descriptions
- Archive after task completion

### Templates
Use the planning templates in `.claude/templates/planning/`:
- `task_plan.md.template`
- `findings.md.template`
- `progress.md.template`
