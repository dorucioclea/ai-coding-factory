'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, XCircle } from 'lucide-react';

import { useOAuthCallback } from '@/hooks';
import { Button } from '@/components/ui';

/**
 * Client component for OAuth callback handling
 */
export default function OAuthCallbackContent() {
  const router = useRouter();
  const { isCompleting, error } = useOAuthCallback();

  useEffect(() => {
    // If no code or state params, redirect back to integrations
    const params = new URLSearchParams(window.location.search);
    if (!params.get('code') || !params.get('state')) {
      router.push('/dashboard/integrations');
    }
  }, [router]);

  if (error) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex flex-col items-center gap-6 text-center max-w-md">
          <XCircle className="h-16 w-16 text-destructive" />
          <div className="space-y-2">
            <h2 className="text-2xl font-bold">Connection Failed</h2>
            <p className="text-muted-foreground">
              {error instanceof Error
                ? error.message
                : 'Failed to complete platform connection'}
            </p>
          </div>
          <Button onClick={() => router.push('/dashboard/integrations')}>
            Back to Integrations
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-[400px] items-center justify-center">
      <div className="flex flex-col items-center gap-6 text-center">
        <Loader2 className="h-16 w-16 animate-spin text-primary" />
        <div className="space-y-2">
          <h2 className="text-2xl font-bold">
            {isCompleting ? 'Completing Connection...' : 'Redirecting...'}
          </h2>
          <p className="text-muted-foreground">
            Please wait while we finalize your platform connection.
          </p>
        </div>
      </div>
    </div>
  );
}
