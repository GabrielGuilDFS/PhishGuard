/**
 * Helper de teste — NÃO é um teste (vive em src/test, fora da coleta do Vitest e do
 * tsc -b, como o setup.ts).
 *
 * Monta um JWT de teste (header.payload.sig) com payload base64url e assinatura
 * fake: a barreira do frontend (validateSession) só lê claims; validar assinatura
 * é papel do backend.
 */
export function jwtDeTeste(payload: Record<string, unknown>): string {
  const b64url = (obj: Record<string, unknown>) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  return `${b64url({ alg: 'HS512', typ: 'JWT' })}.${b64url({ sid: 'sessao-de-teste', ...payload })}.assinatura-fake`;
}
