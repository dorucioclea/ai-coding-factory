import { test, expect, type Page, type Route } from '@playwright/test';

/**
 * E2E Behavioral Tests for Collaboration Requests Feature
 * Story: ACF-011
 *
 * Tests verify user flows for inbox, sent, accept, decline, and filters.
 */

// Mock data
const mockPendingRequest = {
  id: 'req-1',
  senderId: 'user-1',
  recipientId: 'user-2',
  senderDisplayName: 'Alice Creator',
  senderUsername: 'alice_creator',
  senderProfilePictureUrl: 'https://example.com/alice.jpg',
  recipientDisplayName: 'Bob Vlogger',
  recipientUsername: 'bob_vlogger',
  recipientProfilePictureUrl: 'https://example.com/bob.jpg',
  message: 'Let us collaborate on a tech review video!',
  status: 'Pending',
  expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
  createdAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
  isExpired: false,
};

const mockAcceptedRequest = {
  ...mockPendingRequest,
  id: 'req-2',
  senderDisplayName: 'Charlie Vlog',
  senderUsername: 'charlie_vlog',
  message: 'Would love to do a cooking collab!',
  status: 'Accepted',
  respondedAt: new Date().toISOString(),
};

const mockDeclinedRequest = {
  ...mockPendingRequest,
  id: 'req-3',
  senderDisplayName: 'Dana Travel',
  senderUsername: 'dana_travel',
  message: 'Travel collab opportunity',
  status: 'Declined',
  declineReason: 'Schedule conflict',
  respondedAt: new Date().toISOString(),
};

const mockSentRequest = {
  ...mockPendingRequest,
  id: 'req-4',
  senderId: 'user-2',
  recipientId: 'user-3',
  recipientDisplayName: 'Eve Music',
  recipientUsername: 'eve_music',
  message: 'Music video collab?',
  status: 'Pending',
};

function createMockListResponse(
  items: typeof mockPendingRequest[],
  totalCount?: number,
  page = 1,
  hasNextPage = false,
  hasPreviousPage = false
) {
  return {
    items,
    totalCount: totalCount ?? items.length,
    page,
    pageSize: 20,
    totalPages: Math.ceil((totalCount ?? items.length) / 20),
    hasNextPage,
    hasPreviousPage,
  };
}

/**
 * Setup API mocks for collaboration endpoints
 */
async function setupCollaborationMocks(
  page: Page,
  options: {
    inboxItems?: typeof mockPendingRequest[];
    sentItems?: typeof mockPendingRequest[];
    inboxTotal?: number;
    sentTotal?: number;
    shouldFail?: boolean;
    errorStatus?: number;
  } = {}
) {
  const {
    inboxItems = [mockPendingRequest, mockAcceptedRequest, mockDeclinedRequest],
    sentItems = [mockSentRequest],
    inboxTotal = inboxItems.length,
    sentTotal = sentItems.length,
    shouldFail = false,
    errorStatus = 500,
  } = options;

  // Mock inbox endpoint
  await page.route('**/api/collaborations/inbox**', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to load collaboration requests',
        }),
      });
      return;
    }

    const url = route.request().url();
    const statusMatch = url.match(/status=([^&]*)/);
    const statusFilter = statusMatch ? decodeURIComponent(statusMatch[1]) : null;

    let filtered = inboxItems;
    if (statusFilter) {
      filtered = inboxItems.filter((r) => r.status === statusFilter);
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        createMockListResponse(
          filtered,
          statusFilter ? filtered.length : inboxTotal
        )
      ),
    });
  });

  // Mock sent endpoint
  await page.route('**/api/collaborations/sent**', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to load sent requests',
        }),
      });
      return;
    }

    const url = route.request().url();
    const statusMatch = url.match(/status=([^&]*)/);
    const statusFilter = statusMatch ? decodeURIComponent(statusMatch[1]) : null;

    let filtered = sentItems;
    if (statusFilter) {
      filtered = sentItems.filter((r) => r.status === statusFilter);
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        createMockListResponse(
          filtered,
          statusFilter ? filtered.length : sentTotal
        )
      ),
    });
  });

  // Mock accept endpoint
  await page.route('**/api/collaborations/*/accept', async (route: Route) => {
    const url = route.request().url();
    const idMatch = url.match(/collaborations\/([^/]+)\/accept/);
    const requestId = idMatch ? idMatch[1] : 'unknown';

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        ...mockPendingRequest,
        id: requestId,
        status: 'Accepted',
        respondedAt: new Date().toISOString(),
      }),
    });
  });

  // Mock decline endpoint
  await page.route('**/api/collaborations/*/decline', async (route: Route) => {
    const url = route.request().url();
    const idMatch = url.match(/collaborations\/([^/]+)\/decline/);
    const requestId = idMatch ? idMatch[1] : 'unknown';

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        ...mockPendingRequest,
        id: requestId,
        status: 'Declined',
        respondedAt: new Date().toISOString(),
      }),
    });
  });

  // Mock send request endpoint
  await page.route('**/api/collaborations/request', async (route: Route) => {
    if (route.request().method() === 'POST') {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          ...mockPendingRequest,
          id: 'req-new',
          message: 'New collaboration request',
        }),
      });
    }
  });
}

