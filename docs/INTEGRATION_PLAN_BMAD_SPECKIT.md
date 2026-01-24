# Integration Plan: BMAD Method & Spec-Kit

## Executive Summary

Both **BMAD Method** and **GitHub Spec-Kit** offer valuable patterns that can enhance the AI Coding Factory. After thorough analysis, I recommend a **hybrid integration** that takes the best from both while adapting to our existing architecture.

## Analysis of Both Frameworks

### BMAD Method (Breakthrough Method for Agile AI Driven Development)

**Strengths:**
- Rich agent personas (PM, Architect, Developer, UX Designer, Business Analyst)
- Workflow-driven approach with YAML configurations
- Scale-adaptive intelligence (Levels 0-4 based on project complexity)
- Three development tracks: Quick Flow (10-30 min), BMad Method (30 min-2 hrs), Enterprise (1-3 hrs)
- Strong brainstorming and elicitation workflows
- Interactive menus with trigger commands

**Key Concepts to Adopt:**
1. **Agent Personas** - Named agents with distinct communication styles (e.g., "John the PM who asks WHY relentlessly")
2. **Workflow Orchestration** - Multi-step guided workflows with clear phases
3. **Quick Flow Track** - Lightweight path for simple features
4. **Brainstorming Workflows** - Structured ideation before specification

**File Structure:**
```
_bmad/
├── core/
│   ├── agents/         # Agent definitions (YAML)
│   ├── workflows/      # Workflow definitions
│   └── tasks/          # Reusable task templates
└── bmm/
    ├── agents/         # Domain-specific agents (PM, Architect, etc.)
    └── workflows/      # Phase-based workflows (analysis, planning, implementation)
```

### GitHub Spec-Kit

**Strengths:**
- Constitution-first approach (project principles before code)
- Specification-driven development (specs are executable)
- Clear separation: WHAT/WHY vs HOW
- Excellent template structure (spec, plan, tasks, checklist)
- Branching strategy per feature
- Automatic clarification handling (max 3 questions)
- Quality validation checklists

**Key Concepts to Adopt:**
1. **Constitution** - Define project principles upfront
2. **Spec → Plan → Tasks → Implement** - Clear progression
3. **Technology-agnostic specs** - Focus on user value, not implementation
4. **Structured clarification** - Max 3 questions with suggested answers
5. **Quality checklists** - Validation at each phase
6. **Branch-per-feature** - Organized feature development

**File Structure:**
```
specs/
├── {###-feature-name}/
│   ├── spec.md           # Feature specification
│   ├── plan.md           # Implementation plan
│   ├── tasks.md          # Task breakdown
│   ├── research.md       # Technical research
│   ├── data-model.md     # Entity definitions
│   └── checklists/
│       └── requirements.md
└── memory/
    └── constitution.md   # Project principles
```

---

## Integration Plan

### Phase 1: Foundation (Address User's Immediate Needs)

#### 1.1 Create "Use Templates" Hook

Create a hook that automatically suggests using templates when the user describes an app idea.

**File:** `.claude/hooks/template-suggestion.json`
```json
{
  "triggers": [
    "I want to build",
    "build me a",
    "create an app",
    "I need a website",
    "make me a"
  ],
  "action": "suggest",
  "message": "I'll use spec-driven development to understand your requirements, then scaffold using the templates. Let me ask you a few questions first."
}
```

#### 1.2 Update `spec-driven-development` Skill

Make it automatically trigger when detecting project ideas, not requiring explicit invocation.

**Changes to make:**
- Add detection triggers in the skill
- Integrate spec-kit's clarification pattern (max 3 questions with suggested answers)
- Add constitution concept for project principles

#### 1.3 Create Default Behavior Rule

**File:** `.claude/rules/project-creation.md`
```markdown
# Project Creation Behavior

When a user describes a project idea (fishing website, todo app, etc.):

1. **DO NOT** ask if they want to use spec-driven development
2. **AUTOMATICALLY** begin the spec-driven workflow
3. **ALWAYS** use the templates (clean-architecture, react-frontend, infrastructure)
4. **LIMIT** clarification questions to 3 max
5. **PRESENT** questions with suggested answers in table format
```

---

### Phase 2: Adopt Spec-Kit Patterns

#### 2.1 Add Constitution Support

Create a constitution template that defines project principles before any coding.

**File:** `.claude/skills/constitution/SKILL.md`

Purpose: Define non-negotiable principles for the project (TDD, library-first, etc.)

#### 2.2 Enhance Spec Template

Adopt spec-kit's spec template structure with:
- Prioritized user stories (P1, P2, P3)
- Given-When-Then acceptance scenarios
- Measurable success criteria
- [NEEDS CLARIFICATION] markers (max 3)

**File:** `templates/artifacts/spec-template.md`

#### 2.3 Add Plan Template

Adopt spec-kit's plan template with:
- Technical context (language, dependencies, storage)
- Constitution check
- Project structure decision
- Complexity tracking

**File:** `templates/artifacts/plan-template.md`

#### 2.4 Add Tasks Template

Adopt spec-kit's tasks template with:
- Phase-based organization
- [P] parallel markers
- [Story] traceability
- Checkpoint validation

**File:** `templates/artifacts/tasks-template.md`

---

