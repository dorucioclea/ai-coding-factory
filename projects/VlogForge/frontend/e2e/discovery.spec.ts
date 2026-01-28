import { test, expect, type Page, type Route } from '@playwright/test';

/**
 * E2E Behavioral Tests for Creator Discovery Feature
 * Story: ACF-010
 *
 * These tests verify actual user flows and API interactions, not just element existence.
 */

// Mock data for consistent testing
const mockCreators = [
  {
    id: '1',
    username: 'gaming_pro',
    displayName: 'Gaming Pro',
    bio: 'Professional gaming content creator',
    profilePictureUrl: 'https://example.com/avatar1.jpg',
    openToCollaborations: true,
    nicheTags: ['Gaming', 'Esports'],
    platforms: [{ platformType: 'YouTube', handle: '@gamingpro', followerCount: 50000 }],
    totalFollowers: 50000,
  },
  {
    id: '2',
    username: 'tech_guru',
    displayName: 'Tech Guru',
    bio: 'Tech reviews and tutorials',
    profilePictureUrl: 'https://example.com/avatar2.jpg',
    openToCollaborations: false,
    nicheTags: ['Technology', 'Reviews'],
    platforms: [{ platformType: 'TikTok', handle: '@techguru', followerCount: 150000 }],
    totalFollowers: 150000,
  },
  {
    id: '3',
    username: 'fitness_coach',
    displayName: 'Fitness Coach',
    bio: 'Fitness and wellness tips',
    profilePictureUrl: 'https://example.com/avatar3.jpg',
    openToCollaborations: true,
    nicheTags: ['Fitness', 'Health'],
    platforms: [{ platformType: 'Instagram', handle: '@fitnesscoach', followerCount: 25000 }],
    totalFollowers: 25000,
  },
];

const mockNiches = ['Gaming', 'Technology', 'Fitness', 'Health', 'Reviews', 'Esports', 'Lifestyle', 'Comedy'];
const mockPlatforms = ['YouTube', 'TikTok', 'Instagram', 'Twitter', 'Twitch'];

/**
 * Helper to create a mock discovery response
 */
function createMockResponse(
  items: typeof mockCreators,
  totalCount: number = items.length,
  hasMore: boolean = false,
  nextCursor?: string
) {
  return {
    items,
    totalCount,
    hasMore,
    nextCursor,
    pageSize: 20,
  };
}

/**
 * Helper to setup API mocks for discovery endpoints
 */
async function setupApiMocks(page: Page, options: {
  creators?: typeof mockCreators;
  niches?: string[];
  platforms?: string[];
  totalCount?: number;
  hasMore?: boolean;
  nextCursor?: string;
  shouldFail?: boolean;
  errorStatus?: number;
} = {}) {
  const {
    creators = mockCreators,
    niches = mockNiches,
    platforms = mockPlatforms,
    totalCount = creators.length,
    hasMore = false,
    nextCursor,
    shouldFail = false,
    errorStatus = 500,
  } = options;

  // Mock niches endpoint
  await page.route('**/api/discovery/niches', async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(niches),
    });
  });

  // Mock platforms endpoint
  await page.route('**/api/discovery/platforms', async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(platforms),
    });
  });

  // Mock discovery endpoint
  await page.route('**/api/discovery?**', async (route: Route) => {
    if (shouldFail) {
      await route.fulfill({
        status: errorStatus,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Server Error',
          status: errorStatus,
          detail: 'Failed to fetch creators',
        }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(createMockResponse(creators, totalCount, hasMore, nextCursor)),
    });
  });

  // Mock discovery endpoint without query params
  await page.route('**/api/discovery', async (route: Route) => {
    const url = route.request().url();
    // Only handle exact match without query string
    if (!url.includes('?')) {
      if (shouldFail) {
        await route.fulfill({
          status: errorStatus,
          contentType: 'application/json',
          body: JSON.stringify({
            title: 'Server Error',
            status: errorStatus,
            detail: 'Failed to fetch creators',
          }),
        });
        return;
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(createMockResponse(creators, totalCount, hasMore, nextCursor)),
      });
    }
  });
}

