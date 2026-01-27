/**
 * Calendar day cell with drop target (ACF-015 Phase 4)
 */

'use client';

import { useDrop } from 'react-dnd';
import { format, isSameDay, isToday, isPast } from 'date-fns';
import { Plus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui';
import { DraggableContentItem } from './DraggableContentItem';
import type { CalendarDay, DraggedItem, DropResult } from '@/types';

interface CalendarDayCellProps {
  day: CalendarDay;
  isCurrentMonth: boolean;
  onDrop: (result: DropResult) => void;
  onQuickAdd: (date: string) => void;
}

export function CalendarDayCell({
  day,
  isCurrentMonth,
  onDrop,
  onQuickAdd,
}: CalendarDayCellProps) {
  const date = new Date(day.date);
  const isDateToday = isToday(date);
  const isDatePast = isPast(date) && !isToday(date);

  const [{ isOver, canDrop }, drop] = useDrop<
    DraggedItem,
    DropResult,
    { isOver: boolean; canDrop: boolean }
  >({
    accept: 'CONTENT_ITEM',
    drop: (item: DraggedItem) => {
      const result: DropResult = {
        contentId: item.contentId,
        targetDate: day.date,
      };
      onDrop(result);
      return result;
    },
    canDrop: (item) => {
      // Don't allow dropping on the same date
      if (item.sourceDate && isSameDay(new Date(item.sourceDate), date)) {
        return false;
      }
      return true;
    },
    collect: (monitor) => ({
      isOver: monitor.isOver(),
      canDrop: monitor.canDrop(),
    }),
  });

  return (
    <div
      ref={drop}
      className={cn(
        'min-h-[120px] border-b border-r p-2',
        !isCurrentMonth && 'bg-muted/50',
        isDateToday && 'bg-primary/5',
        isOver && canDrop && 'bg-primary/10 ring-2 ring-primary',
        isOver && !canDrop && 'bg-destructive/10'
      )}
    >
      <div className="mb-2 flex items-center justify-between">
        <span
          className={cn(
            'text-sm font-medium',
            isDateToday && 'text-primary',
            isDatePast && 'text-muted-foreground',
            !isCurrentMonth && 'text-muted-foreground'
          )}
        >
          {format(date, 'd')}
        </span>
        {isCurrentMonth && (
          <Button
            variant="ghost"
            size="icon"
            className="h-6 w-6"
            onClick={() => onQuickAdd(day.date)}
            aria-label={`Add content for ${format(date, 'MMMM d')}`}
          >
            <Plus className="h-3 w-3" />
          </Button>
        )}
      </div>

      <div className="space-y-1">
        {day.items.map((item) => (
          <DraggableContentItem
            key={item.id}
            item={item}
            sourceDate={day.date}
          />
        ))}
      </div>

      {isOver && !canDrop && (
        <div className="mt-2 text-[10px] text-destructive">
          Already at this date
        </div>
      )}
    </div>
  );
}
