'use client';

import { useState, useCallback } from 'react';
import { ClipboardList, AlertCircle } from 'lucide-react';
import {
  useMyTasks,
  useUpdateTaskStatus,
  useAddComment,
  useTaskFilters,
  useGroupedTasks,
} from '@/hooks';
import { GroupedTaskList } from '@/components/tasks';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui';
import { AssignmentStatus } from '@/types';

/**
 * My Tasks page - displays user's assigned tasks grouped by status
 * ACF-014: Team Member Task View
 *
 * AC1: Tasks assigned across all teams
 * AC2: Tasks grouped by status (Not Started, In Progress, Completed)
 * AC3: Sorted by due date (earliest first) within each group
 * AC4: Quick status update via dropdown on each card
 * AC5: Task detail modal with comments and history
 */
export default function TasksPage() {
  const { buildFilters } = useTaskFilters();
  const [filters] = useState(() =>
    buildFilters(undefined, undefined, 'dueDate', 'asc')
  );

  const { data, isLoading, error } = useMyTasks(filters);
  const updateStatusMutation = useUpdateTaskStatus();
  const addCommentMutation = useAddComment();

  const tasks = data?.items ?? [];
  const groupedTasks = useGroupedTasks(tasks);

  const handleStatusChange = useCallback(
    (taskId: string, status: AssignmentStatus) => {
      updateStatusMutation.mutate({
        taskId,
        status,
      });
    },
    [updateStatusMutation]
  );

  const handleAddComment = useCallback(
    async (taskId: string, content: string) => {
      await addCommentMutation.mutateAsync({
        taskId,
        content,
      });
    },
    [addCommentMutation]
  );

  const overdueCount = tasks.filter((t) => t.isOverdue).length;
  const notStartedCount = groupedTasks[AssignmentStatus.NotStarted].length;
  const inProgressCount = groupedTasks[AssignmentStatus.InProgress].length;
  const completedCount = groupedTasks[AssignmentStatus.Completed].length;

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <ClipboardList className="h-8 w-8 text-primary" />
          <h1 className="text-3xl font-bold">My Tasks</h1>
        </div>
        <p className="text-muted-foreground">
          View and manage your assigned tasks across all teams
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4 mb-8">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Not Started
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{notStartedCount}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              In Progress
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">
              {inProgressCount}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Completed
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {completedCount}
            </div>
          </CardContent>
        </Card>

        <Card className={overdueCount > 0 ? 'border-destructive' : ''}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-1">
              {overdueCount > 0 && (
                <AlertCircle className="h-4 w-4 text-destructive" />
              )}
              Overdue
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div
              className={`text-2xl font-bold ${
                overdueCount > 0 ? 'text-destructive' : ''
              }`}
            >
              {overdueCount}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Error State */}
      {error && (
        <Card className="border-destructive bg-destructive/5 mb-6">
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 text-destructive">
              <AlertCircle className="h-5 w-5" />
              <p className="font-medium">Failed to load tasks</p>
            </div>
            <p className="text-sm text-muted-foreground mt-1">
              {error instanceof Error ? error.message : 'An error occurred'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Grouped Task List - ACF-014 AC2, AC3 */}
      <GroupedTaskList
        groupedTasks={groupedTasks}
        isLoading={isLoading}
        onStatusChange={handleStatusChange}
        onAddComment={handleAddComment}
        isUpdatingStatus={updateStatusMutation.isPending}
        isAddingComment={addCommentMutation.isPending}
      />
    </div>
  );
}
