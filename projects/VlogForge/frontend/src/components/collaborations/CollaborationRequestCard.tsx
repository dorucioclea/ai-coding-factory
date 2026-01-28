'use client';

import { Clock, Check, X, MessageSquare } from 'lucide-react';
import {
  Card,
  CardContent,
  Avatar,
  AvatarImage,
  AvatarFallback,
  Badge,
  Button,
} from '@/components/ui';
import type { CollaborationRequestDto } from '@/types/collaboration';
import { CollaborationStatusConfig } from '@/types/collaboration';

interface CollaborationRequestCardProps {
  request: CollaborationRequestDto;
  viewMode: 'inbox' | 'sent';
  onAccept?: (id: string) => void;
  onDecline?: (id: string) => void;
  isAccepting?: boolean;
  isDeclining?: boolean;
}

/**
 * Displays a collaboration request in the inbox or sent list
 * Story: ACF-011
 */
export function CollaborationRequestCard({
  request,
  viewMode,
  onAccept,
  onDecline,
  isAccepting = false,
  isDeclining = false,
}: CollaborationRequestCardProps) {
  const isInbox = viewMode === 'inbox';
  const otherUser = isInbox
    ? {
        displayName: request.senderDisplayName,
        username: request.senderUsername,
        profilePictureUrl: request.senderProfilePictureUrl,
      }
    : {
        displayName: request.recipientDisplayName,
        username: request.recipientUsername,
        profilePictureUrl: request.recipientProfilePictureUrl,
      };

  const initials = otherUser.displayName
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  const statusConfig = CollaborationStatusConfig[request.status];
  const canRespond = isInbox && request.status === 'Pending' && !request.isExpired;
  const timeAgo = formatTimeAgo(request.createdAt);

  return (
    <Card className="overflow-hidden">
      <CardContent className="p-4">
        <div className="flex items-start gap-3">
          {/* Avatar */}
          <Avatar className="h-12 w-12 flex-shrink-0">
            <AvatarImage src={otherUser.profilePictureUrl} alt={otherUser.displayName} />
            <AvatarFallback className="text-sm">{initials}</AvatarFallback>
          </Avatar>

          {/* Info */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              <h3 className="font-semibold text-sm">{otherUser.displayName}</h3>
              <span className="text-xs text-muted-foreground">@{otherUser.username}</span>
              <Badge variant={statusConfig.variant} className="text-xs">
                {statusConfig.label}
              </Badge>
            </div>

            {/* Message */}
            <div className="flex items-start gap-1.5 mt-2">
              <MessageSquare className="h-3.5 w-3.5 text-muted-foreground mt-0.5 flex-shrink-0" />
              <p className="text-sm text-muted-foreground line-clamp-2">{request.message}</p>
            </div>

            {/* Decline reason */}
            {request.declineReason && (
              <p className="text-xs text-destructive mt-1 italic">
                Reason: {request.declineReason}
              </p>
            )}

            {/* Meta */}
            <div className="flex items-center gap-3 mt-2 text-xs text-muted-foreground">
              <span className="flex items-center gap-1">
                <Clock className="h-3 w-3" />
                {timeAgo}
              </span>
              {request.status === 'Pending' && !request.isExpired && (
                <span>
                  Expires {formatTimeUntil(request.expiresAt)}
                </span>
              )}
              {request.isExpired && request.status === 'Pending' && (
                <span className="text-destructive">Expired</span>
              )}
            </div>
          </div>
        </div>

        {/* Actions */}
        {canRespond && (
          <div className="flex gap-2 mt-3 justify-end">
            <Button
              variant="outline"
              size="sm"
              onClick={() => onDecline?.(request.id)}
              disabled={isDeclining || isAccepting}
            >
              <X className="h-3.5 w-3.5 mr-1" />
              {isDeclining ? 'Declining...' : 'Decline'}
            </Button>
            <Button
              size="sm"
              onClick={() => onAccept?.(request.id)}
              disabled={isAccepting || isDeclining}
            >
              <Check className="h-3.5 w-3.5 mr-1" />
              {isAccepting ? 'Accepting...' : 'Accept'}
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function formatTimeAgo(dateString: string): string {
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return 'Unknown';
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString();
}

function formatTimeUntil(dateString: string): string {
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return 'unknown';
  const now = new Date();
  const diffMs = date.getTime() - now.getTime();
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

  if (diffDays <= 0) return 'today';
  if (diffDays === 1) return 'tomorrow';
  return `in ${diffDays} days`;
}

export default CollaborationRequestCard;
