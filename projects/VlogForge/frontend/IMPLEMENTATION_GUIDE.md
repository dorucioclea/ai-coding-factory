# Calendar Implementation Guide (ACF-015 Phase 4)

## Quick Start

### 1. Install Dependencies

```bash
npm install react-dnd react-dnd-html5-backend
```

### 2. Verify Installation

```bash
npm run type-check
```

All calendar-related type errors should be resolved after installing react-dnd.

### 3. Start Development Server

```bash
npm run dev
```

### 4. Navigate to Calendar

Open browser and go to: `http://localhost:3000/dashboard/calendar`

## Component Architecture

```
CalendarPage (page.tsx)
├── DndProvider (react-dnd)
│   ├── CalendarHeader
│   │   ├── Navigation buttons (prev/next)
│   │   └── Today button
│   ├── ViewToggle
│   │   ├── Month button
│   │   └── Week button
│   └── CalendarGrid
│       ├── Weekday headers
│       └── CalendarDayCell (×35 for full month)
│           ├── Day number
│           ├── Quick add button (+)
│           └── DraggableContentItem (×N per day)
└── QuickAddModal
    └── Form (title, notes)
```

## Data Flow

### Fetching Calendar Data

```typescript
// Hook automatically fetches when year/month changes
const { data, isLoading, error } = useCalendarMonth(year, month);

// Data structure:
{
  year: 2026,
  month: 1,
  days: [
    {
      date: "2026-01-15",
      items: [
        {
          id: "uuid",
          title: "Video about React",
          status: IdeaStatus.Scheduled,
          platformTags: ["YouTube"],
          scheduledDate: "2026-01-15",
          // ... other fields
        }
      ]
    }
  ]
}
```

### Updating Schedule (Drag & Drop)

```typescript
const updateSchedule = useUpdateSchedule();

// Triggered by drop event
const handleDrop = (result: DropResult) => {
  updateSchedule.mutate({
    contentId: result.contentId,
    targetDate: result.targetDate,
  });
};

// Optimistic update happens automatically
// Rollback on error
// Toast notification on success/error
```

### Quick Add

```typescript
const createMutation = useCreateIdea();

// When user clicks + on a date
const request: CreateContentIdeaRequest = {
  title: "New Content",
  notes: "Optional notes",
  platformTags: [],
  scheduledDate: "2026-01-15", // Pre-filled from clicked date
};

await createMutation.mutateAsync(request);
// Calendar automatically updates via query invalidation
```

## Customization

### Status Colors

Edit `/src/components/calendar/DraggableContentItem.tsx`:

```typescript
const STATUS_COLORS = {
  gray: 'bg-gray-100 border-gray-300 dark:bg-gray-800 dark:border-gray-600',
  yellow: 'bg-yellow-100 border-yellow-300 dark:bg-yellow-900 dark:border-yellow-600',
  // ... add more or modify existing
};
```

### Cell Height

Edit `/src/components/calendar/CalendarDayCell.tsx`:

```typescript
<div
  className="min-h-[120px]" // Change this value
  // ...
>
```

### View Mode Logic

Edit `/src/components/calendar/CalendarGrid.tsx`:

```typescript
// Current: week view shows first 7 days
const displayDays = view === 'week' ? days.slice(0, 7) : days;

// To show current week:
const displayDays = view === 'week'
  ? days.filter(d => isSameWeek(d, new Date()))
  : days;
```

## Styling Tokens

All colors use Tailwind/shadcn tokens:

- `bg-primary` - Primary brand color
- `bg-muted` - Muted backgrounds
- `text-muted-foreground` - Muted text
- `border` - Default border color
- `bg-destructive` - Error states

Dark mode is handled automatically via `dark:` variants.

## Error Handling

### Loading State
```typescript
if (isLoading) {
  return <Skeleton />; // Shows loading skeleton
}
```

### Error State
```typescript
if (error) {
  return <ErrorMessage />; // Shows friendly error
}
```

### Mutation Errors
- Optimistic updates rollback automatically
- Toast notifications show error message
- User can retry operation

## Accessibility

### Keyboard Navigation
- Tab through interactive elements
- Enter/Space to activate buttons
- Focus visible on all interactive elements

