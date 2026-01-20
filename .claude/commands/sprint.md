# /sprint - Sprint Management

Manage sprint planning, tracking, and ceremonies with automated insights.

## Usage
```
/sprint <action> [args]
```

Actions:
- `plan` - Start sprint planning
- `daily` - Generate daily standup report (automated from git)
- `review` - Prepare sprint review with demo script
- `retro` - Run retrospective with automated metrics
- `status` - Show sprint status with burndown
- `velocity` - Calculate and trend team velocity
- `metrics` - Deep dive into sprint metrics

## Instructions

### Sprint Planning (`/sprint plan`)

#### 1. Automated Backlog Analysis

```bash
# Find all stories and their status
find artifacts/stories -name "ACF-*.md" -exec grep -l "Status: ready\|Status: draft" {} \;

# Extract story metadata
for story in artifacts/stories/ACF-*.md; do
  echo "---"
  grep -E "^# |^Status:|^Priority:|^Points:" "$story"
done
```

#### 2. Velocity-Based Capacity Calculation

```bash
# Calculate average velocity from past sprints
cat artifacts/sprints/sprint-*.md | grep "Completed Points:" | \
  awk -F': ' '{sum+=$2; count++} END {print "Average:", sum/count}'
```

#### 3. Create Sprint File

**Location**: `artifacts/sprints/sprint-<N>.md`

```markdown
# Sprint <N>

**Start Date**: {{START_DATE}}
**End Date**: {{END_DATE}}
**Goal**: {{SPRINT_GOAL}}

## Sprint Metrics
- **Team Capacity**: {{CAPACITY}} points
- **Committed Points**: {{COMMITTED}} points
- **Buffer**: {{BUFFER}}% for unplanned work

## Committed Stories

| Story | Title | Points | Assignee | Status |
|-------|-------|--------|----------|--------|
| ACF-040 | Feature A | 5 | Developer | Not Started |
| ACF-041 | Feature B | 3 | Developer | Not Started |
| ACF-042 | Bug Fix | 2 | Developer | Not Started |

**Total Committed**: {{TOTAL}} points

## Sprint Backlog

### ACF-040 - Feature A (5 points)

**Acceptance Criteria**: (from story)

**Tasks**:
- [ ] Design domain model (1 pt)
- [ ] Implement repository (1 pt)
- [ ] Add API endpoint (1 pt)
- [ ] Write unit tests (1 pt)
- [ ] Write integration tests (1 pt)

### ACF-041 - Feature B
...

## Risks and Dependencies

| Risk/Dependency | Impact | Mitigation |
|-----------------|--------|------------|
| {{RISK}} | {{IMPACT}} | {{MITIGATION}} |

## Definition of Done Checklist
- [ ] All committed stories complete
- [ ] All tests passing in CI
- [ ] Code coverage ≥80% for Domain/Application
- [ ] No high/critical security vulnerabilities
- [ ] Documentation updated
- [ ] Demo prepared and rehearsed
```

---

### Daily Standup (`/sprint daily`)

#### Automated Git Analysis

```bash
# Get yesterday's commits
git log --since="yesterday" --until="today" --oneline --all

# Extract story IDs from commits
git log --since="yesterday" --format="%s" | grep -oE "ACF-[0-9]+" | sort -u

# Get files changed per story
for story in $(git log --since="yesterday" --format="%s" | grep -oE "ACF-[0-9]+" | sort -u); do
  echo "=== $story ==="
  git log --since="yesterday" --grep="$story" --name-only --format=""
done

# Calculate lines changed
git diff --stat $(git log --since="yesterday" --format="%H" | tail -1)..HEAD
```

#### Generated Report

