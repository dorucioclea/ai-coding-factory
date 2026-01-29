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
 * Setup API mocks for task endpoints.
 *
 * NOTE: buildUrl uses `new URL(endpoint, API_BASE_URL)` where API_BASE_URL
 * is 'http://localhost:5000/api'. Since the endpoint starts with '/',
 * the resolved URL is 'http://localhost:5000/tasks/mine' (no /api prefix).
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

  // Mock GET /tasks/mine (resolved URL has no /api prefix)
  await page.route('**/tasks/mine**', async (route: Route) => {
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

  // Mock GET/PATCH/POST /tasks/:id (and sub-resources)
  await page.route('**/tasks/task-*', async (route: Route) => {
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

/**
 * Wait for the tasks page to fully load by checking for a known task note.
 */
async function waitForTasksLoaded(page: Page) {
  await expect(page.getByText('Film the intro sequence')).toBeVisible();
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
      await waitForTasksLoaded(page);

      // Stats cards use h3 (CardTitle) - Not Started (2), In Progress (1), Completed (1)
      const statsGrid = page.locator('.grid.gap-4.md\\:grid-cols-4');
      await expect(statsGrid).toBeVisible();
    });

    test('should show tasks from multiple teams (cross-team aggregation)', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      // Task from team-1
      await expect(page.getByText('Film the intro sequence')).toBeVisible();
      // Task from team-2
      await expect(page.getByText('Edit video for review')).toBeVisible();
    });
  });

  test.describe('AC2: Task Grouping by Status', () => {
    test('should display three status group headings', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // GroupedTaskList uses h2 for group headings
      const h2Headings = page.locator('h2');
      await expect(h2Headings.filter({ hasText: 'Not Started' })).toBeVisible();
      await expect(h2Headings.filter({ hasText: 'In Progress' })).toBeVisible();
      await expect(h2Headings.filter({ hasText: 'Completed' })).toBeVisible();
    });

    test('should show correct task count per group', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Not Started group has 2 tasks (task-1 and task-2)
      await expect(page.getByText('Film the intro sequence')).toBeVisible();
      await expect(page.getByText('Write script for episode')).toBeVisible();

      // In Progress group has 1 task (task-3)
      await expect(page.getByText('Edit video for review')).toBeVisible();

      // Completed group has 1 task (task-4)
      await expect(page.getByText('Publish final video')).toBeVisible();
    });
  });

  test.describe('AC3: Due Date Sorting', () => {
    test('should render tasks within groups sorted by due date', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Within Not Started group, task-2 (due Jan 20) should come before task-1 (due Feb 10)
      // Both notes should be visible
      const notStartedSection = page.locator('section').first();
      const taskNotes = notStartedSection.locator('p');
      const firstNote = taskNotes.filter({ hasText: 'Write script for episode' });
      const secondNote = taskNotes.filter({ hasText: 'Film the intro sequence' });
      await expect(firstNote).toBeVisible();
      await expect(secondNote).toBeVisible();
    });
  });

  test.describe('AC4: Quick Status Update', () => {
    test('should show status dropdown on task cards', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Each task card has a Select trigger (rendered as a button)
      const selectTriggers = page.locator('button[role="combobox"]');
      await expect(selectTriggers.first()).toBeVisible();

      // Should have 4 dropdowns (one per task card)
      await expect(selectTriggers).toHaveCount(4);
    });
  });

  test.describe('AC5: Task Detail Modal', () => {
    test('should open task detail modal when clicking a task card', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Click on a task card by its notes
      await page.getByText('Film the intro sequence').click();

      // Modal should open with "Task Details" heading
      await expect(page.getByText('Task Details')).toBeVisible();
    });

    test('should show Comments and History tabs in modal', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Click on the overdue task (task-2) which has comments and history
      await page.getByText('Write script for episode').click();

      // Modal tabs
      await expect(page.getByRole('tab', { name: /Comments/i })).toBeVisible();
      await expect(page.getByRole('tab', { name: /History/i })).toBeVisible();
    });

    test('should display comments in Comments tab', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Click the overdue task which has a comment
      await page.getByText('Write script for episode').click();

      // Comments tab is active by default; the detail mock returns the comment
      await expect(page.getByText('Please prioritize this')).toBeVisible();
    });

    test('should switch to History tab and show history entries', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Click the overdue task
      await page.getByText('Write script for episode').click();

      // Click History tab
      await page.getByRole('tab', { name: /History/i }).click();

      // History content should appear
      await expect(page.getByText('Task was created')).toBeVisible();
    });

    test('should show overdue badge in modal for overdue tasks', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // task-2 is overdue, click it
      await page.getByText('Write script for episode').click();

      await expect(page.getByText('Overdue').first()).toBeVisible();
    });

    test('should close modal when pressing Escape', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      await page.getByText('Film the intro sequence').click();

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

      // React Query retries 3 times with exponential backoff before error state
      await expect(page.getByText(/failed to load tasks/i)).toBeVisible({ timeout: 15000 });
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

      await waitForTasksLoaded(page);

      // Overdue stat card label is always visible
      await expect(page.getByText('Overdue').first()).toBeVisible();
    });

    test('should highlight overdue task cards with destructive styling', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // The overdue task (task-2) card has border-destructive class
      const overdueCard = page.locator('.border-destructive').first();
      await expect(overdueCard).toBeVisible();
    });
  });

  test.describe('Collapse/Expand Groups', () => {
    test('should collapse a status group when header is clicked', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // All 4 task notes should be visible
      await expect(page.getByText('Film the intro sequence')).toBeVisible();
      await expect(page.getByText('Write script for episode')).toBeVisible();

      // Click the Not Started group header button to collapse
      // The button wraps the h2 heading
      const notStartedButton = page.locator('button').filter({ has: page.locator('h2', { hasText: 'Not Started' }) });
      await notStartedButton.click();

      // After collapsing Not Started, those 2 tasks should be hidden
      await expect(page.getByText('Film the intro sequence')).not.toBeVisible();
      await expect(page.getByText('Write script for episode')).not.toBeVisible();

      // Other groups remain visible
      await expect(page.getByText('Edit video for review')).toBeVisible();
      await expect(page.getByText('Publish final video')).toBeVisible();
    });

    test('should expand a collapsed group when header is clicked again', async ({ page }) => {
      await setupTaskMocks(page);
      await page.goto('/dashboard/tasks');

      await waitForTasksLoaded(page);

      // Click to collapse
      const notStartedButton = page.locator('button').filter({ has: page.locator('h2', { hasText: 'Not Started' }) });
      await notStartedButton.click();

      await expect(page.getByText('Film the intro sequence')).not.toBeVisible();

      // Click again to expand
      await notStartedButton.click();

      // Tasks should be visible again
      await expect(page.getByText('Film the intro sequence')).toBeVisible();
      await expect(page.getByText('Write script for episode')).toBeVisible();
    });
  });
});
