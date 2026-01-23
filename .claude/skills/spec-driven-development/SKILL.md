---
name: spec-driven-development
description: Use when starting a new project from an idea or vague requirements. Guides through systematic requirement gathering, specification creation, and user questions before any implementation.
---

# Spec-Driven Development

## Overview

Transform vague ideas into actionable specifications through systematic questioning. **Never write code until requirements are crystal clear.**

Core principle: The cost of unclear requirements grows exponentially - 5 minutes of questions saves 5 hours of rework.

## When to Use

- User says "I want to build..." or "Create a..."
- Vague feature requests without clear acceptance criteria
- New project kickoff
- Major feature additions

## The Process

```
┌─────────────────────────────────────────────────────┐
│                 IDEA RECEIVED                        │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  PHASE 1: DOMAIN DISCOVERY                          │
│  - What problem does this solve?                    │
│  - Who are the users?                               │
│  - What's the core value proposition?               │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  PHASE 2: FEATURE SCOPING                           │
│  - What are the must-have features (MVP)?           │
│  - What's explicitly out of scope?                  │
│  - What are nice-to-haves for later?                │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  PHASE 3: TECHNICAL REQUIREMENTS                    │
│  - Authentication needs?                            │
│  - Data persistence requirements?                   │
│  - Integration with external services?              │
│  - Performance/scale requirements?                  │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  PHASE 4: SPECIFICATION OUTPUT                      │
│  - Write specification document                     │
│  - Get user approval                                │
│  - Create epic and stories                          │
└─────────────────────────────────────────────────────┘
```

## Question Framework

### Phase 1: Domain Discovery (Ask First)

**Core Questions:**
1. "What problem are you trying to solve?" (pain point)
2. "Who will use this?" (personas)
3. "What does success look like?" (metrics)
4. "Are there existing solutions? What's wrong with them?" (differentiation)

**Present as multiple choice when possible:**
```
Who is the primary user?
  1. End consumers (B2C)
  2. Business users (B2B)
  3. Internal team members
  4. Other: ___
```

### Phase 2: Feature Scoping

**MVP Questions:**
1. "If you could only have 3 features, what would they be?"
2. "What's the simplest version that would be useful?"
3. "What features can wait until v2?"

**Scope Boundaries:**
1. "What should this explicitly NOT do?"
2. "Are there integrations we should avoid for now?"

### Phase 3: Technical Requirements

**Stack Questions:**
```
For the backend, what fits your needs?
  1. .NET 8 Clean Architecture (Recommended) - Enterprise-grade, full testing
  2. Microservice - Lightweight, Kubernetes-ready
  3. I have existing backend
```

```
For the frontend:
  1. Next.js + React (Recommended) - Full-featured, SSR, auth ready
  2. SPA only - Simpler, no server rendering
  3. No frontend needed - API only
```

**Data Questions:**
1. "What data needs to persist?"
2. "Any sensitive data requiring encryption?"
3. "Expected data volume?"

**Auth Questions:**
```
Authentication requirements?
  1. Email/password only
  2. Social login (Google, GitHub, etc.)
  3. Enterprise SSO (SAML, OIDC)
  4. No auth needed (public)
```

## Output: Specification Document

After questions, generate `docs/specs/YYYY-MM-DD-{project}-spec.md`:

```markdown
# Project Specification: {Project Name}

## Problem Statement
{What problem this solves}

## Users & Personas
| Persona | Description | Key Needs |
|---------|-------------|-----------|
| {name} | {description} | {needs} |

## Core Features (MVP)
1. {Feature 1} - {Description}
2. {Feature 2} - {Description}
3. {Feature 3} - {Description}

## Out of Scope (v1)
- {Feature X} - Planned for v2
- {Feature Y} - Not in roadmap

## Technical Decisions
- **Backend**: {choice + rationale}
- **Frontend**: {choice + rationale}
- **Database**: {choice + rationale}
- **Authentication**: {choice + rationale}

## Non-Functional Requirements
- **Performance**: {targets}
- **Security**: {requirements}
- **Scalability**: {expectations}

## Success Metrics
1. {Metric 1}
2. {Metric 2}

## Epics & Stories
### Epic 1: {Name}
- Story 1.1: {Title}
- Story 1.2: {Title}

### Epic 2: {Name}
- Story 2.1: {Title}

## Open Questions
- {Any unresolved items}

---
Approved by: {user confirmation}
Date: {date}
```

## Integration with Templates

After specification approval:

1. **Backend**: Use `/scaffold {ProjectName} clean-architecture`
2. **Frontend**: Use `/scaffold {ProjectName} react-frontend`
3. **Full Stack**: Use `/scaffold-fullstack {ProjectName}`

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Jumping to code | Complete all 4 phases first |
| Asking too many questions at once | One question per message |
| Open-ended only | Prefer multiple choice |
| Skipping "out of scope" | Explicitly define boundaries |
| No written spec | Always create spec document |

## Red Flags - STOP

- User says "just build something" → Clarify minimum requirements
- Scope keeps growing → Define MVP boundaries
- No clear user/persona → Cannot proceed without target user
- "I'll know it when I see it" → Need concrete acceptance criteria