```markdown
# Daily Standup - {{DATE}}

## Yesterday's Activity (Auto-generated from Git)

### Commits by Story
| Story | Commits | Files Changed | Lines |
|-------|---------|---------------|-------|
| ACF-040 | 3 | 8 | +245/-32 |
| ACF-041 | 2 | 4 | +89/-12 |

### Detailed Commit Log
```
09:15 ACF-040 Implement Order aggregate root
10:30 ACF-040 Add OrderRepository with EF Core
14:22 ACF-040 Write unit tests for Order
15:45 ACF-041 Add CreateOrderCommand handler
17:00 ACF-041 Add validation for OrderDto
```

### Tests Status
- Unit Tests: 145 passing, 0 failing
- New Tests Added: 8
- Coverage Delta: +2.3%

## Today's Plan

Based on sprint backlog and completed tasks:

| Story | Remaining Tasks | Priority |
|-------|-----------------|----------|
| ACF-040 | Integration tests, docs | High |
| ACF-041 | API endpoint, tests | High |
| ACF-042 | Not started | Medium |

## Blockers Detected

### From Code Comments
```bash
# Auto-scan for TODO, FIXME, BLOCKED tags
grep -rn "TODO\|FIXME\|BLOCKED" src/ --include="*.cs" | head -10
```

### From Recent Commits
- None detected (or list issues)

## Sprint Progress

```
Day 3 of 10
━━━━━━━━━━░░░░░░░░░░ 30%

Stories: 1/3 complete (33%)
Points:  3/10 complete (30%)
Status:  ✅ On Track
```

## Burndown

```
Points │ ·
  10   │ █·
   8   │ ██·░
   6   │ ███·░░
   4   │     ·░░░
   2   │      ·░░
   0   └──────────
       1 2 3 4 5 6 7 8 9 10
       █ Actual  · Ideal  ░ Remaining
```
```

---

### Sprint Review (`/sprint review`)

#### Automated Demo Script Generation

```bash
# Get completed stories
grep -l "Status: complete\|Status: done" artifacts/stories/ACF-*.md

# Extract acceptance criteria for demo points
for story in $(grep -l "Status: complete" artifacts/stories/ACF-*.md); do
  echo "=== Demo: $(basename $story .md) ==="
  sed -n '/## Acceptance Criteria/,/## /p' "$story" | head -20
done
```

#### Generated Review Document

```markdown
# Sprint {{N}} Review

**Date**: {{DATE}}
**Sprint Goal**: {{GOAL}}
**Goal Status**: ✅ Achieved | ⚠️ Partially Achieved | ❌ Not Achieved

## Executive Summary

- **Committed**: {{COMMITTED}} points ({{STORY_COUNT}} stories)
- **Completed**: {{COMPLETED}} points ({{DONE_COUNT}} stories)
- **Velocity**: {{VELOCITY}} points
- **Completion Rate**: {{RATE}}%

## Completed Work

### ACF-040: Order Management System (5 points)

**Demo Script**:
1. Navigate to `/orders` page
2. Click "Create New Order"
3. Fill in customer and product details
4. Submit order - show success notification
5. View order in list with status "Pending"
6. Show order details with line items

**Key Features Delivered**:
- Order creation with validation
- Order listing with filtering
- Order detail view
- Real-time status updates

**Technical Highlights**:
- Clean Architecture implementation
- CQRS with MediatR
- 92% test coverage

### ACF-041: Email Notifications (3 points)
...

## Incomplete Work

| Story | Remaining | Reason | Next Sprint? |
|-------|-----------|--------|--------------|
| ACF-042 | 2 tasks | External API delay | Yes |

## Sprint Metrics

| Metric | Value | Trend |
|--------|-------|-------|
| Velocity | 8 pts | ↑ +2 |
| Completion Rate | 80% | ↓ -5% |
| Bug Escape Rate | 0 | = 0 |
| Test Coverage | 87% | ↑ +3% |
| Cycle Time | 2.5 days | ↓ -0.5 |

## Demo Environment

| Item | Status | URL/Details |
|------|--------|-------------|
| Application | ✅ Ready | https://staging.example.com |
| Test Data | ✅ Loaded | 50 sample orders |
| Accounts | ✅ Ready | demo@example.com / demo123 |

## Demo Checklist

- [ ] Environment verified working
- [ ] Test data refreshed
- [ ] Screen sharing tested
- [ ] Recording setup (optional)
- [ ] Stakeholder calendar confirmed
- [ ] Backup presenter assigned

## Feedback Capture Template

| Feedback | From | Priority | Action |
|----------|------|----------|--------|
| | | | |

## Stakeholder Sign-off

| Stakeholder | Approved | Notes |
|-------------|----------|-------|
| Product Owner | ☐ | |
| Tech Lead | ☐ | |
```

