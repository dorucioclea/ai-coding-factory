/**
 * Calendar page for content scheduling (ACF-015 Phase 4)
 */

import { Suspense } from 'react';
import { Skeleton } from '@/components/ui';
import CalendarContent from './CalendarContent';

export default function CalendarPage() {
  return (
    <Suspense
      fallback={
        <div className="container mx-auto p-6">
          <div className="mb-6">
            <Skeleton className="h-10 w-48" />
            <Skeleton className="mt-2 h-5 w-64" />
          </div>
          <Skeleton className="h-12 w-full" />
          <div className="mt-4 space-y-2">
            <Skeleton className="h-12 w-full" />
            <div className="grid grid-cols-7 gap-2">
              {Array.from({ length: 35 }).map((_, i) => (
                <Skeleton key={i} className="h-32" />
              ))}
            </div>
          </div>
        </div>
      }
    >
      <CalendarContent />
    </Suspense>
  );
}
