// Status de campanha (espelha PhishGuard.Backend/Models/CampaignStatus.cs).
// Fluxo canônico: Rascunho → Agendada → Em Andamento → Finalizada.
export const CampaignStatus = {
  Rascunho: 'Rascunho',
  Agendada: 'Agendada',
  EmAndamento: 'Em Andamento',
  Finalizada: 'Finalizada',
} as const;

export type CampaignStatusValue = typeof CampaignStatus[keyof typeof CampaignStatus];
