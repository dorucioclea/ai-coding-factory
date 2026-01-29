'use client';

import { Activity } from 'lucide-react';
import { Skeleton } from '@/components/ui';
import type { SharedProjectActivityDto } from '@/types/shared-project';

interface ProjectActivityFeedProps {
  activities: SharedProjectActivityDto[];
  isLoading: boolean;
}

/**
 * Activity feed component for shared projects
 * Story: ACF-013
 */
export function ProjectActivityFeed({
  activities,
  isLoading,
}: ProjectActivityFeedProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-12" />
        ))}
      </div>
    );
  }

  if (activities.length === 0) {
    return (
      <p className="text-sm text-muted-foreground py-4 text-center">
        No activity yet.
      </p>
    );
  }

  return (
    <div className="space-y-2">
      {activities.map((activity) => (
        <div
          key={activity.id}
          className="flex items-start gap-3 p-2 text-sm"
        >
          <Activity className="h-4 w-4 mt-0.5 text-muted-foreground flex-shrink-0" />
          <div className="flex-1">
            <span className="text-foreground">{activity.message}</span>
            <span className="text-xs text-muted-foreground ml-2">
              {new Date(activity.createdAt).toLocaleString()}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}
