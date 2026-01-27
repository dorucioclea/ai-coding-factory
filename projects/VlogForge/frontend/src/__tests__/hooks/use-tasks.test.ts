/**
 * Unit tests for use-tasks hook
 * ACF-015 Phase 6 - Task Assignment
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useMyTasks,
  useTask,
  useAssignTask,
  useUpdateTaskStatus,
  useAddComment,
} from '@/hooks/use-tasks';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    patch: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockTask = {
  id: 'task-1',
  contentIdeaId: 'content-1',
  contentTitle: 'Test Content',
  assigneeId: 'user-1',
  assigneeName: 'Test User',
  dueDate: '2024-02-01',
  status: 'Pending' as const,
  assignedAt: '2024-01-01T00:00:00Z',
  assignedBy: 'user-2',
  comments: [],
};

const mockTaskWithComments = {
  ...mockTask,
  comments: [
    {
      id: 'comment-1',
      content: 'Test comment',
      authorId: 'user-1',
      authorName: 'Test User',
      createdAt: '2024-01-02T00:00:00Z',
    },
  ],
};

describe('useMyTasks', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch user tasks successfully', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      tasks: [mockTask],
      total: 1,
    });

    const { result } = renderHook(() => useMyTasks(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data?.tasks).toEqual([mockTask]);
    expect(apiClient.get).toHaveBeenCalledWith('/tasks/mine', { params: undefined });
  });

  it('should fetch tasks with filters', async () => {
    const filters = { status: 'Pending' as const };
    vi.mocked(apiClient.get).mockResolvedValue({
      tasks: [mockTask],
      total: 1,
    });

    const { result } = renderHook(() => useMyTasks(filters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/tasks/mine', { params: filters });
  });

  it('should handle error state', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useMyTasks(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('useTask', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch task detail with comments', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockTaskWithComments);

    const { result } = renderHook(() => useTask('task-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockTaskWithComments);
    expect(apiClient.get).toHaveBeenCalledWith('/tasks/task-1');
  });

  it('should not fetch when id is empty', async () => {
    const { result } = renderHook(() => useTask(''), {
      wrapper: createWrapper(),
    });

    // Query is disabled when id is falsy
    expect(result.current.fetchStatus).toBe('idle');
    expect(apiClient.get).not.toHaveBeenCalled();
  });
});

describe('useAssignTask', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should assign task successfully', async () => {
    const assignData = {
      contentItemId: 'content-1',
      assigneeId: 'user-1',
      dueDate: '2024-02-01',
    };
    vi.mocked(apiClient.post).mockResolvedValue(mockTask);

    const { result } = renderHook(() => useAssignTask(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(assignData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    // The hook posts to /content/{contentItemId}/assign with the rest of the data
    expect(apiClient.post).toHaveBeenCalledWith('/content/content-1/assign', {
      assigneeId: 'user-1',
      dueDate: '2024-02-01',
    });
  });
});

describe('useUpdateTaskStatus', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update task status successfully', async () => {
    vi.mocked(apiClient.patch).mockResolvedValue({
      ...mockTask,
      status: 'InProgress',
    });

    const { result } = renderHook(() => useUpdateTaskStatus(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      status: 'InProgress',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/tasks/task-1/status', {
      status: 'InProgress',
    });
  });

  it('should handle status transition to Completed', async () => {
    vi.mocked(apiClient.patch).mockResolvedValue({
      ...mockTask,
      status: 'Completed',
    });

    const { result } = renderHook(() => useUpdateTaskStatus(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      status: 'Completed',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/tasks/task-1/status', {
      status: 'Completed',
    });
  });
});

describe('useAddComment', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should add comment successfully', async () => {
    const newComment = {
      id: 'comment-2',
      content: 'New comment',
      authorId: 'user-1',
      authorName: 'Test User',
      createdAt: '2024-01-03T00:00:00Z',
    };
    vi.mocked(apiClient.post).mockResolvedValue(newComment);

    const { result } = renderHook(() => useAddComment(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      content: 'New comment',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/tasks/task-1/comments', {
      content: 'New comment',
    });
  });

  it('should handle empty comment error', async () => {
    vi.mocked(apiClient.post).mockRejectedValue(new Error('Comment cannot be empty'));

    const { result } = renderHook(() => useAddComment(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      content: '',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});