---

### Sprint Retrospective (`/sprint retro`)

#### Automated Metrics Analysis

```bash
# Commit frequency by day
git log --format="%ad" --date=short | sort | uniq -c | tail -10

# Code churn analysis
git log --numstat --format="" | awk '{add+=$1; del+=$2} END {print "Added:", add, "Deleted:", del}'

# Build failure rate (if CI logs available)
# Review rate
# PR turnaround time
```

#### Generated Retrospective

```markdown
# Sprint {{N}} Retrospective

**Date**: {{DATE}}
**Facilitator**: Claude Code
**Attendees**: {{ATTENDEES}}

## Sprint Health Score: {{SCORE}}/10

### Automated Health Metrics

| Metric | Value | Target | Status |
|--------|-------|--------|--------|
| Velocity | 8 pts | 10 pts | ⚠️ Below |
| Completion Rate | 80% | 90% | ⚠️ Below |
| Test Coverage | 87% | 80% | ✅ Above |
| Build Success | 95% | 95% | ✅ Met |
| PR Review Time | 4 hrs | 8 hrs | ✅ Above |
| Bug Escape | 0 | 0 | ✅ Met |

### Code Quality Trends

```
Coverage: ████████████████████░░░░ 87%
                            ↑ +3% from last sprint

Tech Debt: ██████░░░░░░░░░░░░░░░░░░ 25%
                 ↓ -5% from last sprint
```

## What Went Well ✅

### Team Observations
- {{ITEM_1}}
- {{ITEM_2}}

### Data-Backed Wins
- PR review time improved by 50%
- Zero production incidents
- Test coverage increased 3%
- All code reviews completed same-day

## What Could Be Improved ⚠️

### Team Observations
- {{ITEM_1}}
- {{ITEM_2}}

### Data-Backed Concerns
- Velocity 20% below target
- 1 story carried over
- Late sprint scope addition
- Commit clustering on final day

### Commit Distribution Analysis

```
Mon  ████████░░░░░░░░ 20%
Tue  ██████████████░░ 35%
Wed  ████████░░░░░░░░ 20%
Thu  ████░░░░░░░░░░░░ 10%
Fri  ██████░░░░░░░░░░ 15%

⚠️ Uneven distribution - consider WIP limits
```

## Action Items

| Action | Owner | Due | Tracking |
|--------|-------|-----|----------|
| Implement WIP limits | Scrum Master | Sprint {{N+1}} | |
| Add integration test stage | DevOps | {{DATE}} | |
| Story refinement session | PO + Team | Weekly | |

## Previous Sprint Actions Status

| Action | Owner | Status | Notes |
|--------|-------|--------|-------|
| Add pre-commit hooks | Dev | ✅ Done | Reduced lint issues 90% |
| Automate deployments | DevOps | ✅ Done | CI/CD now fully automated |
| Story pointing session | Team | ⚠️ Partial | 2 of 3 sessions held |

## Velocity Trend

```
Sprint │
   1   │ ██████░░░░░░░░░░ 6 pts
   2   │ ████████░░░░░░░░ 8 pts
   3   │ ████████░░░░░░░░ 8 pts
   4   │ ██████████░░░░░░ 10 pts
   5   │ ████████░░░░░░░░ 8 pts (current)
       └─────────────────────────
       Average: 8 pts │ Target: 10 pts
```

## Team Happiness Index

Rate 1-5: How was this sprint?
- Collaboration: {{RATING}}
- Workload: {{RATING}}
- Technical challenges: {{RATING}}
- Support from leadership: {{RATING}}

## Next Sprint Focus

Based on this retrospective:
1. {{FOCUS_1}}
2. {{FOCUS_2}}
3. {{FOCUS_3}}
```

