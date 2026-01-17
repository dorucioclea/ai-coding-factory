# /new-story - Create a New User Story

Create a new INVEST-compliant user story with proper traceability setup.

## Usage
```
/new-story <story-title>
```

## Instructions

When invoked, perform the following steps:

1. **Determine the next story ID**:
   - List existing stories in `artifacts/stories/`
   - Find the highest ACF-### number and increment by 1
   - If no stories exist, start with ACF-001

2. **Gather story details** by asking:
   - Who is the user persona? (e.g., "As a registered user")
   - What action do they want? (e.g., "I want to log in")
   - What is the business value? (e.g., "So that I can access my dashboard")

3. **Create the story file** at `artifacts/stories/ACF-###.md` using this template:

```markdown
---
id: ACF-###
title: <Story Title>
status: draft
created: <YYYY-MM-DD>
author: Claude Code
sprint: backlog
points: <estimate after discussion>
---

# ACF-### - <Story Title>

## User Story
As a <persona>,
I want to <action>,
So that <benefit>.

## Acceptance Criteria
- [ ] Given <precondition>, when <action>, then <expected result>
- [ ] Given <precondition>, when <action>, then <expected result>
- [ ] Given <precondition>, when <action>, then <expected result>

## Technical Notes
<Implementation considerations, dependencies, or constraints>

## Definition of Ready
- [ ] Story follows INVEST criteria
- [ ] Acceptance criteria are testable
- [ ] Dependencies identified
- [ ] No blocking questions

## Definition of Done
- [ ] All acceptance criteria met
- [ ] Unit tests with >80% coverage
- [ ] Integration tests for API endpoints
- [ ] Code reviewed
- [ ] Documentation updated
- [ ] Security review (if applicable)
- [ ] Traceability verified (story ID in tests and commits)

## Test Cases
| ID | Description | Expected Result | Story Link |
|----|-------------|-----------------|------------|
| TC-001 | <test description> | <expected> | ACF-### |

## Related
- Epic: <link if applicable>
- ADR: <link if architectural decision needed>
- Dependencies: <list any blocking stories>
```

4. **Validate INVEST criteria**:
   - **I**ndependent: Can be developed separately
   - **N**egotiable: Details can be discussed
   - **V**aluable: Delivers user value
   - **E**stimable: Can be sized
   - **S**mall: Fits in a sprint
   - **T**estable: Has clear acceptance criteria

5. **Output**:
   - Confirm story file created with path
   - Display the story ID for reference
   - Remind to add story ID to related tests and commits

## Example

```
User: /new-story User Login
Claude: Creating story ACF-042...

I need a few details:
1. Who is the persona? (e.g., "registered user", "admin", "guest")
2. What specific action do they want to perform?
3. What business value does this provide?

[After gathering info, creates artifacts/stories/ACF-042.md]

Created: artifacts/stories/ACF-042.md
Story ID: ACF-042 - User Login

Remember to use this ID in:
- Test traits: [Trait("Story", "ACF-042")]
- Commits: ACF-042 Implement user login
```
