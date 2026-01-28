'use client';

import { useState, useCallback } from 'react';
import { Send } from 'lucide-react';
import { Button } from '@/components/ui';
import { useSendCollaborationRequest } from '@/hooks/use-collaborations';
import type { DiscoveryCreatorDto } from '@/types/discovery';

interface SendCollaborationDialogProps {
  creator: DiscoveryCreatorDto;
  isOpen: boolean;
  onClose: () => void;
}

const MAX_MESSAGE_LENGTH = 1000;

/**
 * Dialog for sending a collaboration request to a creator
 * Story: ACF-011
 */
export function SendCollaborationDialog({
  creator,
  isOpen,
  onClose,
}: SendCollaborationDialogProps) {
  const [message, setMessage] = useState('');
  const sendMutation = useSendCollaborationRequest();
  const { mutateAsync: sendRequest, isPending, error: sendError } = sendMutation;

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (!message.trim()) return;

      try {
        await sendRequest({
          recipientId: creator.id,
          message: message.trim(),
        });
        setMessage('');
        onClose();
      } catch {
        // Error is handled by React Query
      }
    },
    [message, creator.id, sendRequest, onClose]
  );

  const handleClose = useCallback(() => {
    setMessage('');
    onClose();
  }, [onClose]);

  if (!isOpen) return null;

  const charsRemaining = MAX_MESSAGE_LENGTH - message.length;
  const isOverLimit = charsRemaining < 0;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/50"
        onClick={handleClose}
        role="presentation"
      />

      {/* Dialog */}
      <div
        className="relative bg-background rounded-lg shadow-xl w-full max-w-md mx-4 p-6"
        role="dialog"
        aria-modal="true"
        aria-labelledby="collab-dialog-title"
        onKeyDown={(e) => {
          if (e.key === 'Escape') handleClose();
        }}
      >
        <h2 id="collab-dialog-title" className="text-lg font-semibold mb-1">Request Collaboration</h2>
        <p className="text-sm text-muted-foreground mb-4">
          Send a collaboration proposal to{' '}
          <span className="font-medium">{creator.displayName}</span>
        </p>

        <form onSubmit={handleSubmit}>
          <div className="space-y-3">
            <div>
              <label htmlFor="collab-message" className="text-sm font-medium">
                Your proposal
              </label>
              <textarea
                id="collab-message"
                className="mt-1 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring min-h-[120px] resize-none"
                placeholder="Describe your collaboration idea..."
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                maxLength={MAX_MESSAGE_LENGTH}
                required
              />
              <p
                className={`text-xs mt-1 ${
                  isOverLimit ? 'text-destructive' : 'text-muted-foreground'
                }`}
              >
                {charsRemaining} characters remaining
              </p>
            </div>

            {sendError && (
              <p className="text-sm text-destructive">
                {sendError instanceof Error
                  ? sendError.message
                  : 'Failed to send request'}
              </p>
            )}
          </div>

          <div className="flex justify-end gap-2 mt-4">
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={
                !message.trim() || isOverLimit || isPending
              }
            >
              <Send className="h-4 w-4 mr-1.5" />
              {isPending ? 'Sending...' : 'Send Request'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default SendCollaborationDialog;
