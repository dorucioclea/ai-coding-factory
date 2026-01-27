'use client';

import { useState } from 'react';
import { Check, RefreshCw, AlertCircle } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
  Button,
} from '@/components/ui';
import { ConnectionStatusBadge } from './ConnectionStatusBadge';
import { OAuthConnectButton } from './OAuthConnectButton';
import { DisconnectConfirmModal } from './DisconnectConfirmModal';
import { useSyncPlatform } from '@/hooks';
import type { PlatformConnectionDto, PlatformMetadata } from '@/types';

interface PlatformCardProps {
  metadata: PlatformMetadata;
  connection?: PlatformConnectionDto;
}

/**
 * Individual platform card with connect/disconnect functionality
 */
export function PlatformCard({ metadata, connection }: PlatformCardProps) {
  const [disconnectModalOpen, setDisconnectModalOpen] = useState(false);
  const { mutate: sync, isPending: isSyncing } = useSyncPlatform();

  const isConnected = connection?.status === 'Connected';
  const hasError = connection?.status === 'Error';

  const handleSync = () => {
    if (connection) {
      sync(metadata.type);
    }
  };

  return (
    <>
      <Card className="relative overflow-hidden">
        {/* Status badge */}
        <div className="absolute right-4 top-4">
          {connection && <ConnectionStatusBadge status={connection.status} />}
        </div>

        <CardHeader>
          <CardTitle className="flex items-center gap-3">
            <span className={`text-2xl ${metadata.iconColor}`}>
              {getPlatformIcon(metadata.type)}
            </span>
            {metadata.name}
          </CardTitle>
          <CardDescription>{metadata.description}</CardDescription>
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Connection info */}
          {isConnected && connection && (
            <div className="rounded-lg bg-muted p-3 space-y-2">
              <div className="flex items-center gap-2 text-sm">
                <Check className="h-4 w-4 text-green-600" />
                <span className="font-medium">
                  Connected as {connection.platformAccountName}
                </span>
              </div>
              {connection.lastSyncAt && (
                <p className="text-xs text-muted-foreground">
                  Last synced{' '}
                  {formatDistanceToNow(new Date(connection.lastSyncAt), {
                    addSuffix: true,
                  })}
                </p>
              )}
            </div>
          )}

          {/* Error message */}
          {hasError && connection?.errorMessage && (
            <div className="rounded-lg bg-destructive/15 p-3 space-y-2">
              <div className="flex items-center gap-2 text-sm text-destructive">
                <AlertCircle className="h-4 w-4" />
                <span className="font-medium">Connection Error</span>
              </div>
              <p className="text-xs text-destructive/80">
                {connection.errorMessage}
              </p>
            </div>
          )}

          {/* Features list */}
          <div className="space-y-2">
            <p className="text-sm font-medium">Features:</p>
            <ul className="space-y-1">
              {metadata.features.map((feature) => (
                <li
                  key={feature}
                  className="flex items-center gap-2 text-sm text-muted-foreground"
                >
                  <Check className="h-3 w-3 text-primary" />
                  {feature}
                </li>
              ))}
            </ul>
          </div>
        </CardContent>

        <CardFooter className="flex gap-2">
          {isConnected ? (
            <>
              <Button
                variant="outline"
                onClick={handleSync}
                disabled={isSyncing}
                className="flex-1"
              >
                {isSyncing ? (
                  <>
                    <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                    Syncing...
                  </>
                ) : (
                  <>
                    <RefreshCw className="mr-2 h-4 w-4" />
                    Sync
                  </>
                )}
              </Button>
              <Button
                variant="destructive"
                onClick={() => setDisconnectModalOpen(true)}
                className="flex-1"
              >
                Disconnect
              </Button>
            </>
          ) : (
            <OAuthConnectButton platform={metadata.type} />
          )}
        </CardFooter>
      </Card>

      {/* Disconnect confirmation modal */}
      {connection && (
        <DisconnectConfirmModal
          platform={metadata.type}
          platformName={metadata.name}
          accountName={connection.platformAccountName}
          open={disconnectModalOpen}
          onOpenChange={setDisconnectModalOpen}
        />
      )}
    </>
  );
}

/**
 * Get platform icon emoji
 */
function getPlatformIcon(platform: string): string {
  const icons: Record<string, string> = {
    YouTube: '📺',
    Instagram: '📷',
    TikTok: '🎵',
  };
  return icons[platform] ?? '🔗';
}

export default PlatformCard;
