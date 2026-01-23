'use client';

import * as React from 'react';
import {
  Controller,
  type Control,
  type FieldPath,
  type FieldValues,
} from 'react-hook-form';
import { Input, Label } from '@/components/ui';
import { cn } from '@/lib/utils';

interface FormFieldProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> {
  control: Control<TFieldValues>;
  name: TName;
  label: string;
  placeholder?: string;
  type?: React.InputHTMLAttributes<HTMLInputElement>['type'];
  description?: string;
  disabled?: boolean;
  className?: string;
  required?: boolean;
  autoComplete?: string;
}

export function FormField<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>({
  control,
  name,
  label,
  placeholder,
  type = 'text',
  description,
  disabled,
  className,
  required,
  autoComplete,
}: FormFieldProps<TFieldValues, TName>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState: { error } }) => (
        <div className={cn('space-y-2', className)}>
          <Label htmlFor={name} className="flex items-center gap-1">
            {label}
            {required && <span className="text-destructive">*</span>}
          </Label>
          <Input
            {...field}
            id={name}
            type={type}
            placeholder={placeholder}
            disabled={disabled}
            autoComplete={autoComplete}
            error={!!error}
            aria-describedby={
              error ? `${name}-error` : description ? `${name}-desc` : undefined
            }
            aria-invalid={!!error}
          />
          {description && !error && (
            <p id={`${name}-desc`} className="text-sm text-muted-foreground">
              {description}
            </p>
          )}
          {error && (
            <p
              id={`${name}-error`}
              className="text-sm text-destructive"
              role="alert"
            >
              {error.message}
            </p>
          )}
        </div>
      )}
    />
  );
}

export default FormField;
