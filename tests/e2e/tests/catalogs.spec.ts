import { test, expect } from '@playwright/test';

// G-TEST-01: E2E solo para flujos críticos. No ejecutar en sandbox sin backend/SQL.
test('flujo completo de catálogo: crear empresa, buscar, editar y eliminar', async ({ page }) => {
  test.skip(true, 'Requiere backend + SQL Server');

  await page.goto('/security/catalogs');
  await page.getByLabel('Código').fill('EMP001');
  await page.getByLabel('Descripción').fill('Mi Minería S.A.');
  await page.getByRole('button', { name: 'Guardar' }).click();

  await page.getByPlaceholder('Buscar por código o descripción').fill('EMP');
  await expect(page.getByText('EMP001')).toBeVisible();

  await page.getByRole('button', { name: 'Editar código' }).click();
  await page.getByLabel('Descripción').fill('Mi Minería Actualizada');
  await page.getByRole('button', { name: 'Guardar' }).click();

  await page.getByRole('button', { name: 'Eliminar' }).click();
  await page.getByRole('button', { name: 'Eliminar' }).click();
});
