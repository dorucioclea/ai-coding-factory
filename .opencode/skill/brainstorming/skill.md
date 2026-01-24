---
name: brainstorming
description: You MUST use this before any creative work - creating features, building components, adding functionality, or modifying behavior. Explores user intent, requirements and design before implementation.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  phase: design
---

# Brainstorming Ideas Into Designs

## Overview

Help turn ideas into fully formed designs and specs through structured collaborative dialogue. This skill combines natural conversation with proven brainstorming techniques.

## When to Use

- Before implementing any new feature
- When exploring "what should we build?"
- When stuck on a design decision
- When requirements are vague or conflicting

## The Process

### Phase 1: Context Gathering

1. Check current project state (files, docs, recent commits)
2. Understand the domain and existing patterns
3. Identify constraints (tech stack, timeline, team)

### Phase 2: Idea Exploration (Choose a Technique)

Select the most appropriate brainstorming technique:

#### Technique 1: SCAMPER (for improving existing features)

| Letter | Question | Example |
|--------|----------|---------|
| **S**ubstitute | What can be replaced? | Different auth provider? |
| **C**ombine | What can merge? | Combine list + detail view? |
| **A**dapt | What can be adjusted? | Mobile-first instead? |
| **M**odify | What can change size/shape? | Pagination vs infinite scroll? |
| **P**ut to other use | Can it serve another purpose? | Export data for analytics? |
| **E**liminate | What can be removed? | Remove rarely-used fields? |
| **R**earrange | What order can change? | Different onboarding flow? |

**Use when**: Enhancing or refactoring existing functionality

#### Technique 2: Six Thinking Hats (for complex decisions)

| Hat | Focus | Questions to Ask |
|-----|-------|------------------|
| White | Facts | What data do we have? What's missing? |
| Red | Feelings | What's our gut reaction? User emotions? |
| Black | Risks | What could go wrong? Edge cases? |
| Yellow | Benefits | What's the best outcome? Quick wins? |
| Green | Creativity | What if we...? Wild ideas? |
| Blue | Process | What's the plan? Next steps? |

**Use when**: Making architectural decisions or evaluating options

#### Technique 3: User Story Mapping (for new features)

    User Journey (left to right)
    +----------+----------+----------+----------+
    | Step 1   | Step 2   | Step 3   | Step 4   |  <- Backbone
    +----------+----------+----------+----------+
MVP | Task A   | Task D   | Task G   | Task J   |  <- Walking Skeleton
    | Task B   | Task E   | Task H   |          |
    +----------+----------+----------+----------+
v2  | Task C   | Task F   | Task I   | Task K   |  <- Enhancements
    +----------+----------+----------+----------+

**Use when**: Defining MVP scope for new features

#### Technique 4: Reverse Brainstorming (for problem-solving)

Instead of "How do we make X work?", ask:
1. "How could we make X fail completely?"
2. List all ways to cause failure
3. Reverse each into a solution

**Example**:
- How to make login fail? -> Don't validate input -> Solution: Add validation
- How to make it slow? -> No caching -> Solution: Add Redis cache
- How to confuse users? -> No feedback -> Solution: Add loading states

**Use when**: Identifying edge cases and failure modes

#### Technique 5: Mind Mapping (for exploration)

                    +--- Auth Method
                    |       +-- Email/Password
        +-- Auth ---+       +-- OAuth
        |           |       +-- Magic Link
        |           +--- Roles
        |                   +-- Admin
        |                   +-- User
Feature +
        |           +--- Create
        |           |       +-- Form
        +-- CRUD ---+       +-- Validation
        |           +-- Read
        |           |       +-- List
        |           |       +-- Detail
        |           +-- Update/Delete
        |
        +-- UI -----+-- Desktop
                    +-- Mobile

**Use when**: Initial feature exploration, understanding scope

### Phase 3: Approach Selection

After brainstorming, present 2-3 approaches:

    ## Approaches
    
    ### Option A: {Name} (Recommended)
    **Pros**: {benefits}
    **Cons**: {drawbacks}
    **Effort**: {Low/Medium/High}
    **Why recommended**: {reasoning}
    
    ### Option B: {Name}
    **Pros**: {benefits}
    **Cons**: {drawbacks}
    **Effort**: {Low/Medium/High}
    
    ### Option C: {Name}
    **Pros**: {benefits}
    **Cons**: {drawbacks}
    **Effort**: {Low/Medium/High}
    
    **My recommendation**: Option A because {specific reasons}

### Phase 4: Design Presentation

Once approach is selected, present design incrementally:

1. **Architecture Overview** (200-300 words)
   - High-level structure
   - Key components
   - Check: "Does this structure make sense?"

2. **Data Model** (200-300 words)
   - Entities and relationships
   - Key fields
   - Check: "Does this data model cover your needs?"

3. **User Flows** (200-300 words)
   - Step-by-step journeys
   - Error cases
   - Check: "Does this flow match your expectations?"

4. **Technical Details** (200-300 words)
   - API endpoints
   - State management
   - Check: "Any technical concerns?"

### Phase 5: Documentation

After validation, write design to:

    docs/designs/YYYY-MM-DD-{topic}-design.md

Include:
- Problem statement
- Chosen approach (and why)
- Architecture diagram (ASCII or Mermaid)
- Data model
- User flows
- Open questions
- Next steps

## Quick Brainstorm (5-10 min)

For smaller decisions, use this abbreviated flow:

1. **State the problem** (1 sentence)
2. **List 3 options** (bullet points)
3. **Pick one with reasoning** (2-3 sentences)
4. **Proceed**

    **Problem**: How should we handle image uploads?
    
    **Options**:
    1. Store locally (simple, limited scale)
    2. Use S3/Cloudinary (scalable, extra cost)
    3. Base64 in DB (no external deps, bloats DB)
    
    **Decision**: Option 2 (Cloudinary) - scalable, has CDN,
    free tier sufficient for MVP. Can migrate later if needed.

## Key Principles

- **One question at a time** - Don't overwhelm
- **Multiple choice preferred** - Easier to answer
- **YAGNI ruthlessly** - Remove unnecessary complexity
- **Explore alternatives** - Always consider 2-3 approaches
- **Incremental validation** - Present in sections, validate each
- **Document decisions** - Future you will thank you

## Integration

After brainstorming, continue with:
- spec-driven-development - Detailed requirements
- planning - Implementation planning
- /scaffold - Project setup
