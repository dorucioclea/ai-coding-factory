/**
 * Unit tests for use-discovery hook
 * Story: ACF-010 - Creator Discovery
 */

import { renderHook, waitFor, act } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useDiscoverCreators,
  useNicheCategories,
  usePlatforms,
  useDiscoveryFilters,
  useDiscoveryHelpers,
} from '@/hooks/use-discovery';
import type { DiscoveryResponse } from '@/types/discovery';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockCreator = {
  id: 'creator-1',
  username: 'testcreator',
  displayName: 'Test Creator',
  bio: 'A test creator bio',
  profilePictureUrl: 'https://example.com/avatar.jpg',
  openToCollaborations: true,
  nicheTags: ['gaming', 'tech'],
  platforms: [
    { platformType: 'YouTube', handle: '@testcreator', followerCount: 50000 },
    { platformType: 'TikTok', handle: '@testcreator', followerCount: 25000 },
  ],
  totalFollowers: 75000,
};

const mockDiscoveryResponse: DiscoveryResponse = {
  items: [mockCreator],
  totalCount: 1,
  nextCursor: undefined,
  hasMore: false,
  pageSize: 20,
};

const mockNiches = ['Gaming', 'Tech', 'Lifestyle', 'Beauty', 'Travel'];
const mockPlatforms = ['YouTube', 'TikTok', 'Instagram', 'Twitter', 'Twitch'];

describe('useDiscoverCreators', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch creators without filters', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(() => useDiscoverCreators(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data?.pages[0]?.items).toHaveLength(1);
    expect(result.current.data?.pages[0]?.items[0]).toEqual(mockCreator);
    expect(apiClient.get).toHaveBeenCalledWith('/discovery?pageSize=20');
  });

  it('should fetch creators with niche filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(
      () => useDiscoverCreators({ niches: ['gaming', 'tech'] }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith(
      expect.stringContaining('niches=gaming%2Ctech')
    );
  });

  it('should fetch creators with platform filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(
      () => useDiscoverCreators({ platforms: ['YouTube', 'TikTok'] }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith(
      expect.stringContaining('platforms=YouTube%2CTikTok')
    );
  });

  it('should fetch creators with audience size filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(
      () => useDiscoverCreators({ audienceSize: 'Medium' }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith(
      expect.stringContaining('audienceSize=Medium')
    );
  });

  it('should fetch creators with search term', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(
      () => useDiscoverCreators({ search: 'gaming creator' }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith(
      expect.stringContaining('search=gaming%20creator')
    );
  });

  it('should fetch creators with open to collab filter', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockDiscoveryResponse);

    const { result } = renderHook(
      () => useDiscoverCreators({ openToCollab: true }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(apiClient.get).toHaveBeenCalledWith(
      expect.stringContaining('openToCollab=true')
    );
  });

  it('should handle pagination with cursor', async () => {
    const responseWithMore: DiscoveryResponse = {
      ...mockDiscoveryResponse,
      hasMore: true,
      nextCursor: 'cursor-123',
    };
    vi.mocked(apiClient.get).mockResolvedValue(responseWithMore);

    const { result } = renderHook(() => useDiscoverCreators(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.hasNextPage).toBe(true);
  });

  it('should handle error', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useDiscoverCreators(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});

describe('useNicheCategories', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch niche categories', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockNiches);

    const { result } = renderHook(() => useNicheCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockNiches);
    expect(apiClient.get).toHaveBeenCalledWith('/discovery/niches');
  });
});

describe('usePlatforms', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch platforms', async () => {
    vi.mocked(apiClient.get).mockResolvedValue(mockPlatforms);

    const { result } = renderHook(() => usePlatforms(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockPlatforms);
    expect(apiClient.get).toHaveBeenCalledWith('/discovery/platforms');
  });
});

describe('useDiscoveryFilters', () => {
  it('should initialize with empty filters', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    expect(result.current.filters).toEqual({});
    expect(result.current.hasActiveFilters).toBe(false);
    expect(result.current.activeFilterCount).toBe(0);
  });

  it('should update niches', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateNiches(['gaming', 'tech']);
    });

    expect(result.current.filters.niches).toEqual(['gaming', 'tech']);
    expect(result.current.hasActiveFilters).toBe(true);
    expect(result.current.activeFilterCount).toBe(1);
  });

  it('should update platforms', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updatePlatforms(['YouTube', 'TikTok']);
    });

    expect(result.current.filters.platforms).toEqual(['YouTube', 'TikTok']);
  });

  it('should update audience size', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateAudienceSize('Large');
    });

    expect(result.current.filters.audienceSize).toBe('Large');
  });

  it('should update search', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateSearch('gaming content');
    });

    expect(result.current.filters.search).toBe('gaming content');
  });

  it('should trim empty search', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateSearch('   ');
    });

    expect(result.current.filters.search).toBeUndefined();
  });

  it('should update open to collab filter', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateOpenToCollab(true);
    });

    expect(result.current.filters.openToCollab).toBe(true);
  });

  it('should clear all filters', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateNiches(['gaming']);
      result.current.updatePlatforms(['YouTube']);
      result.current.updateAudienceSize('Medium');
      result.current.updateSearch('test');
      result.current.updateOpenToCollab(true);
    });

    expect(result.current.activeFilterCount).toBe(5);

    act(() => {
      result.current.clearFilters();
    });

    expect(result.current.filters).toEqual({});
    expect(result.current.hasActiveFilters).toBe(false);
    expect(result.current.activeFilterCount).toBe(0);
  });

  it('should count active filters correctly', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateNiches(['gaming', 'tech']);
      result.current.updatePlatforms(['YouTube']);
    });

    expect(result.current.activeFilterCount).toBe(2);

    act(() => {
      result.current.updateAudienceSize('Small');
    });

    expect(result.current.activeFilterCount).toBe(3);
  });

  it('should clear niches when empty array passed', () => {
    const { result } = renderHook(() => useDiscoveryFilters());

    act(() => {
      result.current.updateNiches(['gaming']);
    });

    expect(result.current.filters.niches).toEqual(['gaming']);

    act(() => {
      result.current.updateNiches([]);
    });

    expect(result.current.filters.niches).toBeUndefined();
  });
});

describe('useDiscoveryHelpers', () => {
  it('should format follower count in millions', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.formatFollowerCount(1500000)).toBe('1.5M');
    expect(result.current.formatFollowerCount(2000000)).toBe('2.0M');
  });

  it('should format follower count in thousands', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.formatFollowerCount(50000)).toBe('50.0K');
    expect(result.current.formatFollowerCount(1500)).toBe('1.5K');
  });

  it('should format small follower count', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.formatFollowerCount(500)).toBe('500');
    expect(result.current.formatFollowerCount(0)).toBe('0');
  });

  it('should get audience size label for large', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.getAudienceSizeLabel(150000)).toBe('100K+');
  });

  it('should get audience size label for medium', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.getAudienceSizeLabel(50000)).toBe('10K-100K');
  });

  it('should get audience size label for small', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.getAudienceSizeLabel(5000)).toBe('1K-10K');
  });

  it('should get audience size label for tiny', () => {
    const { result } = renderHook(() => useDiscoveryHelpers());

    expect(result.current.getAudienceSizeLabel(500)).toBe('<1K');
  });
});
