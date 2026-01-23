# Template & Skills Expansion Plan

## Executive Summary

This document outlines a comprehensive plan to expand the AI Coding Factory's template system and integrate external Claude Code skills from multiple repositories. The goal is to transform this from a .NET-only platform into a multi-stack enterprise development forge.

---

## Part 1: Current Template Assessment

### Existing Templates (13 Total)

| Template | Type | Importance | Usage Frequency |
|----------|------|------------|-----------------|
| `entity.cs.template` | Domain | **Critical** | Every entity creation |
| `repository.cs.template` | Infrastructure | **Critical** | Every data access layer |
| `controller.cs.template` | API | **Critical** | Every endpoint |
| `command-handler.cs.template` | Application | **High** | Every write operation |
| `query-handler.cs.template` | Application | **High** | Every read operation |
| `unit-test.cs.template` | Testing | **Critical** | Every test file |
| `integration-test.cs.template` | Testing | **High** | API testing |
| `adr/*.md` (6 templates) | Documentation | **Medium** | Architecture decisions |

### Template Quality Assessment

**Strengths:**
- Story ID integration (`{{StoryId}}`) for traceability
- Clean Architecture alignment
- DDD patterns (factory methods, domain events)
- xUnit + FluentAssertions + Moq stack

**Gaps Identified:**
- No React/frontend templates
- No infrastructure templates (Dockerfile, K8s, CI/CD)
- No API client generation templates
- No validation template (FluentValidation)
- No middleware template
- No background service template

---

## Part 2: External Skills Repositories Analysis

### 1. Anthropic Skills (`anthropics/skills`)

**Repository:** https://github.com/anthropics/skills

| Skill Category | Available Skills | Relevance |
|----------------|------------------|-----------|
| Document Skills | PDF, DOCX, PPTX, XLSX | **High** - Enterprise docs |
| Development | Web app testing, MCP generation | **Medium** |
| Creative | Design workflows | **Low** for factory |

**Integration Method:**
```bash
# Via plugin marketplace
/plugin marketplace add anthropics/skills
/plugin install document-skills@anthropic-agent-skills
```

**Value:** Production-grade document handling for enterprise requirements.

---

### 2. Superpowers (`obra/superpowers`)

**Repository:** https://github.com/obra/superpowers

| Skill | Purpose | Relevance |
|-------|---------|-----------|
| `test-driven-development` | RED-GREEN-REFACTOR cycle | **Critical** |
| `systematic-debugging` | 4-phase root cause analysis | **High** |
| `brainstorming` | Design refinement | **Medium** |
| `writing-plans` | Detailed task breakdown | **High** |
| `executing-plans` | Batch processing | **High** |
| `code-review` | Request/receive reviews | **High** |
| `git-worktrees` | Parallel development | **Medium** |
| `subagent-development` | Concurrent task handling | **High** |

**Integration Method:**
```bash
# Manual - copy to ~/.claude/skills/ or project .claude/skills/
cp -r superpowers/skills/* .claude/skills/
```

**Value:** Enhances development workflow automation significantly.

---

### 3. Trail of Bits Security Skills (`dorucioclea/skills-security-Claude`)

**Repository:** https://github.com/dorucioclea/skills-security-Claude

| Skill | Purpose | Relevance |
|-------|---------|-----------|
| `building-secure-contracts` | Blockchain security | **Low** (unless crypto) |
| `static-analysis` | Code scanning | **Critical** |
| `testing-handbook-skills` | Security testing | **Critical** |
| `semgrep-rule-creator` | Custom rules | **High** |
| `differential-review` | Change auditing | **High** |
| `property-based-testing` | Verification | **High** |
| `fix-review` | Audit lifecycle | **Medium** |
| `variant-analysis` | Pattern detection | **Medium** |
| `ask-questions-if-underspecified` | Requirements clarity | **High** |

**Integration Method:**
```bash
/plugin marketplace add trailofbits/skills
```

**Value:** Enterprise-grade security scanning and audit capabilities.

---

### 4. Agent Skills (`dorucioclea/agent-skills`)

**Repository:** https://github.com/dorucioclea/agent-skills

| Skill | Purpose | Relevance |
|-------|---------|-----------|
| `vercel-deploy` | Zero-auth deployment | **High** for frontend |

