import { QueryClient } from '@tanstack/react-query';
import { ApiError } from '@/types';

/**
 * Default query client options
 * Optimized for enterprise applications
 */
function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Stale time: how long data is considered fresh
        staleTime: 1000 * 60 * 5, // 5 minutes

        // Cache time: how long inactive data stays in cache
        gcTime: 1000 * 60 * 30, // 30 minutes

        // Retry logic
        retry: (failureCount, error) => {
          // Don't retry on auth errors
          if (error instanceof ApiError) {
            if (error.isUnauthorized() || error.isForbidden()) {
              return false;
            }
            // Don't retry on validation errors
            if (error.isValidationError()) {
              return false;
            }
          }
          // Retry up to 3 times for other errors
          return failureCount < 3;
        },

        // Retry delay with exponential backoff
        retryDelay: (attemptIndex) =>
          Math.min(1000 * 2 ** attemptIndex, 30000),

        // Refetch settings
        refetchOnWindowFocus: false,
        refetchOnReconnect: true,
        refetchOnMount: true,
      },
      mutations: {
        // Don't retry mutations by default
        retry: false,

        // Global error handler for mutations
        onError: (error) => {
          if (error instanceof ApiError) {
            // Could dispatch to global error handler
            if (process.env['NODE_ENV'] === 'development') {
              // eslint-disable-next-line no-console
              console.error('Mutation error:', error);
            }
          }
        },
      },
    },
  });
}

// Singleton query client for SSR consistency
let browserQueryClient: QueryClient | undefined;

export function getQueryClient(): QueryClient {
  // Server: always create new client
  if (typeof window === 'undefined') {
    return createQueryClient();
  }

  // Browser: reuse client
  if (!browserQueryClient) {
    browserQueryClient = createQueryClient();
  }

  return browserQueryClient;
}

/**
 * Query key factory for type-safe query keys
 */
export const queryKeys = {
  // Auth queries
  auth: {
    all: ['auth'] as const,
    user: () => [...queryKeys.auth.all, 'user'] as const,
    session: () => [...queryKeys.auth.all, 'session'] as const,
  },

  // User queries
  users: {
    all: ['users'] as const,
    lists: () => [...queryKeys.users.all, 'list'] as const,
    list: (params: Record<string, unknown>) =>
      [...queryKeys.users.lists(), params] as const,
    details: () => [...queryKeys.users.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.users.details(), id] as const,
  },

  // Analytics queries (ACF-004)
  analytics: {
    all: ['analytics'] as const,
    overview: () => ['analytics', 'overview'] as const,
    trends: (period: string) => ['analytics', 'trends', period] as const,
    topContent: (sortBy: string, limit: number) =>
      ['analytics', 'top-content', sortBy, limit] as const,
  },

  // Content ideas queries (ACF-015)
  content: {
    all: ['content'] as const,
    lists: () => [...queryKeys.content.all, 'list'] as const,
    list: (filters: Record<string, unknown>) =>
      [...queryKeys.content.lists(), filters] as const,
    details: () => [...queryKeys.content.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.content.details(), id] as const,
  },

  // Profile queries (ACF-015)
  profiles: {
    all: ['profiles'] as const,
    my: () => [...queryKeys.profiles.all, 'my'] as const,
    public: (username: string) =>
      [...queryKeys.profiles.all, 'public', username] as const,
  },

  // Integration queries (ACF-015 Phase 2)
  integrations: {
    all: ['integrations'] as const,
    status: () => [...queryKeys.integrations.all, 'status'] as const,
    connection: (platform: string) =>
      [...queryKeys.integrations.all, 'connection', platform] as const,
  },

  // Task queries (ACF-015 Phase 6)
  tasks: {
    all: ['tasks'] as const,
    lists: () => [...queryKeys.tasks.all, 'list'] as const,
    list: (filters?: Record<string, unknown>) =>
      [...queryKeys.tasks.lists(), filters] as const,
    details: () => [...queryKeys.tasks.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.tasks.details(), id] as const,
    myTasks: (filters?: Record<string, unknown>) =>
      [...queryKeys.tasks.all, 'my-tasks', filters] as const,
    comments: (taskId: string) =>
      [...queryKeys.tasks.detail(taskId), 'comments'] as const,
  },

  // Calendar queries (ACF-015 Phase 4)
  calendar: {
    all: ['calendar'] as const,
    months: () => [...queryKeys.calendar.all, 'month'] as const,
    month: (year: number, month: number) =>
      [...queryKeys.calendar.months(), year, month] as const,
  },

  // Team queries (ACF-015 Phase 5)
  teams: {
    all: ['teams'] as const,
    lists: () => [...queryKeys.teams.all, 'list'] as const,
    details: () => [...queryKeys.teams.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.teams.details(), id] as const,
    members: (id: string) => [...queryKeys.teams.detail(id), 'members'] as const,
    invitations: (id: string) => [...queryKeys.teams.detail(id), 'invitations'] as const,
  },

  // Approval queries (ACF-009)
  approvals: {
    all: ['approvals'] as const,
    pending: (teamId: string) => [...queryKeys.approvals.all, 'pending', teamId] as const,
    history: (contentId: string) => [...queryKeys.approvals.all, 'history', contentId] as const,
  },

  // Collaboration queries (ACF-011)
  collaborations: {
    all: ['collaborations'] as const,
    inbox: (filters?: Record<string, unknown>) =>
      [...queryKeys.collaborations.all, 'inbox', filters] as const,
    sent: (filters?: Record<string, unknown>) =>
      [...queryKeys.collaborations.all, 'sent', filters] as const,
    detail: (id: string) =>
      [...queryKeys.collaborations.all, 'detail', id] as const,
  },

  // Messaging queries (ACF-012)
  messaging: {
    all: ['messaging'] as const,
    conversations: (filters?: Record<string, unknown>) =>
      [...queryKeys.messaging.all, 'conversations', filters] as const,
    messages: (conversationId: string, filters?: Record<string, unknown>) =>
      [...queryKeys.messaging.all, 'messages', conversationId, filters] as const,
    unreadCount: () =>
      [...queryKeys.messaging.all, 'unread-count'] as const,
  },

  // Discovery queries (ACF-010)
  discovery: {
    all: ['discovery'] as const,
    creators: (filters?: Record<string, unknown>) =>
      [...queryKeys.discovery.all, 'creators', filters] as const,
    niches: () => [...queryKeys.discovery.all, 'niches'] as const,
    platforms: () => [...queryKeys.discovery.all, 'platforms'] as const,
  },

  // Generic entity factory
  entity: (name: string) => ({
    all: [name] as const,
    lists: () => [name, 'list'] as const,
    list: (params: Record<string, unknown>) => [name, 'list', params] as const,
    details: () => [name, 'detail'] as const,
    detail: (id: string) => [name, 'detail', id] as const,
  }),
} as const;

export default getQueryClient;
