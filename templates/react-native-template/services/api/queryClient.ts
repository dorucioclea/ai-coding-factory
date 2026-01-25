import { QueryClient } from '@tanstack/react-query';
import * as Sentry from '@sentry/react-native';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
      retry: 2,
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      refetchOnWindowFocus: false, // Not applicable on mobile
      refetchOnReconnect: true,
    },
    mutations: {
      retry: 1,
      onError: (error) => {
        // Log mutation errors to Sentry
        Sentry.captureException(error, {
          tags: {
            type: 'mutation_error',
          },
        });
      },
    },
  },
});

// Optional: Add global error handler
queryClient.setMutationDefaults(['*'], {
  onError: (error) => {
    console.error('Mutation error:', error);
  },
});
