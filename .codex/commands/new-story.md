# /new-story - Create a New User Story

Create a new INVEST-compliant user story with interactive wizard and intelligent suggestions.

## Usage
```
/new-story [title] [options]
```

Options:
- `--quick` - Skip interactive prompts, use defaults
- `--epic <epic-id>` - Link to parent epic
- `--type <feature|bug|tech-debt|spike>` - Story type (default: feature)
- `--points <n>` - Set story points
- `--sprint <name>` - Assign to sprint

## Instructions

When invoked, use the Interactive Story Wizard:

### Phase 1: Story Identification

#### 1. Determine Next Story ID
```bash
# Find highest ACF number
LATEST=$(ls artifacts/stories/ACF-*.md 2>/dev/null | \
  sed 's/.*ACF-0*\([0-9]*\).*/\1/' | \
  sort -n | tail -1)
NEXT_ID=$(printf "ACF-%03d" $((LATEST + 1)))
```

#### 2. Analyze Existing Stories for Context
```bash
# Extract common personas
grep -h "As a" artifacts/stories/*.md | sort | uniq -c | sort -rn | head -5

# Extract common domains
grep -h "## Technical Notes" -A 5 artifacts/stories/*.md | \
  grep -oE "(Order|User|Product|Payment|Inventory)" | sort | uniq -c
```

### Phase 2: Interactive Wizard

#### Step 1: Story Type Selection

**Ask user to select story type:**

| Type | Description | Template Focus |
|------|-------------|----------------|
| 🆕 Feature | New functionality | User value, acceptance criteria |
| 🐛 Bug | Fix defect | Steps to reproduce, expected vs actual |
| 🔧 Tech Debt | Code improvement | Technical justification, impact |
| 🔬 Spike | Research/investigation | Questions to answer, timebox |

#### Step 2: Persona Selection (with suggestions)

**Present common personas from existing stories:**

```
Select or enter the user persona:

  Suggested (from existing stories):
  1. registered user (used in 12 stories)
  2. admin (used in 8 stories)
  3. guest (used in 5 stories)
  4. system administrator (used in 3 stories)

  Or enter a new persona: ___________
```

**Persona templates:**
- `registered user` → Full access to user features
- `admin` → Administrative capabilities
- `guest` → Limited, unauthenticated access
- `API consumer` → External system integration
- `support agent` → Customer service functions

#### Step 3: Action Definition (with verb suggestions)

**Suggest action verbs based on domain:**

```
What action does the user want to perform?

  Suggested verbs:
  • create, view, update, delete (CRUD)
  • search, filter, sort (Discovery)
  • submit, approve, reject (Workflow)
  • login, logout, reset password (Auth)
  • export, import, sync (Data)

  Complete the sentence: "I want to ___________"
```

#### Step 4: Business Value (with templates)

**Offer value proposition templates:**

```
What is the business value?

  Templates:
  1. "...so that I can complete my task faster"
  2. "...so that I have visibility into [X]"
  3. "...so that I can make informed decisions"
  4. "...so that the system remains secure"
  5. "...so that I can comply with [requirement]"

  Complete: "So that ___________"
```

#### Step 5: Acceptance Criteria Generator

**Auto-generate acceptance criteria based on action type:**

For CRUD operations, suggest:
```markdown
## Acceptance Criteria

### Create
- [ ] Given I am on the creation form, when I submit valid data, then the item is created
- [ ] Given I submit invalid data, when I click submit, then I see validation errors
- [ ] Given I create an item, when I view the list, then the new item appears

### Read
- [ ] Given I have permission, when I request the item, then I see all fields
- [ ] Given item doesn't exist, when I request it, then I see 404 error

### Update
- [ ] Given I own the item, when I update it, then changes are persisted
- [ ] Given I lack permission, when I try to update, then I see 403 error

### Delete
- [ ] Given I have permission, when I delete, then the item is removed
- [ ] Given item has dependencies, when I delete, then I see a warning
```

