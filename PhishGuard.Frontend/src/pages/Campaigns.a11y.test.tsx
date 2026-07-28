import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { axe } from 'vitest-axe';
import Campaigns from './Campaigns';
import { NotificationProvider } from '../context/NotificationContext';

// Auditoria de acessibilidade automatizada (Passo 10 — Onda 2 da P1).
// 'color-contrast' desligada: jsdom não computa cores/layout (ver Login.a11y.test).
const axeOpts = { rules: { 'color-contrast': { enabled: false } } };

// Timers REAIS aqui (o teste funcional usa timers falsos p/ o polling; a auditoria
// não precisa deles). A lista volta vazia → sem campanha pendente → sem polling.
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

describe('Campaigns — acessibilidade (axe)', () => {
  it('não tem violações de acessibilidade detectáveis', async () => {
    const { container } = render(
      <NotificationProvider>
        <Campaigns />
      </NotificationProvider>,
    );
    // Espera a montagem (título da tela) antes de auditar.
    await screen.findByRole('heading', { name: /Gerenciamento de Campanhas/i });
    expect(await axe(container, axeOpts)).toHaveNoViolations();
  });
});
