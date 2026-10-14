import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route, useLocation } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Login from './Login';
import { NotificationProvider } from '../context/NotificationContext';
import { clearSession, getToken } from '../auth/session';
import PrivateRoute from '../auth/PrivateRoute';
import { jwtDeTeste } from '../test/jwt';
import { AUTH_API_BASE } from '../config';

// Sonda de destino: substitui a LandingHome real (que puxa MUI AppBar/tema completo,
// irrelevante aqui) só para confirmar QUE a navegação chegou em "/".
function SondaHome() {
  return <div>sonda-home</div>;
}

function Destination() {
  const location = useLocation();
  return <div>{`${location.pathname}${location.search}${location.hash}`}</div>;
}

function renderLogin(state?: unknown) {
  return render(
    <MemoryRouter initialEntries={[{ pathname: '/login', state }]}>
      <NotificationProvider>
        <Routes>
          <Route path="/" element={<SondaHome />} />
          <Route path="/login" element={<Login />} />
          <Route path="/admin/*" element={<Destination />} />
        </Routes>
      </NotificationProvider>
    </MemoryRouter>
  );
}

beforeEach(() => {
  clearSession();
  vi.stubGlobal('fetch', vi.fn());
});

afterEach(() => {
  clearSession();
  vi.unstubAllGlobals();
});

const token = jwtDeTeste({
  tenant_id: '11111111-1111-1111-1111-111111111111',
  exp: Math.floor(Date.now() / 1000) + 3600,
});

function fillLogin(email = 'admin@example.com', password = ' Segredo#123 ') {
  fireEvent.change(screen.getByLabelText(/endereço de e-mail/i), { target: { value: email } });
  fireEvent.change(screen.getByLabelText(/^senha/i), { target: { value: password } });
}

function submitLogin() {
  const input = screen.getByLabelText(/^senha/i);
  const form = input.closest('form');
  if (!form) throw new Error('Formulário de login não encontrado.');
  fireEvent.submit(form);
}

