# Frontend Specification: {FEATURE_NAME}

**Version**: 1.0
**Date**: {DATE}
**Designer**: {Author}
**Status**: Draft | Review | Approved

---

## Overview

{1-2 sentences: What UI/UX is being specified?}

---

## User Context

### Target Users

| Persona | Device | Context |
|---------|--------|---------|
| {Persona 1} | Desktop/Mobile/Both | {When/where they use this} |
| {Persona 2} | Desktop/Mobile/Both | {When/where they use this} |

### User Goals

1. {Primary goal - what they want to accomplish}
2. {Secondary goal}
3. {Tertiary goal}

### Entry Points

How do users arrive at this feature?

- [ ] Direct navigation (sidebar/menu)
- [ ] Deep link (URL)
- [ ] From another feature (which one?)
- [ ] Notification/email link
- [ ] Search result

---

## Information Architecture

### Page Structure

```
┌─────────────────────────────────────────────────────────┐
│  Header / Navigation                                     │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│   Sidebar    │              Main Content                │
│  (optional)  │                                          │
│              │  ┌────────────────────────────────────┐  │
│              │  │         Content Area               │  │
│              │  │                                    │  │
│              │  └────────────────────────────────────┘  │
│              │                                          │
├──────────────┴──────────────────────────────────────────┤
│  Footer (optional)                                       │
└─────────────────────────────────────────────────────────┘
```

### Content Hierarchy

1. **Primary**: {Most important element - what user sees first}
2. **Secondary**: {Supporting information}
3. **Tertiary**: {Actions, metadata, less important info}

---

## Screens / Views

### Screen 1: {Screen Name}

**URL**: `/path/to/screen`
**Purpose**: {What this screen does}

#### Layout (ASCII Wireframe)

```
┌─────────────────────────────────────────────┐
│  ← Back        Page Title          [Action] │
├─────────────────────────────────────────────┤
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │  Section Header                     │    │
│  ├─────────────────────────────────────┤    │
│  │                                     │    │
│  │  [Card 1]  [Card 2]  [Card 3]      │    │
│  │                                     │    │
│  └─────────────────────────────────────┘    │
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │  Another Section                    │    │
│  │  - List item 1                      │    │
│  │  - List item 2                      │    │
│  └─────────────────────────────────────┘    │
│                                             │
│            [Primary Action Button]          │
│                                             │
└─────────────────────────────────────────────┘
```

#### Components

| Component | Type | Data | Actions |
|-----------|------|------|---------|
| Page Header | Layout | Title, breadcrumb | Back navigation |
| {Component 1} | Card/List/Form | {Data displayed} | {Interactions} |
| {Component 2} | Card/List/Form | {Data displayed} | {Interactions} |
| Action Button | Button | - | {What it does} |

#### States

| State | Condition | Display |
|-------|-----------|---------|
| Loading | Data fetching | Skeleton/spinner |
| Empty | No data | Empty state message + CTA |
| Error | API failure | Error message + retry |
| Success | Action completed | Toast/redirect |
| Partial | Some data loaded | Progressive display |

---

### Screen 2: {Screen Name}

**URL**: `/path/to/screen/:id`
**Purpose**: {What this screen does}

#### Layout

```
┌─────────────────────────────────────────────┐
│  ...                                        │
└─────────────────────────────────────────────┘
```

#### Components

| Component | Type | Data | Actions |
|-----------|------|------|---------|
| ... | ... | ... | ... |

---

## User Flows

### Flow 1: {Primary Flow Name}

```
[Start]
    │
    ▼
┌─────────────┐
│  Screen A   │
│  View List  │
└──────┬──────┘
       │ Click item
       ▼
┌─────────────┐
│  Screen B   │
│  View Detail│
└──────┬──────┘
       │ Click edit
       ▼
┌─────────────┐     ┌─────────────┐
│  Screen C   │     │   Toast:    │
│  Edit Form  │────▶│  "Saved!"   │
└─────────────┘     └─────────────┘
   Save              Success
```

### Flow 2: {Error Flow Name}

```
[Action]
    │
    ▼
┌─────────────┐
│  API Call   │
└──────┬──────┘
       │ Error
       ▼
┌─────────────────────────────┐
│  Error State                │
│  "Something went wrong"     │
│  [Retry] [Go Back]          │
└─────────────────────────────┘
```

---

## Component Specifications

### Component: {ComponentName}

**Type**: Card | Form | List | Modal | etc.
**Reusable**: Yes/No

#### Visual Design

```
┌──────────────────────────────────────┐
│  [Image/Icon]                        │
│                                      │
│  Title Text                          │
│  Subtitle or description text that   │
│  can wrap to multiple lines          │
│                                      │
│  Meta: value    Meta: value          │
│                                      │
│  [Secondary]         [Primary]       │
└──────────────────────────────────────┘
```

#### Props/Inputs

| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| title | string | Yes | - | Main heading |
| description | string | No | - | Supporting text |
| image | string | No | placeholder | Image URL |
| onAction | function | Yes | - | Primary action handler |

#### Variants

| Variant | When to Use |
|---------|-------------|
| Default | Standard display |
| Compact | List views, less space |
| Featured | Highlighted items |

#### Accessibility

- [ ] Keyboard navigable
- [ ] Screen reader labels
- [ ] Focus indicators
- [ ] Color contrast (4.5:1)

---

## Forms

### Form: {FormName}

**Purpose**: {What this form collects}

#### Fields

| Field | Type | Required | Validation | Help Text |
|-------|------|----------|------------|-----------|
| name | text | Yes | Min 2 chars | Your display name |
| email | email | Yes | Valid email | We'll never share this |
| description | textarea | No | Max 500 chars | Optional details |
| category | select | Yes | From list | Choose one |

#### Validation

| Rule | Message | When Shown |
|------|---------|------------|
| Required | "{Field} is required" | On blur, on submit |
| Format | "Please enter a valid {field}" | On blur |
| Length | "{Field} must be at least X characters" | On change |

#### Form States

```
Initial → Touched → Valid/Invalid → Submitting → Success/Error
```

#### Submit Behavior

- **Button text**: "Save" → "Saving..." → "Saved!"
- **On success**: {Toast message, redirect, etc.}
- **On error**: {Inline errors, toast, etc.}

---

## Responsive Behavior

### Breakpoints

| Breakpoint | Width | Layout Changes |
|------------|-------|----------------|
| Mobile | <640px | Single column, stacked |
| Tablet | 640-1024px | 2 columns, collapsible sidebar |
| Desktop | >1024px | Full layout |

### Mobile Adaptations

| Desktop | Mobile |
|---------|--------|
| Side-by-side cards | Stacked cards |
| Table | Card list |
| Sidebar visible | Hamburger menu |
| Hover states | Tap/long-press |

---

## Interactions

### Micro-interactions

| Trigger | Animation | Duration |
|---------|-----------|----------|
| Button hover | Scale 1.02, shadow | 150ms |
| Card hover | Lift shadow | 200ms |
| Page transition | Fade | 300ms |
| Toast appear | Slide up + fade | 200ms |
| Skeleton | Pulse | 1.5s loop |

### Loading States

| Component | Loading State |
|-----------|---------------|
| Page | Full skeleton |
| Card | Card skeleton |
| Button | Spinner + disabled |
| List | 3-5 skeleton items |

---

## Error Handling

### Error Types

| Error | User Message | Action |
|-------|--------------|--------|
| Network | "Connection lost. Check your internet." | Retry button |
| 404 | "Page not found" | Go home link |
| 403 | "You don't have access" | Request access / Go back |
| 500 | "Something went wrong" | Retry + Contact support |
| Validation | Specific field error | Highlight field |

### Empty States

| State | Message | CTA |
|-------|---------|-----|
| No data yet | "No {items} yet" | "Create your first {item}" |
| No results | "No results for '{query}'" | "Clear filters" |
| No permission | "Nothing to see here" | - |

---

## Accessibility (a11y)

### Requirements

- [ ] All images have alt text
- [ ] Form fields have labels
- [ ] Error messages linked to fields
- [ ] Skip to main content link
- [ ] Focus visible on all interactive elements
- [ ] No keyboard traps
- [ ] Color not sole indicator

### ARIA Labels

| Element | aria-label |
|---------|------------|
| Search input | "Search {items}" |
| Close button | "Close dialog" |
| Menu toggle | "Toggle navigation menu" |

---

## Content Guidelines

### Tone

- Friendly but professional
- Action-oriented
- Clear and concise

### Microcopy

| Element | Copy | Notes |
|---------|------|-------|
| Empty state title | "No spots yet" | Friendly, not blaming |
| Empty state CTA | "Add your first spot" | Action-oriented |
| Error message | "Couldn't save. Please try again." | Helpful, not technical |
| Success message | "Spot saved!" | Brief, positive |

---

## Technical Notes

### Data Requirements

| Screen | API Endpoint | Data Shape |
|--------|--------------|------------|
| List | GET /api/{items} | `{ items: [], total: number }` |
| Detail | GET /api/{items}/:id | `{ ...item }` |
| Create | POST /api/{items} | `{ id: string }` |

### Performance Targets

| Metric | Target |
|--------|--------|
| First Contentful Paint | <1.5s |
| Time to Interactive | <3s |
| Layout Shift | <0.1 |

### Dependencies

- shadcn/ui components
- TanStack Query for data fetching
- react-hook-form for forms
- zod for validation

---

## Open Questions

- [ ] {Question 1}
- [ ] {Question 2}

---

## Changelog

| Date | Version | Changes |
|------|---------|---------|
| {date} | 1.0 | Initial spec |