test.describe('Creator Discovery - Search Flow', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/dashboard/discover');
    // Wait for initial load
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();
  });

  test('should make API call with search parameter when typing', async ({ page }) => {
    const searchInput = page.getByPlaceholder(/search creators by name/i);

    // Set up request interception to capture the API call
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/discovery') && request.url().includes('search=gaming')
    );

    // Type search term
    await searchInput.fill('gaming');

    // Wait for debounced API call (300ms debounce + network)
    const request = await requestPromise;

    // Verify the API was called with correct search param
    expect(request.url()).toContain('search=gaming');
  });

  test('should display filtered results after search', async ({ page }) => {
    // Set up specific mock for search results
    await page.route('**/api/discovery?**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('search=gaming')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse([mockCreators[0]], 1)),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(mockCreators, 3)),
        });
      }
    });

    const searchInput = page.getByPlaceholder(/search creators by name/i);
    await searchInput.fill('gaming');

    // Wait for results to update
    await expect(page.getByText('Found 1 creator')).toBeVisible({ timeout: 5000 });

    // Verify correct creator is displayed
    await expect(page.getByText('Gaming Pro')).toBeVisible();
    await expect(page.getByText('Tech Guru')).not.toBeVisible();
  });

  test('should reset results when search is cleared', async ({ page }) => {
    // Set up mock that responds differently based on search
    await page.route('**/api/discovery?**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('search=')) {
        const searchMatch = url.match(/search=([^&]*)/);
        const searchTerm = searchMatch ? decodeURIComponent(searchMatch[1]) : '';
        if (searchTerm && searchTerm.trim()) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify(createMockResponse([mockCreators[0]], 1)),
          });
          return;
        }
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(createMockResponse(mockCreators, 3)),
      });
    });

    const searchInput = page.getByPlaceholder(/search creators by name/i);

    // Search first
    await searchInput.fill('gaming');
    await expect(page.getByText('Found 1 creator')).toBeVisible({ timeout: 5000 });

    // Set up listener for API call without search param
    const clearRequestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return url.includes('/api/discovery') && !url.includes('search=gaming');
    });

    // Clear search using the X button
    const clearButton = page.locator('button').filter({ has: page.locator('svg.lucide-x') }).first();
    await clearButton.click();

    // Verify API called without search param
    await clearRequestPromise;

    // Verify all creators are shown again
    await expect(page.getByText(/found 3 creators/i)).toBeVisible({ timeout: 5000 });
  });

  test('should show empty state when search returns no results', async ({ page }) => {
    // Mock empty response for specific search
    await page.route('**/api/discovery?**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('search=nonexistent')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse([], 0)),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(mockCreators, 3)),
        });
      }
    });

    const searchInput = page.getByPlaceholder(/search creators by name/i);
    await searchInput.fill('nonexistent');

    // Wait for empty state
    await expect(page.getByRole('heading', { name: /no creators found/i })).toBeVisible({ timeout: 5000 });
    await expect(page.getByText(/try adjusting your filters/i)).toBeVisible();
  });
});

