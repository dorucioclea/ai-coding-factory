'use client';

import { Users, Eye, TrendingUp } from 'lucide-react';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Skeleton,
} from '@/components/ui';
import type { AnalyticsOverview } from '@/types';
import { GrowthIndicator } from './growth-indicator';

interface OverviewCardsProps {
  data?: AnalyticsOverview;
  isLoading: boolean;
}

/**
 * Overview cards showing total metrics across all platforms
 * Story: ACF-004 (AC1)
 */
export function OverviewCards({ data, isLoading }: OverviewCardsProps) {
  const formatNumber = (num: number): string => {
    if (num >= 1_000_000) {
      return `${(num / 1_000_000).toFixed(1)}M`;
    }
    if (num >= 1_000) {
      return `${(num / 1_000).toFixed(1)}K`;
    }
    return num.toLocaleString();
  };

  const cards = [
    {
      title: 'Total Followers',
      value: data?.totalFollowers ?? 0,
      growth: data?.growth?.followerGrowthPercent ?? 0,
      icon: Users,
      color: 'text-blue-500',
    },
    {
      title: 'Total Views',
      value: data?.totalViews ?? 0,
      growth: data?.growth?.viewsGrowthPercent ?? 0,
      icon: Eye,
      color: 'text-purple-500',
    },
    {
      title: 'Engagement Rate',
      value: data?.averageEngagementRate ?? 0,
      growth: data?.growth?.engagementGrowthPercent ?? 0,
      icon: TrendingUp,
      color: 'text-green-500',
      isPercentage: true,
    },
  ];

  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-3">
        {[1, 2, 3].map((i) => (
          <Card key={i}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-4" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-8 w-20" />
              <Skeleton className="mt-1 h-4 w-16" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  return (
    <div className="grid gap-4 md:grid-cols-3">
      {cards.map((card) => (
        <Card key={card.title}>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{card.title}</CardTitle>
            <card.icon className={`h-4 w-4 ${card.color}`} />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {card.isPercentage
                ? `${card.value.toFixed(2)}%`
                : formatNumber(card.value)}
            </div>
            <GrowthIndicator
              value={card.growth}
              comparisonDays={data?.growth?.comparisonDays ?? 7}
            />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
