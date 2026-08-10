import { describe, it, expect, afterEach, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { alpha } from '@mui/material/styles';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Settings from './Settings';
import { ThemeModeProvider } from '../context/ThemeModeContext';
import { NotificationProvider } from '../context/NotificationContext';
import { brandPalette } from '../theme';
import { clearSession, getToken, setToken } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

function renderSettings() {
  return render(
    <ThemeModeProvider>
      <NotificationProvider>
        <Settings />
      </NotificationProvider>
    </ThemeModeProvider>
  );
}

afterEach(() => {
  // Sem token: os dois useEffect de carga (perfil/SMTP) fazem early-return antes de
  // chamar fetch — a tela renderiza puramente síncrona, sem precisar mockar rede.
  localStorage.clear();
  clearSession();
  vi.unstubAllGlobals();
});

describe('Settings — Editar Perfil', () => {
  it('carrega o perfil, salva o nome e substitui o access token retornado pela API', async () => {
    const tenantId = '11111111-1111-1111-1111-111111111111';
    const tokenInicial = jwtDeTeste({
      tenant_id: tenantId,
      exp: Math.floor(Date.now() / 1000) + 3600,
      name: 'Nome do Token',
      email: 'admin@teste.com',
    });
    const tokenAtualizado = jwtDeTeste({
      tenant_id: tenantId,
      exp: Math.floor(Date.now() / 1000) + 3600,
      name: 'Nome Atualizado',
      email: 'admin@teste.com',
    });
    setToken(tokenInicial);

    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input);
      if (url.endsWith('/auth/profile') && (init?.method ?? 'GET') === 'GET') {
        return new Response(JSON.stringify({
          nome: 'Nome da API',
          email: 'admin@teste.com',
          empresa: 'Empresa Segura',
        }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        });
      }
      if (url.endsWith('/auth/profile') && init?.method === 'PUT') {
        return new Response(JSON.stringify({
          nome: 'Nome Atualizado',
          email: 'admin@teste.com',
          empresa: 'Empresa Segura',
          accessToken: tokenAtualizado,
          expiresAtUtc: new Date(Date.now() + 3600_000).toISOString(),
        }), { status: 200, headers: { 'Content-Type': 'application/json' } });
      }
      if (url.includes('/email-delivery/status')) {
        return new Response(JSON.stringify({
          configurado: false,
          senhaConfigurada: false,
          transporteDisponivel: true,
        }), { status: 200, headers: { 'Content-Type': 'application/json' } });
      }
      return new Response(null, { status: 404 });
    });
    vi.stubGlobal('fetch', fetchMock);

    renderSettings();

    const nameInput = await screen.findByLabelText(/nome do administrador/i);
    await waitFor(() => expect(nameInput).toHaveValue('Nome da API'));
    expect(screen.getByLabelText(/e-mail de login/i)).toHaveValue('admin@teste.com');
    expect(screen.getByLabelText(/^empresa$/i)).toHaveValue('Empresa Segura');
    expect(screen.getByRole('tab', { name: /editar perfil/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /editar informações pessoais/i })).toBeInTheDocument();
    expect(screen.queryByLabelText(/função/i)).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Segurança' })).toBeInTheDocument();

    fireEvent.change(nameInput, { target: { value: 'Nome Atualizado' } });
    fireEvent.click(screen.getByRole('button', { name: /salvar informações/i }));

    await waitFor(() => expect(getToken()).toBe(tokenAtualizado));
    const profilePut = fetchMock.mock.calls.find(([url, init]) =>
      String(url).endsWith('/auth/profile') && init?.method === 'PUT');
    expect(profilePut).toBeDefined();
    expect(JSON.parse(String(profilePut?.[1]?.body))).toEqual({ nome: 'Nome Atualizado' });
  });

  it('valida a confirmação e envia somente os campos necessários para alterar a senha', async () => {
    const tenantId = '11111111-1111-1111-1111-111111111111';
    const tokenInicial = jwtDeTeste({
      tenant_id: tenantId,
      exp: Math.floor(Date.now() / 1000) + 3600,
      name: 'Admin Segurança',
      email: 'admin@teste.com',
    });
    const tokenAtualizado = jwtDeTeste({
      tenant_id: tenantId,
      exp: Math.floor(Date.now() / 1000) + 3600,
      name: 'Admin Segurança',
      email: 'admin@teste.com',
    });
    setToken(tokenInicial);

    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input);
      if (url.endsWith('/auth/profile') && (init?.method ?? 'GET') === 'GET') {
        return new Response(JSON.stringify({
          nome: 'Admin Segurança',
          email: 'admin@teste.com',
          empresa: 'Empresa Segura',
        }), { status: 200, headers: { 'Content-Type': 'application/json' } });
      }
      if (url.endsWith('/auth/profile') && init?.method === 'PUT') {
        return new Response(JSON.stringify({
          nome: 'Admin Segurança',
          email: 'admin@teste.com',
          empresa: 'Empresa Segura',
          accessToken: tokenAtualizado,
          expiresAtUtc: new Date(Date.now() + 3600_000).toISOString(),
        }), { status: 200, headers: { 'Content-Type': 'application/json' } });
      }
      if (url.includes('/email-delivery/status')) {
        return new Response(JSON.stringify({
          configurado: false,
          senhaConfigurada: false,
          transporteDisponivel: true,
        }), { status: 200, headers: { 'Content-Type': 'application/json' } });
      }
      return new Response(null, { status: 404 });
    });
    vi.stubGlobal('fetch', fetchMock);
    renderSettings();

    fireEvent.change(screen.getByLabelText(/^senha atual/i), { target: { value: 'SenhaAtual@123' } });
    fireEvent.change(screen.getByLabelText(/^nova senha/i), { target: { value: 'NovaSenha@456' } });
    fireEvent.change(screen.getByLabelText(/confirmar nova senha/i), { target: { value: 'Diferente@456' } });
    fireEvent.click(screen.getByRole('button', { name: /alterar senha/i }));

    expect(await screen.findByText(/a confirmação deve ser idêntica/i)).toBeInTheDocument();
    expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'PUT')).toHaveLength(0);

    fireEvent.change(screen.getByLabelText(/confirmar nova senha/i), { target: { value: 'NovaSenha@456' } });
    expect(screen.getByText(/confirmação idêntica à nova senha/i)).toHaveTextContent('✓');
    fireEvent.click(screen.getByRole('button', { name: /alterar senha/i }));

    await waitFor(() => expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'PUT')).toHaveLength(1));
    const passwordPut = fetchMock.mock.calls.find(([, init]) => init?.method === 'PUT');
    expect(JSON.parse(String(passwordPut?.[1]?.body))).toEqual({
      senhaAtual: 'SenhaAtual@123',
      novaSenha: 'NovaSenha@456',
    });
    await waitFor(() => expect(screen.getByLabelText(/^senha atual/i)).toHaveValue(''));
  });
});

