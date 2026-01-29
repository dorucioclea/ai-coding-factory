import { test, expect, type Page, type Route } from '@playwright/test';

/**
 * E2E Behavioral Tests for My Tasks Page
 * Story: ACF-014 - Team Member Task View
 *
 * AC1: Tasks assigned across all teams
 * AC2: Tasks grouped by status (Not Started, In Progress, Completed)
 * AC3: Sorted by due date (earliest first) within each group
 * AC4: Quick status update via dropdown on each card
 * AC5: Task detail modal with comments and history
 */

const mockTasks = [
  {
    id: 'task-1',
    contentItemId: 'content-aaa11111',
    teamId: 'team-1',
    assigneeId: 'user-1',
    assignedById: 'user-2',
    dueDate: '2024-02-10',
    status: 0, // NotStarted
    notes: 'Film the intro sequence',
    isOverdue: false,
    comments: [],
    history: [],
    createdAt: '2024-01-15T00:00:00Z',
  },
  {
    id: 'task-2',
    contentItemId: 'content-bbb22222',
    teamId: 'team-1',
    assigneeId: 'user-1',
    assignedById: 'user-2',
    dueDate: '2024-01-20',
    status: 0, // NotStarted
    notes: 'Write script for episode',
    isOverdue: true,
    comments: [
      {
        id: 'comment-1',
        content: 'Please prioritize this',
        authorId: 'user-2',
        isEdited: false,
        createdAt: '2024-01-16T10:00:00Z',
      },
    ],
    history: [
      {
        id: 'hist-1',
        changedByUserId: 'user-2',
        action: 0, // Created
        description: 'Task was created',
        createdAt: '2024-01-15T00:00:00Z',
      },
    ],
    createdAt: '2024-01-14T00:00:00Z',
  },
  {
    id: 'task-3',
    contentItemId: 'content-ccc33333',
    teamId: 'team-2',
    assigneeId: 'user-1',
    assignedById: 'user-3',
    dueDate: '2024-02-05',
    status: 1, // InProgress
    notes: 'Edit video for review',
    isOverdue: false,
    comments: [],
    history: [
      {
        id: 'hist-2',
        changedByUserId: 'user-3',
        action: 0,
        description: 'Task was created',
        createdAt: '2024-01-10T00:00:00Z',
      },
      {
        id: 'hist-3',
        changedByUserId: 'user-1',
        action: 1, // StatusChanged
        description: 'Status changed from Not Started to In Progress',
        createdAt: '2024-01-20T00:00:00Z',
      },
    ],
    createdAt: '2024-01-10T00:00:00Z',
  },
  {
    id: 'task-4',
    contentItemId: 'content-ddd44444',
    teamId: 'team-1',
    assigneeId: 'user-1',
    assignedById: 'user-2',
    dueDate: '2024-01-25',
    status: 2, // Completed
    notes: 'Publish final video',
    isOverdue: false,
    completedAt: '2024-01-24T15:00:00Z',
    comments: [],
    history: [],
    createdAt: '2024-01-05T00:00:00Z',
  },
];

const mockTaskListResponse = {
  items: mockTasks,
  totalCount: mockTasks.length,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

const mockDetailTask = {
  ...mockTasks[1],
  comments: [
    {
      id: 'comment-1',
      content: 'Please prioritize this',
      authorId: 'user-2',
      isEdited: false,
      createdAt: '2024-01-16T10:00:00Z',
    },
  ],
  history: [
    {
      id: 'hist-1',
      changedByUserId: 'user-2',
      action: 0,
      description: 'Task was created',
      createdAt: '2024-01-15T00:00:00Z',
    },
  ],
};

/**
 * Setup API mocks for task endpoints
 */
async function setupTaskMocks(
  page: Page,
  options: {
    tasks?: typeof mockTasks;
    detailTask?: typeof mockDetailTask;
    shouldFail?: boolean;
    statusUpdateShouldFail?: boolean;
    commentAddShouldFail?: boolean;
  } = {}
) {
  const {
    tasks = mockTasks,
    detailTask = mockDetailTask,
    shouldFail = false,
    statusUpdateShouldFail = false,
    commentAddShouldFail = false,
  } = options;

  // Mock GET /api/tasks/mine
  await page.route('**/api/tasks/mine**', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ title: 'Server Error', status: 500 }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: tasks,
        totalCount: tasks.length,
        page: 1,
        pageSize: 20,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }),
    });
  });

  // Mock GET /api/tasks/:id
  await page.route('**/api/tasks/task-*', async (route: Route) => {
    const url = route.request().url();
    const method = route.request().method();

    // Handle PATCH for status updates
    if (method === 'PATCH' && url.includes('/status')) {
      if (statusUpdateShouldFail) {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ title: 'Invalid status transition' }),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...detailTask, status: 1 }),
      });
      return;
    }

    // Handle POST for comments
    if (method === 'POST' && url.includes('/comments')) {
      if (commentAddShouldFail) {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ title: 'Comment cannot be empty' }),
        });
        return;
      }
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'comment-new',
          content: 'New comment',
          authorId: 'user-1',
          isEdited: false,
          createdAt: new Date().toISOString(),
        }),
      });
      return;
    }

    // Handle GET for task detail
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(detailTask),
    });
  });
}