test.describe('Collaboration Inbox - Viewing Requests', () => {
  test.beforeEach(async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');
    await expect(page.getByRole('heading', { name: /collaborations/i })).toBeVisible();
  });

  test('should display inbox tab as active by default', async ({ page }) => {
    const inboxButton = page.getByRole('button', { name: /inbox/i });
    // Default variant (not outline) means active
    await expect(inboxButton).not.toHaveClass(/outline/);
  });

  test('should display inbox request count badge', async ({ page }) => {
    // Wait for data to load - inbox has 3 requests
    await expect(page.getByText('3 requests')).toBeVisible({ timeout: 5000 });
  });

  test('should display sender information on inbox request cards', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('@alice_creator')).toBeVisible();
  });

  test('should display request message on cards', async ({ page }) => {
    await expect(
      page.getByText('Let us collaborate on a tech review video!')
    ).toBeVisible({ timeout: 5000 });
  });

  test('should display status badges on request cards', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Multiple status badges visible
    const pendingBadges = page.locator('text=Pending');
    await expect(pendingBadges.first()).toBeVisible();
  });

  test('should show accept and decline buttons for pending inbox requests', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Accept and Decline buttons should be visible for pending request
    await expect(page.getByRole('button', { name: /accept/i }).first()).toBeVisible();
    await expect(page.getByRole('button', { name: /decline/i }).first()).toBeVisible();
  });
});

test.describe('Collaboration Inbox - Tab Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');
    await expect(page.getByRole('heading', { name: /collaborations/i })).toBeVisible();
  });

  test('should switch to Sent tab and make API call', async ({ page }) => {
    // Wait for initial inbox load
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Set up listener for sent API call
    const sentRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/sent')
    );

    // Click Sent tab
    await page.getByRole('button', { name: /sent/i }).click();

    // Verify API call
    await sentRequestPromise;

    // Verify sent tab shows recipient info
    await expect(page.getByText('Eve Music')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('1 request')).toBeVisible();
  });

  test('should switch back to Inbox tab and reload data', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Switch to sent
    await page.getByRole('button', { name: /sent/i }).click();
    await expect(page.getByText('Eve Music')).toBeVisible({ timeout: 5000 });

    // Set up listener for inbox API call
    const inboxRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/inbox')
    );

    // Switch back to inbox
    await page.getByRole('button', { name: /inbox/i }).click();

    await inboxRequestPromise;

    // Inbox data should reload
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });

  test('should reset status filter when switching tabs', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Apply Pending status filter
    await page.getByRole('button', { name: /^pending$/i }).click();

    // Switch to sent tab
    const sentRequestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return url.includes('/api/collaborations/sent') && !url.includes('status=');
    });

    await page.getByRole('button', { name: /sent/i }).click();

    // Verify sent API was called without status filter
    await sentRequestPromise;
  });
});

