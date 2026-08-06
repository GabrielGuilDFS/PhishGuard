import { describe, it, expect } from 'vitest';
import {
  EDUCATIONAL_FEEDBACK_PATH,
  EDU_FEEDBACK_QUERY,
  TRACKING_ACTIONS,
  educationalFeedbackUrl,
  readEducationalFeedbackParams,
  trackingEndpoint,
} from './trackingContract';
import { landingTemplates } from '../data/landingTemplates';

// Contrato de rastreamento compartilhado (Passo 12). Estes testes são o lado do
// FRONT do mesmo contrato exercido no back (TrackingControllerTests +
// TrackingContractTests). Garantem que o formato de URL/params não diverge.

describe('trackingEndpoint', () => {
  it('monta /api/tracking/<ação>/:c/:t para cada ação', () => {
    expect(trackingEndpoint(TRACKING_ACTIONS.click, 'c1', 't1', 'signed')).toBe('/api/tracking/click/c1/t1?k=signed');
    expect(trackingEndpoint(TRACKING_ACTIONS.submit, 'c1', 't1', 'signed')).toBe('/api/tracking/submit/c1/t1?k=signed');
    expect(trackingEndpoint(TRACKING_ACTIONS.educationalView, 'c1', 't1', 'signed')).toBe('/api/tracking/educational-view/c1/t1?k=signed');
    expect(trackingEndpoint(TRACKING_ACTIONS.complete, 'c1', 't1', 'signed')).toBe('/api/tracking/complete/c1/t1?k=signed');
    expect(trackingEndpoint(TRACKING_ACTIONS.open, 'c1', 't1', 'signed')).toBe('/api/tracking/open/c1/t1?k=signed');
  });
});

describe('educationalFeedbackUrl', () => {
  it('usa os parâmetros canônicos c/t (o MESMO formato do backend, §1.3d)', () => {
    expect(educationalFeedbackUrl({ campaignId: 'camp-1', targetId: 'tgt-2' })).toBe(
      '/educational-feedback?c=camp-1&t=tgt-2',
    );
  });

  it('inclui template quando informado (versão completa do front)', () => {
    expect(educationalFeedbackUrl({ template: 'amzprime', campaignId: 'c1', targetId: 't1' })).toBe(
      '/educational-feedback?template=amzprime&c=c1&t=t1',
    );
  });

  it('omite campos ausentes e faz URL-encoding dos valores', () => {
    expect(educationalFeedbackUrl({})).toBe(EDUCATIONAL_FEEDBACK_PATH);
    expect(educationalFeedbackUrl({ template: 'a b' })).toBe('/educational-feedback?template=a+b');
  });

  it('faz round-trip com readEducationalFeedbackParams', () => {
    const url = educationalFeedbackUrl({ template: 'netsflix', campaignId: 'c9', targetId: 't9', trackingToken: 'signed' });
    const params = new URLSearchParams(url.split('?')[1]);
    expect(readEducationalFeedbackParams(params)).toEqual({
      template: 'netsflix',
      campaignId: 'c9',
      targetId: 't9',
      trackingToken: 'signed',
    });
  });
});

describe('readEducationalFeedbackParams — nomes canônicos', () => {
  it('lê c/t/template e IGNORA os nomes legados campaign/target (divergência §1.3d)', () => {
    const legado = new URLSearchParams('campaign=cLegado&target=tLegado');
    expect(readEducationalFeedbackParams(legado)).toEqual({
      template: null,
      campaignId: null,
      targetId: null,
      trackingToken: null,
    });
  });
});

// Conformidade: nenhuma landing pode voltar a divergir do contrato. Cada molde
// redireciona para a tela educacional; a URL DEVE usar os nomes canônicos e nunca
// o antigo `campaign=`/`target=`.
describe('landings conformam ao contrato de redirect', () => {
  const CAMPAIGN = EDU_FEEDBACK_QUERY.campaign; // 'c'
  const TARGET = EDU_FEEDBACK_QUERY.target; // 't'

  it.each(landingTemplates.map((t) => [t.id, t.html] as const))(
    'molde "%s" redireciona para /educational-feedback com params canônicos',
    (_id, html) => {
      // Todo molde leva o alvo à tela educacional após a submissão.
      expect(html).toContain(EDUCATIONAL_FEEDBACK_PATH);

      // Cada URL educacional carrega o `template=` (qual treinamento). O restante da
      // query pode ser estático (`&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}`) OU concatenado
      // em runtime (`&c='+encodeURIComponent(c)+'&t='+...`) — por isso os params c/t
      // são verificados no HTML inteiro (abaixo), não por fragmento.
      const fragmentos = [...html.matchAll(/\/educational-feedback\?([^'"\s)]+)/g)].map((m) => m[1]);
      expect(fragmentos.length).toBeGreaterThan(0);
      for (const frag of fragmentos) {
        expect(frag).toContain(`${EDU_FEEDBACK_QUERY.template}=`);
      }

      // §1.3d: os nomes divergentes NÃO podem existir em nenhum lugar do molde.
      expect(html).not.toContain('campaign=');
      expect(html).not.toMatch(/[?&]target=/);

      // Params canônicos de auditoria presentes (só a URL educacional os usa como
      // query; o endpoint de tracking usa segmentos de rota, não `c=`/`t=`).
      expect(html).toMatch(new RegExp(`[?&]${CAMPAIGN}=`));
      expect(html).toMatch(new RegExp(`[?&]${TARGET}=`));
    },
  );
});
