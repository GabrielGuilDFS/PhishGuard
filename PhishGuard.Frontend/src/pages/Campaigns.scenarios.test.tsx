import { afterEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import Campaigns from './Campaigns';
import { NotificationProvider } from '../context/NotificationContext';
import { clearSession, setToken } from '../auth/session';
import { jwtDeTeste } from '../test/jwt';
import { simulationScenarios } from '../data/predefinedTemplates';
import { resolverPaginaEducativaDoCenario } from '../data/feedbackTrainings';

afterEach(() => {
  clearSession();
  vi.unstubAllGlobals();
});

describe('Campaigns — compatibilidade dos cenários restaurados', () => {
  it.each([
    ['mercado-liv-novo-acesso', 'mercado-liv-login', 'cenario-mercado-liv', 'Mercado Liv'],
    ['mercadoliv-novo-acesso', 'mercadoliv-login', 'cenario-mercado-liv', 'Mercado Liv'],
    ['microcorp-expiracao-senha', 'microcorp-login', 'cenario-microcorp', 'Microsft 365'],
  ])('edita campanha com %s sem duplicar seus recursos', async (emailId, landingId, scenarioId, oldBrand) => {
    const user = userEvent.setup();
    setToken(jwtDeTeste({
      tenant_id: '11111111-1111-1111-1111-111111111111',
      exp: Math.floor(Date.now() / 1000) + 3600,
    }));
    const cenario = simulationScenarios.find(item => item.id === scenarioId)!;
    const educativa = resolverPaginaEducativaDoCenario(cenario);
    const campaign = {
      id: 'campaign-1', nomeCampanha: 'Campanha existente', status: 'Rascunho',
      dataInicio: '2030-09-30T12:00:00Z', dataFim: null,
      emailTemplateId: 'email-row', landingPageId: 'landing-row', educationalPageId: 'edu-row',
      targetIds: ['target-row'], templateNome: 'Mercado Liv', landingPageNome: 'Mercado Liv',
      educationalPageNome: educativa.nome,
    };
    const mutations: { path: string; method: string; body: unknown }[] = [];
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const path = new URL(String(input), 'http://localhost').pathname;
      if (init?.method && init.method !== 'GET') {
        mutations.push({ path, method: init.method, body: JSON.parse(String(init.body)) });
        return new Response('{}', { status: 200 });
      }
      const responses: Record<string, unknown> = {
        '/api/Campaigns': [campaign],
        '/api/Campaigns/campaign-1': campaign,
        '/api/Targets': [{ id: 'target-row', nome: 'Alvo fictício', email: 'alvo@example.test', departamento: 'TI' }],
        '/api/Templates': [{ id: 'email-row', nome: oldBrand, corpoHtml: emailId, assunto: 'Assunto preservado', remetenteNome: oldBrand, remetenteEmail: 'training@example.test' }],
        '/api/PhishingPages': [{ id: 'landing-row', nome: oldBrand, conteudoHtml: landingId }],
        '/api/EducationalPages': [{ id: 'edu-row', nome: `Treinamento Interativo — ${oldBrand}`, conteudoHtml: educativa.html.replace('Abrir treinamento</a>', `Abrir treinamento (${oldBrand})</a>`) }],
        '/api/email-delivery/status': { configurado: true, transporteDisponivel: true },
      };
      if (!(path in responses)) throw new Error(`Requisição inesperada: ${path}`);
      return new Response(JSON.stringify(responses[path]), { status: 200 });
    });
    vi.stubGlobal('fetch', fetchMock);

    render(
      <MemoryRouter initialEntries={['/admin/campaigns?editar=campaign-1']}>
        <NotificationProvider><Campaigns /></NotificationProvider>
      </MemoryRouter>,
    );
    await screen.findByRole('dialog', { name: 'Editar Campanha' });
    expect(screen.getByRole('combobox', { name: /Cenário de Simulação/i })).toHaveValue(cenario.nome);
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(mutations).toHaveLength(4));
    expect(mutations[0]).toMatchObject({
      path: '/api/Templates/email-row', method: 'PUT',
      body: { id: 'email-row', assunto: 'Assunto preservado', remetenteEmail: 'training@example.test', corpoHtml: emailId },
    });
    expect(mutations.every(item => item.method === 'PUT')).toBe(true);
    expect(mutations[2]).toMatchObject({
      path: '/api/EducationalPages/edu-row', method: 'PUT',
      body: { nome: educativa.nome, conteudoHtml: educativa.html },
    });
    expect(mutations[3]).toMatchObject({
      path: '/api/Campaigns/campaign-1', method: 'PUT',
      body: { emailTemplateId: 'email-row', landingPageId: 'landing-row', educationalPageId: 'edu-row' },
    });
  });
});
