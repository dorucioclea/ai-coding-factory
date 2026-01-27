'use client';

import { X } from 'lucide-react';

import { Button, Label } from '@/components/ui';
import { cn } from '@/lib/utils';
import type { ContentFilters as ContentFiltersType } from '@/types';
import {
  IdeaStatus,
  PLATFORM_TAGS,
  STATUS_CONFIG,
} from '@/types';

interface ContentFiltersProps {
  filters: ContentFiltersType;
  onChange: (filters: ContentFiltersType) => void;
  className?: string;
}

export function ContentFilters({
  filters,
  onChange,
  className,
}: ContentFiltersProps) {
  const handleStatusChange = (status: IdeaStatus | undefined) => {
    onChange({
      ...filters,
      status,
      page: 1, // Reset to first page
    });
  };

  const handlePlatformChange = (platform: string | undefined) => {
    onChange({
      ...filters,
      platformTag: platform,
      page: 1, // Reset to first page
    });
  };

  const clearFilters = () => {
    onChange({
      ...filters,
      status: undefined,
      platformTag: undefined,
      page: 1,
    });
  };

  const hasActiveFilters = filters.status !== undefined || filters.platformTag !== undefined;

  return (
    <div className={cn('space-y-4', className)}>
      {/* Status Filter */}
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label>Filter by Status</Label>
          {hasActiveFilters && (
            <Button
              variant="ghost"
              size="sm"
              onClick={clearFilters}
              className="h-auto p-0 text-xs"
            >
              <X className="mr-1 h-3 w-3" />
              Clear
            </Button>
          )}
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => handleStatusChange(undefined)}
            className={cn(
              'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              'border focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
              filters.status === undefined
                ? 'border-primary bg-primary text-primary-foreground'
                : 'border-input bg-background hover:bg-accent hover:text-accent-foreground'
            )}
          >
            All
          </button>
          {Object.values(IdeaStatus)
            .filter((v) => typeof v === 'number')
            .map((status) => {
              const statusNum = status as IdeaStatus;
              const config = STATUS_CONFIG[statusNum];
              const isSelected = filters.status === statusNum;

              return (
                <button
                  key={statusNum}
                  type="button"
                  onClick={() => handleStatusChange(statusNum)}
                  className={cn(
                    'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                    'border focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                    isSelected
                      ? 'border-primary bg-primary text-primary-foreground'
                      : 'border-input bg-background hover:bg-accent hover:text-accent-foreground'
                  )}
                >
                  {config.label}
                </button>
              );
            })}
        </div>
      </div>

      {/* Platform Filter */}
      <div className="space-y-2">
        <Label>Filter by Platform</Label>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => handlePlatformChange(undefined)}
            className={cn(
              'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              'border focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
              filters.platformTag === undefined
                ? 'border-primary bg-primary text-primary-foreground'
                : 'border-input bg-background hover:bg-accent hover:text-accent-foreground'
            )}
          >
            All
          </button>
          {PLATFORM_TAGS.map((platform) => {
            const isSelected = filters.platformTag === platform;

            return (
              <button
                key={platform}
                type="button"
                onClick={() => handlePlatformChange(platform)}
                className={cn(
                  'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                  'border focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                  isSelected
                    ? 'border-primary bg-primary text-primary-foreground'
                    : 'border-input bg-background hover:bg-accent hover:text-accent-foreground'
                )}
              >
                {platform}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default ContentFilters;
