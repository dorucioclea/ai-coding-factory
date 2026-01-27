'use client';

import { useApprovalHistory } from '@/hooks';
import { ApprovalAction, ApprovalActionLabels, ApprovalActionColors, STATUS_CONFIG } from '@/types';
import { formatDistanceToNow } from 'date-fns';
import { MessageSquare, Check, X, Send, RefreshCw } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';

interface ApprovalHistoryProps {
  contentId: string;
  className?: string;
}

const actionIcons = {
  [ApprovalAction.Submitted]: Send,
  [ApprovalAction.Approved]: Check,
  [ApprovalAction.ChangesRequested]: X,
  [ApprovalAction.Resubmitted]: RefreshCw,
};

const actionColorClasses = {
  blue: 'text-blue-600 bg-blue-100 dark:bg-blue-900 dark:text-blue-200',
  green: 'text-green-600 bg-green-100 dark:bg-green-900 dark:text-green-200',
  red: 'text-red-600 bg-red-100 dark:bg-red-900 dark:text-red-200',
  orange: 'text-orange-600 bg-orange-100 dark:bg-orange-900 dark:text-orange-200',
};

/**
 * Approval history timeline component
 * Story: ACF-009 AC5: Approval History
 */
export function ApprovalHistory({ contentId, className }: ApprovalHistoryProps) {
  const { data, isLoading, error } = useApprovalHistory(contentId);

  if (isLoading) {
    return (
      <div className={cn('space-y-4', className)}>
        <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">
          Approval History
        </h3>
        {[1, 2, 3].map((i) => (
          <div key={i} className="flex gap-3">
            <Skeleton className="h-8 w-8 rounded-full" />
            <div className="flex-1 space-y-2">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/4" />
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className={cn('text-sm text-muted-foreground', className)}>
        Failed to load approval history
      </div>
    );
  }

  if (!data?.records || data.records.length === 0) {
    return (
      <div className={cn('text-sm text-muted-foreground', className)}>
        No approval history yet
      </div>
    );
  }

  return (
    <div className={cn('space-y-4', className)}>
      <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">
        Approval History
      </h3>
      <div className="relative">
        <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200 dark:bg-gray-700" />
        <ul className="space-y-4">
          {data.records.map((record) => {
            const Icon = actionIcons[record.action];
            const colorClass = actionColorClasses[ApprovalActionColors[record.action] as keyof typeof actionColorClasses];
            const newStatusConfig = STATUS_CONFIG[record.newStatus];

            return (
              <li key={record.id} className="relative flex gap-3 pl-8">
                <div
                  className={cn(
                    'absolute left-0 flex h-8 w-8 items-center justify-center rounded-full',
                    colorClass
                  )}
                >
                  <Icon className="h-4 w-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                      {ApprovalActionLabels[record.action]}
                    </span>
                    <span
                      className={cn(
                        'inline-flex items-center rounded-full px-2 py-0.5 text-xs',
                        `bg-${newStatusConfig.color}-100 text-${newStatusConfig.color}-800`,
                        `dark:bg-${newStatusConfig.color}-900 dark:text-${newStatusConfig.color}-200`
                      )}
                    >
                      → {newStatusConfig.label}
                    </span>
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    {formatDistanceToNow(new Date(record.createdAt), { addSuffix: true })}
                  </p>
                  {record.feedback && (
                    <div className="mt-2 flex items-start gap-2 rounded-md bg-gray-50 dark:bg-gray-800 p-2">
                      <MessageSquare className="h-4 w-4 text-gray-400 mt-0.5 flex-shrink-0" />
                      <p className="text-sm text-gray-700 dark:text-gray-300">
                        {record.feedback}
                      </p>
                    </div>
                  )}
                </div>
              </li>
            );
          })}
        </ul>
      </div>
    </div>
  );
}

export default ApprovalHistory;
