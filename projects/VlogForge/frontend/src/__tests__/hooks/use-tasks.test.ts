/**
 * Unit tests for use-tasks hook
 * Stories: ACF-008, ACF-014 - Task Assignment & Task View
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
  useGroupedTasks,
} from '@/hooks/use-tasks';
import { AssignmentStatus } from '@/types';

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
  contentItemId: 'content-1',
  teamId: 'team-1',
  assigneeId: 'user-1',
  assignedById: 'user-2',
  dueDate: '2024-02-01T00:00:00Z',
  status: AssignmentStatus.NotStarted,
  notes: 'Test notes',
  isOverdue: false,
  comments: [],
  history: [],
  createdAt: '2024-01-01T00:00:00Z',
};

const mockTaskWithComments = {
  ...mockTask,
  comments: [
    {
      id: 'comment-1',
      content: 'Test comment',
      authorId: 'user-1',
      isEdited: false,
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
      items: [mockTask],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    });

    const { result } = renderHook(() => useMyTasks(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data?.items).toEqual([mockTask]);
    expect(apiClient.get).toHaveBeenCalledWith('/tasks/mine', { params: undefined });
  });

  it('should fetch tasks with filters', async () => {
    const filters = { status: AssignmentStatus.NotStarted };
    vi.mocked(apiClient.get).mockResolvedValue({
      items: [mockTask],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
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
    expect(apiClient.get).toHaveBeenCalledWith('/tasks/task-1', {
      params: { includeComments: true, includeHistory: false },
    });
  });

  it('should fetch task with history when includeHistory is true', async () => {
    const mockWithHistory = {
      ...mockTask,
      history: [
        {
          id: 'hist-1',
          changedByUserId: 'user-1',
          action: 0,
          description: 'Task created',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ],
    };
    vi.mocked(apiClient.get).mockResolvedValue(mockWithHistory);

    const { result } = renderHook(() => useTask('task-1', true), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/tasks/task-1', {
      params: { includeComments: true, includeHistory: true },
    });
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
      status: AssignmentStatus.InProgress,
    });

    const { result } = renderHook(() => useUpdateTaskStatus(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      status: AssignmentStatus.InProgress,
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/tasks/task-1/status', {
      status: AssignmentStatus.InProgress,
    });
  });

  it('should handle status transition to Completed', async () => {
    vi.mocked(apiClient.patch).mockResolvedValue({
      ...mockTask,
      status: AssignmentStatus.Completed,
    });

    const { result } = renderHook(() => useUpdateTaskStatus(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      taskId: 'task-1',
      status: AssignmentStatus.Completed,
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/tasks/task-1/status', {
      status: AssignmentStatus.Completed,
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
      isEdited: false,
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

describe('useGroupedTasks', () => {
  it('should group tasks by status', () => {
    const tasks = [
      { ...mockTask, id: '1', status: AssignmentStatus.NotStarted, dueDate: '2024-02-01T00:00:00Z' },
      { ...mockTask, id: '2', status: AssignmentStatus.InProgress, dueDate: '2024-02-02T00:00:00Z' },
      { ...mockTask, id: '3', status: AssignmentStatus.Completed, dueDate: '2024-02-03T00:00:00Z' },
      { ...mockTask, id: '4', status: AssignmentStatus.NotStarted, dueDate: '2024-01-15T00:00:00Z' },
    ];

    const { result } = renderHook(() => useGroupedTasks(tasks));

    expect(result.current[AssignmentStatus.NotStarted]).toHaveLength(2);
    expect(result.current[AssignmentStatus.InProgress]).toHaveLength(1);
    expect(result.current[AssignmentStatus.Completed]).toHaveLength(1);
  });

  it('should sort tasks by due date within each group', () => {
    const tasks = [
      { ...mockTask, id: '1', status: AssignmentStatus.NotStarted, dueDate: '2024-03-01T00:00:00Z' },
      { ...mockTask, id: '2', status: AssignmentStatus.NotStarted, dueDate: '2024-01-01T00:00:00Z' },
      { ...mockTask, id: '3', status: AssignmentStatus.NotStarted, dueDate: '2024-02-01T00:00:00Z' },
    ];

    const { result } = renderHook(() => useGroupedTasks(tasks));

    const notStarted = result.current[AssignmentStatus.NotStarted];
    expect(notStarted[0]!.id).toBe('2'); // Jan - earliest
    expect(notStarted[1]!.id).toBe('3'); // Feb
    expect(notStarted[2]!.id).toBe('1'); // Mar - latest
  });

  it('should return empty groups when no tasks', () => {
    const { result } = renderHook(() => useGroupedTasks([]));

    expect(result.current[AssignmentStatus.NotStarted]).toHaveLength(0);
    expect(result.current[AssignmentStatus.InProgress]).toHaveLength(0);
    expect(result.current[AssignmentStatus.Completed]).toHaveLength(0);
  });
});
