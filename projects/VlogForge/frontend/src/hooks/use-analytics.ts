'use client';

import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api-client';
import type {
  AnalyticsOverview,
  AnalyticsTrends,
  TopContentResponse,
  AnalyticsPeriod,
  ContentSortBy,
} from '@/types';

/**
 * Query keys for analytics data
 */
export const analyticsKeys = {
  all: ['analytics'] as const,
  overview: () => [...analyticsKeys.all, 'overview'] as const,
  trends: (period: AnalyticsPeriod) =>
    [...analyticsKeys.all, 'trends', period] as const,
  topContent: (sortBy: ContentSortBy, limit: number) =>
    [...analyticsKeys.all, 'top-content', sortBy, limit] as const,
};

/**
 * Hook for analytics overview data
 * Fetches total followers, views, engagement, platform breakdown, and growth indicators
 * Story: ACF-004 (AC1, AC3, AC5)
 */
export function useAnalyticsOverview() {
  return useQuery({
    queryKey: analyticsKeys.overview(),
    queryFn: () => apiClient.get<AnalyticsOverview>('/analytics/overview'),
    staleTime: 1000 * 60 * 5, // 5 minutes
    refetchInterval: 1000 * 60 * 15, // Refresh every 15 minutes
  });
}

/**
 * Hook for analytics trends data
 * Fetches trend data for charts over a specified time period
 * Story: ACF-004 (AC2)
 */
export function useAnalyticsTrends(period: AnalyticsPeriod = '7d') {
  return useQuery({
    queryKey: analyticsKeys.trends(period),
    queryFn: () =>
      apiClient.get<AnalyticsTrends>('/analytics/trends', {
        params: { period },
      }),
    staleTime: 1000 * 60 * 15, // 15 minutes (historical data changes less)
  });
}

/**
 * Hook for top performing content
 * Fetches top content sorted by views or engagement
 * Story: ACF-004 (AC4)
 */
export function useTopContent(sortBy: ContentSortBy = 'views', limit = 10) {
  return useQuery({
    queryKey: analyticsKeys.topContent(sortBy, limit),
    queryFn: () =>
      apiClient.get<TopContentResponse>('/analytics/top-content', {
        params: { sortBy, limit },
      }),
    staleTime: 1000 * 60 * 10, // 10 minutes
  });
}

/**
 * Combined analytics hook for dashboard
 * Returns overview, trends, and top content data
 */
export function useAnalyticsDashboard(
  period: AnalyticsPeriod = '7d',
  contentSortBy: ContentSortBy = 'views'
) {
  const overview = useAnalyticsOverview();
  const trends = useAnalyticsTrends(period);
  const topContent = useTopContent(contentSortBy);

  return {
    overview,
    trends,
    topContent,
    isLoading: overview.isLoading || trends.isLoading || topContent.isLoading,
    isError: overview.isError || trends.isError || topContent.isError,
  };
}
