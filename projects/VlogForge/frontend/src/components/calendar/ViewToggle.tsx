/**
 * Calendar view toggle (month/week) (ACF-015 Phase 4)
 */

'use client';

import { Calendar, CalendarDays } from 'lucide-react';
import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import type { CalendarViewMode } from '@/types';

interface ViewToggleProps {
  view: CalendarViewMode;
  onViewChange: (view: CalendarViewMode) => void;
}

export function ViewToggle({ view, onViewChange }: ViewToggleProps) {
  return (
    <div className="flex rounded-md border">
      <Button
        variant="ghost"
        size="sm"
        className={cn(
          'rounded-r-none',
          view === 'month' && 'bg-accent text-accent-foreground'
        )}
        onClick={() => onViewChange('month')}
        aria-pressed={view === 'month'}
      >
        <Calendar className="mr-2 h-4 w-4" />
        Month
      </Button>
      <Button
        variant="ghost"
        size="sm"
        className={cn(
          'rounded-l-none border-l',
          view === 'week' && 'bg-accent text-accent-foreground'
        )}
        onClick={() => onViewChange('week')}
        aria-pressed={view === 'week'}
      >
        <CalendarDays className="mr-2 h-4 w-4" />
        Week
      </Button>
    </div>
  );
}
