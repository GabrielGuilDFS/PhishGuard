import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { axe } from 'vitest-axe';
import AdminDashboard from './AdminDashboard';
import { NotificationProvider } from '../context/NotificationContext';
import { ThemeModeProvider } from '../context/ThemeModeContext';
import type { DashboardOverview } from './dashboard/dashboard.types';
import { clearSession, setToken } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

vi.mock('recharts', () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  LineChart: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  CartesianGrid: () => null,
  Legend: () => null,
  Line: () => null,
  Tooltip: () => null,
  XAxis: () => null,
  YAxis: () => null,
}));

const payload: DashboardOverview = {
  period: { start: '2026-07-06T12:00:00Z', end: '2026-08-05T12:00:00Z', label: 'Últimos 30 dias' },
  tenant: { name: 'Empresa Segura' },
  scope: { campaignCount: 3, uniqueTargetCount: 280, campaignTargetCount: 320 },
  availableDepartments: ['Financeiro', 'TI'],
  kpis: {
    sent: { total: 320, deltaPercent: 12.4 },
    openRate: { rate: 71.8, uniqueTotal: 230, observedTotal: 210, inferredTotal: 20 },
    clickRate: { rate: 18.4, uniqueTotal: 59 },
    compromiseRate: { rate: 6.3, uniqueTotal: 20 },
    trainingRate: { rate: 67.8, uniqueTotal: 40 },
  },
  trainingEffectiveness: {
    compromised: { rate: 6.3, uniqueTotal: 20 },
    educationViewed: { rate: 15.6, uniqueTotal: 50 },
    completed: { rate: 12.5, uniqueTotal: 40 },
    abandoned: { rate: 20, uniqueTotal: 10 },
    recovery: { rate: 75, uniqueTotal: 15 },
  },
  trend: [
    { label: '05/08', bucketStart: '2026-08-05T00:00:00Z', sent: 320, opened: 230, clicked: 59, compromised: 20, educationViewed: 50, trained: 40 },
  ],
  recentCampaigns: [
    {
      id: '11111111-1111-1111-1111-111111111111',
      name: 'Atualização de senha',
      status: 'Em Andamento',
      date: '2026-08-01T12:00:00Z',
      sent: 80,
      openedTotal: 60,
      openRate: 75,
      openedWithoutClickTotal: 46,
      openedWithoutClickRate: 57.5,
      clickRate: 17.5,
      compromiseRate: 5,
      educationViewRate: 62.5,
      educationAbandonmentTotal: 10,
      educationAbandonmentRate: 20,
      trainedTotal: 40,
      trainingCompletionRate: 50,
      trainingRate: 50,
    },
  ],
};

const fetchMock = vi.fn();

function CampaignRouteProbe() {
  const location = useLocation();
  return <div>sonda-campanhas:{location.pathname}{location.search}</div>;
}

