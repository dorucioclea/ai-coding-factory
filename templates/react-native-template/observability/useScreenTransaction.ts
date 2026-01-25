import { useEffect, useRef, useCallback } from 'react';
import * as Sentry from '@sentry/react-native';

export function useScreenTransaction(screenName: string) {
  const startTime = useRef(performance.now());
  const spanRef = useRef<Sentry.Span | null>(null);

  useEffect(() => {
    // Start a span for screen load using new API
    Sentry.startSpan(
      {
        name: screenName,
        op: 'ui.load',
      },
      (span) => {
        spanRef.current = span;
      }
    );

    // Cleanup on unmount
    return () => {
      spanRef.current?.end();
    };
  }, [screenName]);

  // Start a child span for specific operations
  const startSpan = useCallback(
    (name: string, op: string) => {
      return Sentry.startInactiveSpan({
        name,
        op,
      });
    },
    []
  );

  // Mark when screen becomes interactive
  const markInteractive = useCallback(() => {
    const tti = performance.now() - startTime.current;

    Sentry.setMeasurement('time_to_interactive', tti, 'millisecond');

    Sentry.addBreadcrumb({
      category: 'ui.lifecycle',
      message: `${screenName} became interactive`,
      level: 'info',
      data: { tti: Math.round(tti) },
    });
  }, [screenName]);

  // Add custom measurement
  const addMeasurement = useCallback(
    (name: string, value: number, unit: 'millisecond' | 'second' | 'byte' | 'percent' | 'none' = 'millisecond') => {
      Sentry.setMeasurement(name, value, unit);
    },
    []
  );

  return {
    startSpan,
    markInteractive,
    addMeasurement,
  };
}
