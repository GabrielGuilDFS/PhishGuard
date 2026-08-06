import { describe, expect, it } from 'vitest';
import { buildDashboardQuery, parseDownloadFilename } from './dashboard.export';

describe('dashboard.export', () => {
  it('mantém os mesmos filtros do overview na exportação', () => {
    const params = buildDashboardQuery('90d', '  Segurança da Informação  ');

    expect(params.get('period')).toBe('90d');
    expect(params.get('department')).toBe('Segurança da Informação');
  });

  it('omite o departamento quando o filtro representa Todos', () => {
    const params = buildDashboardQuery('30d', '');

    expect(params.toString()).toBe('period=30d');
  });

  it('prioriza filename* UTF-8 e preserva acentos', () => {
    const filename = parseDownloadFilename(
      'attachment; filename="fallback.pdf"; filename*=UTF-8\'\'phishguard-dashboard-Seguran%C3%A7a.pdf',
      'fallback-local.pdf',
    );

    expect(filename).toBe('phishguard-dashboard-Segurança.pdf');
  });

  it('usa o filename ASCII e neutraliza separadores de caminho', () => {
    const filename = parseDownloadFilename(
      'attachment; filename="../relatorio.csv"',
      'fallback-local.csv',
    );

    expect(filename).toBe('..-relatorio.csv');
  });
});
