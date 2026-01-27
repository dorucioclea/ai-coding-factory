'use client';

import { useState } from 'react';
import {
  useAnalyticsOverview,
  useAnalyticsTrends,
  useTopContent,
} from '@/hooks/use-analytics';
import type { AnalyticsPeriod, ContentSortBy } from '@/types';
import {
  OverviewCards,
  PlatformBreakdown,
  PeriodSelector,
  TrendCharts,
  TopContentTable,
} from './components';

/**
 * Analytics Dashboard Page
 * Story: ACF-004
 *
 * Displays:
 * - AC1: Overview metrics (followers, views, engagement)
 * - AC2: Trend visualization (7/30/90 day charts)
 * - AC3: Platform breakdown with comparison
 * - AC4: Top 10 performing content
 * - AC5: Growth metrics with percentage change
 */
export default function AnalyticsPage() {
  const [period, setPeriod] = useState<AnalyticsPeriod>('7d');
  const [contentSortBy, setContentSortBy] = useState<ContentSortBy>('views');

  const { data: overview, isLoading: overviewLoading } = useAnalyticsOverview();
  const { data: trends, isLoading: trendsLoading } = useAnalyticsTrends(period);
  const { data: topContent, isLoading: contentLoading } = useTopContent(
    contentSortBy,
    10
  );

  return (
    <div className="space-y-8">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Analytics Dashboard</h1>
        <p className="text-muted-foreground">
          Track your performance across all connected platforms
        </p>
      </div>

      {/* Overview Cards - AC1, AC5 */}
      <OverviewCards data={overview} isLoading={overviewLoading} />

      {/* Platform Breakdown - AC3 */}
      <PlatformBreakdown
        platforms={overview?.platformBreakdown}
        isLoading={overviewLoading}
      />

      {/* Trends Section - AC2 */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold">Performance Trends</h2>
          <PeriodSelector value={period} onChange={setPeriod} />
        </div>
        <TrendCharts data={trends} isLoading={trendsLoading} />
      </div>

      {/* Top Content - AC4 */}
      <TopContentTable
        content={topContent?.content}
        sortBy={contentSortBy}
        onSortChange={setContentSortBy}
        isLoading={contentLoading}
      />
    </div>
  );
}
