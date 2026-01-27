'use client';

import { useSearchParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { CheckCircle2, Loader2, AlertCircle } from 'lucide-react';

import { PlatformCard } from '@/components/integrations';
import { useIntegrations } from '@/hooks';
import { PLATFORM_METADATA } from '@/types/integrations';

/**
 * Client component for integrations content
 */
export default function IntegrationsContent() {
  const searchParams = useSearchParams();
  const [showConnectedAlert, setShowConnectedAlert] = useState(false);

  const {
    connections,
    isLoading,
    error,
    getConnection,
  } = useIntegrations();

  // Show success message if redirected after connection
  useEffect(() => {
    if (searchParams.get('connected') === 'true') {
      setShowConnectedAlert(true);
      const timer = setTimeout(() => setShowConnectedAlert(false), 5000);
      return () => clearTimeout(timer);
    }
  }, [searchParams]);

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">
            Loading integrations...
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex flex-col items-center gap-4 text-center">
          <AlertCircle className="h-12 w-12 text-destructive" />
          <div>
            <h3 className="text-lg font-semibold">Failed to load integrations</h3>
            <p className="text-sm text-muted-foreground">
              {error instanceof Error ? error.message : 'An error occurred'}
            </p>
          </div>
        </div>
      </div>
    );
  }

  const connectedCount = connections.filter(
    (conn) => conn.status === 'Connected'
  ).length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Platform Connections
        </h1>
        <p className="mt-2 text-muted-foreground">
          Connect your social media accounts to publish and manage content across
          multiple platforms from one place.
        </p>
      </div>

      {/* Success alert */}
      {showConnectedAlert && (
        <div className="rounded-lg bg-green-50 border border-green-200 p-4 dark:bg-green-900/20 dark:border-green-800">
          <div className="flex items-center gap-3">
            <CheckCircle2 className="h-5 w-5 text-green-600 dark:text-green-400" />
            <div>
              <p className="font-medium text-green-900 dark:text-green-100">
                Platform connected successfully!
              </p>
              <p className="text-sm text-green-700 dark:text-green-300">
                You can now publish content to this platform.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="rounded-lg border bg-card p-4">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-muted-foreground">Connected Platforms</p>
            <p className="text-2xl font-bold">
              {connectedCount} / {Object.keys(PLATFORM_METADATA).length}
            </p>
          </div>
          {connectedCount === Object.keys(PLATFORM_METADATA).length && (
            <CheckCircle2 className="h-8 w-8 text-green-600" />
          )}
        </div>
      </div>

      {/* Platform cards grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {Object.values(PLATFORM_METADATA).map((metadata) => {
          const connection = getConnection(metadata.type);
          return (
            <PlatformCard
              key={metadata.type}
              metadata={metadata}
              connection={connection}
            />
          );
        })}
      </div>

      {/* Info section */}
      <div className="rounded-lg border bg-muted/50 p-6 space-y-4">
        <h2 className="text-lg font-semibold">About Platform Connections</h2>
        <div className="space-y-2 text-sm text-muted-foreground">
          <p>
            Connecting your social media accounts allows VlogForge to publish
            content on your behalf. We use secure OAuth authentication to ensure
            your credentials are never stored on our servers.
          </p>
          <p>
            You can disconnect a platform at any time. Note that disconnecting
            will prevent scheduled content from being published to that platform.
          </p>
          <p className="font-medium text-foreground">
            Your data is encrypted and secure. We only request the minimum
            permissions needed to publish content.
          </p>
        </div>
      </div>
    </div>
  );
}