test.describe('Collaboration Inbox - Status Filter', () => {
  test.beforeEach(async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });

  test('should filter inbox by Pending status', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/inbox') &&
      request.url().includes('status=Pending')
    );

    await page.getByRole('button', { name: /^pending$/i }).click();

    const request = await requestPromise;
    expect(request.url()).toContain('status=Pending');

    // Should show only pending request
    await expect(page.getByText('1 request')).toBeVisible({ timeout: 5000 });
  });

  test('should filter inbox by Accepted status', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/inbox') &&
      request.url().includes('status=Accepted')
    );

    await page.getByRole('button', { name: /^accepted$/i }).click();

    const request = await requestPromise;
    expect(request.url()).toContain('status=Accepted');
  });

  test('should filter inbox by Declined status', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/inbox') &&
      request.url().includes('status=Declined')
    );

    await page.getByRole('button', { name: /^declined$/i }).click();

    const request = await requestPromise;
    expect(request.url()).toContain('status=Declined');
  });

  test('should remove status filter when clicking All', async ({ page }) => {
    // First apply a filter
    await page.getByRole('button', { name: /^pending$/i }).click();
    await expect(page.getByText('1 request')).toBeVisible({ timeout: 5000 });

    // Then click All
    const requestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return url.includes('/api/collaborations/inbox') && !url.includes('status=');
    });

    await page.getByRole('button', { name: /^all$/i }).click();

    await requestPromise;

    // Should show all requests
    await expect(page.getByText('3 requests')).toBeVisible({ timeout: 5000 });
  });

  test('should reset to page 1 when changing status filter', async ({ page }) => {
    const requestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return (
        url.includes('/api/collaborations/inbox') &&
        url.includes('status=Pending') &&
        url.includes('page=1')
      );
    });

    await page.getByRole('button', { name: /^pending$/i }).click();

    const request = await requestPromise;
    expect(request.url()).toContain('page=1');
  });
});

test.describe('Collaboration Inbox - Accept Request', () => {
  test.beforeEach(async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });

  test('should make accept API call when clicking Accept button', async ({ page }) => {
    const acceptRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/') &&
      request.url().includes('/accept') &&
      request.method() === 'POST'
    );

    await page.getByRole('button', { name: /accept/i }).first().click();

    const request = await acceptRequestPromise;
    expect(request.url()).toContain('/accept');
    expect(request.method()).toBe('POST');
  });
});

test.describe('Collaboration Inbox - Decline Request', () => {
  test.beforeEach(async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });

  test('should make decline API call when clicking Decline button', async ({ page }) => {
    const declineRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/') &&
      request.url().includes('/decline') &&
      request.method() === 'POST'
    );

    await page.getByRole('button', { name: /decline/i }).first().click();

    const request = await declineRequestPromise;
    expect(request.url()).toContain('/decline');
    expect(request.method()).toBe('POST');
  });
});

test.describe('Collaboration Inbox - Pagination', () => {
  test('should show pagination when multiple pages exist', async ({ page }) => {
    const manyRequests = Array.from({ length: 20 }, (_, i) => ({
      ...mockPendingRequest,
      id: `req-${i}`,
      senderDisplayName: `Creator ${i}`,
      senderUsername: `creator_${i}`,
    }));

    await setupCollaborationMocks(page, {
      inboxItems: manyRequests,
      inboxTotal: 40,
    });

    // Override the inbox mock to include pagination flags
    await page.route('**/api/collaborations/inbox**', async (route: Route) => {
      const url = route.request().url();
      const pageMatch = url.match(/page=(\d+)/);
      const currentPage = pageMatch ? parseInt(pageMatch[1]) : 1;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: manyRequests.slice(0, 20),
          totalCount: 40,
          page: currentPage,
          pageSize: 20,
          totalPages: 2,
          hasNextPage: currentPage === 1,
          hasPreviousPage: currentPage > 1,
        }),
      });
    });

    await page.goto('/dashboard/collaborations');

    // Wait for content
    await expect(page.getByText('40 requests')).toBeVisible({ timeout: 5000 });

    // Check pagination controls
    await expect(page.getByRole('button', { name: /next/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /previous/i })).toBeDisabled();
    await expect(page.getByText('Page 1')).toBeVisible();
  });

  test('should navigate to next page', async ({ page }) => {
    await page.route('**/api/collaborations/inbox**', async (route: Route) => {
      const url = route.request().url();
      const pageMatch = url.match(/page=(\d+)/);
      const currentPage = pageMatch ? parseInt(pageMatch[1]) : 1;

      const items = [
        {
          ...mockPendingRequest,
          id: `req-page${currentPage}`,
          senderDisplayName: `Page ${currentPage} Creator`,
          senderUsername: `page${currentPage}_creator`,
        },
      ];

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items,
          totalCount: 2,
          page: currentPage,
          pageSize: 1,
          totalPages: 2,
          hasNextPage: currentPage < 2,
          hasPreviousPage: currentPage > 1,
        }),
      });
    });

    await page.goto('/dashboard/collaborations');
    await expect(page.getByText('Page 1 Creator')).toBeVisible({ timeout: 5000 });

    // Set up request listener for page 2
    const page2RequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/collaborations/inbox') &&
      request.url().includes('page=2')
    );

    // Click Next
    await page.getByRole('button', { name: /next/i }).click();

    const request = await page2RequestPromise;
    expect(request.url()).toContain('page=2');

    // Should show page 2 content
    await expect(page.getByText('Page 2 Creator')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Page 2')).toBeVisible();
  });
});

