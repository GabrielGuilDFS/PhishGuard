import { CampaignStatus } from '../../data/campaignStatus';
import type { DashboardOverview, DashboardPeriod, DashboardRecentCampaign } from './dashboard.types';

const DAY_MS = 24 * 60 * 60 * 1000;
const PERIOD_DAYS: Record<DashboardPeriod, number> = { '7d': 7, '30d': 30, '90d': 90 };
const DEPARTMENTS = [
  { name: 'Financeiro', targets: 72, risk: 1.15 },
  { name: 'Tecnologia', targets: 58, risk: 0.7 },
  { name: 'Comercial', targets: 86, risk: 1.25 },
  { name: 'Recursos Humanos', targets: 38, risk: 0.85 },
  { name: 'Operações', targets: 96, risk: 1.1 },
] as const;

const CAMPAIGNS = [
  { name: 'Microsoft 365 — acesso seguro', age: 2, scale: 0.9, open: 0.76, click: 0.18, submit: 0.048, education: 0.145, training: 0.116 },
  { name: 'Confirmação de benefício', age: 6, scale: 1, open: 0.83, click: 0.26, submit: 0.081, education: 0.232, training: 0.189 },
  { name: 'Atualização cadastral', age: 12, scale: 1.1, open: 0.81, click: 0.225, submit: 0.06, education: 0.21, training: 0.178 },
  { name: 'Aviso de segurança da conta', age: 20, scale: 0.95, open: 0.78, click: 0.19, submit: 0.047, education: 0.175, training: 0.147 },
  { name: 'Treinamento de conscientização', age: 27, scale: 1, open: 0.87, click: 0.165, submit: 0.032, education: 0.153, training: 0.132 },
  { name: 'Validação de acesso corporativo', age: 54, scale: 1, open: 0.75, click: 0.3, submit: 0.09, education: 0.26, training: 0.205 },
] as const;

interface Counts {
  sent: number;
  opened: number;
  observed: number;
  inferred: number;
  clicked: number;
  compromised: number;
  educationViewed: number;
  trained: number;
  recovered: number;
}

interface DemoRecord extends Counts {
  campaignId: string;
  campaignName: string;
  status: string;
  department: string;
  timestamp: number;
}

function sumCounts(records: readonly DemoRecord[]): Counts {
  return records.reduce((totals, record) => ({
    sent: totals.sent + record.sent,
    opened: totals.opened + record.opened,
    observed: totals.observed + record.observed,
    inferred: totals.inferred + record.inferred,
    clicked: totals.clicked + record.clicked,
    compromised: totals.compromised + record.compromised,
    educationViewed: totals.educationViewed + record.educationViewed,
    trained: totals.trained + record.trained,
    recovered: totals.recovered + record.recovered,
  }), { sent: 0, opened: 0, observed: 0, inferred: 0, clicked: 0, compromised: 0, educationViewed: 0, trained: 0, recovered: 0 });
}

function rate(numerator: number, denominator: number): number {
  return denominator === 0 ? 0 : Math.round(numerator / denominator * 1000) / 10;
}

