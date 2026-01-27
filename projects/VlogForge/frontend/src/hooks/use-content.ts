'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

import { contentService } from '@/lib/content';
import { queryKeys } from '@/lib/query-client';
import type {
  ContentIdeaResponse,
  ContentFilters,
  CreateContentIdeaRequest,
  UpdateContentIdeaRequest,
  UpdateStatusRequest,
} from '@/types';

/**
 * Hook for fetching content ideas list with filters
 */
export function useContentIdeas(filters?: ContentFilters) {
  return useQuery({
    // eslint-disable-next-line @tanstack/query/exhaustive-deps -- filters is passed to queryKey function
    queryKey: queryKeys.content.list((filters ?? {}) as Record<string, unknown>),
    queryFn: () => contentService.getContentIdeas(filters),
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

/**
 * Hook for fetching single content idea
 */
export function useContentIdea(id: string) {
  return useQuery({
    queryKey: queryKeys.content.detail(id),
    queryFn: () => contentService.getContentIdea(id),
    staleTime: 1000 * 60 * 2, // 2 minutes
    enabled: !!id,
  });
}

/**
 * Hook for creating content idea
 */
export function useCreateIdea() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateContentIdeaRequest) =>
      contentService.createContentIdea(data),
    onSuccess: (newIdea: ContentIdeaResponse) => {
      // Invalidate lists to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });

      // Optimistically add to cache
      queryClient.setQueryData(
        queryKeys.content.detail(newIdea.id),
        newIdea
      );
    },
  });
}

/**
 * Hook for updating content idea
 */
export function useUpdateIdea() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateContentIdeaRequest;
    }) => contentService.updateContentIdea(id, data),
    onSuccess: (updatedIdea: ContentIdeaResponse, variables) => {
      // Update detail cache
      queryClient.setQueryData(
        queryKeys.content.detail(variables.id),
        updatedIdea
      );

      // Invalidate lists to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
    },
  });
}

/**
 * Hook for deleting content idea
 */
export function useDeleteIdea() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => contentService.deleteContentIdea(id),
    onSuccess: (_, deletedId: string) => {
      // Remove from detail cache
      queryClient.removeQueries({
        queryKey: queryKeys.content.detail(deletedId),
      });

      // Invalidate lists to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
    },
  });
}

/**
 * Hook for updating content idea status
 */
export function useUpdateIdeaStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      status,
    }: {
      id: string;
      status: UpdateStatusRequest;
    }) => contentService.updateContentIdeaStatus(id, status),
    onSuccess: (updatedIdea: ContentIdeaResponse, variables) => {
      // Update detail cache
      queryClient.setQueryData(
        queryKeys.content.detail(variables.id),
        updatedIdea
      );

      // Invalidate lists to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
    },
  });
}

/**
 * Hook for content operations (combined)
 */
export function useContent() {
  const createMutation = useCreateIdea();
  const updateMutation = useUpdateIdea();
  const deleteMutation = useDeleteIdea();
  const updateStatusMutation = useUpdateIdeaStatus();

  const createIdea = useCallback(
    (data: CreateContentIdeaRequest) => createMutation.mutateAsync(data),
    [createMutation]
  );

  const updateIdea = useCallback(
    (id: string, data: UpdateContentIdeaRequest) =>
      updateMutation.mutateAsync({ id, data }),
    [updateMutation]
  );

  const deleteIdea = useCallback(
    (id: string) => deleteMutation.mutateAsync(id),
    [deleteMutation]
  );

  const updateStatus = useCallback(
    (id: string, status: UpdateStatusRequest) =>
      updateStatusMutation.mutateAsync({ id, status }),
    [updateStatusMutation]
  );

  return {
    // Actions
    createIdea,
    updateIdea,
    deleteIdea,
    updateStatus,

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,

    // Errors
    createError: createMutation.error,
    updateError: updateMutation.error,
    deleteError: deleteMutation.error,
    statusError: updateStatusMutation.error,
  };
}

export default useContent;
