import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should display hero section', async ({ page }) => {
    await page.goto('/');

    await expect(
      page.getByRole('heading', { name: /build enterprise apps/i })
    ).toBeVisible();
    await expect(page.getByText(/production-ready frontend template/i)).toBeVisible();
  });

  test('should have navigation links', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('link', { name: /sign in/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /get started/i })).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.goto('/');

    await page.getByRole('link', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/auth/login');
  });

  test('should navigate to register page', async ({ page }) => {
    await page.goto('/');

    await page.getByRole('link', { name: /get started/i }).first().click();
    await expect(page).toHaveURL('/auth/register');
  });

  test('should display feature cards', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText(/secure by default/i)).toBeVisible();
    await expect(page.getByText(/lightning fast/i)).toBeVisible();
    await expect(page.getByText(/internationalization ready/i)).toBeVisible();
  });

  test('should display tech stack section', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText(/next\.js 14/i)).toBeVisible();
    await expect(page.getByText(/react 18/i)).toBeVisible();
  });
});
