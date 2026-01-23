# Project Creation Behavior

## Auto-Detection Triggers

When a user message matches any of these patterns, **AUTOMATICALLY** begin the spec-driven development workflow:

- "I want to build..."
- "Build me a..."
- "Create an app for..."
- "I need a website for..."
- "Make me a..."
- "I have an idea for..."
- "Let's build..."
- "Help me create..."
- Any description of an application concept (e.g., "fishing website", "task manager", "booking system")

## Mandatory Behavior

1. **DO NOT** ask "Would you like me to use spec-driven development?"
2. **DO NOT** ask "Should I use the templates?"
3. **AUTOMATICALLY** invoke the `spec-driven-development` skill
4. **ALWAYS** use the templates (clean-architecture-solution, react-frontend-template, infrastructure)
5. **LIMIT** clarification questions to 3 maximum
6. **PRESENT** questions with suggested answers in table format

## Default Flow

When detecting a project idea:

```
1. Acknowledge the idea briefly
2. Begin spec-driven development immediately
3. Ask max 3 clarifying questions (if needed)
4. Generate specification
5. Create implementation plan
6. Scaffold using /scaffold command
7. Implement story by story with TDD
```

## Question Format

When clarification is needed, use this format:

```markdown
## Question 1: [Topic]

**Context**: [Why this matters]

| Option | Description | Implications |
|--------|-------------|--------------|
| A | [First option] | [What this means] |
| B | [Second option] | [What this means] |
| C | [Third option] | [What this means] |

**My recommendation**: Option [X] because [reason]
```

## Templates Always Used

- **Backend**: `templates/clean-architecture-solution/` (.NET 8 Clean Architecture)
- **Frontend**: `templates/react-frontend-template/` (Next.js 14 + shadcn/ui)
- **Infrastructure**: `templates/infrastructure/` (Docker, PostgreSQL, Redis)

## Example Interaction

**User**: "I want a fishing website where people can share their favorite spots"

**Claude** (automatically):
> I'll help you build a fishing spots sharing platform. Let me gather some quick requirements.
>
> ## Question 1: User Scope
>
> | Option | Description | Implications |
> |--------|-------------|--------------|
> | A | Public (anyone can view, registered users post) | Wider audience, need moderation |
> | B | Members only (registration required) | Tighter community, simpler auth |
> | C | Invite-only (exclusive community) | Premium feel, slower growth |
>
> **My recommendation**: Option A - typical for community sites
>
> ## Question 2: Core Features (MVP)
>
> | Option | Description |
> |--------|-------------|
> | A | Spots + basic info (name, location, description) |
> | B | Spots + photos + ratings |
> | C | Spots + photos + ratings + comments |
>
> **My recommendation**: Option B for MVP
>
> ## Question 3: Map Integration
>
> | Option | Description |
> |--------|-------------|
> | A | No map (list view only) |
> | B | Static map display |
> | C | Interactive map with markers |
>
> **My recommendation**: Option A for MVP, add map later

Then proceed to specification after user answers.
