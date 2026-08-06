import { expect, test } from '@playwright/test';
import { registrarELogar } from './helpers';

test('mantém a sessão administrativa após recarregar a página', async ({ page }) => {
  // page.request compartilha o cookie jar do BrowserContext. O login cria o cookie
  // HttpOnly; nenhum access token é injetado no storage ou no JavaScript da página.
  await registrarELogar(page.request);

  await page.goto('/admin/dashboard');
  await expect(page).toHaveURL(/\/admin\/dashboard$/);
  await expect(page.getByRole('heading', { name: /visão geral de segurança/i })).toBeVisible();

  await page.reload();

  await expect(page).toHaveURL(/\/admin\/dashboard$/);
  await expect(page.getByRole('heading', { name: /visão geral de segurança/i })).toBeVisible();
});
