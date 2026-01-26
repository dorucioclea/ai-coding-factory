'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';

import { Button } from '@/components/ui';
import { FormField } from './form-field';
import { registerSchema, type RegisterFormData } from '@/lib/validations';
import { useAuth } from '@/hooks/use-auth';
import { ApiError } from '@/types';

export function RegisterForm() {
  const { register: registerUser, isRegistering, registerError } = useAuth();

  const {
    control,
    handleSubmit,
    setError,
    formState: { isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      firstName: '',
      lastName: '',
    },
  });

  const onSubmit = async (data: RegisterFormData) => {
    try {
      await registerUser(data);
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.isValidationError()) {
          const fieldErrors = error.getFieldErrors();
          Object.entries(fieldErrors).forEach(([field, message]) => {
            setError(field as keyof RegisterFormData, { message });
          });
        } else {
          setError('root', {
            message: error.detail ?? 'Registration failed',
          });
        }
      } else {
        setError('root', {
          message: 'An unexpected error occurred. Please try again.',
        });
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {registerError && (
        <div
          className="rounded-md bg-destructive/10 p-3 text-sm text-destructive"
          role="alert"
        >
          {registerError instanceof ApiError
            ? registerError.detail ?? 'Registration failed'
            : 'An unexpected error occurred'}
        </div>
      )}

      <div className="grid gap-4 sm:grid-cols-2">
        <FormField
          control={control}
          name="firstName"
          label="First name"
          placeholder="John"
          autoComplete="given-name"
          required
        />

        <FormField
          control={control}
          name="lastName"
          label="Last name"
          placeholder="Doe"
          autoComplete="family-name"
          required
        />
      </div>

      <FormField
        control={control}
        name="email"
        label="Email"
        type="email"
        placeholder="john@example.com"
        autoComplete="email"
        required
      />

      <FormField
        control={control}
        name="password"
        label="Password"
        type="password"
        placeholder="Create a strong password"
        autoComplete="new-password"
        description="Must be at least 8 characters with uppercase, lowercase, number, and special character"
        required
      />

      <FormField
        control={control}
        name="confirmPassword"
        label="Confirm password"
        type="password"
        placeholder="Confirm your password"
        autoComplete="new-password"
        required
      />

      <Button
        type="submit"
        className="w-full"
        isLoading={isRegistering || isSubmitting}
        loadingText="Creating account..."
      >
        Create account
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        Already have an account?{' '}
        <Link href="/auth/login" className="text-primary hover:underline">
          Sign in
        </Link>
      </p>
    </form>
  );
}

export default RegisterForm;
