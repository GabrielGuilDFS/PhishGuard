/**
 * ┌──────────────────────────────────────────────────────────────────────────────┐
 * │  DICIONÁRIO DE CORES DO PHISHGUARD — referência de leitura rápida            │
 * └──────────────────────────────────────────────────────────────────────────────┘
 *
 * Mapa "cor → como invocar" para auditorias de código, onboarding e interações com IA.
 * Este arquivo é DOCUMENTAÇÃO ATIVA: as constantes abaixo importam de `./index`, então
 * se um hex mudar lá, o valor aqui muda junto (nada é copiado à mão). Nenhum componente
 * precisa importar daqui — no dia a dia consuma o TEMA (`sx={{ color: 'text.primary' }}`),
 * não os hex. Use este módulo para CONSULTAR o contrato, e importe dele só quando a lib
 * não lê o tema do MUI (o caso do Recharts, em AdminDashboard.tsx).
 *
 * ── ONDE CADA COISA VIVE ──────────────────────────────────────────────────────
 *   src/theme/index.ts   → `brandPalette` (fonte de verdade) + `createAppTheme` (MUI)
 *   src/index.css        → as MESMAS cores como variáveis CSS + tokens do Tailwind v4
 *   ThemeModeContext.tsx → alterna <html data-theme> e dispara o fade de 1s
 *
 * ── AS DUAS FAMÍLIAS (nunca misture) ──────────────────────────────────────────
 *   1. MARCA (azul, muda com o tema)  → identidade. Fundo, texto, ações, gráficos.
 *   2. STATUS (fixo nos dois modos)   → sinal. Perigo, aviso, sucesso. NUNCA azul:
 *      um botão de excluir azul ou um erro azul quebram a leitura universal.
 */

import { brandPalette, statusColors, mutedTextFor, brandActionFor } from './index';
import type { AppThemeMode } from './index';

/**
 * PALETA DE MARCA — 5 cores por modo, alternadas por `<html data-theme>`.
 *
 * | Papel        | Light     | Dark      | MUI (sx / palette)   | Tailwind        | CSS var        |
 * |--------------|-----------|-----------|----------------------|-----------------|----------------|
 * | Texto        | #000000   | #ffffff   | `text.primary`       | `text-text`     | `var(--text)`  |
 * | Fundo/Paper  | #ffffff   | #000000   | `background.paper`   | `bg-background` | `var(--background)` |
 * | Surface 1    | #6682f5   | #0a2799   | (ênfase — ver nota)  | `bg-primary`    | `var(--primary)`    |
 * | Surface 2    | #b2c1fa   | #05134d   | `divider`            | `border-secondary` | `var(--secondary)` |
 * | Accent/Brand | #0600c2   | #443dff   | `primary.main`       | `text-accent`   | `var(--accent)`     |
 *
 * ⚠️ Note a "torção" proposital: o Accent da paleta é o `primary` do MUI (é ele que
 * pinta os botões de ação), enquanto o `primary` da paleta é uma SURFACE. Ao ler
 * `sx={{ bgcolor: 'primary.main' }}` no código, isso é o ACCENT (#0600c2), não #6682f5.
 */