For Authentication operations:
```markdown
## Acceptance Criteria

### Login
- [ ] Given valid credentials, when I login, then I receive a JWT token
- [ ] Given invalid credentials, when I login, then I see an error message
- [ ] Given I'm already logged in, when I visit login, then I'm redirected

### Security
- [ ] Given 5 failed attempts, when I try again, then the account is locked
- [ ] Given locked account, when I request reset, then I receive an email
```

#### Step 6: Story Points Estimation (with similar story reference)

**Find similar stories and their points:**

```bash
# Find stories with similar keywords
grep -l "order\|create\|update" artifacts/stories/*.md | \
  xargs grep "points:" | head -5
```

```
Estimate story points:

  Similar completed stories:
  • ACF-032 "Create Product" - 3 points (2 days actual)
  • ACF-028 "Update User Profile" - 2 points (1 day actual)
  • ACF-019 "Order Management" - 5 points (4 days actual)

  Suggested: 3 points (based on similar stories)

  Enter points [1-13]: ___
```

#### Step 7: Related Items

**Auto-detect related stories:**

```bash
# Find stories with similar terms
grep -l "order\|customer" artifacts/stories/*.md | head -5
```

```
Potentially related items:

  Stories:
  • ACF-028: Customer Profile (same domain)
  • ACF-031: Order History (related feature)

  ADRs:
  • ADR-003: Database Selection (if persistence involved)

  Link any of these? [y/n]
```

### Phase 3: Story Generation

#### Create Story File

```markdown
---
id: {{STORY_ID}}
title: {{TITLE}}
type: {{TYPE}}
status: draft
created: {{DATE}}
author: Claude Code
sprint: backlog
points: {{POINTS}}
epic: {{EPIC_ID}}
priority: medium
---

# {{STORY_ID}} - {{TITLE}}

## User Story

As a **{{PERSONA}}**,
I want to **{{ACTION}}**,
So that **{{BENEFIT}}**.

## Story Type

{{TYPE_BADGE}} {{TYPE_DESCRIPTION}}

## Acceptance Criteria

{{#each ACCEPTANCE_CRITERIA}}
- [ ] Given {{given}}, when {{when}}, then {{then}}
{{/each}}

## Technical Notes

### Domain Impact
- **Entities**: {{AFFECTED_ENTITIES}}
- **Commands/Queries**: {{CQRS_IMPACT}}
- **API Endpoints**: {{API_ENDPOINTS}}

### Dependencies
{{#each DEPENDENCIES}}
- {{dependency}}
{{/each}}

### Out of Scope
- {{OUT_OF_SCOPE_ITEMS}}

## Definition of Ready

- [x] Story follows INVEST criteria
- [x] Acceptance criteria are testable
- [ ] Dependencies identified and unblocked
- [ ] No blocking questions
- [ ] Estimated ({{POINTS}} points)

## Definition of Done

- [ ] All acceptance criteria met
- [ ] Unit tests with >80% coverage for Domain/Application
- [ ] Integration tests for API endpoints
- [ ] Code reviewed and approved
- [ ] Documentation updated
- [ ] Security review completed (if applicable)
- [ ] Traceability verified:
  - [ ] Tests tagged with `[Trait("Story", "{{STORY_ID}}")]`
  - [ ] Commits prefixed with `{{STORY_ID}}`

## Test Plan

| ID | Type | Description | Expected Result |
|----|------|-------------|-----------------|
{{#each TEST_CASES}}
| TC-{{@index}} | {{type}} | {{description}} | {{expected}} |
{{/each}}

## Mockups / Wireframes

{{#if HAS_UI}}
*Attach or describe UI mockups here*
{{else}}
*N/A - API only*
{{/if}}

## Related Items

### Stories
{{#each RELATED_STORIES}}
- [{{id}}]({{path}}): {{title}}
{{/each}}

### ADRs
{{#each RELATED_ADRS}}
- [{{id}}]({{path}}): {{title}}
{{/each}}

### External References
- {{EXTERNAL_REFS}}

## Notes

*Additional context or implementation notes*

---
*Created by Story Wizard - {{DATE}}*
```

