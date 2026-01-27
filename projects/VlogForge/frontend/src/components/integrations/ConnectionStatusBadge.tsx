'use client';

import { Badge } from '@/components/ui';
import type { ConnectionStatus } from '@/types/integrations';

interface ConnectionStatusBadgeProps {
  status: ConnectionStatus;
}

/**
 * Visual status indicator for platform connections
 */
export function ConnectionStatusBadge({ status }: ConnectionStatusBadgeProps) {
  const variants: Record<ConnectionStatus, { variant: 'success' | 'secondary' | 'destructive'; label: string }> = {
    Connected: {
      variant: 'success',
      label: 'Connected',
    },
    Disconnected: {
      variant: 'secondary',
      label: 'Not Connected',
    },
    Error: {
      variant: 'destructive',
      label: 'Error',
    },
  };

  const config = variants[status];

  return <Badge variant={config.variant}>{config.label}</Badge>;
}

export default ConnectionStatusBadge;
