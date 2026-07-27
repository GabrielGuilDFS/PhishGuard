import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
// Teste de UI co-located: reside na mesma pasta do componente de produção que testa
// (padrão de espelhamento / mirroring adotado no frontend).
import Templates from './Templates';

// A Biblioteca de Modelos é um CATÁLOGO somente-leitura de FLUXO ÚNICO: uma tabela de
// Cenários (sem abas de topo). Clicar num cenário abre um diálogo com três abas de
// preview — E-mail, Página Falsa e Página Educacional. Um Cenário amarra a isca de
// e-mail à sua página falsa e ao seu treinamento. Registrar/Descartar cenário foram
// REMOVIDOS: o par de linhas nasce sob demanda ao salvar a campanha (Campaigns.tsx).
// Cenário renomeado para a paródia fictícia "amzprime" (compliance de IP).
const CENARIO_AMAZON = 'amzprime — Alerta de Segurança';

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
  // A tela vive sob o BrowserRouter do app (rota /admin/templates); o preview do
  // treinamento educacional usa hooks de roteamento — daí o MemoryRouter no teste.
  render(
    <MemoryRouter>
      <Templates />
    </MemoryRouter>,
  );
  await screen.findByText('Biblioteca de Modelos');
}

describe('Templates (Biblioteca de Modelos)', () => {
  it('é um fluxo único: sem abas de topo, com a tabela de cenários e a coluna "Observação"', async () => {
    await renderTela();

    // Fluxo único: nenhuma aba de navegação no topo (dialog fechado => sem tablist).
    expect(screen.queryByRole('tab')).not.toBeInTheDocument();

    // Cenários do catálogo aparecem direto na tabela.
    expect(screen.getByText(CENARIO_AMAZON)).toBeInTheDocument();
    expect(screen.getByText('NetsFlix — Atualização de Cobrança')).toBeInTheDocument();
    expect(screen.getByText('bho MAX — Redefinição de Senha')).toBeInTheDocument();

    // Coluna renomeada: "Observação" no lugar de "Amarração".
    expect(screen.getByRole('columnheader', { name: /Observação/i })).toBeInTheDocument();
    expect(screen.queryByRole('columnheader', { name: /Amarração/i })).not.toBeInTheDocument();

    // Nada de editor de HTML bruto (removido na refatoração).
    expect(screen.queryByLabelText(/Código Fonte/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Nome de Identificação Interna/i)).not.toBeInTheDocument();
  });

  it('o diálogo alterna entre as três abas: E-mail, Página Falsa e Página Educacional', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` }));

    // Começa no modo E-mail: o iframe traz o corpo da isca da amzprime.
    const emailFrame = await screen.findByTitle('Email Preview') as HTMLIFrameElement;
    await waitFor(() => expect(emailFrame.getAttribute('srcdoc')).toContain('Alerta de segurança'));

    // Alterna para a Página Falsa (molde amzprime auto-contido: CSS embutido, sem CDN).
    fireEvent.click(screen.getByRole('tab', { name: /Página Falsa/i }));
    const landingFrame = await screen.findByTitle('Landing Preview') as HTMLIFrameElement;
    const srcdoc = () => landingFrame.getAttribute('srcdoc') ?? '';
    // A cor do header (#131921) vive no <style> embutido, não numa classe Tailwind —
    // e NÃO pode depender do Play CDN (bloqueado pelo sandbox do iframe).
    await waitFor(() => expect(srcdoc()).toContain('#131921'));
    expect(srcdoc()).not.toContain('cdn.tailwindcss.com');
    // Reset anti "tudo azul sublinhado": links herdam a cor e não sublinham por padrão.
    expect(srcdoc()).toContain('text-decoration: none');

    // Alterna para a Página Educacional: o treinamento INTERATIVO do cenário amzprime
    // é renderizado DIRETO (modo preview), não recarregado num iframe de rota.
    fireEvent.click(screen.getByRole('tab', { name: /Página Educacional/i }));
    expect(
      await screen.findByText(/simulação de treinamento de segurança do PhishGuard/i),
    ).toBeInTheDocument();
    // Botão de conclusão presente, porém ilustrativo no preview (não navega).
    expect(screen.getByRole('button', { name: /Concluir Treinamento/i })).toBeInTheDocument();
  });

  it('a coluna Cenário mostra só o título (sem descrição embutida)', async () => {
    await renderTela();

    // O subtexto descritivo NÃO fica mais na célula do título — migrou para "Observação".
    const celulaTitulo = screen.getByText(CENARIO_AMAZON);
    expect(celulaTitulo).toBeInTheDocument();
    // A descrição do cenário aparece na tabela (coluna Observação), não sob o título.
    expect(
      screen.getByText(/direciona o alvo à página falsa de alteração de senha/i),
    ).toBeInTheDocument();
  });

  it('é um catálogo somente-leitura: linha clicável, sem coluna Ações nem Registrar/Descartar', async () => {
    await renderTela();

    // Não há mais coluna "Ações" nem botões de registrar/descartar.
    expect(screen.queryByRole('columnheader', { name: /Ações/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: `Registrar ${CENARIO_AMAZON}` })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: `Remover ${CENARIO_AMAZON}` })).not.toBeInTheDocument();

    // A affordance de preview vive na LINHA inteira (role=button + tabIndex p/ teclado).
    const linha = screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` });
    expect(linha.tagName).toBe('TR');
    expect(linha).toHaveAttribute('tabindex', '0');

    // Dica textual sutil de que a linha é clicável.
    expect(screen.getByText(/Clique em um cenário para visualizar/i)).toBeInTheDocument();

    // Navegar/visualizar a tela nunca dispara escrita no backend (nem POST nem DELETE).
    const escritas = fetchMock.mock.calls.filter((c) => {
      const opts = (c as unknown as [string, RequestInit | undefined])[1];
      return opts?.method && opts.method !== 'GET';
    });
    expect(escritas).toHaveLength(0);
  });

  it('abre o preview clicando em QUALQUER lugar da linha do cenário', async () => {
    await renderTela();

    // Clicar numa célula interna (a categoria) — não num botão dedicado — abre o preview.
    fireEvent.click(screen.getByText(CENARIO_AMAZON));
    expect(await screen.findByTitle('Email Preview')).toBeInTheDocument();
  });

  it('o diálogo de preview não oferece ação de registrar cenário', async () => {
    await renderTela();

    fireEvent.click(screen.getByRole('button', { name: `Visualizar ${CENARIO_AMAZON}` }));
    await screen.findByTitle('Email Preview');

    expect(screen.getByRole('button', { name: /Fechar/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Registrar Cenário/i })).not.toBeInTheDocument();
  });
});