### Phase 3: Adopt BMAD Patterns

#### 3.1 Add Quick Flow Track

For simple features (bug fixes, small changes), create a lightweight path:

**File:** `.claude/skills/quick-flow/SKILL.md`

Workflow:
1. Describe the change
2. Implement immediately
3. Test
4. Commit

#### 3.2 Enhance Brainstorming

Adopt BMAD's structured brainstorming techniques:

**File:** `.claude/skills/brainstorming/SKILL.md` (enhance existing)

Add:
- Multiple technique options (SCAMPER, Six Thinking Hats, etc.)
- Structured output report
- Integration with spec-driven workflow

#### 3.3 Add Agent Personas (Optional)

Consider adding personality to key agents:
- **PM Agent** - Focuses on WHY and user value
- **Architect Agent** - Technical decisions and trade-offs
- **Developer Agent** - Implementation focus
- **QA Agent** - Test coverage and quality

---

### Phase 4: Unified Workflow

#### 4.1 Create Master Orchestrator Command

**File:** `.claude/commands/create-project.md`

This single command handles the full workflow:

```markdown
# /create-project

## Workflow

1. **Constitution** (if not exists)
   - Define project principles
   - Establish non-negotiables (TDD, Clean Architecture, etc.)

2. **Specification** (spec-driven-development skill)
   - Gather requirements via questions
   - Max 3 clarifications with suggested answers
   - Output: spec.md

3. **Planning** (planning skill)
   - Technical decisions
   - Architecture choices
   - Output: plan.md

4. **Scaffolding** (/scaffold command)
   - Create backend from template
   - Create frontend from template
   - Setup infrastructure

5. **Task Breakdown** (tasks skill)
   - Convert plan to executable tasks
   - Organize by user story
   - Mark parallel opportunities
   - Output: tasks.md

6. **Implementation** (story by story)
   - Implement P1 first
   - TDD approach
   - Checkpoint after each story
```

---

## Proposed File Structure

```
.claude/
├── rules/
│   └── project-creation.md      # Auto-trigger behavior
├── skills/
│   ├── constitution/            # NEW: Project principles
│   ├── spec-driven-development/ # ENHANCED: Spec-kit patterns
│   ├── quick-flow/              # NEW: BMAD quick track
│   ├── brainstorming/           # ENHANCED: BMAD techniques
│   └── fullstack-development/   # EXISTING: Enhanced
├── commands/
│   ├── create-project.md        # NEW: Master orchestrator
│   └── scaffold.md              # EXISTING
└── hooks/
    └── template-suggestion.json # NEW: Auto-detect ideas

templates/
├── clean-architecture-solution/ # EXISTING
├── react-frontend-template/     # EXISTING
├── infrastructure/              # EXISTING
└── artifacts/                   # NEW: Spec-kit templates
    ├── constitution-template.md
    ├── spec-template.md
    ├── plan-template.md
    └── tasks-template.md
```

---

## Implementation Priority

### Immediate (Address User's Pain Points)

1. **Create project-creation rule** - Auto-trigger spec-driven on project ideas
2. **Update spec-driven-development skill** - Integrate spec-kit clarification pattern
3. **Create /create-project command** - Single entry point for new projects

### Short-term (1-2 days)

4. Add constitution skill and template
5. Add spec, plan, tasks templates from spec-kit
6. Enhance brainstorming with BMAD techniques

### Medium-term (Optional Enhancements)

7. Add quick-flow track for simple changes
8. Add agent personas (PM, Architect, etc.)
9. Add scale-adaptive intelligence (detect project complexity)

---

## Key Differences from Direct Adoption

| Aspect | Spec-Kit | BMAD | Our Approach |
|--------|----------|------|--------------|
| Entry Point | `/speckit.specify` | Agent menus | Auto-detect + `/create-project` |
| Branching | Per-feature branches | Not specified | Align with existing git workflow |
| Templates | Python-based | YAML agents | Markdown skills (our pattern) |
| Constitution | Required | Not used | Optional but recommended |
| Clarifications | Max 3 questions | Interactive | Max 3 with suggested answers |
| Tech Stack | Agnostic | Agnostic | .NET + React (our templates) |

---

## What NOT to Adopt

1. **Spec-Kit's Python CLI** - We use Claude Code skills instead
2. **BMAD's YAML agent format** - We use Markdown skills
3. **Spec-Kit's branch naming** (`###-feature-name`) - Keep our ACF-### format
4. **BMAD's module installation** - We don't need npm-based installation

---

## Summary

The integration brings:

1. **From Spec-Kit:**
   - Constitution (project principles)
   - Structured spec → plan → tasks flow
   - Clarification pattern (max 3, with suggestions)
   - Quality checklists

2. **From BMAD:**
   - Quick flow for simple changes
   - Brainstorming techniques
   - Scale awareness (complexity detection)
   - Agent persona concept (optional)

3. **Our Additions:**
   - Auto-detection of project ideas
   - Single `/create-project` entry point
   - Integration with existing templates
   - No explicit invocation needed for spec-driven

The result: When a user says "I want a fishing website", Claude automatically:
1. Starts spec-driven development
2. Asks max 3 clarifying questions
3. Creates spec, plan, tasks
4. Scaffolds using templates
5. Implements story by story
