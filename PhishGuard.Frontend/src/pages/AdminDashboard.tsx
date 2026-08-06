import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Box, Button, LinearProgress } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useNavigate } from 'react-router-dom';
import PageContainer from '../components/PageContainer';
import { API_BASE } from '../config';
import { useNotify } from '../context/NotificationContext';
import { authFetch, clearSession } from '../auth/session';
import DashboardHeader from './dashboard/DashboardHeader';
import DashboardKpiGrid from './dashboard/DashboardKpiGrid';
import SimulationTrendChart from './dashboard/SimulationTrendChart';
import TrainingEffectivenessPanel from './dashboard/TrainingEffectivenessPanel';
import RecentCampaignsTable from './dashboard/RecentCampaignsTable';
import MetricsGlossaryDialog from './dashboard/MetricsGlossaryDialog';
import type { DashboardOverview, DashboardPeriod } from './dashboard/dashboard.types';
import {
  buildDashboardQuery,
  downloadDashboardBlob,
  parseDownloadFilename,
  type DashboardExportFormat,
} from './dashboard/dashboard.export';

const EMPTY_SCOPE = { campaignCount: 0, uniqueTargetCount: 0, campaignTargetCount: 0 };

async function extractErrorMessage(response: Response): Promise<string> {
  const raw = await response.text().catch(() => '');
  if (!raw) return 'Não foi possível carregar o dashboard.';
  try {
    const parsed = JSON.parse(raw) as { message?: string; title?: string } | string;
    if (typeof parsed === 'string') return parsed;
    return parsed.message || parsed.title || 'Não foi possível carregar o dashboard.';
  } catch {
    return raw;
  }
}

export default function AdminDashboard() {
  const navigate = useNavigate();
  const { showNotify } = useNotify();
  const [period, setPeriod] = useState<DashboardPeriod>('30d');
  const [department, setDepartment] = useState('');
  const [data, setData] = useState<DashboardOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [retryVersion, setRetryVersion] = useState(0);
  const [glossaryOpen, setGlossaryOpen] = useState(false);
  const [exportingFormat, setExportingFormat] = useState<DashboardExportFormat | null>(null);
  const requestIdRef = useRef(0);
  const dataRef = useRef<DashboardOverview | null>(null);
  const showNotifyRef = useRef(showNotify);

  useEffect(() => {
    showNotifyRef.current = showNotify;
  }, [showNotify]);

  const loadDashboard = useCallback(async (signal: AbortSignal, requestId: number) => {
    const params = buildDashboardQuery(period, department);

    setError(null);
    if (dataRef.current) setRefreshing(true);
    else setLoading(true);

    try {
      const response = await authFetch(`${API_BASE}/Dashboard/overview?${params.toString()}`, {
        signal,
      });

      if (response.status === 401) {
        clearSession();
        navigate('/login', { replace: true });
        return;
      }

      if (!response.ok) throw new Error(await extractErrorMessage(response));
      const payload = await response.json() as DashboardOverview;
      if (requestId === requestIdRef.current) {
        dataRef.current = payload;
        setData(payload);
      }
    } catch (caught) {
      if (caught instanceof DOMException && caught.name === 'AbortError') return;
      const message = caught instanceof Error ? caught.message : 'Não foi possível carregar o dashboard.';
      if (requestId === requestIdRef.current) {
        setError(message);
        showNotifyRef.current(message, 'error');
      }
    } finally {
      if (requestId === requestIdRef.current) {
        setLoading(false);
        setRefreshing(false);
      }
    }
  }, [department, navigate, period]);

  const exportDashboard = useCallback(async (format: DashboardExportFormat) => {
    if (exportingFormat) return;
    setExportingFormat(format);
    try {
      const params = buildDashboardQuery(period, department);
      params.set('format', format);
      if (format === 'csv') params.set('dataset', 'campaigns');
      const response = await authFetch(`${API_BASE}/Dashboard/export?${params.toString()}`);

      if (response.status === 401) {
        clearSession();
        navigate('/login', { replace: true });
        return;
      }
      if (!response.ok) throw new Error(await extractErrorMessage(response));

      const blob = await response.blob();
      const filename = parseDownloadFilename(
        response.headers.get('Content-Disposition'),
        `phishguard-dashboard.${format}`,
      );
      downloadDashboardBlob(blob, filename);
      showNotify(`Relatório ${format.toUpperCase()} exportado com sucesso.`, 'success');
    } catch (caught) {
      const message = caught instanceof Error ? caught.message : 'Não foi possível exportar o dashboard.';
      showNotify(message, 'error');
    } finally {
      setExportingFormat(null);
    }
  }, [department, exportingFormat, navigate, period, showNotify]);

  useEffect(() => {
    const controller = new AbortController();
    const requestId = ++requestIdRef.current;
    void loadDashboard(controller.signal, requestId);
    return () => controller.abort();
  }, [loadDashboard, retryVersion]);

  return (
    <PageContainer sx={{ py: { xs: 1, md: 2 } }}>
      <DashboardHeader
        tenantName={data?.tenant.name ?? ''}
        period={period}
        department={department}
        departments={data?.availableDepartments ?? []}
        refreshing={refreshing}
        exportDisabled={loading || refreshing || data === null}
        exportingFormat={exportingFormat}
        onPeriodChange={setPeriod}
        onDepartmentChange={setDepartment}
        onNewCampaign={() => navigate('/admin/campaigns?nova=1')}
        onExport={(format) => { void exportDashboard(format); }}
      />

      {refreshing && <LinearProgress aria-label="Atualizando dashboard" sx={{ mb: 2, borderRadius: 999 }} />}

      {error && !data && (
        <Alert
          severity="error"
          sx={{ mb: 3 }}
          action={
            <Button color="inherit" size="small" startIcon={<RefreshIcon />} onClick={() => setRetryVersion((value) => value + 1)}>
              Tentar novamente
            </Button>
          }
        >
          {error}
        </Alert>
      )}

      <Box aria-busy={loading || refreshing}>
        <DashboardKpiGrid kpis={data?.kpis} loading={loading} />
        <SimulationTrendChart
          data={data?.trend ?? []}
          loading={loading}
          hasSent={(data?.kpis.sent.total ?? 0) > 0}
          scope={data?.scope ?? EMPTY_SCOPE}
          onOpenGlossary={() => setGlossaryOpen(true)}
        />
        <TrainingEffectivenessPanel
          metrics={data?.trainingEffectiveness}
          trend={data?.trend ?? []}
          campaigns={data?.recentCampaigns ?? []}
          loading={loading}
          hasSent={(data?.kpis.sent.total ?? 0) > 0}
        />
        <RecentCampaignsTable
          campaigns={data?.recentCampaigns ?? []}
          loading={loading}
          onNewCampaign={() => navigate('/admin/campaigns?nova=1')}
          onViewCampaign={(id) => navigate(`/admin/campaigns?editar=${encodeURIComponent(id)}`)}
        />
      </Box>
      <MetricsGlossaryDialog open={glossaryOpen} onClose={() => setGlossaryOpen(false)} />
    </PageContainer>
  );
}
