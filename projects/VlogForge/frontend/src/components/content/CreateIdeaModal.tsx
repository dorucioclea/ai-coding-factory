'use client';

import { Plus } from 'lucide-react';
import { useState } from 'react';

import {
  Button,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui';
import { useCreateIdea } from '@/hooks';
import type { CreateContentIdeaRequest } from '@/types';

import { EditIdeaForm } from './EditIdeaForm';

interface CreateIdeaModalProps {
  onSuccess?: () => void;
}

export function CreateIdeaModal({ onSuccess }: CreateIdeaModalProps) {
  const [isOpen, setIsOpen] = useState(false);
  const createMutation = useCreateIdea();

  const handleSubmit = async (data: CreateContentIdeaRequest | { title?: string; notes?: string; platformTags?: string[]; scheduledDate?: string }) => {
    // Validate required fields for create
    if (!data.title || !('platformTags' in data) || !data.platformTags?.length) {
      throw new Error('Title and platform tags are required');
    }
    const createData = data as CreateContentIdeaRequest;
    try {
      await createMutation.mutateAsync(createData);
      setIsOpen(false);
      onSuccess?.();
    } catch (error) {
      // Re-throw to let the form handle the error display
      throw error;
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          New Idea
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create Content Idea</DialogTitle>
          <DialogDescription>
            Add a new content idea to your collection. You can refine it later.
          </DialogDescription>
        </DialogHeader>
        <EditIdeaForm
          onSubmit={handleSubmit}
          onCancel={() => setIsOpen(false)}
          isSubmitting={createMutation.isPending}
        />
      </DialogContent>
    </Dialog>
  );
}

export default CreateIdeaModal;