export const THEME_COLORS = {
  brand: {
    /** Texto principal. MUI: `text.primary` · TW: `text-text` · CSS: `var(--text)` */
    text: { light: brandPalette.light.text, dark: brandPalette.dark.text },
    /** Fundo da página E das surfaces. MUI: `background.default` / `background.paper` */
    background: { light: brandPalette.light.background, dark: brandPalette.dark.background },
    /** Surface 1 — ênfase (ver SURFACE_1_USAGE). MUI: sem token; use este hex/`var(--primary)` */
    primary: { light: brandPalette.light.primary, dark: brandPalette.dark.primary },
    /** Surface 2 — bordas/divisores/cabeçalho de tabela. MUI: `divider` */
    secondary: { light: brandPalette.light.secondary, dark: brandPalette.dark.secondary },
    /** Accent — ações, ícones ativos, séries de gráfico. MUI: `primary.main` */
    accent: { light: brandPalette.light.accent, dark: brandPalette.dark.accent },
  },

  /**
   * DERIVADOS — não são hex novos: são cores da própria paleta reaproveitadas do modo
   * OPOSTO, porque a cor do próprio modo reprovaria no contraste AA como TEXTO.
   * Regra: o light toma os tons fechados do dark; o dark toma os tons abertos do light.
   */
  derived: {
    /** Texto de apoio. MUI: `text.secondary`. light #05134d / dark #b2c1fa */
    mutedText: { light: mutedTextFor('light'), dark: mutedTextFor('dark') },
    /** Ação de marca (botão secundário). MUI: `secondary.main`. light #0a2799 / dark #6682f5 */
    brandAction: { light: brandActionFor('light'), dark: brandActionFor('dark') },
  },

  /**
   * STATUS — IGUAIS nos dois modos, IMUNES ao tema. Sempre pelo `severity`/`color` do
   * MUI (`<Alert severity="error">`, `<Button color="error">`), nunca pelo hex cru.
   */
  status: {
    /** Exclusão, perigo, alvo que CAIU no phishing. MUI: `error.main` · TW: `text-danger` */
    danger: statusColors.danger,
    /** Avisos, campanhas pendentes/em andamento. MUI: `warning.main` · TW: `text-warning` */
    warning: statusColors.warning,
    /** Confirmações, entregues, alvo que RESISTIU. MUI: `success.main` · TW: `text-success` */
    success: statusColors.success,
  },
} as const;

/**
 * RECEITUÁRIO — "qual classe/propriedade uso para...?"
 *
 * Fundo de tela .............. `sx={{ bgcolor: 'background.default' }}`  (ou deixe o
 *                              CssBaseline resolver: ele já pinta o body)
 * Fundo de card/painel ....... `<Paper>` / `<Card>` (herdam `background.paper`)
 * Texto principal ............ `sx={{ color: 'text.primary' }}` — ou nada: é o default
 * Texto de apoio ............. `sx={{ color: 'text.secondary' }}` / `color="textSecondary"`
 * Borda de card .............. `sx={{ border: 1, borderColor: 'divider' }}`
 * Borda de tabela ............ automático (MuiTableCell no createAppTheme)
 * Cabeçalho de tabela ........ automático (`<TableHead>` = Surface 2 + texto do modo)
 * Botão de ação principal .... `<Button variant="contained">` (= Accent)
 * Botão secundário ........... `<Button color="secondary">` (= brandAction)
 * Ícone ativo/selecionado .... `sx={{ color: 'primary.main' }}` (= Accent)
 * Fill translúcido do accent . `alpha(theme.palette.primary.main, 0.15)` — nunca hex+"26"
 * Excluir / risco alto ....... `color="error"` / `<Alert severity="error">`
 * Aviso / pendente ........... `color="warning"` / `<Alert severity="warning">`
 * Sucesso / entregue ......... `color="success"` / `<Alert severity="success">`
 * Gradiente de destaque ...... `sx={{ background: 'var(--linearPrimaryAccent)' }}`
 *                              + `color: '#ffffff'` (ver GRADIENTS abaixo)
 */

/**
 * SURFACE 1 (`primary`) — por que NÃO é o fundo de todo Paper.
 *
 * A paleta traz só dois neutros (background/text). Se todo Paper fosse `primary`
 * (#6682f5 no light), o texto de apoio cairia para ~2:1 e a ação secundária para ~3:1
 * — abaixo do mínimo AA de 4.5:1. Com as surfaces neutras, texto preto/branco puro
 * rende 21:1 em qualquer tela. Então `primary` é a surface de ÊNFASE, usada onde o
 * contraste é controlado caso a caso, e o card é delimitado por BORDA (Surface 2).
 *
 * Onde Surface 1 é usada hoje: tooltip dos gráficos (AdminDashboard).
 * Para usar em algo novo: fixe `color` junto (não confie no `text.primary` herdado) e
 * confira o contraste — no light o par é #000 sobre #6682f5 (5.6:1).
 */
export const SURFACE_1_USAGE = 'ênfase pontual (tooltip de gráfico); NÃO é background.paper' as const;

