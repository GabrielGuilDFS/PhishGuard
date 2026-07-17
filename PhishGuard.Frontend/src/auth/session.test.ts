import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
// Teste co-located (padrão de espelhamento do frontend).
import {
  TOKEN_KEY,
  getToken,
  setToken,
  clearSession,
  decodeJwtPayload,
  validateSession,
} from './session';
import { jwtDeTeste } from '../test/jwt';

const TENANT_A = '11111111-1111-1111-1111-111111111111';
const DAQUI_1_DIA = Math.floor(Date.now() / 1000) + 86400;
const ONTEM = Math.floor(Date.now() / 1000) - 86400;

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
});

afterEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  vi.restoreAllMocks();
});

describe('clearSession — destruição completa da sessão residual', () => {
  it('expurga o token do localStorage e do sessionStorage', () => {
    localStorage.setItem(TOKEN_KEY, 'token-da-conta-antiga');
    sessionStorage.setItem(TOKEN_KEY, 'residuo-em-session-storage');

    clearSession();

    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });

  it('expira os cookies do domínio (best-effort)', () => {
    document.cookie = 'phishguard_legacy=abc; path=/';
    clearSession();
    expect(document.cookie).not.toContain('phishguard_legacy=abc');
  });

  it('preserva preferências de UI que não identificam o usuário (ex.: tema)', () => {
    localStorage.setItem(TOKEN_KEY, 'token-da-conta-antiga');
    localStorage.setItem('phishguard_theme_mode', 'dark');

    clearSession();

    expect(localStorage.getItem('phishguard_theme_mode')).toBe('dark');
  });
});

describe('setToken — novo login nunca herda resíduo', () => {
  it('purga a sessão anterior antes de gravar o novo token', () => {
    sessionStorage.setItem(TOKEN_KEY, 'residuo-antigo');
    localStorage.setItem(TOKEN_KEY, 'token-da-conta-antiga');

    setToken('token-da-conta-nova');

    expect(getToken()).toBe('token-da-conta-nova');
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });
});

describe('decodeJwtPayload — leitura tolerante a lixo', () => {
  it('decodifica um payload base64url válido', () => {
    const token = jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA });
    expect(decodeJwtPayload(token)).toMatchObject({ tenant_id: TENANT_A });
  });

  it.each([
    ['string vazia', ''],
    ['sem os 3 segmentos', 'abc.def'],
    ['payload que não é base64', 'a.###não-base64###.c'],
    ['payload que não é JSON', `a.${btoa('não é json')}.c`],
  ])('retorna null para token malformado (%s), sem lançar', (_caso, token) => {
    expect(decodeJwtPayload(token)).toBeNull();
  });
});

describe('validateSession — barreira de integridade do PrivateRoute', () => {
  it('aprova um token com tenant_id válido e exp no futuro, expondo o escopo', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA }));
    expect(validateSession()).toEqual({ valida: true, tenantId: TENANT_A });
  });

  it('reprova quando não há token', () => {
    expect(validateSession().valida).toBe(false);
  });

  it('reprova token ilegível', () => {
    localStorage.setItem(TOKEN_KEY, 'lixo-que-não-é-jwt');
    expect(validateSession().valida).toBe(false);
  });

  it('reprova token SEM claim tenant_id (sem escopo de dados, sem painel)', () => {
    setToken(jwtDeTeste({ exp: DAQUI_1_DIA }));
    expect(validateSession().valida).toBe(false);
  });

  it('reprova tenant_id com Guid vazio (token forjado/corrompido)', () => {
    setToken(jwtDeTeste({ tenant_id: '00000000-0000-0000-0000-000000000000', exp: DAQUI_1_DIA }));
    expect(validateSession().valida).toBe(false);
  });

  it('reprova token expirado', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT_A, exp: ONTEM }));
    expect(validateSession().valida).toBe(false);
  });

  it('reprova token sem exp (o backend sempre emite com expiração)', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT_A }));
    expect(validateSession().valida).toBe(false);
  });
});
