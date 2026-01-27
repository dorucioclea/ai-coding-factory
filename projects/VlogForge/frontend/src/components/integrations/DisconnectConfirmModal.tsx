'use client';

import { useState } from 'react';
import { Loader2 } from 'lucide-react';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Button,
} from '@/components/ui';
import { useDisconnectPlatform } from '@/hooks';
import type { PlatformType } from '@/types/integrations';

interface DisconnectConfirmModalProps {
  platform: PlatformType;
  platformName: string;
  accountName?: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * Confirmation dialog for disconnecting a platform
 */
export function DisconnectConfirmModal({
  platform,
  platformName,
  accountName,
  open,
  onOpenChange,
}: DisconnectConfirmModalProps) {
  const [error, setError] = useState<string | null>(null);
  const { mutate: disconnect, isPending } = useDisconnectPlatform();

  const handleDisconnect = () => {
    setError(null);
    disconnect(platform, {
      onSuccess: () => {
        onOpenChange(false);
      },
      onError: (err) => {
        setError(err instanceof Error ? err.message : 'Failed to disconnect');
      },
    });
  };

  const handleCancel = () => {
    if (!isPending) {
      setError(null);
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Disconnect {platformName}?</DialogTitle>
          <DialogDescription>
            {accountName ? (
              <>
                This will disconnect your account <strong>{accountName}</strong>{' '}
                from VlogForge.
              </>
            ) : (
              <>This will disconnect {platformName} from VlogForge.</>
            )}
            <br />
            <br />
            You will no longer be able to publish content to this platform until
            you reconnect.
          </DialogDescription>
        </DialogHeader>

        {error && (
          <div className="rounded-md bg-destructive/15 p-3 text-sm text-destructive">
            {error}
          </div>
        )}

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isPending}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleDisconnect}
            disabled={isPending}
          >
            {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isPending ? 'Disconnecting...' : 'Disconnect'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default DisconnectConfirmModal;