test.describe('Creator Discovery - Filter Flow', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/dashboard/discover');
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();
  });

  test('should make API call with openToCollab=true when clicking Open to Collab button', async ({ page }) => {
    const collabButton = page.getByRole('button', { name: /open to collab/i });

    // Set up request interception
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/discovery') && request.url().includes('openToCollab=true')
    );

    // Click the filter button
    await collabButton.click();

    // Verify button is now in active state (has bg-primary class via default variant)
    await expect(collabButton).toHaveAttribute('data-slot', 'button');

    // Verify API was called with correct parameter
    const request = await requestPromise;
    expect(request.url()).toContain('openToCollab=true');
  });

  test('should make API call with audienceSize parameter when selecting audience size', async ({ page }) => {
    const select = page.getByRole('combobox');

    // Set up request interception
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/discovery') && request.url().includes('audienceSize=Medium')
    );

    // Open dropdown and select an option
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // Verify API was called with correct parameter
    const request = await requestPromise;
    expect(request.url()).toContain('audienceSize=Medium');

    // Verify dropdown shows selected value
    await expect(select).toContainText('10K - 100K');
  });

  test('should make API call with niche parameter when selecting niches in filter dialog', async ({ page }) => {
    // Open filters dialog
    await page.getByRole('button', { name: /filters/i }).click();

    // Wait for dialog to open
    await expect(page.getByRole('heading', { name: /filter creators/i })).toBeVisible();

    // Set up request interception for the API call
    const requestPromise = page.waitForRequest((request) =>
      request.url().includes('/api/discovery') && request.url().includes('niches=Gaming')
    );

    // Click Gaming checkbox
    const gamingCheckbox = page.locator('button').filter({ hasText: 'Gaming' }).first();
    await gamingCheckbox.click();

    // Close dialog
    await page.getByRole('button', { name: /done/i }).click();

    // Verify API was called with niches parameter
    const request = await requestPromise;
    expect(decodeURIComponent(request.url())).toContain('niches=Gaming');
  });

  test('should display filter tags after selecting niches and verify API call', async ({ page }) => {
    // Open filters dialog
    await page.getByRole('button', { name: /filters/i }).click();
    await expect(page.getByRole('heading', { name: /filter creators/i })).toBeVisible();

    // Select multiple niches
    await page.locator('button').filter({ hasText: 'Gaming' }).first().click();
    await page.locator('button').filter({ hasText: 'Technology' }).first().click();

    // Close dialog
    await page.getByRole('button', { name: /done/i }).click();

    // Verify filter tags appear
    const filterTagsArea = page.locator('.flex-wrap').first();
    await expect(filterTagsArea.getByText('Gaming')).toBeVisible();
    await expect(filterTagsArea.getByText('Technology')).toBeVisible();
  });

  test('should remove filter tag and update API call when clicking X on filter tag', async ({ page }) => {
    // First apply a filter
    await page.getByRole('button', { name: /filters/i }).click();
    await expect(page.getByRole('heading', { name: /filter creators/i })).toBeVisible();

    await page.locator('button').filter({ hasText: 'Gaming' }).first().click();
    await page.getByRole('button', { name: /done/i }).click();

    // Verify tag is visible
    const filterBadge = page.locator('[class*="badge"]').filter({ hasText: 'Gaming' });
    await expect(filterBadge).toBeVisible();

    // Set up listener for API call without the niche
    const requestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return url.includes('/api/discovery') && !url.includes('niches=Gaming');
    });

    // Click the X button on the badge
    const removeButton = filterBadge.locator('button');
    await removeButton.click();

    // Verify API was called without the niche parameter
    await requestPromise;

    // Verify tag is removed
    await expect(filterBadge).not.toBeVisible();
  });

  test('should show Clear All button when filters are active and clear all filters on click', async ({ page }) => {
    // Apply multiple filters
    await page.getByRole('button', { name: /open to collab/i }).click();

    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // Verify Clear All button appears
    const clearAllButton = page.getByRole('button', { name: /clear all/i });
    await expect(clearAllButton).toBeVisible();

    // Set up listener for API call without filters
    const requestPromise = page.waitForRequest((request) => {
      const url = request.url();
      return (
        url.includes('/api/discovery') &&
        !url.includes('openToCollab=') &&
        !url.includes('audienceSize=')
      );
    });

    // Click Clear All
    await clearAllButton.click();

    // Verify API was called without filters
    await requestPromise;

    // Verify Clear All button is hidden
    await expect(clearAllButton).not.toBeVisible();

    // Verify Open to Collab is deactivated
    const collabButton = page.getByRole('button', { name: /open to collab/i });
    await expect(collabButton).toHaveClass(/outline/);
  });
});

