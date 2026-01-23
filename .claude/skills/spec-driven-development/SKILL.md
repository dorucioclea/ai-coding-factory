---
name: spec-driven-development
description: Use when starting a new project from an idea or vague requirements. Guides through systematic requirement gathering, specification creation, and user questions before any implementation.
autoTrigger: true
triggers:
  - "I want to build"
  - "build me a"
  - "create an app"
  - "I need a website"
  - "make me a"
  - "I have an idea"
  - "let's build"
  - "help me create"
---

# Spec-Driven Development

## Overview

Transform vague ideas into actionable specifications through **automated, systematic questioning**. This skill triggers automatically when a project idea is detected.

**Core Principles:**
- Never ask "should I use spec-driven development?" - just do it
- Never ask "should I use the templates?" - always use them
- Maximum 3 clarification questions per phase
- Present questions with suggested answers in table format
- Make reasonable assumptions for anything not critical

## Auto-Trigger Behavior

When detecting any project idea, immediately:
1. Acknowledge the idea (1 sentence)
2. State you'll gather requirements (1 sentence)
3. Present first batch of questions (max 3)

**Example opener:**
> "A fishing spots sharing platform - great idea! Let me ask a few quick questions to nail down the requirements."

## Question Format (MANDATORY)

Always present questions in this table format with recommendations:

```markdown
## Question 1: [Topic]

**Context**: [Why this matters for the build]

| Option | Description | Implications |
|--------|-------------|--------------|
| A (Recommended) | [Best option] | [Why it's recommended] |
| B | [Alternative] | [Trade-offs] |
| C | [Another option] | [Trade-offs] |
| Custom | Your own answer | Describe what you need |

## Question 2: [Topic]
...

## Question 3: [Topic]
...

**Reply with your choices (e.g., "1: A, 2: B, 3: Custom - I want X")**
```

## The 3-Question Phases

### Phase 1: Core Understanding (3 questions max)

Pick the 3 most important from:
- **Users**: Who will use this? (B2C, B2B, internal)
- **Problem**: What's the main pain point being solved?
- **MVP Scope**: What are the 3 must-have features?
- **Success**: How will you measure success?

**Make assumptions for:**
- Authentication method (default: email/password)
- Data storage (default: PostgreSQL)
- Hosting (default: containerized)

### Phase 2: Feature Specifics (3 questions max)

Based on Phase 1 answers, ask about:
- Specific feature behaviors
- User permissions/roles
- Data relationships
- Integration requirements

**Make assumptions for:**
- Standard CRUD operations
- Basic validation rules
- Common UI patterns

### Phase 3: Technical Confirmation (usually 0-2 questions)

Only ask if:
- Unusual scale requirements mentioned
- Specific technology preferences hinted
- Complex integrations needed

**Default assumptions (don't ask):**
- Backend: .NET 8 Clean Architecture template
- Frontend: Next.js 14 React template
- Database: PostgreSQL
- Cache: Redis
- Auth: JWT with refresh tokens
- API: RESTful

## Specification Output

After questions, generate specification in this format:

```markdown
# Project Specification: {Project Name}

**Created**: {date}
**Status**: Draft → Approved

## Problem Statement
{1-2 sentences on the problem being solved}

## Target Users

| Persona | Description | Key Needs |
|---------|-------------|-----------|
| {type} | {description} | {needs} |

## User Stories (Prioritized)

### P1 - Must Have (MVP)
- **US-001**: As a {user}, I want to {action} so that {benefit}
  - Acceptance: Given {context}, When {action}, Then {result}
- **US-002**: ...
- **US-003**: ...

### P2 - Should Have
- **US-004**: ...

### P3 - Nice to Have
- **US-005**: ...

## Out of Scope (v1)
- {Feature X} - Planned for v2
- {Feature Y} - Not in roadmap

## Technical Decisions
| Decision | Choice | Rationale |
|----------|--------|-----------|
| Backend | .NET 8 Clean Architecture | Enterprise-grade, testable |
| Frontend | Next.js 14 + React | SSR, auth integration |
| Database | PostgreSQL | Relational, proven |
| Auth | JWT + Refresh Tokens | Stateless, scalable |

## Key Entities
- **{Entity1}**: {description, key fields}
- **{Entity2}**: {description, relationships}

## Success Criteria
- [ ] SC-001: {Measurable outcome}
- [ ] SC-002: {Measurable outcome}

## Assumptions Made
- {List any assumptions made during specification}

---
**Awaiting user approval before implementation**
```

## Post-Specification Flow

After user approves spec:

1. **Scaffold Projects**
   ```bash
   /scaffold {ProjectName} clean-architecture
   /scaffold {ProjectName} react-frontend
   cp -r templates/infrastructure projects/{ProjectName}/
   ```

2. **Create Implementation Plan**
   - Break into phases (Setup → Core → Features → Polish)
   - Identify parallel work opportunities

3. **Generate Tasks**
   - One task per user story
   - Mark dependencies
   - Estimate complexity

4. **Begin Implementation**
   - Start with P1 stories
   - TDD approach
   - Commit after each story

## What NOT to Do

| Don't | Do Instead |
|-------|------------|
| Ask "Should I use spec-driven?" | Just start the process |
| Ask "Which template?" | Use both (backend + frontend) |
| Ask more than 3 questions at once | Batch into phases |
| Ask open-ended questions | Provide multiple choice with recommendation |
| Ask about standard tech choices | Use defaults, mention in spec |
| Wait for permission to scaffold | Scaffold after spec approval |

## Integration with Other Skills

After specification:
- `project-from-idea` - Full orchestration
- `dotnet-clean-architecture` - Backend implementation
- `react-scaffold` - Frontend implementation
- `fullstack-development` - Cross-stack features
- `docker-infrastructure` - Running the app

## Example Interaction

**User**: "I want a fishing website where people can share spots"

**Claude**:
> A fishing spots sharing platform - nice! Let me gather requirements quickly.
>
> ## Question 1: Target Audience
>
> | Option | Description | Implications |
> |--------|-------------|--------------|
> | A (Recommended) | Public - anyone views, registered users post | Wider reach, need moderation |
> | B | Members only - registration required | Tighter community |
> | C | Invite-only - exclusive | Premium, slower growth |
>
> ## Question 2: MVP Features
>
> | Option | Description |
> |--------|-------------|
> | A | Spots with name, location, description only |
> | B (Recommended) | Spots + photos + star ratings |
> | C | Spots + photos + ratings + comments + reviews |
>
> ## Question 3: Location Display
>
> | Option | Description |
> |--------|-------------|
> | A | List view only (simplest) |
> | B (Recommended) | List + static map per spot |
> | C | Interactive map with all spots |
>
> **Reply with your choices (e.g., "1: A, 2: B, 3: B")**

**User**: "1: A, 2: B, 3: A for MVP"

**Claude**:
> Perfect! Based on your choices:
> - Public platform with registration for posting
> - Spots with photos and ratings
> - List view for MVP (map can come later)
>
> [Generates full specification...]
>
> Does this specification look correct? Once approved, I'll scaffold the project.
