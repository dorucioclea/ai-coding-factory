import { test, expect, type Page, type Route } from '@playwright/test';

/**
 * E2E Behavioral Tests for Messaging Feature
 * Story: ACF-012
 *
 * Tests verify user flows for conversation list, message thread,
 * sending messages, marking as read, navigation, and error handling.
 */

// Mock data
const mockConversation = {
  id: 'conv-1',
  participantId: 'user-2',
  participantDisplayName: 'Alice Creator',
  participantUsername: 'alice_creator',
  participantProfilePictureUrl: 'https://example.com/alice.jpg',
  lastMessagePreview: 'Hey, want to collaborate?',
  lastMessageAt: new Date().toISOString(),
  unreadCount: 3,
  createdAt: new Date(Date.now() - 60000).toISOString(),
};

const mockConversationNoUnread = {
  ...mockConversation,
  id: 'conv-2',
  participantDisplayName: 'Bob Vlogger',
  participantUsername: 'bob_vlogger',
  participantProfilePictureUrl: 'https://example.com/bob.jpg',
  lastMessagePreview: 'Thanks for the video!',
  unreadCount: 0,
};

const mockMessage1 = {
  id: 'msg-1',
  conversationId: 'conv-1',
  senderId: 'user-1', // current user (sent)
  senderDisplayName: 'Current User',
  senderUsername: 'current_user',
  content: 'Hey Alice!',
  isRead: true,
  readAt: new Date().toISOString(),
  createdAt: new Date(Date.now() - 120000).toISOString(),
};

const mockMessage2 = {
  id: 'msg-2',
  conversationId: 'conv-1',
  senderId: 'user-2', // other user (received)
  senderDisplayName: 'Alice Creator',
  senderUsername: 'alice_creator',
  content: 'Hey, want to collaborate?',
  isRead: false,
  createdAt: new Date(Date.now() - 60000).toISOString(),
};

function createConversationListResponse(
  items: typeof mockConversation[],
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

function createMessageListResponse(
  items: typeof mockMessage1[],
  totalCount?: number,
  page = 1,
  hasNextPage = false,
  hasPreviousPage = false
) {
  return {
    items,
    totalCount: totalCount ?? items.length,
    page,
    pageSize: 50,
    totalPages: Math.ceil((totalCount ?? items.length) / 50),
    hasNextPage,
    hasPreviousPage,
  };
}

/**
 * Setup API mocks for messaging endpoints
 */
async function setupMessagingMocks(
  page: Page,
  options: {
    conversations?: typeof mockConversation[];
    messages?: typeof mockMessage1[];
    totalConversations?: number;
    totalMessages?: number;
    shouldFail?: boolean;
    errorStatus?: number;
  } = {}
) {
  const {
    conversations = [mockConversation, mockConversationNoUnread],
    messages = [mockMessage1, mockMessage2],
    totalConversations = conversations.length,
    totalMessages = messages.length,
    shouldFail = false,
    errorStatus = 500,
  } = options;

  // Mock GET /api/conversations
  await page.route('**/api/conversations', async (route: Route) => {
    if (route.request().method() !== 'GET') {
      return route.fallback();
    }

    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to load conversations',
        }),
      });
      return;
    }

    const url = route.request().url();
    const pageMatch = url.match(/page=(\d+)/);
    const currentPage = pageMatch ? parseInt(pageMatch[1]) : 1;

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        createConversationListResponse(
          conversations,
          totalConversations,
          currentPage
        )
      ),
    });
  });

  // Mock GET /api/conversations/unread-count
  await page.route('**/api/conversations/unread-count', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to load unread count',
        }),
      });
      return;
    }

    const totalUnread = conversations.reduce(
      (sum, c) => sum + (c.unreadCount ?? 0),
      0
    );

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ unreadCount: totalUnread }),
    });
  });

  // Mock GET /api/conversations/*/messages
  await page.route('**/api/conversations/*/messages', async (route: Route) => {
    if (route.request().method() !== 'GET') {
      return route.fallback();
    }

    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to load messages',
        }),
      });
      return;
    }

    const url = route.request().url();
    const pageMatch = url.match(/page=(\d+)/);
    const currentPage = pageMatch ? parseInt(pageMatch[1]) : 1;

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        createMessageListResponse(
          messages,
          totalMessages,
          currentPage
        )
      ),
    });
  });

  // Mock POST /api/conversations/*/messages (send message)
  await page.route('**/api/conversations/*/messages', async (route: Route) => {
    if (route.request().method() !== 'POST') {
      return route.fallback();
    }

    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to send message',
        }),
      });
      return;
    }

    const body = route.request().postDataJSON();

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 'msg-new',
        conversationId: 'conv-1',
        senderId: 'user-1',
        senderDisplayName: 'Current User',
        senderUsername: 'current_user',
        content: body?.content ?? '',
        isRead: false,
        createdAt: new Date().toISOString(),
      }),
    });
  });

  // Mock POST /api/conversations/*/read (mark as read)
  await page.route('**/api/conversations/*/read', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to mark as read',
        }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock POST /api/conversations (start conversation)
  await page.route('**/api/conversations', async (route: Route) => {
    if (route.request().method() !== 'POST') {
      return route.fallback();
    }

    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to start conversation',
        }),
      });
      return;
    }

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        ...mockConversation,
        id: 'conv-new',
      }),
    });
  });
}

