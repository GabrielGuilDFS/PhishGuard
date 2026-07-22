import { describe, it, expect } from 'vitest';
import {
  formatarExpiracaoLink,
  formatarDataAcessoBRT,
  resolverDatasDinamicasNoHtml,
} from './emailExpiration';

// A base é interpretada em horário LOCAL (new Date(y,m,d,h,min)); o helper soma a
// validade e formata como "MMM DD, YYYY às HH:MM AM/PM".
describe('formatarExpiracaoLink', () => {
  it('soma 2h por padrão e formata no padrão do template', () => {
    // 22/07/2026 14:30 local + 2h = 16:30 → "Jul 22, 2026 às 04:30 PM"
    const base = new Date(2026, 6, 22, 14, 30, 0);
    expect(formatarExpiracaoLink(base)).toBe('Jul 22, 2026 às 04:30 PM');
  });

  it('respeita janela de validade customizada', () => {
    const base = new Date(2026, 0, 5, 9, 5, 0); // 05/01/2026 09:05
    expect(formatarExpiracaoLink(base, 1)).toBe('Jan 05, 2026 às 10:05 AM');
  });

  it('meia-noite vira 12 AM e meio-dia vira 12 PM (12h correto)', () => {
    // 22:30 + 2h = 00:30 do dia seguinte → 12:30 AM
    expect(formatarExpiracaoLink(new Date(2026, 6, 22, 22, 30))).toBe('Jul 23, 2026 às 12:30 AM');
    // 10:00 + 2h = 12:00 → 12:00 PM
    expect(formatarExpiracaoLink(new Date(2026, 6, 22, 10, 0))).toBe('Jul 22, 2026 às 12:00 PM');
  });

  it('resolverDatasDinamicasNoHtml substitui todos os placeholders de data', () => {
    const base = new Date(2026, 6, 22, 14, 30);
    const html = 'expira em {{DATA_EXPIRACAO}}; acesso em {{DATA_ACESSO}}';
    const out = resolverDatasDinamicasNoHtml(html, base);
    expect(out).not.toContain('{{DATA_EXPIRACAO}}');
    expect(out).not.toContain('{{DATA_ACESSO}}');
    expect(out).toContain('Jul 22, 2026 às 04:30 PM');
    expect(out).toContain('22/07/2026 às 14:30 (BRT)');
  });
});

// Padrão pt-BR do Mercado Liv: DD/MM/YYYY às HH:MM (BRT), SEM offset (o acesso é o
// próprio momento base).
describe('formatarDataAcessoBRT', () => {
  it('formata o momento do acesso no padrão brasileiro com fuso BRT', () => {
    expect(formatarDataAcessoBRT(new Date(2026, 6, 22, 14, 32))).toBe('22/07/2026 às 14:32 (BRT)');
  });

  it('zero-padroniza dia, mês, hora e minuto', () => {
    expect(formatarDataAcessoBRT(new Date(2026, 0, 5, 9, 7))).toBe('05/01/2026 às 09:07 (BRT)');
  });
});
