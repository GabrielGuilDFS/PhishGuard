import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { render, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { axe } from 'vitest-axe';
import LandingPage from './LandingPage';

// Auditoria de acessibilidade automatizada das landings (Passo 10 — Onda 2 da P1).
// A landing servida é HTML de molde oficial resolvido por ID via API (interceptada
// por MSW) — o mesmo fluxo real de LandingPage.test.tsx.
//
// 'color-contrast' desligada: jsdom não computa cores/layout. As demais regras de
// WCAG/ARIA (labels de form, landmarks, atributos) continuam ativas — é justamente
// nas landings (HTML de terceiros clonado) que essas violações costumam surgir.
const axeOpts = { rules: { 'color-contrast': { enabled: false } } };

const server = setupServer();
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

function renderLanding(conteudoHtml: string) {
  server.use(
    http.get('/api/PhishingPages/:id', () => HttpResponse.json({ conteudoHtml })),
  );
  return render(
    <MemoryRouter initialEntries={['/landing/pp-1?c=camp-abc&t=tgt-xyz']}>
      <Routes>
        <Route path="/landing/:id" element={<LandingPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('LandingPage — acessibilidade (axe)', () => {
  // Amostra representativa dos moldes oficiais (login de captura). Adicione novos
  // moldes a esta lista conforme forem entrando na biblioteca.
  it.each(['amazon-login', 'netflix-login'])(
    'molde "%s" não tem violações de acessibilidade detectáveis',
    async (molde) => {
      const { container } = renderLanding(molde);
      await waitFor(() => expect(container.querySelector('form')).toBeInTheDocument());
      expect(await axe(container, axeOpts)).toHaveNoViolations();
    },
  );
});
