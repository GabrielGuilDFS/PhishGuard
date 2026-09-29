import { describe, expect, it } from 'vitest';
import { feedbackTrainings, paginaEducativaPrecisaReconciliar, resolverPaginaEducativaDoCenario } from './feedbackTrainings';
import { simulationScenarios } from './predefinedTemplates';

describe('resolverPaginaEducativaDoCenario', () => {
  it('resolve a imagem CID do e-mail no treinamento do navegador', () => {
    const email = feedbackTrainings.mercadoliv.mockups.find(mockup => mockup.id === 'email')!;
    expect(email.html).toContain('/scenarios/mercado-livre-logo.png');
    expect(email.html).not.toContain('cid:');
  });
  it('mantém o rótulo e o marcador canônicos do treinamento Mercado Liv', () => {
    const cenario = simulationScenarios.find(item => item.id === 'cenario-mercado-liv');

    expect(cenario).toBeDefined();
    const pagina = resolverPaginaEducativaDoCenario(cenario!);

    expect(pagina.nome).toBe('Treinamento Interativo — Mercado Livre');
    expect(pagina.html).toContain('data-feedback-training="mercadoliv"');
  });

  it('reconcilia apenas a mesma página quando o nome persistido está desatualizado', () => {
    const cenario = simulationScenarios.find(item => item.id === 'cenario-mercado-liv')!;
    const descritor = resolverPaginaEducativaDoCenario(cenario);

    expect(paginaEducativaPrecisaReconciliar(
      { nome: 'Treinamento Interativo — Mercado Liv', html: descritor.html.replace('Abrir treinamento</a>', 'Abrir treinamento (Mercado Liv)</a>') },
      descritor,
    )).toBe(true);
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
