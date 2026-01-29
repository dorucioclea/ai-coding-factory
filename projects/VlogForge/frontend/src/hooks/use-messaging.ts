import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  ConversationDto,
  ConversationListResponse,
  MessageDto,
  MessageListResponse,
  SendMessagePayload,
  StartConversationPayload,
} from '@/types/messaging';

/**
 * Hook to get paginated conversations
 * Story: ACF-012
 */
export function useConversations(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: queryKeys.messaging.conversations({ page, pageSize }),
    queryFn: () =>
      apiClient.get<ConversationListResponse>('/conversations', {
        params: {
          page,
          pageSize,
        },
      }),
    staleTime: 1000 * 60,
  });
}

/**
 * Hook to get paginated messages for a conversation
 * Story: ACF-012
 */
export function useMessages(
  conversationId: string,
  page = 1,
  pageSize = 50,
  enabled = true
) {
  return useQuery({
    queryKey: queryKeys.messaging.messages(conversationId, { page, pageSize }),
    queryFn: () =>
      apiClient.get<MessageListResponse>(
        `/conversations/${encodeURIComponent(conversationId)}/messages`,
        {
          params: {
            page,
            pageSize,
          },
        }
      ),
    staleTime: 1000 * 60,
    enabled,
  });
}

/**
 * Hook to get unread message count
 * Story: ACF-012
 */
export function useUnreadCount() {
  return useQuery({
    queryKey: queryKeys.messaging.unreadCount(),
    queryFn: () =>
      apiClient.get<{ unreadCount: number }>('/conversations/unread-count'),
    staleTime: 1000 * 30,
  });
}

/**
 * Hook to start a new conversation
 * Story: ACF-012
 */
export function useStartConversation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: StartConversationPayload) =>
      apiClient.post<ConversationDto>('/conversations', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.messaging.all });
    },
  });
}

/**
 * Hook to send a message in a conversation
 * Story: ACF-012
 */
export function useSendMessage(conversationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: SendMessagePayload) =>
      apiClient.post<MessageDto>(
        `/conversations/${encodeURIComponent(conversationId)}/messages`,
        payload
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.messaging.messages(conversationId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.messaging.conversations(),
      });
    },
  });
}

/**
 * Hook to mark conversation messages as read
 * Story: ACF-012
 */
export function useMarkAsRead(conversationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      apiClient.post<void>(
        `/conversations/${encodeURIComponent(conversationId)}/read`
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.messaging.all });
    },
  });
}
