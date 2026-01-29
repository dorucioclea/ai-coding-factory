/**
 * Unit tests for use-shared-projects hooks
 * Story: ACF-013 - Shared Project Spaces
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useSharedProjects,
  useSharedProject,
  useProjectActivity,
  useAddProjectTask,
  useUpdateProjectTask,
  useAddProjectLink,
  useLeaveProject,
  useCloseProject,
} from '@/hooks/use-shared-projects';
import type {
  SharedProjectDto,
  SharedProjectDetailDto,
  SharedProjectListResponse,
  SharedProjectActivityListResponse,
  SharedProjectTaskDto,
  SharedProjectLinkDto,
} from '@/types/shared-project';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockProject: SharedProjectDto = {
  id: 'proj-1',
  name: 'Joint Video Project',
  description: 'A collaboration between creators',
  status: 'Active',
  collaborationRequestId: 'collab-1',
  ownerId: 'user-1',
  memberCount: 2,
  taskCount: 3,
  linkCount: 1,
  createdAt: new Date().toISOString(),
  members: [
    { id: 'mem-1', userId: 'user-1', role: 'Owner', joinedAt: new Date().toISOString() },
    { id: 'mem-2', userId: 'user-2', role: 'Member', joinedAt: new Date().toISOString() },
  ],
};

const mockDetailProject: SharedProjectDetailDto = {
  id: 'proj-1',
  name: 'Joint Video Project',
  description: 'A collaboration between creators',
  status: 'Active',
  collaborationRequestId: 'collab-1',
  ownerId: 'user-1',
  createdAt: new Date().toISOString(),
  members: [
    { id: 'mem-1', userId: 'user-1', role: 'Owner', joinedAt: new Date().toISOString() },
    { id: 'mem-2', userId: 'user-2', role: 'Member', joinedAt: new Date().toISOString() },
  ],
  tasks: [
    {
      id: 'task-1',
      createdByUserId: 'user-1',
      title: 'Film intro',
      status: 'Open',
      createdAt: new Date().toISOString(),
    },
  ],
  links: [
    {
      id: 'link-1',
      addedByUserId: 'user-1',
      title: 'Script',
      url: 'https://docs.google.com/doc/123',
      createdAt: new Date().toISOString(),
    },
  ],
};

const mockListResponse: SharedProjectListResponse = {
  items: [mockProject],
  totalCount: 1,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

const mockActivityResponse: SharedProjectActivityListResponse = {
  items: [
    {
      id: 'act-1',
      userId: 'user-1',
      activityType: 'ProjectCreated',
      message: 'Project created',
      createdAt: new Date().toISOString(),
    },
  ],
  totalCount: 1,
  page: 1,
  pageSize: 50,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

describe('useSharedProjects', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch shared projects list', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(() => useSharedProjects(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockListResponse);
    expect(apiClient.get).toHaveBeenCalledWith('/projects', {
      params: { status: undefined, page: 1, pageSize: 20 },
    });
  });

  it('should fetch with status filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockListResponse);

    const { result } = renderHook(() => useSharedProjects('Active', 1, 10), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith('/projects', {
      params: { status: 'Active', page: 1, pageSize: 10 },
    });
  });

  it('should not fetch when disabled', () => {
    const { result } = renderHook(() => useSharedProjects(undefined, 1, 20, false), {
      wrapper: createWrapper(),
    });

    expect(result.current.isFetching).toBe(false);
    expect(apiClient.get).not.toHaveBeenCalled();
  });
});

describe('useSharedProject', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch project by ID', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDetailProject);

    const { result } = renderHook(() => useSharedProject('proj-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockDetailProject);
    expect(apiClient.get).toHaveBeenCalledWith('/projects/proj-1');
  });

  it('should not fetch when projectId is empty', () => {
    const { result } = renderHook(() => useSharedProject(''), {
      wrapper: createWrapper(),
    });

    expect(result.current.isFetching).toBe(false);
    expect(apiClient.get).not.toHaveBeenCalled();
  });
});

describe('useProjectActivity', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch project activity', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockActivityResponse);

    const { result } = renderHook(() => useProjectActivity('proj-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockActivityResponse);
    expect(apiClient.get).toHaveBeenCalledWith('/projects/proj-1/activity', {
      params: { page: 1, pageSize: 50 },
    });
  });
});

describe('useAddProjectTask', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should add a task to a project', async () => {
    const mockTask: SharedProjectTaskDto = {
      id: 'task-new',
      createdByUserId: 'user-1',
      title: 'New task',
      status: 'Open',
      createdAt: new Date().toISOString(),
    };
    vi.mocked(apiClient.post).mockResolvedValue(mockTask);

    const { result } = renderHook(() => useAddProjectTask('proj-1'), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ title: 'New task', description: 'Task description' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/projects/proj-1/tasks', {
      title: 'New task',
      description: 'Task description',
    });
  });
});

describe('useUpdateProjectTask', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update a task status', async () => {
    vi.mocked(apiClient.put).mockResolvedValue(mockDetailProject);

    const { result } = renderHook(() => useUpdateProjectTask('proj-1'), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ taskId: 'task-1', status: 'Completed' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.put).toHaveBeenCalledWith('/projects/proj-1/tasks/task-1', {
      status: 'Completed',
    });
  });
});

describe('useAddProjectLink', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should add a link to a project', async () => {
    const mockLink: SharedProjectLinkDto = {
      id: 'link-new',
      addedByUserId: 'user-1',
      title: 'New link',
      url: 'https://example.com',
      createdAt: new Date().toISOString(),
    };
    vi.mocked(apiClient.post).mockResolvedValue(mockLink);

    const { result } = renderHook(() => useAddProjectLink('proj-1'), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ title: 'New link', url: 'https://example.com' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/projects/proj-1/links', {
      title: 'New link',
      url: 'https://example.com',
    });
  });
});

describe('useLeaveProject', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should leave a project', async () => {
    vi.mocked(apiClient.post).mockResolvedValue(undefined);

    const { result } = renderHook(() => useLeaveProject(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('proj-1');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/projects/proj-1/leave');
  });
});

describe('useCloseProject', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should close a project', async () => {
    vi.mocked(apiClient.post).mockResolvedValue(undefined);

    const { result } = renderHook(() => useCloseProject(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('proj-1');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/projects/proj-1/close');
  });
});
