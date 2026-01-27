/**
 * Analytics types matching the backend DTOs
 * Story: ACF-004
 */

/**
 * Analytics overview response
 */
export interface AnalyticsOverview {
  totalFollowers: number;
  totalViews: number;
  averageEngagementRate: number;
  platformBreakdown: PlatformMetricsSummary[];
  growth: GrowthIndicators;
}

/**
 * Summary of metrics for a single platform
 */
export interface PlatformMetricsSummary {
  platformType: string;
  followerCount: number;
  viewCount: number;
  engagementRate: number;
  lastSyncAt?: string;
}

/**
 * Growth indicators comparing periods
 */
export interface GrowthIndicators {
  followerGrowthPercent: number;
  viewsGrowthPercent: number;
  engagementGrowthPercent: number;
  comparisonDays: number;
}

/**
 * Analytics trends response
 */
export interface AnalyticsTrends {
  period: string;
  followerTrend: TrendDataPoint[];
  viewsTrend: TrendDataPoint[];
  engagementTrend: TrendDataPoint[];
}

/**
 * A single data point in a trend chart
 */
export interface TrendDataPoint {
  date: string;
  value: number;
  platformType?: string;
}

/**
 * Top content response
 */
export interface TopContentResponse {
  content: ContentPerformance[];
  sortedBy: string;
}

/**
 * Content performance data
 */
export interface ContentPerformance {
  contentId: string;
  platformType: string;
  title: string;
  thumbnailUrl?: string;
  contentUrl: string;
  publishedAt: string;
  viewCount: number;
  likeCount: number;
  commentCount: number;
  engagementRate: number;
}

/**
 * Time period options for analytics
 */
export type AnalyticsPeriod = '7d' | '30d' | '90d';

/**
 * Sort options for top content
 */
export type ContentSortBy = 'views' | 'engagement' | 'likes' | 'comments';

/**
 * Platform types for filtering
 */
export type PlatformType =
  | 'YouTube'
  | 'Instagram'
  | 'TikTok'
  | 'Twitter'
  | 'Twitch'
  | 'LinkedIn'
  | 'Facebook'
  | 'Website';

/**
 * Platform display info
 */
export interface PlatformInfo {
  type: PlatformType;
  displayName: string;
  icon: string;
  color: string;
}

/**
 * Platform configuration
 */
export const PLATFORMS: Record<PlatformType, PlatformInfo> = {
  YouTube: {
    type: 'YouTube',
    displayName: 'YouTube',
    icon: 'youtube',
    color: '#FF0000',
  },
  Instagram: {
    type: 'Instagram',
    displayName: 'Instagram',
    icon: 'instagram',
    color: '#E4405F',
  },
  TikTok: {
    type: 'TikTok',
    displayName: 'TikTok',
    icon: 'tiktok',
    color: '#000000',
  },
  Twitter: {
    type: 'Twitter',
    displayName: 'Twitter',
    icon: 'twitter',
    color: '#1DA1F2',
  },
  Twitch: {
    type: 'Twitch',
    displayName: 'Twitch',
    icon: 'twitch',
    color: '#9146FF',
  },
  LinkedIn: {
    type: 'LinkedIn',
    displayName: 'LinkedIn',
    icon: 'linkedin',
    color: '#0A66C2',
  },
  Facebook: {
    type: 'Facebook',
    displayName: 'Facebook',
    icon: 'facebook',
    color: '#1877F2',
  },
  Website: {
    type: 'Website',
    displayName: 'Website',
    icon: 'globe',
    color: '#6B7280',
  },
};