describe('Settings — entrega multi-provedor', () => {
  it('mantém SMTP e permite alternar para API HTTPS com campos do AWS SES', () => {
    renderSettings();
    fireEvent.click(screen.getByRole('tab', { name: /entrega de e-mail/i }));

    expect(screen.getByRole('button', { name: /servidor smtp/i })).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /api https/i }));

    expect(screen.getByLabelText(/provedor por api/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/aws access key id/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/aws secret access key/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/região aws ses/i)).toBeInTheDocument();
  });

  it('oferece Mailtrap Sandbox e informa que destinatários reais não recebem mensagens', () => {
    renderSettings();
    fireEvent.click(screen.getByRole('tab', { name: /entrega de e-mail/i }));
    fireEvent.click(screen.getByRole('button', { name: /api https/i }));

    fireEvent.mouseDown(screen.getByLabelText(/provedor por api/i));
    fireEvent.click(screen.getByRole('option', { name: /mailtrap sandbox/i }));

    expect(screen.getByLabelText(/mailtrap sandbox id/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/mailtrap api token/i)).toBeInTheDocument();
    expect(screen.getByText(/não receberão e-mails em suas caixas reais/i)).toBeInTheDocument();
  });
});

describe('Settings — bordas suavizadas', () => {
  it('painel principal não tem borda própria (só a sombra de elevation delimita)', () => {
    renderSettings();
    // O h4 "Configurações do Sistema" é irmão do Paper; o Paper é o próximo elemento.
    const titulo = screen.getByRole('heading', { name: 'Configurações do Sistema' });
    const paper = titulo.nextElementSibling as HTMLElement;
    expect(paper).toHaveClass('MuiPaper-root');
    // Antes: `border: 1, borderColor: 'divider'` (borda sólida cheia). Removida —
    // sem essa asserção, um retorno acidental da borda passaria despercebido.
    // jsdom não resolve o valor inicial da spec ('none') para propriedades nunca
    // declaradas — devolve string vazia; é o sinal correto de "nenhuma borda setada".
    expect(getComputedStyle(paper).borderStyle).toBe('');
  });

  it('separador entre a faixa de abas e o conteúdo usa a cor secundária diluída, não a cor cheia', () => {
    renderSettings();
    // O <Box sx={{ borderBottom }}> envolve o <Tabs> inteiro (MuiTabs-root), não só o
    // elemento role="tablist" — que fica bem mais fundo na árvore interna do MUI
    // (tablist → MuiTabs-scroller → MuiTabs-root → o Box procurado).
    const tabsRoot = screen.getByRole('tablist').closest('.MuiTabs-root') as HTMLElement;
    const wrapperDoDivisor = tabsRoot.parentElement as HTMLElement;

    const corEsperada = alpha(brandPalette.light.secondary, 0.16);
    expect(getComputedStyle(wrapperDoDivisor).borderBottomColor).toBe(corEsperada);
  });
});
