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
