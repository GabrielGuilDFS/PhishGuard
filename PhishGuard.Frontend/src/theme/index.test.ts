import { describe, it, expect } from 'vitest';
import { alpha } from '@mui/material/styles';
// Teste co-located (padrão de espelhamento do frontend).
import { createAppTheme, brandPalette, SOFT_BORDER_ALPHA } from './index';

// Card, TableCell e Paper `variant="outlined"` são a fonte REAL das bordas azuis
// vibrantes vistas em Settings, AdminDashboard e Templates (aba Páginas Educativas) —
// as três telas herdam este único override central, então o risco real de regressão
// mora aqui: se alguém reverter `alpha(c.secondary, SOFT_BORDER_ALPHA)` para o hex cru
// `c.secondary`, TODAS as telas voltam a ficar vibrantes de uma vez, silenciosamente.
describe('createAppTheme — bordas estruturais diluídas (SOFT_BORDER_ALPHA)', () => {
  it.each(['light', 'dark'] as const)('MuiCard usa a cor secundária diluída no modo %s, não a cor cheia', (mode) => {
    const theme = createAppTheme(mode);
    const secundariaCheia = brandPalette[mode].secondary;
    const bordaDiluida = alpha(secundariaCheia, SOFT_BORDER_ALPHA);

    const cardBorder = (theme.components?.MuiCard?.styleOverrides as any)?.root?.border;
    expect(cardBorder).toBe(`1px solid ${bordaDiluida}`);
    expect(cardBorder).not.toContain(secundariaCheia);
  });

  it.each(['light', 'dark'] as const)('MuiTableCell (linhas) usa a mesma diluição no modo %s', (mode) => {
    const theme = createAppTheme(mode);
    const bordaDiluida = alpha(brandPalette[mode].secondary, SOFT_BORDER_ALPHA);

    const cellBorder = (theme.components?.MuiTableCell?.styleOverrides as any)?.root?.borderBottom;
    expect(cellBorder).toBe(`1px solid ${bordaDiluida}`);
  });

  it.each(['light', 'dark'] as const)('MuiPaper variant="outlined" (previewers/cards de molde) usa a mesma diluição no modo %s', (mode) => {
    const theme = createAppTheme(mode);
    const bordaDiluida = alpha(brandPalette[mode].secondary, SOFT_BORDER_ALPHA);

    const outlinedBorderColor = (theme.components?.MuiPaper?.styleOverrides as any)?.outlined?.borderColor;
    expect(outlinedBorderColor).toBe(bordaDiluida);
  });

  it('cabeçalho de tabela (fill sólido, não borda) permanece intocado — fora do escopo da suavização', () => {
    const theme = createAppTheme('light');
    const headBg = (theme.components?.MuiTableCell?.styleOverrides as any)?.head?.backgroundColor;
    // Este é um PREENCHIMENTO (contraste de texto já calculado sobre ele), não uma
    // borda — não deve ser diluído junto, ou o texto do cabeçalho perde legibilidade.
    expect(headBg).toBe(brandPalette.light.secondary);
  });

  it('SOFT_BORDER_ALPHA é estritamente menor que opacidade total (a mudança não é cosmética/nula)', () => {
    expect(SOFT_BORDER_ALPHA).toBeGreaterThan(0);
    expect(SOFT_BORDER_ALPHA).toBeLessThan(1);
  });
});
