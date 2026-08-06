import { APIRequestContext, expect } from '@playwright/test';

// ============================================================================
// Helpers da suíte E2E — semeiam, via API pública, todo o cenário do §2.1 e falam
// com o Mailpit. Mantêm o spec legível e centralizam o contrato HTTP.
// ============================================================================

// Base da API do PhishGuard. Same-origin com o app: o Vite (frontend) faz reverse-proxy
// de /api para o backend, então tudo passa por localhost:5173 (como em produção via túnel).
export const API_URL = process.env.E2E_API_URL ?? 'http://localhost:5173/api';
// API HTTP do Mailpit (UI/consulta de e-mails capturados).
export const MAILPIT_URL = process.env.E2E_MAILPIT_URL ?? 'http://localhost:8025';
// Domínio da allowlist de homologação — todo destino de teste vive aqui (§2.1d).
export const TEST_DOMAIN = process.env.E2E_TEST_DOMAIN ?? 'test.io';

/** CNPJ fictício de 14 dígitos (a coluna exige exatamente 14). */
export function cnpjAleatorio(): string {
  let s = '';
  for (let i = 0; i < 14; i++) s += Math.floor(Math.random() * 10);
  return s;
}

export interface TenantSession {
  email: string;
  senha: string;
  token: string;
  /** Cabeçalho pronto para as chamadas autenticadas. */
  authHeader: { Authorization: string };
}

/** Registra um tenant DEDICADO ao run e autentica — isolamento por execução (§2.1a). */
export async function registrarELogar(request: APIRequestContext): Promise<TenantSession> {
  const carimbo = `${Date.now()}-${Math.floor(Math.random() * 1e4)}`;
  const email = `tenant-${carimbo}@${TEST_DOMAIN}`;
  const senha = 'Test1234!';

  const reg = await request.post(`${API_URL}/auth/register`, {
    data: {
      nomeEmpresa: `E2E ${carimbo}`,
      cnpj: cnpjAleatorio(),
      nome: 'Admin E2E',
      email,
      password: senha,
      plano: 'ouro',
    },
  });
  expect(reg.ok(), `register falhou: ${reg.status()} ${await reg.text()}`).toBeTruthy();

  const login = await request.post(`${API_URL}/auth/login`, { data: { email, password: senha } });
  expect(login.ok(), `login falhou: ${login.status()} ${await login.text()}`).toBeTruthy();
  const auth = (await login.json()) as { accessToken: string };
  const token = auth.accessToken?.trim() ?? '';
  expect(token.length, 'token vazio').toBeGreaterThan(0);

  return { email, senha, token, authHeader: { Authorization: `Bearer ${token}` } };
}

/** Aponta o SMTP do tenant para o Mailpit (nunca um provedor real). */
export async function configurarSmtpMailpit(request: APIRequestContext, s: TenantSession) {
  const r = await request.put(`${API_URL}/SmtpConfig`, {
    headers: s.authHeader,
    data: {
      host: process.env.E2E_SMTP_HOST ?? 'mailpit',
      porta: Number(process.env.E2E_SMTP_PORT ?? 1025),
      usuario: `phishguard@${TEST_DOMAIN}`,
      senha: 'irrelevante-mailpit-aceita-qualquer',
    },
  });
  expect(r.ok(), `SmtpConfig falhou: ${r.status()} ${await r.text()}`).toBeTruthy();
}

interface RecursosCampanha {
  campaignId: string;
  targetId: string;
  targetEmail: string;
}