function response(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

function renderDashboard() {
  return render(
    <ThemeModeProvider>
      <NotificationProvider>
        <MemoryRouter initialEntries={['/admin/dashboard']}>
          <Routes>
            <Route path="/login" element={<div>sonda-login</div>} />
            <Route path="/admin/dashboard" element={<AdminDashboard />} />
            <Route path="/admin/campaigns" element={<CampaignRouteProbe />} />
          </Routes>
        </MemoryRouter>
      </NotificationProvider>
    </ThemeModeProvider>,
  );
}

beforeEach(() => {
  fetchMock.mockReset();
  fetchMock.mockResolvedValue(response(payload));
  vi.stubGlobal('fetch', fetchMock);
  setToken(jwtDeTeste({
    tenant_id: '11111111-1111-1111-1111-111111111111',
    exp: Math.floor(Date.now() / 1000) + 3600,
  }));
});

afterEach(() => {
  vi.unstubAllGlobals();
  clearSession();
  localStorage.clear();
});

describe('AdminDashboard — visão executiva essencial', () => {
  it('carrega um contrato consolidado e renderiza KPIs, tendência e campanhas', async () => {
    renderDashboard();

    expect(await screen.findByRole('heading', { name: 'Visão Geral de Segurança' })).toBeInTheDocument();
    expect(screen.getByText('Empresa Segura')).toBeInTheDocument();
    expect(screen.getByText('320')).toBeInTheDocument();
    expect(screen.getByText('71,8%')).toBeInTheDocument();
    expect(screen.getByText('18,4%')).toBeInTheDocument();
    expect(screen.getAllByText('6,3%').length).toBeGreaterThan(0);
    expect(screen.getByText('Aprendizado Concluído')).toBeInTheDocument();
    expect(screen.getByText('40 treinamentos concluídos')).toBeInTheDocument();
    expect(screen.getByText('Tendência do Funil de Simulação')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Efetividade do Treinamento' })).toBeInTheDocument();
    expect(screen.getByLabelText('Recuperação após comprometimento: 75,0%')).toBeInTheDocument();
    expect(screen.getByText(/3 campanhas · 280 destinatários únicos · 320 e-mails enviados/)).toBeInTheDocument();
    expect(screen.getAllByText('Atualização de senha').length).toBeGreaterThan(0);
    expect(screen.getByLabelText('75,0% de abertura; 46 abriram sem clicar')).toBeInTheDocument();
    expect(screen.getByText('46 abriram sem clicar')).toBeInTheDocument();
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(String(fetchMock.mock.calls[0][0])).toContain('/Dashboard/overview?period=30d');
  });

  it('explica as fórmulas em tooltips e em um glossário acessível', async () => {
    renderDashboard();
    await screen.findByText('Empresa Segura');

    expect(screen.getByRole('button', { name: 'Como E-mails Enviados é calculado' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Como são calculadas?' }));
    expect(await screen.findByRole('dialog')).toBeInTheDocument();
    await act(async () => { await new Promise((resolve) => setTimeout(resolve, 250)); });
    expect(screen.getByRole('heading', { name: 'Como as métricas são calculadas?' })).toBeInTheDocument();
    expect(screen.getByText(/O gráfico agrega todas as campanhas/i)).toBeInTheDocument();
    expect(await axe(document.body, { rules: { 'color-contrast': { enabled: false } } })).toHaveNoViolations();

    fireEvent.click(screen.getByRole('button', { name: 'Fechar' }));
    await act(async () => { await new Promise((resolve) => setTimeout(resolve, 250)); });
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  });

  it('usa singular ao mostrar uma única abertura sem clique', async () => {
    fetchMock.mockResolvedValue(response({
      ...payload,
      recentCampaigns: [{
        ...payload.recentCampaigns[0],
        openedWithoutClickTotal: 1,
        openedWithoutClickRate: 1.3,
      }],
    }));

    renderDashboard();

    expect(await screen.findByText('1 abriu sem clicar')).toBeInTheDocument();
    expect(screen.getByLabelText('75,0% de abertura; 1 abriu sem clicar')).toBeInTheDocument();
  });

  it('não exibe uma frase estática quando não há abertura ou quando todas tiveram clique', async () => {
    fetchMock.mockResolvedValueOnce(response({
      ...payload,
      recentCampaigns: [{
        ...payload.recentCampaigns[0],
        openedTotal: 0,
        openRate: 0,
        openedWithoutClickTotal: 0,
        openedWithoutClickRate: 0,
      }],
    }));

    const first = renderDashboard();
    expect(await screen.findByText('Nenhuma abertura registrada')).toBeInTheDocument();
    first.unmount();

    fetchMock.mockResolvedValueOnce(response({
      ...payload,
      recentCampaigns: [{
        ...payload.recentCampaigns[0],
        openedTotal: 14,
        openedWithoutClickTotal: 0,
        openedWithoutClickRate: 0,
      }],
    }));
    renderDashboard();
    expect(await screen.findByText('Todas as aberturas tiveram clique')).toBeInTheDocument();
  });

  it('refaz somente o overview quando período e departamento mudam', async () => {
    const user = userEvent.setup();
    renderDashboard();
    await screen.findByText('Empresa Segura');

    await user.click(screen.getByLabelText('Período'));
    await user.click(screen.getByRole('option', { name: 'Últimos 7 dias' }));
    await waitFor(() => expect(fetchMock.mock.calls.some(([url]) => String(url).includes('period=7d'))).toBe(true));

    await user.click(screen.getByLabelText('Departamento'));
    await user.click(screen.getByRole('option', { name: 'TI' }));
    await waitFor(() => expect(fetchMock.mock.calls.some(([url]) => {
      const value = String(url);
      return value.includes('period=7d') && value.includes('department=TI');
    })).toBe(true));
  });

  it('navega para o fluxo de nova campanha', async () => {
    const user = userEvent.setup();
    renderDashboard();
    await screen.findByText('Empresa Segura');

    await user.click(screen.getByRole('button', { name: 'Nova Campanha' }));

    expect(await screen.findByText('sonda-campanhas:/admin/campaigns?nova=1')).toBeInTheDocument();
  });

  it('exporta PDF com token, filtros ativos e filename UTF-8 do backend', async () => {
    const user = userEvent.setup();
    const pdfResponse = new Response(new Blob(['%PDF-test']), {
      status: 200,
      headers: {
        'Content-Type': 'application/pdf',
        'Content-Disposition': 'attachment; filename="fallback.pdf"; filename*=UTF-8\'\'phishguard-dashboard-Seguran%C3%A7a.pdf',
      },
    });
    fetchMock.mockResolvedValueOnce(response(payload)).mockResolvedValueOnce(pdfResponse);
    const createObjectUrl = vi.fn(() => 'blob:dashboard-export');
    const revokeObjectUrl = vi.fn();
    Object.defineProperty(URL, 'createObjectURL', { configurable: true, value: createObjectUrl });
    Object.defineProperty(URL, 'revokeObjectURL', { configurable: true, value: revokeObjectUrl });
    const clickSpy = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);

    renderDashboard();
    await screen.findByText('Empresa Segura');
    await user.click(screen.getByRole('button', { name: 'Exportar' }));
    await user.click(screen.getByRole('menuitem', { name: 'Relatório PDF' }));

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2));
    const [url, options] = fetchMock.mock.calls[1] as [string, RequestInit];
    expect(url).toContain('/Dashboard/export?period=30d&format=pdf');
    expect(new Headers(options.headers).get('Authorization')).toMatch(/^Bearer .+/);
    expect(options.credentials).toBe('include');
    await waitFor(() => expect(clickSpy).toHaveBeenCalledOnce());
    expect(createObjectUrl).toHaveBeenCalledOnce();
    expect(revokeObjectUrl).toHaveBeenCalledWith('blob:dashboard-export');
    expect(await screen.findByText('Relatório PDF exportado com sucesso.')).toBeInTheDocument();
  });

  it('exibe o erro do backend quando a exportação falha', async () => {
    const user = userEvent.setup();
    fetchMock
      .mockResolvedValueOnce(response(payload))
      .mockResolvedValueOnce(response({ message: 'Não foi possível gerar o relatório.' }, 500));

    renderDashboard();
    await screen.findByText('Empresa Segura');
    await user.click(screen.getByRole('button', { name: 'Exportar' }));
    await user.click(screen.getByRole('menuitem', { name: 'CSV de campanhas' }));

    expect(await screen.findByText('Não foi possível gerar o relatório.')).toBeInTheDocument();
  });

  it('leva a ação da tabela ao deep-link de edição da campanha', async () => {
    const user = userEvent.setup();
    renderDashboard();
    expect((await screen.findAllByText('Atualização de senha')).length).toBeGreaterThan(0);

    await user.click(screen.getByRole('button', { name: 'Ver campanha Atualização de senha' }));

    expect(await screen.findByText('sonda-campanhas:/admin/campaigns?editar=11111111-1111-1111-1111-111111111111')).toBeInTheDocument();
  });

  it('revoga a sessão e redireciona quando a API responde 401', async () => {
    fetchMock.mockResolvedValue(response({ message: 'Não autorizado' }, 401));
    renderDashboard();

    expect(await screen.findByText('sonda-login')).toBeInTheDocument();
    expect(localStorage.getItem('phishguard_token')).toBeNull();
  });

  it('exibe erro recuperável quando a rede falha', async () => {
    fetchMock.mockRejectedValue(new Error('Falha de rede controlada'));
    renderDashboard();

    expect((await screen.findAllByText('Falha de rede controlada')).length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: /Tentar novamente/i })).toBeInTheDocument();
  });

  it('não apresenta violações detectáveis de acessibilidade no estado carregado', async () => {
    const { container } = renderDashboard();
    await screen.findByText('Empresa Segura');
    expect(await axe(container, { rules: { 'color-contrast': { enabled: false } } })).toHaveNoViolations();
  });
});