---

### Sprint Status (`/sprint status`)

Real-time sprint status with burndown visualization.

```markdown
# Sprint {{N}} Status

**Updated**: {{TIMESTAMP}}
**Days Remaining**: {{DAYS}} of {{TOTAL_DAYS}}

## Goal Progress

**Goal**: {{SPRINT_GOAL}}
**Status**: ✅ On Track | ⚠️ At Risk | ❌ Off Track

## Burndown Chart

```
Points
  12 │ ·
  10 │ █·
   8 │ ██·░
   6 │ ███·░░
   4 │ ████·░░░
   2 │      ·░░░░
   0 └──────────────────
     │ 1  2  3  4  5  6  7  8  9  10
     Day

Legend: █ Completed  · Ideal  ░ Remaining
Current: 6 pts done │ Ideal: 4.8 pts │ Δ: +1.2 pts ahead
```

## Story Status

| Story | Title | Points | Progress | Status |
|-------|-------|--------|----------|--------|
| ACF-040 | Order Management | 5 | ████████░░ 80% | 🔵 In Progress |
| ACF-041 | Notifications | 3 | ██████████ 100% | ✅ Complete |
| ACF-042 | Dashboard | 2 | ░░░░░░░░░░ 0% | ⚪ Not Started |

## Task Breakdown

### ACF-040 - Order Management (4/5 tasks)
- [x] Design domain model
- [x] Implement repository
- [x] Add API endpoint
- [x] Write unit tests
- [ ] Write integration tests ← In Progress

### ACF-042 - Dashboard (0/4 tasks)
- [ ] Design wireframe
- [ ] Create React components
- [ ] Implement data fetching
- [ ] Write tests

## Blockers

| Blocker | Story | Raised | Owner | Status |
|---------|-------|--------|-------|--------|
| External API unavailable | ACF-042 | Day 3 | Tech Lead | 🔴 Active |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| ACF-042 may not complete | High | Medium | Move to next sprint |

## Today's Git Activity

```bash
# Commits today
3 commits by team
├── ACF-040: Add integration test setup
├── ACF-040: Fix order validation bug
└── ACF-041: Update email template
```

## Sprint Health

| Indicator | Status |
|-----------|--------|
| Burndown | ✅ Ahead of plan |
| Scope | ✅ Stable (no changes) |
| Quality | ✅ All tests passing |
| Blockers | ⚠️ 1 active blocker |
```

---

### Velocity (`/sprint velocity`)

Historical velocity analysis with predictions.

```markdown
# Team Velocity Analysis

**Generated**: {{DATE}}
**Analysis Period**: Last 6 sprints

## Velocity History

| Sprint | Planned | Committed | Completed | Velocity |
|--------|---------|-----------|-----------|----------|
| Sprint 6 | 12 | 10 | 10 | 10 |
| Sprint 5 | 10 | 10 | 8 | 8 |
| Sprint 4 | 12 | 12 | 10 | 10 |
| Sprint 3 | 10 | 8 | 8 | 8 |
| Sprint 2 | 8 | 8 | 6 | 6 |
| Sprint 1 | 10 | 10 | 6 | 6 |

## Velocity Chart

```
Points
  12 │          ┌───┐
  10 │      ┌───┤   ├───┬───┐
   8 │  ┌───┤   │   │   │   │
   6 │──┴───┴───┴───┴───┴───┴───
     │  S1  S2  S3  S4  S5  S6

     ═══ Committed  ─── Completed
```

## Statistical Analysis

| Metric | Value |
|--------|-------|
| **Average Velocity** | 8.0 points |
| **Median Velocity** | 8.0 points |
| **Standard Deviation** | 1.6 points |
| **Min Velocity** | 6 points |
| **Max Velocity** | 10 points |
| **Trend** | ↑ Improving |

## Commitment Accuracy

```
Sprint 6: ██████████ 100% (10/10)
Sprint 5: ████████░░ 80% (8/10)
Sprint 4: ████████░░ 83% (10/12)
Sprint 3: ██████████ 100% (8/8)
Sprint 2: ████████░░ 75% (6/8)
Sprint 1: ██████░░░░ 60% (6/10)