/** Cria alvo + isca + página falsa + página educativa e monta a campanha. */
export async function criarCampanhaComAlvo(
  request: APIRequestContext,
  s: TenantSession,
): Promise<RecursosCampanha> {
  const targetEmail = `vitima-${Date.now()}@${TEST_DOMAIN}`;

  const alvo = await postJson(request, `${API_URL}/Targets`, s, {
    nome: 'Vítima E2E',
    email: targetEmail,
    departamento: 'TI',
  });
  const targetId: string = alvo.id;

  // Isca de e-mail: o link de clique vai como TEXTO ({{LINK_PHISHING}}), sobrevivendo à
  // sanitização e sendo trivial de extrair do e-mail capturado pelo Mailpit.
  const template = await postJson(request, `${API_URL}/Templates`, s, {
    nome: 'Isca E2E',
    assunto: 'Ação necessária na sua conta',
    remetenteNome: 'Suporte',
    remetenteEmail: `suporte@${TEST_DOMAIN}`,
    corpoHtml: '<p>Olá {{NOME}}, acesse para verificar sua conta: {{LINK_PHISHING}}</p>',
  });

  // Página falsa: usa um molde oficial resolvido por ID (o LandingPage.tsx renderiza o
  // molde real com o formulário de captura #amz-new/#amz-confirm).
  const pagina = await postJson(request, `${API_URL}/PhishingPages`, s, {
    nome: 'Landing E2E',
    conteudoHtml: 'amazon-login',
  });

  const educativa = await postJson(request, `${API_URL}/EducationalPages`, s, {
    nome: 'Educativa E2E',
    conteudoHtml: 'basico_phishing',
  });

  // DataInicio no passado => ao ativar, a campanha entra em "Processando" e o worker
  // dispara no próximo ciclo (≤ 1 min), sem esperar horário agendado.
  const campanha = await postJson(request, `${API_URL}/Campaigns`, s, {
    nomeCampanha: `Campanha E2E ${Date.now()}`,
    dataInicio: new Date(Date.now() - 60_000).toISOString(),
    dataFim: null,
    emailTemplateId: template.id,
    landingPageId: pagina.id,
    educationalPageId: educativa.id,
    targetIds: [targetId],
  });

  return { campaignId: campanha.id, targetId, targetEmail };
}

/** Ativa (homologa) a campanha — transiciona o estado; o worker fará o disparo. */
export async function ativarCampanha(request: APIRequestContext, s: TenantSession, campaignId: string) {
  const r = await request.post(`${API_URL}/Campaigns/${campaignId}/ativar`, { headers: s.authHeader });
  expect(r.ok(), `ativar falhou: ${r.status()} ${await r.text()}`).toBeTruthy();
}

interface MailpitMessage {
  ID: string;
  To: { Address: string }[];
}

/**
 * Faz polling no Mailpit até o e-mail do alvo aparecer e devolve o corpo (texto+html).
 * O worker roda a cada 1 min; o expect.poll abaixo tolera essa latência.
 */
export async function esperarEmailNoMailpit(request: APIRequestContext, destino: string): Promise<string> {
  let messageId = '';
  await expect
    .poll(
      async () => {
        const list = await request.get(`${MAILPIT_URL}/api/v1/messages?limit=200`);
        if (!list.ok()) return false;
        const body = (await list.json()) as { messages: MailpitMessage[] };
        const msg = body.messages?.find((m) => m.To?.some((t) => t.Address.toLowerCase() === destino.toLowerCase()));
        if (msg) messageId = msg.ID;
        return Boolean(msg);
      },
      { message: `e-mail para ${destino} não chegou ao Mailpit`, timeout: 120_000, intervals: [2000] },
    )
    .toBeTruthy();

  const full = await request.get(`${MAILPIT_URL}/api/v1/message/${messageId}`);
  expect(full.ok()).toBeTruthy();
  const msg = (await full.json()) as { Text?: string; HTML?: string };
  return `${msg.Text ?? ''}\n${msg.HTML ?? ''}`;
}

/** Extrai a primeira URL que casa o padrão informado (ex.: link de clique do tracking). */
export function extrairLink(corpo: string, padrao: RegExp): string {
  const match = corpo.match(padrao);
  expect(match, `nenhum link casou ${padrao} no corpo do e-mail`).toBeTruthy();
  return match![0];
}

/** Lê o funil do dashboard do tenant: cliques e submissões (par único por alvo). */
export async function getFunnel(request: APIRequestContext, s: TenantSession): Promise<{ cliques: number; submissoes: number }> {
  const r = await request.get(`${API_URL}/Dashboard/funnel`, { headers: s.authHeader });
  expect(r.ok(), `funnel falhou: ${r.status()}`).toBeTruthy();
  return r.json();
}

// ---------------------------------------------------------------------------

async function postJson(request: APIRequestContext, url: string, s: TenantSession, data: unknown) {
  const r = await request.post(url, { headers: s.authHeader, data });
  expect(r.ok(), `POST ${url} falhou: ${r.status()} ${await r.text()}`).toBeTruthy();
  return r.json();
}
