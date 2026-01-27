/**
 * Profile types matching the backend ProfileDtos.cs
 * Used for creator profiles and collaboration features
 */

/**
 * Connected platform DTO for public display
 */
export interface PublicConnectedPlatformDto {
  platform: string;
  isConnected: boolean;
}

/**
 * Connected platform DTO with full details
 */
export interface ConnectedPlatformDto extends PublicConnectedPlatformDto {
  platformUsername?: string;
  followerCount?: number;
  lastSyncedAt?: string;
}

/**
 * Public profile response (visible to all users)
 */
export interface PublicProfileResponse {
  username: string;
  displayName: string;
  bio: string;
  profilePictureUrl?: string;
  openToCollaborations: boolean;
  nicheTags: string[];
  connectedPlatforms: PublicConnectedPlatformDto[];
}

/**
 * Full creator profile response (owner's view)
 */
export interface CreatorProfileResponse {
  id: string;
  userId: string;
  username: string;
  displayName: string;
  bio: string;
  profilePictureUrl?: string;
  openToCollaborations: boolean;
  collaborationPreferences?: string;
  nicheTags: string[];
  connectedPlatforms: ConnectedPlatformDto[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * Request to update profile
 */
export interface UpdateProfileRequest {
  displayName?: string;
  bio?: string;
  openToCollaborations?: boolean;
  collaborationPreferences?: string;
  nicheTags?: string[];
}

/**
 * Avatar upload response
 */
export interface AvatarUploadResponse {
  profilePictureUrl: string;
  uploadedAt: string;
}

/**
 * Platform types supported by VlogForge
 */
export const SupportedPlatforms = {
  YouTube: 'YouTube',
  TikTok: 'TikTok',
  Instagram: 'Instagram',
  Twitter: 'Twitter',
  Facebook: 'Facebook',
} as const;

export type SupportedPlatform =
  (typeof SupportedPlatforms)[keyof typeof SupportedPlatforms];

/**
 * Niche tag constants (common categories)
 */
export const CommonNicheTags = [
  'Tech',
  'Gaming',
  'Lifestyle',
  'Beauty',
  'Fitness',
  'Travel',
  'Food',
  'Music',
  'Education',
  'Comedy',
  'Fashion',
  'Sports',
  'Business',
  'DIY',
  'Art',
  'Science',
  'Health',
  'Finance',
  'Vlog',
  'Review',
] as const;

export type CommonNicheTag = (typeof CommonNicheTags)[number];

/**
 * Validation constraints
 */
export const ProfileConstraints = {
  maxNicheTags: 5,
  maxBioLength: 500,
  maxDisplayNameLength: 100,
  maxUsernameLength: 50,
  maxCollaborationPreferencesLength: 1000,
} as const;
