import { describe, expect, it } from 'vitest';
import { paginaEducativaPrecisaReconciliar, resolverPaginaEducativaDoCenario } from './feedbackTrainings';
import { simulationScenarios } from './predefinedTemplates';

describe('resolverPaginaEducativaDoCenario', () => {
  it('mantém o rótulo e o marcador canônicos do treinamento Mercado Liv', () => {
    const cenario = simulationScenarios.find(item => item.id === 'cenario-mercadoliv');

    expect(cenario).toBeDefined();
    const pagina = resolverPaginaEducativaDoCenario(cenario!);

    expect(pagina.nome).toBe('Treinamento Interativo — Mercado Liv');
    expect(pagina.html).toContain('data-feedback-training="mercadoliv"');
  });

  it('reconcilia apenas a mesma página quando o nome persistido está desatualizado', () => {
    const cenario = simulationScenarios.find(item => item.id === 'cenario-mercadoliv')!;
    const descritor = resolverPaginaEducativaDoCenario(cenario);

    expect(paginaEducativaPrecisaReconciliar(
      { nome: 'Cenário descontinuado', html: descritor.html },
      descritor,
    )).toBe(true);
    expect(paginaEducativaPrecisaReconciliar(
      { nome: descritor.nome, html: descritor.html },
      descritor,
    )).toBe(false);
    expect(paginaEducativaPrecisaReconciliar(
      { nome: 'Cenário descontinuado', html: '<div data-feedback-training="outro">' },
      descritor,
    )).toBe(false);
  });
});
