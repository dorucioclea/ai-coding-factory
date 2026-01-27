'use client';

import { MoreVertical, UserMinus, UserCog } from 'lucide-react';
import {
  Avatar,
  AvatarFallback,
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui';
import { RoleBadge } from './RoleBadge';
import type { TeamMemberResponse, TeamRole } from '@/types';
import { canModifyMember } from '@/types';

interface MemberCardProps {
  member: TeamMemberResponse;
  currentUserRole: TeamRole;
  isCurrentUserOwner: boolean;
  isOwner: boolean;
  onChangeRole?: () => void;
  onRemove?: () => void;
}

/**
 * Individual team member display card
 * ACF-015 Phase 5
 */
export function MemberCard({
  member,
  currentUserRole,
  isCurrentUserOwner,
  isOwner,
  onChangeRole,
  onRemove,
}: MemberCardProps) {
  const canModify = canModifyMember(
    currentUserRole,
    member.role,
    isCurrentUserOwner,
    isOwner
  );

  const displayName =
    member.displayName ||
    (member.firstName && member.lastName
      ? `${member.firstName} ${member.lastName}`
      : member.email) || 'Unknown User';

  const initials = member.firstName && member.lastName
    ? `${member.firstName[0]}${member.lastName[0]}`.toUpperCase()
    : displayName.substring(0, 2).toUpperCase();

  const joinedDate = new Date(member.joinedAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <div className="flex items-center justify-between p-4 rounded-lg border bg-card hover:bg-accent/50 transition-colors">
      <div className="flex items-center gap-3 flex-1 min-w-0">
        <Avatar>
          <AvatarFallback>{initials}</AvatarFallback>
        </Avatar>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <p className="font-medium truncate">{displayName}</p>
            {isOwner && <RoleBadge role={member.role} />}
          </div>
          <p className="text-sm text-muted-foreground truncate">{member.email}</p>
          <p className="text-xs text-muted-foreground">Joined {joinedDate}</p>
        </div>

        {!isOwner && <RoleBadge role={member.role} />}
      </div>

      {canModify && (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon">
              <MoreVertical className="h-4 w-4" />
              <span className="sr-only">Member actions</span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {onChangeRole && (
              <DropdownMenuItem onClick={onChangeRole}>
                <UserCog className="h-4 w-4 mr-2" />
                Change Role
              </DropdownMenuItem>
            )}
            {onRemove && (
              <DropdownMenuItem
                onClick={onRemove}
                className="text-destructive focus:text-destructive"
              >
                <UserMinus className="h-4 w-4 mr-2" />
                Remove Member
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      )}
    </div>
  );
}

export default MemberCard;
