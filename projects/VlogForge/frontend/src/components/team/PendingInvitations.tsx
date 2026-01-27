'use client';

import { Mail, Clock } from 'lucide-react';
import { Card, CardContent } from '@/components/ui';
import { RoleBadge } from './RoleBadge';
import type { TeamInvitationResponse } from '@/types';

interface PendingInvitationsProps {
  invitations: TeamInvitationResponse[];
}

/**
 * List of pending team invitations
 * ACF-015 Phase 5
 */
export function PendingInvitations({ invitations }: PendingInvitationsProps) {
  if (invitations.length === 0) {
    return null;
  }

  // Filter only pending invitations
  const pending = invitations.filter((inv) => !inv.isAccepted && !inv.isExpired);

  if (pending.length === 0) {
    return null;
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">Pending Invitations</h3>
      <div className="space-y-2">
        {pending.map((invitation) => {
          const expiresDate = new Date(invitation.expiresAt);
          const now = new Date();
          const daysUntilExpiry = Math.ceil(
            (expiresDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24)
          );

          return (
            <Card key={invitation.id}>
              <CardContent className="flex items-center justify-between p-4">
                <div className="flex items-center gap-3 flex-1 min-w-0">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-muted">
                    <Mail className="h-5 w-5 text-muted-foreground" />
                  </div>

                  <div className="flex-1 min-w-0">
                    <p className="font-medium truncate">{invitation.email}</p>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Clock className="h-3 w-3" />
                      <span>
                        Expires in {daysUntilExpiry}{' '}
                        {daysUntilExpiry === 1 ? 'day' : 'days'}
                      </span>
                    </div>
                  </div>

                  <RoleBadge role={invitation.role} />
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
}

export default PendingInvitations;
