import { test, expect } from '@playwright/test';
import {
  ativarCampanha,
  configurarSmtpMailpit,
  criarCampanhaComAlvo,
  esperarEmailNoMailpit,
  extrairLink,
  getFunnel,
  registrarELogar,
} from './helpers';

// ============================================================================
// Ciclo de vida completo de uma campanha (§2.1) — ponta a ponta, sem e-mail real.
//
//   criar campanha → worker dispara → Mailpit captura → alvo clica → landing →
//   submete credenciais falsas → tela educacional → conclusão auditada → métricas.
//
// Isolamento (§2.1): tenant dedicado por run; SMTP sempre no Mailpit; PublicAppBaseUrl
// no host de teste; allowlist @test.io no backend garante que nada escape mesmo se o
// SMTP for trocado por engano.
// ============================================================================

test('ciclo de vida completo de uma campanha (§2.1)', async ({ page, request }) => {
  // 1. Tenant de teste + SMTP → Mailpit + campanha com 1 alvo, e ativa.
  const sessao = await registrarELogar(request);
  await configurarSmtpMailpit(request, sessao);
  const { campaignId, targetId, targetEmail } = await criarCampanhaComAlvo(request, sessao);
  await ativarCampanha(request, sessao, campaignId);

  // 2. O worker dispara e o Mailpit captura (nenhum e-mail sai para a internet).
  const corpo = await esperarEmailNoMailpit(request, targetEmail);
  const linkClique = extrairLink(corpo, new RegExp(`https?://[^\\s"']+/api/tracking/click/${campaignId}/${targetId}`));

  // 3. O alvo "clica": o backend loga o Clique e redireciona para a página falsa.
  await page.goto(linkClique);
  await expect(page).toHaveURL(/\/landing\//);

  // A página falsa (molde amazon-login) renderiza o formulário de captura.
  await expect(page.locator('#amz-new')).toBeVisible();

  // 4. O alvo submete "credenciais" — só metadados trafegam; a senha NUNCA (LGPD).
  const senhaDigitada = 'SenhaSecreta#123';
  await page.fill('#amz-new', senhaDigitada);
  await page.fill('#amz-confirm', senhaDigitada);

  const [submit] = await Promise.all([
    page.waitForRequest((r) => r.url().includes(`/api/tracking/submit/${campaignId}/${targetId}`) && r.method() === 'POST'),
    page.click('button[type=submit]'),
  ]);
  expect(submit.postData() ?? '', 'a senha real vazou no payload de submissão').not.toContain(senhaDigitada);

  // 5. A tela educacional (Just-in-Time Training) aparece e a conclusão é auditada.
  await expect(page).toHaveURL(/educational-feedback/);
  const [completeResp] = await Promise.all([
    page.waitForResponse((r) => r.url().includes(`/api/tracking/complete/${campaignId}/${targetId}`) && r.request().method() === 'POST'),
    page.getByRole('button', { name: /concluir treinamento/i }).click(),
  ]);
  expect(completeResp.ok(), 'a conclusão do treinamento não foi registrada').toBeTruthy();

  // 6. Métrica do tenant: exatamente 1 Clique + 1 Submissão (par único por alvo).
  await expect
    .poll(async () => await getFunnel(request, sessao), { timeout: 30_000, intervals: [1000] })
    .toMatchObject({ cliques: 1, submissoes: 1 });
});
