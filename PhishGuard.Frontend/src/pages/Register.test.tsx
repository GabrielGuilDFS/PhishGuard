import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Register from './Register';
import { TOKEN_KEY } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';

function SondaHome() {
  return <div>sonda-home</div>;
}

function renderRegister() {
  return render(
    <MemoryRouter initialEntries={['/register']}>
      <Routes>
        <Route path="/" element={<SondaHome />} />
        <Route path="/register" element={<Register />} />
      </Routes>
    </MemoryRouter>
  );
}

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  vi.stubGlobal('fetch', vi.fn());
});

describe('Register — isolamento de sessão (entrada do fluxo de nova conta)', () => {
  it('destrói a sessão residual de outra conta ao montar a tela', () => {
    // Usuário anterior deste navegador deixou uma sessão VÁLIDA para trás.
    localStorage.setItem(
      TOKEN_KEY,
      jwtDeTeste({ tenant_id: '11111111-1111-1111-1111-111111111111', exp: Math.floor(Date.now() / 1000) + 86400 })
    );
    sessionStorage.setItem(TOKEN_KEY, 'residuo-session-storage');

    renderRegister();

    // Criar conta nova = ambiente limpo: nenhum vestígio da sessão anterior pode
    // sobreviver para o checkout/painel que vêm a seguir.
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });

  it('preserva preferências de UI que não identificam usuário (tema)', () => {
    localStorage.setItem(TOKEN_KEY, 'token-antigo');
    localStorage.setItem('phishguard_theme_mode', 'dark');

    renderRegister();

    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(localStorage.getItem('phishguard_theme_mode')).toBe('dark');
  });
});

describe('Register — logo clicável', () => {
  it('aponta para "/" com href real', () => {
    renderRegister();
    const logo = screen.getByRole('link', { name: /voltar para a página inicial do phishguard/i });
    expect(logo).toHaveAttribute('href', '/');
  });

  it('navega via SPA (React Router) para a LandingHome ao clicar, sem reload de página', async () => {
    const user = userEvent.setup();
    renderRegister();

    const logo = screen.getByRole('link', { name: /voltar para a página inicial do phishguard/i });
    await user.click(logo);

    expect(await screen.findByText('sonda-home')).toBeInTheDocument();
  });
});

// Preenche os campos obrigatórios que não são o foco do teste (empresa/CNPJ/nome/
// e-mail), deixando senha e confirmação para o chamador decidir.
async function preencherCamposBase(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText(/nome da empresa/i), 'Empresa Teste');
  await user.type(screen.getByLabelText(/cnpj/i), '12345678000199');
  await user.type(screen.getByLabelText(/nome completo/i), 'Fulano de Tal');
  await user.type(screen.getByLabelText(/endereço de email/i), 'fulano@teste.com');
}

describe('Register — confirmação de senha', () => {
  it('bloqueia o cadastro e avisa quando as senhas divergem, sem chamar a API', async () => {
    const user = userEvent.setup();
    const fetchMock = vi.mocked(fetch);
    renderRegister();

    await preencherCamposBase(user);
    await user.type(screen.getByLabelText('Senha *'), 'senha123');
    await user.type(screen.getByLabelText(/confirmar senha/i), 'senha456');
    await user.click(screen.getByRole('button', { name: /cadastrar/i }));

    expect(await screen.findByText('As senhas não coincidem.')).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('segue o fluxo normal (chama a API) quando as senhas coincidem', async () => {
    const user = userEvent.setup();
    const fetchMock = vi.mocked(fetch);
    fetchMock.mockResolvedValueOnce({
      ok: true,
      headers: { get: () => 'application/json' },
      json: async () => ({}),
    } as Response);
    renderRegister();

    await preencherCamposBase(user);
    await user.type(screen.getByLabelText('Senha *'), 'senha123');
    await user.type(screen.getByLabelText(/confirmar senha/i), 'senha123');
    await user.click(screen.getByRole('button', { name: /cadastrar/i }));

    await screen.findByText(/cadastro realizado/i);
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('As senhas não coincidem.')).not.toBeInTheDocument();
  });
});
