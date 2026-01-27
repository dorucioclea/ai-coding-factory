/**
 * Integration types for platform connections
 * Matches backend IntegrationDtos.cs
 */

/**
 * Supported platform types
 */
export type PlatformType = 'YouTube' | 'Instagram' | 'TikTok';

/**
 * Connection status
 */
export type ConnectionStatus = 'Connected' | 'Disconnected' | 'Error';

/**
 * Platform connection DTO
 */
export interface PlatformConnectionDto {
  id: string;
  platformType: PlatformType;
  status: ConnectionStatus;
  platformAccountId: string;
  platformAccountName: string;
  lastSyncAt?: string;
  errorMessage?: string;
  createdAt: string;
}

/**
 * Connection status response
 */
export interface ConnectionStatusResponse {
  connections: PlatformConnectionDto[];
  availablePlatforms: string[];
}

/**
 * OAuth initiation response
 */
export interface OAuthInitiationResponse {
  authorizationUrl: string;
  state: string;
}

/**
 * OAuth callback parameters
 */
export interface OAuthCallbackParams {
  code: string;
  state: string;
  platform: PlatformType;
}

/**
 * Disconnect platform request
 */
export interface DisconnectPlatformRequest {
  platform: PlatformType;
}

/**
 * Platform metadata for UI
 */
export interface PlatformMetadata {
  type: PlatformType;
  name: string;
  description: string;
  iconColor: string;
  features: string[];
}

/**
 * Platform metadata constants
 */
export const PLATFORM_METADATA: Record<PlatformType, PlatformMetadata> = {
  YouTube: {
    type: 'YouTube',
    name: 'YouTube',
    description: 'Upload and manage videos on YouTube',
    iconColor: 'text-red-600',
    features: [
      'Auto-upload videos',
      'Manage descriptions & tags',
      'Track video performance',
      'Schedule uploads',
    ],
  },
  Instagram: {
    type: 'Instagram',
    name: 'Instagram',
    description: 'Share content on Instagram',
    iconColor: 'text-pink-600',
    features: [
      'Post to feed & stories',
      'Auto-hashtag generation',
      'Engagement tracking',
      'Schedule posts',
    ],
  },
  TikTok: {
    type: 'TikTok',
    name: 'TikTok',
    description: 'Publish videos on TikTok',
    iconColor: 'text-black',
    features: [
      'Direct video uploads',
      'Trending hashtags',
      'Analytics dashboard',
      'Schedule TikToks',
    ],
  },
} as const;
