import { Box, Button, Card, CardContent, Skeleton, Stack, Typography } from '@mui/material';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
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
import { formatInteger } from './dashboard.formatters';
import type { DashboardScope, DashboardTrendPoint } from './dashboard.types';

interface SimulationTrendChartProps {
  data: DashboardTrendPoint[];
  loading: boolean;
  hasSent: boolean;
  scope: DashboardScope;
  onOpenGlossary: () => void;
}

function plural(value: number, singular: string, pluralForm: string) {
  return value === 1 ? singular : pluralForm;
}

export default function SimulationTrendChart({
  data,
  loading,
  hasSent,
  scope,
  onOpenGlossary,
}: SimulationTrendChartProps) {
  const theme = useTheme();
  const grid = alpha(theme.palette.text.primary, theme.palette.mode === 'dark' ? 0.16 : 0.12);
  const tick = { fill: theme.palette.text.secondary, fontSize: 12 };

  return (
    <Card elevation={0} sx={{ mb: 3 }}>
      <CardContent sx={{ p: { xs: 2, sm: 3 }, '&:last-child': { pb: { xs: 2, sm: 3 } } }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'flex-start' }} spacing={1.5} sx={{ mb: 2.5 }}>
          <Box>
            <Typography component="h2" variant="h6" sx={{ fontWeight: 750 }}>Tendência do Funil de Simulação</Typography>
            <Typography variant="body2" color="text.secondary">
              Valores acumulados de todas as campanhas com envios no período e filtros selecionados.
            </Typography>
            {!loading && hasSent && (
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.75 }}>
                {formatInteger(scope.campaignCount)} {plural(scope.campaignCount, 'campanha', 'campanhas')}
                {' · '}{formatInteger(scope.uniqueTargetCount)} {plural(scope.uniqueTargetCount, 'destinatário único', 'destinatários únicos')}
                {' · '}{formatInteger(scope.campaignTargetCount)} {plural(scope.campaignTargetCount, 'e-mail enviado', 'e-mails enviados')}
              </Typography>
            )}
          </Box>
          <Button size="small" variant="text" startIcon={<InfoOutlinedIcon />} onClick={onOpenGlossary} sx={{ flexShrink: 0 }}>
            Como são calculadas?
          </Button>
        </Stack>

        {loading ? (
          <Skeleton variant="rounded" height={320} />
        ) : !hasSent ? (
          <Box sx={{ height: 280, display: 'grid', placeItems: 'center', textAlign: 'center', px: 2 }}>
            <Box>
              <Typography sx={{ fontWeight: 700 }}>Nenhum envio no período</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Ajuste os filtros ou crie uma campanha para iniciar a leitura do funil.
              </Typography>
            </Box>
          </Box>
        ) : (
          <Box sx={{ width: '100%', height: { xs: 290, sm: 340 } }}>
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={data} margin={{ top: 8, right: 12, left: -16, bottom: 4 }}>
                <CartesianGrid stroke={grid} strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="label" tick={tick} axisLine={{ stroke: grid }} tickLine={false} minTickGap={24} />
                <YAxis tick={tick} axisLine={false} tickLine={false} allowDecimals={false} />
                <Tooltip
                  contentStyle={{
                    background: theme.palette.background.paper,
                    border: `1px solid ${theme.palette.divider}`,
                    borderRadius: 10,
                    color: theme.palette.text.primary,
                  }}
                  labelStyle={{ color: theme.palette.text.primary, fontWeight: 700 }}
                />
                <Legend wrapperStyle={{ paddingTop: 12, fontSize: 12 }} iconType="circle" />
                <Line type="monotone" dataKey="sent" name="E-mails disparados" stroke={theme.palette.primary.main} strokeWidth={2.5} dot={false} activeDot={{ r: 5 }} />
                <Line type="monotone" dataKey="opened" name="Abertos" stroke={statusColors.success} strokeWidth={2.25} dot={false} activeDot={{ r: 5 }} />
                <Line type="monotone" dataKey="clicked" name="Clicados" stroke={statusColors.warning} strokeWidth={2.25} dot={false} activeDot={{ r: 5 }} />
                <Line type="monotone" dataKey="compromised" name="Comprometidos" stroke={statusColors.danger} strokeWidth={2.25} dot={false} activeDot={{ r: 5 }} />
                <Line type="monotone" dataKey="trained" name="Treinados" stroke={theme.palette.info.main} strokeWidth={2.25} dot={false} activeDot={{ r: 5 }} />
              </LineChart>
            </ResponsiveContainer>
          </Box>
        )}
      </CardContent>
    </Card>
  );
}
