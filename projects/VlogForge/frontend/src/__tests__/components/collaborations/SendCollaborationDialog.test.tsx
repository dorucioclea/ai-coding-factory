/**
 * Unit tests for SendCollaborationDialog component
 * Story: ACF-011 - Collaboration Requests
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../../utils/test-utils';
import { SendCollaborationDialog } from '@/components/collaborations/SendCollaborationDialog';
import type { DiscoveryCreatorDto } from '@/types/discovery';

// Mock the collaborations hook
const mockMutateAsync = vi.fn();
vi.mock('@/hooks/use-collaborations', () => ({
  useSendCollaborationRequest: () => ({
    mutateAsync: mockMutateAsync,
    isPending: false,
    error: null,
  }),
}));

const mockCreator: DiscoveryCreatorDto = {
  id: 'creator-1',
  username: 'testcreator',
  displayName: 'Test Creator',
  bio: 'A test creator',
  profilePictureUrl: 'https://example.com/avatar.jpg',
  openToCollaborations: true,
  nicheTags: ['gaming'],
  platforms: [],
  totalFollowers: 50000,
};

describe('SendCollaborationDialog', () => {
  const defaultProps = {
    creator: mockCreator,
    isOpen: true,
    onClose: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockMutateAsync.mockResolvedValue({});
  });

  it('should not render when isOpen is false', () => {
    render(
      <SendCollaborationDialog {...defaultProps} isOpen={false} />,
      { wrapper: createWrapper() }
    );

    expect(screen.queryByText('Request Collaboration')).not.toBeInTheDocument();
  });

  it('should render dialog when isOpen is true', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('Request Collaboration')).toBeInTheDocument();
  });

  it('should display creator name in dialog text', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('Test Creator')).toBeInTheDocument();
  });

  it('should have dialog accessibility attributes', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const dialog = screen.getByRole('dialog');
    expect(dialog).toHaveAttribute('aria-modal', 'true');
    expect(dialog).toHaveAttribute('aria-labelledby', 'collab-dialog-title');
  });

  it('should show character count', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('1000 characters remaining')).toBeInTheDocument();
  });

  it('should update character count when typing', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    fireEvent.change(textarea, { target: { value: 'Hello world' } });

    expect(screen.getByText('989 characters remaining')).toBeInTheDocument();
  });

  it('should disable submit button when message is empty', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const submitButton = screen.getByRole('button', { name: /send request/i });
    expect(submitButton).toBeDisabled();
  });

  it('should enable submit button when message is provided', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    fireEvent.change(textarea, { target: { value: 'Let us collaborate!' } });

    const submitButton = screen.getByRole('button', { name: /send request/i });
    expect(submitButton).not.toBeDisabled();
  });

  it('should call onClose when Cancel is clicked', () => {
    const onClose = vi.fn();
    render(
      <SendCollaborationDialog {...defaultProps} onClose={onClose} />,
      { wrapper: createWrapper() }
    );

    fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('should call onClose when backdrop is clicked', () => {
    const onClose = vi.fn();
    render(
      <SendCollaborationDialog {...defaultProps} onClose={onClose} />,
      { wrapper: createWrapper() }
    );

    // Click the backdrop (role="presentation")
    const backdrop = screen.getByRole('presentation');
    fireEvent.click(backdrop);
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('should close dialog on Escape key', () => {
    const onClose = vi.fn();
    render(
      <SendCollaborationDialog {...defaultProps} onClose={onClose} />,
      { wrapper: createWrapper() }
    );

    const dialog = screen.getByRole('dialog');
    fireEvent.keyDown(dialog, { key: 'Escape' });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('should submit form with correct data', async () => {
    const onClose = vi.fn();
    render(
      <SendCollaborationDialog {...defaultProps} onClose={onClose} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    fireEvent.change(textarea, { target: { value: 'Let us make a video together!' } });

    const submitButton = screen.getByRole('button', { name: /send request/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith({
        recipientId: 'creator-1',
        message: 'Let us make a video together!',
      });
    });
  });

  it('should call onClose after successful submission', async () => {
    const onClose = vi.fn();
    render(
      <SendCollaborationDialog {...defaultProps} onClose={onClose} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    fireEvent.change(textarea, { target: { value: 'Collab proposal' } });

    fireEvent.click(screen.getByRole('button', { name: /send request/i }));

    await waitFor(() => {
      expect(onClose).toHaveBeenCalledOnce();
    });
  });

  it('should have required attribute on textarea', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    expect(textarea).toBeRequired();
  });

  it('should have maxLength attribute matching MAX_MESSAGE_LENGTH', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    const textarea = screen.getByPlaceholderText(/describe your collaboration idea/i);
    expect(textarea).toHaveAttribute('maxLength', '1000');
  });

  it('should have proper label for textarea', () => {
    render(
      <SendCollaborationDialog {...defaultProps} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByLabelText(/your proposal/i)).toBeInTheDocument();
  });
});