### ARIA Labels
```typescript
<Button aria-label="Previous month" />
<Button aria-label={`Add content for ${format(date, 'MMMM d')}`} />
<Button aria-pressed={view === 'month'} />
```

### Screen Reader Support
- Semantic HTML structure
- Proper heading hierarchy
- Descriptive button labels

## Performance

### Query Optimization
```typescript
// Stale time: 2 minutes
staleTime: 1000 * 60 * 2,

// Cache time: 30 minutes (from global config)
gcTime: 1000 * 60 * 30,
```

### Memoization
```typescript
const handleDrop = useCallback((result: DropResult) => {
  // Prevents re-render on parent update
}, [updateScheduleMutation]);
```

### Date Calculations
Uses `date-fns` for efficient date operations:
- `startOfMonth`, `endOfMonth`
- `startOfWeek`, `endOfWeek`
- `eachDayOfInterval`
- `format`, `isSameDay`, `isToday`

## Testing Strategy

### Unit Tests
```typescript
describe('useCalendarMonth', () => {
  it('fetches calendar data for specific month', async () => {
    // Test hook functionality
  });

  it('handles loading and error states', async () => {
    // Test error handling
  });
});
```

### Component Tests
```typescript
describe('CalendarDayCell', () => {
  it('renders day number and items', () => {
    // Test rendering
  });

  it('handles drop events', async () => {
    // Test drag and drop
  });

  it('shows quick add button on hover', () => {
    // Test interaction
  });
});
```

### E2E Tests
```typescript
test('schedule content via drag and drop', async ({ page }) => {
  await page.goto('/dashboard/calendar');

  // Drag item
  const item = page.locator('[data-content-id="123"]');
  const target = page.locator('[data-date="2026-01-15"]');
  await item.dragTo(target);

  // Verify update
  await expect(target).toContainText('Video about React');
});
```

## Troubleshooting

### react-dnd not working
1. Verify packages installed: `npm list react-dnd`
2. Check DndProvider wraps calendar: `<DndProvider backend={HTML5Backend}>`
3. Verify backend import: `import { HTML5Backend } from 'react-dnd-html5-backend'`

### Dates showing incorrectly
1. Check timezone: calendar uses local timezone
2. Verify date format: backend should send ISO format "YYYY-MM-DD"
3. Check date-fns locale: default is English

### Optimistic update not working
1. Check query key structure matches
2. Verify mutation onMutate returns snapshot
3. Check onError receives snapshot context

### Drag drop validation failing
1. Verify sourceDate is passed to DraggableContentItem
2. Check canDrop logic in CalendarDayCell
3. Ensure dates are compared correctly (use isSameDay)

## Backend Integration Checklist

- [ ] GET /api/calendar endpoint implemented
- [ ] Query param `?month=YYYY-MM` supported
- [ ] Returns CalendarMonth structure
- [ ] PATCH /api/content/{id}/schedule endpoint implemented
- [ ] Accepts UpdateScheduleRequest body
- [ ] Returns updated ContentIdeaResponse
- [ ] Handles null scheduledDate (unschedule)
- [ ] Validates date format
- [ ] Handles timezone correctly
- [ ] Returns proper error messages

## Deployment Checklist

- [ ] react-dnd packages in dependencies
- [ ] TypeScript compiles without errors
- [ ] All tests passing (80%+ coverage)
- [ ] Backend API endpoints deployed
- [ ] Environment variables configured
- [ ] Error tracking configured (Sentry, etc.)
- [ ] Performance monitoring enabled
- [ ] Accessibility audit passed
- [ ] Cross-browser testing completed
- [ ] Mobile responsiveness verified

## Support

For issues or questions:
1. Check CALENDAR_SETUP.md for detailed setup
2. Review ACF-015-PHASE-4-SUMMARY.md for architecture
3. Check backend API documentation
4. Review React DnD documentation: https://react-dnd.github.io/react-dnd/

## Next Phase: Team Management (ACF-015 Phase 5)

After calendar is working:
1. Implement team creation and management
2. Add member invitations
3. Integrate team context into calendar
4. Add team-wide calendar view
5. Implement role-based permissions
