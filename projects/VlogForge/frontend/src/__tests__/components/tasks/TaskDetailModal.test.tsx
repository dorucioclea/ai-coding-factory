/**
 * Unit tests for TaskDetailModal component
 * ACF-014 AC5: Task details modal with comments and history tabs
 */

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { TaskDetailModal } from '@/components/tasks/TaskDetailModal';
import { AssignmentStatus, TaskHistoryAction, type TaskAssignmentResponse } from '@/types';

const mockTask: TaskAssignmentResponse = {
  id: 'task-1',
  contentItemId: 'content-12345678',
  teamId: 'team-1',
  assigneeId: 'user-abcd1234',
  assignedById: 'user-efgh5678',
  dueDate: '2024-02-15',
  status: AssignmentStatus.InProgress,
  notes: 'Important task notes',
  isOverdue: false,
  comments: [
    {
      id: 'comment-1',
      content: 'First comment',
      authorId: 'user-zzzz9999',
      isEdited: false,
      createdAt: '2024-01-16T00:00:00Z',
    },
  ],
  history: [
    {
      id: 'hist-1',
      changedByUserId: 'user-efgh5678',
      action: TaskHistoryAction.Created,
      description: 'Task was created',
      createdAt: '2024-01-15T00:00:00Z',
    },
    {
      id: 'hist-2',
      changedByUserId: 'user-abcd1234',
      action: TaskHistoryAction.StatusChanged,
      description: 'Status changed to In Progress',
      createdAt: '2024-01-16T12:00:00Z',
    },
  ],
  createdAt: '2024-01-15T00:00:00Z',
};

const defaultProps = {
  task: mockTask,
  isOpen: true,
  onClose: vi.fn(),
  onStatusChange: vi.fn(),
  onAddComment: vi.fn().mockResolvedValue(undefined),
};

describe('TaskDetailModal', () => {
  it('should render nothing when task is null', () => {
    const { container } = render(
      <TaskDetailModal {...defaultProps} task={null} />
    );

    expect(container.innerHTML).toBe('');
  });

  it('should render task details when open', () => {
    render(<TaskDetailModal {...defaultProps} />);

    expect(screen.getByText('Task Details')).toBeInTheDocument();
    expect(screen.getByText(/content-12345678/)).toBeInTheDocument();
  });

  it('should show assigned to and assigned by info', () => {
    render(<TaskDetailModal {...defaultProps} />);

    expect(screen.getByText(/Assigned to:/)).toBeInTheDocument();
    expect(screen.getByText(/User user-abc/)).toBeInTheDocument();
    expect(screen.getByText(/Assigned by:/)).toBeInTheDocument();
    expect(screen.getByText(/User user-efg/)).toBeInTheDocument();
  });

  it('should show due date', () => {
    render(<TaskDetailModal {...defaultProps} />);

    expect(screen.getByText(/Due date:/)).toBeInTheDocument();
    expect(screen.getByText(/February 15, 2024/)).toBeInTheDocument();
  });

  it('should show task notes', () => {
    render(<TaskDetailModal {...defaultProps} />);

    expect(screen.getByText('Important task notes')).toBeInTheDocument();
  });

  it('should render Comments and History tabs', () => {
    render(<TaskDetailModal {...defaultProps} />);

    // Tab triggers and inner component headers both show counts.
    // Use getAllByText to handle duplicates.
    const commentsTabs = screen.getAllByText(/Comments \(1\)/);
    expect(commentsTabs.length).toBeGreaterThanOrEqual(1);

    // History tab is not active by default, so only the tab trigger shows count
    const historyTab = screen.getByRole('tab', { name: /History/ });
    expect(historyTab).toBeInTheDocument();
  });

  it('should show comments tab by default', () => {
    render(<TaskDetailModal {...defaultProps} />);

    // Comments tab is active by default - comment content should be visible
    expect(screen.getByText('First comment')).toBeInTheDocument();
  });

  it('should switch to history tab when clicked', async () => {
    const user = userEvent.setup();
    render(<TaskDetailModal {...defaultProps} />);

    const historyTab = screen.getByRole('tab', { name: /History/ });
    await user.click(historyTab);

    // History content should now be visible
    expect(screen.getByText('Task was created')).toBeInTheDocument();
    expect(screen.getByText('Status changed to In Progress')).toBeInTheDocument();
  });

  it('should show overdue badge when task is overdue', () => {
    const overdueTask = { ...mockTask, isOverdue: true };
    render(<TaskDetailModal {...defaultProps} task={overdueTask} />);

    expect(screen.getByText('Overdue')).toBeInTheDocument();
  });

  it('should show status badge', () => {
    render(<TaskDetailModal {...defaultProps} />);

    // In Progress status badge
    const badges = screen.getAllByText('In Progress');
    expect(badges.length).toBeGreaterThanOrEqual(1);
  });

  it('should show history count of 0 when no history', () => {
    const taskNoHistory = { ...mockTask, history: [] };
    render(<TaskDetailModal {...defaultProps} task={taskNoHistory} />);

    const historyTab = screen.getByRole('tab', { name: /History \(0\)/ });
    expect(historyTab).toBeInTheDocument();
  });

  it('should show Update Status section', () => {
    render(<TaskDetailModal {...defaultProps} />);

    expect(screen.getByText('Update Status')).toBeInTheDocument();
  });

  it('should show completed date when task is completed', () => {
    const completedTask = {
      ...mockTask,
      status: AssignmentStatus.Completed,
      completedAt: '2024-02-10T15:30:00Z',
    };
    render(<TaskDetailModal {...defaultProps} task={completedTask} />);

    expect(screen.getByText(/Completed:/)).toBeInTheDocument();
  });
});
