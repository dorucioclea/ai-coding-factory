import React, { Component, ReactNode } from 'react';
import { View, Text, Button, StyleSheet } from 'react-native';
import * as Sentry from '@sentry/react-native';
import { ErrorBoundary as SentryErrorBoundary } from '@sentry/react-native';

// Root Error Boundary - wraps entire app
export function RootErrorBoundary({ children }: { children: ReactNode }) {
  return (
    <SentryErrorBoundary
      fallback={({ error, resetError }) => (
        <CrashScreen error={error as Error} onRetry={resetError} />
      )}
      onError={(_error, componentStack) => {
        Sentry.withScope((scope) => {
          scope.setTag('error.boundary', 'root');
          scope.setTag('error.fatal', 'true');
          scope.setContext('react', { componentStack: componentStack ?? '' });
        });
      }}
    >
      {children}
    </SentryErrorBoundary>
  );
}

// Screen-level Error Boundary
interface ScreenErrorBoundaryProps {
  screenName: string;
  children: ReactNode;
  fallback?: ReactNode;
}

interface ScreenErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ScreenErrorBoundary extends Component<
  ScreenErrorBoundaryProps,
  ScreenErrorBoundaryState
> {
  state: ScreenErrorBoundaryState = {
    hasError: false,
    error: null,
  };

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    Sentry.withScope((scope) => {
      scope.setTag('error.boundary', 'screen');
      scope.setTag('screen.name', this.props.screenName);
      scope.setContext('react', { componentStack: errorInfo.componentStack });
      Sentry.captureException(error);
    });
  }

  handleRetry = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback ?? (
          <ScreenErrorFallback
            error={this.state.error}
            onRetry={this.handleRetry}
          />
        )
      );
    }
    return this.props.children;
  }
}

// Crash Screen - shown for fatal errors
function CrashScreen({
  error,
  onRetry,
}: {
  error: Error;
  onRetry: () => void;
}) {
  return (
    <View style={styles.container}>
      <View style={styles.content}>
        <Text style={styles.title}>Something went wrong</Text>
        <Text style={styles.message}>
          We've been notified and are working on a fix.
        </Text>
        {__DEV__ && (
          <Text style={styles.errorText}>{error.message}</Text>
        )}
        <View style={styles.buttonContainer}>
          <Button title="Try Again" onPress={onRetry} />
        </View>
      </View>
    </View>
  );
}

// Screen Error Fallback
function ScreenErrorFallback({
  error,
  onRetry,
}: {
  error: Error | null;
  onRetry: () => void;
}) {
  return (
    <View style={styles.screenErrorContainer}>
      <Text style={styles.screenErrorTitle}>Unable to load</Text>
      <Text style={styles.screenErrorMessage}>
        There was a problem loading this screen.
      </Text>
      {__DEV__ && error && (
        <Text style={styles.errorText}>{error.message}</Text>
      )}
      <View style={styles.buttonContainer}>
        <Button title="Retry" onPress={onRetry} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#FFFFFF',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  content: {
    alignItems: 'center',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#171717',
    marginBottom: 12,
    textAlign: 'center',
  },
  message: {
    fontSize: 16,
    color: '#525252',
    textAlign: 'center',
    marginBottom: 24,
  },
  errorText: {
    fontSize: 12,
    color: '#EF4444',
    textAlign: 'center',
    marginBottom: 24,
    padding: 12,
    backgroundColor: '#FEF2F2',
    borderRadius: 8,
    overflow: 'hidden',
  },
  buttonContainer: {
    marginTop: 16,
  },
  screenErrorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  screenErrorTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#171717',
    marginBottom: 8,
  },
  screenErrorMessage: {
    fontSize: 14,
    color: '#525252',
    textAlign: 'center',
    marginBottom: 16,
  },
});
