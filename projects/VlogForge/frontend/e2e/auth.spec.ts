import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('should display login page', async ({ page }) => {
    await page.goto('/auth/login');

    await expect(page.getByRole('heading', { name: /sign in/i })).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });

  test('should display registration page', async ({ page }) => {
    await page.goto('/auth/register');

    await expect(
      page.getByRole('heading', { name: /create an account/i })
    ).toBeVisible();
    await expect(page.getByLabel(/first name/i)).toBeVisible();
    await expect(page.getByLabel(/last name/i)).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
  });

  test('should navigate from login to register', async ({ page }) => {
    await page.goto('/auth/login');

    await page.getByRole('link', { name: /create one/i }).click();
    await expect(page).toHaveURL('/auth/register');
  });

  test('should navigate from register to login', async ({ page }) => {
    await page.goto('/auth/register');

    await page.getByRole('link', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/auth/login');
  });

  test('should show validation errors on empty login submit', async ({
    page,
  }) => {
    await page.goto('/auth/login');

    await page.getByRole('button', { name: /sign in/i }).click();

    await expect(page.getByText(/email is required/i)).toBeVisible();
    await expect(page.getByText(/password is required/i)).toBeVisible();
  });

  test('should show email validation error for invalid email', async ({
    page,
  }) => {
    await page.goto('/auth/login');

    await page.getByLabel(/email/i).fill('invalid-email');
    await page.getByLabel(/password/i).fill('password123');
    await page.getByRole('button', { name: /sign in/i }).click();

    await expect(
      page.getByText(/please enter a valid email/i)
    ).toBeVisible();
  });

  test('should display forgot password page', async ({ page }) => {
    await page.goto('/auth/forgot-password');

    await expect(
      page.getByRole('heading', { name: /reset your password/i })
    ).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(
      page.getByRole('button', { name: /send reset link/i })
    ).toBeVisible();
  });
});
