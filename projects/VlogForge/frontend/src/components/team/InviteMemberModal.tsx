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
import { Button, Input, Label } from '@/components/ui';
import { RoleSelector } from './RoleSelector';
import { useInviteMember } from '@/hooks';
import { TeamRole, getAssignableRoles, ApiError } from '@/types';

interface InviteMemberModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  teamId: string;
  currentUserRole: TeamRole;
  isCurrentUserOwner: boolean;
}

/**
 * Modal for inviting members to team
 * ACF-015 Phase 5
 */
export function InviteMemberModal({
  open,
  onOpenChange,
  teamId,
  currentUserRole,
  isCurrentUserOwner,
}: InviteMemberModalProps) {
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<TeamRole>(TeamRole.Viewer);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const inviteMember = useInviteMember(teamId);

  const availableRoles = getAssignableRoles(currentUserRole, isCurrentUserOwner);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    if (!email.trim()) {
      setError('Email is required');
      return;
    }

    // Basic email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setError('Please enter a valid email address');
      return;
    }

    try {
      await inviteMember.mutateAsync({
        email: email.trim(),
        role,
      });

      setSuccess(true);
      setEmail('');
      setRole(TeamRole.Viewer);

      // Auto-close after 2 seconds
      setTimeout(() => {
        setSuccess(false);
        onOpenChange(false);
      }, 2000);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail ?? err.title);
      } else {
        setError('Failed to send invitation. Please try again.');
      }
    }
  };

  const handleCancel = () => {
    setEmail('');
    setRole(TeamRole.Viewer);
    setError(null);
    setSuccess(false);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Invite Team Member</DialogTitle>
            <DialogDescription>
              Send an invitation email to add a new member to your team.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="email">
                Email Address <span className="text-destructive">*</span>
              </Label>
              <Input
                id="email"
                type="email"
                placeholder="member@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

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

            {success && (
              <div className="rounded-md bg-green-500/15 px-3 py-2 text-sm text-green-600 dark:text-green-400">
                Invitation sent successfully!
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={inviteMember.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={inviteMember.isPending}>
              Send Invitation
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default InviteMemberModal;
