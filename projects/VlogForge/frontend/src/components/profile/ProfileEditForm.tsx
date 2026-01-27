'use client';

import { useCallback, useState, useEffect } from 'react';
import { Save, Loader2 } from 'lucide-react';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Button,
  Input,
  Label,
  Textarea,
  Separator,
} from '@/components/ui';
import { AvatarUpload } from './AvatarUpload';
import { NicheTagSelector } from './NicheTagSelector';
import { CollaborationToggle } from './CollaborationToggle';
import type {
  CreatorProfileResponse,
  UpdateProfileRequest,
} from '@/types';
import { ProfileConstraints } from '@/types';
import { cn } from '@/lib/utils';

interface ProfileEditFormProps {
  profile: CreatorProfileResponse;
  onUpdate: (data: UpdateProfileRequest) => Promise<void>;
  onAvatarUpload: (file: File) => Promise<void>;
  onAvatarRemove?: () => Promise<void>;
  isUpdating?: boolean;
  isUploadingAvatar?: boolean;
  className?: string;
}

/**
 * Form for editing creator profile
 */
export function ProfileEditForm({
  profile,
  onUpdate,
  onAvatarUpload,
  onAvatarRemove,
  isUpdating = false,
  isUploadingAvatar = false,
  className,
}: ProfileEditFormProps) {
  // Form state (immutable updates)
  const [displayName, setDisplayName] = useState(profile.displayName);
  const [bio, setBio] = useState(profile.bio);
  const [nicheTags, setNicheTags] = useState(profile.nicheTags);
  const [openToCollaborations, setOpenToCollaborations] = useState(
    profile.openToCollaborations
  );
  const [collaborationPreferences, setCollaborationPreferences] = useState(
    profile.collaborationPreferences ?? ''
  );
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Update form when profile changes
  useEffect(() => {
    setDisplayName(profile.displayName);
    setBio(profile.bio);
    setNicheTags(profile.nicheTags);
    setOpenToCollaborations(profile.openToCollaborations);
    setCollaborationPreferences(profile.collaborationPreferences ?? '');
  }, [profile]);

  // Validation
  const validate = useCallback((): boolean => {
    const newErrors: Record<string, string> = {};

    if (!displayName.trim()) {
      newErrors['displayName'] = 'Display name is required';
    } else if (displayName.length > ProfileConstraints.maxDisplayNameLength) {
      newErrors['displayName'] = `Display name must be ${ProfileConstraints.maxDisplayNameLength} characters or less`;
    }

    if (!bio.trim()) {
      newErrors['bio'] = 'Bio is required';
    } else if (bio.length > ProfileConstraints.maxBioLength) {
      newErrors['bio'] = `Bio must be ${ProfileConstraints.maxBioLength} characters or less`;
    }

    if (nicheTags.length === 0) {
      newErrors['nicheTags'] = 'At least one niche tag is required';
    } else if (nicheTags.length > ProfileConstraints.maxNicheTags) {
      newErrors['nicheTags'] = `Maximum ${ProfileConstraints.maxNicheTags} tags allowed`;
    }

    if (
      openToCollaborations &&
      collaborationPreferences.length >
        ProfileConstraints.maxCollaborationPreferencesLength
    ) {
      newErrors['collaborationPreferences'] = `Preferences must be ${ProfileConstraints.maxCollaborationPreferencesLength} characters or less`;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [
    displayName,
    bio,
    nicheTags,
    openToCollaborations,
    collaborationPreferences,
  ]);

  // Check if form has changes
  const hasChanges = useCallback((): boolean => {
    return (
      displayName !== profile.displayName ||
      bio !== profile.bio ||
      JSON.stringify(nicheTags) !== JSON.stringify(profile.nicheTags) ||
      openToCollaborations !== profile.openToCollaborations ||
      collaborationPreferences !== (profile.collaborationPreferences ?? '')
    );
  }, [
    displayName,
    bio,
    nicheTags,
    openToCollaborations,
    collaborationPreferences,
    profile,
  ]);

  // Submit handler
  const handleSubmit = useCallback(
    async (event: React.FormEvent) => {
      event.preventDefault();

      if (!validate()) {
        return;
      }

      if (!hasChanges()) {
        return;
      }

      const updateData: UpdateProfileRequest = {
        displayName: displayName.trim(),
        bio: bio.trim(),
        nicheTags,
        openToCollaborations,
        collaborationPreferences: openToCollaborations
          ? collaborationPreferences.trim() || undefined
          : undefined,
      };

      try {
        await onUpdate(updateData);
      } catch {
        // Error handling is done by the parent
      }
    },
    [
      validate,
      hasChanges,
      displayName,
      bio,
      nicheTags,
      openToCollaborations,
      collaborationPreferences,
      onUpdate,
    ]
  );

  return (
    <form onSubmit={handleSubmit} className={cn('space-y-6', className)}>
      {/* Avatar Upload */}
      <Card>
        <CardHeader>
          <CardTitle>Profile Picture</CardTitle>
        </CardHeader>
        <CardContent>
          <AvatarUpload
            currentImageUrl={profile.profilePictureUrl}
            displayName={displayName}
            onUpload={onAvatarUpload}
            onRemove={onAvatarRemove}
            isUploading={isUploadingAvatar}
          />
        </CardContent>
      </Card>

      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Basic Information</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Display Name */}
          <div className="space-y-2">
            <Label htmlFor="displayName">
              Display Name <span className="text-destructive">*</span>
            </Label>
            <Input
              id="displayName"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              maxLength={ProfileConstraints.maxDisplayNameLength}
              placeholder="Your display name"
              aria-invalid={!!errors['displayName']}
            />
            {errors['displayName'] && (
              <p className="text-sm text-destructive">{errors['displayName']}</p>
            )}
            <p className="text-xs text-muted-foreground text-right">
              {displayName.length}/{ProfileConstraints.maxDisplayNameLength}
            </p>
          </div>

          {/* Username (read-only) */}
          <div className="space-y-2">
            <Label htmlFor="username">Username</Label>
            <Input
              id="username"
              type="text"
              value={profile.username}
              disabled
              className="bg-muted"
            />
            <p className="text-xs text-muted-foreground">
              Username cannot be changed
            </p>
          </div>

          <Separator />

          {/* Bio */}
          <div className="space-y-2">
            <Label htmlFor="bio">
              Bio <span className="text-destructive">*</span>
            </Label>
            <Textarea
              id="bio"
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              maxLength={ProfileConstraints.maxBioLength}
              placeholder="Tell others about yourself and your content..."
              rows={5}
              className="resize-none"
              aria-invalid={!!errors['bio']}
            />
            {errors['bio'] && (
              <p className="text-sm text-destructive">{errors['bio']}</p>
            )}
            <p className="text-xs text-muted-foreground text-right">
              {bio.length}/{ProfileConstraints.maxBioLength}
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Niche Tags */}
      <Card>
        <CardHeader>
          <CardTitle>Content Niches</CardTitle>
        </CardHeader>
        <CardContent>
          <NicheTagSelector
            selectedTags={nicheTags}
            onChange={setNicheTags}
          />
          {errors['nicheTags'] && (
            <p className="text-sm text-destructive mt-2">{errors['nicheTags']}</p>
          )}
        </CardContent>
      </Card>

      {/* Collaboration Settings */}
      <Card>
        <CardHeader>
          <CardTitle>Collaboration Settings</CardTitle>
        </CardHeader>
        <CardContent>
          <CollaborationToggle
            isOpen={openToCollaborations}
            preferences={collaborationPreferences}
            onToggle={setOpenToCollaborations}
            onPreferencesChange={setCollaborationPreferences}
          />
          {errors['collaborationPreferences'] && (
            <p className="text-sm text-destructive mt-2">
              {errors['collaborationPreferences']}
            </p>
          )}
        </CardContent>
      </Card>

      {/* Submit Button */}
      <div className="flex justify-end gap-4">
        <Button
          type="submit"
          disabled={isUpdating || !hasChanges()}
          isLoading={isUpdating}
        >
          {isUpdating ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Saving...
            </>
          ) : (
            <>
              <Save className="mr-2 h-4 w-4" />
              Save Changes
            </>
          )}
        </Button>
      </div>
    </form>
  );
}

export default ProfileEditForm;
