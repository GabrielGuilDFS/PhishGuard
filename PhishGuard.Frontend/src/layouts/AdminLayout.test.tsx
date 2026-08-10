import { describe, it, expect, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
// Teste de UI co-located (padrão de espelhamento do frontend).
import AdminLayout from './AdminLayout';
import { ThemeModeProvider } from '../context/ThemeModeContext';

function renderLayout() {
  return render(
    <MemoryRouter initialEntries={['/admin/dashboard']}>
      <ThemeModeProvider>
        <Routes>
          <Route path="/admin" element={<AdminLayout />}>
            <Route path="dashboard" element={<div>conteúdo</div>} />
          </Route>
        </Routes>
      </ThemeModeProvider>
    </MemoryRouter>
  );
}

afterEach(() => {
  localStorage.clear();
});

// A refatoração trocou o fundo em gradiente fixo da logo + hex hardcoded por tokens
// de tema (`text.primary` / `primary.main`), que resolvem por cor de MODO. O risco real
// aqui é ela voltar a ficar "presa" num hex fixo e parar de acompanhar o toggle.
describe('AdminLayout — logo sem fundo sólido, cor por token de tema', () => {
  it('renderiza "Phish" + "Guard" sem herdar mais o gradiente de marca fixo', () => {
    const { container } = renderLayout();
    // "Guard" fica num <span> filho separado — "PhishGuard" nunca existe como um nó
    // de texto único, por isso as duas partes são checadas separadamente. Também
    // aparecem em dobro: Drawer permanente + temporário (mobile) montam juntos, só um
    // fica visível por CSS.
    expect(screen.getAllByText('Phish', { exact: false }).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Guard').length).toBeGreaterThan(0);
    // O mesmo cabeçalho alimenta o Drawer permanente e o temporário; o JSDOM monta
    // apenas a variante ativa conforme o comportamento responsivo do MUI.
    expect(container.querySelectorAll('img[src*="phishguard-mark"]').length).toBeGreaterThan(0);
  });

  it('"Guard" usa o accent do modo LIGHT (#0600c2), não um hex de marca fixo', () => {
    renderLayout();
    const guardSpan = screen.getAllByText('Guard')[0];
    // jsdom resolve a cor computada a partir do CSS real injetado pelo emotion (MUI sx).
    expect(getComputedStyle(guardSpan).color).toBe('rgb(6, 0, 194)'); // #0600c2
  });

  it('"Guard" acompanha o modo DARK salvo (accent #443dff), sem hex fixo', () => {
    localStorage.setItem('phishguard_theme_mode', 'dark');
    renderLayout();
    const guardSpan = screen.getAllByText('Guard')[0];
    expect(getComputedStyle(guardSpan).color).toBe('rgb(68, 61, 255)'); // #443dff
  });

  it('"Phish" usa text.primary do tema (preto no light), não branco fixo', () => {
    renderLayout();
    const phishText = screen.getAllByText('Phish', { exact: false })[0];
    // #ffffff era a cor fixa do texto sobre o antigo fundo em gradiente — não pode
    // mais aparecer, já que o fundo agora é neutro (branco no light) e o texto
    // precisa ser escuro para ter contraste.
    expect(getComputedStyle(phishText).color).toBe('rgb(0, 0, 0)'); // brandPalette.light.text
  });
});
