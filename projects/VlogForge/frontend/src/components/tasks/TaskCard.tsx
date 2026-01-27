'use client';

import { format, isPast, parseISO } from 'date-fns';
import { Calendar, MessageSquare } from 'lucide-react';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Badge,
} from '@/components/ui';
import { OverdueBadge } from './OverdueBadge';
import { TaskStatusDropdown } from './TaskStatusDropdown';
import {
  AssignmentStatus,
  AssignmentStatusLabels,
  type TaskAssignmentResponse,
} from '@/types';
import { cn } from '@/lib/utils';

interface TaskCardProps {
  task: TaskAssignmentResponse;
  onStatusChange: (taskId: string, status: AssignmentStatus) => void;
  onClick?: (taskId: string) => void;
  isUpdating?: boolean;
  className?: string;
}

/**
 * Individual task card with status management
 * ACF-015 Phase 6
 */
export function TaskCard({
  task,
  onStatusChange,
  onClick,
  isUpdating = false,
  className,
}: TaskCardProps) {
  const dueDate = parseISO(task.dueDate);
  const isOverdue = task.isOverdue || (isPast(dueDate) && task.status !== AssignmentStatus.Completed);

  const handleClick = () => {
    if (onClick) {
      onClick(task.id);
    }
  };

  const handleStatusChange = (status: AssignmentStatus) => {
    onStatusChange(task.id, status);
  };

  const getStatusBadgeVariant = () => {
    switch (task.status) {
      case AssignmentStatus.Completed:
        return 'success' as const;
      case AssignmentStatus.InProgress:
        return 'secondary' as const;
      default:
        return 'default' as const;
    }
  };

  return (
    <Card
      className={cn(
        'transition-all hover:shadow-md cursor-pointer',
        isOverdue && 'border-destructive bg-destructive/5',
        className
      )}
      onClick={handleClick}
    >
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1">
            <CardTitle className="text-lg">
              Content Item {task.contentItemId.substring(0, 8)}
            </CardTitle>
            <CardDescription className="mt-1">
              Assigned by User {task.assignedById.substring(0, 8)}
            </CardDescription>
          </div>
          <div className="flex items-center gap-2">
            <OverdueBadge isOverdue={isOverdue} />
            <Badge variant={getStatusBadgeVariant()}>
              {AssignmentStatusLabels[task.status]}
            </Badge>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {task.notes && (
            <p className="text-sm text-muted-foreground">{task.notes}</p>
          )}

          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Calendar className="h-4 w-4" />
              <span className={isOverdue ? 'text-destructive font-medium' : ''}>
                Due: {format(dueDate, 'MMM d, yyyy')}
              </span>
            </div>
            {task.comments.length > 0 && (
              <div className="flex items-center gap-1 text-sm text-muted-foreground">
                <MessageSquare className="h-4 w-4" />
                <span>{task.comments.length}</span>
              </div>
            )}
          </div>

          <div
            onClick={(e) => e.stopPropagation()}
            className="pt-2 border-t"
          >
            <TaskStatusDropdown
              currentStatus={task.status}
              onStatusChange={handleStatusChange}
              disabled={isUpdating}
              className="w-full"
            />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default TaskCard;