test.describe('Messaging - Conversation List', () => {
  test.beforeEach(async ({ page }) => {
    await setupMessagingMocks(page);
    await page.goto('/dashboard/messages');
    await expect(page.getByRole('heading', { name: /messages/i })).toBeVisible();
  });

  test('should display conversations', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Bob Vlogger')).toBeVisible({ timeout: 5000 });
  });

  test('should show unread count badge on conversations with unread messages', async ({ page }) => {
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Alice has unreadCount: 3, should display a badge
    const unreadBadge = page.locator('text=3').first();
    await expect(unreadBadge).toBeVisible();
  });

  test('should show last message preview', async ({ page }) => {
    await expect(
      page.getByText('Hey, want to collaborate?')
    ).toBeVisible({ timeout: 5000 });
    await expect(
      page.getByText('Thanks for the video!')
    ).toBeVisible({ timeout: 5000 });
  });

  test('should show empty state when no conversations', async ({ page }) => {
    await page.route('**/api/conversations', async (route: Route) => {
      if (route.request().method() !== 'GET') {
        return route.fallback();
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(
          createConversationListResponse([], 0)
        ),
      });
    });

    await page.goto('/dashboard/messages');

    await expect(
      page.getByText(/no conversations|no messages/i)
    ).toBeVisible({ timeout: 5000 });
  });

  test('should show loading skeletons', async ({ page }) => {
    await page.route('**/api/conversations', async (route: Route) => {
      if (route.request().method() !== 'GET') {
        return route.fallback();
      }
      // Delay to observe loading state
      await new Promise((resolve) => setTimeout(resolve, 500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(
          createConversationListResponse([mockConversation], 1)
        ),
      });
    });

    await page.goto('/dashboard/messages');

    // Check for skeleton elements
    const skeletons = page.locator('[class*="skeleton"], .animate-pulse');
    const skeletonCount = await skeletons.count();
    expect(skeletonCount).toBeGreaterThan(0);

    // Wait for actual content
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });

  test('should show error message when API fails', async ({ page }) => {
    await page.route('**/api/conversations', async (route: Route) => {
      if (route.request().method() !== 'GET') {
        return route.fallback();
      }
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: 500,
          detail: 'Failed to load conversations',
        }),
      });
    });

    await page.goto('/dashboard/messages');

    await expect(
      page.getByText(/error loading|failed to load|something went wrong/i)
    ).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Messaging - Message Thread', () => {
  test.beforeEach(async ({ page }) => {
    await setupMessagingMocks(page);
  });

  test('should display messages when navigating to conversation', async ({ page }) => {
    await page.goto('/dashboard/messages/conv-1');

    await expect(page.getByText('Hey Alice!')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Hey, want to collaborate?')).toBeVisible({ timeout: 5000 });
  });

  test('should show message input', async ({ page }) => {
    await page.goto('/dashboard/messages/conv-1');

    await expect(
      page.getByPlaceholderText(/type a message/i)
    ).toBeVisible({ timeout: 5000 });
  });

  test('should send message via API when submitting', async ({ page }) => {
    await page.goto('/dashboard/messages/conv-1');

    // Wait for messages to load
    await expect(page.getByText('Hey Alice!')).toBeVisible({ timeout: 5000 });

    // Type and send a message
    const messageInput = page.getByPlaceholderText(/type a message/i);
    await messageInput.fill('Great, let us do it!');

    // Set up request listener for send message API call
    const sendRequestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/conversations/conv-1/messages') &&
      request.method() === 'POST'
    );

    await page.getByRole('button', { name: /send/i }).click();

    const request = await sendRequestPromise;
    expect(request.method()).toBe('POST');

    const postData = request.postDataJSON();
    expect(postData.content).toBe('Great, let us do it!');
  });

  test('should mark messages as read on entry', async ({ page }) => {
    // Set up request listener for mark as read API call
    const markReadPromise = page.waitForRequest((request) =>
      request.url().includes('/api/conversations/conv-1/read') &&
      request.method() === 'POST'
    );

    await page.goto('/dashboard/messages/conv-1');

    const request = await markReadPromise;
    expect(request.url()).toContain('/api/conversations/conv-1/read');
    expect(request.method()).toBe('POST');
  });
});

