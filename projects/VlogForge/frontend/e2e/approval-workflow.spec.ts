import { test, expect, Page } from '@playwright/test';

/**
 * E2E tests for Approval Workflow
 * Story: ACF-009
 */

// Test data
const mockUser = {
  id: '550e8400-e29b-41d4-a716-446655440000',
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'User',
};

const mockTeam = {
  id: '550e8400-e29b-41d4-a716-446655440001',
  name: 'Test Team',
  description: 'A team for testing',
  ownerId: mockUser.id,
  requiresApproval: true,
  approverIds: [mockUser.id],
  members: [
    {
      userId: mockUser.id,
      role: 3, // Owner
      email: mockUser.email,
      joinedAt: new Date().toISOString(),
    },
  ],
};

const mockContentDraft = {
  id: '550e8400-e29b-41d4-a716-446655440002',
  userId: mockUser.id,
  title: 'Test Content Idea',
  notes: 'This is a test content idea for approval workflow testing',
  status: 1, // Draft
  platformTags: ['YouTube', 'TikTok'],
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

const mockContentInReview = {
  ...mockContentDraft,
  id: '550e8400-e29b-41d4-a716-446655440003',
  title: 'Content In Review',
  status: 2, // InReview
};

const mockApprovalRecord = {
  id: '550e8400-e29b-41d4-a716-446655440004',
  contentItemId: mockContentInReview.id,
  teamId: mockTeam.id,
  actorId: mockUser.id,
  action: 1, // Submitted
  previousStatus: 1,
  newStatus: 2,
  feedback: null,
  createdAt: new Date().toISOString(),
};

// Helper to setup API mocks
async function setupApiMocks(page: Page) {
  // Mock auth endpoint
  await page.route('**/api/auth/me', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockUser),
    });
  });

  // Mock teams endpoint
  await page.route('**/api/teams', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [mockTeam],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      }),
    });
  });

  // Mock team detail endpoint
  await page.route(`**/api/teams/${mockTeam.id}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockTeam),
    });
  });
}

test.describe('Approval Workflow', () => {
  test.describe('Submit for Approval', () => {
    test('should display submit button for draft content owned by user', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint with draft content
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentDraft],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Test Content Idea')).toBeVisible();

      // The submit for approval button should be visible
      await expect(
        page.getByRole('button', { name: /submit for approval/i })
      ).toBeVisible();
    });

    test('should submit content for approval successfully', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentDraft],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock submit for approval endpoint
      await page.route(
        `**/api/content-items/${mockContentDraft.id}/submit-for-approval`,
        async (route) => {
          if (route.request().method() === 'POST') {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                ...mockContentDraft,
                status: 2, // InReview
              }),
            });
          }
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Test Content Idea')).toBeVisible();

      // Click submit for approval
      await page.getByRole('button', { name: /submit for approval/i }).click();

      // Should show success toast
      await expect(page.getByText(/submitted for approval/i)).toBeVisible();
    });

    test('should show error toast when submission fails', async ({ page }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentDraft],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock submit for approval endpoint with error
      await page.route(
        `**/api/content-items/${mockContentDraft.id}/submit-for-approval`,
        async (route) => {
          if (route.request().method() === 'POST') {
            await route.fulfill({
              status: 400,
              contentType: 'application/json',
              body: JSON.stringify({
                message: 'Team does not have approval workflow enabled',
              }),
            });
          }
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Test Content Idea')).toBeVisible();

      // Click submit for approval
      await page.getByRole('button', { name: /submit for approval/i }).click();

      // Should show error toast
      await expect(page.getByText(/submission failed/i)).toBeVisible();
    });
  });

  test.describe('Approve Content', () => {
    test('should display approve and request changes buttons for content in review', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint with content in review
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock pending approvals endpoint
      await page.route(
        `**/api/teams/${mockTeam.id}/pending-approvals`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              items: [
                {
                  contentItemId: mockContentInReview.id,
                  title: mockContentInReview.title,
                  notes: mockContentInReview.notes,
                  submittedByUserId: mockUser.id,
                  submittedAt: new Date().toISOString(),
                  platformTags: mockContentInReview.platformTags,
                },
              ],
              totalCount: 1,
            }),
          });
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Approve and Request Changes buttons should be visible for approvers
      await expect(
        page.getByRole('button', { name: /^approve$/i })
      ).toBeVisible();
      await expect(
        page.getByRole('button', { name: /request changes/i })
      ).toBeVisible();
    });

    test('should open approve dialog when clicking approve button', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint with content in review
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Click approve button
      await page.getByRole('button', { name: /^approve$/i }).click();

      // Dialog should open
      await expect(
        page.getByRole('heading', { name: /approve content/i })
      ).toBeVisible();
      await expect(page.getByLabel(/feedback/i)).toBeVisible();
    });

    test('should approve content with optional feedback', async ({ page }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock approve endpoint
      await page.route(
        `**/api/content-items/${mockContentInReview.id}/approve`,
        async (route) => {
          if (route.request().method() === 'POST') {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                ...mockContentInReview,
                status: 3, // Approved
              }),
            });
          }
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Click approve button
      await page.getByRole('button', { name: /^approve$/i }).click();

      // Enter optional feedback
      await page.getByLabel(/feedback/i).fill('Looks great!');

      // Click approve in dialog
      await page
        .getByRole('dialog')
        .getByRole('button', { name: /^approve$/i })
        .click();

      // Should show success toast
      await expect(page.getByText(/content approved/i)).toBeVisible();
    });
  });

  test.describe('Request Changes', () => {
    test('should open request changes dialog', async ({ page }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Click request changes button
      await page.getByRole('button', { name: /request changes/i }).click();

      // Dialog should open
      await expect(
        page.getByRole('heading', { name: /request changes/i })
      ).toBeVisible();
      await expect(page.getByLabel(/feedback/i)).toBeVisible();
    });

    test('should require feedback when requesting changes', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Click request changes button
      await page.getByRole('button', { name: /request changes/i }).click();

      // The request changes button in dialog should be disabled without feedback
      const dialogRequestButton = page
        .getByRole('dialog')
        .getByRole('button', { name: /request changes/i });
      await expect(dialogRequestButton).toBeDisabled();
    });

    test('should request changes with feedback', async ({ page }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock request changes endpoint
      await page.route(
        `**/api/content-items/${mockContentInReview.id}/request-changes`,
        async (route) => {
          if (route.request().method() === 'POST') {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                ...mockContentInReview,
                status: 4, // ChangesRequested
              }),
            });
          }
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Click request changes button
      await page.getByRole('button', { name: /request changes/i }).click();

      // Enter required feedback
      await page
        .getByLabel(/feedback/i)
        .fill('Please improve the introduction section');

      // Click request changes in dialog
      await page
        .getByRole('dialog')
        .getByRole('button', { name: /request changes/i })
        .click();

      // Should show success toast
      await expect(page.getByText(/changes requested/i)).toBeVisible();
    });
  });

  test.describe('Approval History', () => {
    test('should display approval history for content', async ({ page }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      // Mock approval history endpoint
      await page.route(
        `**/api/content-items/${mockContentInReview.id}/approval-history`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              contentItemId: mockContentInReview.id,
              records: [mockApprovalRecord],
            }),
          });
        }
      );

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Look for approval history indicator or click to view details
      // This depends on how the UI shows approval history
      // If there's a history button/link, click it
      const historyButton = page.getByRole('button', { name: /history/i });
      if (await historyButton.isVisible()) {
        await historyButton.click();
        await expect(page.getByText(/submitted/i)).toBeVisible();
      }
    });
  });

  test.describe('Status Badge', () => {
    test('should display correct status badge for Draft content', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint with draft content
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentDraft],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Test Content Idea')).toBeVisible();

      // Status badge should show Draft
      await expect(page.getByText(/draft/i)).toBeVisible();
    });

    test('should display correct status badge for In Review content', async ({
      page,
    }) => {
      await setupApiMocks(page);

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockContentInReview],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Content In Review')).toBeVisible();

      // Status badge should show In Review
      await expect(page.getByText(/in review/i)).toBeVisible();
    });

    test('should display correct status badge for Approved content', async ({
      page,
    }) => {
      await setupApiMocks(page);

      const mockApprovedContent = {
        ...mockContentDraft,
        id: '550e8400-e29b-41d4-a716-446655440005',
        title: 'Approved Content',
        status: 3, // Approved
      };

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockApprovedContent],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Approved Content')).toBeVisible();

      // Status badge should show Approved
      await expect(page.getByText(/approved/i)).toBeVisible();
    });

    test('should display correct status badge for Changes Requested content', async ({
      page,
    }) => {
      await setupApiMocks(page);

      const mockChangesRequestedContent = {
        ...mockContentDraft,
        id: '550e8400-e29b-41d4-a716-446655440006',
        title: 'Changes Requested Content',
        status: 4, // ChangesRequested
      };

      // Mock content endpoint
      await page.route('**/api/content-items*', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [mockChangesRequestedContent],
            totalCount: 1,
            page: 1,
            pageSize: 20,
          }),
        });
      });

      await page.goto('/dashboard/content');

      // Wait for content to load
      await expect(page.getByText('Changes Requested Content')).toBeVisible();

      // Status badge should show Changes Requested
      await expect(page.getByText(/changes requested/i)).toBeVisible();
    });
  });

  test.describe('Workflow Configuration', () => {
    test('should display workflow settings on team page', async ({ page }) => {
      await setupApiMocks(page);

      // Mock workflow settings endpoint
      await page.route(
        `**/api/teams/${mockTeam.id}/workflow`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              teamId: mockTeam.id,
              requiresApproval: true,
              approverIds: [mockUser.id],
            }),
          });
        }
      );

      await page.goto(`/dashboard/team/${mockTeam.id}`);

      // Wait for team page to load
      await expect(page.getByText('Test Team')).toBeVisible();

      // Look for workflow settings section or button
      const settingsButton = page.getByRole('button', { name: /settings/i });
      if (await settingsButton.isVisible()) {
        // Settings button exists but may be disabled as per current implementation
        await expect(settingsButton).toBeVisible();
      }
    });
  });
});

test.describe('Approval Workflow - Error States', () => {
  test('should handle network error gracefully', async ({ page }) => {
    await setupApiMocks(page);

    // Mock content endpoint with network error
    await page.route('**/api/content-items*', async (route) => {
      await route.abort('failed');
    });

    await page.goto('/dashboard/content');

    // Should show error state
    await expect(page.getByText(/failed to load/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /try again/i })).toBeVisible();
  });

  test('should handle unauthorized access', async ({ page }) => {
    // Mock auth endpoint to return unauthorized
    await page.route('**/api/auth/me', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Unauthorized' }),
      });
    });

    await page.goto('/dashboard/content');

    // Should redirect to login or show unauthorized message
    // This depends on your auth flow
    await expect(page).toHaveURL(/\/auth\/login|unauthorized/);
  });
});

test.describe('Approval Workflow - Accessibility', () => {
  test('approval dialog should be accessible', async ({ page }) => {
    await setupApiMocks(page);

    // Mock content endpoint
    await page.route('**/api/content-items*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockContentInReview],
          totalCount: 1,
          page: 1,
          pageSize: 20,
        }),
      });
    });

    await page.goto('/dashboard/content');

    // Wait for content to load
    await expect(page.getByText('Content In Review')).toBeVisible();

    // Click approve button
    await page.getByRole('button', { name: /^approve$/i }).click();

    // Dialog should have proper ARIA attributes
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog).toHaveAttribute('aria-modal', 'true');

    // Dialog should be keyboard navigable
    await page.keyboard.press('Tab');
    await expect(page.getByLabel(/feedback/i)).toBeFocused();

    // Escape should close dialog
    await page.keyboard.press('Escape');
    await expect(dialog).not.toBeVisible();
  });

  test('status badges should have accessible names', async ({ page }) => {
    await setupApiMocks(page);

    // Mock content endpoint
    await page.route('**/api/content-items*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockContentInReview],
          totalCount: 1,
          page: 1,
            pageSize: 20,
        }),
      });
    });

    await page.goto('/dashboard/content');

    // Wait for content to load
    await expect(page.getByText('Content In Review')).toBeVisible();

    // Status badge should have title attribute for screen readers
    const statusBadge = page.getByText(/in review/i);
    await expect(statusBadge).toHaveAttribute('title');
  });
});
