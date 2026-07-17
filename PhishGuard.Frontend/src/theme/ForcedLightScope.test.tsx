import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { useTheme } from '@mui/material/styles';
// Teste co-located (padrão de espelhamento do frontend): vive ao lado do componente.
import ForcedLightScope from './ForcedLightScope';
import { ThemeModeProvider } from '../context/ThemeModeContext';
import { brandPalette } from './index';

// A vitrine pública (LandingHome, Login, Register) é TRAVADA no light. O risco real que
// estes testes cobrem: o painel estar em dark e a landing herdar o tema escuro — algo
// que só aparece quando alguém abre a home depois de usar o admin no modo escuro.
const STORAGE_KEY = 'phishguard_theme_mode';

/** Sonda: publica o que o MUI de fato entregou ao subtree. */
function ProbeDoTema() {
  const theme = useTheme();
  return (
    <>
      <span data-testid="mode">{theme.palette.mode}</span>
      <span data-testid="bg">{theme.palette.background.default}</span>
      <span data-testid="paper">{theme.palette.background.paper}</span>
      <span data-testid="text">{theme.palette.text.primary}</span>
      <span data-testid="text-secondary">{theme.palette.text.secondary}</span>
      <span data-testid="primary">{theme.palette.primary.main}</span>
    </>
  );
}

beforeEach(() => {
  // Pior cenário: usuário deixou o PAINEL em dark; a vitrine não pode acompanhar.
  localStorage.setItem(STORAGE_KEY, 'dark');
});

afterEach(() => {
  localStorage.clear();
  document.documentElement.removeAttribute('data-theme');
  document.documentElement.classList.remove('dark');
});

describe('ForcedLightScope — trava da vitrine pública no modo light', () => {
  it('entrega tema light mesmo com o painel global em dark', () => {
    render(
      <ThemeModeProvider>
        <ForcedLightScope>
          <ProbeDoTema />
        </ForcedLightScope>
      </ThemeModeProvider>
    );

    // O <html> global ESTÁ em dark — é exatamente o que o escopo precisa ignorar.
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');

    expect(screen.getByTestId('mode').textContent).toBe('light');
    // A vitrine usa `surface` (branco puro), não `background` (canvas fosco do painel).
    expect(screen.getByTestId('bg').textContent).toBe(brandPalette.light.surface); // #ffffff
    expect(screen.getByTestId('text').textContent).toBe(brandPalette.light.text);     // #000000
    expect(screen.getByTestId('primary').textContent).toBe(brandPalette.light.accent); // #0600c2
    expect(screen.getByTestId('paper').textContent).toBe(brandPalette.light.primary);  // #6682f5
  });

  it('mantém o texto de apoio legível: nunca igual ao fundo do card', () => {
    render(
      <ThemeModeProvider>
        <ForcedLightScope>
          <ProbeDoTema />
        </ForcedLightScope>
      </ThemeModeProvider>
    );

    // Regressão do desvio documentado em forcedLight.ts: text.secondary = #6682f5
    // (valor originalmente especificado) seria a MESMA cor do paper → contraste 1.00,
    // e todo label de TextField sumiria dentro dos cards.
    const textoApoio = screen.getByTestId('text-secondary').textContent;
    const fundoCard = screen.getByTestId('paper').textContent;
    expect(textoApoio).not.toBe(fundoCard);
  });

  it('marca o subtree com a classe que neutraliza as variáveis CSS do dark', () => {
    const { container } = render(
      <ThemeModeProvider>
        <ForcedLightScope>
          <span>conteúdo</span>
        </ForcedLightScope>
      </ThemeModeProvider>
    );

    // Camada CSS do bloqueio: sem esta classe, `var(--background)` e as classes do
    // Tailwind resolveriam pelo :root[data-theme="dark"] do <html>.
    expect(container.querySelector('.forced-light-theme')).not.toBeNull();
  });

  it('não vaza o light para fora do escopo (o painel segue dark)', () => {
    render(
      <ThemeModeProvider>
        <div>
          <ForcedLightScope>
            <span>vitrine</span>
          </ForcedLightScope>
          {/* Fora do escopo = painel administrativo, que deve seguir o toggle. */}
          <ProbeDoTema />
        </div>
      </ThemeModeProvider>
    );

    expect(screen.getByTestId('mode').textContent).toBe('dark');
    expect(screen.getByTestId('bg').textContent).toBe(brandPalette.dark.background); // #0a0a0a
  });
});
