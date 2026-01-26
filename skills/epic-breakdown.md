# Epic Breakdown

## Overview

Break down large projects into manageable, hierarchical pieces that can be planned, tracked, and delivered incrementally.

## Hierarchy

```
Initiative (optional)
    └── Epic
          └── Feature
                └── User Story
                      └── Task
```

| Level | Size | Duration | Example |
|-------|------|----------|---------|
| **Initiative** | Multiple epics | Quarters | "Mobile Experience" |
| **Epic** | Large body of work | 1-3 months | "User Authentication" |
| **Feature** | Deliverable capability | 1-4 weeks | "Social Login" |
| **User Story** | Single user value | 1-5 days | "Login with Google" |
| **Task** | Technical work item | Hours | "Implement OAuth callback" |

## When to Use

- Starting a new product/project
- Planning a major release
- Breaking down a large feature request
- Creating a roadmap
- Organizing backlog

## Epic Template

```markdown
# Epic: {EPIC_NAME}

**ID**: EPIC-{XXX}
**Owner**: {Product Owner}
**Status**: Draft | Planning | In Progress | Done
**Target**: {Quarter/Release}

## Vision

{2-3 sentences: What does success look like when this epic is complete?}

## Problem Statement

{What user/business problem does this solve?}

## Success Metrics

| Metric | Current | Target |
|--------|---------|--------|
| {KPI 1} | {baseline} | {goal} |
| {KPI 2} | {baseline} | {goal} |

## Features

| ID | Feature | Priority | Status |
|----|---------|----------|--------|
| F-001 | {Feature 1} | P1 | Draft |
| F-002 | {Feature 2} | P1 | Draft |
| F-003 | {Feature 3} | P2 | Draft |

## Dependencies

- {Dependency 1}
- {Dependency 2}

## Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| {Risk 1} | High/Med/Low | {Strategy} |

## Out of Scope

- {Explicitly excluded}
```

## Feature Template

```markdown
# Feature: {FEATURE_NAME}

**ID**: F-{XXX}
**Epic**: EPIC-{XXX} - {Epic Name}
**Owner**: {Owner}
**Status**: Draft | Ready | In Progress | Done

## Description

{What does this feature do? Why does the user need it?}

## User Stories

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-001 | As a {user}, I want to {action} | {1-13} | Draft |
| US-002 | As a {user}, I want to {action} | {1-13} | Draft |
| US-003 | As a {user}, I want to {action} | {1-13} | Draft |

## Acceptance Criteria (Feature Level)

- [ ] {Criterion that applies to whole feature}
- [ ] {Criterion that applies to whole feature}

## Technical Notes

{Any architecture considerations, tech debt, or implementation hints}

## Dependencies

- Depends on: {other features/stories}
- Blocks: {features/stories that need this}
```

## Breakdown Process

### Step 1: Identify Epics

Start with the big picture. What are the major areas of work?

**Heuristics for Epic boundaries**:
- Different user personas
- Different parts of the system
- Different business capabilities
- Can be released independently

**Common Epic patterns**:
```
Product Epics:
├── User Management (auth, profiles, permissions)
├── Core Domain (main business functionality)
├── Admin/Back-office (management tools)
├── Integrations (external systems)
├── Reporting/Analytics (dashboards, exports)
└── Platform (infrastructure, DevOps)
```

### Step 2: Break Epics into Features

Each epic contains 3-7 features.

**Feature characteristics**:
- Delivers specific user value
- Can be demo'd when complete
- Has clear boundaries
- Testable end-to-end

**Example**:
```
Epic: User Authentication
├── Feature: Email/Password Auth
├── Feature: Social Login (Google, GitHub)
├── Feature: Password Reset
├── Feature: Two-Factor Authentication
└── Feature: Session Management
```

### Step 3: Break Features into User Stories

Each feature contains 3-10 user stories.

**Story characteristics**:
- Follows "As a... I want... So that..."
- Independent (can be built alone)
- Negotiable (details can change)
- Valuable (delivers user value)
- Estimable (can size it)
- Small (fits in a sprint)
- Testable (has clear acceptance criteria)

