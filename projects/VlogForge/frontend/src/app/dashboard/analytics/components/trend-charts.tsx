'use client';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Skeleton,
} from '@/components/ui';
import type { AnalyticsTrends } from '@/types';

interface TrendChartsProps {
  data?: AnalyticsTrends;
  isLoading: boolean;
}

/**
 * Trend charts for followers, views, and engagement
 * Story: ACF-004 (AC2)
 *
 * Note: This is a placeholder implementation using CSS-based visualization.
 * For production, integrate Recharts for proper line charts.
 */
export function TrendCharts({ data, isLoading }: TrendChartsProps) {
  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-3">
        {[1, 2, 3].map((i) => (
          <Card key={i}>
            <CardHeader>
              <Skeleton className="h-5 w-32" />
              <Skeleton className="h-4 w-48" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-40 w-full" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  const renderMiniChart = (dataPoints: { value: number }[], color: string) => {
    if (!dataPoints || dataPoints.length === 0) {
      return (
        <div className="flex h-40 items-center justify-center text-muted-foreground">
          No data available
        </div>
      );
    }

    const maxValue = Math.max(...dataPoints.map((d) => d.value), 1);
    const chartWidth = 100;
    const chartHeight = 160;
    const points = dataPoints.map((point, index) => {
      const x = (index / (dataPoints.length - 1 || 1)) * chartWidth;
      const y = chartHeight - (point.value / maxValue) * chartHeight;
      return `${x},${y}`;
    });

    return (
      <svg
        viewBox={`0 0 ${chartWidth} ${chartHeight}`}
        className="h-40 w-full"
        preserveAspectRatio="none"
      >
        {/* Area fill */}
        <path
          d={`M0,${chartHeight} ${points.map((p) => `L${p}`).join(' ')} L${chartWidth},${chartHeight} Z`}
          fill={color}
          fillOpacity={0.1}
        />
        {/* Line */}
        <polyline
          points={points.join(' ')}
          fill="none"
          stroke={color}
          strokeWidth={2}
          vectorEffect="non-scaling-stroke"
        />
      </svg>
    );
  };

  const formatValue = (value: number, isEngagement = false): string => {
    if (isEngagement) {
      return `${(value / 100).toFixed(2)}%`;
    }
    if (value >= 1_000_000) {
      return `${(value / 1_000_000).toFixed(1)}M`;
    }
    if (value >= 1_000) {
      return `${(value / 1_000).toFixed(1)}K`;
    }
    return value.toLocaleString();
  };

  const getLatestValue = (
    dataPoints: { value: number }[],
    isEngagement = false
  ): string => {
    if (!dataPoints || dataPoints.length === 0) return '-';
    const latest = dataPoints[dataPoints.length - 1];
    if (!latest) return '-';
    return formatValue(latest.value, isEngagement);
  };

  return (
    <div className="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-base">Followers</CardTitle>
          <CardDescription>
            Current: {getLatestValue(data?.followerTrend ?? [])}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {renderMiniChart(data?.followerTrend ?? [], '#3B82F6')}
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-base">Views</CardTitle>
          <CardDescription>
            Total: {getLatestValue(data?.viewsTrend ?? [])}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {renderMiniChart(data?.viewsTrend ?? [], '#8B5CF6')}
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-base">Engagement</CardTitle>
          <CardDescription>
            Average: {getLatestValue(data?.engagementTrend ?? [], true)}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {renderMiniChart(data?.engagementTrend ?? [], '#22C55E')}
        </CardContent>
      </Card>
    </div>
  );
}