**Integration Method:**
```bash
cp -r agent-skills/skills/* ~/.claude/skills/
```

**Value:** Rapid frontend deployment without authentication setup.

---

### 5. Planning with Files (`OthmanAdi/planning-with-files`)

**Repository:** https://github.com/OthmanAdi/planning-with-files

| Component | Purpose | Relevance |
|-----------|---------|-----------|
| `task_plan.md` | Phase/progress tracking | **Critical** |
| `findings.md` | Research documentation | **High** |
| `progress.md` | Session logging | **High** |
| Hooks (Pre/Post/Stop) | Workflow automation | **Critical** |

**Integration Method:**
```bash
# Copy skill and templates
cp -r planning-with-files/templates/* .claude/templates/
cp -r planning-with-files/skills/* .claude/skills/
```

**Value:** Structured planning methodology for complex tasks.

---

## Part 3: React/Frontend Template Expansion

### Proposed React Templates

| Template | Purpose | Priority |
|----------|---------|----------|
| `component.tsx.template` | React functional component | **P0** |
| `hook.ts.template` | Custom React hook | **P0** |
| `context.tsx.template` | React Context + Provider | **P1** |
| `page.tsx.template` | Next.js page component | **P1** |
| `api-route.ts.template` | Next.js API route | **P1** |
| `component.test.tsx.template` | React Testing Library | **P0** |
| `e2e.spec.ts.template` | Playwright E2E test | **P2** |
| `store.ts.template` | Zustand/Redux store | **P2** |
| `form.tsx.template` | React Hook Form + Zod | **P1** |
| `table.tsx.template` | TanStack Table component | **P2** |

### React Template Example Structure

```tsx
// component.tsx.template
// Story: {{StoryId}}
// Generated by Claude Code

import { FC } from 'react';

interface {{ComponentName}}Props {
  // Define props
}

export const {{ComponentName}}: FC<{{ComponentName}}Props> = (props) => {
  return (
    <div data-testid="{{componentName}}">
      {/* Component content */}
    </div>
  );
};

{{ComponentName}}.displayName = '{{ComponentName}}';
```

---

## Part 4: Additional .NET Templates Needed

### Infrastructure Templates

| Template | Purpose | Priority |
|----------|---------|----------|
| `dockerfile.template` | Multi-stage .NET build | **P0** |
| `docker-compose.yml.template` | Local dev environment | **P1** |
| `k8s-deployment.yaml.template` | Kubernetes manifest | **P1** |
| `helm-chart.template` | Helm chart structure | **P2** |
| `github-workflow.yml.template` | CI/CD pipeline | **P0** |

### Application Layer Templates

| Template | Purpose | Priority |
|----------|---------|----------|
| `validator.cs.template` | FluentValidation | **P0** |
| `behavior.cs.template` | MediatR pipeline | **P1** |
| `dto.cs.template` | Data transfer object | **P1** |
| `mapper.cs.template` | AutoMapper profile | **P2** |

### Infrastructure Layer Templates

| Template | Purpose | Priority |
|----------|---------|----------|
| `db-context.cs.template` | EF Core DbContext | **P0** |
| `entity-config.cs.template` | EF Core configuration | **P1** |
| `migration.cs.template` | EF Core migration | **P2** |
| `background-service.cs.template` | Hosted service | **P1** |

---

## Part 5: Skills Integration Strategy

### Option A: Plugin Marketplace (Recommended)

**For repositories with plugin support:**
```bash
# Anthropic skills
/plugin marketplace add anthropics/skills

# Trail of Bits security
/plugin marketplace add trailofbits/skills
```

**Pros:** Automatic updates, version management, official support
**Cons:** Limited to plugin-enabled repos

### Option B: Manual Copy

**For repositories without plugin support:**
```bash
# Create skills directory
mkdir -p .claude/skills

# Copy from cloned repos
git clone https://github.com/obra/superpowers /tmp/superpowers
cp -r /tmp/superpowers/skills/* .claude/skills/

git clone https://github.com/OthmanAdi/planning-with-files /tmp/planning
cp -r /tmp/planning/skills/* .claude/skills/
```

**Pros:** Full control, can customize
**Cons:** Manual updates required, potential conflicts