test.describe('Collaboration Inbox - Empty State', () => {
  test('should show empty inbox state when no requests', async ({ page }) => {
    await setupCollaborationMocks(page, {
      inboxItems: [],
      inboxTotal: 0,
    });

    await page.goto('/dashboard/collaborations');

    await expect(
      page.getByRole('heading', { name: /no incoming requests/i })
    ).toBeVisible({ timeout: 5000 });
    await expect(
      page.getByText(/when other creators send you collaboration proposals/i)
    ).toBeVisible();
  });

  test('should show empty sent state when no sent requests', async ({ page }) => {
    await setupCollaborationMocks(page, {
      sentItems: [],
      sentTotal: 0,
    });

    await page.goto('/dashboard/collaborations');

    // Switch to sent tab
    await page.getByRole('button', { name: /sent/i }).click();

    await expect(
      page.getByRole('heading', { name: /no sent requests/i })
    ).toBeVisible({ timeout: 5000 });
    await expect(
      page.getByText(/discover creators and send them collaboration requests/i)
    ).toBeVisible();
  });

  test('should show "No collaboration requests" text when empty', async ({ page }) => {
    await setupCollaborationMocks(page, {
      inboxItems: [],
      inboxTotal: 0,
    });

    await page.goto('/dashboard/collaborations');

    await expect(page.getByText('No collaboration requests')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Collaboration Inbox - Error Handling', () => {
  test('should display error message when API fails', async ({ page }) => {
    await setupCollaborationMocks(page, {
      shouldFail: true,
      errorStatus: 500,
    });

    await page.goto('/dashboard/collaborations');

    await expect(page.getByText(/error loading requests/i)).toBeVisible({ timeout: 5000 });
  });

  test('should display error for 503 service unavailable', async ({ page }) => {
    await setupCollaborationMocks(page, {
      shouldFail: true,
      errorStatus: 503,
    });

    await page.goto('/dashboard/collaborations');

    await expect(page.getByText(/error loading requests/i)).toBeVisible({ timeout: 5000 });
  });

  test('should recover after page reload', async ({ page }) => {
    let requestCount = 0;

    await page.route('**/api/collaborations/inbox**', async (route: Route) => {
      requestCount++;
      if (requestCount === 1) {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({
            title: 'Server Error',
            status: 500,
            detail: 'Internal server error',
          }),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(
            createMockListResponse([mockPendingRequest], 1)
          ),
        });
      }
    });

    await page.goto('/dashboard/collaborations');

    // First load fails
    await expect(page.getByText(/error loading requests/i)).toBeVisible({ timeout: 5000 });

    // Reload
    await page.reload();

    // Should show data
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Collaboration Inbox - Loading States', () => {
  test('should show skeleton loading state while fetching', async ({ page }) => {
    await page.route('**/api/collaborations/inbox**', async (route: Route) => {
      // Delay to observe loading state
      await new Promise((resolve) => setTimeout(resolve, 500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(
          createMockListResponse([mockPendingRequest], 1)
        ),
      });
    });

    await page.goto('/dashboard/collaborations');

    // Check for skeleton elements
    const skeletons = page.locator('[class*="skeleton"], .animate-pulse');
    const skeletonCount = await skeletons.count();
    expect(skeletonCount).toBeGreaterThan(0);

    // Wait for actual content
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Collaboration Inbox - Results Count', () => {
  test('should show plural "requests" for count > 1', async ({ page }) => {
    await setupCollaborationMocks(page);
    await page.goto('/dashboard/collaborations');

    await expect(page.getByText('3 requests')).toBeVisible({ timeout: 5000 });
  });

  test('should show singular "request" for count of 1', async ({ page }) => {
    await setupCollaborationMocks(page, {
      inboxItems: [mockPendingRequest],
      inboxTotal: 1,
    });
    await page.goto('/dashboard/collaborations');

    await expect(page.getByText('1 request')).toBeVisible({ timeout: 5000 });
  });

  test('should show "No collaboration requests" for count of 0', async ({ page }) => {
    await setupCollaborationMocks(page, {
      inboxItems: [],
      inboxTotal: 0,
    });
    await page.goto('/dashboard/collaborations');

    await expect(page.getByText('No collaboration requests')).toBeVisible({ timeout: 5000 });
  });
});
