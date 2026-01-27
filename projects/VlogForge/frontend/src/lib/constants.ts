/**
 * Application-wide constants
 * Story: ACF-009
 */

/**
 * Query stale times for TanStack Query
 * These control how long data is considered fresh before refetching
 */
export const QUERY_STALE_TIMES = {
  /** Default stale time for most queries */
  DEFAULT: 1000 * 60, // 1 minute

  /** Stale time for approval-related queries (higher due to less frequent changes) */
  APPROVALS: 1000 * 60 * 2, // 2 minutes

  /** Stale time for content queries */
  CONTENT: 1000 * 60, // 1 minute

  /** Stale time for team data */
  TEAM: 1000 * 60 * 5, // 5 minutes

  /** Stale time for user profile data */
  PROFILE: 1000 * 60 * 5, // 5 minutes

  /** Stale time for frequently changing data like notifications */
  REAL_TIME: 1000 * 30, // 30 seconds
} as const;

/**
 * Query keys for cache invalidation
 */
export const QUERY_KEYS = {
  APPROVALS: {
    pending: (teamId: string) => ['approvals', 'pending', teamId] as const,
    history: (contentId: string) => ['approvals', 'history', contentId] as const,
  },
  CONTENT: {
    all: ['content'] as const,
    list: () => ['content', 'list'] as const,
    detail: (id: string) => ['content', 'detail', id] as const,
  },
  TEAM: {
    all: ['team'] as const,
    detail: (id: string) => ['team', 'detail', id] as const,
    members: (id: string) => ['team', 'members', id] as const,
  },
} as const;

/**
 * API response status codes
 */
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  UNPROCESSABLE_ENTITY: 422,
  INTERNAL_SERVER_ERROR: 500,
} as const;

/**
 * Maximum lengths for form fields (should match backend validation)
 */
export const MAX_LENGTHS = {
  FEEDBACK: 2000,
  TITLE: 200,
  NOTES: 5000,
} as const;
