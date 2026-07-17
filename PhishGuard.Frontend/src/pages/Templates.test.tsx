import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Teste de UI co-located: reside na mesma pasta do componente de produção que testa
// (padrão de espelhamento / mirroring adotado no frontend).
import Templates from './Templates';

// A Biblioteca de Modelos é um CATÁLOGO somente-leitura com duas abas (Cenários de
// Simulação / Páginas Educativas). Um Cenário amarra a isca de e-mail à sua página
// falsa. Registrar/Descartar cenário foram REMOVIDOS desta tela: o par de linhas
// (Templates + PhishingPages) nasce sob demanda ao salvar a campanha que o usa
// (garantirCenario em Campaigns.tsx). Aqui a tela não escreve nada no backend.
const CENARIO_AMAZON = 'Amazon — Alerta de Segurança';

// A tela é 100% estática (não faz fetch): o mock existe só para flagrar qualquer
// escrita indevida que uma regressão venha a introduzir.
const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => [] }));

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

  it('a aba Cenários é um catálogo somente-leitura: só Visualizar, sem Registrar nem Descartar', async () => {
    await renderTela();

    // A única ação por cenário é o preview. Os botões de registrar e de descartar
    // (delete) foram removidos da listagem principal — evita registro opcional e
    // exclusões acidentais direto daqui.
    expect(screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: `Registrar ${CENARIO_AMAZON}` })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: `Remover ${CENARIO_AMAZON}` })).not.toBeInTheDocument();

    // Navegar/visualizar a tela nunca dispara escrita no backend (nem POST nem DELETE).
    const escritas = fetchMock.mock.calls.filter(
      (c) => (c[1] as { method?: string } | undefined)?.method && (c[1] as { method?: string }).method !== 'GET',
    );
    expect(escritas).toHaveLength(0);
  });

  it('o diálogo de preview não oferece ação de registrar cenário', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` }));
    await screen.findByTitle('Email Preview');

    expect(screen.getByRole('button', { name: /Fechar/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Registrar Cenário/i })).not.toBeInTheDocument();
  });

  it('na aba Páginas Educativas, é um catálogo somente-leitura (sem botão de registrar)', async () => {
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

    // Decisão de UX: moldes educativos são fixos do sistema — não há registro manual.
    // A aba não deve mais expor botões de Registrar/Remover molde.
    expect(screen.queryByRole('button', { name: /Registrar molde/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Remover registro/i })).not.toBeInTheDocument();
    // E não dispara escrita no endpoint educacional só por navegar na aba.
    expect(fetchMock).not.toHaveBeenCalledWith(
      'http://localhost:5000/api/EducationalPages',
      expect.objectContaining({ method: 'POST' }),
    );
  });
});
