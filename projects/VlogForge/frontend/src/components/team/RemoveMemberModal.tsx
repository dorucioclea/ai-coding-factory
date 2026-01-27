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
import { useRemoveMember } from '@/hooks';
import { ApiError, type TeamMemberResponse } from '@/types';

interface RemoveMemberModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  teamId: string;
  member: TeamMemberResponse | null;
}

/**
 * Confirmation dialog for removing team member
 * ACF-015 Phase 5
 */
export function RemoveMemberModal({
  open,
  onOpenChange,
  teamId,
  member,
}: RemoveMemberModalProps) {
  const [error, setError] = useState<string | null>(null);

  const removeMember = useRemoveMember(teamId, member?.userId ?? '');

  const displayName =
    member?.displayName ||
    (member?.firstName && member?.lastName
      ? `${member.firstName} ${member.lastName}`
      : member?.email);

  const handleRemove = async () => {
    if (!member) return;

    setError(null);

    try {
      await removeMember.mutateAsync();
      onOpenChange(false);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail ?? err.title);
      } else {
        setError('Failed to remove member. Please try again.');
      }
    }
  };

  const handleCancel = () => {
    setError(null);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[450px]">
        <DialogHeader>
          <DialogTitle>Remove Team Member</DialogTitle>
          <DialogDescription>
            Are you sure you want to remove <strong>{displayName}</strong> from the
            team? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        {error && (
          <div className="rounded-md bg-destructive/15 px-3 py-2 text-sm text-destructive">
            {error}
          </div>
        )}

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            disabled={removeMember.isPending}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleRemove}
            isLoading={removeMember.isPending}
          >
            Remove Member
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default RemoveMemberModal;
