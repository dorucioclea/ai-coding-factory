<!-- Use when user wants to create a new project from scratch, has an idea for an app, or says "build me a..." - orchestrates the full journey from concept to working code using templates. -->


# Project From Idea

## Overview

Orchestrate the complete journey from vague idea to working application. Combines spec-driven-development, brainstorming, scaffolding, and implementation into a seamless workflow.

**Core principle:** Great software starts with great understanding. We build the right thing, not just build things right.

## When to Use

Trigger phrases:
- "I want to build a..."
- "Create an app for..."
- "Build me a..."
- "I have an idea for..."
- "Help me create..."
- New project from scratch

## The Journey

```
┌─────────────────────────────────────────────────────┐
│  STAGE 1: UNDERSTAND                                │
│  Use: spec-driven-development skill                 │
│  Output: Approved specification document            │
│  Duration: ~10-15 questions                         │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  STAGE 2: DESIGN                                    │
│  Use: brainstorming skill                           │
│  Output: Technical design document                  │
│  Duration: Present 2-3 approaches, get approval     │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  STAGE 3: SCAFFOLD                                  │
│  Use: /scaffold-fullstack command                   │
│  Output: Working project structure                  │
│  Duration: Automated (~2-3 minutes)                 │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  STAGE 4: PLAN                                      │
│  Use: /new-story for each feature                   │
│  Output: Prioritized backlog with stories           │
│  Duration: Per feature                              │
└────────────────────┬────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────┐
│  STAGE 5: IMPLEMENT                                 │
│  Use: /implement for each story                     │
│  Output: Working features with tests                │
│  Duration: Per story                                │
└─────────────────────────────────────────────────────┘
```

## Stage Details

### Stage 1: Understand (spec-driven-development)

**REQUIRED SKILL:** Use spec-driven-development

Ask questions to understand:
1. **Problem** - What pain point does this solve?
2. **Users** - Who will use it?
3. **Features** - What are the must-haves?
4. **Scope** - What's explicitly out?
5. **Technical** - Stack preferences, constraints

**Output:** `docs/specs/{date}-{project}-spec.md`

**Gate:** User must approve spec before proceeding.

### Stage 2: Design (brainstorming)

**REQUIRED SKILL:** Use brainstorming

Explore:
1. **Architecture** - How will components interact?
2. **Data Model** - What entities and relationships?
3. **UI/UX** - What's the user experience?
4. **API Design** - What endpoints needed?

**Output:** `docs/plans/{date}-{project}-design.md`

**Gate:** User must approve design before proceeding.

### Stage 3: Scaffold

**Use templates based on spec decisions:**

| Stack Choice | Backend Command | Frontend Command |
|--------------|-----------------|------------------|
| Full Stack | `/scaffold {Name} clean-architecture` | `/scaffold {Name} react-frontend` |
| Backend Only | `/scaffold {Name} clean-architecture` | - |
| Microservice | `/scaffold {Name} microservice` | - |
| Frontend Only | - | `/scaffold {Name} react-frontend` |

**For Full Stack:**
```bash
# 1. Create backend
/scaffold {ProjectName} clean-architecture

# 2. Create frontend
/scaffold {ProjectName} react-frontend

# 3. Configure integration
# - Update frontend .env with API URL
# - Ensure CORS configured in backend
# - Match JWT secrets
```

### Stage 4: Plan (Story Creation)

Convert spec features into stories:

```
For each MVP feature in spec:
  1. /new-story {feature title}
  2. Link to epic
  3. Estimate points
  4. Add to sprint backlog
```

**Prioritization:**
1. Core domain entities first
2. Authentication (if needed)
3. CRUD operations
4. Business logic
5. UI polish

### Stage 5: Implement

For each story in priority order:

```
/implement {story-id}
```

This triggers:
- Technical design for story
- TDD implementation
- Test coverage validation
- Documentation updates
- Commit with story ID

## Example: Fishing Website

```
User: "I want to build a fishing website"

Claude: Great! Let me understand what you're envisioning.

[STAGE 1: UNDERSTAND - spec-driven-development]

What type of fishing website?
  1. 🎣 Community forum for anglers
  2. 🛒 E-commerce for fishing gear
  3. 📍 Fishing spot finder/tracker
  4. 📚 Educational content/guides
  5. Combination or other

User: 3 - Fishing spot finder

Claude: Who will use this?
  1. Casual recreational fishers
  2. Serious sport fishermen
  3. Charter boat operators
  4. All of the above

User: 1 - Casual fishers

Claude: Core features for MVP (pick top 3):
  1. ☐ Map of fishing spots
  2. ☐ User reviews/ratings
  3. ☐ Fish species database
  4. ☐ Weather integration
  5. ☐ Catch logging/diary
  6. ☐ Community forums

User: 1, 2, 5

Claude: Authentication needs?
  1. Email/password (simple)
  2. Social login (Google, Facebook)
  3. Both
  4. No auth - public only

User: 2 - Social login

[... more questions ...]

Claude: Here's your specification:
[Generates docs/specs/2026-01-23-fishspot-spec.md]

Does this capture your vision? [Approve/Modify]

User: Approve

[STAGE 2: DESIGN - brainstorming]

Claude: Let me propose the architecture...

[Presents 2-3 approaches with tradeoffs]

User: Option 1

[STAGE 3: SCAFFOLD]

Claude: Creating project structure...

/scaffold FishSpot clean-architecture
/scaffold FishSpot react-frontend

Project created at: projects/FishSpot/

[STAGE 4: PLAN]

Claude: Creating stories from spec...

Epic 1: Fishing Spots
  - ACF-001: View fishing spots on map
  - ACF-002: Add new fishing spot
  - ACF-003: Rate and review spots

Epic 2: Catch Logging
  - ACF-004: Log a catch
  - ACF-005: View catch history

Epic 3: Authentication
  - ACF-006: Social login with Google
  - ACF-007: User profile

Ready to start implementing? Begin with ACF-006 (auth foundation)?

User: Yes

[STAGE 5: IMPLEMENT]

/implement ACF-006
```

## Templates Reference

| Template | Path | Use For |
|----------|------|---------|
| Clean Architecture | `templates/clean-architecture-solution/` | .NET 8 backend |
| Microservice | `templates/microservice-template/` | Lightweight services |
| React Frontend | `templates/react-frontend-template/` | Next.js frontend |

## Quick Reference

| Stage | Skill/Command | Output |
|-------|---------------|--------|
| Understand | spec-driven-development | Spec document |
| Design | brainstorming | Design document |
| Scaffold | /scaffold-fullstack | Project structure |
| Plan | /new-story | Story backlog |
| Implement | /implement | Working features |

## Red Flags - STOP

- Skipping stages → Each stage gates the next
- User impatient → Explain "5 min questions saves 5 hours rework"
- Scope creep → Refer back to approved spec
- No approval → Cannot proceed to next stage
