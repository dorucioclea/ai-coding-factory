/**
 * Unit tests for ApprovalActions component
 * Story: ACF-009 - Approval Workflows
 */

import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ApprovalActions } from '@/components/approval';
import { IdeaStatus } from '@/types/content';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useApproval: vi.fn(),
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

import { useApproval } from '@/hooks';

const mockSubmitForApproval = vi.fn();
const mockApproveContent = vi.fn();
const mockRequestChanges = vi.fn();

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

const baseContent = {
  id: 'content-1',
  userId: 'user-1',
  title: 'Test Video',
  notes: 'Test notes',
  status: IdeaStatus.Draft,
  platformTags: [],
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

describe('ApprovalActions', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useApproval).mockReturnValue({
      submitForApproval: mockSubmitForApproval,
      approveContent: mockApproveContent,
      requestChanges: mockRequestChanges,
      configureWorkflow: vi.fn(),
      isSubmitting: false,
      isApproving: false,
      isRequestingChanges: false,
      isConfiguringWorkflow: false,
      submitError: null,
      approveError: null,
      requestChangesError: null,
      configureError: null,
    });
  });

  describe('Submit for Approval', () => {
    it('should show submit button for content owner with Draft status', () => {
      render(
        <ApprovalActions
          content={baseContent}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Submit for Approval')).toBeInTheDocument();
    });

    it('should show submit button for ChangesRequested status', () => {
      const content = { ...baseContent, status: IdeaStatus.ChangesRequested };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Submit for Approval')).toBeInTheDocument();
    });

    it('should not show submit button for non-owner', () => {
      render(
        <ApprovalActions
          content={baseContent}
          teamId="team-1"
          canApprove={false}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.queryByText('Submit for Approval')).not.toBeInTheDocument();
    });

    it('should not show submit button for InReview status', () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.queryByText('Submit for Approval')).not.toBeInTheDocument();
    });

    it('should call submitForApproval when submit button is clicked', async () => {
      mockSubmitForApproval.mockResolvedValue({});

      render(
        <ApprovalActions
          content={baseContent}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      fireEvent.click(screen.getByText('Submit for Approval'));

      await waitFor(() => {
        expect(mockSubmitForApproval).toHaveBeenCalledWith('content-1', {
          teamId: 'team-1',
        });
      });
    });
  });

  describe('Approve Content', () => {
    it('should show approve button for approver with InReview status', () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={true}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Approve')).toBeInTheDocument();
    });

    it('should not show approve button for non-approver', () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={false}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.queryByText('Approve')).not.toBeInTheDocument();
    });

    it('should open approve dialog when approve button is clicked', async () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={true}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      fireEvent.click(screen.getByText('Approve'));

      expect(screen.getByText('Approve Content')).toBeInTheDocument();
    });

    it('should call approveContent when dialog is submitted', async () => {
      const user = userEvent.setup();
      mockApproveContent.mockResolvedValue({});
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={true}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      // Open dialog
      fireEvent.click(screen.getByText('Approve'));

      // Click approve in dialog
      const approveButtons = screen.getAllByText('Approve');
      const lastApproveButton = approveButtons[approveButtons.length - 1];
      expect(lastApproveButton).toBeDefined();
      fireEvent.click(lastApproveButton!); // Click the one in dialog

      await waitFor(() => {
        expect(mockApproveContent).toHaveBeenCalledWith('content-1', {
          teamId: 'team-1',
          feedback: undefined,
        });
      });
    });
  });

  describe('Request Changes', () => {
    it('should show request changes button for approver with InReview status', () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={true}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Request Changes')).toBeInTheDocument();
    });

    it('should open request changes dialog when button is clicked', async () => {
      const content = { ...baseContent, status: IdeaStatus.InReview };

      render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={true}
          isOwner={false}
        />,
        { wrapper: createWrapper() }
      );

      // Click the Request Changes button
      fireEvent.click(screen.getByRole('button', { name: /Request Changes/i }));

      // Verify dialog content is visible (using partial match since text includes more)
      await waitFor(() => {
        expect(screen.getByText(/Please explain what changes are needed/i)).toBeInTheDocument();
      });
    });
  });

  describe('Loading States', () => {
    it('should show submitting state', () => {
      vi.mocked(useApproval).mockReturnValue({
        submitForApproval: mockSubmitForApproval,
        approveContent: mockApproveContent,
        requestChanges: mockRequestChanges,
        configureWorkflow: vi.fn(),
        isSubmitting: true,
        isApproving: false,
        isRequestingChanges: false,
        isConfiguringWorkflow: false,
        submitError: null,
        approveError: null,
        requestChangesError: null,
        configureError: null,
      });

      render(
        <ApprovalActions
          content={baseContent}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Submitting...')).toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    it('should return null when user cannot submit or review', () => {
      const content = { ...baseContent, status: IdeaStatus.Published };

      const { container } = render(
        <ApprovalActions
          content={content}
          teamId="team-1"
          canApprove={false}
          isOwner={true}
        />,
        { wrapper: createWrapper() }
      );

      expect(container.firstChild).toBeNull();
    });
  });
});
