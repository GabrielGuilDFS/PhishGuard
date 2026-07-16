import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Login from './Login';
import { NotificationProvider } from '../context/NotificationContext';

// Sonda de destino: substitui a LandingHome real (que puxa MUI AppBar/tema completo,
// irrelevante aqui) só para confirmar QUE a navegação chegou em "/".
function SondaHome() {
  return <div>sonda-home</div>;
}

function renderLogin() {
  return render(
    <MemoryRouter initialEntries={['/login']}>
      <NotificationProvider>
        <Routes>
          <Route path="/" element={<SondaHome />} />
          <Route path="/login" element={<Login />} />
        </Routes>
      </NotificationProvider>
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn());
});

describe('Login — logo clicável', () => {
  it('aponta para "/" com href real (funciona mesmo se o JS falhar / clique do meio)', () => {
    renderLogin();
    const logo = screen.getByRole('link', { name: /voltar para a página inicial do phishguard/i });
    expect(logo).toHaveAttribute('href', '/');
  });

  it('navega via SPA (React Router) para a LandingHome ao clicar, sem reload de página', async () => {
    const user = userEvent.setup();
    renderLogin();

    expect(screen.queryByText('sonda-home')).not.toBeInTheDocument();

    const logo = screen.getByRole('link', { name: /voltar para a página inicial do phishguard/i });
    await user.click(logo);

    // Se a navegação fosse um reload de página inteiro (<a> comum fora do Router),
    // o DOM montado pelo React Testing Library não mudaria nem apareceria a sonda —
    // o jsdom não navega de verdade. Encontrar a sonda prova que foi o React Router
    // (client-side) quem trocou a rota, não um navigation event do browser.
    expect(await screen.findByText('sonda-home')).toBeInTheDocument();
  });

  it('preserva o texto "PhishGuard" (ícone + marca) dentro do link', () => {
    renderLogin();
    const logo = screen.getByRole('link', { name: /voltar para a página inicial do phishguard/i });
    expect(logo).toHaveTextContent('PhishGuard');
  });
});
