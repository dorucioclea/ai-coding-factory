/**
 * Messaging types
 * Story: ACF-012
 */

/**
 * Conversation response from API
 */
export interface ConversationDto {
  id: string;
  participantId: string;
  participantDisplayName: string;
  participantUsername: string;
  participantProfilePictureUrl?: string;
  lastMessagePreview?: string;
  lastMessageAt?: string;
  unreadCount: number;
  createdAt: string;
}

/**
 * Paginated list of conversations
 */
export interface ConversationListResponse {
  items: ConversationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Message response from API
 */
export interface MessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderDisplayName: string;
  senderUsername: string;
  content: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

/**
 * Paginated list of messages
 */
export interface MessageListResponse {
  items: MessageDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Payload for sending a message
 */
export interface SendMessagePayload {
  content: string;
}

/**
 * Payload for starting a new conversation
 */
export interface StartConversationPayload {
  participantId: string;
}