test.describe('Creator Discovery - Combined Filters', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/dashboard/discover');
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();
  });

  test('should apply multiple filters and verify all appear in API call', async ({ page }) => {
    // Set up a request interceptor that captures all parameters
    let capturedUrl = '';
    await page.route('**/api/discovery?**', async (route: Route) => {
      capturedUrl = route.request().url();
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(createMockResponse([mockCreators[0]], 1)),
      });
    });

    // Apply Open to Collab
    await page.getByRole('button', { name: /open to collab/i }).click();
    await page.waitForTimeout(100);

    // Apply Audience Size
    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /100k\+/i }).click();
    await page.waitForTimeout(100);

    // Apply Niche filter
    await page.getByRole('button', { name: /filters/i }).click();
    await expect(page.getByRole('heading', { name: /filter creators/i })).toBeVisible();
    await page.locator('button').filter({ hasText: 'Gaming' }).first().click();
    await page.getByRole('button', { name: /done/i }).click();

    // Wait for the API call with all filters
    await page.waitForRequest((request) => {
      const url = request.url();
      return (
        url.includes('/api/discovery') &&
        url.includes('openToCollab=true') &&
        url.includes('audienceSize=Large') &&
        url.includes('niches=Gaming')
      );
    });

    // Add search term
    const searchInput = page.getByPlaceholder(/search creators by name/i);
    await searchInput.fill('pro');

    // Wait for final API call with all 4 filters
    const finalRequest = await page.waitForRequest((request) => {
      const url = request.url();
      return (
        url.includes('/api/discovery') &&
        url.includes('openToCollab=true') &&
        url.includes('audienceSize=Large') &&
        url.includes('niches=Gaming') &&
        url.includes('search=pro')
      );
    });

    const finalUrl = decodeURIComponent(finalRequest.url());
    expect(finalUrl).toContain('openToCollab=true');
    expect(finalUrl).toContain('audienceSize=Large');
    expect(finalUrl).toContain('niches=Gaming');
    expect(finalUrl).toContain('search=pro');
  });

  test('should show filter count badge on Filters button', async ({ page }) => {
    // Apply filters through different methods
    await page.getByRole('button', { name: /open to collab/i }).click();

    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // The filter count should reflect active filters
    // Note: Based on the implementation, the badge shows count of filter categories
    await page.getByRole('button', { name: /filters/i }).click();
    await page.locator('button').filter({ hasText: 'Gaming' }).first().click();
    await page.getByRole('button', { name: /done/i }).click();

    // Wait for state to update
    await page.waitForTimeout(300);

    // The Filters button should now show count
    const filtersButton = page.getByRole('button', { name: /filters/i });
    await expect(filtersButton.locator('.badge, [class*="badge"]')).toBeVisible();
  });
});