**Example**:
```
Feature: Email/Password Auth
├── US-001: User can register with email/password
├── US-002: User can log in with credentials
├── US-003: User sees validation errors on invalid input
├── US-004: User is redirected after successful login
└── US-005: User can log out
```

### Step 4: Break Stories into Tasks

Each story contains 2-8 tasks.

**Task characteristics**:
- Technical work item
- Assigned to one person
- Completable in hours
- Has clear "done" state

**Example**:
```
US-001: User can register with email/password
├── T1: Create User entity and migration
├── T2: Create RegisterCommand and handler
├── T3: Create RegisterEndpoint
├── T4: Write integration tests
├── T5: Create RegisterForm component
├── T6: Connect form to API
└── T7: Add E2E test
```

## Story Point Reference

Use Fibonacci for relative sizing:

| Points | Meaning | Example |
|--------|---------|---------|
| 1 | Trivial | Fix typo, update config |
| 2 | Small | Add field to existing form |
| 3 | Medium-small | New simple endpoint |
| 5 | Medium | New feature with tests |
| 8 | Large | Complex feature, multiple components |
| 13 | Very large | Consider splitting |
| 21+ | Epic-sized | Must split |

## Visualization

### Story Map Format

```
                        User Journey →
    ┌─────────────┬─────────────┬─────────────┬─────────────┐
    │   Browse    │   Select    │   Purchase  │   Review    │
    ├─────────────┼─────────────┼─────────────┼─────────────┤
MVP │ View list   │ See details │ Add to cart │ Rate item   │
    │ Search      │ View photos │ Checkout    │             │
    ├─────────────┼─────────────┼─────────────┼─────────────┤
v2  │ Filter      │ Compare     │ Save cart   │ Write review│
    │ Sort        │ Wishlist    │ Gift option │ Share       │
    └─────────────┴─────────────┴─────────────┴─────────────┘
```

### Dependency Graph

```
     ┌──────────────┐
     │   US-001     │
     │  Register    │
     └──────┬───────┘
            │
     ┌──────▼───────┐
     │   US-002     │
     │    Login     │
     └──────┬───────┘
            │
    ┌───────┴───────┐
    │               │
┌───▼────┐    ┌────▼────┐
│ US-003 │    │ US-004  │
│ Profile│    │Dashboard│
└────────┘    └─────────┘
```

## Output Structure

After breakdown, create:

```
specs/{project}/
├── epics/
│   ├── EPIC-001-user-management.md
│   ├── EPIC-002-core-domain.md
│   └── EPIC-003-admin.md
├── features/
│   ├── F-001-email-auth.md
│   ├── F-002-social-login.md
│   └── ...
├── stories/
│   ├── US-001-register.md
│   ├── US-002-login.md
│   └── ...
└── roadmap.md
```

## Roadmap Template

```markdown
# Product Roadmap

## Current Quarter: Q1 2024

### Sprint 1-2: Foundation
- [ ] EPIC-001: User Management (MVP)
  - [x] F-001: Email Auth
  - [ ] F-002: Password Reset

### Sprint 3-4: Core Features
- [ ] EPIC-002: Core Domain (MVP)
  - [ ] F-003: Create Items
  - [ ] F-004: Browse Items

## Next Quarter: Q2 2024

### Sprint 5-6: Enhancement
- [ ] EPIC-001: User Management (v2)
  - [ ] F-005: Social Login
  - [ ] F-006: 2FA

## Backlog (Unprioritized)
- EPIC-003: Admin Dashboard
- EPIC-004: Analytics
```

## Quick Reference

### Commands

```bash
# Start epic breakdown
"Let's break down this project into epics"

# Create specific epic
"Create an epic for user authentication"

# Break feature into stories
"Break the social login feature into user stories"
```

### Common Mistakes

| Mistake | Fix |
|---------|-----|
| Epics too small | Combine related features |
| Epics too big | Split by user persona or capability |
| Stories have tasks in name | Focus on user value, not implementation |
| Stories aren't independent | Identify and document dependencies |
| No acceptance criteria | Add Given/When/Then for each story |