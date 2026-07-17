/**
 * Sessão de autenticação — fonte ÚNICA de leitura/escrita/destruição do JWT.
 *
 * Contexto de segurança (bug de isolamento corrigido): `/api/Auth/register` cria a
 * conta mas NÃO emite token. Antes, o fluxo landing → cadastro → checkout desembocava
 * em `/admin/dashboard` com o token RESIDUAL da conta anterior ainda no localStorage —
 * o PrivateRoute aceitava esse token velho e o painel renderizava os dados do OUTRO
 * tenant. Pior: o checkout anexava esse Bearer residual ao POST /api/Tenants/ativar,
 * ativando o plano no tenant errado (escrita cross-tenant).
 *
 * As garantias deste módulo:
 *  1. `clearSession()` — destruição completa (localStorage + sessionStorage + cookies).
 *     Chamada ao ENTRAR no fluxo de nova conta (Register/Checkout montam limpando) e
 *     no logout. Depois dela, não existe caminho para o painel sem um login explícito.
 *  2. `setToken()` — purga a sessão anterior ANTES de gravar a nova: um login nunca
 *     herda resíduo (storage ou estado) do usuário anterior do mesmo navegador.
 *  3. `validateSession()` — barreira de integridade consumida pelo PrivateRoute:
 *     token ilegível, sem claim `tenant_id` (ou com Guid vazio) ou expirado é
 *     REVOGADO na hora (purge) e o acesso ao painel é negado.
 *
 * Não há AuthContext/Redux neste app: o estado global de autenticação É o storage
 * (cada tela lê o token por requisição), então purgar o storage reseta o "estado
 * global" por definição — nenhum dado da conta anterior sobrevive em memória entre
 * rotas além do que o React desmonta na navegação.
 */

export const TOKEN_KEY = 'phishguard_token';

/** Guid zerado — um `tenant_id` assim significa token forjado/corrompido. */
const GUID_VAZIO = '00000000-0000-0000-0000-000000000000';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

/** Grava um novo token DEPOIS de purgar qualquer sessão anterior (nunca herda resíduo). */
export function setToken(token: string): void {
  clearSession();
  localStorage.setItem(TOKEN_KEY, token);
}

/**
 * Destrói a sessão por completo: token no localStorage, qualquer chave de auth em
 * sessionStorage e cookies do domínio (best-effort — o JWT vive no localStorage, mas
 * a limpeza de cookies cobre resíduos de versões/futuras integrações).
 * Preferências de UI (ex.: tema) são preservadas: não identificam o usuário.
 */
export function clearSession(): void {
  localStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
  for (const cookie of document.cookie.split(';')) {
    const nome = cookie.split('=')[0]?.trim();
    if (nome) {
      document.cookie = `${nome}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
    }
  }
}

interface JwtPayload {
  tenant_id?: string;
  exp?: number;
  [claim: string]: unknown;
}

/** Decodifica o payload de um JWT (base64url) sem validar assinatura — a assinatura
 *  é responsabilidade do backend; aqui só extraímos claims para a barreira de UI. */
export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const partes = token.split('.');
    if (partes.length !== 3) return null;
    const base64 = partes[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64)) as JwtPayload;
  } catch {
    return null;
  }
}

export interface SessaoValidada {
  valida: boolean;
  /** Presente apenas quando `valida` — escopo de dados que o painel pode requisitar. */
  tenantId?: string;
}

/**
 * Barreira de integridade da sessão (consumida pelo PrivateRoute):
 *  - sem token → inválida;
 *  - token ilegível/malformado → inválida;
 *  - sem claim `tenant_id`, ou com Guid vazio → inválida (não há escopo de dados);
 *  - `exp` ausente ou vencido → inválida (o backend sempre emite com expiração).
 * Retorna o `tenantId` para quem precisar conferir o escopo dos dados requisitados.
 */
export function validateSession(): SessaoValidada {
  const token = getToken();
  if (!token) return { valida: false };

  const payload = decodeJwtPayload(token);
  if (!payload) return { valida: false };

  const tenantId = payload.tenant_id;
  if (!tenantId || tenantId === GUID_VAZIO) return { valida: false };

  if (typeof payload.exp !== 'number' || payload.exp * 1000 <= Date.now()) {
    return { valida: false };
  }

  return { valida: true, tenantId };
}
