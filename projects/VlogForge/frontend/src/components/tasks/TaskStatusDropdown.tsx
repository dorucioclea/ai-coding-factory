'use client';

import { useState } from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui';
import { AssignmentStatus, AssignmentStatusLabels } from '@/types';

interface TaskStatusDropdownProps {
  currentStatus: AssignmentStatus;
  onStatusChange: (status: AssignmentStatus) => void;
  disabled?: boolean;
  className?: string;
}

/**
 * Dropdown for changing task status
 * ACF-015 Phase 6
 */
export function TaskStatusDropdown({
  currentStatus,
  onStatusChange,
  disabled = false,
  className,
}: TaskStatusDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);

  const handleValueChange = (value: string) => {
    const newStatus = parseInt(value, 10) as AssignmentStatus;
    onStatusChange(newStatus);
    setIsOpen(false);
  };

  return (
    <Select
      value={currentStatus.toString()}
      onValueChange={handleValueChange}
      disabled={disabled}
      open={isOpen}
      onOpenChange={setIsOpen}
    >
      <SelectTrigger className={className}>
        <SelectValue>
          {AssignmentStatusLabels[currentStatus]}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={AssignmentStatus.NotStarted.toString()}>
          {AssignmentStatusLabels[AssignmentStatus.NotStarted]}
        </SelectItem>
        <SelectItem value={AssignmentStatus.InProgress.toString()}>
          {AssignmentStatusLabels[AssignmentStatus.InProgress]}
        </SelectItem>
        <SelectItem value={AssignmentStatus.Completed.toString()}>
          {AssignmentStatusLabels[AssignmentStatus.Completed]}
        </SelectItem>
      </SelectContent>
    </Select>
  );
}

export default TaskStatusDropdown;
