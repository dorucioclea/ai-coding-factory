/**
 * Discovery types for creator discovery feature
 * Story: ACF-010
 */

/**
 * Platform info in discovery results
 */
export interface DiscoveryPlatformDto {
  platformType: string;
  handle: string;
  followerCount?: number;
}

/**
 * Creator in discovery results
 */
export interface DiscoveryCreatorDto {
  id: string;
  username: string;
  displayName: string;
  bio: string;
  profilePictureUrl?: string;
  openToCollaborations: boolean;
  nicheTags: string[];
  platforms: DiscoveryPlatformDto[];
  totalFollowers: number;
}

/**
 * Discovery response with cursor-based pagination
 */
export interface DiscoveryResponse {
  items: DiscoveryCreatorDto[];
  totalCount: number;
  nextCursor?: string;
  hasMore: boolean;
  pageSize: number;
}

/**
 * Audience size range for filtering
 */
export type AudienceSizeRange = 'Small' | 'Medium' | 'Large';

/**
 * Discovery filter parameters
 */
export interface DiscoveryFilters {
  niches?: string[];
  platforms?: string[];
  audienceSize?: AudienceSizeRange;
  search?: string;
  openToCollab?: boolean;
}

/**
 * Discovery query parameters (for API request)
 */
export interface DiscoveryQueryParams extends DiscoveryFilters {
  cursor?: string;
  pageSize?: number;
}

/**
 * Audience size display labels
 */
export const AudienceSizeLabels: Record<AudienceSizeRange, string> = {
  Small: '1K - 10K',
  Medium: '10K - 100K',
  Large: '100K+',
};

/**
 * Platform display config
 */
export const PlatformConfig: Record<string, { label: string; color: string }> = {
  YouTube: { label: 'YouTube', color: '#FF0000' },
  TikTok: { label: 'TikTok', color: '#000000' },
  Instagram: { label: 'Instagram', color: '#E4405F' },
  Twitter: { label: 'Twitter', color: '#1DA1F2' },
  Twitch: { label: 'Twitch', color: '#9146FF' },
  LinkedIn: { label: 'LinkedIn', color: '#0A66C2' },
  Facebook: { label: 'Facebook', color: '#1877F2' },
  Website: { label: 'Website', color: '#6B7280' },
};
