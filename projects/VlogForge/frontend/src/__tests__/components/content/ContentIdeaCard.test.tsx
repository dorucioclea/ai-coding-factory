/**
 * Unit tests for ContentIdeaCard component
 * ACF-015 Phase 3 - Content Ideas
 */

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { createWrapper } from '../../utils/test-utils';
import { ContentIdeaCard } from '@/components/content/ContentIdeaCard';
import { IdeaStatus } from '@/types';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useUpdateIdeaStatus: () => ({
    mutateAsync: vi.fn().mockResolvedValue({}),
    isPending: false,
  }),
  useAuth: () => ({
    user: { id: 'user-1', email: 'test@example.com' },
    isLoading: false,
    isAuthenticated: true,
  }),
}));

const mockContentIdea = {
  id: 'content-1',
  userId: 'user-1',
  title: 'Test Content Idea',
  notes: 'This is a test note for the content idea',
  status: IdeaStatus.Idea,
  platformTags: ['YouTube', 'TikTok'],
  scheduledDate: undefined,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-15T00:00:00Z',
};

const mockScheduledContent = {
  ...mockContentIdea,
  id: 'content-2',
  title: 'Scheduled Content',
  status: IdeaStatus.Scheduled,
  scheduledDate: '2024-02-01',
};

const mockPublishedContent = {
  ...mockContentIdea,
  id: 'content-3',
  title: 'Published Content',
  status: IdeaStatus.Published,
  scheduledDate: '2024-01-20',
};

describe('ContentIdeaCard', () => {
  const defaultProps = {
    idea: mockContentIdea,
    onEdit: vi.fn(),
    onDelete: vi.fn(),
  };

  it('should render content title', () => {
    render(<ContentIdeaCard {...defaultProps} />, { wrapper: createWrapper() });

    expect(screen.getByText('Test Content Idea')).toBeInTheDocument();
  });

  it('should render content notes', () => {
    render(<ContentIdeaCard {...defaultProps} />, { wrapper: createWrapper() });

    expect(screen.getByText('This is a test note for the content idea')).toBeInTheDocument();
  });

  it('should render platform tags', () => {
    render(<ContentIdeaCard {...defaultProps} />, { wrapper: createWrapper() });

    expect(screen.getByText('YouTube')).toBeInTheDocument();
    expect(screen.getByText('TikTok')).toBeInTheDocument();
  });

  it('should render status badge for Idea', () => {
    render(<ContentIdeaCard {...defaultProps} />, { wrapper: createWrapper() });

    // Status appears in both badge and dropdown, so use getAllByText
    const ideaElements = screen.getAllByText('Idea');
    expect(ideaElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should render status badge for Scheduled', () => {
    render(<ContentIdeaCard {...defaultProps} idea={mockScheduledContent} />, {
      wrapper: createWrapper(),
    });

    // Status appears in both badge and dropdown
    const scheduledElements = screen.getAllByText('Scheduled');
    expect(scheduledElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should render status badge for Published', () => {
    render(<ContentIdeaCard {...defaultProps} idea={mockPublishedContent} />, {
      wrapper: createWrapper(),
    });

    // Status appears in both badge and dropdown
    const publishedElements = screen.getAllByText('Published');
    expect(publishedElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should show scheduled date when present', () => {
    render(<ContentIdeaCard {...defaultProps} idea={mockScheduledContent} />, {
      wrapper: createWrapper(),
    });

    expect(screen.getByText(/scheduled for/i)).toBeInTheDocument();
    expect(screen.getByText(/feb 1, 2024/i)).toBeInTheDocument();
  });

  it('should show created date', () => {
    render(<ContentIdeaCard {...defaultProps} />, { wrapper: createWrapper() });

    expect(screen.getByText(/created/i)).toBeInTheDocument();
    expect(screen.getByText(/jan 1, 2024/i)).toBeInTheDocument();
  });

  it('should call onEdit when edit menu item is clicked', async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();
    render(<ContentIdeaCard {...defaultProps} onEdit={onEdit} />, {
      wrapper: createWrapper(),
    });

    const menuButton = screen.getByRole('button', { name: /open menu/i });
    await user.click(menuButton);

    const editButton = screen.getByText(/edit/i);
    await user.click(editButton);

    expect(onEdit).toHaveBeenCalledWith(mockContentIdea);
  });

  it('should call onDelete when delete menu item is clicked', async () => {
    const user = userEvent.setup();
    const onDelete = vi.fn();
    render(<ContentIdeaCard {...defaultProps} onDelete={onDelete} />, {
      wrapper: createWrapper(),
    });

    const menuButton = screen.getByRole('button', { name: /open menu/i });
    await user.click(menuButton);

    const deleteButton = screen.getByText(/delete/i);
    await user.click(deleteButton);

    expect(onDelete).toHaveBeenCalledWith(mockContentIdea.id);
  });

  it('should handle content without notes', () => {
    const ideaWithoutNotes = {
      ...mockContentIdea,
      notes: undefined,
    };

    render(<ContentIdeaCard {...defaultProps} idea={ideaWithoutNotes} />, {
      wrapper: createWrapper(),
    });

    expect(screen.getByText('Test Content Idea')).toBeInTheDocument();
  });

  it('should handle empty platform tags', () => {
    const ideaWithoutTags = {
      ...mockContentIdea,
      platformTags: [],
    };

    render(<ContentIdeaCard {...defaultProps} idea={ideaWithoutTags} />, {
      wrapper: createWrapper(),
    });

    expect(screen.getByText('Test Content Idea')).toBeInTheDocument();
    expect(screen.queryByText('YouTube')).not.toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <ContentIdeaCard {...defaultProps} className="custom-class" />,
      { wrapper: createWrapper() }
    );

    const card = container.querySelector('.custom-class');
    expect(card).toBeInTheDocument();
  });
});
