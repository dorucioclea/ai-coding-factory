/**
 * Unit tests for CollaborationRequestCard component
 * Story: ACF-011 - Collaboration Requests
 */

import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { CollaborationRequestCard } from '@/components/collaborations/CollaborationRequestCard';
import type { CollaborationRequestDto } from '@/types/collaboration';

const mockPendingRequest: CollaborationRequestDto = {
  id: 'req-1',
  senderId: 'user-1',
  recipientId: 'user-2',
  senderDisplayName: 'Alice Creator',
  senderUsername: 'alice_creator',
  senderProfilePictureUrl: 'https://example.com/alice.jpg',
  recipientDisplayName: 'Bob Vlogger',
  recipientUsername: 'bob_vlogger',
  recipientProfilePictureUrl: 'https://example.com/bob.jpg',
  message: 'Let us collaborate on a tech review video together!',
  status: 'Pending',
  expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
  createdAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(), // 2 hours ago
  isExpired: false,
};

const mockAcceptedRequest: CollaborationRequestDto = {
  ...mockPendingRequest,
  id: 'req-2',
  status: 'Accepted',
  respondedAt: new Date().toISOString(),
};

const mockDeclinedRequest: CollaborationRequestDto = {
  ...mockPendingRequest,
  id: 'req-3',
  status: 'Declined',
  declineReason: 'Schedule conflict',
  respondedAt: new Date().toISOString(),
};

const mockExpiredRequest: CollaborationRequestDto = {
  ...mockPendingRequest,
  id: 'req-4',
  isExpired: true,
  expiresAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
};

describe('CollaborationRequestCard', () => {
  describe('inbox view', () => {
    it('should render sender info in inbox mode', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText('Alice Creator')).toBeInTheDocument();
      expect(screen.getByText('@alice_creator')).toBeInTheDocument();
    });

    it('should render request message', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      expect(
        screen.getByText('Let us collaborate on a tech review video together!')
      ).toBeInTheDocument();
    });

    it('should render Pending status badge', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText('Pending')).toBeInTheDocument();
    });

    it('should show accept and decline buttons for pending inbox requests', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
        />
      );

      expect(screen.getByRole('button', { name: /accept/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /decline/i })).toBeInTheDocument();
    });

    it('should call onAccept when accept button is clicked', () => {
      const onAccept = vi.fn();
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
          onAccept={onAccept}
          onDecline={vi.fn()}
        />
      );

      fireEvent.click(screen.getByRole('button', { name: /accept/i }));
      expect(onAccept).toHaveBeenCalledWith('req-1');
    });

    it('should call onDecline when decline button is clicked', () => {
      const onDecline = vi.fn();
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={onDecline}
        />
      );

      fireEvent.click(screen.getByRole('button', { name: /decline/i }));
      expect(onDecline).toHaveBeenCalledWith('req-1');
    });

    it('should disable buttons while accepting', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
          isAccepting={true}
        />
      );

      expect(screen.getByRole('button', { name: /accepting/i })).toBeDisabled();
      expect(screen.getByRole('button', { name: /decline/i })).toBeDisabled();
    });

    it('should disable buttons while declining', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
          isDeclining={true}
        />
      );

      expect(screen.getByRole('button', { name: /accept/i })).toBeDisabled();
      expect(screen.getByRole('button', { name: /declining/i })).toBeDisabled();
    });

    it('should not show action buttons for accepted requests', () => {
      render(
        <CollaborationRequestCard
          request={mockAcceptedRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
        />
      );

      expect(screen.queryByRole('button', { name: /accept/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /decline/i })).not.toBeInTheDocument();
    });

    it('should not show action buttons for expired pending requests', () => {
      render(
        <CollaborationRequestCard
          request={mockExpiredRequest}
          viewMode="inbox"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
        />
      );

      expect(screen.queryByRole('button', { name: /accept/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /decline/i })).not.toBeInTheDocument();
    });

    it('should show expired label for expired pending requests', () => {
      render(
        <CollaborationRequestCard
          request={mockExpiredRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText('Expired')).toBeInTheDocument();
    });
  });

  describe('sent view', () => {
    it('should render recipient info in sent mode', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="sent"
        />
      );

      expect(screen.getByText('Bob Vlogger')).toBeInTheDocument();
      expect(screen.getByText('@bob_vlogger')).toBeInTheDocument();
    });

    it('should not show action buttons in sent mode', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="sent"
          onAccept={vi.fn()}
          onDecline={vi.fn()}
        />
      );

      expect(screen.queryByRole('button', { name: /accept/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /decline/i })).not.toBeInTheDocument();
    });
  });

  describe('status display', () => {
    it('should show Accepted badge for accepted requests', () => {
      render(
        <CollaborationRequestCard
          request={mockAcceptedRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText('Accepted')).toBeInTheDocument();
    });

    it('should show Declined badge for declined requests', () => {
      render(
        <CollaborationRequestCard
          request={mockDeclinedRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText('Declined')).toBeInTheDocument();
    });

    it('should show decline reason when present', () => {
      render(
        <CollaborationRequestCard
          request={mockDeclinedRequest}
          viewMode="inbox"
        />
      );

      expect(screen.getByText(/Schedule conflict/)).toBeInTheDocument();
    });

    it('should not show decline reason when not present', () => {
      render(
        <CollaborationRequestCard
          request={mockAcceptedRequest}
          viewMode="inbox"
        />
      );

      expect(screen.queryByText(/Reason:/)).not.toBeInTheDocument();
    });
  });

  describe('avatar display', () => {
    it('should render initials from display name', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      // "Alice Creator" -> "AC"
      expect(screen.getByText('AC')).toBeInTheDocument();
    });

    it('should render initials for sent view from recipient name', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="sent"
        />
      );

      // "Bob Vlogger" -> "BV"
      expect(screen.getByText('BV')).toBeInTheDocument();
    });
  });

  describe('time display', () => {
    it('should show time ago for creation date', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      // Created 2 hours ago - should show "2h ago"
      expect(screen.getByText('2h ago')).toBeInTheDocument();
    });

    it('should show expiration info for pending non-expired requests', () => {
      render(
        <CollaborationRequestCard
          request={mockPendingRequest}
          viewMode="inbox"
        />
      );

      // Expires in 7 days
      expect(screen.getByText(/in \d+ days/)).toBeInTheDocument();
    });
  });
});
