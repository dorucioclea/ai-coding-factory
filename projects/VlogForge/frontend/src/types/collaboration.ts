/**
 * Collaboration request types
 * Story: ACF-011
 */

/**
 * Status of a collaboration request
 */
export type CollaborationRequestStatus =
  | 'Pending'
  | 'Accepted'
  | 'Declined'
  | 'Withdrawn'
  | 'Expired';

/**
 * Collaboration request response from API
 */
export interface CollaborationRequestDto {
  id: string;
  senderId: string;
  recipientId: string;
  senderDisplayName: string;
  senderUsername: string;
  senderProfilePictureUrl?: string;
  recipientDisplayName: string;
  recipientUsername: string;
  recipientProfilePictureUrl?: string;
  message: string;
  status: CollaborationRequestStatus;
  expiresAt: string;
  respondedAt?: string;
  declineReason?: string;
  createdAt: string;
  isExpired: boolean;
}

/**
 * Paginated list of collaboration requests
 */
export interface CollaborationRequestListResponse {
  items: CollaborationRequestDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Request to send a collaboration request
 */
export interface SendCollaborationRequestPayload {
  recipientId: string;
  message: string;
}

/**
 * Request to decline a collaboration request
 */
export interface DeclineCollaborationRequestPayload {
  reason?: string;
}

/**
 * Status badge display config
 */
export const CollaborationStatusConfig: Record<
  CollaborationRequestStatus,
  { label: string; variant: 'default' | 'secondary' | 'success' | 'destructive' | 'outline' }
> = {
  Pending: { label: 'Pending', variant: 'default' },
  Accepted: { label: 'Accepted', variant: 'success' },
  Declined: { label: 'Declined', variant: 'destructive' },
  Withdrawn: { label: 'Withdrawn', variant: 'secondary' },
  Expired: { label: 'Expired', variant: 'outline' },
};
