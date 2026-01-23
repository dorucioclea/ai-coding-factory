---
description: "Create a new fullstack project from an idea. Orchestrates the complete workflow: constitution → specification → planning → scaffolding → implementation."
---

# /create-project - Fullstack Project Creation

## Overview

This command orchestrates the complete project creation workflow from idea to running code. It combines multiple skills into a single cohesive flow.

**Usage**: `/create-project {project idea}`

**Example**: `/create-project A fishing website where users share their favorite spots`

---

## The Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│  1. CONSTITUTION (Optional)                                      │
│     Define project principles if not already established         │
│     Output: docs/constitution.md                                 │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. SPECIFICATION                                                │
│     Gather requirements via max 3 questions                      │
│     Auto-triggers, no permission needed                          │
│     Output: specs/{project}/spec.md                              │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. PLANNING                                                     │
│     Technical architecture and approach                          │
│     Output: specs/{project}/plan.md                              │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. SCAFFOLDING                                                  │
│     Create backend + frontend + infrastructure                   │
│     Uses templates automatically                                 │
│     Output: projects/{ProjectName}-api/                          │
│             projects/{ProjectName}-frontend/                     │
│             projects/{ProjectName}/infrastructure/               │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. TASK BREAKDOWN                                               │
│     Convert plan to executable tasks by user story               │
│     Output: specs/{project}/tasks.md                             │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  6. IMPLEMENTATION                                               │
│     Execute tasks story by story with TDD                        │
│     P1 stories first (MVP)                                       │
│     Checkpoint after each story                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Execution Instructions

### Step 1: Constitution Check

Check if `docs/constitution.md` exists:

**If exists**: Skip to Step 2
**If not exists**: Ask user ONE question:

```markdown
## Project Governance

| Option | Type | Description |
|--------|------|-------------|
| A (Recommended) | Enterprise | Strict architecture, TDD, 80% coverage |
| B | Startup | Pragmatic, speed-focused, critical paths tested |
| C | Minimal | Basic structure only, test as needed |

Which approach fits this project?
```

Based on answer, create `docs/constitution.md` using `constitution` skill.

### Step 2: Specification (Auto-triggered)

Use `spec-driven-development` skill:

1. Acknowledge the idea (1 sentence)
2. Present max 3 questions with recommendations
3. Generate specification after answers
4. Get approval before proceeding

**Output**: `specs/{ProjectSlug}/spec.md`

### Step 3: Planning

Use `planning` skill:

1. Read the approved specification
2. Make technical decisions (stack already decided from templates)
3. Define data model
4. Plan API endpoints
5. Identify phases and dependencies

**Output**: `specs/{ProjectSlug}/plan.md`

### Step 4: Scaffolding

Execute scaffolding commands:

```bash
# Create project directory
mkdir -p projects/{ProjectName}

# Scaffold backend
/scaffold {ProjectName} clean-architecture

# Scaffold frontend
/scaffold {ProjectName} react-frontend

# Copy infrastructure
cp -r templates/infrastructure projects/{ProjectName}/infrastructure

# Setup infrastructure environment
cd projects/{ProjectName}/infrastructure
cp .env.example .env
```

Rename placeholders in scaffolded code:
- `ProjectName` → actual project name
- `projectname` → lowercase version

### Step 5: Task Breakdown

Generate `specs/{ProjectSlug}/tasks.md`:

1. Read spec.md for user stories
2. Read plan.md for technical approach
3. Create tasks organized by user story
4. Mark parallel opportunities [P]
5. Include checkpoints

Use `templates/artifacts/tasks-template.md` as reference.

### Step 6: Implementation

Begin implementation:

1. Start infrastructure: `docker compose up -d`
2. Work through tasks in order
3. TDD: Write test → See it fail → Implement → Pass
4. Commit after each logical unit
5. Checkpoint after each user story (US1, US2, etc.)

