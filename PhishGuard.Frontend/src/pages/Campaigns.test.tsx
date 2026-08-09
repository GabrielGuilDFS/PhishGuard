import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Campaigns from './Campaigns';
import { toDateTimeLocal } from '../utils/dateTime';
import { NotificationProvider } from '../context/NotificationContext';
import { clearSession, setToken } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

// Deve casar com POLL_INTERVAL_MS em Campaigns.tsx.
const POLL_MS = 8000;

// Status atual devolvido pelo mock de /Campaigns — o teste o altera para simular o
// worker mudando o estado no backend entre um poll e outro.
let statusAtual = 'Agendada';
// Data de encerramento da coleta devolvida pelo mock (null = coleta sem prazo).
let dataFimAtual: string | null = null;
let smtpConfigurado = true;
let smtpTransporteDisponivel = true;

function campanha() {
  return {
    id: 'c1',
    nomeCampanha: 'Campanha 1',
    status: statusAtual,
    dataInicio: new Date().toISOString(),
    dataFim: dataFimAtual,
    templateNome: 'Isca',
    landingPageNome: 'Pagina',
    educationalPageNome: 'Edu',
  };
}

const fetchMock = vi.fn((url: string) => {
  if (String(url).includes('/Campaigns')) {
    return Promise.resolve({ ok: true, json: async () => [campanha()] });
  }
  if (String(url).includes('/email-delivery/status')) {
    return Promise.resolve({
      ok: true,
      json: async () => ({
        configurado: smtpConfigurado,
        transporteDisponivel: smtpTransporteDisponivel,
        transporteIndisponivelMotivo: smtpTransporteDisponivel ? undefined : 'SMTP bloqueado pelo ambiente.',
      })
    });
  }
  return Promise.resolve({ ok: true, json: async () => [] });
});

const chamadasCampaigns = () =>
  fetchMock.mock.calls.filter(c => String(c[0]).includes('/Campaigns')).length;

function renderTela(initialEntry = '/admin/campaigns') {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <NotificationProvider>
        <Campaigns />
      </NotificationProvider>
    </MemoryRouter>
  );
}

// Descarrega o fetch de montagem (promessas/microtasks) sob timers falsos.
async function flush() {
  await act(async () => { await vi.advanceTimersByTimeAsync(0); });
}

beforeEach(() => {
  vi.useFakeTimers();
  fetchMock.mockClear();
  statusAtual = 'Agendada';
  dataFimAtual = null;
  smtpConfigurado = true;
  smtpTransporteDisponivel = true;
  vi.stubGlobal('fetch', fetchMock);
  setToken(jwtDeTeste({
    tenant_id: '11111111-1111-1111-1111-111111111111',
    exp: Math.floor(Date.now() / 1000) + 3600,
  }));
});

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
  clearSession();
  localStorage.clear();
});

describe('Campaigns — atualização reativa do status (polling inteligente)', () => {
  it('exibe alerta acionável quando o SMTP não está pronto', async () => {
    statusAtual = 'Rascunho';
    smtpConfigurado = false;
    renderTela();
    await flush();

    expect(screen.getByText(/entrega de e-mail ainda não foi configurada/i)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /configurar entrega/i })).toHaveAttribute('href', '/admin/settings?tab=smtp');
  });

  it('abre o modal novo via ?nova=1 sem recarregar a listagem', async () => {
    statusAtual = 'Finalizada';
    renderTela('/admin/campaigns?nova=1');
    await flush();

    expect(screen.getByRole('dialog', { name: 'Nova Campanha' })).toBeInTheDocument();
    expect(chamadasCampaigns()).toBe(1);
  });

  it('faz polling enquanto há campanha Agendada e reflete a virada para "Em Andamento" sozinho', async () => {
    renderTela();
    await flush();

    // Estado inicial: badge "Agendada".
    expect(screen.getByText('Agendada')).toBeInTheDocument();

    // O worker mudou o status no backend; o próximo poll deve trazer "Em Andamento".
    statusAtual = 'Em Andamento';
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS); });

    expect(screen.getByText('Em Andamento')).toBeInTheDocument();
    expect(screen.queryByText('Agendada')).not.toBeInTheDocument();
  });

  it('para de fazer polling quando não há mais campanha pendente (poupa a API)', async () => {
    renderTela();
    await flush();

    // Vira "Em Andamento" no 1º poll → deixa de ser pendente → o intervalo é limpo.
    statusAtual = 'Em Andamento';
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS); });
    expect(screen.getByText('Em Andamento')).toBeInTheDocument();

    const chamadasAteAgora = chamadasCampaigns();

    // Muito tempo depois, NENHUMA requisição nova a /Campaigns deve ocorrer.
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS * 5); });
    expect(chamadasCampaigns()).toBe(chamadasAteAgora);
  });

  it('não faz polling quando a lista já carrega sem campanhas pendentes', async () => {
    statusAtual = 'Finalizada';
    renderTela();
    await flush();

    expect(screen.getByText('Finalizada')).toBeInTheDocument();
    const aposCarga = chamadasCampaigns(); // só a busca de montagem

    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS * 5); });
    expect(chamadasCampaigns()).toBe(aposCarga);
  });
});

