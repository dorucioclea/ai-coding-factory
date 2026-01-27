'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { useApproval } from '@/hooks';
import { useToast } from '@/hooks/use-toast';
import { IdeaStatus, type ContentIdeaResponse } from '@/types';
import { Check, X, Send } from 'lucide-react';

interface ApprovalActionsProps {
  content: ContentIdeaResponse;
  teamId: string;
  canApprove: boolean;
  isOwner: boolean;
  onActionComplete?: () => void;
}

/**
 * Approval action buttons for content items
 * Story: ACF-009
 */
export function ApprovalActions({
  content,
  teamId,
  canApprove,
  isOwner,
  onActionComplete,
}: ApprovalActionsProps) {
  const { toast } = useToast();
  const { submitForApproval, approveContent, requestChanges, isSubmitting, isApproving, isRequestingChanges } = useApproval();

  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [feedback, setFeedback] = useState('');

  const canSubmit = isOwner && (content.status === IdeaStatus.Draft || content.status === IdeaStatus.ChangesRequested);
  const canReview = canApprove && content.status === IdeaStatus.InReview;

  // Helper to extract error message from various error types
  const getErrorMessage = (error: unknown, fallback: string): string => {
    if (error instanceof Error) {
      return error.message;
    }
    if (typeof error === 'object' && error !== null && 'message' in error) {
      return String((error as { message: unknown }).message);
    }
    return fallback;
  };

  const handleSubmit = async () => {
    try {
      await submitForApproval(content.id, { teamId });
      toast({
        title: 'Submitted for approval',
        description: 'Your content has been submitted for review.',
      });
      onActionComplete?.();
    } catch (error) {
      toast({
        title: 'Submission failed',
        description: getErrorMessage(error, 'Failed to submit for approval. Please try again.'),
        variant: 'destructive',
      });
    }
  };

  const handleApprove = async () => {
    try {
      await approveContent(content.id, { teamId, feedback: feedback || undefined });
      toast({
        title: 'Content approved',
        description: 'The content has been approved and is ready to be scheduled.',
      });
      setApproveDialogOpen(false);
      setFeedback('');
      onActionComplete?.();
    } catch (error) {
      toast({
        title: 'Approval failed',
        description: getErrorMessage(error, 'Failed to approve content. Please try again.'),
        variant: 'destructive',
      });
    }
  };

  const handleRequestChanges = async () => {
    if (!feedback.trim()) {
      toast({
        title: 'Feedback required',
        description: 'Please provide feedback explaining what changes are needed.',
        variant: 'destructive',
      });
      return;
    }

    try {
      await requestChanges(content.id, { teamId, feedback });
      toast({
        title: 'Changes requested',
        description: 'The creator has been notified of the required changes.',
      });
      setRejectDialogOpen(false);
      setFeedback('');
      onActionComplete?.();
    } catch (error) {
      toast({
        title: 'Request failed',
        description: getErrorMessage(error, 'Failed to request changes. Please try again.'),
        variant: 'destructive',
      });
    }
  };

  if (!canSubmit && !canReview) {
    return null;
  }

  return (
    <div className="flex items-center gap-2">
      {canSubmit && (
        <Button
          variant="outline"
          size="sm"
          onClick={handleSubmit}
          disabled={isSubmitting}
        >
          <Send className="mr-2 h-4 w-4" />
          {isSubmitting ? 'Submitting...' : 'Submit for Approval'}
        </Button>
      )}

      {canReview && (
        <>
          <Button
            variant="default"
            size="sm"
            onClick={() => setApproveDialogOpen(true)}
            disabled={isApproving}
          >
            <Check className="mr-2 h-4 w-4" />
            Approve
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setRejectDialogOpen(true)}
            disabled={isRequestingChanges}
          >
            <X className="mr-2 h-4 w-4" />
            Request Changes
          </Button>
        </>
      )}

      {/* Approve Dialog */}
      <Dialog open={approveDialogOpen} onOpenChange={setApproveDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Approve Content</DialogTitle>
            <DialogDescription>
              This will approve the content and allow it to be scheduled for publication.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="approve-feedback">Feedback (optional)</Label>
              <Textarea
                id="approve-feedback"
                placeholder="Add any comments or feedback..."
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setApproveDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleApprove} disabled={isApproving}>
              {isApproving ? 'Approving...' : 'Approve'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Request Changes Dialog */}
      <Dialog open={rejectDialogOpen} onOpenChange={setRejectDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Request Changes</DialogTitle>
            <DialogDescription>
              Please explain what changes are needed. The creator will be notified.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="reject-feedback">Feedback *</Label>
              <Textarea
                id="reject-feedback"
                placeholder="Describe what changes are needed..."
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRejectDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleRequestChanges} disabled={isRequestingChanges || !feedback.trim()}>
              {isRequestingChanges ? 'Sending...' : 'Request Changes'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ApprovalActions;
