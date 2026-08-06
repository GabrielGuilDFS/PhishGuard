import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Outlet, useLocation } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { clearSession, setToken } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

const TENANT = '11111111-1111-1111-1111-111111111111';
const AMANHA = Math.floor(Date.now() / 1000) + 86400;

function LocalizacaoAtual() {
  const location = useLocation();
  return <output aria-label="localização atual">{location.pathname}</output>;
}

function AdminLayoutSonda() {
  return <Outlet />;
}

function abrirEm(caminho: string) {
  return render(
    <MemoryRouter initialEntries={[caminho]}>
      <AppRoutes
        home={<div>sonda-home</div>}
        login={<div>sonda-login</div>}
        register={<div>sonda-register</div>}
        checkout={<div>sonda-checkout</div>}
        landing={<div>sonda-landing-publica</div>}
        educationalFeedback={<div>sonda-feedback-publico</div>}
        adminLayout={<AdminLayoutSonda />}
        dashboard={<div>sonda-dashboard</div>}
        targets={<div>sonda-targets</div>}
        campaigns={<div>sonda-campaigns</div>}
        templates={<div>sonda-templates</div>}
        settings={<div>sonda-settings</div>}
      />
      <LocalizacaoAtual />
    </MemoryRouter>,
  );
}

beforeEach(() => {
  clearSession();
  localStorage.clear();
  sessionStorage.clear();
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(null, { status: 401 }));
});

afterEach(() => {
  clearSession();
  vi.restoreAllMocks();
});

describe('AppRoutes — acesso direto e fallbacks', () => {
  it('redireciona uma URL global desconhecida para o login', async () => {
    abrirEm('/pagina-que-nao-existe');
    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(screen.getByLabelText('localização atual')).toHaveTextContent('/login');
  });

  it('redireciona uma rota administrativa válida sem sessão para o login', async () => {
    abrirEm('/admin/targets');
    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(screen.getByLabelText('localização atual')).toHaveTextContent('/login');
  });

  it('redireciona uma rota administrativa desconhecida sem sessão para o login', async () => {
    abrirEm('/admin/pagina-que-nao-existe');
    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(screen.getByLabelText('localização atual')).toHaveTextContent('/login');
  });

  it('redireciona /admin para o dashboard quando a sessão é válida', async () => {
    setToken(jwtDeTeste({ tenant_id: TENANT, exp: AMANHA }));
    abrirEm('/admin');
    expect(await screen.findByText('sonda-dashboard')).toBeInTheDocument();
    expect(screen.getByLabelText('localização atual')).toHaveTextContent('/admin/dashboard');
  });

  it('redireciona URL administrativa desconhecida ao dashboard com sessão válida', async () => {
    setToken(jwtDeTeste({ tenant_id: TENANT, exp: AMANHA }));
    abrirEm('/admin/pagina-que-nao-existe');
    expect(await screen.findByText('sonda-dashboard')).toBeInTheDocument();
    expect(screen.getByLabelText('localização atual')).toHaveTextContent('/admin/dashboard');
  });

  it('preserva o acesso anônimo às páginas públicas de campanha', () => {
    abrirEm('/landing/pagina-publica?c=campanha&t=alvo');
    expect(screen.getByText('sonda-landing-publica')).toBeInTheDocument();
    expect(screen.queryByText('sonda-login')).not.toBeInTheDocument();
  });
});
