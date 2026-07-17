import { describe, it, expect, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { alpha } from '@mui/material/styles';
// Teste de UI co-located (padrão de espelhamento do frontend).
import Settings from './Settings';
import { ThemeModeProvider } from '../context/ThemeModeContext';
import { NotificationProvider } from '../context/NotificationContext';
import { brandPalette } from '../theme';

function renderSettings() {
  return render(
    <ThemeModeProvider>
      <NotificationProvider>
        <Settings />
      </NotificationProvider>
    </ThemeModeProvider>
  );
}

afterEach(() => {
  // Sem token: os dois useEffect de carga (perfil/SMTP) fazem early-return antes de
  // chamar fetch — a tela renderiza puramente síncrona, sem precisar mockar rede.
  localStorage.clear();
});

describe('Settings — bordas suavizadas', () => {
  it('painel principal não tem borda própria (só a sombra de elevation delimita)', () => {
    renderSettings();
    // O h4 "Configurações do Sistema" é irmão do Paper; o Paper é o próximo elemento.
    const titulo = screen.getByRole('heading', { name: 'Configurações do Sistema' });
    const paper = titulo.nextElementSibling as HTMLElement;
    expect(paper).toHaveClass('MuiPaper-root');
    // Antes: `border: 1, borderColor: 'divider'` (borda sólida cheia). Removida —
    // sem essa asserção, um retorno acidental da borda passaria despercebido.
    // jsdom não resolve o valor inicial da spec ('none') para propriedades nunca
    // declaradas — devolve string vazia; é o sinal correto de "nenhuma borda setada".
    expect(getComputedStyle(paper).borderStyle).toBe('');
  });

  it('separador entre a faixa de abas e o conteúdo usa a cor secundária diluída, não a cor cheia', () => {
    renderSettings();
    // O <Box sx={{ borderBottom }}> envolve o <Tabs> inteiro (MuiTabs-root), não só o
    // elemento role="tablist" — que fica bem mais fundo na árvore interna do MUI
    // (tablist → MuiTabs-scroller → MuiTabs-root → o Box procurado).
    const tabsRoot = screen.getByRole('tablist').closest('.MuiTabs-root') as HTMLElement;
    const wrapperDoDivisor = tabsRoot.parentElement as HTMLElement;

    const corEsperada = alpha(brandPalette.light.secondary, 0.16);
    expect(getComputedStyle(wrapperDoDivisor).borderBottomColor).toBe(corEsperada);
  });
});
