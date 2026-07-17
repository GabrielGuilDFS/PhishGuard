import { createTheme, alpha, type Theme } from '@mui/material/styles';

/**
 * Identidade visual OFICIAL do PhishGuard — paleta azul (Realtime Colors), aplicada a
 * TODO o painel administrativo. Fábrica única `createAppTheme(mode)` para os dois modos;
 * o modo padrão de primeira interação é `light` (ver ThemeModeContext).
 *
 * Estas 6 chaves são a FONTE DE VERDADE e espelham, valor a valor, as variáveis CSS
 * `--text/--background/--surface/--primary/--secondary/--accent` de src/index.css. Ao
 * mudar um hex aqui, mude no index.css também (o Tailwind v4 lê as variáveis; o MUI lê
 * este arquivo).
 *
 * Papéis das cores (mapa completo → MUI/Tailwind em src/theme/themeHelper.ts):
 * - text       → text.primary
 * - background → background.default (fundo de PÁGINA/canvas, atrás de tudo)
 * - surface    → background.paper (fundo de Card/Paper/AppBar/Drawer — ver nota abaixo)
 * - primary    → "Surface 1": surface de ÊNFASE (bolha de ícone, item selecionado,
 *                tooltip de gráfico, gradientes). Ver nota "Surface 1" abaixo.
 * - secondary  → "Surface 2": divisores, bordas e cabeçalho de tabela
 * - accent     → palette.primary (botões de ação, ícones ativos, séries de gráfico)
 *
 * NÃO usar estes tokens em e-mails simulados, páginas falsas ou telas educacionais:
 * esses conteúdos têm identidade própria (fidelidade à marca imitada).
 *
 * ⚠️ NOTA "background × surface" (identidade minimalista de alto contraste): `background`
 * é um neutro FOSCO (off-white no light, grafite no dark) — reduz o brilho/fadiga do
 * branco/preto absoluto no canvas da página. `surface` é o neutro mais afastado do
 * canvas (branco puro no light, #121212 no dark), reservado para Card/Paper/AppBar/
 * Drawer: sem essa distância os elementos "somem" contra o fundo. Cards continuam
 * delimitados por borda `secondary` (ver SOFT_BORDER_ALPHA) — a distância de tom já dá
 * profundidade, a borda dá o contorno definitivo.
 *
 * NOTA "Surface 1" (decisão de contraste): `primary` NÃO é o fundo de Paper/Card — é
 * ênfase pontual. Pintar todo Paper de `primary` (#6682f5 no light) derrubaria o texto
 * de apoio para ~2:1 e a ação secundária para ~3:1, abaixo do mínimo AA de 4.5:1.
 */
export const brandPalette = {
  light: {
    /** Texto principal — preto puro. */
    text: '#000000',
    /** Fundo de página/canvas — off-white fosco (reduz o brilho do branco puro). */
    background: '#f5f5f5',
    /** Fundo de Card/Paper/AppBar/Drawer — branco puro (contraste contra o canvas). */
    surface: '#ffffff',
    /** Surface 1 — azul periwinkle de ênfase (ver nota acima). */
    primary: '#6682f5',
    /** Surface 2 — bordas, divisores e cabeçalho de tabela. */
    secondary: '#b2c1fa',
    /** Accent/Brand — azul ultramar profundo: ações, ícones ativos, gráficos. */
    accent: '#0600c2',
  },
  dark: {
    /** Texto principal — branco puro. */
    text: '#ffffff',
    /** Fundo de página/canvas — preto fosco/grafite (evita a fadiga do preto absoluto). */
    background: '#0a0a0a',
    /** Fundo de Card/Paper/AppBar/Drawer — leve elevação sobre o canvas (profundidade). */
    surface: '#121212',
    /** Surface 1 — azul escuro de ênfase (ver nota acima). */
    primary: '#0a2799',
    /** Surface 2 — bordas, divisores e cabeçalho de tabela. */
    secondary: '#05134d',
    /** Accent/Brand — azul elétrico neon: ações, ícones ativos, gráficos. */
    accent: '#443dff',
  },
} as const;

export type AppThemeMode = keyof typeof brandPalette;

/**
 * Opacidade padrão das bordas "estruturais" (Card, TableCell, Paper outlined, grid de
 * gráficos) — o que delimita um container contra um fundo da MESMA cor (`paper` ===
 * `background`, ver nota "Surface 1"), sem apelar para sombra. Mesmo a 100% de opacidade
 * essa borda nunca teve contraste de LUMINÂNCIA alto (secondary é deliberadamente próximo
 * do neutro do modo: ~1.76:1 no light, ~1.21:1 no dark — ambos abaixo do 3:1 de UI da
 * WCAG, ou seja, nunca foi "contraste" que a tornava vibrante). O que lê como vibrante é
 * a SATURAÇÃO — azul puro sobre um neutro absoluto (#fff/#000). Diluir com alpha() ataca
 * exatamente isso: mistura a cor para perto do fundo sem apagar o traço.
 * NÃO usar este valor para separadores funcionais dentro de uma lista/menu (Divider) —
 * esses usam um alpha mais baixo (~0.16, ver AdminLayout.tsx), pois já estão cercados de
 * conteúdo e não precisam competir visualmente por atenção como o contorno de um card.
 */
