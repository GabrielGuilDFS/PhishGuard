import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import LandingPage from './LandingPage';
import { NotificationProvider } from '../context/NotificationContext';

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
    <NotificationProvider>
    <MemoryRouter initialEntries={[`/landing/${pageId}?c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`]}>
      <Routes>
        <Route path="/landing/:id" element={<LandingPage />} />
      </Routes>
    </MemoryRouter>
    </NotificationProvider>,
  );
}

describe('LandingPage (componente real)', () => {
  it('carrega o conteúdo preservado da campanha com a capability assinada', async () => {
    let requestUrl: URL | undefined;
    server.use(http.get('/api/PhishingPages/:id', ({ request }) => {
      requestUrl = new URL(request.url);
      return HttpResponse.json({ conteudoHtml: '<form id="snapshot-form"><p>Conteúdo preservado</p></form>', templateId: 'amazon-login' });
    }));
    const { container } = renderLanding('pp-snapshot');
    expect(await screen.findByText('Conteúdo preservado')).toBeInTheDocument();
    expect(container.querySelector('#snapshot-form')).toBeInTheDocument();
    expect(container.querySelector('#amz-new')).not.toBeInTheDocument();
    expect(requestUrl?.searchParams.get('c')).toBe(CAMP);
    expect(requestUrl?.searchParams.get('t')).toBe(TGT);
    expect(requestUrl?.searchParams.get('k')).toBe(TRACKING_TOKEN);
  });

  it.each(['mercado-liv-login', 'mercadoliv-login'])(
    'resolve %s e mantém o rastreamento assinado sem transmitir credenciais', async (templateId) => {
      let submitted: { url: string; body: string } | undefined;
      server.use(
        http.get('/api/PhishingPages/:id', () => HttpResponse.json({ conteudoHtml: templateId })),
        http.post('/api/tracking/submit/:campaign/:target', async ({ request }) => {
          submitted = { url: request.url, body: await request.text() };
          return HttpResponse.json({});
        }),
      );
      const locationDescriptor = Object.getOwnPropertyDescriptor(window, 'location');
      const locationStub = { href: window.location.href, origin: window.location.origin, search: '' };
      Object.defineProperty(window, 'location', { configurable: true, value: locationStub });
      try {
        const { container } = renderLanding('pp-mercado');
        await screen.findByText('Digite seu e-mail e senha atual para alterar sua senha');
        const form = container.querySelector('form')!;
        expect(form).not.toHaveAttribute('onsubmit');
        fireEvent.change(container.querySelector('#ml-email')!, { target: { value: 'alvo@example.test' } });
        fireEvent.change(container.querySelector('#ml-password')!, { target: { value: 'SenhaFicticia123' } });
        fireEvent.submit(form);

        await waitFor(() => expect(submitted).toBeDefined());
        const url = new URL(submitted!.url);
        expect(url.pathname).toBe(`/api/tracking/submit/${CAMP}/${TGT}`);
        expect(url.searchParams.get('k')).toBe(TRACKING_TOKEN);
        expect(JSON.parse(submitted!.body)).toMatchObject({ camposPreenchidos: true, tamanhoSenha: 'SenhaFicticia123'.length });
        expect(submitted!.body).not.toContain('SenhaFicticia123');
        expect(submitted!.body).not.toContain('alvo@example.test');
        await waitFor(() => expect(locationStub.href).toBe(
          `/educational-feedback?template=mercadoliv&c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`,
        ));
      } finally {
        if (locationDescriptor) Object.defineProperty(window, 'location', locationDescriptor);
      }
    },
  );

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
