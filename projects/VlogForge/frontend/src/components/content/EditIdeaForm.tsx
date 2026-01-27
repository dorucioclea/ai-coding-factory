'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { X } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';

import { Button, Input, Label, Textarea } from '@/components/ui';
import { cn } from '@/lib/utils';
import type {
  ContentIdeaResponse,
  CreateContentIdeaRequest,
  UpdateContentIdeaRequest,
} from '@/types';
import { PLATFORM_TAGS } from '@/types';

const ideaFormSchema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title must be less than 200 characters'),
  notes: z.string().max(2000, 'Notes must be less than 2000 characters').optional(),
  platformTags: z.array(z.string()).min(1, 'Select at least one platform'),
  scheduledDate: z.string().optional(),
});

type IdeaFormData = z.infer<typeof ideaFormSchema>;

interface EditIdeaFormProps {
  idea?: ContentIdeaResponse;
  onSubmit: (data: CreateContentIdeaRequest | UpdateContentIdeaRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function EditIdeaForm({
  idea,
  onSubmit,
  onCancel,
  isSubmitting,
}: EditIdeaFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<IdeaFormData>({
    resolver: zodResolver(ideaFormSchema),
    defaultValues: {
      title: idea?.title ?? '',
      notes: idea?.notes ?? '',
      platformTags: idea?.platformTags ?? [],
      scheduledDate: idea?.scheduledDate
        ? new Date(idea.scheduledDate).toISOString().split('T')[0]
        : '',
    },
  });

  const selectedTags = watch('platformTags');

  const toggleTag = (tag: string) => {
    const newTags = selectedTags.includes(tag)
      ? selectedTags.filter((t) => t !== tag)
      : [...selectedTags, tag];
    setValue('platformTags', newTags);
  };

  const onFormSubmit = async (data: IdeaFormData) => {
    const submitData = {
      title: data.title,
      notes: data.notes || undefined,
      platformTags: data.platformTags,
      scheduledDate: data.scheduledDate || undefined,
    };

    await onSubmit(submitData);
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
      {/* Title */}
      <div className="space-y-2">
        <Label htmlFor="title">
          Title <span className="text-destructive">*</span>
        </Label>
        <Input
          id="title"
          placeholder="Enter content idea title"
          {...register('title')}
          disabled={isSubmitting}
        />
        {errors.title && (
          <p className="text-sm text-destructive">{errors.title.message}</p>
        )}
      </div>

      {/* Notes */}
      <div className="space-y-2">
        <Label htmlFor="notes">Notes</Label>
        <Textarea
          id="notes"
          placeholder="Add details, scripts, or notes about this content idea"
          rows={5}
          {...register('notes')}
          disabled={isSubmitting}
        />
        {errors.notes && (
          <p className="text-sm text-destructive">{errors.notes.message}</p>
        )}
      </div>

      {/* Platform Tags */}
      <div className="space-y-2">
        <Label>
          Platforms <span className="text-destructive">*</span>
        </Label>
        <div className="flex flex-wrap gap-2">
          {PLATFORM_TAGS.map((tag) => {
            const isSelected = selectedTags.includes(tag);
            return (
              <button
                key={tag}
                type="button"
                onClick={() => toggleTag(tag)}
                disabled={isSubmitting}
                className={cn(
                  'inline-flex items-center rounded-full px-3 py-1.5 text-sm font-medium transition-colors',
                  'border-2 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                  isSelected
                    ? 'border-primary bg-primary text-primary-foreground hover:bg-primary/90'
                    : 'border-input bg-background hover:bg-accent hover:text-accent-foreground',
                  isSubmitting && 'opacity-50 cursor-not-allowed'
                )}
              >
                {tag}
                {isSelected && <X className="ml-1 h-3 w-3" />}
              </button>
            );
          })}
        </div>
        {errors.platformTags && (
          <p className="text-sm text-destructive">
            {errors.platformTags.message}
          </p>
        )}
      </div>

      {/* Scheduled Date */}
      <div className="space-y-2">
        <Label htmlFor="scheduledDate">Scheduled Date (Optional)</Label>
        <Input
          id="scheduledDate"
          type="date"
          {...register('scheduledDate')}
          disabled={isSubmitting}
        />
        {errors.scheduledDate && (
          <p className="text-sm text-destructive">
            {errors.scheduledDate.message}
          </p>
        )}
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3 pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : idea ? 'Update' : 'Create'}
        </Button>
      </div>
    </form>
  );
}

export default EditIdeaForm;
