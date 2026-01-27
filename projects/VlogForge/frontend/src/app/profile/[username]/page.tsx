'use client';

import { use } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft, UserX } from 'lucide-react';
import { Button, Skeleton } from '@/components/ui';
import { ProfileCard } from '@/components/profile';
import { useProfile } from '@/hooks';

interface ProfilePageProps {
  params: Promise<{
    username: string;
  }>;
}

/**
 * Public profile view page - displays creator's public profile
 */
export default function PublicProfilePage({ params }: ProfilePageProps) {
  const router = useRouter();
  const { username } = use(params);

  // Fetch public profile
  const {
    data: profile,
    isLoading,
    error,
  } = useProfile(username);

  // Loading state
  if (isLoading) {
    return (
      <div className="container max-w-4xl py-8">
        <div className="space-y-6">
          <Skeleton className="h-8 w-32" />
          <Skeleton className="h-96 w-full" />
        </div>
      </div>
    );
  }

  // Error state
  if (error || !profile) {
    return (
      <div className="container max-w-4xl py-8">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>

        <div className="rounded-lg border border-muted bg-muted/10 p-12 text-center">
          <UserX className="h-16 w-16 mx-auto text-muted-foreground mb-4" />
          <h2 className="text-xl font-semibold mb-2">Profile not found</h2>
          <p className="text-sm text-muted-foreground mb-6">
            The profile you&apos;re looking for doesn&apos;t exist or has been removed.
          </p>
          <Button onClick={() => router.push('/dashboard')}>
            Go to Dashboard
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container max-w-4xl py-8">
      {/* Header */}
      <div className="mb-8">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>
        <h1 className="text-3xl font-bold">Creator Profile</h1>
      </div>

      {/* Profile Card */}
      <ProfileCard profile={profile} />
    </div>
  );
}
