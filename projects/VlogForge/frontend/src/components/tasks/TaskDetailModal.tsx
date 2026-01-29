'use client';

import { useState } from 'react';
import { format, parseISO } from 'date-fns';
import { Calendar, User, Clock } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  Badge,
  Separator,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@/components/ui';
import { TaskComments } from './TaskComments';
import { TaskHistory } from './TaskHistory';
import { TaskStatusDropdown } from './TaskStatusDropdown';
import { OverdueBadge } from './OverdueBadge';
import {
  AssignmentStatus,
  AssignmentStatusLabels,
  type TaskAssignmentResponse,
} from '@/types';

interface TaskDetailModalProps {
  task: TaskAssignmentResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onStatusChange: (status: AssignmentStatus) => void;
  onAddComment: (content: string) => Promise<void>;
  isUpdatingStatus?: boolean;
  isAddingComment?: boolean;
}

/**
 * Modal for viewing full task details with comments and history
 * Stories: ACF-008, ACF-014
 * ACF-014 AC5: Modal shows full details including comments and history
 */
export function TaskDetailModal({
  task,
  isOpen,
  onClose,
  onStatusChange,
  onAddComment,
  isUpdatingStatus = false,
  isAddingComment = false,
}: TaskDetailModalProps) {
  const [activeTab, setActiveTab] = useState('comments');

  if (!task) return null;

  const dueDate = parseISO(task.dueDate);
  const createdDate = parseISO(task.createdAt);
  const completedDate = task.completedAt ? parseISO(task.completedAt) : null;

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
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-start justify-between gap-4">
            <div className="flex-1">
              <DialogTitle className="text-xl">Task Details</DialogTitle>
              <DialogDescription className="mt-1">
                Content Item {task.contentItemId}
              </DialogDescription>
            </div>
            <div className="flex items-center gap-2">
              <OverdueBadge isOverdue={task.isOverdue} />
              <Badge variant={getStatusBadgeVariant()}>
                {AssignmentStatusLabels[task.status]}
              </Badge>
            </div>
          </div>
        </DialogHeader>

        <div className="space-y-6 mt-4">
          {/* Task Information */}
          <div className="space-y-3">
            <div className="flex items-center gap-2 text-sm">
              <User className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Assigned to:</span>
              <span className="font-medium">
                User {task.assigneeId.substring(0, 8)}
              </span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <User className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Assigned by:</span>
              <span className="font-medium">
                User {task.assignedById.substring(0, 8)}
              </span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <Calendar className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Due date:</span>
              <span
                className={
                  task.isOverdue
                    ? 'font-medium text-destructive'
                    : 'font-medium'
                }
              >
                {format(dueDate, 'MMMM d, yyyy')}
              </span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <Clock className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Created:</span>
              <span className="font-medium">
                {format(createdDate, 'MMM d, yyyy h:mm a')}
              </span>
            </div>

            {completedDate && (
              <div className="flex items-center gap-2 text-sm">
                <Clock className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Completed:</span>
                <span className="font-medium">
                  {format(completedDate, 'MMM d, yyyy h:mm a')}
                </span>
              </div>
            )}
          </div>

          {task.notes && (
            <>
              <Separator />
              <div>
                <h4 className="font-semibold mb-2">Notes</h4>
                <p className="text-sm text-muted-foreground whitespace-pre-wrap">
                  {task.notes}
                </p>
              </div>
            </>
          )}

          <Separator />

          {/* Status Dropdown */}
          <div>
            <h4 className="font-semibold mb-2">Update Status</h4>
            <TaskStatusDropdown
              currentStatus={task.status}
              onStatusChange={onStatusChange}
              disabled={isUpdatingStatus}
              className="w-full max-w-xs"
            />
          </div>

          <Separator />

          {/* Tabbed Comments & History Section - ACF-014 AC5 */}
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="w-full">
              <TabsTrigger value="comments" className="flex-1">
                Comments ({task.comments.length})
              </TabsTrigger>
              <TabsTrigger value="history" className="flex-1">
                History ({task.history?.length ?? 0})
              </TabsTrigger>
            </TabsList>
            <TabsContent value="comments" className="mt-4">
              <TaskComments
                comments={task.comments}
                onAddComment={onAddComment}
                isAddingComment={isAddingComment}
              />
            </TabsContent>
            <TabsContent value="history" className="mt-4">
              <TaskHistory history={task.history ?? []} />
            </TabsContent>
          </Tabs>
        </div>
      </DialogContent>
    </Dialog>
  );
}

export default TaskDetailModal;
