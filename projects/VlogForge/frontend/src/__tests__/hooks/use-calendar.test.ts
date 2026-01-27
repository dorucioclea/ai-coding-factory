/**
 * Unit tests for use-calendar hook
 * ACF-015 Phase 4 - Calendar
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useCalendarMonth,
  useUpdateSchedule,
  useUnschedule,
} from '@/hooks/use-calendar';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    patch: vi.fn(),
  },
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';
import { toast } from 'sonner';

const mockCalendarMonth = {
  year: 2024,
  month: 1,
  days: [
    {
      date: '2024-01-15',
      items: [
        {
          id: 'content-1',
          title: 'Test Content',
          status: 'Scheduled' as const,
          platformTags: ['YouTube'],
          scheduledDate: '2024-01-15',
        },
      ],
    },
    {
      date: '2024-01-20',
      items: [
        {
          id: 'content-2',
          title: 'Another Content',
          status: 'Draft' as const,
          platformTags: ['TikTok'],
          scheduledDate: '2024-01-20',
        },
      ],
    },
  ],
};

const mockContentResponse = {
  id: 'content-1',
  title: 'Test Content',
  status: 'Scheduled' as const,
  platformTags: ['YouTube'],
  scheduledDate: '2024-01-16',
};

describe('useCalendarMonth', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch calendar month data successfully', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockCalendarMonth);

    const { result } = renderHook(() => useCalendarMonth(2024, 1), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockCalendarMonth);
    expect(apiClient.get).toHaveBeenCalledWith('/calendar?month=2024-01');
  });

  it('should pad single digit months', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockCalendarMonth);

    renderHook(() => useCalendarMonth(2024, 5), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(apiClient.get).toHaveBeenCalledWith('/calendar?month=2024-05');
    });
  });

  it('should handle error state', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useCalendarMonth(2024, 1), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useUpdateSchedule', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update schedule successfully', async () => {
    vi.mocked(apiClient.patch).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useUpdateSchedule(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      scheduledDate: '2024-01-16',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/content/content-1/schedule', {
      scheduledDate: '2024-01-16',
    });
    expect(toast.success).toHaveBeenCalled();
  });

  it('should show error toast on failure', async () => {
    vi.mocked(apiClient.patch).mockRejectedValue(new Error('Update failed'));

    const { result } = renderHook(() => useUpdateSchedule(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      scheduledDate: '2024-01-16',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalledWith('Failed to update schedule', expect.any(Object));
  });
});

describe('useUnschedule', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should unschedule content successfully', async () => {
    vi.mocked(apiClient.patch).mockResolvedValue({
      ...mockContentResponse,
      scheduledDate: null,
    });

    const { result } = renderHook(() => useUnschedule(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('content-1');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.patch).toHaveBeenCalledWith('/content/content-1/schedule', {
      scheduledDate: null,
    });
    expect(toast.success).toHaveBeenCalledWith('Schedule removed');
  });

  it('should show error toast on failure', async () => {
    vi.mocked(apiClient.patch).mockRejectedValue(new Error('Unschedule failed'));

    const { result } = renderHook(() => useUnschedule(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('content-1');

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalled();
  });
});
