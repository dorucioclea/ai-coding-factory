'use client';

import { usePendingApprovals } from '@/hooks';
import { formatDistanceToNow } from 'date-fns';
import { Clock, FileText } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';

interface PendingApprovalsListProps {
  teamId: string;
  onSelectContent?: (contentId: string) => void;
  className?: string;
}

/**
 * List of content items pending approval
 * Story: ACF-009
 */
export function PendingApprovalsList({
  teamId,
  onSelectContent,
  className,
}: PendingApprovalsListProps) {
  const { data, isLoading, error } = usePendingApprovals(teamId);

  if (isLoading) {
    return (
      <Card className={cn(className)}>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Pending Approvals
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="flex items-start gap-3 p-3 rounded-lg border">
                <Skeleton className="h-10 w-10 rounded" />
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className={cn(className)}>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Pending Approvals
          </CardTitle>
          <CardDescription className="text-destructive">
            Failed to load pending approvals
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card className={cn(className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Clock className="h-5 w-5" />
          Pending Approvals
          {data && data.totalCount > 0 && (
            <Badge variant="secondary" className="ml-2">
              {data.totalCount}
            </Badge>
          )}
        </CardTitle>
        <CardDescription>
          Content items waiting for your review
        </CardDescription>
      </CardHeader>
      <CardContent>
        {!data?.items || data.items.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            <FileText className="h-12 w-12 mx-auto mb-3 opacity-50" />
            <p>No content pending approval</p>
          </div>
        ) : (
          <div className="space-y-3">
            {data.items.map((item) => (
              <div
                key={item.contentItemId}
                className={cn(
                  'flex items-start gap-3 p-3 rounded-lg border transition-colors',
                  onSelectContent && 'cursor-pointer hover:bg-muted/50'
                )}
                onClick={() => onSelectContent?.(item.contentItemId)}
              >
                <div className="flex h-10 w-10 items-center justify-center rounded bg-orange-100 dark:bg-orange-900">
                  <FileText className="h-5 w-5 text-orange-600 dark:text-orange-200" />
                </div>
                <div className="flex-1 min-w-0">
                  <h4 className="text-sm font-medium truncate">
                    {item.title}
                  </h4>
                  {item.notes && (
                    <p className="text-xs text-muted-foreground line-clamp-2 mt-1">
                      {item.notes}
                    </p>
                  )}
                  <div className="flex items-center gap-2 mt-2">
                    <span className="text-xs text-muted-foreground">
                      Submitted {formatDistanceToNow(new Date(item.submittedAt), { addSuffix: true })}
                    </span>
                    {item.platformTags.length > 0 && (
                      <>
                        <span className="text-muted-foreground">•</span>
                        <div className="flex gap-1">
                          {item.platformTags.slice(0, 2).map((tag) => (
                            <Badge key={tag} variant="outline" className="text-xs">
                              {tag}
                            </Badge>
                          ))}
                          {item.platformTags.length > 2 && (
                            <Badge variant="outline" className="text-xs">
                              +{item.platformTags.length - 2}
                            </Badge>
                          )}
                        </div>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default PendingApprovalsList;
