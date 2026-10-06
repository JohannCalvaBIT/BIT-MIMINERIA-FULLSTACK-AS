import { test, expect } from '@playwright/test';

test('la app carga y muestra el encabezado principal', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('h1')).toBeVisible();
});
