/**
 * Quick add modal for creating content with pre-filled date (ACF-015 Phase 4)
 */

'use client';

import { useState } from 'react';
import { format } from 'date-fns';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Button,
  Input,
  Label,
} from '@/components/ui';
import { useCreateIdea } from '@/hooks';
import type { CreateContentIdeaRequest } from '@/types';
import { toast } from 'sonner';

interface QuickAddModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  date: string;
}

export function QuickAddModal({
  open,
  onOpenChange,
  date,
}: QuickAddModalProps) {
  const [title, setTitle] = useState('');
  const [notes, setNotes] = useState('');
  const createMutation = useCreateIdea();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!title.trim()) {
      toast.error('Title is required');
      return;
    }

    const request: CreateContentIdeaRequest = {
      title: title.trim(),
      notes: notes.trim() || undefined,
      platformTags: [],
      scheduledDate: date,
    };

    try {
      await createMutation.mutateAsync(request);
      toast.success('Content created and scheduled');
      setTitle('');
      setNotes('');
      onOpenChange(false);
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : 'Failed to create content'
      );
    }
  };

  const handleCancel = () => {
    setTitle('');
    setNotes('');
    onOpenChange(false);
  };

  // Format date safely - only when date is valid
  const formattedDate = date
    ? format(new Date(date), 'MMMM d, yyyy')
    : '';

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Quick Add Content</DialogTitle>
          <DialogDescription>
            Create content scheduled for {formattedDate}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="title">Title *</Label>
            <Input
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Enter content title..."
              autoFocus
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Input
              id="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Optional notes..."
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={createMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              isLoading={createMutation.isPending}
              loadingText="Creating..."
            >
              Create
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
