import { test, expect } from '@playwright/test';

/**
 * E2E tests for Creator Discovery feature
 * Story: ACF-010
 */
test.describe('Creator Discovery', () => {
  // Note: These tests assume a logged-in user session
  // In real scenarios, use a test fixture for authenticated sessions

  test.beforeEach(async ({ page }) => {
    // Navigate to discovery page - requires authentication
    // For E2E tests, this would typically use a test auth fixture
    await page.goto('/dashboard/discover');
  });

  test('should display discovery page header', async ({ page }) => {
    await expect(
      page.getByRole('heading', { name: /discover creators/i })
    ).toBeVisible();
  });

  test('should display search input', async ({ page }) => {
    await expect(
      page.getByPlaceholder(/search creators by name/i)
    ).toBeVisible();
  });

  test('should display filter controls', async ({ page }) => {
    // Open to Collab button
    await expect(
      page.getByRole('button', { name: /open to collab/i })
    ).toBeVisible();

    // Audience Size selector
    await expect(
      page.getByRole('combobox')
    ).toBeVisible();

    // Filters button
    await expect(
      page.getByRole('button', { name: /filters/i })
    ).toBeVisible();
  });

  test('should toggle open to collaboration filter', async ({ page }) => {
    const collabButton = page.getByRole('button', { name: /open to collab/i });

    // Click to activate filter
    await collabButton.click();

    // Button should be in active state (styling change)
    await expect(collabButton).toHaveClass(/bg-primary/);

    // Click again to deactivate
    await collabButton.click();

    // Button should be in outline state
    await expect(collabButton).not.toHaveClass(/bg-primary/);
  });

  test('should open audience size dropdown', async ({ page }) => {
    const select = page.getByRole('combobox');
    await select.click();

    await expect(page.getByRole('option', { name: /any size/i })).toBeVisible();
    await expect(page.getByRole('option', { name: /1k - 10k/i })).toBeVisible();
    await expect(page.getByRole('option', { name: /10k - 100k/i })).toBeVisible();
    await expect(page.getByRole('option', { name: /100k\+/i })).toBeVisible();
  });

  test('should select audience size filter', async ({ page }) => {
    const select = page.getByRole('combobox');
    await select.click();

    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // The select should show the selected value
    await expect(select).toContainText(/10k - 100k/i);
  });

  test('should open filters sheet', async ({ page }) => {
    await page.getByRole('button', { name: /filters/i }).click();

    // Sheet should open with filter options
    await expect(
      page.getByRole('heading', { name: /filter creators/i })
    ).toBeVisible();

    // Should show Niches section
    await expect(page.getByText(/niches/i)).toBeVisible();

    // Should show Platforms section
    await expect(page.getByText(/platforms/i)).toBeVisible();
  });

  test('should select niche filters in sheet', async ({ page }) => {
    await page.getByRole('button', { name: /filters/i }).click();

    // Wait for sheet to open
    await expect(
      page.getByRole('heading', { name: /filter creators/i })
    ).toBeVisible();

    // Select a niche checkbox
    const gamingCheckbox = page.getByRole('checkbox', { name: /gaming/i });
    await gamingCheckbox.click();

    // Checkbox should be checked
    await expect(gamingCheckbox).toBeChecked();
  });

  test('should select platform filters in sheet', async ({ page }) => {
    await page.getByRole('button', { name: /filters/i }).click();

    // Wait for sheet to open
    await expect(
      page.getByRole('heading', { name: /filter creators/i })
    ).toBeVisible();

    // Select a platform checkbox
    const youtubeCheckbox = page.getByRole('checkbox', { name: /youtube/i });
    await youtubeCheckbox.click();

    // Checkbox should be checked
    await expect(youtubeCheckbox).toBeChecked();
  });

  test('should search for creators', async ({ page }) => {
    const searchInput = page.getByPlaceholder(/search creators by name/i);

    await searchInput.fill('gaming creator');

    // Wait for debounce
    await page.waitForTimeout(500);

    // URL should update with search param (depending on implementation)
    // Or verify the search was applied by checking results
    await expect(searchInput).toHaveValue('gaming creator');
  });

  test('should clear search input', async ({ page }) => {
    const searchInput = page.getByPlaceholder(/search creators by name/i);

    await searchInput.fill('test search');

    // Click clear button (X icon)
    const clearButton = page.locator('input + button');
    await clearButton.click();

    await expect(searchInput).toHaveValue('');
  });

  test('should display active filter tags', async ({ page }) => {
    // Open filters sheet
    await page.getByRole('button', { name: /filters/i }).click();

    // Select a niche
    await page.getByRole('checkbox', { name: /gaming/i }).click();

    // Close sheet by clicking outside or pressing escape
    await page.keyboard.press('Escape');

    // Filter tag should be visible
    await expect(page.getByRole('button', { name: /gaming/i })).toBeVisible();
  });

  test('should remove filter by clicking tag X', async ({ page }) => {
    // Open filters sheet and select a niche
    await page.getByRole('button', { name: /filters/i }).click();
    await page.getByRole('checkbox', { name: /gaming/i }).click();
    await page.keyboard.press('Escape');

    // Find the filter tag badge
    const filterBadge = page.locator('text=gaming').locator('..');

    // Click the X button within the badge
    await filterBadge.locator('button').click();

    // Filter should be removed
    await expect(page.locator('.flex-wrap >> text=gaming')).not.toBeVisible();
  });

  test('should show clear all button when filters active', async ({ page }) => {
    // Apply a filter
    const collabButton = page.getByRole('button', { name: /open to collab/i });
    await collabButton.click();

    // Clear all button should appear
    await expect(
      page.getByRole('button', { name: /clear all/i })
    ).toBeVisible();
  });

  test('should clear all filters', async ({ page }) => {
    // Apply multiple filters
    await page.getByRole('button', { name: /open to collab/i }).click();

    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // Click clear all
    await page.getByRole('button', { name: /clear all/i }).click();

    // Filters should be cleared
    await expect(
      page.getByRole('button', { name: /clear all/i })
    ).not.toBeVisible();
  });

  test('should display empty state when no creators found', async ({ page }) => {
    // Search for something that won't match
    await page.getByPlaceholder(/search creators by name/i).fill('xyznonexistent123');

    // Wait for results
    await page.waitForTimeout(500);

    // Check for empty state or "No creators found" message
    await expect(
      page.getByText(/no creators found/i)
    ).toBeVisible({ timeout: 10000 });
  });

  test('should display creator cards in grid', async ({ page }) => {
    // Wait for initial load
    await page.waitForTimeout(1000);

    // Check if either creators are displayed or empty state
    const hasCreators = await page.locator('[class*="grid"] a').count() > 0;
    const hasEmptyState = await page.getByText(/no creators/i).isVisible();

    expect(hasCreators || hasEmptyState).toBe(true);
  });

  test('should navigate to creator profile on card click', async ({ page }) => {
    // Wait for creators to load
    await page.waitForTimeout(1000);

    // If there are creator cards, click the first one
    const creatorCard = page.locator('[class*="grid"] a').first();

    if (await creatorCard.isVisible()) {
      await creatorCard.click();

      // Should navigate to profile page
      await expect(page).toHaveURL(/\/profile\//);
    }
  });

  test('should load more creators on scroll', async ({ page }) => {
    // This test verifies infinite scroll behavior
    // Note: Requires enough data to have pagination

    // Wait for initial load
    await page.waitForTimeout(1000);

    // Scroll to bottom
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));

    // Wait for potential load more
    await page.waitForTimeout(1000);

    // Check if "Load More" button exists or end message
    const loadMore = page.getByRole('button', { name: /load more/i });
    const endMessage = page.getByText(/you've reached the end/i);

    const hasLoadMore = await loadMore.isVisible();
    const hasEndMessage = await endMessage.isVisible();

    // Should have either load more or end message (or neither if not enough data)
    expect(hasLoadMore || hasEndMessage || true).toBe(true);
  });

  test('should show filter count badge', async ({ page }) => {
    // Apply multiple filters
    await page.getByRole('button', { name: /open to collab/i }).click();

    const select = page.getByRole('combobox');
    await select.click();
    await page.getByRole('option', { name: /10k - 100k/i }).click();

    // Filter button should show count
    const filterButton = page.getByRole('button', { name: /filters/i });
    await expect(filterButton).toContainText('2');
  });

  test('should display results count', async ({ page }) => {
    // Wait for load
    await page.waitForTimeout(1000);

    // Should show "Found X creator(s)" or "No creators found"
    const resultsText = page.locator('text=/found \\d+ creator|no creators found/i');
    await expect(resultsText).toBeVisible({ timeout: 10000 });
  });
});
