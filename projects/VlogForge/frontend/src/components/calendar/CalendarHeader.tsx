/**
 * Calendar header with month navigation (ACF-015 Phase 4)
 */

'use client';

import { ChevronLeft, ChevronRight } from 'lucide-react';
import { format, addMonths, subMonths, startOfToday } from 'date-fns';
import { Button } from '@/components/ui';

interface CalendarHeaderProps {
  year: number;
  month: number;
  onNavigate: (year: number, month: number) => void;
}

export function CalendarHeader({
  year,
  month,
  onNavigate,
}: CalendarHeaderProps) {
  const currentDate = new Date(year, month - 1, 1);

  const handlePrevMonth = () => {
    const prevMonth = subMonths(currentDate, 1);
    onNavigate(prevMonth.getFullYear(), prevMonth.getMonth() + 1);
  };

  const handleNextMonth = () => {
    const nextMonth = addMonths(currentDate, 1);
    onNavigate(nextMonth.getFullYear(), nextMonth.getMonth() + 1);
  };

  const handleToday = () => {
    const today = startOfToday();
    onNavigate(today.getFullYear(), today.getMonth() + 1);
  };

  return (
    <div className="flex items-center justify-between border-b pb-4">
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="icon"
          onClick={handlePrevMonth}
          aria-label="Previous month"
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <Button
          variant="outline"
          size="icon"
          onClick={handleNextMonth}
          aria-label="Next month"
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
        <h2 className="ml-2 text-2xl font-semibold">
          {format(currentDate, 'MMMM yyyy')}
        </h2>
      </div>
      <Button variant="outline" onClick={handleToday}>
        Today
      </Button>
    </div>
  );
}
