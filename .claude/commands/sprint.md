# /sprint - Sprint Management

Manage sprint planning, tracking, and ceremonies.

## Usage
```
/sprint <action> [args]
```

Actions:
- `plan` - Start sprint planning
- `daily` - Generate daily standup report
- `review` - Prepare sprint review
- `retro` - Run sprint retrospective
- `status` - Show sprint status
- `velocity` - Calculate team velocity

## Instructions

### Sprint Planning (`/sprint plan`)

1. **Review backlog**:
   - List stories in `artifacts/stories/` with status: draft or ready
   - Sort by priority (from Product Owner)

2. **Determine capacity**:
   - Default sprint length: 2 weeks
   - Calculate available story points

3. **Create sprint file** (`artifacts/sprints/sprint-<N>.md`):

```markdown
# Sprint <N>

**Start Date**: <YYYY-MM-DD>
**End Date**: <YYYY-MM-DD>
**Goal**: <Sprint goal statement>

## Committed Stories

| Story | Title | Points | Assignee | Status |
|-------|-------|--------|----------|--------|
| ACF-040 | Feature A | 5 | Developer | Not Started |
| ACF-041 | Feature B | 3 | Developer | Not Started |
| ACF-042 | Bug Fix | 2 | Developer | Not Started |

**Total Points**: 10
**Team Capacity**: 12

## Sprint Backlog

### ACF-040 - Feature A
**Tasks**:
- [ ] Design domain model
- [ ] Implement repository
- [ ] Add API endpoint
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Update documentation

### ACF-041 - Feature B
...

## Risks and Dependencies
- <Risk 1>
- <Dependency 1>

## Definition of Done (Sprint Level)
- [ ] All committed stories complete
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Demo prepared
```

### Daily Standup (`/sprint daily`)

Generate report from git activity:

```markdown
# Daily Standup - <YYYY-MM-DD>

## Yesterday's Progress
<Parse recent commits and file changes>

| Story | Activity | Status |
|-------|----------|--------|
| ACF-040 | Implemented domain model | In Progress |
| ACF-041 | Added unit tests | Complete |

## Today's Plan
<Based on sprint backlog and remaining tasks>

## Blockers
<Any identified blockers from code or comments>

## Sprint Progress
- Stories Complete: X/Y
- Points Complete: X/Y
- Days Remaining: X
```

### Sprint Review (`/sprint review`)

Prepare for sprint review:

```markdown
# Sprint <N> Review

**Sprint Goal**: <goal>
**Status**: Achieved | Partially Achieved | Not Achieved

## Completed Stories

| Story | Title | Demo Notes |
|-------|-------|------------|
| ACF-040 | Feature A | Show user flow |
| ACF-041 | Feature B | Demo API endpoint |

## Incomplete Stories

| Story | Title | Remaining | Reason |
|-------|-------|-----------|--------|
| ACF-042 | Feature C | 2 tasks | Blocked by dependency |

## Metrics
- Planned Points: X
- Completed Points: Y
- Velocity: Y points

## Demo Checklist
- [ ] Environment ready
- [ ] Test data prepared
- [ ] Stakeholders invited
- [ ] Recording setup (if needed)

## Feedback Captured
<To be filled during review>
```

### Sprint Retrospective (`/sprint retro`)

```markdown
# Sprint <N> Retrospective

**Date**: <YYYY-MM-DD>
**Facilitator**: Claude Code

## What Went Well
- <Item 1>
- <Item 2>

## What Could Be Improved
- <Item 1>
- <Item 2>

## Action Items

| Action | Owner | Due Date |
|--------|-------|----------|
| <Action 1> | <Owner> | <Date> |

## Metrics Review
- Velocity trend: X → Y
- Bug escape rate: X
- Test coverage: X%
- Cycle time: X days

## Previous Action Items Status
| Action | Status |
|--------|--------|
| <Previous action> | Done/In Progress/Dropped |
```

### Sprint Status (`/sprint status`)

```markdown
# Sprint <N> Status

**Days Remaining**: X
**Goal Progress**: On Track | At Risk | Off Track

## Burndown

```
Points │
  10   │▓▓
   8   │▓▓▓▓░░
   6   │▓▓▓▓▓▓░░░░
   4   │▓▓▓▓▓▓▓▓░░░░
   2   │▓▓▓▓▓▓▓▓▓▓░░
   0   └──────────────
       Day 1  2  3  4  5
       ▓ Actual  ░ Ideal
```

## Story Status

| Story | Status | Progress |
|-------|--------|----------|
| ACF-040 | In Progress | 60% |
| ACF-041 | Complete | 100% |
| ACF-042 | Not Started | 0% |

## Blockers
- <Current blockers>

## Risks
- <At-risk items>
```

### Velocity (`/sprint velocity`)

```markdown
# Team Velocity

## Last 5 Sprints

| Sprint | Planned | Completed | Velocity |
|--------|---------|-----------|----------|
| Sprint 5 | 12 | 10 | 10 |
| Sprint 4 | 10 | 10 | 10 |
| Sprint 3 | 12 | 8 | 8 |
| Sprint 2 | 8 | 8 | 8 |
| Sprint 1 | 10 | 6 | 6 |

**Average Velocity**: 8.4 points
**Recommended Next Sprint**: 8-10 points

## Trend Analysis
- Velocity improving: Yes
- Consistency: Medium
- Recommendation: <planning recommendation>
```

## Example

```
User: /sprint plan

Claude: Starting sprint planning...

## Backlog Review

Stories ready for sprint:
| Story | Title | Priority | Points |
|-------|-------|----------|--------|
| ACF-043 | Order Management | High | 5 |
| ACF-044 | Email Notifications | High | 3 |
| ACF-045 | Report Dashboard | Medium | 8 |
| ACF-046 | User Preferences | Low | 2 |

## Capacity
- Sprint length: 2 weeks
- Average velocity: 10 points
- Recommended capacity: 10 points

## Suggested Commitment
| Story | Points | Cumulative |
|-------|--------|------------|
| ACF-043 | 5 | 5 |
| ACF-044 | 3 | 8 |
| ACF-046 | 2 | 10 |

Total: 10 points (matches velocity)

Should I create the sprint file with these stories?
```
