'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import { ArrowLeft, Send, Check } from 'lucide-react';
import { Button, Skeleton } from '@/components/ui';
import { useMessages, useSendMessage, useMarkAsRead } from '@/hooks/use-messaging';
import { useUser } from '@/hooks/use-auth';
import { useRouter, useParams } from 'next/navigation';
import type { MessageDto } from '@/types/messaging';

const MAX_MESSAGE_LENGTH = 2000;
const MAX_ACCUMULATED_MESSAGES = 500;

/**
 * Format a date as HH:MM
 */
function formatTime(dateStr: string): string {
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return '';
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

/**
 * Message thread page
 * Story: ACF-012
 */
export default function MessageThreadPage() {
  const router = useRouter();
  const params = useParams();
  const rawId = params['id'];
  const conversationId = Array.isArray(rawId) ? rawId[0] : rawId;

  const [page, setPage] = useState(1);
  const [messageContent, setMessageContent] = useState('');
  const [allMessages, setAllMessages] = useState<MessageDto[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messageAreaRef = useRef<HTMLDivElement>(null);
  const hasMarkedRead = useRef(false);

  const { data: currentUser } = useUser();
  const currentUserId = currentUser?.id ?? null;

  const messagesQuery = useMessages(conversationId ?? '', page, 50, !!conversationId);
  const sendMessageMutation = useSendMessage(conversationId ?? '');
  const markAsReadMutation = useMarkAsRead(conversationId ?? '');

  // Accumulate messages from all loaded pages, capped to prevent unbounded growth
  useEffect(() => {
    if (messagesQuery.data?.items) {
      if (page === 1) {
        setAllMessages(messagesQuery.data.items);
      } else {
        setAllMessages((prev) => {
          const existingIds = new Set(prev.map((m) => m.id));
          const newMessages = messagesQuery.data.items.filter(
            (m) => !existingIds.has(m.id)
          );
          if (newMessages.length === 0) return prev;
          const merged = [...newMessages, ...prev];
          // Keep only the most recent messages to prevent memory growth
          return merged.length > MAX_ACCUMULATED_MESSAGES
            ? merged.slice(merged.length - MAX_ACCUMULATED_MESSAGES)
            : merged;
        });
      }
    }
  }, [messagesQuery.data?.items, page]);

  // Reset mark-as-read tracking when conversation changes
  useEffect(() => {
    hasMarkedRead.current = false;
  }, [conversationId]);

  // Mark conversation as read on mount and conversation change
  useEffect(() => {
    if (conversationId && !hasMarkedRead.current) {
      hasMarkedRead.current = true;
      markAsReadMutation.mutate();
    }
  }, [conversationId, markAsReadMutation]);

  // Auto-scroll to bottom when new messages arrive on page 1
  useEffect(() => {
    if (page === 1 && messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [allMessages, page]);

  const handleSendMessage = useCallback(() => {
    const trimmed = messageContent.trim();
    if (!trimmed || trimmed.length > MAX_MESSAGE_LENGTH) return;

    sendMessageMutation.mutate(
      { content: trimmed },
      {
        onSuccess: () => {
          setMessageContent('');
          // Reset to page 1 to see the latest message
          setPage(1);
        },
      }
    );
  }, [messageContent, sendMessageMutation]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        handleSendMessage();
      }
    },
    [handleSendMessage]
  );

  const handleLoadMore = useCallback(() => {
    setPage((p) => p + 1);
  }, []);

  const handleBack = useCallback(() => {
    router.push('/dashboard/messages');
  }, [router]);

  if (!conversationId) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-4rem)]">
        <p className="text-muted-foreground">Invalid conversation</p>
      </div>
    );
  }

  // Determine participant name from first received message
  const participantName =
    allMessages.find((m) => m.senderId !== currentUserId)?.senderDisplayName ??
    'Conversation';

  const hasOlderMessages = messagesQuery.data?.hasNextPage ?? false;

  return (
    <div className="flex flex-col h-[calc(100vh-4rem)]">
      {/* Header */}
      <div className="flex items-center gap-3 px-4 py-3 border-b bg-background">
        <Button
          variant="ghost"
          size="sm"
          onClick={handleBack}
          className="p-1"
          aria-label="Go back to conversations"
        >
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-semibold text-xs">
            {participantName.charAt(0).toUpperCase()}
          </div>
          <h1 className="text-lg font-semibold">{participantName}</h1>
        </div>
      </div>

      {/* Message Area */}
      <div
        ref={messageAreaRef}
        className="flex-1 overflow-y-auto px-4 py-4 space-y-3"
        role="log"
        aria-label="Message history"
      >
        {/* Loading State */}
        {messagesQuery.isLoading && page === 1 ? (
          <div className="space-y-3">
            {[1, 2, 3, 4, 5].map((i) => (
              <div
                key={i}
                className={`flex ${i % 2 === 0 ? 'justify-end' : 'justify-start'}`}
              >
                <Skeleton className="h-10 w-48 rounded-lg" />
              </div>
            ))}
          </div>
        ) : (
          <>
            {/* Load More Button */}
            {hasOlderMessages && (
              <div className="flex justify-center pb-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleLoadMore}
                  disabled={messagesQuery.isFetching}
                >
                  {messagesQuery.isFetching ? 'Loading...' : 'Load older messages'}
                </Button>
              </div>
            )}

            {/* Messages */}
            {allMessages.length === 0 && !messagesQuery.isLoading ? (
              <div className="flex flex-col items-center justify-center py-12 text-center">
                <p className="text-muted-foreground">
                  No messages yet. Send the first message to start the conversation.
                </p>
              </div>
            ) : (
              allMessages.map((message) => {
                const isSent = message.senderId === currentUserId;

                return (
                  <div
                    key={message.id}
                    className={`flex ${isSent ? 'justify-end' : 'justify-start'}`}
                  >
                    <div
                      className={`max-w-[70%] rounded-lg px-3 py-2 ${
                        isSent
                          ? 'bg-primary text-primary-foreground'
                          : 'bg-muted'
                      }`}
                    >
                      <p className="text-sm whitespace-pre-wrap break-words">
                        {message.content}
                      </p>
                      <div
                        className={`flex items-center gap-1 mt-1 ${
                          isSent ? 'justify-end' : 'justify-start'
                        }`}
                      >
                        <span
                          className={`text-[10px] ${
                            isSent
                              ? 'text-primary-foreground/70'
                              : 'text-muted-foreground'
                          }`}
                        >
                          {formatTime(message.createdAt)}
                        </span>
                        {isSent && message.isRead && (
                          <span
                            className="text-primary-foreground/70 flex items-center"
                            title={
                              message.readAt
                                ? `Read at ${formatTime(message.readAt)}`
                                : 'Read'
                            }
                          >
                            <Check className="h-3 w-3" />
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })
            )}

            {/* Scroll anchor */}
            <div ref={messagesEndRef} />
          </>
        )}
      </div>

      {/* Error State */}
      {messagesQuery.error && (
        <div className="px-4 py-2 bg-destructive/15 text-destructive text-sm" role="alert">
          {messagesQuery.error instanceof Error
            ? messagesQuery.error.message
            : 'Failed to load messages'}
        </div>
      )}

      {/* Message Input */}
      <div className="border-t bg-background px-4 py-3">
        <div className="flex items-center gap-2">
          <input
            type="text"
            value={messageContent}
            onChange={(e) => setMessageContent(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Type a message..."
            maxLength={MAX_MESSAGE_LENGTH}
            aria-label="Message content"
            className="flex-1 rounded-lg border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            disabled={sendMessageMutation.isPending}
          />
          <Button
            size="sm"
            onClick={handleSendMessage}
            disabled={!messageContent.trim() || sendMessageMutation.isPending}
            aria-label="Send message"
          >
            <Send className="h-4 w-4" />
          </Button>
        </div>
        {sendMessageMutation.error && (
          <p className="text-xs text-destructive mt-1" role="alert">
            {sendMessageMutation.error instanceof Error
              ? sendMessageMutation.error.message
              : 'Failed to send message'}
          </p>
        )}
      </div>
    </div>
  );
}
