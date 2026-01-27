'use client';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Skeleton,
} from '@/components/ui';
import type { ContentPerformance, ContentSortBy } from '@/types';
import { PLATFORMS } from '@/types';
import { Eye, ThumbsUp, MessageCircle } from 'lucide-react';

interface TopContentTableProps {
  content?: ContentPerformance[];
  sortBy: ContentSortBy;
  onSortChange: (sort: ContentSortBy) => void;
  isLoading: boolean;
}

/**
 * Top performing content table
 * Story: ACF-004 (AC4)
 */
export function TopContentTable({
  content,
  sortBy,
  onSortChange,
  isLoading,
}: TopContentTableProps) {
  const formatNumber = (num: number): string => {
    if (num >= 1_000_000) {
      return `${(num / 1_000_000).toFixed(1)}M`;
    }
    if (num >= 1_000) {
      return `${(num / 1_000).toFixed(1)}K`;
    }
    return num.toLocaleString();
  };

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const sortOptions: { value: ContentSortBy; label: string }[] = [
    { value: 'views', label: 'Views' },
    { value: 'engagement', label: 'Engagement' },
    { value: 'likes', label: 'Likes' },
    { value: 'comments', label: 'Comments' },
  ];

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <Skeleton className="h-6 w-32" />
              <Skeleton className="mt-1 h-4 w-48" />
            </div>
            <Skeleton className="h-9 w-32" />
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex items-center gap-4">
                <Skeleton className="h-16 w-24 rounded" />
                <div className="flex-1">
                  <Skeleton className="h-5 w-48" />
                  <Skeleton className="mt-1 h-4 w-32" />
                </div>
                <Skeleton className="h-5 w-20" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Top Content</CardTitle>
            <CardDescription>Your best performing content</CardDescription>
          </div>
          <select
            value={sortBy}
            onChange={(e) => onSortChange(e.target.value as ContentSortBy)}
            className="rounded-md border bg-background px-3 py-2 text-sm"
          >
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                Sort by {option.label}
              </option>
            ))}
          </select>
        </div>
      </CardHeader>
      <CardContent>
        {!content || content.length === 0 ? (
          <div className="flex h-40 items-center justify-center text-muted-foreground">
            No content data available
          </div>
        ) : (
          <div className="space-y-4">
            {content.map((item, index) => {
              const platformInfo =
                PLATFORMS[item.platformType as keyof typeof PLATFORMS];
              return (
                <div
                  key={item.contentId}
                  className="flex items-center gap-4 rounded-lg border p-3"
                >
                  <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-sm font-bold">
                    {index + 1}
                  </div>
                  {item.thumbnailUrl ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                      src={item.thumbnailUrl}
                      alt={item.title}
                      className="h-16 w-24 rounded object-cover"
                    />
                  ) : (
                    <div className="flex h-16 w-24 items-center justify-center rounded bg-muted text-muted-foreground">
                      No image
                    </div>
                  )}
                  <div className="flex-1 min-w-0">
                    <a
                      href={item.contentUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="font-medium hover:underline line-clamp-1"
                    >
                      {item.title}
                    </a>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span
                        className="inline-flex items-center rounded-full px-2 py-0.5 text-white"
                        style={{
                          backgroundColor: platformInfo?.color ?? '#6B7280',
                        }}
                      >
                        {item.platformType}
                      </span>
                      <span>{formatDate(item.publishedAt)}</span>
                    </div>
                  </div>
                  <div className="flex gap-4 text-sm">
                    <div className="flex items-center gap-1 text-muted-foreground">
                      <Eye className="h-4 w-4" />
                      <span>{formatNumber(item.viewCount)}</span>
                    </div>
                    <div className="flex items-center gap-1 text-muted-foreground">
                      <ThumbsUp className="h-4 w-4" />
                      <span>{formatNumber(item.likeCount)}</span>
                    </div>
                    <div className="flex items-center gap-1 text-muted-foreground">
                      <MessageCircle className="h-4 w-4" />
                      <span>{formatNumber(item.commentCount)}</span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
