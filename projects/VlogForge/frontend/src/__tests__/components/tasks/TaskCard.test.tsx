/**
 * Unit tests for TaskCard component
 * ACF-015 Phase 6 - Task Assignment
 */

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { TaskCard } from '@/components/tasks/TaskCard';
import { AssignmentStatus } from '@/types';

const mockTask = {
  id: 'task-1',
  contentItemId: 'content-12345678',
  teamId: 'team-1',
  assigneeId: 'user-1',
  assignedById: 'user-23456789',
  dueDate: '2024-02-15',
  status: AssignmentStatus.NotStarted,
  notes: 'This is a task note',
  isOverdue: false,
  comments: [],
  history: [],
  createdAt: '2024-01-15T00:00:00Z',
};

const mockOverdueTask = {
  ...mockTask,
  id: 'task-2',
  dueDate: '2024-01-01',
  isOverdue: true,
};

const mockTaskWithComments = {
  ...mockTask,
  id: 'task-3',
  comments: [
    {
      id: 'comment-1',
      content: 'Test comment',
      authorId: 'user-1',
      isEdited: false,
      createdAt: '2024-01-16T00:00:00Z',
    },
    {
      id: 'comment-2',
      content: 'Another comment',
      authorId: 'user-2',
      isEdited: false,
      createdAt: '2024-01-17T00:00:00Z',
    },
  ],
};

describe('TaskCard', () => {
  const defaultProps = {
    task: mockTask,
    onStatusChange: vi.fn(),
    onClick: vi.fn(),
  };

  it('should render task content item info', () => {
    render(<TaskCard {...defaultProps} />);

    // Shows first 8 chars of contentItemId
    expect(screen.getByText(/Content Item/)).toBeInTheDocument();
  });

  it('should render due date', () => {
    render(<TaskCard {...defaultProps} />);

    expect(screen.getByText(/Due:/)).toBeInTheDocument();
    expect(screen.getByText(/Feb 15, 2024/)).toBeInTheDocument();
  });

  it('should render Not Started status badge', () => {
    render(<TaskCard {...defaultProps} />);

    // Status appears in badge and dropdown, use getAllByText
    const statusElements = screen.getAllByText('Not Started');
    expect(statusElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should render InProgress status badge', () => {
    const inProgressTask = {
      ...mockTask,
      status: AssignmentStatus.InProgress,
    };
    render(<TaskCard {...defaultProps} task={inProgressTask} />);

    // Status appears in badge and dropdown, use getAllByText
    const statusElements = screen.getAllByText('In Progress');
    expect(statusElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should render Completed status badge', () => {
    const completedTask = {
      ...mockTask,
      status: AssignmentStatus.Completed,
    };
    render(<TaskCard {...defaultProps} task={completedTask} />);

    // Badge and dropdown both may show "Completed" - find at least one
    const completedElements = screen.getAllByText('Completed');
    expect(completedElements.length).toBeGreaterThanOrEqual(1);
  });

  it('should show task notes when present', () => {
    render(<TaskCard {...defaultProps} />);

    expect(screen.getByText('This is a task note')).toBeInTheDocument();
  });

  it('should show overdue indicator for overdue tasks', () => {
    render(<TaskCard {...defaultProps} task={mockOverdueTask} />);

    expect(screen.getByText('Overdue')).toBeInTheDocument();
  });

  it('should call onClick when card is clicked', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(<TaskCard {...defaultProps} onClick={onClick} />);

    // Click on the card (not the dropdown)
    const card = screen.getByText(/Content Item/);
    await user.click(card);

    expect(onClick).toHaveBeenCalledWith(mockTask.id);
  });

  it('should show comment count when there are comments', () => {
    render(<TaskCard {...defaultProps} task={mockTaskWithComments} />);

    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('should not show comment count when there are no comments', () => {
    render(<TaskCard {...defaultProps} />);

    // Should not have the comment count displayed
    expect(screen.queryByText('0')).not.toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <TaskCard {...defaultProps} className="custom-class" />
    );

    const card = container.querySelector('.custom-class');
    expect(card).toBeInTheDocument();
  });

  it('should highlight overdue tasks with special styling', () => {
    const { container } = render(
      <TaskCard {...defaultProps} task={mockOverdueTask} />
    );

    const card = container.querySelector('.border-destructive');
    expect(card).toBeInTheDocument();
  });

  it('should show assigned by info', () => {
    render(<TaskCard {...defaultProps} />);

    // Shows "Assigned by User {first 8 chars of assignedById}"
    expect(screen.getByText(/Assigned by User user-234/)).toBeInTheDocument();
  });
});
