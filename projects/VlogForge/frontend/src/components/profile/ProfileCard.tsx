'use client';

import { Calendar, Users } from 'lucide-react';
import {
  Card,
  CardContent,
  CardHeader,
  Avatar,
  AvatarImage,
  AvatarFallback,
  Badge,
  Separator,
} from '@/components/ui';
import type { PublicProfileResponse, CreatorProfileResponse } from '@/types';
import { cn } from '@/lib/utils';

interface ProfileCardProps {
  profile: PublicProfileResponse | CreatorProfileResponse;
  showEmail?: boolean;
  className?: string;
}

/**
 * Displays creator profile information in a card
 */
export function ProfileCard({
  profile,
  showEmail: _showEmail = false,
  className,
}: ProfileCardProps) {
  const isFullProfile = 'userId' in profile;
  const fullProfile = isFullProfile
    ? (profile as CreatorProfileResponse)
    : null;

  // Generate initials for avatar fallback
  const initials = profile.displayName
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  return (
    <Card className={cn('overflow-hidden', className)}>
      <CardHeader className="pb-4">
        <div className="flex items-start gap-4">
          {/* Avatar */}
          <Avatar className="h-20 w-20">
            <AvatarImage
              src={profile.profilePictureUrl}
              alt={profile.displayName}
            />
            <AvatarFallback className="text-lg">{initials}</AvatarFallback>
          </Avatar>

          {/* Name and username */}
          <div className="flex-1 space-y-1">
            <h2 className="text-2xl font-bold">{profile.displayName}</h2>
            <p className="text-sm text-muted-foreground">@{profile.username}</p>

            {/* Collaboration status */}
            {profile.openToCollaborations && (
              <Badge variant="success" className="mt-2">
                <Users className="mr-1 h-3 w-3" />
                Open to Collaborations
              </Badge>
            )}
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Bio */}
        {profile.bio && (
          <div>
            <h3 className="text-sm font-semibold mb-1">About</h3>
            <p className="text-sm text-muted-foreground whitespace-pre-wrap">
              {profile.bio}
            </p>
          </div>
        )}

        {/* Niche Tags */}
        {profile.nicheTags.length > 0 && (
          <div>
            <h3 className="text-sm font-semibold mb-2">Niches</h3>
            <div className="flex flex-wrap gap-2">
              {profile.nicheTags.map((tag) => (
                <Badge key={tag} variant="secondary">
                  {tag}
                </Badge>
              ))}
            </div>
          </div>
        )}

        {/* Collaboration Preferences (full profile only) */}
        {fullProfile?.collaborationPreferences && (
          <div>
            <h3 className="text-sm font-semibold mb-1">
              Collaboration Preferences
            </h3>
            <p className="text-sm text-muted-foreground whitespace-pre-wrap">
              {fullProfile.collaborationPreferences}
            </p>
          </div>
        )}

        <Separator />

        {/* Connected Platforms */}
        <div>
          <h3 className="text-sm font-semibold mb-2">Connected Platforms</h3>
          <div className="grid grid-cols-2 gap-2">
            {profile.connectedPlatforms.map((platform) => (
              <div
                key={platform.platform}
                className={cn(
                  'flex items-center gap-2 rounded-lg border p-2',
                  platform.isConnected
                    ? 'border-success bg-success/10'
                    : 'border-muted bg-muted/10'
                )}
              >
                <div
                  className={cn(
                    'h-2 w-2 rounded-full',
                    platform.isConnected ? 'bg-success' : 'bg-muted-foreground'
                  )}
                />
                <span className="text-sm font-medium">{platform.platform}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Metadata (full profile only) */}
        {fullProfile && (
          <div className="text-xs text-muted-foreground space-y-1 pt-2">
            <div className="flex items-center gap-2">
              <Calendar className="h-3 w-3" />
              <span>
                Joined{' '}
                {new Date(fullProfile.createdAt).toLocaleDateString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}
              </span>
            </div>
            {fullProfile.updatedAt && (
              <div className="flex items-center gap-2">
                <Calendar className="h-3 w-3" />
                <span>
                  Last updated{' '}
                  {new Date(fullProfile.updatedAt).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </span>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default ProfileCard;
