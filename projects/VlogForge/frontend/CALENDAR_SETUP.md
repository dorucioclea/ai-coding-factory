# Calendar Feature Setup (ACF-015 Phase 4)

## Dependencies Required

The calendar feature uses `react-dnd` for drag-and-drop functionality. Install the required dependencies:

```bash
npm install react-dnd react-dnd-html5-backend
```

Or with yarn:

```bash
yarn add react-dnd react-dnd-html5-backend
```

Or with pnpm:

```bash
pnpm add react-dnd react-dnd-html5-backend
```

## Features Implemented

### 1. Calendar Types (`src/types/calendar.ts`)
- CalendarDay, CalendarMonth interfaces
- UpdateScheduleRequest interface
- CalendarViewMode (month/week)
- Drag and drop type definitions

### 2. Calendar Query Keys (`src/lib/query-client.ts`)
- `calendar.all` - All calendar queries
- `calendar.months()` - All month queries
- `calendar.month(year, month)` - Specific month query

### 3. Calendar Hooks (`src/hooks/use-calendar.ts`)
- `useCalendarMonth(year, month)` - Fetch calendar data for a specific month
- `useUpdateSchedule()` - Update content scheduled date with optimistic updates
- `useUnschedule()` - Remove scheduled date from content

### 4. Calendar Components (`src/components/calendar/`)

#### CalendarHeader
- Month/year display
- Previous/next month navigation
- "Today" button to jump to current month

#### ViewToggle
- Switch between month and week views
- Visual indicator for active view

#### DraggableContentItem
- Drag-enabled content card
- Color-coded by status (Idea=gray, Draft=yellow, Review=orange, Scheduled=blue, Published=green)
- Displays title and platform tags
- Shows opacity while dragging

#### CalendarDayCell
- Drop target for dragged items
- Visual feedback on hover (valid/invalid drop)
- Quick add button (+) for empty dates
- Day number display with current day highlighting
- Past dates shown in muted colors

#### CalendarGrid
- Month/week grid layout
- Weekday headers
- Handles date calculations
- Maps content items to calendar days

#### QuickAddModal
- Fast content creation dialog
- Pre-filled scheduled date
- Title and notes fields
- Creates content and schedules in one step

### 5. Calendar Page (`src/app/dashboard/calendar/page.tsx`)
- DndProvider wrapper for drag and drop
- Month/week view toggle
- Calendar navigation
- Drag and drop scheduling with optimistic updates
- Status color legend
- Loading and error states
- Quick add functionality

### 6. Sidebar Navigation Updated
- Added Calendar menu item with Calendar icon
- Positioned between Dashboard and Analytics

## Usage

### Navigate to Calendar
Access the calendar at `/dashboard/calendar`

### Drag and Drop
1. Click and drag any content item
2. Drop it on a different date to reschedule
3. Optimistic UI update with automatic rollback on error
4. Toast notifications for success/error

### Quick Add
1. Click the + button on any date cell
2. Enter content title and optional notes
3. Content is created and scheduled for that date

### Navigation
- Use arrow buttons to navigate between months
- Click "Today" to return to current month
- Toggle between month and week views

## Backend API Requirements

The calendar feature expects these backend endpoints:

### GET /api/calendar
Query params: `month=YYYY-MM`

Response:
```json
{
  "year": 2026,
  "month": 1,
  "days": [
    {
      "date": "2026-01-15",
      "items": [ContentIdeaResponse...]
    }
  ]
}
```

### PATCH /api/content/{id}/schedule
Request body:
```json
{
  "scheduledDate": "2026-01-15"
}
```

Response: `ContentIdeaResponse`

## Styling Notes

### Status Colors
- **Idea**: Gray (bg-gray-100, border-gray-300)
- **Draft**: Yellow (bg-yellow-100, border-yellow-300)
- **In Review**: Orange (bg-orange-100, border-orange-300)
- **Scheduled**: Blue (bg-blue-100, border-blue-300)
- **Published**: Green (bg-green-100, border-green-300)

Dark mode variants are included for all colors.

### Layout
- Grid: 7 columns (one per weekday)
- Cell min-height: 120px
- Border-left colored by status (4px)
- Responsive design with proper spacing

## Immutability Patterns

All state updates follow immutability principles:

```typescript
// Optimistic update example from useUpdateSchedule
days: old.days.map((day) => ({
  ...day,
  items: day.items.map((item) =>
    item.id === contentId
      ? { ...item, scheduledDate }
      : item
  ),
}))
```

No mutations - always create new objects.

## Error Handling

- Loading states with Skeleton components
- Error boundary with user-friendly messages
- Toast notifications for mutations
- Optimistic updates with automatic rollback
- Validation for drag-drop (can't drop on same date)

## Performance Optimizations

- Query stale time: 2 minutes
- Memoized callbacks (useCallback)
- Optimistic UI updates
- Efficient date calculations with date-fns
- Query key invalidation on mutations

## Testing Considerations

When writing tests for the calendar:

1. Mock `react-dnd` hooks (useDrag, useDrop)
2. Mock date-fns functions for consistent test dates
3. Test optimistic updates and rollback
4. Test drag validation (same date rejection)
5. Test quick add modal form submission
6. Test navigation (prev/next month, today)
7. Test view toggle (month/week)

## Future Enhancements

Potential improvements for future phases:

- Week view implementation (currently shows first week only)
- Recurring events
- Bulk scheduling
- Calendar export (iCal format)
- Time-of-day scheduling
- Drag from external source (unscheduled items)
- Keyboard navigation
- Multi-select and bulk operations
