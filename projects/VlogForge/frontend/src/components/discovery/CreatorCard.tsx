'use client';

import Link from 'next/link';
import { Users } from 'lucide-react';
import {
  Card,
  CardContent,
  Avatar,
  AvatarImage,
  AvatarFallback,
  Badge,
} from '@/components/ui';
import { cn } from '@/lib/utils';
import type { DiscoveryCreatorDto } from '@/types/discovery';
import { PlatformConfig } from '@/types/discovery';
import { useDiscoveryHelpers } from '@/hooks/use-discovery';

interface CreatorCardProps {
  creator: DiscoveryCreatorDto;
  className?: string;
}

/**
 * Displays a creator in the discovery grid
 * Story: ACF-010
 */
export function CreatorCard({ creator, className }: CreatorCardProps) {
  const { formatFollowerCount } = useDiscoveryHelpers();

  const initials = creator.displayName
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  return (
    <Link href={`/profile/${creator.username}`}>
      <Card
        className={cn(
          'overflow-hidden hover:shadow-lg transition-shadow cursor-pointer h-full',
          className
        )}
      >
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            {/* Avatar */}
            <Avatar className="h-14 w-14 flex-shrink-0">
              <AvatarImage
                src={creator.profilePictureUrl}
                alt={creator.displayName}
              />
              <AvatarFallback className="text-sm">{initials}</AvatarFallback>
            </Avatar>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-sm truncate">
                {creator.displayName}
              </h3>
              <p className="text-xs text-muted-foreground truncate">
                @{creator.username}
              </p>
              <p className="text-xs text-muted-foreground mt-0.5">
                {formatFollowerCount(creator.totalFollowers)} followers
              </p>
            </div>

            {/* Collaboration badge */}
            {creator.openToCollaborations && (
              <Badge variant="success" className="flex-shrink-0 text-xs px-1.5 py-0.5">
                <Users className="mr-0.5 h-3 w-3" />
                Open
              </Badge>
            )}
          </div>

          {/* Bio */}
          {creator.bio && (
            <p className="text-xs text-muted-foreground mt-3 line-clamp-2">
              {creator.bio}
            </p>
          )}

          {/* Niche Tags */}
          {creator.nicheTags.length > 0 && (
            <div className="flex flex-wrap gap-1 mt-3">
              {creator.nicheTags.slice(0, 3).map((tag) => (
                <Badge key={tag} variant="secondary" className="text-xs px-1.5 py-0">
                  {tag}
                </Badge>
              ))}
              {creator.nicheTags.length > 3 && (
                <Badge variant="outline" className="text-xs px-1.5 py-0">
                  +{creator.nicheTags.length - 3}
                </Badge>
              )}
            </div>
          )}

          {/* Platforms */}
          {creator.platforms.length > 0 && (
            <div className="flex flex-wrap gap-1.5 mt-3">
              {creator.platforms.map((platform) => {
                const config = PlatformConfig[platform.platformType];
                return (
                  <div
                    key={platform.platformType}
                    className="flex items-center gap-1 text-xs text-muted-foreground"
                    title={`${platform.handle} - ${formatFollowerCount(platform.followerCount ?? 0)} followers`}
                  >
                    <div
                      className="h-2 w-2 rounded-full"
                      style={{ backgroundColor: config?.color ?? '#6B7280' }}
                    />
                    <span>{config?.label ?? platform.platformType}</span>
                  </div>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>
    </Link>
  );
}

export default CreatorCard;
