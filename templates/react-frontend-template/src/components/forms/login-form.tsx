'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';

import { Button } from '@/components/ui';
import { FormField } from './form-field';
import { loginSchema, type LoginFormData } from '@/lib/validations';
import { useAuth } from '@/hooks/use-auth';
import { ApiError } from '@/types';

export function LoginForm() {
  const { login, isLoggingIn, loginError } = useAuth();

  const {
    control,
    handleSubmit,
    setError,
    formState: { isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      await login(data);
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.isValidationError()) {
          const fieldErrors = error.getFieldErrors();
          Object.entries(fieldErrors).forEach(([field, message]) => {
            setError(field as keyof LoginFormData, { message });
          });
        } else {
          setError('root', {
            message: error.detail ?? 'Invalid email or password',
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
      {loginError && (
        <div
          className="rounded-md bg-destructive/10 p-3 text-sm text-destructive"
          role="alert"
        >
          {loginError instanceof ApiError
            ? loginError.detail ?? 'Login failed'
            : 'An unexpected error occurred'}
        </div>
      )}

      <FormField
        control={control}
        name="email"
        label="Email"
        type="email"
        placeholder="Enter your email"
        autoComplete="email"
        required
      />

      <FormField
        control={control}
        name="password"
        label="Password"
        type="password"
        placeholder="Enter your password"
        autoComplete="current-password"
        required
      />

      <div className="flex items-center justify-between">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-input"
            {...control.register('rememberMe')}
          />
          Remember me
        </label>
        <Link
          href="/auth/forgot-password"
          className="text-sm text-primary hover:underline"
        >
          Forgot password?
        </Link>
      </div>

      <Button
        type="submit"
        className="w-full"
        isLoading={isLoggingIn || isSubmitting}
        loadingText="Signing in..."
      >
        Sign in
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        Don&apos;t have an account?{' '}
        <Link href="/auth/register" className="text-primary hover:underline">
          Create one
        </Link>
      </p>
    </form>
  );
}

export default LoginForm;
