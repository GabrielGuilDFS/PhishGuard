import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Componente real de produção vive no projeto do frontend. Este teste de UI foi
// separado do código de produção e mora em PhishGuard.Tests/Frontend.
import Templates from '../../PhishGuard.Frontend/src/pages/Templates';

// Isca oficial Amazon (deve existir no seletor "Escolha a Isca").
const AMAZON_ISCA = 'Amazon - Notificação Geral';
const AMAZON_ID = 'amazon-notificacao-geral';
const API = 'http://localhost:5000/api/Templates';

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

// Abre o diálogo "Novo Cenário" clicando no botão do cabeçalho.
async function abrirDialogo() {
  render(<Templates />);
  await screen.findByText('Biblioteca de Cenários');
  fireEvent.click(screen.getByRole('button', { name: /Novo Cenário/i }));
  // O diálogo (MUI) renderiza em portal — aguarda o seletor aparecer.
  return await screen.findByRole('combobox');
}

// Seleciona a isca Amazon no seletor e a carrega no estado local.
async function carregarIscaAmazon() {
  const combobox = await abrirDialogo();
  fireEvent.mouseDown(combobox); // MUI Select abre no mousedown
  const opcao = await screen.findByRole('option', { name: new RegExp(AMAZON_ISCA, 'i') });
  fireEvent.click(opcao);
  fireEvent.click(screen.getByRole('button', { name: /Carregar Isca/i }));
}

describe('Templates (Biblioteca de Cenários)', () => {
  it('renderiza a tela base e o seletor de iscas, sem campos de entrada livre', async () => {
    await abrirDialogo();

    // O seletor de isca oficial está presente.
    expect(screen.getByLabelText('Escolha a Isca')).toBeInTheDocument();

    // Entradas livres foram REMOVIDAS no MVP: nada de "Assistente Rápido" nem TextArea de HTML.
    expect(screen.queryByText(/Assistente Rápido/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Corpo do E-mail/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Nome de Identificação Interna/i)).not.toBeInTheDocument();

    // Salvar sem carregar uma isca deve barrar e avisar.
    fireEvent.click(screen.getByRole('button', { name: /Salvar Cenário/i }));
    expect(await screen.findByText(/carregue uma isca oficial antes de salvar/i)).toBeInTheDocument();

    // Nenhuma requisição de salvamento (POST) deve ter sido disparada.
    expect(fetchMock).not.toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('ao carregar a isca Amazon, renderiza o e-mail no preview (iframe)', async () => {
    await carregarIscaAmazon();

    const iframe = screen.getByTitle('Email Preview') as HTMLIFrameElement;
    await waitFor(() => expect(iframe.getAttribute('srcdoc')).toContain('Amazon'));
  });

  it('ao submeter, envia ao backend apenas o identificador da isca (não o HTML)', async () => {
    await carregarIscaAmazon();

    fireEvent.click(screen.getByRole('button', { name: /Salvar Cenário/i }));

    // A chamada POST ao endpoint de Templates deve ocorrer.
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

    // O corpo enviado carrega os metadados curtos + o ID da isca em corpoHtml.
    const postCall = fetchMock.mock.calls.find((c) => (c[1] as { method?: string })?.method === 'POST');
    expect(postCall).toBeTruthy();
    const body = JSON.parse((postCall![1] as { body: string }).body);
    expect(body.nome).toBe(AMAZON_ISCA);
    expect(body.corpoHtml).toBe(AMAZON_ID);
    // Garante que NÃO estamos mais enviando o HTML bruto.
    expect(body.corpoHtml).not.toContain('<html');
    expect(body.corpoHtml).not.toContain('<!doctype');
  });
});
