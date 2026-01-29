/**
 * Unit tests for ProjectTaskList component
 * Story: ACF-013 - Shared Project Spaces
 */

import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent } from '../../utils/test-utils';
import { ProjectTaskList } from '@/components/shared-projects/ProjectTaskList';
import type { SharedProjectTaskDto } from '@/types/shared-project';

const mockTasks: SharedProjectTaskDto[] = [
  {
    id: 'task-1',
    createdByUserId: 'user-1',
    title: 'Film intro',
    description: 'Film the intro segment',
    status: 'Open',
    createdAt: '2025-06-15T10:00:00Z',
  },
  {
    id: 'task-2',
    createdByUserId: 'user-2',
    title: 'Edit video',
    status: 'InProgress',
    createdAt: '2025-06-16T10:00:00Z',
  },
  {
    id: 'task-3',
    createdByUserId: 'user-1',
    title: 'Upload thumbnail',
    status: 'Completed',
    completedAt: '2025-06-17T10:00:00Z',
    createdAt: '2025-06-15T10:00:00Z',
  },
];

describe('ProjectTaskList', () => {
  const defaultProps = {
    tasks: mockTasks,
    isActive: true,
    onAddTask: vi.fn(),
    onUpdateTaskStatus: vi.fn(),
  };

  it('should render task count', () => {
    render(<ProjectTaskList {...defaultProps} />);
    expect(screen.getByText('Tasks (3)')).toBeDefined();
  });

  it('should render task titles', () => {
    render(<ProjectTaskList {...defaultProps} />);
    expect(screen.getByText('Film intro')).toBeDefined();
    expect(screen.getByText('Edit video')).toBeDefined();
    expect(screen.getByText('Upload thumbnail')).toBeDefined();
  });

  it('should render task descriptions when present', () => {
    render(<ProjectTaskList {...defaultProps} />);
    expect(screen.getByText('Film the intro segment')).toBeDefined();
  });

  it('should render status badges', () => {
    render(<ProjectTaskList {...defaultProps} />);
    expect(screen.getByText('Open')).toBeDefined();
    expect(screen.getByText('In Progress')).toBeDefined();
    expect(screen.getByText('Completed')).toBeDefined();
  });

  it('should show Add Task button when active', () => {
    render(<ProjectTaskList {...defaultProps} />);
    expect(screen.getByText('Add Task')).toBeDefined();
  });

  it('should not show Add Task button when inactive', () => {
    render(<ProjectTaskList {...defaultProps} isActive={false} />);
    expect(screen.queryByText('Add Task')).toBeNull();
  });

  it('should show add form when Add Task is clicked', () => {
    render(<ProjectTaskList {...defaultProps} />);

    fireEvent.click(screen.getByText('Add Task'));

    expect(screen.getByPlaceholderText('Task title')).toBeDefined();
    expect(screen.getByPlaceholderText('Description (optional)')).toBeDefined();
  });

  it('should show empty message when no tasks', () => {
    render(<ProjectTaskList {...defaultProps} tasks={[]} />);
    expect(screen.getByText('No tasks yet. Add a task to get started.')).toBeDefined();
  });

  it('should render due date when present', () => {
    const taskWithDueDate = {
      ...mockTasks[0],
      dueDate: '2025-07-01T00:00:00Z',
    };
    render(<ProjectTaskList {...defaultProps} tasks={[taskWithDueDate]} />);
    expect(screen.getByText(/Due:/)).toBeDefined();
  });
});
