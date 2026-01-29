'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import {
  AssignmentStatus,
  type TaskListResponse,
  type TaskAssignmentResponse,
  type AssignTaskRequest,
  type UpdateTaskStatusRequest,
  type AddCommentRequest,
  type TaskFilters,
  type GroupedTasks,
} from '@/types';

/**
 * Hook for fetching user's assigned tasks
 * GET /api/tasks/mine
 * Stories: ACF-008, ACF-014
 */
export function useMyTasks(filters?: TaskFilters) {
  return useQuery({
    // eslint-disable-next-line @tanstack/query/exhaustive-deps -- filters is passed to queryKey function
    queryKey: queryKeys.tasks.myTasks(filters as Record<string, unknown> | undefined),
    queryFn: () =>
      apiClient.get<TaskListResponse>('/tasks/mine', {
        params: filters as Record<string, string | number | boolean | undefined>,
      }),
    staleTime: 1000 * 60 * 2, // 2 minutes - tasks change frequently
  });
}

/**
 * Hook for fetching a single task with comments and history
 * GET /api/tasks/{id}?includeComments=true&includeHistory=true
 * Stories: ACF-008, ACF-014
 */
export function useTask(id: string, includeHistory = false) {
  return useQuery({
    queryKey: queryKeys.tasks.detail(id),
    queryFn: () =>
      apiClient.get<TaskAssignmentResponse>(`/tasks/${id}`, {
        params: {
          includeComments: true,
          includeHistory,
        },
      }),
    enabled: !!id,
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

/**
 * Groups tasks by status and sorts by due date within each group.
 * Story: ACF-014 (AC2, AC3)
 */
export function useGroupedTasks(tasks: TaskAssignmentResponse[]): GroupedTasks {
  return useMemo(() => {
    const grouped: GroupedTasks = {
      [AssignmentStatus.NotStarted]: [],
      [AssignmentStatus.InProgress]: [],
      [AssignmentStatus.Completed]: [],
    };

    for (const task of tasks) {
      const statusKey = task.status as AssignmentStatus;
      if (statusKey in grouped) {
        grouped[statusKey] = [...grouped[statusKey], task];
      }
    }

    // Sort each group by due date (earliest first)
    const sortByDueDate = (a: TaskAssignmentResponse, b: TaskAssignmentResponse) =>
      new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();

    return {
      [AssignmentStatus.NotStarted]: [...grouped[AssignmentStatus.NotStarted]].sort(sortByDueDate),
      [AssignmentStatus.InProgress]: [...grouped[AssignmentStatus.InProgress]].sort(sortByDueDate),
      [AssignmentStatus.Completed]: [...grouped[AssignmentStatus.Completed]].sort(sortByDueDate),
    };
  }, [tasks]);
}

/**
 * Hook for fetching task comments
 * GET /api/tasks/{id}/comments
 */
export function useTaskComments(taskId: string) {
  return useQuery({
    queryKey: queryKeys.tasks.comments(taskId),
    queryFn: () =>
      apiClient.get<TaskAssignmentResponse['comments']>(
        `/tasks/${taskId}/comments`
      ),
    enabled: !!taskId,
    staleTime: 1000 * 30, // 30 seconds - comments change very frequently
  });
}

/**
 * Hook for assigning a task to a team member
 * POST /api/content/{id}/assign
 */
export function useAssignTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      contentItemId,
      ...data
    }: AssignTaskRequest & { contentItemId: string }) =>
      apiClient.post<TaskAssignmentResponse>(
        `/content/${contentItemId}/assign`,
        data
      ),
    onSuccess: () => {
      // Invalidate my tasks list
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.myTasks(),
      });
      // Invalidate all tasks lists
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.lists(),
      });
    },
  });
}

/**
 * Hook for updating task status
 * PATCH /api/tasks/{id}/status
 */
export function useUpdateTaskStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      ...data
    }: UpdateTaskStatusRequest & { taskId: string }) =>
      apiClient.patch<TaskAssignmentResponse>(`/tasks/${taskId}/status`, data),
    onSuccess: (data, variables) => {
      // Update task detail cache
      queryClient.setQueryData(
        queryKeys.tasks.detail(variables.taskId),
        data
      );
      // Invalidate my tasks list
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.myTasks(),
      });
      // Invalidate all tasks lists
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.lists(),
      });
    },
  });
}

/**
 * Hook for adding a comment to a task
 * POST /api/tasks/{id}/comments
 */
export function useAddComment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      ...data
    }: AddCommentRequest & { taskId: string }) =>
      apiClient.post(`/tasks/${taskId}/comments`, data),
    onSuccess: (_, variables) => {
      // Invalidate task detail to refetch with new comment
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.detail(variables.taskId),
      });
      // Invalidate comments
      queryClient.invalidateQueries({
        queryKey: queryKeys.tasks.comments(variables.taskId),
      });
    },
  });
}

/**
 * Helper hook for task filtering
 */
export function useTaskFilters() {
  const buildFilters = useCallback(
    (
      status?: number,
      isOverdue?: boolean,
      sortBy?: string,
      sortDirection?: string
    ): TaskFilters => {
      const filters: TaskFilters = {};

      if (status !== undefined) {
        filters.status = status;
      }

      if (isOverdue !== undefined) {
        filters.isOverdue = isOverdue;
      }

      if (sortBy) {
        filters.sortBy = sortBy as TaskFilters['sortBy'];
      }

      if (sortDirection) {
        filters.sortDirection = sortDirection as 'asc' | 'desc';
      }

      return filters;
    },
    []
  );

  return { buildFilters };
}