### Phase 4: Post-Creation Actions

#### 1. Validate Story Quality

```
INVEST Validation:

✅ Independent: No blocking dependencies
✅ Negotiable: AC can be refined
✅ Valuable: Clear user benefit stated
✅ Estimable: {{POINTS}} points assigned
✅ Small: Fits in single sprint
✅ Testable: {{AC_COUNT}} testable criteria

Story Quality Score: 6/6 ✅
```

#### 2. Generate Test Stubs (optional)

```
Would you like me to generate test stubs for this story?

  Files to create:
  • tests/UnitTests/{{Feature}}Tests.cs
  • tests/IntegrationTests/{{Feature}}IntegrationTests.cs

  [y/n]
```

#### 3. Output Summary

```markdown
## Story Created Successfully

**Story ID**: {{STORY_ID}}
**File**: artifacts/stories/{{STORY_ID}}.md
**Type**: {{TYPE}}
**Points**: {{POINTS}}
**Status**: Draft

### Quick Reference

Use in tests:
```csharp
[Trait("Story", "{{STORY_ID}}")]
```

Use in commits:
```
{{STORY_ID}} <description>
```

### Next Steps

1. Review and refine acceptance criteria
2. Add to sprint: `/sprint plan`
3. Start implementation: `/implement {{STORY_ID}}`
```

## Example Session

```
User: /new-story

Claude: 🧙 Story Wizard - Let's create a new user story!

Next available ID: ACF-043

Step 1/6: What type of story is this?
  1. 🆕 Feature - New functionality
  2. 🐛 Bug - Fix a defect
  3. 🔧 Tech Debt - Code improvement
  4. 🔬 Spike - Research/investigation

User: 1

Claude: Step 2/6: Who is the user persona?

  Suggested personas (from your stories):
  1. registered user (12 stories)
  2. admin (8 stories)
  3. guest (5 stories)

  Or type a custom persona.

User: registered user

Claude: Step 3/6: What action do they want to perform?

  Suggested verbs: create, view, update, delete, search, filter

  "I want to _____"

User: view my order history

Claude: Step 4/6: What is the business value?

  Templates:
  1. "...so that I can track my purchases"
  2. "...so that I have visibility into my orders"
  3. Custom

User: 2

Claude: Step 5/6: Acceptance Criteria

  Auto-generated for "view" operation:

  - [ ] Given I am logged in, when I visit /orders, then I see my order list
  - [ ] Given I have no orders, when I visit /orders, then I see "No orders yet"
  - [ ] Given I click an order, when details load, then I see all line items

  Add more? [y/n]

User: n

Claude: Step 6/6: Story Points

  Similar stories:
  • ACF-028 "View Profile" - 2 points
  • ACF-035 "View Products" - 3 points

  Suggested: 2 points

  Enter points [1-13] or press Enter for suggested:

User: [Enter]

Claude: ✅ Story Created!

  ID:     ACF-043
  Title:  View Order History
  Type:   Feature
  Points: 2
  File:   artifacts/stories/ACF-043.md

  INVEST Score: 6/6 ✅

  Use in code:
    [Trait("Story", "ACF-043")]

  Use in commits:
    ACF-043 Implement order history view

  Would you like to:
  1. Generate test stubs
  2. Start implementation (/implement ACF-043)
  3. Done
```

```
User: /new-story Add payment method --quick --type feature --points 5

Claude: Creating story with quick mode...

✅ Created: ACF-044 - Add Payment Method

  Type:   Feature
  Points: 5
  File:   artifacts/stories/ACF-044.md

  Default acceptance criteria generated.
  Review and customize: artifacts/stories/ACF-044.md
```
