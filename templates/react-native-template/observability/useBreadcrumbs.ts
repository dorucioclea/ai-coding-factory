import { useCallback } from 'react';
import * as Sentry from '@sentry/react-native';

export function useBreadcrumbs() {
  // Track user actions
  const trackAction = useCallback(
    (action: string, data?: Record<string, unknown>) => {
      Sentry.addBreadcrumb({
        category: 'user.action',
        message: action,
        level: 'info',
        data: {
          ...data,
          timestamp: new Date().toISOString(),
        },
      });
    },
    []
  );

  // Track state changes
  const trackStateChange = useCallback(
    (stateName: string, oldValue: unknown, newValue: unknown) => {
      Sentry.addBreadcrumb({
        category: 'state.change',
        message: `${stateName} changed`,
        level: 'info',
        data: {
          stateName,
          from: typeof oldValue === 'object' ? '[object]' : oldValue,
          to: typeof newValue === 'object' ? '[object]' : newValue,
        },
      });
    },
    []
  );

  // Track navigation
  const trackNavigation = useCallback((from: string, to: string) => {
    Sentry.addBreadcrumb({
      category: 'navigation',
      message: `Navigate from ${from} to ${to}`,
      level: 'info',
      data: { from, to },
    });
  }, []);

  // Track network requests (for custom tracking beyond automatic)
  const trackNetworkRequest = useCallback(
    (method: string, url: string, status: number, duration: number) => {
      Sentry.addBreadcrumb({
        category: 'http',
        message: `${method} ${url}`,
        level: status >= 400 ? 'warning' : 'info',
        data: { method, url, status, duration },
      });
    },
    []
  );

  // Track errors (non-fatal)
  const trackError = useCallback(
    (message: string, error?: Error, context?: Record<string, unknown>) => {
      Sentry.addBreadcrumb({
        category: 'error',
        message,
        level: 'error',
        data: {
          errorMessage: error?.message,
          ...context,
        },
      });
    },
    []
  );

  return {
    trackAction,
    trackStateChange,
    trackNavigation,
    trackNetworkRequest,
    trackError,
  };
}
