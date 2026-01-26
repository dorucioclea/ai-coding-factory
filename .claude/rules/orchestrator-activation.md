# Orchestrator Activation

Auto-routes tasks to appropriate agents based on triggers.

## Task Routing

| Triggers | Route To | Skills |
|----------|----------|--------|
| "user story", "requirements", "epic", "PRD" | Product Owner | `brainstorming`, `spec-driven-development` |
| "architecture", "design", "ADR", "layer" | Architect | `dotnet-clean-architecture`, `planning` |
| "implement", "code", "feature", "endpoint" | Developer + TDD Guide | `coding-standards` |
| "test", "coverage", "TDD", "unit test" | TDD Guide | `systematic-debugging` |
| "security", "auth", "vulnerability" | Security Reviewer | *CRITICAL for auth code* |
| "React Native", "mobile", "Expo" | RN Developer | + rn-navigator, rn-state-architect |
| "deploy", "CI/CD", "Docker", "K8s" | DevOps | - |

## Cross-Cutting Collaborations

Auto-suggest when editing:

| File Pattern | Collaborators |
|--------------|---------------|
| `**/auth/**`, `**/Auth*` | Security Reviewer |
| `**/Controllers/**`, `**/api/**` | Security + Doc-Updater |
| `**/Domain/**` | Architect |
| `**/migrations/**` | Architect + Security |

## Phase Behavior

| Phase | Allowed | Forbidden |
|-------|---------|-----------|
| Ideation | Requirements, architecture, risk | Code changes |
| Development | Implement, test, docs | Unreviewed deps |
| Validation | CI checks, validation scripts | Disable tests |
| Release | Package, release, final docs | Policy violations |
