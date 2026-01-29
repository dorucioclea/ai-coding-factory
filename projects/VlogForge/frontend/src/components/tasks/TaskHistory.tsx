'use client';

import { formatDistanceToNow } from 'date-fns';
import { History, ArrowRight, Calendar, MessageSquare, UserCheck, Plus } from 'lucide-react';
import { Avatar, AvatarFallback } from '@/components/ui';
import {
  TaskHistoryAction,
  TaskHistoryActionLabels,
  type TaskHistoryResponse,
} from '@/types';

interface TaskHistoryProps {
  history: TaskHistoryResponse[];
  className?: string;
}

const historyIcons: Record<TaskHistoryAction, React.ReactNode> = {
  [TaskHistoryAction.Created]: <Plus className="h-3 w-3" />,
  [TaskHistoryAction.StatusChanged]: <ArrowRight className="h-3 w-3" />,
  [TaskHistoryAction.Reassigned]: <UserCheck className="h-3 w-3" />,
  [TaskHistoryAction.DueDateChanged]: <Calendar className="h-3 w-3" />,
  [TaskHistoryAction.CommentAdded]: <MessageSquare className="h-3 w-3" />,
};

/**
 * Task history timeline display
 * ACF-014 AC5: Task details modal with history
 */
export function TaskHistory({ history, className }: TaskHistoryProps) {
  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch {
      return 'recently';
    }
  };

  const getInitials = (userId: string) => {
    return userId.substring(0, 2).toUpperCase();
  };

  return (
    <div className={className}>
      <div className="mb-4 flex items-center gap-2">
        <History className="h-5 w-5 text-muted-foreground" />
        <h3 className="font-semibold text-lg">
          History ({history.length})
        </h3>
      </div>

      {history.length === 0 ? (
        <p className="text-sm text-muted-foreground text-center py-8">
          No history recorded yet.
        </p>
      ) : (
        <div className="space-y-3">
          {history.map((entry) => (
            <div
              key={entry.id}
              className="flex items-start gap-3 text-sm"
            >
              <Avatar className="h-7 w-7 mt-0.5">
                <AvatarFallback className="text-xs">
                  {getInitials(entry.changedByUserId)}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="inline-flex items-center gap-1 rounded-md bg-muted px-2 py-0.5 text-xs font-medium">
                    {historyIcons[entry.action]}
                    {TaskHistoryActionLabels[entry.action]}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {formatDate(entry.createdAt)}
                  </span>
                </div>
                <p className="text-muted-foreground mt-1 truncate">
                  {entry.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default TaskHistory;