**Stop points**:
- After US1 (MVP) - Demo/validate
- After each subsequent story - Validate independently

---

## Question Limits

| Phase | Max Questions | Format |
|-------|---------------|--------|
| Constitution | 1 | Single table with 3 options |
| Specification | 3 per phase | Tables with recommendations |
| Planning | 0-2 | Only if unusual requirements |
| Implementation | As needed | For clarifications only |

---

## Templates Used

| Template | Purpose |
|----------|---------|
| `templates/clean-architecture-solution/` | .NET 8 backend |
| `templates/react-frontend-template/` | Next.js 14 frontend |
| `templates/infrastructure/` | Docker, PostgreSQL, Redis |
| `templates/artifacts/spec-template.md` | Specification format |
| `templates/artifacts/plan-template.md` | Plan format |
| `templates/artifacts/tasks-template.md` | Task breakdown format |

---

## Skills Invoked

| Skill | When |
|-------|------|
| `constitution` | Step 1 (if needed) |
| `spec-driven-development` | Step 2 |
| `planning` | Step 3 |
| `dotnet-clean-architecture` | Step 6 (backend) |
| `react-scaffold` | Step 6 (frontend) |
| `fullstack-development` | Step 6 (cross-stack) |
| `docker-infrastructure` | Step 6 (running app) |
| `tdd` | Step 6 (testing) |

---

## Example Flow

**User**: `/create-project A fishing website where users share spots`

**Claude**:
> I'll help you create a fishing spots sharing platform. Let me gather requirements.
>
> ## Question 1: Target Audience
> | Option | Description | Implications |
> |--------|-------------|--------------|
> | A (Recommended) | Public - anyone views, registered users post | Wide reach |
> | B | Members only | Tighter community |
> | C | Invite-only | Exclusive |
>
> ## Question 2: MVP Features
> | Option | Description |
> |--------|-------------|
> | A | Basic: name, location, description |
> | B (Recommended) | Standard: + photos + ratings |
> | C | Full: + comments + reviews |
>
> ## Question 3: Map Display
> | Option | Description |
> |--------|-------------|
> | A (Recommended) | List view only (MVP) |
> | B | Static map per spot |
> | C | Interactive map |
>
> **Reply with choices (e.g., "1: A, 2: B, 3: A")**

**User**: "1: A, 2: B, 3: A"

**Claude**:
> Perfect! Here's the specification...
> [Generates spec.md]
>
> Specification ready. Shall I proceed with planning and scaffolding?

**User**: "yes"

**Claude**:
> [Generates plan.md]
> [Scaffolds projects]
> [Generates tasks.md]
>
> Project scaffolded! Structure:
> ```
> projects/FishingSpots/
> ├── FishingSpots-api/          # .NET backend
> ├── FishingSpots-frontend/     # React frontend
> └── infrastructure/            # Docker setup
> ```
>
> To start development:
> ```bash
> cd projects/FishingSpots/infrastructure
> docker compose up -d
> ```
>
> Ready to begin implementation with US-001 (User Registration)?

---

## Output Structure

After `/create-project`, you'll have:

```
projects/{ProjectName}/
├── {ProjectName}-api/           # .NET Clean Architecture backend
│   ├── src/
│   │   ├── {ProjectName}.Domain/
│   │   ├── {ProjectName}.Application/
│   │   ├── {ProjectName}.Infrastructure/
│   │   └── {ProjectName}.Api/
│   └── tests/
├── {ProjectName}-frontend/      # Next.js React frontend
│   ├── src/
│   │   ├── app/
│   │   ├── components/
│   │   ├── hooks/
│   │   └── lib/
│   └── e2e/
└── infrastructure/              # Shared Docker setup
    ├── docker-compose.yml
    └── .env

specs/{project-slug}/
├── spec.md                      # Feature specification
├── plan.md                      # Implementation plan
└── tasks.md                     # Task breakdown
```