test.describe('Messaging - Navigation', () => {
  test('should navigate to message thread when clicking conversation', async ({ page }) => {
    await setupMessagingMocks(page);
    await page.goto('/dashboard/messages');

    // Wait for conversations to load
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });

    // Click on Alice's conversation
    await page.getByText('Alice Creator').click();

    // Verify navigation to message thread
    await expect(page).toHaveURL(/\/dashboard\/messages\/conv-1/);

    // Verify messages load
    await expect(page.getByText('Hey Alice!')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Messaging - Pagination', () => {
  test('should show load more/pagination for conversations', async ({ page }) => {
    const manyConversations = Array.from({ length: 20 }, (_, i) => ({
      ...mockConversation,
      id: `conv-${i}`,
      participantDisplayName: `Creator ${i}`,
      participantUsername: `creator_${i}`,
      lastMessagePreview: `Message preview ${i}`,
      unreadCount: i % 3 === 0 ? 1 : 0,
    }));

    await page.route('**/api/conversations', async (route: Route) => {
      if (route.request().method() !== 'GET') {
        return route.fallback();
      }

      const url = route.request().url();
      const pageMatch = url.match(/page=(\d+)/);
      const currentPage = pageMatch ? parseInt(pageMatch[1]) : 1;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: manyConversations.slice(0, 20),
          totalCount: 40,
          page: currentPage,
          pageSize: 20,
          totalPages: 2,
          hasNextPage: currentPage === 1,
          hasPreviousPage: currentPage > 1,
        }),
      });
    });

    // Mock unread count
    await page.route('**/api/conversations/unread-count', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ unreadCount: 7 }),
      });
    });

    await page.goto('/dashboard/messages');

    // Wait for content to load
    await expect(page.getByText('Creator 0')).toBeVisible({ timeout: 5000 });

    // Check pagination controls
    await expect(page.getByRole('button', { name: /next/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /previous/i })).toBeDisabled();
  });
});

test.describe('Messaging - Error Handling', () => {
  test('should display error when conversations API fails', async ({ page }) => {
    await setupMessagingMocks(page, {
      shouldFail: true,
      errorStatus: 500,
    });

    await page.goto('/dashboard/messages');

    await expect(
      page.getByText(/error loading|failed to load|something went wrong/i)
    ).toBeVisible({ timeout: 5000 });
  });

  test('should recover after page reload', async ({ page }) => {
    let requestCount = 0;

    await page.route('**/api/conversations', async (route: Route) => {
      if (route.request().method() !== 'GET') {
        return route.fallback();
      }

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
            createConversationListResponse([mockConversation], 1)
          ),
        });
      }
    });

    // Mock unread count
    await page.route('**/api/conversations/unread-count', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ unreadCount: 3 }),
      });
    });

    await page.goto('/dashboard/messages');

    // First load fails
    await expect(
      page.getByText(/error loading|failed to load|something went wrong/i)
    ).toBeVisible({ timeout: 5000 });

    // Reload
    await page.reload();

    // Should show data
    await expect(page.getByText('Alice Creator')).toBeVisible({ timeout: 5000 });
  });
});
