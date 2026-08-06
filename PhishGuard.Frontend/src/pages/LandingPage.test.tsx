import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import LandingPage from './LandingPage';

// ============================================================================
// Testa o COMPONENTE REAL LandingPage — o que os testes de cenário NÃO faziam
// (eles reimplementavam a substituição de placeholder e renderizavam o molde
// estático direto). Aqui o fluxo real é exercitado ponta a ponta:
//   fetch(/api/PhishingPages/:id) → resolve molde por ID → substitui
//   {{CAMPAIGN_ID}}/{{TARGET_ID}} → dangerouslySetInnerHTML.
// A API é interceptada por MSW; o componente roda de verdade dentro das rotas.
// ============================================================================

const server = setupServer();
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const CAMP = 'camp-abc';
const TGT = 'tgt-xyz';
const TRACKING_TOKEN = 'token-assinado';

function renderLanding(pageId: string) {
  return render(
    <MemoryRouter initialEntries={[`/landing/${pageId}?c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`]}>
      <Routes>
        <Route path="/landing/:id" element={<LandingPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('LandingPage (componente real)', () => {
  it('resolve o molde oficial por ID e substitui os placeholders de campanha/alvo', async () => {
    server.use(
      http.get('/api/PhishingPages/:id', () => HttpResponse.json({ conteudoHtml: 'amazon-login' })),
    );

    const { container } = renderLanding('pp-1');

    // O molde 'amazon-login' foi resolvido e renderizado (form de captura presente).
    await waitFor(() => expect(container.querySelector('form')).toBeInTheDocument());
    expect(container.querySelector('#amz-new')).toBeInTheDocument();
    expect(container.querySelector('#amz-confirm')).toBeInTheDocument();

    // Os placeholders foram substituídos pelos valores da query string: o gatilho de
    // telemetria embutido no <form> aponta para a URL real, sem {{...}} remanescente.
    const html = container.innerHTML;
    expect(container.querySelector('form')).not.toHaveAttribute('onsubmit');
    expect(html).not.toMatch(/\{\{\s*CAMPAIGN_ID\s*\}\}/);
    expect(html).not.toMatch(/\{\{\s*TARGET_ID\s*\}\}/);
    expect(html).not.toMatch(/\{\{\s*TRACKING_TOKEN\s*\}\}/);
  });

  it('fallback legado: quando o conteúdo não casa um molde, injeta o HTML bruto do banco', async () => {
    // Registro legado (anterior à refatoração): conteudoHtml é HTML cru, não um ID.
    const htmlLegado = '<form id="legacy-form"><input id="legacy-input" /></form>';
    server.use(
      http.get('/api/PhishingPages/:id', () => HttpResponse.json({ conteudoHtml: htmlLegado })),
    );

    const { container } = renderLanding('pp-legada');

    await waitFor(() => expect(container.querySelector('#legacy-form')).toBeInTheDocument());
    expect(container.querySelector('#legacy-input')).toBeInTheDocument();
  });

  it('erro da API (404/rede) renderiza a mensagem de link expirado, sem quebrar', async () => {
    server.use(
      http.get('/api/PhishingPages/:id', () => new HttpResponse(null, { status: 404 })),
    );

    renderLanding('pp-inexistente');

    await waitFor(() => expect(screen.getByText(/link expirado/i)).toBeInTheDocument());
  });
});
