// Expiração dinâmica do link de redefinição de senha (isca "bho MAX").
//
// IMPORTANTE — o e-mail NÃO é um componente React: é um template HTML com
// placeholders resolvidos em dois pontos, sempre com a MESMA formatação:
//   1. DISPARO real (autoritativo): server-side no CampaignDispatchService, que
//      substitui {{DATA_EXPIRACAO}} pela data/hora de ENVIO + validade.
//   2. PREVIEW / mockup educacional: aqui no front, para exibir uma data real em
//      vez do placeholder cru (data/hora de RENDERIZAÇÃO + validade).
//
// Mantido puro/typed e sem dependências de i18n: o mês usa abreviação inglesa de
// 3 letras (ex.: "Jul") para casar com o estilo original do template.

const MESES_ABREV = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
] as const;

/** Horas de validade padrão do link a partir do momento base (envio/renderização). */
export const HORAS_VALIDADE_LINK = 2;

/**
 * Formata o horário de expiração no padrão do template bho MAX:
 * `MMM DD, YYYY às HH:MM AM/PM` (ex.: `Jul 22, 2026 às 04:30 PM`).
 *
 * @param base           Momento de referência (default: agora).
 * @param horasValidade  Janela de validade em horas (default: {@link HORAS_VALIDADE_LINK}).
 */
export function formatarExpiracaoLink(
  base: Date = new Date(),
  horasValidade: number = HORAS_VALIDADE_LINK,
): string {
  const d = new Date(base.getTime() + horasValidade * 60 * 60 * 1000);

  const mes = MESES_ABREV[d.getMonth()];
  const dia = String(d.getDate()).padStart(2, '0');
  const ano = d.getFullYear();

  const h24 = d.getHours();
  const periodo = h24 < 12 ? 'AM' : 'PM';
  const h12 = String(((h24 + 11) % 12) + 1).padStart(2, '0'); // 0→12, 13→01...
  const min = String(d.getMinutes()).padStart(2, '0');

  return `${mes} ${dia}, ${ano} às ${h12}:${min} ${periodo}`;
}

/**
 * Formata a data/hora de um "acesso detectado" no padrão do template Microsft 365:
 * `DD/MM/YYYY às HH:MM (BRT)` (ex.: `22/07/2026 às 14:32 (BRT)`).
 *
 * Ao contrário da expiração (futuro, +2h), o acesso reflete o MOMENTO do
 * evento — por isso sem offset (base = agora, o instante do disparo/renderização).
 */
export function formatarDataAcessoBRT(base: Date = new Date()): string {
  const dia = String(base.getDate()).padStart(2, '0');
  const mes = String(base.getMonth() + 1).padStart(2, '0');
  const ano = base.getFullYear();
  const h = String(base.getHours()).padStart(2, '0');
  const min = String(base.getMinutes()).padStart(2, '0');
  return `${dia}/${mes}/${ano} às ${h}:${min} (BRT)`;
}

/**
 * Resolve os placeholders de data dinâmica (`{{DATA_EXPIRACAO}}`, `{{DATA_ACESSO}}`)
 * no HTML de uma isca — usado para previews/mockups no front, espelhando o que o
 * backend faz no disparo. Placeholders ausentes são ignorados.
 */
export function resolverDatasDinamicasNoHtml(html: string, base: Date = new Date()): string {
  return html
    .replaceAll('{{DATA_EXPIRACAO}}', formatarExpiracaoLink(base))
    .replaceAll('{{DATA_ACESSO}}', formatarDataAcessoBRT(base));
}
