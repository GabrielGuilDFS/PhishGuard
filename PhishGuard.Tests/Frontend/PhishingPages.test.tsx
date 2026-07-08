import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Componente real de produção vive no projeto do frontend. Este teste de UI foi
// separado do código de produção e mora em PhishGuard.Tests/Frontend.
import PhishingPages from '../../PhishGuard.Frontend/src/pages/PhishingPages';

// Nome do molde HBO Max consolidado (deve existir no dropdown "Escolha a Interface").
const HBO_MOLDE = 'HBO Max - Redefinição de Senha';
const API = 'http://localhost:5000/api/PhishingPages';

// Mock global de fetch: GET (listagem) devolve [], POST/PUT devolvem ok.
const fetchMock = vi.fn((_url: string, options?: { method?: string }) => {
  const method = options?.method ?? 'GET';
  if (method === 'GET') {
    return Promise.resolve({ ok: true, json: async () => [] });
  }
  return Promise.resolve({ ok: true, json: async () => ({ id: 'novo-id' }) });
});

beforeEach(() => {
  fetchMock.mockClear();
  vi.stubGlobal('fetch', fetchMock);
  localStorage.setItem('phishguard_token', 'test-token');
});

afterEach(() => {
  vi.unstubAllGlobals();
  localStorage.clear();
});

// Abre o diálogo "Construir" clicando em "Novo Registro".
async function abrirDialogo() {
  render(<PhishingPages />);
  await screen.findByText('Páginas Simuladas (Armadilhas)');
  fireEvent.click(screen.getByRole('button', { name: /Novo Registro/i }));
  // O diálogo (MUI) renderiza em portal — aguarda o seletor aparecer.
  return await screen.findByRole('combobox');
}

// Seleciona o molde HBO Max no dropdown e carrega o HTML no editor.
async function carregarMoldeHbo() {
  const combobox = await abrirDialogo();
  fireEvent.mouseDown(combobox); // MUI Select abre no mousedown
  const opcao = await screen.findByRole('option', { name: HBO_MOLDE });
  fireEvent.click(opcao);
  fireEvent.click(screen.getByRole('button', { name: /Carregar HTML/i }));
}

describe('PhishingPages (Páginas Simuladas)', () => {
  it('renderiza a tela base, o seletor "Escolha a Interface" e valida campos obrigatórios vazios', async () => {
    await abrirDialogo();

    // Campos base + seletor presentes.
    expect(screen.getByLabelText('Escolha a Interface')).toBeInTheDocument();
    expect(screen.getByLabelText('Nome de Identificação Interna')).toBeInTheDocument();
    expect(screen.getByLabelText('Código Fonte (HTML + CSS)')).toBeInTheDocument();

    // Clicar em "Salvar Registro" com tudo vazio deve barrar e avisar.
    fireEvent.click(screen.getByRole('button', { name: /Salvar Registro/i }));
    expect(await screen.findByText(/Preencha o nome e o código HTML/i)).toBeInTheDocument();

    // Nenhuma requisição de salvamento (POST) deve ter sido disparada.
    expect(fetchMock).not.toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('ao selecionar o molde HBO Max no dropdown, popula a área de código automaticamente', async () => {
    await carregarMoldeHbo();

    const codeField = screen.getByLabelText('Código Fonte (HTML + CSS)') as HTMLTextAreaElement;
    await waitFor(() => expect(codeField.value).toContain('Mude sua senha'));
    // Preserva a fidelidade do gradiente do molde.
    expect(codeField.value).toContain('radial-gradient(120% 90% at 50% -10%');

    // O nome de identificação é preenchido com o prefixo de molde.
    const nomeField = screen.getByLabelText('Nome de Identificação Interna') as HTMLInputElement;
    expect(nomeField.value).toBe(`[Molde] ${HBO_MOLDE}`);
  });

  it('repassa o código do template para o container de preview (iframe) em tempo real', async () => {
    await carregarMoldeHbo();

    const iframe = screen.getByTitle('Live Preview') as HTMLIFrameElement;
    await waitFor(() => expect(iframe.getAttribute('srcdoc')).toContain('Mude sua senha'));
    expect(iframe.getAttribute('srcdoc')).toContain('radial-gradient(120% 90% at 50% -10%');
  });

  it('ao submeter, envia ao endpoint de salvamento as propriedades mapeadas corretamente', async () => {
    await carregarMoldeHbo();

    fireEvent.click(screen.getByRole('button', { name: /Salvar Registro/i }));

    // A chamada POST ao endpoint de PhishingPages deve ocorrer.
    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        API,
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: 'Bearer test-token',
          }),
        }),
      ),
    );

    // O corpo enviado carrega as propriedades mapeadas (nome + conteudoHtml).
    const postCall = fetchMock.mock.calls.find((c) => (c[1] as { method?: string })?.method === 'POST');
    expect(postCall).toBeTruthy();
    const body = JSON.parse((postCall![1] as { body: string }).body);
    expect(body.nome).toBe(`[Molde] ${HBO_MOLDE}`);
    expect(body.conteudoHtml).toContain('Mude sua senha');
  });
});
