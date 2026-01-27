'use client';

import { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { Button, Skeleton } from '@/components/ui';
import { ProfileEditForm } from '@/components/profile';
import {
  useMyProfile,
  useUpdateProfile,
  useUploadAvatar,
  useRequireAuth,
} from '@/hooks';
import type { UpdateProfileRequest } from '@/types';

/**
 * Profile edit page - allows users to edit their creator profile
 */
export default function ProfilePage() {
  const router = useRouter();

  // Require authentication
  useRequireAuth();

  // Fetch profile data
  const {
    data: profile,
    isLoading: isLoadingProfile,
    error: profileError,
  } = useMyProfile();

  // Update profile mutation
  const updateProfileMutation = useUpdateProfile();

  // Avatar upload mutation
  const {
    upload: uploadAvatar,
    clearAvatar,
    isUploading: isUploadingAvatar,
  } = useUploadAvatar();

  // Handle profile update
  const handleUpdate = useCallback(
    async (data: UpdateProfileRequest) => {
      try {
        await updateProfileMutation.mutateAsync(data);
        toast.success('Profile updated successfully');
      } catch (error) {
        toast.error(
          error instanceof Error ? error.message : 'Failed to update profile'
        );
        throw error;
      }
    },
    [updateProfileMutation]
  );

  // Handle avatar upload
  const handleAvatarUpload = useCallback(
    async (file: File) => {
      try {
        await uploadAvatar(file);
        toast.success('Avatar uploaded successfully');
      } catch (error) {
        toast.error(
          error instanceof Error ? error.message : 'Failed to upload avatar'
        );
        throw error;
      }
    },
    [uploadAvatar]
  );

  // Handle avatar removal
  const handleAvatarRemove = useCallback(async () => {
    try {
      await clearAvatar();
      toast.success('Avatar removed successfully');
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : 'Failed to remove avatar'
      );
      throw error;
    }
  }, [clearAvatar]);

  // Loading state
  if (isLoadingProfile) {
    return (
      <div className="container max-w-4xl py-8">
        <div className="space-y-6">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-64 w-full" />
          <Skeleton className="h-96 w-full" />
        </div>
      </div>
    );
  }

  // Error state
  if (profileError || !profile) {
    return (
      <div className="container max-w-4xl py-8">
        <div className="rounded-lg border border-destructive bg-destructive/10 p-6 text-center">
          <h2 className="text-lg font-semibold text-destructive mb-2">
            Failed to load profile
          </h2>
          <p className="text-sm text-muted-foreground mb-4">
            {profileError instanceof Error
              ? profileError.message
              : 'An unexpected error occurred'}
          </p>
          <Button onClick={() => router.push('/dashboard')}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Dashboard
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
        <h1 className="text-3xl font-bold">Edit Profile</h1>
        <p className="text-muted-foreground mt-2">
          Manage your creator profile and collaboration settings
        </p>
      </div>

      {/* Profile Edit Form */}
      <ProfileEditForm
        profile={profile}
        onUpdate={handleUpdate}
        onAvatarUpload={handleAvatarUpload}
        onAvatarRemove={handleAvatarRemove}
        isUpdating={updateProfileMutation.isPending}
        isUploadingAvatar={isUploadingAvatar}
      />
    </div>
  );
}
