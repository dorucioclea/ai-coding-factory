import { test, expect, type Page, type Route } from '@playwright/test';

/**
 * E2E Behavioral Tests for Shared Project Spaces Feature
 * Story: ACF-013
 *
 * Tests verify user flows for project list, detail, tasks, links, activity, leave, close.
 */

// Mock data
const mockProject = {
  id: 'proj-1',
  name: 'Joint Video Project',
  description: 'A collaboration between two creators',
  status: 'Active',
  collaborationRequestId: 'collab-1',
  ownerId: 'user-1',
  memberCount: 2,
  taskCount: 2,
  linkCount: 1,
  createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
  members: [
    { id: 'mem-1', userId: 'user-1', role: 'Owner', joinedAt: new Date().toISOString() },
    { id: 'mem-2', userId: 'user-2', role: 'Member', joinedAt: new Date().toISOString() },
  ],
};

const mockClosedProject = {
  ...mockProject,
  id: 'proj-2',
  name: 'Completed Collab',
  status: 'Closed',
  closedAt: new Date().toISOString(),
  taskCount: 5,
  linkCount: 3,
};

const mockDetailProject = {
  id: 'proj-1',
  name: 'Joint Video Project',
  description: 'A collaboration between two creators',
  status: 'Active',
  collaborationRequestId: 'collab-1',
  ownerId: 'user-1',
  createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
  members: [
    { id: 'mem-1', userId: 'user-1', role: 'Owner', joinedAt: new Date().toISOString() },
    { id: 'mem-2', userId: 'user-2', role: 'Member', joinedAt: new Date().toISOString() },
  ],
  tasks: [
    {
      id: 'task-1',
      createdByUserId: 'user-1',
      title: 'Film intro segment',
      description: 'Record the intro for our collab video',
      status: 'Open',
      createdAt: new Date().toISOString(),
    },
    {
      id: 'task-2',
      createdByUserId: 'user-2',
      title: 'Edit final cut',
      status: 'InProgress',
      assigneeId: 'user-2',
      createdAt: new Date().toISOString(),
    },
  ],
  links: [
    {
      id: 'link-1',
      addedByUserId: 'user-1',
      title: 'Script Document',
      url: 'https://docs.google.com/doc/123',
      description: 'Our shared script',
      createdAt: new Date().toISOString(),
    },
  ],
};

const mockActivityResponse = {
  items: [
    {
      id: 'act-1',
      userId: 'user-1',
      activityType: 'ProjectCreated',
      message: 'Project "Joint Video Project" was created',
      createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: 'act-2',
      userId: 'user-1',
      activityType: 'MemberJoined',
      message: 'User joined the project as Owner',
      createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: 'act-3',
      userId: 'user-1',
      activityType: 'TaskAdded',
      message: 'Task "Film intro segment" was added',
      createdAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ],
  totalCount: 3,
  page: 1,
  pageSize: 50,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

function createMockListResponse(
  items: typeof mockProject[],
  totalCount?: number,
  page = 1
) {
  return {
    items,
    totalCount: totalCount ?? items.length,
    page,
    pageSize: 20,
    totalPages: Math.ceil((totalCount ?? items.length) / 20),
    hasNextPage: false,
    hasPreviousPage: page > 1,
  };
}

/**
 * Setup API mocks for shared project endpoints
 */
async function setupProjectMocks(
  page: Page,
  options: {
    listItems?: typeof mockProject[];
    detailProject?: typeof mockDetailProject;
    shouldFail?: boolean;
    errorStatus?: number;
  } = {}
) {
  const {
    listItems = [mockProject, mockClosedProject],
    detailProject = mockDetailProject,
    shouldFail = false,
    errorStatus = 500,
  } = options;

  // Mock list endpoint
  await page.route('**/api/projects?**', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({ title: 'Server Error', status: errorStatus }),
      });
      return;
    }

    const url = route.request().url();
    const statusMatch = url.match(/status=([^&]*)/);
    const statusFilter = statusMatch ? decodeURIComponent(statusMatch[1]) : null;

    let filtered = listItems;
    if (statusFilter) {
      filtered = listItems.filter((p) => p.status === statusFilter);
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(createMockListResponse(filtered)),
    });
  });

  // Mock bare /api/projects endpoint (no query params)
  await page.route('**/api/projects', async (route: Route) => {
    const url = route.request().url();
    if (url.includes('?')) return route.fallback();

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(createMockListResponse(listItems)),
    });
  });

  // Mock detail endpoint
  await page.route('**/api/projects/proj-*', async (route: Route) => {
    const url = route.request().url();
    if (url.includes('/tasks') || url.includes('/links') || url.includes('/activity') || url.includes('/leave') || url.includes('/close')) {
      return route.fallback();
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(detailProject),
    });
  });

  // Mock activity endpoint
  await page.route('**/api/projects/*/activity**', async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockActivityResponse),
    });
  });

  // Mock add task endpoint
  await page.route('**/api/projects/*/tasks', async (route: Route) => {
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'task-new',
          createdByUserId: 'user-1',
          title: body.title,
          description: body.description,
          status: 'Open',
          createdAt: new Date().toISOString(),
        }),
      });
    } else {
      await route.fallback();
    }
  });

  // Mock update task endpoint
  await page.route('**/api/projects/*/tasks/*', async (route: Route) => {
    if (route.request().method() === 'PUT') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(detailProject),
      });
    } else {
      await route.fallback();
    }
  });

  // Mock add link endpoint
  await page.route('**/api/projects/*/links', async (route: Route) => {
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'link-new',
          addedByUserId: 'user-1',
          title: body.title,
          url: body.url,
          description: body.description,
          createdAt: new Date().toISOString(),
        }),
      });
    } else {
      await route.fallback();
    }
  });

  // Mock leave endpoint
  await page.route('**/api/projects/*/leave', async (route: Route) => {
    await route.fulfill({ status: 204 });
  });

  // Mock close endpoint
  await page.route('**/api/projects/*/close', async (route: Route) => {
    await route.fulfill({ status: 204 });
  });
}

