/**
 * Calendar hooks for content scheduling (ACF-015 Phase 4)
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  CalendarMonth,
  UpdateScheduleRequest,
  ContentIdeaResponse,
} from '@/types';
import { toast } from 'sonner';

/**
 * Fetch calendar month data
 */
export function useCalendarMonth(year: number, month: number) {
  return useQuery({
    queryKey: queryKeys.calendar.month(year, month),
    queryFn: async (): Promise<CalendarMonth> => {
      const monthStr = String(month).padStart(2, '0');
      const response = await apiClient.get<CalendarMonth>(
        `/calendar?month=${year}-${monthStr}`
      );
      return response;
    },
    staleTime: 1000 * 60 * 2, // 2 minutes - calendar data changes frequently
  });
}

/**
 * Update content item's scheduled date
 */
export function useUpdateSchedule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      contentId,
      scheduledDate,
    }: {
      contentId: string;
      scheduledDate: string;
    }) => {
      const request: UpdateScheduleRequest = { scheduledDate };
      const response = await apiClient.patch<ContentIdeaResponse>(
        `/content/${contentId}/schedule`,
        request
      );
      return response;
    },

    // Optimistic update
    onMutate: async ({ contentId, scheduledDate }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: queryKeys.calendar.all });
      await queryClient.cancelQueries({ queryKey: queryKeys.content.all });

      // Snapshot previous values
      const previousCalendarData = queryClient.getQueriesData({
        queryKey: queryKeys.calendar.months(),
      });
      const previousContentData = queryClient.getQueriesData({
        queryKey: queryKeys.content.lists(),
      });

      // Optimistically update calendar data
      queryClient.setQueriesData<CalendarMonth>(
        { queryKey: queryKeys.calendar.months() },
        (old) => {
          if (!old) return old;

          return {
            ...old,
            days: old.days.map((day) => ({
              ...day,
              items: day.items.map((item) =>
                item.id === contentId
                  ? { ...item, scheduledDate }
                  : item
              ),
            })),
          };
        }
      );

      return { previousCalendarData, previousContentData };
    },

    // Rollback on error
    onError: (error, variables, context) => {
      if (context) {
        // Restore calendar data
        context.previousCalendarData.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
        // Restore content data
        context.previousContentData.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
      }
      toast.error('Failed to update schedule', {
        description:
          error instanceof Error ? error.message : 'Please try again',
      });
    },

    // Refetch on success
    onSuccess: (data) => {
      // Invalidate calendar queries
      queryClient.invalidateQueries({ queryKey: queryKeys.calendar.all });
      // Invalidate content queries
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all });

      const dateDescription = data.scheduledDate
        ? `Content scheduled for ${new Date(data.scheduledDate).toLocaleDateString()}`
        : 'Schedule updated';
      toast.success('Schedule updated', { description: dateDescription });
    },
  });
}

/**
 * Unschedule content item (remove scheduled date)
 */
export function useUnschedule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (contentId: string) => {
      const response = await apiClient.patch<ContentIdeaResponse>(
        `/content/${contentId}/schedule`,
        { scheduledDate: null }
      );
      return response;
    },

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.calendar.all });
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all });
      toast.success('Schedule removed');
    },

    onError: (error) => {
      toast.error('Failed to remove schedule', {
        description:
          error instanceof Error ? error.message : 'Please try again',
      });
    },
  });
}