/**
 * GRADIENTES — declarados em src/index.css, invocados por `var()` no `sx`.
 * São azul-escuro nos DOIS modos (não alternam com data-theme), então o conteúdo por
 * cima vai sempre de branco fixo — herdar `text.primary` deixaria preto sobre azul
 * escuro no modo light.
 *
 *   <Box sx={{ background: 'var(--linearPrimaryAccent)', color: '#ffffff' }}>
 *
 * Disponíveis: --linearPrimarySecondary · --linearPrimaryAccent · --linearSecondaryAccent
 *              --radialPrimarySecondary · --radialPrimaryAccent · --radialSecondaryAccent
 * Em uso: cabeçalho da Navbar (AdminLayout) e card de destaque da aba Aparência (Settings).
 */
export const GRADIENTS = {
  linearPrimarySecondary: 'var(--linearPrimarySecondary)',
  linearPrimaryAccent: 'var(--linearPrimaryAccent)',
  linearSecondaryAccent: 'var(--linearSecondaryAccent)',
  radialPrimarySecondary: 'var(--radialPrimarySecondary)',
  radialPrimaryAccent: 'var(--radialPrimaryAccent)',
  radialSecondaryAccent: 'var(--radialSecondaryAccent)',
  /** Conteúdo sobre qualquer gradiente: branco fixo nos dois modos. */
  onGradientText: '#ffffff',
} as const;

/**
 * TRANSIÇÃO DE TEMA — 1.0s, só no instante da troca.
 * O ThemeModeContext põe `.theme-transition` no <html>, espera 1s e remove. Fora dessa
 * janela valem as transições curtas do MUI (~150-250ms), então hover/foco não ficam
 * "arrastados". Não existe transição permanente de 1s em lugar nenhum — se for mexer
 * na duração, mude os DOIS lados: `TRANSITION_MS` (ThemeModeContext) e o CSS.
 * `prefers-reduced-motion: reduce` cancela o fade.
 */
export const THEME_TRANSITION = {
  durationMs: 1000,
  easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
  /** Classe temporária no <html> durante a troca. */
  className: 'theme-transition',
  /** Só propriedades de cor animam (nunca layout). */
  properties: ['background-color', 'border-color', 'color', 'fill', 'stroke', 'box-shadow'],
} as const;

/**
 * VITRINE PÚBLICA TRAVADA NO LIGHT — LandingHome (`/`), Login e Register.
 *
 * Essas três rotas são envolvidas pelo `<ForcedLightScope>` em App.tsx e NÃO respondem
 * ao toggle de tema: sempre claras. Se criar outra página pública, envolva a rota da
 * mesma forma (o CheckoutPage ainda está fora — ver OUT_OF_SCOPE/débitos).
 *
 * Duas camadas, porque cobrem caminhos diferentes e nenhuma basta sozinha:
 *   MUI → `forcedLightTheme` (src/theme/forcedLight.ts) via ThemeProvider local.
 *   CSS → classe `.forced-light-theme` (index.css) redeclara as vars no subtree.
 *
 * Diferenças em relação ao tema light global (as duas são deliberadas e testadas em
 * ForcedLightScope.test.tsx):
 *   1. `background.paper` = #6682f5 (Surface 1) em vez de #ffffff — os cards da landing
 *      são cobalto. Só texto quase-preto passa AA sobre eles (accent como TEXTO = 3.40:1),
 *      por isso ali o accent aparece em ícone/botão sólido, nunca em corpo de texto.
 *   2. Login/Register sobrescrevem o card para BRANCO: são formulários com erro inline
 *      vermelho, e vermelho sobre cobalto é ilegível (#f43f5e = 1.06:1; nenhum tom rose
 *      chega a AA antes de virar quase preto). Ver a nota em forcedLight.ts.
 */
export const FORCED_LIGHT_ROUTES = ['/', '/login', '/register'] as const;

/**
 * FORA DO ESCOPO desta paleta — não aplique azul aqui:
 * e-mails simulados (Resources/OfficialBaits), landing pages falsas (src/data/
 * landingTemplates.ts) e telas educacionais. Esses conteúdos imitam marcas reais e
 * dependem de fidelidade visual à marca imitada; usam Tailwind com cores próprias.
 */
export const OUT_OF_SCOPE = ['iscas de e-mail', 'landing pages falsas', 'telas educacionais'] as const;

/** Reexport de conveniência — evita importar de dois módulos numa mesma tela. */
export type { AppThemeMode };
