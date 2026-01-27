'use client';

import { useState } from 'react';
import { MemberCard } from './MemberCard';
import { RemoveMemberModal } from './RemoveMemberModal';
import { ChangeRoleModal } from './ChangeRoleModal';
import type { TeamMemberResponse } from '@/types';
import { useAuth } from '@/hooks';

interface MemberListProps {
  teamId: string;
  ownerId: string;
  members: TeamMemberResponse[];
}

/**
 * List of team members
 * ACF-015 Phase 5
 */
export function MemberList({ teamId, ownerId, members }: MemberListProps) {
  const { user } = useAuth();
  const [memberToRemove, setMemberToRemove] = useState<TeamMemberResponse | null>(null);
  const [memberToChangeRole, setMemberToChangeRole] = useState<TeamMemberResponse | null>(null);

  const currentUserMember = members.find((m) => m.userId === user?.id);
  const currentUserRole = currentUserMember?.role ?? 0;
  const isCurrentUserOwner = user?.id === ownerId;

  // Sort members: Owner first, then by role, then by join date
  const sortedMembers = [...members].sort((a, b) => {
    if (a.userId === ownerId) return -1;
    if (b.userId === ownerId) return 1;
    if (a.role !== b.role) return b.role - a.role;
    return new Date(a.joinedAt).getTime() - new Date(b.joinedAt).getTime();
  });

  return (
    <>
      <div className="space-y-2">
        {sortedMembers.map((member) => (
          <MemberCard
            key={member.id}
            member={member}
            currentUserRole={currentUserRole}
            isCurrentUserOwner={isCurrentUserOwner}
            isOwner={member.userId === ownerId}
            onChangeRole={() => setMemberToChangeRole(member)}
            onRemove={() => setMemberToRemove(member)}
          />
        ))}
      </div>

      <RemoveMemberModal
        open={!!memberToRemove}
        onOpenChange={(open) => !open && setMemberToRemove(null)}
        teamId={teamId}
        member={memberToRemove}
      />

      <ChangeRoleModal
        open={!!memberToChangeRole}
        onOpenChange={(open) => !open && setMemberToChangeRole(null)}
        teamId={teamId}
        member={memberToChangeRole}
        currentUserRole={currentUserRole}
        isCurrentUserOwner={isCurrentUserOwner}
      />
    </>
  );
}

export default MemberList;