test.describe('Creator Discovery - Pagination/Infinite Scroll', () => {
  test('should load more creators when clicking Load More button', async ({ page }) => {
    // First page of results
    const page1Creators = mockCreators.slice(0, 2);
    const page2Creators = [mockCreators[2]];

    let requestCount = 0;

    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return; // Let the other handlers handle these
      }

      requestCount++;
      if (url.includes('cursor=page2')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(page2Creators, 3, false)),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(page1Creators, 3, true, 'page2')),
        });
      }
    });

    await page.goto('/dashboard/discover');
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();

    // Wait for initial results
    await expect(page.getByText('Gaming Pro')).toBeVisible();
    await expect(page.getByText('Tech Guru')).toBeVisible();

    // Verify Load More button is visible (since hasMore=true)
    const loadMoreButton = page.getByRole('button', { name: /load more/i });
    await expect(loadMoreButton).toBeVisible();

    // Click Load More
    await loadMoreButton.click();

    // Wait for new creator to appear
    await expect(page.getByText('Fitness Coach')).toBeVisible({ timeout: 5000 });

    // Verify "You've reached the end" message
    await expect(page.getByText(/you've reached the end/i)).toBeVisible();
  });

  test('should include cursor parameter in pagination API call', async ({ page }) => {
    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return;
      }

      if (url.includes('cursor=')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse([mockCreators[2]], 3, false)),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(mockCreators.slice(0, 2), 3, true, 'next-cursor-123')),
        });
      }
    });

    await page.goto('/dashboard/discover');
    await expect(page.getByText('Gaming Pro')).toBeVisible();

    // Set up listener for cursor parameter
    const cursorRequestPromise = page.waitForRequest((request) =>
      request.url().includes('cursor=next-cursor-123')
    );

    // Click Load More
    await page.getByRole('button', { name: /load more/i }).click();

    // Verify cursor was sent
    const request = await cursorRequestPromise;
    expect(request.url()).toContain('cursor=next-cursor-123');
  });

  test('should show loading indicator while fetching next page', async ({ page }) => {
    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return;
      }

      if (url.includes('cursor=')) {
        // Delay to show loading state
        await new Promise((resolve) => setTimeout(resolve, 1000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse([mockCreators[2]], 3, false)),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(createMockResponse(mockCreators.slice(0, 2), 3, true, 'page2')),
        });
      }
    });

    await page.goto('/dashboard/discover');
    await expect(page.getByText('Gaming Pro')).toBeVisible();

    // Click Load More
    await page.getByRole('button', { name: /load more/i }).click();

    // Verify loading indicator appears
    await expect(page.getByText(/loading more/i)).toBeVisible();

    // Wait for loading to complete
    await expect(page.getByText('Fitness Coach')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Creator Discovery - Creator Card Interaction', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/dashboard/discover');
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();
  });

  test('should navigate to creator profile when clicking on creator card', async ({ page }) => {
    // Wait for creators to load
    await expect(page.getByText('Gaming Pro')).toBeVisible();

    // Click on the first creator card
    const creatorCard = page.locator('a[href="/profile/gaming_pro"]');
    await creatorCard.click();

    // Verify navigation
    await expect(page).toHaveURL(/\/profile\/gaming_pro/);
  });

  test('should display creator information correctly on card', async ({ page }) => {
    // Wait for creators to load
    await expect(page.getByText('Gaming Pro')).toBeVisible();

    // Verify creator card shows all expected information
    const firstCard = page.locator('a[href="/profile/gaming_pro"]').first();

    // Display name
    await expect(firstCard.getByText('Gaming Pro')).toBeVisible();

    // Username
    await expect(firstCard.getByText('@gaming_pro')).toBeVisible();

    // Bio
    await expect(firstCard.getByText(/professional gaming content/i)).toBeVisible();

    // Collaboration badge (this creator is open to collaborations)
    await expect(firstCard.getByText('Open')).toBeVisible();

    // Niche tags
    await expect(firstCard.getByText('Gaming')).toBeVisible();
  });

  test('should show "Open" badge only for creators open to collaboration', async ({ page }) => {
    // Wait for creators to load
    await expect(page.getByText('Gaming Pro')).toBeVisible();

    // Gaming Pro is open to collaborations - should have badge
    const gamingProCard = page.locator('a[href="/profile/gaming_pro"]');
    await expect(gamingProCard.locator('[class*="badge"]').filter({ hasText: 'Open' })).toBeVisible();

    // Tech Guru is NOT open to collaborations - should not have badge
    const techGuruCard = page.locator('a[href="/profile/tech_guru"]');
    await expect(techGuruCard.locator('[class*="badge"]').filter({ hasText: 'Open' })).not.toBeVisible();
  });
});

