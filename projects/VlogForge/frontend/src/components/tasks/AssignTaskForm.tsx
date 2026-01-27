'use client';

import { useState } from 'react';
import { format } from 'date-fns';
import {
  Button,
  Input,
  Label,
  Textarea,
  DialogFooter,
} from '@/components/ui';
import { DueDatePicker } from './DueDatePicker';
import type { AssignTaskRequest } from '@/types';

interface AssignTaskFormProps {
  contentItemId: string;
  onSubmit: (data: AssignTaskRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
  className?: string;
}

/**
 * Form for assigning a task to a team member
 * ACF-015 Phase 6
 */
export function AssignTaskForm({
  contentItemId,
  onSubmit,
  onCancel,
  isSubmitting = false,
  className,
}: AssignTaskFormProps) {
  const [assigneeId, setAssigneeId] = useState('');
  const [dueDate, setDueDate] = useState(
    format(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd')
  );
  const [notes, setNotes] = useState('');

  interface FormErrors {
    assigneeId?: string;
    dueDate?: string;
    submit?: string;
  }
  const [errors, setErrors] = useState<FormErrors>({});

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!assigneeId.trim()) {
      newErrors.assigneeId = 'Assignee ID is required';
    }

    if (!dueDate) {
      newErrors.dueDate = 'Due date is required';
    }

    setErrors(newErrors);
    return !newErrors.assigneeId && !newErrors.dueDate;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    const data: AssignTaskRequest = {
      contentItemId,
      assigneeId: assigneeId.trim(),
      dueDate,
      notes: notes.trim() || undefined,
    };

    try {
      await onSubmit(data);
    } catch (error) {
      if (error instanceof Error) {
        setErrors({ submit: error.message });
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className={className}>
      <div className="space-y-4">
        <div>
          <Label htmlFor="assignee-id">
            Assignee ID
            <span className="text-destructive ml-1">*</span>
          </Label>
          <Input
            id="assignee-id"
            type="text"
            value={assigneeId}
            onChange={(e) => {
              setAssigneeId(e.target.value);
              if (errors.assigneeId) {
                setErrors({ ...errors, assigneeId: undefined });
              }
            }}
            placeholder="Enter user ID"
            disabled={isSubmitting}
            className={errors.assigneeId ? 'border-destructive' : ''}
          />
          {errors.assigneeId && (
            <p className="text-sm text-destructive mt-1">
              {errors.assigneeId}
            </p>
          )}
        </div>

        <DueDatePicker
          value={dueDate}
          onChange={(date) => {
            setDueDate(date);
            if (errors.dueDate) {
              setErrors({ ...errors, dueDate: undefined });
            }
          }}
          required
        />
        {errors.dueDate && (
          <p className="text-sm text-destructive mt-1">
            {errors.dueDate}
          </p>
        )}

        <div>
          <Label htmlFor="notes">Notes (Optional)</Label>
          <Textarea
            id="notes"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Add any additional notes or instructions..."
            rows={4}
            disabled={isSubmitting}
          />
        </div>

        {errors.submit && (
          <p className="text-sm text-destructive">{errors.submit}</p>
        )}
      </div>

      <DialogFooter className="mt-6">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Assigning...' : 'Assign Task'}
        </Button>
      </DialogFooter>
    </form>
  );
}

export default AssignTaskForm;
