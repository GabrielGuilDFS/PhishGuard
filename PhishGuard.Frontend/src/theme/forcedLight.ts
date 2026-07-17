import { createTheme, type Theme } from '@mui/material/styles';
import { brandPalette, statusColors, mutedTextFor } from './index';

/**
 * TEMA LIGHT TRAVADO — páginas públicas e de autenticação (LandingHome, Login, Register).
 *
 * Essas telas são a vitrine do produto: devem ser sempre claras, brilhantes e
 * corporativas, ignorando o toggle dark/light do painel, o `data-theme="dark"` no
 * <html> e a preferência de sistema do usuário. O bloqueio é feito em DUAS camadas
 * (as duas são necessárias, ver ForcedLightScope.tsx):
 *   1. MUI  → este tema, injetado por um <ThemeProvider> local que substitui o global.
 *   2. CSS  → a classe `.forced-light-theme` (index.css) redeclara as variáveis da
 *             paleta no subtree, vencendo o `:root[data-theme="dark"]` por herança.
 *
 * Os valores são IDÊNTICOS aos do modo light global (`brandPalette.light`) — por isso
 * são importados de lá, e não recopiados: a vitrine é o tema light, apenas congelado.
 *
 * ⚠️ Usa `L.surface` (branco puro), NÃO `L.background` — desde a identidade minimalista
 * (ver theme/index.ts), `background` virou o canvas fosco (#f5f5f5), reservado ao painel
 * administrativo. A vitrine pública é intencionalmente mais brilhante/corporativa que o
 * canvas do painel, então mantém o branco puro que sempre teve.
 */
const L = brandPalette.light;

export const forcedLightTheme: Theme = createTheme({
  palette: {
    mode: 'light',
    background: {
      default: L.surface,  // #ffffff
      paper: L.primary,    // #6682f5 — Surface 1 (ver nota "cards de autenticação")
    },
    primary: { main: L.accent, contrastText: '#ffffff' },  // #0600c2 Accent/Brand
    secondary: { main: L.secondary },                      // #b2c1fa Surface 2
    text: {
      primary: L.text,                 // #000000
      // ⚠️ DESVIO DELIBERADO do spec, que pedia `secondary: '#6682f5'`.
      // #6682f5 é EXATAMENTE a cor do `paper` acima → contraste 1.00 (invisível):
      // os labels e helper texts de todo TextField sumiriam dentro dos cards.
      // #05134d (mutedTextFor('light'), já usado no tema global) é o único tom que
      // passa AA nas DUAS surfaces: 17.4:1 no branco, 5.04:1 no #6682f5.
      secondary: mutedTextFor('light'),
    },
    divider: L.secondary,
    // Status permanece semântico e imune à paleta (perigo/aviso/sucesso).
    error: { main: statusColors.danger },
    warning: { main: statusColors.warning },
    success: { main: statusColors.success },
  },
  shape: { borderRadius: 8 },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: 'none' },
      },
    },
  },
});

/**
 * NOTA — "cards de autenticação" (por que Login/Register NÃO usam `paper` = #6682f5).
 *
 * Surface 1 funciona como fundo de card na landing, onde todo o texto é preto (6.08:1).
 * Já Login e Register são FORMULÁRIOS com validação inline em vermelho, e vermelho sobre
 * cobalto é impossível de ler: #f43f5e sobre #6682f5 dá 1.06:1 e nenhum tom da família
 * rose chega a AA 4.5 antes de virar quase preto (#450a0a = 4.67:1) — aí já não lê mais
 * como "erro". Os erros de campo do Register ficariam invisíveis.
 *
 * Por isso os dois cards de auth sobrescrevem o fundo para branco (`background.default`)
 * e recebem borda `divider`; a marca entra pelo tint radial de Surface 1 na página.
 * O valor `paper: #6682f5` do tema segue valendo para os cards da landing e para
 * qualquer surface interna do MUI.
 */
export const AUTH_CARD_BG = brandPalette.light.surface;