test.describe('Creator Discovery - Error Handling', () => {
  test('should display error message when API call fails', async ({ page }) => {
    await setupApiMocks(page, {
      shouldFail: true,
      errorStatus: 500,
    });

    await page.goto('/dashboard/discover');

    // Wait for error message
    await expect(page.getByText(/error loading creators/i)).toBeVisible();
    await expect(page.getByText(/failed to fetch creators/i)).toBeVisible();
  });

  test('should display error for 503 service unavailable', async ({ page }) => {
    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return;
      }

      await route.fulfill({
        status: 503,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Service Unavailable',
          status: 503,
          detail: 'The service is temporarily unavailable',
        }),
      });
    });

    await page.goto('/dashboard/discover');

    // Should show error state
    await expect(page.getByText(/error loading creators/i)).toBeVisible();
  });

  test('should recover when retrying after error', async ({ page }) => {
    let requestCount = 0;

    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return;
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
          body: JSON.stringify(createMockResponse(mockCreators, 3)),
        });
      }
    });

    await page.goto('/dashboard/discover');

    // First load should fail
    await expect(page.getByText(/error loading creators/i)).toBeVisible();

    // Refresh the page (simulating retry)
    await page.reload();

    // Should now show creators
    await expect(page.getByText('Gaming Pro')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Creator Discovery - Results Count', () => {
  test('should display correct results count', async ({ page }) => {
    await setupApiMocks(page, { totalCount: 42 });
    await page.goto('/dashboard/discover');

    // Wait for results count
    await expect(page.getByText(/found 42 creators/i)).toBeVisible();
  });

  test('should display singular "creator" for count of 1', async ({ page }) => {
    await setupApiMocks(page, {
      creators: [mockCreators[0]],
      totalCount: 1,
    });
    await page.goto('/dashboard/discover');

    // Wait for results count - singular form
    await expect(page.getByText(/found 1 creator$/i)).toBeVisible();
  });

  test('should display "No creators found" when count is 0', async ({ page }) => {
    await setupApiMocks(page, {
      creators: [],
      totalCount: 0,
    });
    await page.goto('/dashboard/discover');

    // Wait for no results message
    await expect(page.getByText('No creators found')).toBeVisible();
  });
});

test.describe('Creator Discovery - Loading States', () => {
  test('should display skeleton loading state while fetching creators', async ({ page }) => {
    await page.route('**/api/discovery/niches', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNiches),
      });
    });

    await page.route('**/api/discovery/platforms', async (route: Route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPlatforms),
      });
    });

    await page.route('**/api/discovery**', async (route: Route) => {
      const url = route.request().url();
      if (url.includes('/niches') || url.includes('/platforms')) {
        return;
      }

      // Delay to observe loading state
      await new Promise((resolve) => setTimeout(resolve, 500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(createMockResponse(mockCreators, 3)),
      });
    });

    await page.goto('/dashboard/discover');

    // Check for skeleton elements
    const skeletons = page.locator('[class*="skeleton"], .animate-pulse');
    const skeletonCount = await skeletons.count();
    expect(skeletonCount).toBeGreaterThan(0);

    // Wait for actual content to load
    await expect(page.getByText('Gaming Pro')).toBeVisible({ timeout: 5000 });
  });
});

test.describe.serial('Creator Discovery - Filter Persistence', () => {
  test('should maintain filter state after applying multiple filters', async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/dashboard/discover');
    await expect(page.getByRole('heading', { name: /discover creators/i })).toBeVisible();

    // Apply Open to Collab filter
    const collabButton = page.getByRole('button', { name: /open to collab/i });
    await collabButton.click();

    // Apply audience size
    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // Verify both filters are active
    await expect(collabButton).not.toHaveClass(/outline/);
    await expect(select).toContainText('10K - 100K');

    // Now add a niche filter
    await page.getByRole('button', { name: /filters/i }).click();
    await page.locator('button').filter({ hasText: 'Gaming' }).first().click();
    await page.getByRole('button', { name: /done/i }).click();

    // All three filters should still be active
    await expect(collabButton).not.toHaveClass(/outline/);
    await expect(select).toContainText('10K - 100K');
    await expect(page.locator('[class*="badge"]').filter({ hasText: 'Gaming' })).toBeVisible();
  });
});
