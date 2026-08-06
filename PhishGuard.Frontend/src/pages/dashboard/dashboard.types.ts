export type DashboardPeriod = '7d' | '30d' | '90d';

export interface DashboardRateKpi {
  rate: number;
  uniqueTotal: number;
}

export interface DashboardOpenRateKpi extends DashboardRateKpi {
  observedTotal: number;
  inferredTotal: number;
}

export interface DashboardOverview {
  period: {
    start: string;
    end: string;
    label: string;
  };
  tenant: {
    name: string;
  };
  scope: DashboardScope;
  availableDepartments: string[];
  kpis: {
    sent: {
      total: number;
      deltaPercent: number | null;
    };
    openRate: DashboardOpenRateKpi;
    clickRate: DashboardRateKpi;
    compromiseRate: DashboardRateKpi;
    trainingRate: DashboardRateKpi;
  };
  trainingEffectiveness: {
    compromised: DashboardRateKpi;
    educationViewed: DashboardRateKpi;
    completed: DashboardRateKpi;
    abandoned: DashboardRateKpi;
    recovery: DashboardRateKpi;
  };
  trend: DashboardTrendPoint[];
  recentCampaigns: DashboardRecentCampaign[];
}

export interface DashboardScope {
  campaignCount: number;
  uniqueTargetCount: number;
  campaignTargetCount: number;
}

export interface DashboardTrendPoint {
  label: string;
  bucketStart: string;
  sent: number;
  opened: number;
  clicked: number;
  compromised: number;
  educationViewed: number;
  trained: number;
}

export interface DashboardRecentCampaign {
  id: string;
  name: string;
  status: string;
  date: string;
  sent: number;
  openedTotal: number;
  openRate: number;
  openedWithoutClickTotal: number;
  openedWithoutClickRate: number;
  clickRate: number;
  compromiseRate: number;
  educationViewRate: number;
  educationAbandonmentTotal: number;
  educationAbandonmentRate: number;
  trainedTotal: number;
  trainingCompletionRate: number;
  trainingRate: number;
}
