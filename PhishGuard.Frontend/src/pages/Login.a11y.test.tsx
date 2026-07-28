import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { axe } from 'vitest-axe';
import Login from './Login';
import { NotificationProvider } from '../context/NotificationContext';

// Auditoria de acessibilidade automatizada (Passo 10 — Onda 2 da P1).
// O axe-core roda sobre o DOM renderizado e falha se houver violação de WCAG/ARIA.
//
// A regra 'color-contrast' fica DESLIGADA: o jsdom não faz layout nem calcula cores
// computadas, então a checagem de contraste é sempre inconclusiva/ruidosa aqui —
// contraste é validado visualmente (ver [[tema-paleta-azul-global]]), não no unit.
const axeOpts = { rules: { 'color-contrast': { enabled: false } } };

function renderLogin() {
  return render(
    <MemoryRouter initialEntries={['/login']}>
      <NotificationProvider>
        <Routes>
          <Route path="/" element={<div>home</div>} />
          <Route path="/login" element={<Login />} />
        </Routes>
      </NotificationProvider>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) })));
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('Login — acessibilidade (axe)', () => {
  it('não tem violações de acessibilidade detectáveis', async () => {
    const { container } = renderLogin();
    expect(await axe(container, axeOpts)).toHaveNoViolations();
  });
});
