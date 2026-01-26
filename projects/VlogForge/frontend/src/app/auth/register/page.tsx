import type { Metadata } from 'next';
import Link from 'next/link';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui';
import { RegisterForm } from '@/components/forms';

export const metadata: Metadata = {
  title: 'Create Account',
  description: 'Create a new account',
};

export default function RegisterPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/50 px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <Link href="/" className="mb-4 inline-block text-xl font-bold">
            ProjectName
          </Link>
          <CardTitle className="text-2xl">Create an account</CardTitle>
          <CardDescription>
            Get started with your free account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <RegisterForm />
        </CardContent>
      </Card>
    </div>
  );
}