Average Accuracy: 83%
```

## Recommendations

### For Next Sprint Planning

Based on velocity analysis:

| Scenario | Recommended Points | Confidence |
|----------|-------------------|------------|
| Conservative | 6 points | 95% |
| Normal | 8 points | 80% |
| Optimistic | 10 points | 60% |

**Suggestion**: Commit to **8 points** with 1-2 stretch goals

### Improvement Opportunities

1. **Reduce variability**: Current σ=1.6 → Target σ<1.0
2. **Improve accuracy**: Invest in story refinement
3. **Sustain trend**: Velocity improving +33% over 6 sprints

## Factors Affecting Velocity

| Factor | Impact | Sprint(s) |
|--------|--------|-----------|
| Holiday | -2 pts | Sprint 2 |
| New team member | -2 pts | Sprint 1-2 |
| Technical debt sprint | -3 pts | Sprint 3 |
| Normal capacity | Baseline | Sprint 4-6 |
```

---

### Metrics Deep Dive (`/sprint metrics`)

Comprehensive sprint metrics analysis.

```markdown
# Sprint Metrics Dashboard

**Sprint**: {{N}}
**Period**: {{START}} to {{END}}

## Delivery Metrics

| Metric | Value | Target | Status |
|--------|-------|--------|--------|
| Velocity | 8 pts | 10 pts | ⚠️ 80% |
| Throughput | 3 stories | 4 stories | ⚠️ 75% |
| Cycle Time | 2.5 days | 3 days | ✅ Better |
| Lead Time | 5 days | 7 days | ✅ Better |

## Quality Metrics

| Metric | Value | Target | Status |
|--------|-------|--------|--------|
| Test Coverage | 87% | 80% | ✅ Exceeded |
| Bug Escape Rate | 0 | 0 | ✅ Met |
| Code Review Coverage | 100% | 100% | ✅ Met |
| Build Success Rate | 95% | 95% | ✅ Met |

## Process Metrics

| Metric | Value | Target | Status |
|--------|-------|--------|--------|
| Sprint Goal Met | Yes | Yes | ✅ |
| Scope Stability | 100% | 90% | ✅ |
| Unplanned Work | 5% | <10% | ✅ |
| Blocked Time | 8 hrs | <16 hrs | ✅ |

## Trend Charts

### Velocity Trend
```
  10 │      ●──────●
   8 │  ●───●
   6 │──●
     └──────────────
       S1 S2 S3 S4 S5
```

### Quality Trend
```
 100│  ●───●───●───●
  80│
  60│
     └──────────────
       S1 S2 S3 S4 S5
       Coverage %
```

## Actionable Insights

1. **Velocity Gap**: 2 points below target
   - Root cause: 1 blocked story (8 hrs)
   - Action: Improve dependency management

2. **Cycle Time Improved**: Down from 3.2 to 2.5 days
   - Contributing factor: Better story refinement
   - Action: Continue refinement practices

3. **Coverage Stable**: Maintaining 87%
   - Action: Focus on critical path coverage
```

## Example

```
User: /sprint daily

Claude: Generating daily standup report from git activity...

## Yesterday's Activity (Auto-generated)

Analyzed 5 commits from yesterday:

| Story | Commits | Activity |
|-------|---------|----------|
| ACF-040 | 3 | Domain model, repository, tests |
| ACF-041 | 2 | Command handler, validation |

## Sprint Progress

Day 4 of 10 | 3/10 points complete | ✅ On Track

Burndown shows we're 0.5 points ahead of ideal.

## Today's Suggested Focus

Based on sprint backlog:
1. ACF-040: Integration tests (final task)
2. ACF-041: API endpoint implementation
3. ACF-042: Can start after ACF-040 complete

Any blockers to discuss?
```
