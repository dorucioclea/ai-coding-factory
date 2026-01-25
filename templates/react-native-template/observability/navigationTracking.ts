import { useEffect } from 'react';
import { usePathname, useSegments } from 'expo-router';
import * as Sentry from '@sentry/react-native';

export function useNavigationTracking() {
  const pathname = usePathname();
  const segments = useSegments();

  useEffect(() => {
    const routeName = segments.join('/') || 'index';

    // Add navigation breadcrumb
    Sentry.addBreadcrumb({
      category: 'navigation',
      message: `Navigated to ${routeName}`,
      level: 'info',
      data: {
        pathname,
        segments,
        timestamp: new Date().toISOString(),
      },
    });

    // Set current route as tag for better filtering
    Sentry.setTag('route', routeName);
  }, [pathname, segments]);
}
