/**
 * Calendar grid layout (ACF-015 Phase 4)
 */

'use client';

import {
  startOfMonth,
  endOfMonth,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  format,
  isSameMonth,
} from 'date-fns';
import { CalendarDayCell } from './CalendarDayCell';
import type { CalendarMonth, CalendarViewMode, DropResult } from '@/types';

interface CalendarGridProps {
  calendarData: CalendarMonth;
  view: CalendarViewMode;
  onDrop: (result: DropResult) => void;
  onQuickAdd: (date: string) => void;
}

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export function CalendarGrid({
  calendarData,
  view,
  onDrop,
  onQuickAdd,
}: CalendarGridProps) {
  const currentDate = new Date(calendarData.year, calendarData.month - 1, 1);

  // Get days to display
  const monthStart = startOfMonth(currentDate);
  const monthEnd = endOfMonth(currentDate);
  const calendarStart = startOfWeek(monthStart);
  const calendarEnd = endOfWeek(monthEnd);

  const days = eachDayOfInterval({ start: calendarStart, end: calendarEnd });

  // Create a map for quick lookup of calendar data
  const dayMap = new Map(
    calendarData.days.map((day) => [day.date, day])
  );

  // Get day data or create empty
  const getDayData = (date: Date) => {
    const dateStr = format(date, 'yyyy-MM-dd');
    return (
      dayMap.get(dateStr) || {
        date: dateStr,
        items: [],
      }
    );
  };

  // Filter days based on view mode
  const displayDays = view === 'week' ? days.slice(0, 7) : days;

  return (
    <div className="mt-4 overflow-hidden rounded-lg border">
      {/* Weekday headers */}
      <div className="grid grid-cols-7 border-b bg-muted/50">
        {WEEKDAYS.map((day) => (
          <div
            key={day}
            className="border-r p-2 text-center text-sm font-semibold last:border-r-0"
          >
            {day}
          </div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7">
        {displayDays.map((date) => {
          const dayData = getDayData(date);
          const isCurrentMonth = isSameMonth(date, currentDate);

          return (
            <CalendarDayCell
              key={date.toISOString()}
              day={dayData}
              isCurrentMonth={isCurrentMonth}
              onDrop={onDrop}
              onQuickAdd={onQuickAdd}
            />
          );
        })}
      </div>
    </div>
  );
}