### Option C: Git Submodules

**For maintaining external skill references:**
```bash
git submodule add https://github.com/obra/superpowers .claude/external/superpowers
git submodule add https://github.com/OthmanAdi/planning-with-files .claude/external/planning
```

**Pros:** Version tracked, easy updates
**Cons:** Complexity, requires submodule awareness

### Recommendation: Hybrid Approach

1. **Use Plugin Marketplace** for `anthropics/skills` and `trailofbits/skills`
2. **Manual Copy + Customize** for `superpowers` and `planning-with-files`
3. **Fork and Maintain** your own versions for long-term customization

---

## Part 6: Implementation Phases

### Phase 1: Foundation (1-2 days)

1. Install plugin marketplace skills:
   ```bash
   /plugin marketplace add anthropics/skills
   /plugin marketplace add trailofbits/skills
   ```

2. Create skills directory structure:
   ```
   .claude/
   ├── skills/
   │   ├── planning/          # From planning-with-files
   │   ├── tdd/               # From superpowers
   │   ├── code-review/       # From superpowers
   │   └── debugging/         # From superpowers
   ```

3. Add critical .NET templates:
   - `validator.cs.template`
   - `dockerfile.template`
   - `github-workflow.yml.template`

### Phase 2: React Foundation (2-3 days)

1. Create React template directory:
   ```
   .claude/templates/react/
   ├── component.tsx.template
   ├── hook.ts.template
   ├── component.test.tsx.template
   └── page.tsx.template
   ```

2. Add React-specific context files:
   ```
   .claude/context/
   ├── react-patterns.md
   ├── typescript-standards.md
   └── frontend-architecture.md
   ```

3. Create React slash commands:
   - `/add-component` - Generate React component
   - `/add-hook` - Generate custom hook
   - `/add-page` - Generate Next.js page

### Phase 3: Security Integration (1-2 days)

1. Integrate Trail of Bits skills
2. Create security-focused templates:
   - `security-checklist.md.template`
   - `threat-model.md.template`
3. Add security hooks:
   - `pre-push-security-scan.sh`
   - `post-dependency-audit.sh`

### Phase 4: Planning & Workflow (1-2 days)

1. Integrate planning-with-files methodology
2. Create planning templates:
   - `task_plan.md.template`
   - `findings.md.template`
   - `progress.md.template`
3. Add workflow commands:
   - `/plan-task` - Initialize 3-file planning
   - `/update-progress` - Log session updates

### Phase 5: Full Stack Templates (3-5 days)

1. Complete .NET infrastructure templates
2. Complete React template library
3. Add full-stack templates:
   - `api-client.ts.template` - Generated from OpenAPI
   - `graphql-query.ts.template` - GraphQL operations
4. Cross-stack integration tests

---

## Part 7: Directory Structure After Expansion

```
.claude/
├── settings.json
├── file-map.json
├── error-patterns.json
├── context-priorities.json
├── quick-actions.md
│
├── commands/                    # 30+ commands
│   ├── validate.md
│   ├── ...existing...
│   ├── add-component.md         # NEW: React
│   ├── add-hook.md              # NEW: React
│   ├── add-page.md              # NEW: React
│   ├── plan-task.md             # NEW: Planning
│   └── security-audit.md        # NEW: Security
│
├── context/                     # 8+ context files
│   ├── architecture.md
│   ├── patterns.md
│   ├── anti-patterns.md
│   ├── glossary.md
│   ├── recent-decisions.md
│   ├── react-patterns.md        # NEW
│   ├── typescript-standards.md  # NEW
│   └── frontend-architecture.md # NEW
│
├── templates/                   # 30+ templates
│   ├── # .NET Templates (existing + new)
│   ├── entity.cs.template
│   ├── repository.cs.template
│   ├── controller.cs.template
│   ├── command-handler.cs.template
│   ├── query-handler.cs.template
│   ├── unit-test.cs.template
│   ├── integration-test.cs.template
│   ├── validator.cs.template        # NEW
│   ├── dockerfile.template          # NEW
│   ├── github-workflow.yml.template # NEW
│   │
│   ├── react/                       # NEW: React templates
│   │   ├── component.tsx.template
│   │   ├── hook.ts.template
│   │   ├── context.tsx.template
│   │   ├── page.tsx.template
│   │   ├── component.test.tsx.template
│   │   └── e2e.spec.ts.template
│   │
│   ├── planning/                    # NEW: Planning templates
│   │   ├── task_plan.md.template
│   │   ├── findings.md.template
│   │   └── progress.md.template
│   │
│   └── adr/
│       └── ...existing...
│
├── skills/                          # NEW: External skills
│   ├── tdd/
│   │   └── SKILL.md
│   ├── debugging/
│   │   └── SKILL.md
│   ├── code-review/
│   │   └── SKILL.md
│   ├── planning/
│   │   └── SKILL.md
│   └── frontend-design/
│       └── SKILL.md
│
└── hooks/                           # 12+ hooks
    ├── ...existing 10...
    ├── pre-push-security-scan.sh    # NEW
    └── post-dependency-audit.sh     # NEW
```

