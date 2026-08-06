import { API_BASE } from '../config';

/** Chave legada, mantida apenas para remover tokens persistidos por versões anteriores. */
export const TOKEN_KEY = 'phishguard_token';
const GUID_VAZIO = '00000000-0000-0000-0000-000000000000';

let accessToken: string | null = null;
let refreshInFlight: Promise<string | null> | null = null;

interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
}

interface JwtPayload {
  tenant_id?: string;
  sid?: string;
  exp?: number;
  [claim: string]: unknown;
}

export function getToken(): string | null {
  return accessToken;
}

/** O access token existe somente na memória desta aba. */
export function setToken(token: string): void {
  clearLegacyStorage();
  accessToken = token;
}

export function clearSession(): void {
  accessToken = null;
  clearLegacyStorage();
}

function clearLegacyStorage(): void {
  localStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    return JSON.parse(atob(padded)) as JwtPayload;
  } catch {
    return null;
  }
}

export interface SessaoValidada {
  valida: boolean;
  tenantId?: string;
}

export function validateSession(): SessaoValidada {
  const token = accessToken;
  if (!token) return { valida: false };
  const payload = decodeJwtPayload(token);
  if (!payload) return { valida: false };
  if (!payload.tenant_id || payload.tenant_id === GUID_VAZIO || !payload.sid) return { valida: false };
  if (typeof payload.exp !== 'number' || payload.exp * 1000 <= Date.now()) return { valida: false };
  return { valida: true, tenantId: payload.tenant_id };
}

export function refreshSession(): Promise<string | null> {
  if (refreshInFlight) return refreshInFlight;

  refreshInFlight = fetch(`${API_BASE}/Auth/refresh`, {
    method: 'POST',
    credentials: 'include',
  })
    .then(async (response) => {
      if (!response.ok) {
        clearSession();
        return null;
      }
      const payload = await response.json() as AuthResponse;
      setToken(payload.accessToken);
      return payload.accessToken;
    })
    .catch(() => {
      clearSession();
      return null;
    })
    .finally(() => {
      refreshInFlight = null;
    });

  return refreshInFlight;
}

export async function restoreSession(): Promise<boolean> {
  if (validateSession().valida) return true;
  return (await refreshSession()) !== null;
}

export async function logoutSession(): Promise<void> {
  try {
    await fetch(`${API_BASE}/Auth/logout`, {
      method: 'POST',
      credentials: 'include',
    });
  } finally {
    clearSession();
  }
}

/** Fetch autenticado com renovação única e deduplicada após expiração/401. */
export async function authFetch(input: RequestInfo | URL, init: RequestInit = {}): Promise<Response> {
  let token = validateSession().valida ? getToken() : await refreshSession();
  if (!token) return new Response(null, { status: 401 });

  const execute = (bearer: string) => {
    const headers = new Headers(init.headers);
    headers.set('Authorization', `Bearer ${bearer}`);
    return fetch(input, { ...init, headers, credentials: 'include' });
  };

  let response = await execute(token);
  if (response.status !== 401) return response;

  clearSession();
  token = await refreshSession();
  if (!token) return response;
  response = await execute(token);
  return response;
}
