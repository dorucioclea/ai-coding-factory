/**
 * Unit tests for use-profile hook
 * ACF-015 Phase 1 - Profile Management
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useMyProfile,
  useProfile,
  useUpdateProfile,
} from '@/hooks/use-profile';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockProfile = {
  id: 'profile-1',
  userId: 'user-1',
  displayName: 'Test User',
  bio: 'Test bio',
  avatarUrl: 'https://example.com/avatar.jpg',
  nicheTags: ['tech', 'gaming'],
  openToCollaborations: true,
  collaborationPreferences: 'Looking for video collaborations',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

const mockPublicProfile = {
  id: 'profile-1',
  username: 'testuser',
  displayName: 'Test User',
  bio: 'Test bio',
  avatarUrl: 'https://example.com/avatar.jpg',
  nicheTags: ['tech', 'gaming'],
  openToCollaborations: true,
  collaborationPreferences: 'Looking for video collaborations',
};

describe('useMyProfile', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch current user profile successfully', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: mockProfile });

    const { result } = renderHook(() => useMyProfile(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockProfile);
    expect(apiClient.get).toHaveBeenCalledWith('/profiles/me');
  });

  it('should handle error when profile not found', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Profile not found'));

    const { result } = renderHook(() => useMyProfile(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useProfile', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch public profile by username', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: mockPublicProfile });

    const { result } = renderHook(() => useProfile('testuser'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockPublicProfile);
    expect(apiClient.get).toHaveBeenCalledWith('/profiles/testuser');
  });

  it('should not fetch when username is empty', async () => {
    const { result } = renderHook(() => useProfile(''), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(apiClient.get).not.toHaveBeenCalled();
  });

  it('should handle 404 error for non-existent profile', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Profile not found'));

    const { result } = renderHook(() => useProfile('nonexistent'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('useUpdateProfile', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should update profile successfully', async () => {
    const updateData = {
      displayName: 'Updated Name',
      bio: 'Updated bio',
    };
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { ...mockProfile, ...updateData },
    });

    const { result } = renderHook(() => useUpdateProfile(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(updateData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.put).toHaveBeenCalledWith('/profiles/me', updateData);
  });

  it('should update collaboration settings', async () => {
    const updateData = {
      openToCollaborations: false,
      collaborationPreferences: undefined,
    };
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { ...mockProfile, ...updateData },
    });

    const { result } = renderHook(() => useUpdateProfile(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(updateData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.put).toHaveBeenCalledWith('/profiles/me', updateData);
  });

  it('should update niche tags', async () => {
    const updateData = {
      nicheTags: ['tech', 'gaming', 'music'],
    };
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { ...mockProfile, ...updateData },
    });

    const { result } = renderHook(() => useUpdateProfile(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(updateData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.put).toHaveBeenCalledWith('/profiles/me', updateData);
  });

  it('should handle validation error', async () => {
    vi.mocked(apiClient.put).mockRejectedValue(new Error('Display name too long'));

    const { result } = renderHook(() => useUpdateProfile(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ displayName: 'A'.repeat(100) });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});
