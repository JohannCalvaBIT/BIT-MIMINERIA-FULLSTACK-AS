import { defineConfig } from '@playwright/test';

// G-TEST-01: E2E solo para flujos críticos de negocio, no para cada pantalla.
// Este smoke test es el punto de partida — agrega specs en tests/ a medida
// que el proyecto crezca (login, checkout, etc.), sin invertir la pirámide.
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
  },
  webServer: {
    command: 'pnpm start',
    cwd: '../../src/apps/web/frontend',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
