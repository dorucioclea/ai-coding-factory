/**
 * Content Ideas types matching the backend DTOs
 * Corresponds to ContentIdeaDtos.cs in the backend
 */

/**
 * Content idea status enum matching backend
 */
export enum IdeaStatus {
  Idea = 0,
  Draft = 1,
  InReview = 2,
  Scheduled = 3,
  Published = 4,
}

/**
 * Status display properties for UI
 */
export interface StatusConfig {
  label: string;
  color: 'gray' | 'yellow' | 'orange' | 'blue' | 'green';
  description: string;
}

/**
 * Status configuration map
 */
export const STATUS_CONFIG: Record<IdeaStatus, StatusConfig> = {
  [IdeaStatus.Idea]: {
    label: 'Idea',
    color: 'gray',
    description: 'Initial concept or idea',
  },
  [IdeaStatus.Draft]: {
    label: 'Draft',
    color: 'yellow',
    description: 'Work in progress',
  },
  [IdeaStatus.InReview]: {
    label: 'In Review',
    color: 'orange',
    description: 'Under review',
  },
  [IdeaStatus.Scheduled]: {
    label: 'Scheduled',
    color: 'blue',
    description: 'Scheduled for publication',
  },
  [IdeaStatus.Published]: {
    label: 'Published',
    color: 'green',
    description: 'Published content',
  },
};

/**
 * Valid status transitions
 */
export const VALID_TRANSITIONS: Record<IdeaStatus, IdeaStatus[]> = {
  [IdeaStatus.Idea]: [IdeaStatus.Draft],
  [IdeaStatus.Draft]: [IdeaStatus.Idea, IdeaStatus.InReview],
  [IdeaStatus.InReview]: [IdeaStatus.Draft, IdeaStatus.Scheduled],
  [IdeaStatus.Scheduled]: [IdeaStatus.InReview, IdeaStatus.Published],
  [IdeaStatus.Published]: [],
};

/**
 * Content idea response DTO
 */
export interface ContentIdeaResponse {
  id: string;
  userId: string;
  title: string;
  notes?: string;
  status: IdeaStatus;
  platformTags: string[];
  scheduledDate?: string;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Content ideas list response
 */
export interface ContentIdeasListResponse {
  items: ContentIdeaResponse[];
  totalCount: number;
}

/**
 * Create content idea request
 */
export interface CreateContentIdeaRequest {
  title: string;
  notes?: string;
  platformTags: string[];
  scheduledDate?: string;
}

/**
 * Update content idea request
 */
export interface UpdateContentIdeaRequest {
  title?: string;
  notes?: string;
  platformTags?: string[];
  scheduledDate?: string;
}

/**
 * Update status request
 */
export interface UpdateStatusRequest {
  status: IdeaStatus;
}

/**
 * Content ideas filter parameters
 */
export interface ContentFilters {
  status?: IdeaStatus;
  platformTag?: string;
  search?: string;
  sortBy?: 'createdAt' | 'updatedAt' | 'scheduledDate' | 'title';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

/**
 * Platform tag options
 */
export const PLATFORM_TAGS = [
  'YouTube',
  'TikTok',
  'Instagram',
  'Twitter',
  'Facebook',
  'LinkedIn',
  'Twitch',
  'Other',
] as const;

export type PlatformTag = (typeof PLATFORM_TAGS)[number];

/**
 * Helper to check if status transition is valid
 */
export function isValidTransition(
  currentStatus: IdeaStatus,
  newStatus: IdeaStatus
): boolean {
  return VALID_TRANSITIONS[currentStatus]?.includes(newStatus) ?? false;
}

/**
 * Helper to get available transitions for a status
 */
export function getAvailableTransitions(status: IdeaStatus): IdeaStatus[] {
  return VALID_TRANSITIONS[status] ?? [];
}
