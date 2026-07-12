import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Componente real de produção vive no projeto do frontend. Este teste de UI foi
// separado do código de produção e mora em PhishGuard.Tests/Frontend.
import Templates from '../../PhishGuard.Frontend/src/pages/Templates';

// Refatoração "Cenários de Simulação": a Biblioteca de Modelos tem duas abas
// (Cenários de Simulação / Páginas Educativas). Um Cenário amarra a isca de e-mail
// à sua página falsa. Registrar um cenário persiste o PAR: uma linha em Templates
// (corpoHtml = id da isca) e uma em PhishingPages (conteudoHtml = id da landing).
const CENARIO_AMAZON = 'Amazon — Alerta de Segurança';
const ISCA_AMAZON_ID = 'amazon-notificacao-seguranca';
const LANDING_AMAZON_ID = 'amazon-login';
const API_TEMPLATES = 'http://localhost:5000/api/Templates';
const API_PHISHING = 'http://localhost:5000/api/PhishingPages';

// GET (listagens) devolve [] (nada registrado); POST/DELETE devolvem ok.
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

async function renderTela() {
  render(<Templates />);
  await screen.findByText('Biblioteca de Modelos');
}

describe('Templates (Biblioteca de Modelos)', () => {
  it('renderiza as duas abas e lista os cenários amarrados, sem entrada livre de HTML', async () => {
    await renderTela();

    expect(screen.getByRole('tab', { name: /Cenários de Simulação/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Páginas Educativas/i })).toBeInTheDocument();

    // Cenários do catálogo aparecem na tabela.
    expect(screen.getByText(CENARIO_AMAZON)).toBeInTheDocument();
    expect(screen.getByText('Netflix — Atualização de Cobrança')).toBeInTheDocument();
    expect(screen.getByText('HBO Max — Redefinição de Senha')).toBeInTheDocument();

    // Nada de editor de HTML bruto (removido na refatoração).
    expect(screen.queryByLabelText(/Código Fonte/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Nome de Identificação Interna/i)).not.toBeInTheDocument();
  });

  it('abre o preview emparelhado e alterna entre E-mail (SMTP) e Página Falsa', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` }));

    // Começa no modo E-mail: o iframe traz o corpo da isca da Amazon.
    const emailFrame = await screen.findByTitle('Email Preview') as HTMLIFrameElement;
    await waitFor(() => expect(emailFrame.getAttribute('srcdoc')).toContain('Alerta de segurança'));

    // Alterna para a Página Falsa (molde Amazon com Tailwind intacto).
    fireEvent.click(screen.getByRole('button', { name: /Página Falsa/i }));
    const landingFrame = await screen.findByTitle('Landing Preview') as HTMLIFrameElement;
    await waitFor(() => expect(landingFrame.getAttribute('srcdoc')).toContain('bg-[#131921]'));
  });

  it('ao registrar um cenário, persiste o PAR (Templates + PhishingPages) por identificador', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('button', { name: `Registrar ${CENARIO_AMAZON}` }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith(API_TEMPLATES, expect.objectContaining({ method: 'POST' }));
      expect(fetchMock).toHaveBeenCalledWith(API_PHISHING, expect.objectContaining({ method: 'POST' }));
    });

    const postTemplate = fetchMock.mock.calls.find(
      (c) => c[0] === API_TEMPLATES && (c[1] as { method?: string })?.method === 'POST',
    );
    const postPhishing = fetchMock.mock.calls.find(
      (c) => c[0] === API_PHISHING && (c[1] as { method?: string })?.method === 'POST',
    );
    expect(postTemplate).toBeTruthy();
    expect(postPhishing).toBeTruthy();

    const corpoTemplate = JSON.parse((postTemplate![1] as { body: string }).body);
    const corpoPhishing = JSON.parse((postPhishing![1] as { body: string }).body);

    // Persistência por identificador: nunca o HTML bruto.
    expect(corpoTemplate.corpoHtml).toBe(ISCA_AMAZON_ID);
    expect(corpoTemplate.corpoHtml).not.toContain('<html');
    expect(corpoPhishing.conteudoHtml).toBe(LANDING_AMAZON_ID);
    expect(corpoPhishing.conteudoHtml).not.toContain('<html');
  });

  it('na aba Páginas Educativas, mostra o previewer dos moldes fixos sem edição de HTML', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('tab', { name: /Páginas Educativas/i }));

    // Seletor de molde fixo + previewer presentes; nenhum textarea de código.
    const seletor = await screen.findByLabelText('Escolha o molde educativo');
    expect(seletor).toBeInTheDocument();
    expect(screen.getByTitle('Educational Preview')).toBeInTheDocument();
    expect(screen.queryByLabelText(/Código Fonte/i)).not.toBeInTheDocument();

    // Preview inicial carrega o primeiro molde do catálogo.
    const eduFrame = screen.getByTitle('Educational Preview') as HTMLIFrameElement;
    expect(eduFrame.getAttribute('srcdoc')).toContain('Phishing');

    // Registrar persiste apenas o molde escolhido no endpoint educacional.
    fireEvent.click(screen.getByRole('button', { name: /Registrar molde/i }));
    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        'http://localhost:5000/api/EducationalPages',
        expect.objectContaining({ method: 'POST' }),
      ),
    );
  });
});
