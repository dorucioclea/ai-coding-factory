'use client';

import { useState } from 'react';
import { Loader2 } from 'lucide-react';

import { Button } from '@/components/ui';
import { useInitiateOAuth } from '@/hooks';
import type { PlatformType } from '@/types/integrations';

interface OAuthConnectButtonProps {
  platform: PlatformType;
  disabled?: boolean;
}

/**
 * Button that initiates OAuth flow for platform connection
 */
export function OAuthConnectButton({
  platform,
  disabled,
}: OAuthConnectButtonProps) {
  const [isRedirecting, setIsRedirecting] = useState(false);
  const { mutate: connect, isPending, error } = useInitiateOAuth();

  const handleConnect = () => {
    setIsRedirecting(true);
    connect(platform, {
      onError: () => {
        setIsRedirecting(false);
      },
    });
  };

  const isLoading = isPending || isRedirecting;

  return (
    <div className="space-y-2">
      <Button
        onClick={handleConnect}
        disabled={disabled || isLoading}
        className="w-full"
      >
        {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
        {isLoading ? 'Connecting...' : 'Connect'}
      </Button>
      {error && (
        <p className="text-sm text-destructive">
          {error instanceof Error ? error.message : 'Failed to connect'}
        </p>
      )}
    </div>
  );
}

export default OAuthConnectButton;
