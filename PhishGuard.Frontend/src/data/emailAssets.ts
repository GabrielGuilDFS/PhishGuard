/** Resolve anexos CID do envio para assets locais nos previews do navegador. */
export function prepararEmailParaPreview(html: string): string {
  return html.replaceAll('cid:logo-mercadoliv', '/scenarios/mercado-livre-logo.png');
}
