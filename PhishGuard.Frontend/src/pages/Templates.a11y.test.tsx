import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { axe } from 'vitest-axe';
import Templates from './Templates';

// Auditoria de acessibilidade automatizada (Passo 10 — Onda 2 da P1).
// 'color-contrast' desligada: jsdom não computa cores/layout (ver Login.a11y.test).
const axeOpts = { rules: { 'color-contrast': { enabled: false } } };

// A Biblioteca de Modelos é 100% estática (não faz fetch); o mock só flagra escrita
// indevida de uma regressão futura.
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

describe('Templates (Biblioteca de Modelos) — acessibilidade (axe)', () => {
  it('não tem violações de acessibilidade detectáveis', async () => {
    const { container } = render(
      <MemoryRouter>
        <Templates />
      </MemoryRouter>,
    );
    // Espera a tela montar antes de auditar.
    await screen.findByText('Biblioteca de Modelos');
    expect(await axe(container, axeOpts)).toHaveNoViolations();
  });
});
