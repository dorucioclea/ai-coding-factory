'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { ArrowLeft, UserPlus, Settings } from 'lucide-react';
import { Button, Skeleton, Separator } from '@/components/ui';
import { MemberList, InviteMemberModal, PendingInvitations } from '@/components/team';
import { useTeam, useAuth } from '@/hooks';
import { getTeamPermissions } from '@/types';

/**
 * Team Detail Page
 * ACF-015 Phase 5
 *
 * Shows team members, pending invitations, and allows team management
 */
export default function TeamDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const teamId = params?.['id'] as string | undefined;

  const [inviteModalOpen, setInviteModalOpen] = useState(false);

  const { data: team, isLoading, error } = useTeam(teamId);

  if (!teamId) {
    return (
      <div className="container mx-auto py-6">
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          Invalid team ID
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <Skeleton className="h-8 w-32" />
        <Skeleton className="h-12 w-64" />
        <Skeleton className="h-6 w-48" />
        <Separator />
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-20" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto py-6 space-y-4">
        <Button variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading team</h3>
          <p className="text-sm mt-1">
            {error instanceof Error ? error.message : 'An unexpected error occurred'}
          </p>
        </div>
      </div>
    );
  }

  if (!team) {
    return (
      <div className="container mx-auto py-6">
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          Team not found
        </div>
      </div>
    );
  }

  const currentUserMember = team.members.find((m) => m.userId === user?.id);
  const currentUserRole = currentUserMember?.role ?? 0;
  const isOwner = user?.id === team.ownerId;

  const permissions = getTeamPermissions(currentUserRole, isOwner);

  // Mock pending invitations - in real app, fetch from API
  const pendingInvitations: never[] = [];

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => router.push('/dashboard/team')}>
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back to Teams
      </Button>

      {/* Team Header */}
      <div className="flex items-start justify-between">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold">{team.name}</h1>
          {team.description && (
            <p className="text-muted-foreground max-w-2xl">{team.description}</p>
          )}
          <p className="text-sm text-muted-foreground">
            {team.members.length} {team.members.length === 1 ? 'member' : 'members'}
          </p>
        </div>

        <div className="flex gap-2">
          {permissions.canInvite && (
            <Button onClick={() => setInviteModalOpen(true)}>
              <UserPlus className="h-4 w-4 mr-2" />
              Invite Member
            </Button>
          )}
          {permissions.canEditTeam && (
            <Button variant="outline" disabled>
              <Settings className="h-4 w-4 mr-2" />
              Settings
            </Button>
          )}
        </div>
      </div>

      <Separator />

      {/* Pending Invitations */}
      {pendingInvitations.length > 0 && (
        <>
          <PendingInvitations invitations={pendingInvitations} />
          <Separator />
        </>
      )}

      {/* Members List */}
      <div className="space-y-4">
        <h2 className="text-xl font-semibold">Team Members</h2>
        <MemberList teamId={teamId} ownerId={team.ownerId} members={team.members} />
      </div>

      {/* Invite Modal */}
      {permissions.canInvite && (
        <InviteMemberModal
          open={inviteModalOpen}
          onOpenChange={setInviteModalOpen}
          teamId={teamId}
          currentUserRole={currentUserRole}
          isCurrentUserOwner={isOwner}
        />
      )}
    </div>
  );
}