describe('Campaigns — coluna "Encerramento da Coleta"', () => {
  it('sem prazo (dataFim nula): mostra indicador "Sem prazo", não fica vazia', async () => {
    statusAtual = 'Rascunho';
    dataFimAtual = null;
    renderTela();
    await flush();

    expect(screen.getByText('Sem prazo')).toBeInTheDocument();
  });

  it('rascunho com prazo: mostra a data como "prevista" (só vale ao ativar)', async () => {
    statusAtual = 'Rascunho';
    dataFimAtual = new Date('2030-05-20T10:00:00Z').toISOString();
    renderTela();
    await flush();

    // A data aparece (não é '-') e vem marcada como prevista.
    expect(screen.getByText('prevista')).toBeInTheDocument();
    expect(screen.queryByText('Sem prazo')).not.toBeInTheDocument();
  });

  // Reproduz a EVIDÊNCIA: a mesma campanha que no modal mostra a data, com dataFim (string
  // ISO) na resposta da LISTA, NÃO pode cair em "Sem prazo". Prova que a renderização está
  // correta quando o payload da lista carrega o campo — logo, "Sem prazo" na tela real só
  // ocorre se a lista vier SEM dataFim (backend), não por esta condicional.
  it('lista com dataFim (string ISO): renderiza a data, jamais "Sem prazo"', async () => {
    statusAtual = 'Rascunho';
    dataFimAtual = '2026-07-18T19:12:00.000Z';
    renderTela();
    await flush();

    expect(screen.queryByText('Sem prazo')).not.toBeInTheDocument();
    // Mostra algo de data (a tag "prevista" acompanha rascunho com prazo).
    expect(screen.getByText('prevista')).toBeInTheDocument();
  });

  it('dataFim inválida (string não-parseável): cai em "Sem prazo", nunca "Invalid Date"', async () => {
    statusAtual = 'Rascunho';
    dataFimAtual = 'xx/não é data/xx';
    renderTela();
    await flush();

    expect(screen.getByText('Sem prazo')).toBeInTheDocument();
    expect(screen.queryByText(/Invalid Date/i)).not.toBeInTheDocument();
  });

  it('campanha já coletando (Em Andamento) com prazo: mostra a data SEM tag "prevista"', async () => {
    statusAtual = 'Em Andamento';
    dataFimAtual = new Date('2030-05-20T10:00:00Z').toISOString();
    renderTela();
    await flush();

    expect(screen.queryByText('prevista')).not.toBeInTheDocument();
    expect(screen.queryByText('Sem prazo')).not.toBeInTheDocument();
    // Sanidade: a linha renderizou (badge de status presente).
    expect(screen.getByText('Em Andamento')).toBeInTheDocument();
  });
});

describe('toDateTimeLocal — UTC da API → hora local do <input datetime-local>', () => {
  it('compensa o offset do fuso (a hora não escorrega no round-trip de edição)', () => {
    // Simula UTC-3: getTimezoneOffset devolve minutos ATRÁS do UTC (+180).
    const spy = vi.spyOn(Date.prototype, 'getTimezoneOffset').mockReturnValue(180);
    // 20:00Z equivale a 17:00 no horário local (UTC-3).
    expect(toDateTimeLocal('2026-08-01T20:00:00.000Z')).toBe('2026-08-01T17:00');
    spy.mockRestore();
  });

  it('retorna string vazia para valor nulo/indefinido (sem quebrar o form)', () => {
    expect(toDateTimeLocal(null)).toBe('');
    expect(toDateTimeLocal(undefined)).toBe('');
  });
});