test.describe('Shared Projects - List Page', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects');
    await expect(page.getByRole('heading', { name: /shared projects/i })).toBeVisible();
  });

  test('should display project list with counts', async ({ page }) => {
    await expect(page.getByText('2 projects')).toBeVisible({ timeout: 5000 });
  });

  test('should display project cards with names', async ({ page }) => {
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Completed Collab')).toBeVisible();
  });

  test('should display project descriptions', async ({ page }) => {
    await expect(page.getByText('A collaboration between two creators')).toBeVisible({ timeout: 5000 });
  });

  test('should display task and link counts on cards', async ({ page }) => {
    await expect(page.getByText('2 tasks').first()).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('1 links').first()).toBeVisible();
  });

  test('should display status badges', async ({ page }) => {
    await expect(page.getByText('Active').first()).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Closed').first()).toBeVisible();
  });
});

test.describe('Shared Projects - Status Filter', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });

  test('should filter by Active status', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects') &&
      request.url().includes('status=Active')
    );

    await page.getByRole('button', { name: /^active$/i }).click();

    await requestPromise;
    await expect(page.getByText('1 project')).toBeVisible({ timeout: 5000 });
  });

  test('should filter by Closed status', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects') &&
      request.url().includes('status=Closed')
    );

    await page.getByRole('button', { name: /^closed$/i }).click();

    await requestPromise;
    await expect(page.getByText('1 project')).toBeVisible({ timeout: 5000 });
  });

  test('should show all projects when All filter is selected', async ({ page }) => {
    await page.getByRole('button', { name: /^active$/i }).click();
    await expect(page.getByText('1 project')).toBeVisible({ timeout: 5000 });

    await page.getByRole('button', { name: /^all$/i }).click();
    await expect(page.getByText('2 projects')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Shared Projects - Empty State', () => {
  test('should show empty state when no projects', async ({ page }) => {
    await setupProjectMocks(page, { listItems: [] });
    await page.goto('/dashboard/projects');

    await expect(
      page.getByRole('heading', { name: /no shared projects yet/i })
    ).toBeVisible({ timeout: 5000 });

    await expect(
      page.getByText(/when you accept a collaboration request/i)
    ).toBeVisible();
  });
});

test.describe('Shared Projects - Detail Page', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });

  test('should display project name and status', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Joint Video Project' })).toBeVisible();
    await expect(page.getByText('Active')).toBeVisible();
  });

  test('should display project description', async ({ page }) => {
    await expect(page.getByText('A collaboration between two creators')).toBeVisible();
  });

  test('should display member count', async ({ page }) => {
    await expect(page.getByText('2 members')).toBeVisible();
  });

  test('should display leave and close buttons for active project', async ({ page }) => {
    await expect(page.getByRole('button', { name: /leave/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /close project/i })).toBeVisible();
  });
});

test.describe('Shared Projects - Tasks Tab', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });

  test('should display tasks by default', async ({ page }) => {
    await expect(page.getByText('Tasks (2)')).toBeVisible();
    await expect(page.getByText('Film intro segment')).toBeVisible();
    await expect(page.getByText('Edit final cut')).toBeVisible();
  });

  test('should display task status badges', async ({ page }) => {
    await expect(page.getByText('Open')).toBeVisible();
    await expect(page.getByText('In Progress')).toBeVisible();
  });

  test('should show task description', async ({ page }) => {
    await expect(page.getByText('Record the intro for our collab video')).toBeVisible();
  });

  test('should show add task form when button clicked', async ({ page }) => {
    await page.getByRole('button', { name: /add task/i }).click();

    await expect(page.getByPlaceholder('Task title')).toBeVisible();
    await expect(page.getByPlaceholder('Description (optional)')).toBeVisible();
  });

  test('should submit add task form', async ({ page }) => {
    await page.getByRole('button', { name: /add task/i }).click();

    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects/') &&
      request.url().includes('/tasks') &&
      request.method() === 'POST'
    );

    await page.getByPlaceholder('Task title').fill('New task');
    await page.getByRole('button', { name: /^add$/i }).click();

    const request = await requestPromise;
    const body = request.postDataJSON();
    expect(body.title).toBe('New task');
  });
});

