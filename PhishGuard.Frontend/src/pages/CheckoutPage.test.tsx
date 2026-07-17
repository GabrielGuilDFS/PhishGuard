import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import CheckoutPage from './CheckoutPage';
import { NotificationProvider } from '../context/NotificationContext';
import { TOKEN_KEY } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

const TENANT_ANTIGO = '11111111-1111-1111-1111-111111111111';

/** Simula o pior cenário: OUTRA conta usou este navegador e deixou sessão válida. */
function plantarSessaoResidual() {
  localStorage.setItem(
    TOKEN_KEY,
    jwtDeTeste({ tenant_id: TENANT_ANTIGO, exp: Math.floor(Date.now() / 1000) + 86400 })
  );
  sessionStorage.setItem(TOKEN_KEY, 'residuo-session-storage');
}

function renderCheckout() {
  return render(
    <NotificationProvider>
      <MemoryRouter initialEntries={['/checkout?plano=prata']}>
        <Routes>
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/login" element={<div>sonda-login</div>} />
          <Route path="/admin/dashboard" element={<div>sonda-painel</div>} />
        </Routes>
      </MemoryRouter>
    </NotificationProvider>
  );
}

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true } as Response));
});

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
  localStorage.clear();
  sessionStorage.clear();
});

describe('CheckoutPage — isolamento de sessão no fluxo de nova conta', () => {
  it('destrói a sessão residual de outra conta já na ENTRADA do checkout', () => {
    plantarSessaoResidual();
    renderCheckout();

    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });

  // O cenário exato do bug: usuário previamente autenticado no navegador → nova conta
  // criada → pagamento. Antes, o Bearer residual ia no POST /Tenants/ativar (ativação
  // no tenant ERRADO) e o redirect caía no painel da conta ANTIGA.
  it('finaliza o pagamento SEM anexar o token residual e exige login limpo (não abre o painel)', async () => {
    vi.useFakeTimers();
    plantarSessaoResidual();
    const fetchMock = vi.mocked(fetch);
    renderCheckout();

    fireEvent.click(screen.getByRole('button', { name: /confirmar pagamento/i }));
    await act(async () => {
      await vi.advanceTimersByTimeAsync(2100);
    });

    // A ativação de plano é anônima: nenhum Authorization de sessão antiga.
    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toContain('/api/Tenants/ativar');
    expect(init?.headers).not.toHaveProperty('Authorization');

    // Ambiente 100% isolado: nada da conta anterior sobrou no armazenamento…
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();

    // …e o destino é o LOGIN limpo, nunca o painel (que renderizaria a conta antiga).
    expect(screen.getByText('sonda-login')).toBeInTheDocument();
    expect(screen.queryByText('sonda-painel')).not.toBeInTheDocument();
  });
});
