# Claude Code Optimization - Improvements List

This document catalogs improvements that would help Claude Code work more effectively with the AI Coding Factory platform.

## Priority Legend
- **P0**: Critical - Implement immediately
- **P1**: High - Implement in current sprint
- **P2**: Medium - Implement in next sprint
- **P3**: Low - Nice to have

## Completed Optimizations

### ✅ Core Configuration
| Item | Status | Description |
|------|--------|-------------|
| CLAUDE.md | ✅ Done | Created comprehensive Claude Code instruction file |
| .claude/settings.json | ✅ Done | Configured permissions, hooks, environment |
| .claude/commands/ | ✅ Done | Created 10 slash commands |
| .claude/hooks/ | ✅ Done | Created pre/post execution hooks |
| .claude/mcp-servers.json | ✅ Done | MCP server configuration template |
| Documentation updates | ✅ Done | Updated README, AGENTS.md for Claude Code |

### ✅ Slash Commands Created
| Command | Purpose |
|---------|---------|
| `/validate` | Run all validation scripts |
| `/new-story` | Create INVEST-compliant user story |
| `/scaffold` | Create new .NET project from template |
| `/implement` | Full implementation workflow for a story |
| `/traceability` | Generate story-test-commit linkage report |
| `/security-review` | Perform security audit |
| `/code-review` | Perform code quality review |
| `/adr` | Create Architecture Decision Record |
| `/release` | Prepare release with all artifacts |
| `/sprint` | Sprint management (plan, daily, review, retro) |

---

## P0: Critical Improvements

### 1. Enhanced Context Files
**Status**: Pending
**Effort**: Medium

Create additional context files that Claude Code can reference:

```
.claude/
├── context/
│   ├── architecture.md      # Cached architecture overview
│   ├── patterns.md          # Common code patterns to follow
│   ├── anti-patterns.md     # Patterns to avoid
│   ├── glossary.md          # Domain terminology
│   └── recent-decisions.md  # Recent ADR summaries
```

**Benefits**:
- Reduces repeated exploration of the same files
- Provides consistent patterns across sessions
- Improves code generation accuracy

**Implementation**:
```bash
# Create context directory
mkdir -p .claude/context

# Generate architecture context from existing docs
cat docs/*/ARCHITECTURE*.md > .claude/context/architecture.md

# Create patterns file from skills
for skill in .opencode/skill/*/SKILL.md; do
  echo "## $(dirname $skill | xargs basename)" >> .claude/context/patterns.md
  cat "$skill" >> .claude/context/patterns.md
done
```

---

### 2. Automated Test Generation Templates
**Status**: Pending
**Effort**: High

Create `.claude/templates/` with test templates Claude Code can use:

```csharp
// .claude/templates/unit-test.cs.template
using Xunit;
using FluentAssertions;
using Moq;

namespace {{Namespace}}.UnitTests;

public class {{ClassName}}Tests
{
    private readonly Mock<I{{Dependency}}> _{{dependency}}Mock;
    private readonly {{ClassName}} _sut;

    public {{ClassName}}Tests()
    {
        _{{dependency}}Mock = new Mock<I{{Dependency}}>();
        _sut = new {{ClassName}}(_{{dependency}}Mock.Object);
    }

    [Fact]
    [Trait("Story", "{{StoryId}}")]
    public void {{MethodName}}_{{Scenario}}_{{ExpectedResult}}()
    {
        // Arrange

        // Act
        var result = _sut.{{MethodName}}();

        // Assert
        result.Should().NotBeNull();
    }
}
```

**Benefits**:
- Consistent test structure
- Automatic story ID linking
- Reduces boilerplate generation time

---

### 3. Interactive Story Wizard
**Status**: Pending
**Effort**: Medium

Enhance `/new-story` command with interactive prompts:

```markdown
# Enhanced /new-story flow

1. Ask for persona (with suggestions from existing stories)
2. Ask for action (with verb suggestions)
3. Ask for benefit (with template options)
4. Generate acceptance criteria from common patterns
5. Auto-link to related stories if detected
6. Suggest story points based on similar completed stories
```

**Implementation location**: `.claude/commands/new-story.md`

---

## P1: High Priority Improvements

### 4. Pre-commit Validation Hook
**Status**: Pending
**Effort**: Low

Enhance `.claude/hooks/` with a pre-commit hook that validates before allowing commits:

