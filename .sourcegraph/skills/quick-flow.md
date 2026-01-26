# Quick Flow

## Overview

Not everything needs a full specification. Quick Flow is the fast track for changes that are:
- Well-defined and small in scope
- Low risk
- Easily reversible
- Don't affect architecture

## When to Use

### ✅ Good for Quick Flow

| Change Type | Example |
|-------------|---------|
| Bug fix | "Fix the null check in UserService" |
| Config change | "Update the timeout to 30 seconds" |
| Text update | "Change button text to 'Submit'" |
| Style tweak | "Make the header font larger" |
| Add field | "Add email to the user form" |
| Simple endpoint | "Add GET endpoint for user count" |

### ❌ Use Full Spec Instead

| Change Type | Why |
|-------------|-----|
| New feature with UI | Multiple components, user flows |
| Database schema change | Migration, data integrity |
| Authentication changes | Security implications |
| API contract changes | Breaking changes possible |
| Cross-cutting concerns | Affects multiple areas |

## The Quick Flow Process

```
Describe → Confirm → Implement → Test → Commit
   │          │          │         │        │
   └──────────┴──────────┴─────────┴────────┘
                   10-30 minutes
```

### Step 1: Describe (30 seconds)

User describes the change in 1-2 sentences.

```
"Add a 'last login' timestamp to the user profile page"
```

### Step 2: Confirm (1 minute)

Claude confirms understanding and scope:

```markdown
**Quick Flow Change**

**What**: Display last login timestamp on profile
**Where**: ProfilePage component + User API
**Impact**: Display only, no data changes needed

**Plan**:
1. Add `lastLogin` to User DTO (already in DB)
2. Display in ProfilePage below email
3. Format as "Last login: Jan 23, 2024 at 3:45 PM"

Proceed? [Y/n]
```

### Step 3: Implement (5-20 minutes)

Implement the change directly:
- Write the code
- Follow existing patterns
- Keep changes minimal

### Step 4: Test (2-5 minutes)

Verify the change works:
- Manual verification
- Run related tests
- Check for regressions

### Step 5: Commit (1 minute)

```bash
git add .
git commit -m "Add last login display to profile page

- Added lastLogin to UserDto
- Display formatted timestamp in ProfilePage
- No migration needed (field already exists)"
```

## Quick Flow Rules

### Do

- ✅ Keep changes small and focused
- ✅ Follow existing patterns exactly
- ✅ Test before committing
- ✅ Use clear commit messages
- ✅ Ask for clarification if unsure

### Don't

- ❌ Refactor "while you're in there"
- ❌ Add features beyond the request
- ❌ Skip testing
- ❌ Make breaking changes
- ❌ Touch unrelated code

## Scope Check

Before starting, verify scope is appropriate:

```markdown
## Scope Checklist

- [ ] Change is well-defined (I know exactly what to do)
- [ ] Less than 5 files affected
- [ ] No database migrations needed
- [ ] No new dependencies
- [ ] No API contract changes
- [ ] Easily reversible if wrong
- [ ] Can complete in <30 minutes

**If any unchecked**: Consider using `spec-driven-development` instead
```

## Escalation

If during implementation you discover:
- The change is bigger than expected
- You need to modify the data model
- Multiple components are affected
- There are unclear requirements

**STOP and escalate**:

```
"This is bigger than a quick flow. I discovered we need to:
- Add a new database column
- Update 3 API endpoints
- Modify the auth middleware

Should I create a proper spec for this?"
```

## Examples

### Example 1: Bug Fix

```
User: "The save button is disabled when form is valid"

Claude:
**Quick Flow: Bug Fix**

**Issue**: Save button stays disabled with valid form
**Cause**: Checking `isValid` instead of `formState.isValid`
**Fix**: Update condition in SaveButton component

Implementing now...

[Makes change]

Fixed! The button now enables when form is valid.
Tested with valid and invalid inputs.

Commit: "Fix save button disabled state check"
```

### Example 2: Add Field

```
User: "Add phone number to contact form"

Claude:
**Quick Flow: Add Field**

**What**: Add phone field to contact form
**Where**: ContactForm.tsx, ContactDto.cs, CreateContactCommand.cs
**Validation**: Optional, format xxx-xxx-xxxx if provided

Plan:
1. Add field to frontend form
2. Add to DTO and command
3. Add FluentValidation rule (optional with format)

Proceed? [Y/n]
```

### Example 3: Escalation

```
User: "Add user roles to the system"

Claude:
This is too big for Quick Flow. User roles would require:
- New database tables (Roles, UserRoles)
- Migrations
- Authorization middleware changes
- UI for role management
- Multiple API endpoints

Should I create a spec for this feature instead?
```

## Integration

Quick Flow integrates with:
- `tdd` - Write test first even for small changes
- `verification-before-completion` - Always verify before commit
- `code-reviewer` - Optional quick review for non-trivial changes

## Commands

```bash
# Start quick flow explicitly
"quick flow: add loading spinner to save button"

# Or just describe small changes naturally
"fix the typo in the header"
"add the created date to the list view"
```