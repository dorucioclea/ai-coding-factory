import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  CollaborationRequestDto,
  CollaborationRequestListResponse,
  CollaborationRequestStatus,
  SendCollaborationRequestPayload,
  DeclineCollaborationRequestPayload,
} from '@/types/collaboration';

/**
 * Hook to get collaboration inbox (received requests)
 * Story: ACF-011
 */
export function useCollaborationInbox(
  status?: CollaborationRequestStatus,
  page = 1,
  pageSize = 20,
  enabled = true
) {
  const filters = { status, page, pageSize };

  return useQuery({
    queryKey: queryKeys.collaborations.inbox(filters),
    queryFn: () =>
      apiClient.get<CollaborationRequestListResponse>('/collaborations/inbox', {
        params: {
          status,
          page,
          pageSize,
        },
      }),
    staleTime: 1000 * 60 * 2,
    enabled,
  });
}

/**
 * Hook to get sent collaboration requests
 * Story: ACF-011
 */
export function useSentCollaborations(
  status?: CollaborationRequestStatus,
  page = 1,
  pageSize = 20,
  enabled = true
) {
  const filters = { status, page, pageSize };

  return useQuery({
    queryKey: queryKeys.collaborations.sent(filters),
    queryFn: () =>
      apiClient.get<CollaborationRequestListResponse>('/collaborations/sent', {
        params: {
          status,
          page,
          pageSize,
        },
      }),
    staleTime: 1000 * 60 * 2,
    enabled,
  });
}

/**
 * Hook to send a collaboration request
 * Story: ACF-011
 */
export function useSendCollaborationRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: SendCollaborationRequestPayload) =>
      apiClient.post<CollaborationRequestDto>('/collaborations/request', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collaborations.all });
    },
  });
}

/**
 * Hook to accept a collaboration request
 * Story: ACF-011
 */
export function useAcceptCollaborationRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (requestId: string) =>
      apiClient.post<CollaborationRequestDto>(
        `/collaborations/${encodeURIComponent(requestId)}/accept`
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collaborations.all });
    },
  });
}

/**
 * Hook to decline a collaboration request
 * Story: ACF-011
 */
export function useDeclineCollaborationRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      requestId,
      reason,
    }: {
      requestId: string;
      reason?: string;
    }) => {
      const payload: DeclineCollaborationRequestPayload = { reason };
      return apiClient.post<CollaborationRequestDto>(
        `/collaborations/${encodeURIComponent(requestId)}/decline`,
        payload
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collaborations.all });
    },
  });
}
