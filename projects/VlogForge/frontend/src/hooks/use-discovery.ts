'use client';

import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  DiscoveryResponse,
  DiscoveryFilters,
  DiscoveryQueryParams,
  AudienceSizeRange,
} from '@/types/discovery';

/**
 * Build query string from filters
 */
function buildQueryString(params: DiscoveryQueryParams): string {
  const queryParts: string[] = [];

  if (params.niches && params.niches.length > 0) {
    queryParts.push(`niches=${encodeURIComponent(params.niches.join(','))}`);
  }
  if (params.platforms && params.platforms.length > 0) {
    queryParts.push(`platforms=${encodeURIComponent(params.platforms.join(','))}`);
  }
  if (params.audienceSize) {
    queryParts.push(`audienceSize=${encodeURIComponent(params.audienceSize)}`);
  }
  if (params.search) {
    queryParts.push(`search=${encodeURIComponent(params.search)}`);
  }
  if (params.openToCollab !== undefined) {
    queryParts.push(`openToCollab=${params.openToCollab}`);
  }
  if (params.cursor) {
    queryParts.push(`cursor=${encodeURIComponent(params.cursor)}`);
  }
  if (params.pageSize) {
    queryParts.push(`pageSize=${params.pageSize}`);
  }

  return queryParts.length > 0 ? `?${queryParts.join('&')}` : '';
}

/**
 * Hook for discovering creators with infinite scroll
 * Story: ACF-010
 */
export function useDiscoverCreators(filters: DiscoveryFilters = {}, pageSize = 20) {
  const queryKey = queryKeys.discovery.creators({ ...filters, pageSize });

  return useInfiniteQuery({
    queryKey,
    queryFn: async ({ pageParam }: { pageParam: string | undefined }) => {
      const queryString = buildQueryString({
        ...filters,
        cursor: pageParam,
        pageSize,
      });

      const response = await apiClient.get<DiscoveryResponse>(
        `/discovery${queryString}`
      );
      return response;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook to get available niche categories
 * Story: ACF-010
 */
export function useNicheCategories() {
  return useQuery({
    queryKey: queryKeys.discovery.niches(),
    queryFn: async () => {
      const response = await apiClient.get<string[]>('/discovery/niches');
      return response;
    },
    staleTime: 1000 * 60 * 60, // 1 hour - niches rarely change
  });
}

/**
 * Hook to get available platforms
 * Story: ACF-010
 */
export function usePlatforms() {
  return useQuery({
    queryKey: queryKeys.discovery.platforms(),
    queryFn: async () => {
      const response = await apiClient.get<string[]>('/discovery/platforms');
      return response;
    },
    staleTime: 1000 * 60 * 60, // 1 hour - platforms rarely change
  });
}

/**
 * Hook for managing discovery filters state
 * Story: ACF-010
 */
export function useDiscoveryFilters() {
  const [filters, setFilters] = useState<DiscoveryFilters>({});

  const updateNiches = useCallback((niches: string[]) => {
    setFilters((prev) => ({ ...prev, niches: niches.length > 0 ? niches : undefined }));
  }, []);

  const updatePlatforms = useCallback((platforms: string[]) => {
    setFilters((prev) => ({ ...prev, platforms: platforms.length > 0 ? platforms : undefined }));
  }, []);

  const updateAudienceSize = useCallback((audienceSize: AudienceSizeRange | undefined) => {
    setFilters((prev) => ({ ...prev, audienceSize }));
  }, []);

  const updateSearch = useCallback((search: string) => {
    setFilters((prev) => ({
      ...prev,
      search: search.trim() || undefined,
    }));
  }, []);

  const updateOpenToCollab = useCallback((openToCollab: boolean | undefined) => {
    setFilters((prev) => ({ ...prev, openToCollab }));
  }, []);

  const clearFilters = useCallback(() => {
    setFilters({});
  }, []);

  const hasActiveFilters = useMemo(() => {
    return !!(
      filters.niches?.length ||
      filters.platforms?.length ||
      filters.audienceSize ||
      filters.search ||
      filters.openToCollab !== undefined
    );
  }, [filters]);

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.niches?.length) count += 1;
    if (filters.platforms?.length) count += 1;
    if (filters.audienceSize) count += 1;
    if (filters.search) count += 1;
    if (filters.openToCollab !== undefined) count += 1;
    return count;
  }, [filters]);

  return {
    filters,
    updateNiches,
    updatePlatforms,
    updateAudienceSize,
    updateSearch,
    updateOpenToCollab,
    clearFilters,
    hasActiveFilters,
    activeFilterCount,
  };
}

/**
 * Helper hook for formatting discovery data
 * Story: ACF-010
 */
export function useDiscoveryHelpers() {
  /**
   * Format follower count for display
   */
  const formatFollowerCount = useCallback((count: number): string => {
    if (count >= 1_000_000) {
      return `${(count / 1_000_000).toFixed(1)}M`;
    }
    if (count >= 1_000) {
      return `${(count / 1_000).toFixed(1)}K`;
    }
    return count.toString();
  }, []);

  /**
   * Get audience size label from follower count
   */
  const getAudienceSizeLabel = useCallback((totalFollowers: number): string => {
    if (totalFollowers >= 100_000) return '100K+';
    if (totalFollowers >= 10_000) return '10K-100K';
    if (totalFollowers >= 1_000) return '1K-10K';
    return '<1K';
  }, []);

  return {
    formatFollowerCount,
    getAudienceSizeLabel,
  };
}
