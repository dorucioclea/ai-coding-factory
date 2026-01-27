'use client';

import { useState, useCallback } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import {
  CalendarGrid,
  CalendarHeader,
  ViewToggle,
  QuickAddModal,
} from '@/components/calendar';
import { useCalendarMonth, useUpdateSchedule } from '@/hooks';
import { Skeleton } from '@/components/ui';
import type { CalendarViewMode, DropResult } from '@/types';

export default function CalendarContent() {
  // Use lazy initialization to avoid SSR issues with date
  const [year, setYear] = useState(() => new Date().getFullYear());
  const [month, setMonth] = useState(() => new Date().getMonth() + 1);
  const [view, setView] = useState<CalendarViewMode>('month');
  const [quickAddDate, setQuickAddDate] = useState<string | null>(null);

  // Fetch calendar data
  const { data: calendarData, isLoading, error } = useCalendarMonth(year, month);
  const updateScheduleMutation = useUpdateSchedule();

  // Navigation handler
  const handleNavigate = useCallback((newYear: number, newMonth: number) => {
    setYear(newYear);
    setMonth(newMonth);
  }, []);

  // View change handler
  const handleViewChange = useCallback((newView: CalendarViewMode) => {
    setView(newView);
  }, []);

  // Drop handler for drag and drop
  const handleDrop = useCallback(
    (result: DropResult) => {
      updateScheduleMutation.mutate({
        contentId: result.contentId,
        scheduledDate: result.targetDate,
      });
    },
    [updateScheduleMutation]
  );

  // Quick add handler
  const handleQuickAdd = useCallback((date: string) => {
    setQuickAddDate(date);
  }, []);

  if (error) {
    return (
      <div className="flex h-[calc(100vh-4rem)] items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-semibold text-destructive">
            Failed to load calendar
          </h2>
          <p className="mt-2 text-muted-foreground">
            {error instanceof Error ? error.message : 'Please try again'}
          </p>
        </div>
      </div>
    );
  }

  return (
    <DndProvider backend={HTML5Backend}>
      <div className="container mx-auto p-6">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Content Calendar</h1>
            <p className="mt-1 text-muted-foreground">
              Schedule and organize your content
            </p>
          </div>
          <ViewToggle view={view} onViewChange={handleViewChange} />
        </div>

        {/* Calendar Navigation */}
        {isLoading ? (
          <Skeleton className="h-12 w-full" />
        ) : (
          <CalendarHeader
            year={year}
            month={month}
            onNavigate={handleNavigate}
          />
        )}

        {/* Calendar Grid */}
        {isLoading ? (
          <div className="mt-4 space-y-2">
            <Skeleton className="h-12 w-full" />
            <div className="grid grid-cols-7 gap-2">
              {Array.from({ length: 35 }).map((_, i) => (
                <Skeleton key={i} className="h-32" />
              ))}
            </div>
          </div>
        ) : calendarData ? (
          <CalendarGrid
            calendarData={calendarData}
            view={view}
            onDrop={handleDrop}
            onQuickAdd={handleQuickAdd}
          />
        ) : null}

        {/* Legend */}
        <div className="mt-6 rounded-lg border p-4">
          <h3 className="mb-3 font-semibold">Status Legend</h3>
          <div className="flex flex-wrap gap-4 text-sm">
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border-l-4 border-gray-300 bg-gray-100" />
              <span>Idea</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border-l-4 border-yellow-300 bg-yellow-100" />
              <span>Draft</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border-l-4 border-orange-300 bg-orange-100" />
              <span>In Review</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border-l-4 border-blue-300 bg-blue-100" />
              <span>Scheduled</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border-l-4 border-green-300 bg-green-100" />
              <span>Published</span>
            </div>
          </div>
          <p className="mt-3 text-xs text-muted-foreground">
            Drag and drop content items to reschedule them. Click the + button to
            quickly add new content.
          </p>
        </div>

        {/* Quick Add Modal */}
        <QuickAddModal
          open={quickAddDate !== null}
          onOpenChange={(open) => !open && setQuickAddDate(null)}
          date={quickAddDate ?? ''}
        />
      </div>
    </DndProvider>
  );
}
