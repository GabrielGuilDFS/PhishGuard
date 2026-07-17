import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
// Teste co-located (padrão de espelhamento do frontend).
import PrivateRoute from './PrivateRoute';
import { TOKEN_KEY, setToken } from './session';
import { jwtDeTeste } from '../test/jwt';

const TENANT_A = '11111111-1111-1111-1111-111111111111';
const DAQUI_1_DIA = Math.floor(Date.now() / 1000) + 86400;
const ONTEM = Math.floor(Date.now() / 1000) - 86400;

function renderProtegido() {
  return render(
    <MemoryRouter initialEntries={['/admin']}>
      <Routes>
        <Route path="/login" element={<div>sonda-login</div>} />
        <Route
          path="/admin"
          element={
            <PrivateRoute>
              <div>sonda-painel</div>
            </PrivateRoute>
          }
        />
      </Routes>
    </MemoryRouter>
  );
}

beforeEach(() => {
  localStorage.clear();
});

afterEach(() => {
  localStorage.clear();
});

describe('PrivateRoute — barreira de integridade da sessão', () => {
  it('renderiza o painel com sessão íntegra (tenant_id + exp válidos)', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT_A, exp: DAQUI_1_DIA }));
    renderProtegido();
    expect(screen.getByText('sonda-painel')).toBeInTheDocument();
  });

  it('sem token: redireciona ao login', () => {
    renderProtegido();
    expect(screen.getByText('sonda-login')).toBeInTheDocument();
    expect(screen.queryByText('sonda-painel')).not.toBeInTheDocument();
  });

  // O cenário do bug original: um token de conta ANTERIOR sobrevive no storage, mas
  // está podre (expirado). Antes, "existe token" bastava para abrir o painel — e as
  // telas requisitavam a API no escopo do tenant antigo.
  it('token expirado: bloqueia o painel, REVOGA a sessão e manda ao login', () => {
    setToken(jwtDeTeste({ tenant_id: TENANT_A, exp: ONTEM }));
    renderProtegido();

    expect(screen.getByText('sonda-login')).toBeInTheDocument();
    // Revogação: o token reprovado não fica no storage esperando a próxima navegação.
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
  });

  it('token sem claim tenant_id: bloqueia, revoga e manda ao login', () => {
    setToken(jwtDeTeste({ exp: DAQUI_1_DIA }));
    renderProtegido();

    expect(screen.getByText('sonda-login')).toBeInTheDocument();
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
  });

  it('token ilegível (lixo no storage): bloqueia, revoga e manda ao login', () => {
    localStorage.setItem(TOKEN_KEY, 'não-é-um-jwt');
    renderProtegido();

    expect(screen.getByText('sonda-login')).toBeInTheDocument();
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
  });
});
