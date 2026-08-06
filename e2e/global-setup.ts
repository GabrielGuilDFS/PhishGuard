import { request as pwRequest } from '@playwright/test';

const READY_URL = process.env.E2E_READY_URL ?? 'http://localhost:5000/health/ready';
const APP_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173';

// Aguarda a prontidão real da API. Em banco vazio, o processo só publica este endpoint
// depois de aplicar todas as migrations. Aceitar qualquer resposta do proxy mascarava
// o HTTP 500 recebido enquanto o backend ainda estava inicializando.
export default async function globalSetup(): Promise<void> {
  const ctx = await pwRequest.newContext();
  const deadline = Date.now() + 120_000;
  let ultimoErro = 'sem resposta';

  try {
    while (Date.now() < deadline) {
      try {
        const readyResponse = await ctx.get(READY_URL);
        if (readyResponse.status() !== 200) {
          ultimoErro = `readiness HTTP ${readyResponse.status()}`;
          await new Promise((resolve) => setTimeout(resolve, 2_000));
          continue;
        }

        const frontendResponse = await ctx.get(APP_URL);
        if (frontendResponse.status() !== 200) {
          ultimoErro = `frontend HTTP ${frontendResponse.status()}`;
          await new Promise((resolve) => setTimeout(resolve, 2_000));
          continue;
        }

        // A rota protegida deve atravessar o proxy Vite e chegar à API. O 401 é o
        // resultado saudável esperado sem Bearer; 500 indicaria proxy ainda indisponível.
        const proxyResponse = await ctx.get(`${APP_URL}/api/Templates`);
        if (proxyResponse.status() === 401) return;
        ultimoErro = `proxy HTTP ${proxyResponse.status()}: ${await proxyResponse.text()}`;
      } catch (error) {
        ultimoErro = String(error);
      }

      await new Promise((resolve) => setTimeout(resolve, 2_000));
    }

    throw new Error(`API não ficou pronta via ${READY_URL}. Último erro: ${ultimoErro}`);
  } finally {
    await ctx.dispose();
  }
}
