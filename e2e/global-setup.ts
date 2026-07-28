import { request as pwRequest } from '@playwright/test';
import { API_URL } from './helpers';

// Aquecimento da stack antes de QUALQUER teste. A primeira requisição que passa pelo
// reverse-proxy do Vite (dev server recém-subido) às vezes reseta a conexão
// (ECONNRESET) enquanto o proxy/backend terminam de aquecer. Aqui fazemos polling até
// obter uma resposta HTTP real (mesmo 401) — o que prova que proxy + backend estão de
// pé — evitando um flake de cold-start no primeiro teste.
export default async function globalSetup(): Promise<void> {
  const ctx = await pwRequest.newContext();
  const deadline = Date.now() + 90_000;
  let ultimoErro = 'sem resposta';

  try {
    while (Date.now() < deadline) {
      try {
        // /Templates exige auth → 401. Qualquer status HTTP > 0 já confirma o round-trip.
        const r = await ctx.get(`${API_URL}/Templates`);
        if (r.status() > 0) return;
      } catch (e) {
        ultimoErro = String(e);
      }
      await new Promise((res) => setTimeout(res, 2000));
    }
    throw new Error(`API não respondeu a tempo via ${API_URL}. Último erro: ${ultimoErro}`);
  } finally {
    await ctx.dispose();
  }
}
