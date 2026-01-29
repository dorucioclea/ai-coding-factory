'use client';

import { useState, useCallback } from 'react';
import { MessageSquare } from 'lucide-react';
import { Button, Skeleton } from '@/components/ui';
import { UnreadBadge } from '@/components/messaging/UnreadBadge';
import { useConversations, useUnreadCount } from '@/hooks/use-messaging';
import { useRouter } from 'next/navigation';
import type { ConversationDto } from '@/types/messaging';

/**
 * Format a date string as relative time ago
 */
function formatTimeAgo(dateStr?: string): string {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return '';
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${minutes}m`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h`;
  const days = Math.floor(hours / 24);
  return `${days}d`;
}

/**
 * Get the initial letter for the avatar circle
 */
function getInitial(name: string): string {
  return name.charAt(0).toUpperCase();
}

/**
 * Messages page - conversation list
 * Story: ACF-012
 */
export default function MessagesPage() {
  const [page, setPage] = useState(1);
  const router = useRouter();

  const conversationsQuery = useConversations(page, 20);
  const unreadCountQuery = useUnreadCount();

  const conversations = conversationsQuery.data?.items ?? [];
  const hasNextPage = conversationsQuery.data?.hasNextPage ?? false;
  const hasPreviousPage = conversationsQuery.data?.hasPreviousPage ?? false;
  const totalUnread = unreadCountQuery.data?.unreadCount ?? 0;

  const handleConversationClick = useCallback(
    (conversation: ConversationDto) => {
      router.push(`/dashboard/messages/${conversation.id}`);
    },
    [router]
  );

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2">
        <MessageSquare className="h-6 w-6" />
        <h1 className="text-2xl font-bold">Messages</h1>
        {totalUnread > 0 && (
          <UnreadBadge count={totalUnread} />
        )}
      </div>

      {/* Error State */}
      {conversationsQuery.error && (
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading conversations</h3>
          <p className="text-sm mt-1">
            {conversationsQuery.error instanceof Error
              ? conversationsQuery.error.message
              : 'An unexpected error occurred'}
          </p>
        </div>
      )}

      {/* Loading State */}
      {conversationsQuery.isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center gap-3 p-4 rounded-lg border">
              <Skeleton className="h-10 w-10 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-3 w-48" />
              </div>
              <Skeleton className="h-3 w-8" />
            </div>
          ))}
        </div>
      ) : conversations.length > 0 ? (
        <>
          {/* Conversation List */}
          <div className="space-y-2">
            {conversations.map((conversation) => (
              <button
                key={conversation.id}
                type="button"
                className="w-full flex items-center gap-3 p-4 rounded-lg border hover:bg-accent/50 transition-colors text-left cursor-pointer"
                onClick={() => handleConversationClick(conversation)}
              >
                {/* Avatar Initial Circle */}
                <div className="flex-shrink-0 h-10 w-10 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-semibold text-sm">
                  {getInitial(conversation.participantDisplayName)}
                </div>

                {/* Content */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span
                      className={`text-sm truncate ${
                        conversation.unreadCount > 0
                          ? 'font-bold'
                          : 'font-medium'
                      }`}
                    >
                      {conversation.participantDisplayName}
                    </span>
                  </div>
                  {conversation.lastMessagePreview && (
                    <p className="text-sm text-muted-foreground truncate mt-0.5">
                      {conversation.lastMessagePreview}
                    </p>
                  )}
                </div>

                {/* Right: Time + Unread */}
                <div className="flex-shrink-0 flex flex-col items-end gap-1">
                  <span className="text-xs text-muted-foreground">
                    {formatTimeAgo(conversation.lastMessageAt)}
                  </span>
                  {conversation.unreadCount > 0 && (
                    <UnreadBadge count={conversation.unreadCount} />
                  )}
                </div>
              </button>
            ))}
          </div>

          {/* Pagination */}
          {(hasNextPage || hasPreviousPage) && (
            <div className="flex justify-center gap-2 pt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={!hasPreviousPage}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </Button>
              <span className="flex items-center text-sm text-muted-foreground px-2">
                Page {page}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={!hasNextPage}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      ) : (
        /* Empty State */
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="rounded-full bg-muted p-6 mb-4">
            <MessageSquare className="h-12 w-12 text-muted-foreground" />
          </div>
          <h2 className="text-xl font-semibold mb-2">No conversations yet</h2>
          <p className="text-muted-foreground max-w-md">
            Start a conversation with a creator from the Discover page or from a
            collaboration request.
          </p>
        </div>
      )}
    </div>
  );
}