export const SOFT_BORDER_ALPHA = 0.35;

/**
 * Cores semânticas de status — papéis EXCLUSIVOS, IGUAIS nos dois modos e IMUNES à
 * paleta azul. Elementos funcionais que exigem atenção imediata não são azuis: o azul
 * é identidade, estes são sinal. Consumidos via palette.error/warning/success do MUI.
 * - danger  (rose-500):    exclusão, perigo, alvo que caiu no phishing (clique/credenciais).
 * - warning (amber-500):   avisos, mensagens, campanhas pendentes/em andamento.
 * - success (emerald-500): confirmações, envios entregues, alvos que resistiram.
 */
export const statusColors = {
  danger: '#f43f5e',
  warning: '#f59e0b',
  success: '#10b981',
} as const;

/**
 * Texto de apoio (text.secondary) — DERIVADO: é o `secondary` do modo OPOSTO.
 * Regra: o modo claro toma os tons fechados do dark; o dark toma os tons abertos do
 * light. Assim o texto de apoio fica legível tanto sobre `background` quanto sobre a
 * surface de ênfase `primary` — light #05134d: 17:1 no branco / 4.5:1 no periwinkle;
 * dark #b2c1fa: 11:1 no preto / 6.9:1 no azul escuro. Zero hex fora da paleta.
 */
export function mutedTextFor(mode: AppThemeMode): string {
  return mode === 'light' ? brandPalette.dark.secondary : brandPalette.light.secondary;
}

/**
 * Ação de marca (palette.secondary) — DERIVADO pela mesma regra: é o `primary` do modo
 * OPOSTO. `primary` do próprio modo reprovaria como TEXTO sobre o fundo do modo
 * (#0a2799 no preto = 2.7:1); invertido, passa AA — light #0a2799: 12:1 no branco;
 * dark #6682f5: 5.6:1 no preto.
 */
export function brandActionFor(mode: AppThemeMode): string {
  return mode === 'light' ? brandPalette.dark.primary : brandPalette.light.primary;
}

/**
 * Canais "R, G, B" de um hex — para compor fills translúcidos (`rgba(${ch}, 0.1)`) sem
 * repetir o valor da cor por extenso, o que sairia de sincronia com a paleta.
 */
export function rgbChannelsOf(hex: string): string {
  const h = hex.replace('#', '');
  const n = parseInt(h, 16);
  return `${(n >> 16) & 255}, ${(n >> 8) & 255}, ${n & 255}`;
}

export function createAppTheme(mode: AppThemeMode = 'light'): Theme {
  const c = brandPalette[mode];
  return createTheme({
    palette: {
      mode,
      primary: { main: c.accent, contrastText: '#ffffff' },
      secondary: { main: brandActionFor(mode), contrastText: '#ffffff' },
      // Canvas da página × surface de Card/Paper (ver nota "background × surface").
      background: { default: c.background, paper: c.surface },
      text: { primary: c.text, secondary: mutedTextFor(mode) },
      divider: c.secondary,
      error: { main: statusColors.danger },
      warning: { main: statusColors.warning },
      success: { main: statusColors.success },
    },
    shape: { borderRadius: 8 },
    typography: {
      fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    },
    components: {
      // No modo dark o MUI aplica um overlay gradiente por elevação sobre o Paper
      // (backgroundImage), que descaracteriza a cor exata das surfaces — desligado.
      MuiPaper: {
        styleOverrides: {
          root: { backgroundImage: 'none' },
          // Paper `variant="outlined"` (previewers, cards de molde) usa por padrão
          // `theme.palette.divider` a 100% — mesma borda vibrante do Card. Diluída
          // aqui para o painel inteiro herdar a suavização sem precisar de sx local
          // em cada tela que usa a variante.
          outlined: { borderColor: alpha(c.secondary, SOFT_BORDER_ALPHA) },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            borderRadius: 16,
            // Com paper == background, a BORDA é o que delimita o card (não o fundo) —
            // por isso ela não vira `border: 'none'` (ver SOFT_BORDER_ALPHA acima):
            // sem sombra e sem diferença de fundo, um card sem borda nenhuma some.
            border: `1px solid ${alpha(c.secondary, SOFT_BORDER_ALPHA)}`,
          },
        },
      },
      MuiTableCell: {
        styleOverrides: {
          root: { borderBottom: `1px solid ${alpha(c.secondary, SOFT_BORDER_ALPHA)}` },
          // Cabeçalho de tabela = Surface 2 SÓLIDA (preenchimento, não borda — fora do
          // escopo da suavização). O texto acompanha o `text` do modo (não branco
          // fixo): no light, branco sobre #b2c1fa daria 1.6:1 — ilegível.
          head: {
            backgroundColor: c.secondary,
            color: c.text,
            fontWeight: 600,
          },
        },
      },
    },
  });
}

/** Instância light — tema padrão do painel (primeira interação). */
export const lightTheme = createAppTheme('light');

/** Instância dark — alternável pelo toggle em Configurações. */
export const darkTheme = createAppTheme('dark');
