/**
 * Unit tests for TaskHistory component
 * ACF-014 AC5: Task details modal with history
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { TaskHistory } from '@/components/tasks/TaskHistory';
import { TaskHistoryAction, type TaskHistoryResponse } from '@/types';

const mockHistory: TaskHistoryResponse[] = [
  {
    id: 'hist-1',
    changedByUserId: 'user-abc12345',
    action: TaskHistoryAction.Created,
    description: 'Task was created',
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: 'hist-2',
    changedByUserId: 'user-def67890',
    action: TaskHistoryAction.StatusChanged,
    description: 'Status changed from Not Started to In Progress',
    createdAt: '2024-01-16T14:30:00Z',
  },
  {
    id: 'hist-3',
    changedByUserId: 'user-abc12345',
    action: TaskHistoryAction.CommentAdded,
    description: 'Added a comment',
    createdAt: '2024-01-17T09:15:00Z',
  },
];

describe('TaskHistory', () => {
  it('should show empty state when no history', () => {
    render(<TaskHistory history={[]} />);

    expect(screen.getByText('No history recorded yet.')).toBeInTheDocument();
  });

  it('should show history count in header', () => {
    render(<TaskHistory history={mockHistory} />);

    expect(screen.getByText('History (3)')).toBeInTheDocument();
  });

  it('should render all history entries', () => {
    render(<TaskHistory history={mockHistory} />);

    expect(screen.getByText('Task was created')).toBeInTheDocument();
    expect(screen.getByText('Status changed from Not Started to In Progress')).toBeInTheDocument();
    expect(screen.getByText('Added a comment')).toBeInTheDocument();
  });

  it('should render action labels for each entry', () => {
    render(<TaskHistory history={mockHistory} />);

    expect(screen.getByText('Created')).toBeInTheDocument();
    expect(screen.getByText('Status Changed')).toBeInTheDocument();
    expect(screen.getByText('Comment Added')).toBeInTheDocument();
  });

  it('should show avatar initials from user ID', () => {
    render(<TaskHistory history={mockHistory} />);

    // getInitials takes first 2 chars of userId and uppercases
    // "user-abc12345" → "US", "user-def67890" → "US"
    const avatars = screen.getAllByText('US');
    expect(avatars.length).toBeGreaterThanOrEqual(2);
  });

  it('should show zero count when empty', () => {
    render(<TaskHistory history={[]} />);

    expect(screen.getByText('History (0)')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <TaskHistory history={mockHistory} className="custom-history-class" />
    );

    expect(container.firstChild).toHaveClass('custom-history-class');
  });

  it('should render all history action types', () => {
    const allActionHistory: TaskHistoryResponse[] = [
      {
        id: 'h1',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.Created,
        description: 'Created',
        createdAt: '2024-01-01T00:00:00Z',
      },
      {
        id: 'h2',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.StatusChanged,
        description: 'Status changed',
        createdAt: '2024-01-02T00:00:00Z',
      },
      {
        id: 'h3',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.Reassigned,
        description: 'Reassigned to new user',
        createdAt: '2024-01-03T00:00:00Z',
      },
      {
        id: 'h4',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.DueDateChanged,
        description: 'Due date updated',
        createdAt: '2024-01-04T00:00:00Z',
      },
      {
        id: 'h5',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.CommentAdded,
        description: 'Comment added',
        createdAt: '2024-01-05T00:00:00Z',
      },
    ];

    render(<TaskHistory history={allActionHistory} />);

    expect(screen.getByText('History (5)')).toBeInTheDocument();
    expect(screen.getByText('Reassigned')).toBeInTheDocument();
    expect(screen.getByText('Due Date Changed')).toBeInTheDocument();
  });

  it('should handle invalid date gracefully', () => {
    const historyWithBadDate: TaskHistoryResponse[] = [
      {
        id: 'h-bad',
        changedByUserId: 'user-1',
        action: TaskHistoryAction.Created,
        description: 'Task created',
        createdAt: 'invalid-date',
      },
    ];

    render(<TaskHistory history={historyWithBadDate} />);

    // Should fall back to 'recently' for invalid dates
    expect(screen.getByText('recently')).toBeInTheDocument();
  });
});
