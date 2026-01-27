'use client';

import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui';
import { RoleSelector } from './RoleSelector';
import { useChangeMemberRole } from '@/hooks';
import {
  TeamRole,
  getAssignableRoles,
  ApiError,
  type TeamMemberResponse,
} from '@/types';

interface ChangeRoleModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  teamId: string;
  member: TeamMemberResponse | null;
  currentUserRole: TeamRole;
  isCurrentUserOwner: boolean;
}

/**
 * Modal for changing a member's role
 * ACF-015 Phase 5
 */
export function ChangeRoleModal({
  open,
  onOpenChange,
  teamId,
  member,
  currentUserRole,
  isCurrentUserOwner,
}: ChangeRoleModalProps) {
  const [role, setRole] = useState<TeamRole>(member?.role ?? TeamRole.Viewer);
  const [error, setError] = useState<string | null>(null);

  const changeMemberRole = useChangeMemberRole(teamId, member?.userId ?? '');

  const availableRoles = getAssignableRoles(currentUserRole, isCurrentUserOwner);

  const displayName =
    member?.displayName ||
    (member?.firstName && member?.lastName
      ? `${member.firstName} ${member.lastName}`
      : member?.email);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!member) return;

    setError(null);

    if (role === member.role) {
      onOpenChange(false);
      return;
    }

    try {
      await changeMemberRole.mutateAsync({ role });
      onOpenChange(false);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail ?? err.title);
      } else {
        setError('Failed to change role. Please try again.');
      }
    }
  };

  const handleCancel = () => {
    setRole(member?.role ?? TeamRole.Viewer);
    setError(null);
    onOpenChange(false);
  };

  // Update role when member changes
  if (member && role !== member.role && !changeMemberRole.isPending) {
    setRole(member.role);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[450px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Change Member Role</DialogTitle>
            <DialogDescription>
              Update the role for <strong>{displayName}</strong>.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <RoleSelector
              value={role}
              onChange={setRole}
              availableRoles={availableRoles}
            />

            {error && (
              <div className="rounded-md bg-destructive/15 px-3 py-2 text-sm text-destructive">
                {error}
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={changeMemberRole.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={changeMemberRole.isPending}>
              Update Role
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default ChangeRoleModal;
