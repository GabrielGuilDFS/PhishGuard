import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import PrivateRoute from './PrivateRoute';
import { clearSession, getToken, setToken, TOKEN_KEY } from './session';
import { jwtDeTeste } from '../test/jwt';

const TENANT = '11111111-1111-1111-1111-111111111111';
const AMANHA = Math.floor(Date.now() / 1000) + 86400;
const ONTEM = Math.floor(Date.now() / 1000) - 86400;

function renderProtegido() {
  return render(
    <MemoryRouter initialEntries={['/admin']}>
      <Routes>
        <Route path="/login" element={<div>sonda-login</div>} />
        <Route path="/admin" element={<PrivateRoute><div>sonda-painel</div></PrivateRoute>} />
      </Routes>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  clearSession();
  localStorage.clear();
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(null, { status: 401 }));
});

afterEach(() => {
  clearSession();
  vi.restoreAllMocks();
});

describe('PrivateRoute', () => {
  it('renderiza o painel com access token íntegro', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT, exp: AMANHA }));
    renderProtegido();
    expect(screen.getByText('sonda-painel')).toBeInTheDocument();
  });

  it.each([
    ['sem token', null],
    ['token expirado', jwtDeTeste({ tenant_id: TENANT, exp: ONTEM })],
    ['token sem tenant', jwtDeTeste({ tenant_id: '', exp: AMANHA })],
  ])('%s: tenta restaurar pelo cookie e redireciona ao login se falhar', async (_caso, token) => {
    if (token) setToken(token);
    renderProtegido();

    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(screen.queryByText('sonda-painel')).not.toBeInTheDocument();
    expect(getToken()).toBeNull();
  });

  it('ignora e remove bearer legado do localStorage', async () => {
    localStorage.setItem(TOKEN_KEY, 'token-legado');
    renderProtegido();
    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
  });
});
