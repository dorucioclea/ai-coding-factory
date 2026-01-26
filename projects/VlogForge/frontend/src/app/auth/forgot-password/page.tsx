'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { useState } from 'react';
import { ArrowLeft, Mail } from 'lucide-react';

import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui';
import { FormField } from '@/components/forms';
import {
  forgotPasswordSchema,
  type ForgotPasswordFormData,
} from '@/lib/validations';
import { useAuth } from '@/hooks/use-auth';
import { ApiError } from '@/types';

export default function ForgotPasswordPage() {
  const { forgotPassword } = useAuth();
  const [isSuccess, setIsSuccess] = useState(false);

  const {
    control,
    handleSubmit,
    setError,
    formState: { isSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: '',
    },
  });

  const onSubmit = async (data: ForgotPasswordFormData) => {
    try {
      await forgotPassword(data);
      setIsSuccess(true);
    } catch (error) {
      if (error instanceof ApiError) {
        setError('root', {
          message: error.detail ?? 'Failed to send reset email',
        });
      } else {
        setError('root', {
          message: 'An unexpected error occurred. Please try again.',
        });
      }
    }
  };

  if (isSuccess) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-muted/50 px-4">
        <Card className="w-full max-w-md">
          <CardHeader className="space-y-1 text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
              <Mail className="h-6 w-6 text-primary" />
            </div>
            <CardTitle className="text-2xl">Check your email</CardTitle>
            <CardDescription>
              We&apos;ve sent a password reset link to your email address. Please
              check your inbox and follow the instructions.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="outline" className="w-full" asChild>
              <Link href="/auth/login">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to sign in
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/50 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <Link href="/" className="mb-4 inline-block text-xl font-bold">
            ProjectName
          </Link>
          <CardTitle className="text-2xl">Reset your password</CardTitle>
          <CardDescription>
            Enter your email and we&apos;ll send you a reset link
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={control}
              name="email"
              label="Email"
              type="email"
              placeholder="Enter your email"
              autoComplete="email"
              required
            />

            <Button
              type="submit"
              className="w-full"
              isLoading={isSubmitting}
              loadingText="Sending..."
            >
              Send reset link
            </Button>

            <Button variant="outline" className="w-full" asChild>
              <Link href="/auth/login">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to sign in
              </Link>
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
