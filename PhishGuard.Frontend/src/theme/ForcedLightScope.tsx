import { ThemeProvider } from '@mui/material/styles';
import { Box } from '@mui/material';
import type { ReactNode } from 'react';
import { forcedLightTheme } from './forcedLight';

/**
 * Escopo que TRAVA um subtree no modo light — usado em LandingHome, Login e Register
 * (ver App.tsx, onde envolve essas três rotas). Nenhuma dessas telas responde ao toggle
 * de tema do painel.
 *
 * As duas camadas são necessárias porque cobrem caminhos diferentes:
 * - `<ThemeProvider>` cobre o que lê o tema do MUI (`sx`, palette, componentes). Sozinho
 *   NÃO basta: `var(--background)` e as classes do Tailwind continuariam resolvendo pelo
 *   `:root[data-theme="dark"]` do <html>.
 * - `.forced-light-theme` redeclara as variáveis da paleta NESTE elemento. Variáveis CSS
 *   herdam por proximidade, então a redeclaração no descendente vence a do <html>
 *   independentemente de especificidade — é o que neutraliza o data-theme/.dark global.
 *
 * O Box é full-bleed (minHeight 100vh) de propósito: o <body> é pintado pelo CssBaseline
 * do tema GLOBAL e ficaria preto por trás no modo dark. Não montamos um <CssBaseline>
 * aqui — ele estiliza o <body>, que é compartilhado, e vazaria o light para o painel.
 */
export default function ForcedLightScope({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider theme={forcedLightTheme}>
      <Box className="forced-light-theme" sx={{ minHeight: '100vh' }}>
        {children}
      </Box>
    </ThemeProvider>
  );
}