/** Dados fictícios apenas para apresentação; nunca são enviados ou persistidos na API. */
export function createDashboardDemoOverview(
  period: DashboardPeriod,
  department: string,
  now = new Date(),
): DashboardOverview {
  const days = PERIOD_DAYS[period];
  const end = now.getTime();
  const start = end - days * DAY_MS;
  const records: DemoRecord[] = CAMPAIGNS.flatMap((campaign, index) => {
    if (campaign.age > days) return [];
    return DEPARTMENTS.filter((item) => !department || item.name === department).map((item) => {
      const sent = Math.round(item.targets * campaign.scale);
      const opened = Math.round(sent * campaign.open);
      const clicked = Math.min(opened, Math.round(sent * campaign.click * item.risk));
      const compromised = Math.min(clicked, Math.round(sent * campaign.submit * item.risk));
      const educationViewed = Math.min(clicked, Math.max(compromised, Math.round(sent * campaign.education * item.risk)));
      const trained = Math.min(educationViewed, Math.round(sent * campaign.training * item.risk));
      const inferred = Math.round(clicked * 0.12);
      return {
        campaignId: `demo-campaign-${index + 1}`,
        campaignName: campaign.name,
        status: campaign.age <= 6 ? CampaignStatus.EmAndamento : CampaignStatus.Finalizada,
        department: item.name,
        timestamp: end - campaign.age * DAY_MS,
        sent, opened, clicked, compromised, educationViewed, trained, inferred,
        observed: opened - inferred,
        recovered: Math.min(trained, Math.round(compromised * 0.78)),
      };
    });
  });
  const totals = sumCounts(records);
  const campaignIds = [...new Set(records.map((record) => record.campaignId))];
  const recentCampaigns: DashboardRecentCampaign[] = campaignIds.map((id) => {
    const campaignRecords = records.filter((record) => record.campaignId === id);
    const campaign = campaignRecords[0];
    const counts = sumCounts(campaignRecords);
    return {
      id, name: campaign.campaignName, status: campaign.status,
      date: new Date(campaign.timestamp).toISOString(),
      sent: counts.sent,
      openedTotal: counts.opened,
      openRate: rate(counts.opened, counts.sent),
      openedWithoutClickTotal: counts.opened - counts.clicked,
      openedWithoutClickRate: rate(counts.opened - counts.clicked, counts.sent),
      clickRate: rate(counts.clicked, counts.sent),
      compromiseRate: rate(counts.compromised, counts.sent),
      educationViewRate: rate(counts.educationViewed, counts.sent),
      educationAbandonmentTotal: counts.educationViewed - counts.trained,
      educationAbandonmentRate: rate(counts.educationViewed - counts.trained, counts.educationViewed),
      trainedTotal: counts.trained,
      trainingCompletionRate: rate(counts.trained, counts.sent),
      trainingRate: rate(counts.trained, counts.clicked),
    };
  });

  const points = Math.min(days, 12);
  const dateLabel = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', timeZone: 'America/Sao_Paulo' });
  const trend = Array.from({ length: points + 1 }, (_, index) => {
    const timestamp = start + Math.round(index * days / points) * DAY_MS;
    const counts = sumCounts(records.filter((record) => record.timestamp <= timestamp));
    return {
      label: dateLabel.format(new Date(timestamp)),
      bucketStart: new Date(timestamp).toISOString(),
      sent: counts.sent, opened: counts.opened, clicked: counts.clicked,
      compromised: counts.compromised, educationViewed: counts.educationViewed, trained: counts.trained,
    };
  });

  return {
    period: { start: new Date(start).toISOString(), end: now.toISOString(), label: `Últimos ${days} dias` },
    tenant: { name: 'Empresa Exemplo' },
    availableDepartments: DEPARTMENTS.map((item) => item.name),
    scope: {
      campaignCount: campaignIds.length,
      // Os mesmos grupos fictícios participam de várias campanhas; não somar destinatários repetidos.
      uniqueTargetCount: DEPARTMENTS.reduce((count, item) => count + Math.max(0, ...records.filter((record) => record.department === item.name).map((record) => record.sent)), 0),
      campaignTargetCount: totals.sent,
    },
    kpis: {
      sent: { total: totals.sent, deltaPercent: totals.sent ? 18.6 : null },
      openRate: { rate: rate(totals.opened, totals.sent), uniqueTotal: totals.opened, observedTotal: totals.observed, inferredTotal: totals.inferred },
      clickRate: { rate: rate(totals.clicked, totals.sent), uniqueTotal: totals.clicked },
      compromiseRate: { rate: rate(totals.compromised, totals.sent), uniqueTotal: totals.compromised },
      trainingRate: { rate: rate(totals.trained, totals.clicked), uniqueTotal: totals.trained },
    },
    trainingEffectiveness: {
      compromised: { rate: rate(totals.compromised, totals.sent), uniqueTotal: totals.compromised },
      educationViewed: { rate: rate(totals.educationViewed, totals.sent), uniqueTotal: totals.educationViewed },
      completed: { rate: rate(totals.trained, totals.sent), uniqueTotal: totals.trained },
      abandoned: { rate: rate(totals.educationViewed - totals.trained, totals.educationViewed), uniqueTotal: totals.educationViewed - totals.trained },
      recovery: { rate: rate(totals.recovered, totals.compromised), uniqueTotal: totals.recovered },
    },
    trend,
    recentCampaigns,
  };
}