```bash
#!/bin/bash
# .claude/hooks/pre-commit.sh

# Check for story ID in staged files
if ! git diff --cached --name-only | grep -q "ACF-"; then
    echo "WARNING: No story ID found in staged changes"
fi

# Run quick validation
./scripts/validate-project.sh --quick

# Check test coverage for changed files
dotnet test --filter "Category=Unit" --no-build
```

---

### 5. Smart File Discovery
**Status**: Pending
**Effort**: Medium

Create a `.claude/file-map.json` that maps concepts to files:

```json
{
  "authentication": [
    "src/*/Services/AuthService.cs",
    "src/*/Controllers/AuthController.cs",
    "src/*/Configuration/JwtSettings.cs"
  ],
  "domain-entities": [
    "src/*.Domain/Entities/*.cs"
  ],
  "repositories": [
    "src/*.Infrastructure/Repositories/*.cs",
    "src/*.Domain/Repositories/I*.cs"
  ],
  "commands": [
    "src/*.Application/Commands/**/*.cs"
  ],
  "queries": [
    "src/*.Application/Queries/**/*.cs"
  ]
}
```

**Benefits**:
- Claude Code can quickly find relevant files
- Reduces exploration time
- Improves accuracy of modifications

---

### 6. Skill-to-Command Migration
**Status**: Pending
**Effort**: High

Convert OpenCode skills to Claude Code commands:

| OpenCode Skill | Claude Code Command |
|----------------|---------------------|
| net-web-api | /scaffold-api |
| net-domain-model | /add-entity |
| net-repository-pattern | /add-repository |
| net-jwt-auth | /add-auth |
| net-testing | /generate-tests |
| net-docker | /add-docker |
| net-kubernetes | /add-k8s |
| net-cqrs | /add-cqrs |

Each command would be a detailed markdown file in `.claude/commands/`.

---

### 7. Coverage Integration
**Status**: Pending
**Effort**: Medium

Create a `/coverage` command that:
1. Runs tests with coverage
2. Parses Coverlet output
3. Reports coverage by layer
4. Highlights uncovered lines in context

```bash
# .claude/commands/coverage.md
# Usage: /coverage [project]

dotnet test --collect:"XPlat Code Coverage"
python3 scripts/coverage/check-coverage.py coverage.xml

# Output format:
# Layer          | Coverage | Status
# Domain         | 92%      | PASS
# Application    | 85%      | PASS
# Infrastructure | 78%      | WARN (below 80%)
```

---

## P2: Medium Priority Improvements

### 8. Dependency Graph Visualization
**Status**: Pending
**Effort**: High

Create a command to generate and display project dependencies:

```bash
# /dependencies command
# Generates ASCII dependency graph

Domain (no deps)
    ↓
Application (→ Domain)
    ↓
Infrastructure (→ Domain, Application)
    ↓
API (→ All)
```

---

### 9. Quick Actions Menu
**Status**: Pending
**Effort**: Low

Create `.claude/quick-actions.md` with common operations:

```markdown
# Quick Actions

## Common Development Tasks
- Add new entity: /add-entity <EntityName>
- Add new endpoint: /add-endpoint <controller> <action>
- Generate tests: /generate-tests <file>

## Quality Checks
- Run all tests: dotnet test
- Check coverage: /coverage
- Security scan: /security-review

## Git Operations
- Create feature branch: /branch <story-id>
- Commit with story: /commit <message>
```

---

### 10. Error Pattern Database
**Status**: Pending
**Effort**: Medium

Create `.claude/error-patterns.json` with common errors and solutions:

```json
{
  "CS0246": {
    "message": "The type or namespace name could not be found",
    "solutions": [
      "Add missing using statement",
      "Add package reference to .csproj",
      "Check namespace in project"
    ]
  },
  "EF001": {
    "message": "Unable to create migrations",
    "solutions": [
      "Ensure DbContext is registered",
      "Check connection string",
      "Run from Infrastructure project"
    ]
  }
}
```

---

### 11. Sprint Automation
**Status**: Pending
**Effort**: High

Enhance `/sprint` command with automated ceremonies:

- **Daily standup**: Parse git log for yesterday's work
- **Sprint review**: Generate demo script from completed stories
- **Retrospective**: Analyze metrics and suggest improvements
- **Velocity tracking**: Calculate and trend velocity

---

### 12. ADR Templates Library
**Status**: Pending
**Effort**: Low

Create pre-filled ADR templates for common decisions:

```
.claude/templates/adr/
├── database-choice.md
├── authentication-method.md
├── caching-strategy.md
├── message-queue.md
├── api-versioning.md
└── deployment-strategy.md
```

---

## P3: Nice to Have Improvements

### 13. Learning Mode
**Status**: Pending
**Effort**: High

Create a mode where Claude Code learns from user corrections:

```markdown
# /learn command

When user corrects Claude Code's output:
1. Record the correction
2. Store pattern in .claude/learned-patterns.json
3. Reference in future similar tasks
```

---

### 14. Project Health Dashboard
**Status**: Pending
**Effort**: Medium

Create `/health` command that shows project status:

```
Project Health Report
=====================
Tests:        ✅ 156 passing, 0 failing
Coverage:     ✅ 87% (target: 80%)
Security:     ✅ 0 vulnerabilities
Dependencies: ⚠️  2 outdated packages
Docs:         ✅ All required docs present
Traceability: ✅ 100% story coverage
```

---

### 15. Context Window Optimization
**Status**: Pending
**Effort**: Medium

Create `.claude/context-priorities.json` to optimize what Claude Code reads:

```json
{
  "always_read": [
    "CLAUDE.md",
    "CORPORATE_RND_POLICY.md"
  ],
  "read_on_demand": [
    "docs/**/*.md",
    ".claude/context/*.md"
  ],
  "cache_aggressively": [
    "templates/**/*",
    ".opencode/skill/**/*.md"
  ],
  "skip": [
    "node_modules/**",
    "**/bin/**",
    "**/obj/**"
  ]
}
```

---

### 16. Multi-Project Support
**Status**: Pending
**Effort**: High

Enhance commands to work with multiple projects in `projects/`:

```bash
# /switch command
/switch ProjectA  # Set context to ProjectA
/switch ProjectB  # Set context to ProjectB

# Project-aware commands
/implement ACF-001 --project ProjectA
```

---

### 17. Refactoring Assistant
**Status**: Pending
**Effort**: High

Create `/refactor` command with safety checks:

```markdown
# /refactor <pattern> [scope]

1. Identify all usages
2. Create backup branch
3. Apply refactoring
4. Run tests
5. Show diff summary
6. Rollback if tests fail
```

---

### 18. API Documentation Generator
**Status**: Pending
**Effort**: Medium

Create `/generate-docs` command:

```bash
# Generate OpenAPI documentation
dotnet swagger tofile output.json

# Generate API reference
/generate-docs api

# Generate module documentation
/generate-docs modules
```

---

## Implementation Roadmap

### Phase 1: Foundation (Current Sprint)
- [x] CLAUDE.md
- [x] .claude/settings.json
- [x] Core slash commands
- [x] Basic hooks
- [ ] Context files (#1)
- [ ] File discovery map (#5)

### Phase 2: Developer Experience (Next Sprint)
- [ ] Test templates (#2)
- [ ] Enhanced story wizard (#3)
- [ ] Skill-to-command migration (#6)
- [ ] Coverage command (#7)

### Phase 3: Quality & Governance (Sprint +2)
- [ ] Pre-commit hooks (#4)
- [ ] Error patterns (#10)
- [ ] ADR templates (#12)
- [ ] Health dashboard (#14)

### Phase 4: Advanced Features (Sprint +3)
- [ ] Dependency graph (#8)
- [ ] Sprint automation (#11)
- [ ] Multi-project support (#16)
- [ ] Refactoring assistant (#17)

---

## Contributing

To add a new improvement:

1. Add entry to this document with:
   - Clear description
   - Priority level
   - Effort estimate
   - Implementation details

2. Create implementation in appropriate location:
   - Commands: `.claude/commands/<name>.md`
   - Hooks: `.claude/hooks/<name>.sh`
   - Templates: `.claude/templates/<name>`
   - Context: `.claude/context/<name>.md`

3. Update CLAUDE.md if it affects core behavior

4. Test with Claude Code in this repository

---

## Metrics to Track

| Metric | Current | Target | Notes |
|--------|---------|--------|-------|
| Commands available | 10 | 20 | Core workflow commands |
| Hook coverage | 3 | 10 | Pre/post execution validation |
| Context files | 0 | 5 | Cached knowledge base |
| Error patterns | 0 | 50 | Common issues and solutions |
| Template files | 0 | 10 | Code generation templates |

---

*Last updated: January 2025*
*Next review: End of current sprint*
