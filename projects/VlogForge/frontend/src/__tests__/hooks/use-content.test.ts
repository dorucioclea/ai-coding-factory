/**
 * Unit tests for use-content hook
 * ACF-015 Phase 3 - Content Ideas
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useContentIdeas,
  useContentIdea,
  useCreateIdea,
  useUpdateIdea,
  useDeleteIdea,
  useUpdateIdeaStatus,
} from '@/hooks/use-content';
import { IdeaStatus } from '@/types';

// Mock the content service
vi.mock('@/lib/content', () => ({
  contentService: {
    getContentIdeas: vi.fn(),
    getContentIdea: vi.fn(),
    createContentIdea: vi.fn(),
    updateContentIdea: vi.fn(),
    deleteContentIdea: vi.fn(),
    updateContentIdeaStatus: vi.fn(),
  },
}));

import { contentService } from '@/lib/content';

const mockContentIdea = {
  id: '1',
  userId: 'user-1',
  title: 'Test Content Idea',
  notes: 'Test notes',
  status: IdeaStatus.Idea,
  platformTags: ['YouTube', 'TikTok'],
  scheduledDate: undefined,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

describe('useContentIdeas', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch content ideas successfully', async () => {
    const mockIdeas = [mockContentIdea];
    vi.mocked(contentService.getContentIdeas).mockResolvedValue(mockIdeas);

    const { result } = renderHook(() => useContentIdeas(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockIdeas);
    expect(contentService.getContentIdeas).toHaveBeenCalledTimes(1);
  });

  it('should fetch content ideas with filters', async () => {
    const filters = { status: IdeaStatus.Idea };
    vi.mocked(contentService.getContentIdeas).mockResolvedValue([mockContentIdea]);

    const { result } = renderHook(() => useContentIdeas(filters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(contentService.getContentIdeas).toHaveBeenCalledWith(filters);
  });

  it('should handle error state', async () => {
    const error = new Error('Failed to fetch');
    vi.mocked(contentService.getContentIdeas).mockRejectedValue(error);

    const { result } = renderHook(() => useContentIdeas(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useContentIdea', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch single content idea', async () => {
    vi.mocked(contentService.getContentIdea).mockResolvedValue(mockContentIdea);

    const { result } = renderHook(() => useContentIdea('1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockContentIdea);
    expect(contentService.getContentIdea).toHaveBeenCalledWith('1');
  });

  it('should not fetch when id is empty', async () => {
    const { result } = renderHook(() => useContentIdea(''), {
      wrapper: createWrapper(),
    });

    // Should not be loading since query is disabled
    expect(result.current.isLoading).toBe(false);
    expect(contentService.getContentIdea).not.toHaveBeenCalled();
  });
});

describe('useCreateIdea', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should create content idea', async () => {
    const newIdea = {
      title: 'New Idea',
      notes: 'Notes',
      platformTags: ['YouTube'],
    };
    vi.mocked(contentService.createContentIdea).mockResolvedValue({
      ...mockContentIdea,
      ...newIdea,
    });

    const { result } = renderHook(() => useCreateIdea(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(newIdea);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(contentService.createContentIdea).toHaveBeenCalledWith(newIdea);
  });
});

describe('useUpdateIdea', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update content idea', async () => {
    const updateData = {
      id: '1',
      data: { title: 'Updated Title' },
    };
    vi.mocked(contentService.updateContentIdea).mockResolvedValue({
      ...mockContentIdea,
      title: 'Updated Title',
    });

    const { result } = renderHook(() => useUpdateIdea(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(updateData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(contentService.updateContentIdea).toHaveBeenCalledWith('1', { title: 'Updated Title' });
  });
});

describe('useDeleteIdea', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should delete content idea', async () => {
    vi.mocked(contentService.deleteContentIdea).mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteIdea(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(contentService.deleteContentIdea).toHaveBeenCalledWith('1');
  });
});

describe('useUpdateIdeaStatus', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update content status', async () => {
    vi.mocked(contentService.updateContentIdeaStatus).mockResolvedValue({
      ...mockContentIdea,
      status: IdeaStatus.Draft,
    });

    const { result } = renderHook(() => useUpdateIdeaStatus(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ id: '1', status: { status: IdeaStatus.Draft } });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(contentService.updateContentIdeaStatus).toHaveBeenCalledWith('1', { status: IdeaStatus.Draft });
  });
});
