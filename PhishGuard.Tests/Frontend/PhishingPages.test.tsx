import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Componente real de produção vive no projeto do frontend. Este teste de UI foi
// separado do código de produção e mora em PhishGuard.Tests/Frontend.
import PhishingPages from '../../PhishGuard.Frontend/src/pages/PhishingPages';

// Molde HBO Max consolidado (deve existir no dropdown "Escolha a Interface").
const HBO_MOLDE = 'HBO Max - Redefinição de Senha';
const HBO_ID = 'hbomax-redefinicao-senha';
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

// Abre o diálogo "Nova Armadilha" clicando em "Novo Registro".
async function abrirDialogo() {
  render(<PhishingPages />);
  await screen.findByText('Páginas Simuladas (Armadilhas)');
  fireEvent.click(screen.getByRole('button', { name: /Novo Registro/i }));
  // O diálogo (MUI) renderiza em portal — aguarda o seletor aparecer.
  return await screen.findByRole('combobox');
}

// Seleciona o molde HBO Max no dropdown e o carrega no estado local.
async function carregarMoldeHbo() {
  const combobox = await abrirDialogo();
  fireEvent.mouseDown(combobox); // MUI Select abre no mousedown
  const opcao = await screen.findByRole('option', { name: HBO_MOLDE });
  fireEvent.click(opcao);
  fireEvent.click(screen.getByRole('button', { name: /Carregar HTML/i }));
}

describe('PhishingPages (Páginas Simuladas)', () => {
  it('renderiza a tela base e o seletor "Escolha a Interface", sem campos de entrada livre', async () => {
    await abrirDialogo();

    // O seletor de molde oficial está presente.
    expect(screen.getByLabelText('Escolha a Interface')).toBeInTheDocument();

    // Os campos de entrada livre foram REMOVIDOS no MVP.
    expect(screen.queryByLabelText('Nome de Identificação Interna')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Código Fonte (HTML + CSS)')).not.toBeInTheDocument();

    // Salvar sem carregar um molde deve barrar e avisar.
    fireEvent.click(screen.getByRole('button', { name: /Salvar Registro/i }));
    expect(await screen.findByText(/carregue um molde oficial antes de salvar/i)).toBeInTheDocument();

    // Nenhuma requisição de salvamento (POST) deve ter sido disparada.
    expect(fetchMock).not.toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('ao carregar o molde HBO Max, renderiza a captura no preview (iframe) em tempo real', async () => {
    await carregarMoldeHbo();

    const iframe = screen.getByTitle('Live Preview') as HTMLIFrameElement;
    await waitFor(() => expect(iframe.getAttribute('srcdoc')).toContain('Mude sua senha'));
    // Preserva a fidelidade do gradiente do molde.
    expect(iframe.getAttribute('srcdoc')).toContain('radial-gradient(120% 90% at 50% -10%');
  });

  it('ao submeter, envia ao backend apenas o identificador do molde (não o HTML)', async () => {
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

    // O corpo enviado carrega o NOME do molde e o ID (não a string massiva de HTML).
    const postCall = fetchMock.mock.calls.find((c) => (c[1] as { method?: string })?.method === 'POST');
    expect(postCall).toBeTruthy();
    const body = JSON.parse((postCall![1] as { body: string }).body);
    expect(body.nome).toBe(HBO_MOLDE);
    expect(body.conteudoHtml).toBe(HBO_ID);
    // Garante que NÃO estamos mais enviando o HTML bruto.
    expect(body.conteudoHtml).not.toContain('<html');
  });
});
