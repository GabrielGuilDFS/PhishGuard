import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, act } from '@testing-library/react';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Campaigns from './Campaigns';
import { NotificationProvider } from '../context/NotificationContext';

// Deve casar com POLL_INTERVAL_MS em Campaigns.tsx.
const POLL_MS = 8000;

// Status atual devolvido pelo mock de /Campaigns — o teste o altera para simular o
// worker mudando o estado no backend entre um poll e outro.
let statusAtual = 'Agendada';

function campanha() {
  return {
    id: 'c1',
    nomeCampanha: 'Campanha 1',
    status: statusAtual,
    dataInicio: new Date().toISOString(),
    dataFim: null,
    templateNome: 'Isca',
    landingPageNome: 'Pagina',
    educationalPageNome: 'Edu',
  };
}

const fetchMock = vi.fn((url: string) => {
  if (String(url).includes('/Campaigns')) {
    return Promise.resolve({ ok: true, json: async () => [campanha()] });
  }
  return Promise.resolve({ ok: true, json: async () => [] });
});

const chamadasCampaigns = () =>
  fetchMock.mock.calls.filter(c => String(c[0]).includes('/Campaigns')).length;

function renderTela() {
  return render(
    <NotificationProvider>
      <Campaigns />
    </NotificationProvider>
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
  vi.stubGlobal('fetch', fetchMock);
  localStorage.setItem('phishguard_token', 'test-token');
});

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
  localStorage.clear();
});

describe('Campaigns — atualização reativa do status (polling inteligente)', () => {
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