describe('Login — acesso e feedback', () => {
  it('mostra e oculta a senha por teclado, preservando o valor e sem enviar', async () => {
    const user = userEvent.setup();
    renderLogin();
    fillLogin();
    const password = screen.getByLabelText(/^senha/i);
    expect(password).toHaveAttribute('type', 'password');
    expect(password).toHaveAttribute('autocomplete', 'current-password');

    act(() => screen.getByRole('button', { name: 'Mostrar senha' }).focus());
    await user.keyboard('{Enter}');
    expect(password).toHaveAttribute('type', 'text');
    expect(password).toHaveValue(' Segredo#123 ');
    expect(screen.getByRole('button', { name: 'Ocultar senha' })).toHaveAttribute('aria-pressed', 'true');
    await user.keyboard(' ');
    expect(password).toHaveAttribute('type', 'password');
    expect(password).toHaveValue(' Segredo#123 ');
    expect(fetch).not.toHaveBeenCalled();
  });

  it('mostra validação junto aos campos e foca o primeiro erro sem chamar a API', () => {
    renderLogin();
    submitLogin();
    expect(screen.getByLabelText(/endereço de e-mail/i)).toHaveFocus();
    expect(screen.getByLabelText(/endereço de e-mail/i)).toHaveAccessibleDescription('Informe seu e-mail.');
    expect(screen.getByLabelText(/^senha/i)).toHaveAccessibleDescription('Informe sua senha.');
    fillLogin('email-invalido');
    submitLogin();
    expect(screen.getByLabelText(/endereço de e-mail/i)).toHaveAccessibleDescription('Informe um e-mail válido.');
    expect(fetch).not.toHaveBeenCalled();
  });

  it('informa Caps Lock e remove o aviso ao desativar ou sair do campo', () => {
    renderLogin();
    const password = screen.getByLabelText(/^senha/i);
    fireEvent.keyDown(password, { key: 'A', modifierCapsLock: true });
    expect(screen.getByRole('status')).toHaveTextContent('Caps Lock está ativado.');
    expect(password).toHaveAccessibleDescription('Caps Lock está ativado.');
    fireEvent.keyUp(password, { key: 'CapsLock', modifierCapsLock: false });
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    fireEvent.keyDown(password, { key: 'A', modifierCapsLock: true });
    fireEvent.blur(password);
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
  });

  it('bloqueia envios repetidos durante a requisição e permite tentar novamente após falha', async () => {
    let resolveRequest!: (response: Response) => void;
    vi.mocked(fetch).mockImplementationOnce(() => new Promise<Response>((resolve) => { resolveRequest = resolve; }));
    renderLogin();
    fillLogin();
    fireEvent.click(screen.getByRole('button', { name: 'Mostrar senha' }));
    submitLogin();
    submitLogin();
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(screen.getByRole('button', { name: 'Entrando…' })).toBeDisabled();
    expect(screen.getByLabelText(/^senha/i)).toHaveAttribute('type', 'password');
    expect(screen.getByLabelText(/^senha/i)).toBeDisabled();

    resolveRequest(new Response(null, { status: 500 }));
    await waitFor(() => expect(screen.getByRole('button', { name: 'Entrar no Sistema' })).toBeEnabled());
    vi.mocked(fetch).mockResolvedValueOnce(new Response(null, { status: 400 }));
    submitLogin();
    await waitFor(() => expect(fetch).toHaveBeenCalledTimes(2));
    await waitFor(() => expect(screen.getByRole('button', { name: 'Entrar no Sistema' })).toBeEnabled());
  });

  it.each([
    [400, 'E-mail ou senha inválidos.'],
    [401, 'E-mail ou senha inválidos.'],
    [429, 'Muitas tentativas. Aguarde um momento antes de tentar novamente.'],
    [500, 'Não foi possível entrar agora. Tente novamente em instantes.'],
    [503, 'Não foi possível entrar agora. Tente novamente em instantes.'],
  ])('traduz HTTP %s sem mostrar o corpo de erro do servidor', async (status, message) => {
    vi.mocked(fetch).mockResolvedValue(new Response('stack trace / segredo de infraestrutura', { status }));
    const { container } = renderLogin();
    fillLogin();
    submitLogin();
    expect(await within(container.querySelector('form')!).findByRole('alert')).toHaveTextContent(message);
    expect(screen.queryByText(/segredo de infraestrutura/)).not.toBeInTheDocument();
    expect(screen.getByLabelText(/^senha/i)).toHaveValue(' Segredo#123 ');
  });

  it('traduz erros de rede e limpa o erro persistente ao corrigir um campo', async () => {
    vi.mocked(fetch).mockRejectedValue(new TypeError('Failed to fetch'));
    const { container } = renderLogin();
    fillLogin();
    submitLogin();
    const form = container.querySelector('form')!;
    expect(await within(form).findByRole('alert')).toHaveTextContent('Não foi possível conectar. Verifique sua conexão e tente novamente.');
    fireEvent.change(screen.getByLabelText(/^senha/i), { target: { value: 'OutraSenha' } });
    expect(within(form).queryByRole('alert')).not.toBeInTheDocument();
  });

  it('envia por Enter, normaliza somente o e-mail e armazena o token em memória', async () => {
    vi.mocked(fetch).mockResolvedValue(Response.json({ accessToken: token }));
    const user = userEvent.setup();
    renderLogin();
    fillLogin('  admin@example.com  ');
    act(() => screen.getByLabelText(/^senha/i).focus());
    await user.keyboard('{Enter}');
    expect(await screen.findByText('/admin/dashboard')).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith(`${AUTH_API_BASE}/login`, expect.objectContaining({
      credentials: 'include',
      body: JSON.stringify({ email: 'admin@example.com', password: ' Segredo#123 ' }),
    }));
    expect(getToken()).toBe(token);
    expect(localStorage.getItem('phishguard_token')).toBeNull();
    expect(sessionStorage.getItem('phishguard_token')).toBeNull();
  });

  it.each([{}, { accessToken: '' }, { accessToken: 123 }, null])('não anuncia sucesso com resposta de autenticação inválida: %j', async (payload) => {
    vi.mocked(fetch).mockResolvedValue(Response.json(payload));
    const { container } = renderLogin();
    fillLogin();
    submitLogin();
    expect(await within(container.querySelector('form')!).findByRole('alert')).toHaveTextContent('Não foi possível entrar agora.');
    expect(getToken()).toBeNull();
    expect(screen.queryByText('/admin/dashboard')).not.toBeInTheDocument();
  });

  it.each([
    'https://example.com/admin/campaigns',
    '//example.com/admin/campaigns',
    '/admin\\example.com',
    '/admin/../login',
    '/login',
    '/admin/desconhecida',
  ])('ignora destino de retorno não permitido: %s', async (from) => {
    vi.mocked(fetch).mockResolvedValue(Response.json({ accessToken: token }));
    renderLogin({ from });
    fillLogin();
    submitLogin();
    expect(await screen.findByText('/admin/dashboard')).toBeInTheDocument();
  });

  it('retorna à rota protegida com query e hash após autenticar', async () => {
    vi.mocked(fetch)
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(Response.json({ accessToken: token }));
    render(
      <MemoryRouter initialEntries={['/admin/campaigns?status=agendada#lista']}>
        <NotificationProvider>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/admin/*" element={<PrivateRoute><Destination /></PrivateRoute>} />
          </Routes>
        </NotificationProvider>
      </MemoryRouter>,
    );
    await screen.findByRole('heading', { name: 'Login' });
    fillLogin();
    submitLogin();
    expect(await screen.findByText('/admin/campaigns?status=agendada#lista')).toBeInTheDocument();
  });
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
