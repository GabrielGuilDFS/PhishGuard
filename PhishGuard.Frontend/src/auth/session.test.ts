import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  TOKEN_KEY,
  authFetch,
  clearSession,
  decodeJwtPayload,
  getToken,
  refreshSession,
  setToken,
  validateSession,
} from './session';
import { jwtDeTeste } from '../test/jwt';

const TENANT_A = '11111111-1111-1111-1111-111111111111';
const DAQUI_1_DIA = Math.floor(Date.now() / 1000) + 86400;
const ONTEM = Math.floor(Date.now() / 1000) - 86400;

beforeEach(() => {
  clearSession();
  localStorage.clear();
  sessionStorage.clear();
});

afterEach(() => {
  clearSession();
  vi.restoreAllMocks();
});

describe('armazenamento seguro da sessão', () => {
  it('remove tokens legados sem apagar preferências de UI', () => {
    localStorage.setItem(TOKEN_KEY, 'token-antigo');
    sessionStorage.setItem(TOKEN_KEY, 'residuo');
    localStorage.setItem('phishguard_theme_mode', 'dark');

    clearSession();

    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(localStorage.getItem('phishguard_theme_mode')).toBe('dark');
  });

  it('mantém o access token somente em memória', () => {
    setToken('token-novo');

    expect(getToken()).toBe('token-novo');
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });
});

describe('decodeJwtPayload e validateSession', () => {
  it('aprova tenant, sessão e expiração válidos', () => {
    const token = jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA });
    expect(decodeJwtPayload(token)).toMatchObject({ tenant_id: TENANT_A, sid: 'sessao-de-teste' });
    setToken(token);
    expect(validateSession()).toEqual({ valida: true, tenantId: TENANT_A });
  });

  it.each([
    ['sem token', null],
    ['token ilegível', 'lixo'],
    ['sem tenant', jwtDeTeste({ exp: DAQUI_1_DIA })],
    ['tenant vazio', jwtDeTeste({ tenant_id: '00000000-0000-0000-0000-000000000000', exp: DAQUI_1_DIA })],
    ['expirado', jwtDeTeste({ tenant_id: TENANT_A, exp: ONTEM })],
    ['sem expiração', jwtDeTeste({ tenant_id: TENANT_A })],
    ['sem sessão', jwtDeTeste({ sid: '', tenant_id: TENANT_A, exp: DAQUI_1_DIA })],
  ])('reprova %s', (_cenario, token) => {
    if (token) setToken(token);
    expect(validateSession().valida).toBe(false);
  });

  it.each(['', 'abc.def', 'a.###nao-base64###.c', `a.${btoa('nao-json')}.c`])(
    'decodifica lixo como null sem lançar (%s)',
    (token) => expect(decodeJwtPayload(token)).toBeNull(),
  );
});

describe('refreshSession e authFetch', () => {
  it('restaura via cookie HttpOnly sem persistir o bearer', async () => {
    const token = jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA });
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(new Response(
      JSON.stringify({ accessToken: token, expiresAtUtc: new Date().toISOString() }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    ));

    expect(await refreshSession()).toBe(token);
    expect(getToken()).toBe(token);
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining('/auth/refresh'), {
      method: 'POST',
      credentials: 'include',
    });
  });

  it('renova após 401 e repete a chamada com o novo bearer', async () => {
    const antigo = jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA });
    const novo = jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA + 60 });
    setToken(antigo);
    const fetchMock = vi.spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(
        JSON.stringify({ accessToken: novo, expiresAtUtc: new Date().toISOString() }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ))
      .mockResolvedValueOnce(new Response('{}', { status: 200 }));

    expect((await authFetch('/api/protegida')).status).toBe(200);
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(new Headers(fetchMock.mock.calls[2][1]?.headers).get('Authorization')).toBe(`Bearer ${novo}`);
  });
});
