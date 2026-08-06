import { describe, expect, it } from 'vitest';
import { clampPercent, formatDate, formatInteger, formatPercent } from './dashboard.formatters';

describe('dashboard.formatters', () => {
  it('nunca deixa NaN ou Infinity chegarem aos indicadores', () => {
    expect(formatPercent(Number.NaN)).toBe('0,0%');
    expect(formatPercent(Number.POSITIVE_INFINITY)).toBe('0,0%');
    expect(formatInteger(Number.NaN)).toBe('0');
  });

  it('limita barras de progresso ao intervalo aceito pelo MUI', () => {
    expect(clampPercent(-10)).toBe(0);
    expect(clampPercent(45.5)).toBe(45.5);
    expect(clampPercent(120)).toBe(100);
  });

  it('formata datas válidas e protege contra datas inválidas', () => {
    expect(formatDate('valor-inválido')).toBe('—');
    expect(formatDate('2026-08-05T12:00:00Z')).toMatch(/05.*ago.*2026/i);
  });
});
