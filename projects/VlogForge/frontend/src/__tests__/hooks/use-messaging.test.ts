/**
 * Unit tests for use-messaging hooks
 * Story: ACF-012 - Messaging Feature
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';

// Mock api client BEFORE importing hooks
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';
import {
  useConversations,
  useMessages,
  useUnreadCount,
  useStartConversation,
  useSendMessage,
  useMarkAsRead,
} from '@/hooks/use-messaging';
import type {
  ConversationDto,
  ConversationListResponse,
  MessageDto,
  MessageListResponse,
} from '@/types/messaging';

const mockConversation: ConversationDto = {
  id: 'conv-1',
  participantId: 'user-2',
  participantDisplayName: 'Alice Creator',
  participantUsername: 'alice_creator',
  participantProfilePictureUrl: 'https://example.com/alice.jpg',
  lastMessagePreview: 'Hey, want to collaborate?',
  lastMessageAt: new Date().toISOString(),
  unreadCount: 3,
  createdAt: new Date().toISOString(),
};

const mockConversationList: ConversationListResponse = {
  items: [mockConversation],
  totalCount: 1,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

const mockMessage: MessageDto = {
  id: 'msg-1',
  conversationId: 'conv-1',
  senderId: 'user-1',
  senderDisplayName: 'Bob Vlogger',
  senderUsername: 'bob_vlogger',
  content: 'Hey, want to collaborate?',
  isRead: false,
  createdAt: new Date().toISOString(),
};

const mockMessageList: MessageListResponse = {
  items: [mockMessage],
  totalCount: 1,
  page: 1,
  pageSize: 50,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

describe('useConversations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch conversations with default pagination', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockConversationList);

    const { result } = renderHook(() => useConversations(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockConversationList);
    expect(apiClient.get).toHaveBeenCalledWith('/conversations', {
      params: {
        page: 1,
        pageSize: 20,
      },
    });
  });

  it('should fetch conversations with custom page', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockConversationList);

    const { result } = renderHook(
      () => useConversations(3),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/conversations', {
      params: {
        page: 3,
        pageSize: 20,
      },
    });
  });

  it('should handle error when fetching conversations', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useConversations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useMessages', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch messages for conversation', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockMessageList);

    const { result } = renderHook(
      () => useMessages('conv-1'),
      { wrapper: createWrapper() }
    );

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockMessageList);
    expect(apiClient.get).toHaveBeenCalledWith('/conversations/conv-1/messages', {
      params: {
        page: 1,
        pageSize: 50,
      },
    });
  });

  it('should fetch messages with custom page size', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockMessageList);

    const { result } = renderHook(
      () => useMessages('conv-1', 2, 25),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/conversations/conv-1/messages', {
      params: {
        page: 2,
        pageSize: 25,
      },
    });
  });

  it('should handle error when fetching messages', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Conversation not found'));

    const { result } = renderHook(
      () => useMessages('conv-1'),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useUnreadCount', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch unread count', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ unreadCount: 5 });

    const { result } = renderHook(() => useUnreadCount(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual({ unreadCount: 5 });
    expect(apiClient.get).toHaveBeenCalledWith('/conversations/unread-count');
  });

  it('should handle error when fetching unread count', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Unauthorized'));

    const { result } = renderHook(() => useUnreadCount(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useStartConversation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should start conversation', async () => {
    const createdConversation: ConversationDto = {
      ...mockConversation,
      id: 'conv-new',
    };
    vi.mocked(apiClient.post).mockResolvedValue(createdConversation);

    const { result } = renderHook(() => useStartConversation(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      participantId: 'user-2',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/conversations', {
      participantId: 'user-2',
    });
  });

  it('should handle error when starting conversation', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('User not found'));

    const { result } = renderHook(() => useStartConversation(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      participantId: 'user-invalid',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useSendMessage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should send message', async () => {
    const sentMessage: MessageDto = {
      ...mockMessage,
      id: 'msg-new',
      content: 'Hello!',
    };
    vi.mocked(apiClient.post).mockResolvedValue(sentMessage);

    const { result } = renderHook(
      () => useSendMessage('conv-1'),
      { wrapper: createWrapper() }
    );

    result.current.mutate({
      content: 'Hello!',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/conversations/conv-1/messages', {
      content: 'Hello!',
    });
  });

  it('should handle error when sending message', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Message too long'));

    const { result } = renderHook(
      () => useSendMessage('conv-1'),
      { wrapper: createWrapper() }
    );

    result.current.mutate({
      content: 'Hello!',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useMarkAsRead', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should mark conversation as read', async () => {
    vi.mocked(apiClient.post).mockResolvedValue(undefined);

    const { result } = renderHook(
      () => useMarkAsRead('conv-1'),
      { wrapper: createWrapper() }
    );

    result.current.mutate();

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/conversations/conv-1/read');
  });

  it('should handle error when marking as read', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Conversation not found'));

    const { result } = renderHook(
      () => useMarkAsRead('conv-1'),
      { wrapper: createWrapper() }
    );

    result.current.mutate();

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});
