'use client';

import { Calendar } from 'lucide-react';
import { format } from 'date-fns';
import { Input, Label } from '@/components/ui';

interface DueDatePickerProps {
  value?: string;
  onChange: (date: string) => void;
  label?: string;
  required?: boolean;
  minDate?: string;
  className?: string;
}

/**
 * Date picker for task due dates
 * ACF-015 Phase 6
 */
export function DueDatePicker({
  value,
  onChange,
  label = 'Due Date',
  required = false,
  minDate,
  className,
}: DueDatePickerProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
  };

  const today = format(new Date(), 'yyyy-MM-dd');
  const min = minDate ?? today;

  return (
    <div className={className}>
      {label && (
        <Label htmlFor="due-date" className="mb-2 block">
          {label}
          {required && <span className="text-destructive ml-1">*</span>}
        </Label>
      )}
      <div className="relative">
        <Input
          id="due-date"
          type="date"
          value={value ?? ''}
          onChange={handleChange}
          min={min}
          required={required}
          className="pl-10"
        />
        <Calendar className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground pointer-events-none" />
      </div>
    </div>
  );
}

export default DueDatePicker;