---

## Part 8: Metrics After Expansion

| Metric | Current | After Expansion | Growth |
|--------|---------|-----------------|--------|
| Slash Commands | 25 | 35+ | +40% |
| Context Files | 5 | 8+ | +60% |
| Templates | 13 | 30+ | +130% |
| Hooks | 10 | 12+ | +20% |
| Skills | 0 | 10+ | New! |
| Stacks Supported | 1 (.NET) | 2 (.NET + React) | +100% |

---

## Part 9: Integration Commands Summary

### Immediate Actions (Can Run Now)

```bash
# 1. Install Anthropic skills via marketplace
/plugin marketplace add anthropics/skills
/plugin install document-skills@anthropic-agent-skills

# 2. Install Trail of Bits security skills
/plugin marketplace add trailofbits/skills

# 3. Clone and copy planning skills
git clone https://github.com/OthmanAdi/planning-with-files /tmp/planning
mkdir -p .claude/skills/planning
cp /tmp/planning/skills/planning-with-files/SKILL.md .claude/skills/planning/

# 4. Clone and copy superpowers skills
git clone https://github.com/obra/superpowers /tmp/superpowers
mkdir -p .claude/skills/tdd
mkdir -p .claude/skills/debugging
mkdir -p .claude/skills/code-review
cp /tmp/superpowers/skills/test-driven-development/SKILL.md .claude/skills/tdd/
cp /tmp/superpowers/skills/systematic-debugging/SKILL.md .claude/skills/debugging/
cp /tmp/superpowers/skills/code-review/SKILL.md .claude/skills/code-review/
```

### Skills That Require Manual Copy

| Repository | Reason | Action |
|------------|--------|--------|
| `obra/superpowers` | No plugin manifest | Manual copy |
| `OthmanAdi/planning-with-files` | No plugin manifest | Manual copy |
| `dorucioclea/agent-skills` | Fork - customize first | Manual copy |

### Skills Available via Marketplace

| Repository | Plugin Name |
|------------|-------------|
| `anthropics/skills` | `anthropic-agent-skills` |
| `trailofbits/skills` | `trailofbits-skills` |

---

## Conclusion

This plan provides a roadmap to:

1. **Expand templates** from 13 to 30+ covering .NET and React
2. **Integrate external skills** for TDD, security, planning, and frontend design
3. **Add new commands** for React component generation and workflow automation
4. **Maintain traceability** with Story ID integration across all templates

**Estimated Total Effort:** 8-12 days for full implementation

**Recommended Starting Point:** Phase 1 (Foundation) - install marketplace skills and add critical .NET templates.

---

## Sources

- [Anthropic Skills Repository](https://github.com/anthropics/skills)
- [Superpowers by Obra](https://github.com/obra/superpowers)
- [Trail of Bits Security Skills](https://github.com/dorucioclea/skills-security-Claude)
- [Agent Skills (Vercel)](https://github.com/dorucioclea/agent-skills)
- [Planning with Files](https://github.com/OthmanAdi/planning-with-files)
- [Claude Frontend Design Skill](https://github.com/anthropics/claude-code/blob/main/plugins/frontend-design/skills/frontend-design/SKILL.md)
- [Claude Code Plugins Directory](https://claude-plugins.dev/skills)

---

*Last Updated: January 2025*