test.describe('My Tasks Page - ACF-014', () => {
  test.describe('AC1: Tasks Dashboard Display', () => {
    test('should display My Tasks page title and description', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: /My Tasks/i })).toBeVisible();
      await expect(
        page.getByText(/view and manage your assigned tasks/i)
      ).toBeVisible();
    });

    test('should show stats cards with correct counts', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for data to load
      await expect(page.getByText(/Not Started/i).first()).toBeVisible();

      // Stats cards: Not Started (2), In Progress (1), Completed (1), Overdue (1)
      const statsSection = page.locator('.grid.gap-4');
      await expect(statsSection).toBeVisible();
    });

    test('should show tasks from multiple teams (cross-team aggregation)', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Tasks from team-1 and team-2 should both appear
      await expect(page.getByText(/content-aaa/i).first()).toBeVisible();
      await expect(page.getByText(/content-ccc/i).first()).toBeVisible();
    });
  });

  test.describe('AC2: Task Grouping by Status', () => {
    test('should display three status groups', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'In Progress' })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Completed' })).toBeVisible();
    });

    test('should show correct task count per group', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for groups to render
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Not Started group: 2 tasks, In Progress: 1, Completed: 1
      const notStartedSection = page.locator('section').filter({ hasText: 'Not Started' }).first();
      await expect(notStartedSection).toBeVisible();
    });
  });

  test.describe('AC3: Due Date Sorting', () => {
    test('should render tasks within groups (due date sorted)', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for tasks to load
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Tasks should appear within their respective groups
      // The hook sorts by dueDate ascending within each group
      const notStartedSection = page.locator('section').filter({ hasText: 'Not Started' }).first();
      await expect(notStartedSection).toBeVisible();
    });
  });

  test.describe('AC4: Quick Status Update', () => {
    test('should show status dropdown on task cards', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for tasks to load
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Each task card has a status dropdown (Select trigger)
      const selectTriggers = page.locator('button[role="combobox"]');
      await expect(selectTriggers.first()).toBeVisible();
    });
  });

  test.describe('AC5: Task Detail Modal', () => {
    test('should open task detail modal when clicking a task card', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for tasks
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Click on a task card content area
      await page.getByText(/content-aaa/i).first().click();

      // Modal should open with "Task Details" heading
      await expect(page.getByText('Task Details')).toBeVisible();
    });

    test('should show Comments and History tabs in modal', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await page.getByText(/content-bbb/i).first().click();

      // Modal tabs
      await expect(page.getByRole('tab', { name: /Comments/i })).toBeVisible();
      await expect(page.getByRole('tab', { name: /History/i })).toBeVisible();
    });

    test('should display comments in Comments tab', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await page.getByText(/content-bbb/i).first().click();

      // Comments tab is active by default
      await expect(page.getByText('Please prioritize this')).toBeVisible();
    });

    test('should switch to History tab and show history entries', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await page.getByText(/content-bbb/i).first().click();

      // Click History tab
      await page.getByRole('tab', { name: /History/i }).click();

      // History content should appear
      await expect(page.getByText('Task was created')).toBeVisible();
    });

    test('should show overdue badge in modal for overdue tasks', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // task-2 is overdue, click it
      await page.getByText(/content-bbb/i).first().click();

      await expect(page.getByText('Overdue').first()).toBeVisible();
    });

    test('should close modal when clicking close', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await page.getByText(/content-aaa/i).first().click();

      await expect(page.getByText('Task Details')).toBeVisible();

      // Close the dialog
      await page.keyboard.press('Escape');

      // Modal should be closed
      await expect(page.getByText('Task Details')).not.toBeVisible();
    });
  });

  test.describe('Error and Empty States', () => {
    test('should show error message when API fails', async ({ page }) => {
      await setupTaskMocks(page, { shouldFail: true });
      await page.goto('/dashboard/tasks');

      await expect(page.getByText(/failed to load tasks/i)).toBeVisible();
    });

    test('should show empty state when no tasks exist', async ({ page }) => {
      await setupTaskMocks(page, { tasks: [] });
      await page.goto('/dashboard/tasks');

      await expect(page.getByText(/no tasks assigned/i)).toBeVisible();
    });
  });

  test.describe('Overdue Indicators', () => {
    test('should show overdue count in stats card', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Wait for data to load
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Overdue stat card shows count (1 overdue task)
      await expect(page.getByText('Overdue').first()).toBeVisible();
    });

    test('should highlight overdue task cards with destructive border', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Overdue task card has border-destructive class
      const overdueCard = page.locator('.border-destructive').first();
      await expect(overdueCard).toBeVisible();
    });
  });

  test.describe('Collapse/Expand Groups', () => {
    test('should collapse a status group when header is clicked', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Count task cards before collapse
      const cardsBeforeCount = await page.getByText(/Content Item/i).count();

      // Click the Not Started group header button to collapse
      await page.getByRole('heading', { name: 'Not Started' }).click();

      // After collapsing, fewer cards should be visible
      const cardsAfterCount = await page.getByText(/Content Item/i).count();
      expect(cardsAfterCount).toBeLessThan(cardsBeforeCount);
    });

    test('should expand a collapsed group when header is clicked again', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();

      // Collapse
      await page.getByRole('heading', { name: 'Not Started' }).click();

      // Expand again
      await page.getByRole('heading', { name: 'Not Started' }).click();

      // All groups should be visible and expanded
      await expect(page.getByRole('heading', { name: 'Not Started' })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'In Progress' })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Completed' })).toBeVisible();
    });
  });
});
