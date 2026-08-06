// Status de campanha (espelha PhishGuard.Backend/Models/CampaignStatus.cs).
// Fluxo canônico: Rascunho → Agendada → Processando → Em Andamento → Finalizada.
// 'Processando' = disparo reivindicado (claim), enviando o lote de forma assíncrona.
export const CampaignStatus = {
  Rascunho: 'Rascunho',
  Agendada: 'Agendada',
  Processando: 'Processando',
  FalhaNoDisparo: 'Falha no Disparo',
  EmAndamento: 'Em Andamento',
  Finalizada: 'Finalizada',
} as const;

export type CampaignStatusValue = typeof CampaignStatus[keyof typeof CampaignStatus];
