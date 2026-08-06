// ============================================================================
// Contrato de rastreamento COMPARTILHADO (Passo 12 do roadmap de QA).
// ============================================================================
//
// ÚNICO ponto de verdade sobre as URLs, o payload e o formato do `redirectUrl`
// do fluxo de tracking. Existe para eliminar a divergência histórica (§1.3d) em
// que o FRONT redirecionava para `/educational-feedback?template=…&c=…&t=…` e o
// BACK devolvia `redirectUrl = /educational-feedback?campaign=…` — nomes de
// parâmetro incompatíveis que quebravam qualquer cliente que honrasse a resposta.
//
import { API_BASE } from '../config';

// O backend tem um ESPELHO deste contrato em C#:
//   PhishGuard.Backend/Contracts/TrackingContract.cs
// Os dois lados são testados contra o MESMO formato (ver *.test.ts e os testes
// do TrackingController). Ao mudar algo aqui, mude lá também — e vice-versa.

/** Ações de rastreamento — o segmento de rota em `/api/tracking/<ação>/:c/:t`. */
export const TRACKING_ACTIONS = {
  open: 'open',
  click: 'click',
  submit: 'submit',
  educationalView: 'educational-view',
  complete: 'complete',
} as const;

export type TrackingAction = (typeof TRACKING_ACTIONS)[keyof typeof TRACKING_ACTIONS];

/** Prefixo do tracking: relativo em dev/Docker e absoluto no Static Site. */
export const TRACKING_API_BASE = `${API_BASE}/tracking`;

/** Monta a URL de um endpoint de rastreamento: `/api/tracking/<ação>/:c/:t`. */
export function trackingEndpoint(
  action: TrackingAction,
  campaignId: string,
  targetId: string,
  trackingToken: string,
): string {
  const query = new URLSearchParams({ k: trackingToken });
  return `${TRACKING_API_BASE}/${action}/${campaignId}/${targetId}?${query.toString()}`;
}

/**
 * Payload do `POST /submit` — APENAS metadados de validação.
 * GARANTIA LGPD/escopo educacional: a senha real NUNCA trafega; só flags/tamanhos.
 * Espelha `CaptureMetadataDto` no backend.
 */
export interface CaptureMetadata {
  camposPreenchidos: boolean;
  senhasCoincidem: boolean;
  tamanhoSenha: number;
  /** Algumas landings sinalizam se o e-mail foi informado (opcional no contrato). */
  emailInformado?: boolean;
}

/** Resposta do `POST /submit`. Espelha `TrackSubmitResponseDto` no backend. */
export interface TrackSubmitResponse {
  status: string;
  redirectUrl: string;
}

/** Rota (SPA) da tela educacional de feedback. */
export const EDUCATIONAL_FEEDBACK_PATH = '/educational-feedback';

/**
 * Nomes CANÔNICOS dos parâmetros de query da tela educacional — o coração do
 * contrato. Front e back usam EXATAMENTE estes nomes (fim do `campaign` vs `c`).
 */
export const EDU_FEEDBACK_QUERY = {
  /** Qual treinamento renderizar (conceito de front; opcional no redirect do back). */
  template: 'template',
  /** ID da campanha. */
  campaign: 'c',
  /** ID do alvo. */
  target: 't',
  /** Capability token assinado do participante. */
  token: 'k',
} as const;

export interface EducationalFeedbackParams {
  template?: string;
  campaignId?: string;
  targetId?: string;
  trackingToken?: string;
}

/**
 * Monta a URL canônica da tela educacional de feedback (com URL-encoding).
 * Campos ausentes/vazios são omitidos. Use em navegação de runtime no SPA.
 *
 * ⚠️ NÃO use para embutir placeholders `{{CAMPAIGN_ID}}` em HTML de landing: o
 * encoding quebraria o replace literal do backend — para isso existe o formato
 * cru documentado nos moldes (data/landingTemplates.ts), validado por teste de
 * conformidade contra ESTE contrato.
 */
export function educationalFeedbackUrl(params: EducationalFeedbackParams): string {
  const qs = new URLSearchParams();
  if (params.template) qs.set(EDU_FEEDBACK_QUERY.template, params.template);
  if (params.campaignId) qs.set(EDU_FEEDBACK_QUERY.campaign, params.campaignId);
  if (params.targetId) qs.set(EDU_FEEDBACK_QUERY.target, params.targetId);
  if (params.trackingToken) qs.set(EDU_FEEDBACK_QUERY.token, params.trackingToken);
  const query = qs.toString();
  return query ? `${EDUCATIONAL_FEEDBACK_PATH}?${query}` : EDUCATIONAL_FEEDBACK_PATH;
}

/**
 * Lê os parâmetros canônicos (`template`/`c`/`t`) de uma query string. Fonte única
 * de leitura para que nenhum consumidor volte a inventar nomes divergentes.
 */
export function readEducationalFeedbackParams(
  search: URLSearchParams,
): { template: string | null; campaignId: string | null; targetId: string | null; trackingToken: string | null } {
  return {
    template: search.get(EDU_FEEDBACK_QUERY.template),
    campaignId: search.get(EDU_FEEDBACK_QUERY.campaign),
    targetId: search.get(EDU_FEEDBACK_QUERY.target),
    trackingToken: search.get(EDU_FEEDBACK_QUERY.token),
  };
}