test.describe('Shared Projects - Links Tab', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });

  test('should display links when tab is selected', async ({ page }) => {
    await page.getByRole('button', { name: /^links$/i }).click();

    await expect(page.getByText('Links & Resources (1)')).toBeVisible();
    await expect(page.getByText('Script Document')).toBeVisible();
    await expect(page.getByText('Our shared script')).toBeVisible();
  });

  test('should show add link form when button clicked', async ({ page }) => {
    await page.getByRole('button', { name: /^links$/i }).click();

    await page.getByRole('button', { name: /add link/i }).click();

    await expect(page.getByPlaceholder('Link title')).toBeVisible();
    await expect(page.getByPlaceholder('https://...')).toBeVisible();
  });

  test('should submit add link form', async ({ page }) => {
    await page.getByRole('button', { name: /^links$/i }).click();
    await page.getByRole('button', { name: /add link/i }).click();

    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects/') &&
      request.url().includes('/links') &&
      request.method() === 'POST'
    );

    await page.getByPlaceholder('Link title').fill('New resource');
    await page.getByPlaceholder('https://...').fill('https://example.com/resource');
    await page.getByRole('button', { name: /^add link$/i }).click();

    const request = await requestPromise;
    const body = request.postDataJSON();
    expect(body.title).toBe('New resource');
    expect(body.url).toBe('https://example.com/resource');
  });
});

test.describe('Shared Projects - Activity Tab', () => {
  test.beforeEach(async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });

  test('should display activity feed when tab is selected', async ({ page }) => {
    const activityRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects/') &&
      request.url().includes('/activity')
    );

    await page.getByRole('button', { name: /^activity$/i }).click();

    await activityRequestPromise;

    await expect(page.getByText(/Project.*was created/)).toBeVisible({ timeout: 5000 });
    await expect(page.getByText(/User joined the project/)).toBeVisible();
    await expect(page.getByText(/Task.*was added/)).toBeVisible();
  });
});

test.describe('Shared Projects - Leave Project', () => {
  test('should show confirmation dialog and make API call', async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });

    await page.getByRole('button', { name: /leave/i }).first().click();

    await expect(page.getByText(/are you sure you want to leave/i)).toBeVisible();

    const leaveRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects/') &&
      request.url().includes('/leave') &&
      request.method() === 'POST'
    );

    await page.getByRole('button', { name: /confirm leave/i }).click();

    const request = await leaveRequestPromise;
    expect(request.method()).toBe('POST');
  });

  test('should cancel leave when clicking Cancel', async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });

    await page.getByRole('button', { name: /leave/i }).first().click();
    await expect(page.getByText(/are you sure you want to leave/i)).toBeVisible();

    await page.getByRole('button', { name: /^cancel$/i }).click();

    await expect(page.getByText(/are you sure you want to leave/i)).not.toBeVisible();
  });
});

test.describe('Shared Projects - Close Project', () => {
  test('should show confirmation dialog and make API call', async ({ page }) => {
    await setupProjectMocks(page);
    await page.goto('/dashboard/projects/proj-1');
    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });

    await page.getByRole('button', { name: /close project/i }).click();

    await expect(page.getByText(/are you sure you want to close/i)).toBeVisible();

    const closeRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/projects/') &&
      request.url().includes('/close') &&
      request.method() === 'POST'
    );

    await page.getByRole('button', { name: /confirm close/i }).click();

    const request = await closeRequestPromise;
    expect(request.method()).toBe('POST');
  });
});

test.describe('Shared Projects - Error Handling', () => {
  test('should show error on list page when API fails', async ({ page }) => {
    await setupProjectMocks(page, { shouldFail: true });
    await page.goto('/dashboard/projects');

    await expect(page.getByText(/error loading projects/i)).toBeVisible({ timeout: 5000 });
  });

  test('should show error on detail page when project not found', async ({ page }) => {
    await page.route('**/api/projects/proj-missing*', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/tasks') || url.includes('/links') || url.includes('/activity')) {
        return route.fallback();
      }
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Shared project not found.' }),
      });
    });

    await page.goto('/dashboard/projects/proj-missing');

    await expect(page.getByText(/error loading project/i)).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Shared Projects - Loading States', () => {
  test('should show skeleton loading on list page', async ({ page }) => {
    await page.route('**/api/projects**', async (route: Route) => {
      await new Promise((resolve) => setTimeout(resolve, 500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(createMockListResponse([mockProject])),
      });
    });

    await page.goto('/dashboard/projects');

    const skeletons = page.locator('[class*="skeleton"], .animate-pulse');
    const skeletonCount = await skeletons.count();
    expect(skeletonCount).toBeGreaterThan(0);

    await expect(page.getByText('Joint Video Project')).toBeVisible({ timeout: 5000 });
  });
});
