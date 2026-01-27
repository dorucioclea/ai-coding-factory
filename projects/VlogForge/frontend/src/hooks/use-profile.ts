'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  CreatorProfileResponse,
  PublicProfileResponse,
  UpdateProfileRequest,
  AvatarUploadResponse,
  ApiResponse,
} from '@/types';

/**
 * Hook to fetch public profile by username
 */
export function useProfile(username: string) {
  return useQuery({
    queryKey: queryKeys.profiles.public(username),
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<PublicProfileResponse>>(
        `/profiles/${username}`
      );
      return response.data;
    },
    enabled: !!username,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook to fetch current user's full profile
 */
export function useMyProfile() {
  return useQuery({
    queryKey: queryKeys.profiles.my(),
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<CreatorProfileResponse>>(
        '/profiles/me'
      );
      return response.data;
    },
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook to update profile
 */
export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: UpdateProfileRequest) => {
      const response = await apiClient.put<ApiResponse<CreatorProfileResponse>>(
        '/profiles/me',
        data
      );
      return response.data;
    },
    onSuccess: (updatedProfile) => {
      // Update the my profile cache
      queryClient.setQueryData(
        queryKeys.profiles.my(),
        updatedProfile
      );

      // Invalidate public profile if username exists
      if (updatedProfile?.username) {
        queryClient.invalidateQueries({
          queryKey: queryKeys.profiles.public(updatedProfile.username),
        });
      }
    },
  });
}

/**
 * Hook to upload avatar image
 */
export function useUploadAvatar() {
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      // Validate file
      if (!file.type.startsWith('image/')) {
        throw new Error('File must be an image');
      }

      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        throw new Error('Image must be less than 5MB');
      }

      // Create form data
      const formData = new FormData();
      formData.append('avatar', file);

      // Upload using fetch directly for multipart/form-data
      const token = document.cookie
        .split('; ')
        .find((row) => row.startsWith('auth-token='))
        ?.split('=')[1];

      const response = await fetch(
        `${process.env['NEXT_PUBLIC_API_URL'] ?? 'http://localhost:5000/api'}/profiles/me/avatar`,
        {
          method: 'POST',
          headers: {
            ...(token && { Authorization: `Bearer ${token}` }),
          },
          body: formData,
          credentials: 'include',
        }
      );

      if (!response.ok) {
        throw new Error('Avatar upload failed');
      }

      const data = (await response.json()) as ApiResponse<AvatarUploadResponse>;
      return data.data;
    },
    onSuccess: () => {
      // Invalidate profile queries to refetch with new avatar
      queryClient.invalidateQueries({
        queryKey: queryKeys.profiles.all,
      });
    },
  });

  const clearAvatar = useCallback(async () => {
    // Delete avatar by sending null/empty request
    const response = await apiClient.delete<ApiResponse<void>>(
      '/profiles/me/avatar'
    );

    // Invalidate profile queries
    queryClient.invalidateQueries({
      queryKey: queryKeys.profiles.all,
    });

    return response;
  }, [queryClient]);

  return {
    upload: uploadMutation.mutateAsync,
    clearAvatar,
    isUploading: uploadMutation.isPending,
    uploadError: uploadMutation.error,
  };
}

/**
 * Hook for profile utilities
 */
export function useProfileHelpers() {
  /**
   * Validate niche tags
   */
  const validateNicheTags = useCallback((tags: string[]): boolean => {
    const maxTags = 5;
    return tags.length <= maxTags;
  }, []);

  /**
   * Format niche tags for display
   */
  const formatNicheTags = useCallback((tags: string[]): string => {
    return tags.join(', ');
  }, []);

  /**
   * Check if profile is complete
   */
  const isProfileComplete = useCallback(
    (profile: CreatorProfileResponse | PublicProfileResponse | undefined | null): boolean => {
      if (!profile) return false;
      return !!(
        profile.displayName &&
        profile.bio &&
        profile.nicheTags.length > 0
      );
    },
    []
  );

  return {
    validateNicheTags,
    formatNicheTags,
    isProfileComplete,
  };
}

export default useMyProfile;
