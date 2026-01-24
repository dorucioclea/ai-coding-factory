---
name: spec-driven-development
description: Transform vague project ideas into actionable specifications through automated, systematic questioning. Guides through requirement gathering before any implementation.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  phase: project-start
  auto-trigger: "true"
---

# Spec-Driven Development

## Overview

Transform vague ideas into actionable specifications through **automated, systematic questioning**. This skill triggers automatically when a project idea is detected.

**Core Principles:**
- Never ask "should I use spec-driven development?" - just do it
- Never ask "should I use the templates?" - always use them
- Maximum 3 clarification questions per phase (in Standard mode)
- Present questions with suggested answers in table format
- Make reasonable assumptions for anything not critical

## Trigger Phrases

Activate this skill when user says:
- "I want to build"
- "build me a"
- "create an app"
- "I need a website"
- "make me a"
- "I have an idea"
- "let's build"
- "help me create"

## Modes

### Yolo Mode

**Trigger**: User says "yolo", "just build it", "skip questions", or "fast mode"

**Behavior**:
- Ask **0-1 questions** (only if truly ambiguous)
- Make **maximum assumptions** based on common patterns
- Generate specification immediately
- Scaffold and start implementation without waiting

**Assumptions in Yolo Mode**:
- Public app with user registration
- Standard CRUD for main entity
- Email/password auth
- List view for MVP (no maps/complex UI)
- Basic features only (no comments, ratings, etc. unless core to concept)
- PostgreSQL + Redis + .NET + Next.js stack

**Example**:
```
User: "Build me a recipe sharing app, yolo mode"

Claude: "Going full yolo! Building a recipe sharing platform with:
- User registration/login
- Create, view, browse recipes
- Basic search
- Clean Architecture backend + Next.js frontend

Scaffolding now... [proceeds immediately]"
```

---

### Standard Mode (Default)

**Trigger**: Default behavior when project idea detected

**Behavior**:
- Ask **max 3 questions per phase** (usually 2 phases)
- Present options with recommendations
- Make reasonable assumptions for non-critical items
- Generate specification after answers
- Wait for approval before scaffolding

---

### Deep Mode

**Trigger**: User says "deep mode", "thorough", "detailed spec", or "comprehensive"

**Behavior**:
- Ask **up to 5 questions per phase** across 3-4 phases
- Explore edge cases and error handling
- Document all assumptions explicitly
- Generate comprehensive PRD-level specification
- Include architecture decisions and rationale
- Create detailed user journey maps

**Phases in Deep Mode**:

1. **Vision & Users** (5 questions)
   - Target market/audience
   - Problem being solved
   - Success metrics
   - Competitive landscape
   - User personas

2. **Features & Scope** (5 questions)
   - Core features (must-have)
   - Secondary features (should-have)
   - Explicit exclusions
   - MVP vs full vision
   - Integration requirements

3. **User Experience** (5 questions)
   - Key user journeys
   - Error handling expectations
   - Accessibility requirements
   - Mobile vs desktop priority
   - Onboarding flow

4. **Technical & Operations** (3-5 questions)
   - Scale expectations
   - Performance requirements
   - Security/compliance needs
   - Deployment preferences
   - Monitoring/alerting needs

**Output in Deep Mode**:
- Full PRD document (using `prd-template.md`)
- Architecture document (using `architecture-template.md`)
- Detailed user stories with all acceptance criteria
- Technical research items identified

---

## Mode Detection

Automatically detect mode from user language:

| User Says | Mode |
|-----------|------|
| "yolo", "just build it", "skip the questions", "fast" | Yolo |
| "thorough", "detailed", "comprehensive", "deep dive" | Deep |
| (default - no modifier) | Standard |

Can also be explicitly set:
- `/create-project --mode=yolo {idea}`
- `/create-project --mode=deep {idea}`

---

## Scale-Adaptive Intelligence

Automatically detect project complexity and suggest appropriate mode:

### Complexity Signals

| Signal | Low (Yolo) | Medium (Standard) | High (Deep) |
|--------|------------|-------------------|-------------|
| **Entities** | 1-2 | 3-5 | 6+ |
| **User types** | 1 | 2-3 | 4+ |
| **Integrations** | None | 1-2 | 3+ |
| **Auth needs** | Basic | Roles | Multi-tenant |
| **Data sensitivity** | Public | User data | Financial/health |
| **Scale hints** | Personal | Team/SMB | Enterprise |

### Detection Patterns

**Low Complexity (Suggest Yolo)**:
- "simple", "basic", "just a", "quick", "MVP"
- Single user type mentioned
- No integrations mentioned
- Personal/hobby project indicators

**Medium Complexity (Default Standard)**:
- Multiple features mentioned
- 2-3 user types
- Some integrations
- Business use case

**High Complexity (Suggest Deep)**:
- "enterprise", "production", "scale"
- Multiple user types with different permissions
- Compliance/security requirements mentioned
- External integrations required
- Multi-tenant hints

### Auto-Suggestion

When complexity signals detected, suggest but don't force:

```markdown
**Complexity Assessment**

Based on your description, this looks like a **medium complexity** project:
- 3 main entities (Users, Spots, Reviews)
- 2 user types (regular users, moderators)
- No external integrations

I'll use **Standard Mode** (recommended).
Say "yolo" to skip questions or "deep mode" for comprehensive spec.
```

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
**Status**: Draft -> Approved

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
   - Break into phases (Setup -> Core -> Features -> Polish)
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
