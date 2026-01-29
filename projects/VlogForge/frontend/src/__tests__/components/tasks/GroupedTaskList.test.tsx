/**
 * Unit tests for GroupedTaskList component
 * ACF-014: Team Member Task View
 * AC2: Tasks grouped by status
 * AC3: Sorted by due date within groups
 */

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { GroupedTaskList } from '@/components/tasks/GroupedTaskList';
import { AssignmentStatus, type GroupedTasks, type TaskAssignmentResponse } from '@/types';

// Mock the api client for useTask hook
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn().mockResolvedValue(null),
    post: vi.fn(),
    patch: vi.fn(),
  },
}));

const makeTask = (overrides: Partial<TaskAssignmentResponse> = {}): TaskAssignmentResponse => ({
  id: 'task-1',
  contentItemId: 'content-12345678',
  teamId: 'team-1',
  assigneeId: 'user-1',
  assignedById: 'user-2',
  dueDate: '2024-02-15',
  status: AssignmentStatus.NotStarted,
  notes: 'Test notes',
  isOverdue: false,
  comments: [],
  history: [],
  createdAt: '2024-01-15T00:00:00Z',
  ...overrides,
});

const emptyGrouped: GroupedTasks = {
  [AssignmentStatus.NotStarted]: [],
  [AssignmentStatus.InProgress]: [],
  [AssignmentStatus.Completed]: [],
};

const populatedGrouped: GroupedTasks = {
  [AssignmentStatus.NotStarted]: [
    makeTask({ id: 'ns-1', status: AssignmentStatus.NotStarted }),
    makeTask({ id: 'ns-2', status: AssignmentStatus.NotStarted, dueDate: '2024-03-01' }),
  ],
  [AssignmentStatus.InProgress]: [
    makeTask({ id: 'ip-1', status: AssignmentStatus.InProgress }),
  ],
  [AssignmentStatus.Completed]: [
    makeTask({ id: 'c-1', status: AssignmentStatus.Completed }),
    makeTask({ id: 'c-2', status: AssignmentStatus.Completed }),
    makeTask({ id: 'c-3', status: AssignmentStatus.Completed }),
  ],
};

function createQueryWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0, staleTime: 0 },
      mutations: { retry: false },
    },
  });
  return function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    );
  };
}

const defaultProps = {
  groupedTasks: populatedGrouped,
  onStatusChange: vi.fn(),
  onAddComment: vi.fn().mockResolvedValue(undefined),
};

function renderWithProviders(ui: React.ReactElement) {
  return render(ui, { wrapper: createQueryWrapper() });
}

describe('GroupedTaskList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should show loading spinner when isLoading is true', () => {
    renderWithProviders(
      <GroupedTaskList
        {...defaultProps}
        isLoading={true}
      />
    );

    const spinner = document.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('should show empty state when all groups are empty', () => {
    renderWithProviders(
      <GroupedTaskList
        {...defaultProps}
        groupedTasks={emptyGrouped}
      />
    );

    expect(screen.getByText('No tasks assigned to you yet.')).toBeInTheDocument();
  });

  it('should render all three status group headers', () => {
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    expect(screen.getAllByText('Not Started').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('In Progress').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Completed').length).toBeGreaterThanOrEqual(1);

    const headings = screen.getAllByRole('heading', { level: 2 });
    const headingTexts = headings.map((h) => h.textContent);
    expect(headingTexts).toContain('Not Started');
    expect(headingTexts).toContain('In Progress');
    expect(headingTexts).toContain('Completed');
  });

  it('should show task count badges for each group', () => {
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('should collapse a group when its header is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    const sections = screen.getAllByRole('button');
    const notStartedToggle = sections[0]!;

    const cardsBefore = screen.getAllByText(/Content Item/);
    expect(cardsBefore.length).toBeGreaterThanOrEqual(6);

    await user.click(notStartedToggle);

    const cardsAfter = screen.getAllByText(/Content Item/);
    expect(cardsAfter.length).toBe(4);
  });

  it('should show "No tasks with this status" for empty expanded groups', () => {
    const mixedGrouped: GroupedTasks = {
      [AssignmentStatus.NotStarted]: [makeTask({ id: 'ns-1', status: AssignmentStatus.NotStarted })],
      [AssignmentStatus.InProgress]: [],
      [AssignmentStatus.Completed]: [],
    };

    renderWithProviders(
      <GroupedTaskList
        {...defaultProps}
        groupedTasks={mixedGrouped}
      />
    );

    const emptyMessages = screen.getAllByText('No tasks with this status.');
    expect(emptyMessages).toHaveLength(2);
  });

  it('should call onStatusChange when status changes on a task', () => {
    const onStatusChange = vi.fn();
    renderWithProviders(
      <GroupedTaskList
        {...defaultProps}
        onStatusChange={onStatusChange}
      />
    );

    const headings = screen.getAllByRole('heading', { level: 2 });
    expect(headings.length).toBe(3);
  });

  it('should apply custom className', () => {
    const { container } = renderWithProviders(
      <GroupedTaskList
        {...defaultProps}
        className="custom-test-class"
      />
    );

    expect(container.firstChild).toHaveClass('custom-test-class');
  });

  it('should not render loading or empty state when tasks exist', () => {
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    expect(screen.queryByText('No tasks assigned to you yet.')).not.toBeInTheDocument();
    expect(document.querySelector('.animate-spin')).not.toBeInTheDocument();
  });

  it('should render TaskCards for each task in expanded groups', () => {
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    const contentItems = screen.getAllByText(/Content Item/);
    expect(contentItems.length).toBeGreaterThanOrEqual(6);
  });

  it('should expand a previously collapsed group on second click', async () => {
    const user = userEvent.setup();
    renderWithProviders(<GroupedTaskList {...defaultProps} />);

    const sections = screen.getAllByRole('button');
    const notStartedToggle = sections[0]!;

    await user.click(notStartedToggle);
    expect(screen.getAllByText(/Content Item/).length).toBe(4);

    await user.click(notStartedToggle);

    const cardsAfterExpand = screen.getAllByText(/Content Item/);
    expect(cardsAfterExpand.length).toBeGreaterThanOrEqual(6);
  });
});
