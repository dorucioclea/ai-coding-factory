/**
 * Unit tests for use-collaborations hooks
 * Story: ACF-011 - Collaboration Requests
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useCollaborationInbox,
  useSentCollaborations,
  useSendCollaborationRequest,
  useAcceptCollaborationRequest,
  useDeclineCollaborationRequest,
} from '@/hooks/use-collaborations';
import type {
  CollaborationRequestDto,
  CollaborationRequestListResponse,
} from '@/types/collaboration';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockRequest: CollaborationRequestDto = {
  id: 'req-1',
  senderId: 'user-1',
  recipientId: 'user-2',
  senderDisplayName: 'Alice Creator',
  senderUsername: 'alice_creator',
  senderProfilePictureUrl: 'https://example.com/alice.jpg',
  recipientDisplayName: 'Bob Vlogger',
  recipientUsername: 'bob_vlogger',
  recipientProfilePictureUrl: 'https://example.com/bob.jpg',
  message: 'Let us collaborate on a tech review video!',
  status: 'Pending',
  expiresAt: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString(),
  createdAt: new Date().toISOString(),
  isExpired: false,
};

const mockListResponse: CollaborationRequestListResponse = {
  items: [mockRequest],
  totalCount: 1,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

describe('useCollaborationInbox', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch inbox without status filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(() => useCollaborationInbox(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockListResponse);
    expect(apiClient.get).toHaveBeenCalledWith('/collaborations/inbox', {
      params: {
        status: undefined,
        page: 1,
        pageSize: 20,
      },
    });
  });

  it('should fetch inbox with Pending status filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(
      () => useCollaborationInbox('Pending'),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/collaborations/inbox', {
      params: {
        status: 'Pending',
        page: 1,
        pageSize: 20,
      },
    });
  });

  it('should fetch inbox with page parameter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(
      () => useCollaborationInbox(undefined, 3),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/collaborations/inbox', {
      params: {
        status: undefined,
        page: 3,
        pageSize: 20,
      },
    });
  });

  it('should handle error when fetching inbox', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useCollaborationInbox(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useSentCollaborations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch sent requests without status filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(() => useSentCollaborations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockListResponse);
    expect(apiClient.get).toHaveBeenCalledWith('/collaborations/sent', {
      params: {
        status: undefined,
        page: 1,
        pageSize: 20,
      },
    });
  });

  it('should fetch sent requests with Accepted status filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(
      () => useSentCollaborations('Accepted'),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/collaborations/sent', {
      params: {
        status: 'Accepted',
        page: 1,
        pageSize: 20,
      },
    });
  });

  it('should handle error when fetching sent requests', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Server error'));

    const { result } = renderHook(() => useSentCollaborations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useSendCollaborationRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should send collaboration request', async () => {
    const createdRequest: CollaborationRequestDto = {
      ...mockRequest,
      id: 'req-new',
    };
    vi.mocked(apiClient.post).mockResolvedValue(createdRequest);

    const { result } = renderHook(() => useSendCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      recipientId: 'user-2',
      message: 'Let us collaborate!',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/collaborations/request', {
      recipientId: 'user-2',
      message: 'Let us collaborate!',
    });
  });

  it('should handle error when sending request', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Rate limit exceeded'));

    const { result } = renderHook(() => useSendCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      recipientId: 'user-2',
      message: 'Let us collaborate!',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useAcceptCollaborationRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should accept collaboration request', async () => {
    const acceptedRequest: CollaborationRequestDto = {
      ...mockRequest,
      status: 'Accepted',
      respondedAt: new Date().toISOString(),
    };
    vi.mocked(apiClient.post).mockResolvedValue(acceptedRequest);

    const { result } = renderHook(() => useAcceptCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('req-1');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/collaborations/req-1/accept');
  });

  it('should handle error when accepting request', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Request not found'));

    const { result } = renderHook(() => useAcceptCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('req-invalid');

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('useDeclineCollaborationRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should decline collaboration request with reason', async () => {
    const declinedRequest: CollaborationRequestDto = {
      ...mockRequest,
      status: 'Declined',
      declineReason: 'Not interested at this time',
      respondedAt: new Date().toISOString(),
    };
    vi.mocked(apiClient.post).mockResolvedValue(declinedRequest);

    const { result } = renderHook(() => useDeclineCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      requestId: 'req-1',
      reason: 'Not interested at this time',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/collaborations/req-1/decline', {
      reason: 'Not interested at this time',
    });
  });

  it('should decline collaboration request without reason', async () => {
    const declinedRequest: CollaborationRequestDto = {
      ...mockRequest,
      status: 'Declined',
      respondedAt: new Date().toISOString(),
    };
    vi.mocked(apiClient.post).mockResolvedValue(declinedRequest);

    const { result } = renderHook(() => useDeclineCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ requestId: 'req-1' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/collaborations/req-1/decline', {
      reason: undefined,
    });
  });

  it('should handle error when declining request', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Already declined'));

    const { result } = renderHook(() => useDeclineCollaborationRequest(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ requestId: 'req-1' });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});
