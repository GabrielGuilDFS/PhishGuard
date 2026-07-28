import { defineConfig, devices } from '@playwright/test';

// Config da suíte E2E (Passo 13). A stack (db + backend + frontend + mailpit) sobe por
// FORA do Playwright, via docker-compose.e2e.yml — por isso NÃO há `webServer` aqui.
// Fluxo típico (ver README/e2e):
//   docker compose -f ../docker-compose.e2e.yml --env-file ../.env.e2e up -d --build
//   npx playwright test
//
// URLs configuráveis por env (defaults = homologação local). O `.env.e2e` alimenta os
// CONTAINERS; estas variáveis alimentam o Playwright que roda no HOST.
const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173';

export default defineConfig({
  testDir: '.',
  testMatch: '**/*.spec.ts',
  // Aquece proxy/backend antes de qualquer teste (evita ECONNRESET de cold-start).
  globalSetup: './global-setup.ts',
  // O ciclo espera o worker de agendamento (ciclo de 1 min) disparar os e-mails — daí
  // o timeout generoso por teste. A asserção sobre o Mailpit faz polling com backoff.
  timeout: 150_000,
  expect: { timeout: 90_000 },
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : [['list']],
  use: {
    baseURL: BASE_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
