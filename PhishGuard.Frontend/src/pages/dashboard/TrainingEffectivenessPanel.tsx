import {
  Box,
  Card,
  CardContent,
  LinearProgress,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { alpha, useTheme } from '@mui/material/styles';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { statusColors } from '../../theme';
import { clampPercent, formatInteger, formatPercent } from './dashboard.formatters';
import type {
  DashboardOverview,
  DashboardRateKpi,
  DashboardRecentCampaign,
  DashboardTrendPoint,
} from './dashboard.types';

interface TrainingEffectivenessPanelProps {
  metrics?: DashboardOverview['trainingEffectiveness'];
  trend: DashboardTrendPoint[];
  campaigns: DashboardRecentCampaign[];
  loading: boolean;
  hasSent: boolean;
}

interface MetricCardProps {
  title: string;
  metric?: DashboardRateKpi;
  detail: string;
  color: string;
  loading: boolean;
}

function MetricCard({ title, metric, detail, color, loading }: MetricCardProps) {
  return (
    <Box sx={{ p: 2, border: 1, borderColor: 'divider', borderRadius: 2, minWidth: 0 }}>
      <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 650 }}>{title}</Typography>
      {loading ? <Skeleton width={100} height={42} /> : (
        <Typography component="p" variant="h5" sx={{ mt: 0.5, fontWeight: 800, fontVariantNumeric: 'tabular-nums' }}>
          {formatPercent(metric?.rate ?? 0)}
        </Typography>
      )}
      <LinearProgress
        variant="determinate"
        value={loading ? 0 : clampPercent(metric?.rate ?? 0)}
        aria-label={`${title}: ${formatPercent(metric?.rate ?? 0)}`}
        sx={{ my: 1, height: 5, borderRadius: 999, bgcolor: alpha(color, 0.13), '& .MuiLinearProgress-bar': { bgcolor: color } }}
      />
      <Typography variant="caption" color="text.secondary">
        {loading ? <Skeleton width="70%" /> : `${formatInteger(metric?.uniqueTotal ?? 0)} ${detail}`}
      </Typography>
    </Box>
  );
}

export default function TrainingEffectivenessPanel({
  metrics,
  trend,
  campaigns,
  loading,
  hasSent,
}: TrainingEffectivenessPanelProps) {
  const theme = useTheme();
  const grid = alpha(theme.palette.text.primary, theme.palette.mode === 'dark' ? 0.16 : 0.12);
  const tick = { fill: theme.palette.text.secondary, fontSize: 12 };

  return (
    <Card elevation={0} sx={{ mb: 3 }}>
      <CardContent sx={{ p: { xs: 2, sm: 3 }, '&:last-child': { pb: { xs: 2, sm: 3 } } }}>
        <Typography component="h2" variant="h6" sx={{ fontWeight: 750 }}>Efetividade do Treinamento</Typography>
        <Typography variant="body2" color="text.secondary">
          Compara comprometimento, acesso ao conteúdo educativo e conclusão, sem identificar participantes.
        </Typography>

        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(190px, 1fr))', gap: 1.5, my: 2.5 }}>
          <MetricCard title="Comprometimento" metric={metrics?.compromised} detail="submissões únicas entre os enviados" color={statusColors.danger} loading={loading} />
          <MetricCard title="Acessaram o treinamento" metric={metrics?.educationViewed} detail="acessos únicos entre os enviados" color={theme.palette.info.main} loading={loading} />
          <MetricCard title="Concluíram o treinamento" metric={metrics?.completed} detail="conclusões únicas entre os enviados" color={statusColors.success} loading={loading} />
          <MetricCard title="Abandono educacional" metric={metrics?.abandoned} detail="acessos sem conclusão, entre quem acessou" color={statusColors.warning} loading={loading} />
        </Box>

        {!loading && hasSent && (
          <Typography
            variant="body2"
            aria-label={`Recuperação após comprometimento: ${formatPercent(metrics?.recovery.rate ?? 0)}`}
            sx={{ mb: 2 }}
          >
            Recuperação após comprometimento: <strong>{formatPercent(metrics?.recovery.rate ?? 0)}</strong>
            {' · '}{formatInteger(metrics?.recovery.uniqueTotal ?? 0)} participantes comprometidos concluíram o treinamento.
          </Typography>
        )}

        {loading ? (
          <Skeleton variant="rounded" height={280} />
        ) : !hasSent ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography sx={{ fontWeight: 700 }}>Nenhum envio no período</Typography>
          </Box>
        ) : (
          <Box sx={{ width: '100%', height: { xs: 260, sm: 310 } }}>
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={trend} margin={{ top: 8, right: 12, left: -16, bottom: 4 }}>
                <CartesianGrid stroke={grid} strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="label" tick={tick} axisLine={{ stroke: grid }} tickLine={false} minTickGap={24} />
                <YAxis tick={tick} axisLine={false} tickLine={false} allowDecimals={false} />
                <Tooltip
                  contentStyle={{ background: theme.palette.background.paper, border: `1px solid ${theme.palette.divider}`, borderRadius: 10, color: theme.palette.text.primary }}
                  labelStyle={{ color: theme.palette.text.primary, fontWeight: 700 }}
                />
                <Legend wrapperStyle={{ paddingTop: 12, fontSize: 12 }} iconType="circle" />
                <Line type="monotone" dataKey="compromised" name="Comprometidos" stroke={statusColors.danger} strokeWidth={2.5} dot={false} />
                <Line type="monotone" dataKey="educationViewed" name="Acessaram o treinamento" stroke={theme.palette.info.main} strokeWidth={2.5} dot={false} />
                <Line type="monotone" dataKey="trained" name="Concluíram" stroke={statusColors.success} strokeWidth={2.5} dot={false} />
              </LineChart>
            </ResponsiveContainer>
          </Box>
        )}

        {!loading && campaigns.length > 0 && (
          <TableContainer sx={{ mt: 2.5, overflowX: 'auto' }}>
            <Table size="small" aria-label="Efetividade do treinamento por campanha" sx={{ minWidth: 760 }}>
              <TableHead>
                <TableRow>
                  <TableCell>Campanha</TableCell>
                  <TableCell align="center">Comprometimento</TableCell>
                  <TableCell align="center">Acesso educacional</TableCell>
                  <TableCell align="center">Conclusão</TableCell>
                  <TableCell align="center">Abandono</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {campaigns.map((campaign) => (
                  <TableRow key={campaign.id} hover>
                    <TableCell component="th" scope="row" sx={{ fontWeight: 650 }}>{campaign.name}</TableCell>
                    <TableCell align="center">{formatPercent(campaign.compromiseRate)}</TableCell>
                    <TableCell align="center">{formatPercent(campaign.educationViewRate)}</TableCell>
                    <TableCell align="center">{formatPercent(campaign.trainingCompletionRate)}</TableCell>
                    <TableCell align="center">
                      {formatPercent(campaign.educationAbandonmentRate)}
                      <Typography component="span" variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                        {formatInteger(campaign.educationAbandonmentTotal)} sem concluir
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </CardContent>
    </Card>
  );
}
