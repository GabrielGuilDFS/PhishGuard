import { Box, Card, CardContent, IconButton, LinearProgress, Skeleton, Stack, Tooltip, Typography } from '@mui/material';
import MailOutlineIcon from '@mui/icons-material/MailOutline';
import MarkEmailReadOutlinedIcon from '@mui/icons-material/MarkEmailReadOutlined';
import TouchAppOutlinedIcon from '@mui/icons-material/TouchAppOutlined';
import GppBadOutlinedIcon from '@mui/icons-material/GppBadOutlined';
import SchoolOutlinedIcon from '@mui/icons-material/SchoolOutlined';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import { alpha, useTheme } from '@mui/material/styles';
import { formatInteger, formatPercent, clampPercent } from './dashboard.formatters';
import type { DashboardOverview } from './dashboard.types';

interface DashboardKpiGridProps {
  kpis?: DashboardOverview['kpis'];
  loading: boolean;
}

export default function DashboardKpiGrid({ kpis, loading }: DashboardKpiGridProps) {
  const theme = useTheme();
  const cards = [
    {
      title: 'E-mails Enviados',
      value: kpis ? formatInteger(kpis.sent.total) : '',
      detail: 'Cada campanha + destinatário conta uma vez',
      comparison: kpis?.sent.deltaPercent == null
        ? 'Sem ciclo anterior comparável'
        : `${kpis.sent.deltaPercent >= 0 ? '+' : ''}${formatPercent(kpis.sent.deltaPercent)} vs. ciclo anterior`,
      tooltip: 'Total de envios SMTP bem-sucedidos. O mesmo destinatário em campanhas diferentes conta uma vez em cada campanha; reenvios da mesma campanha não duplicam o total.',
      icon: MailOutlineIcon,
      color: theme.palette.primary.main,
      lgSpan: 2,
    },
    {
      title: 'Taxa de Abertura',
      value: kpis ? formatPercent(kpis.openRate.rate) : '',
      detail: `${formatInteger(kpis?.openRate.uniqueTotal ?? 0)} aberturas efetivas`,
      progress: kpis?.openRate.rate,
      icon: MarkEmailReadOutlinedIcon,
      color: theme.palette.success.main,
      tooltip: kpis
        ? `Aberturas efetivas ÷ e-mails enviados. ${formatInteger(kpis.openRate.observedTotal)} observadas pelo pixel e ${formatInteger(kpis.openRate.inferredTotal)} inferidas por ações posteriores.`
        : undefined,
      lgSpan: 1,
    },
    {
      title: 'Taxa de Clique',
      value: kpis ? formatPercent(kpis.clickRate.rate) : '',
      detail: `${formatInteger(kpis?.clickRate.uniqueTotal ?? 0)} cliques únicos`,
      progress: kpis?.clickRate.rate,
      icon: TouchAppOutlinedIcon,
      color: theme.palette.warning.main,
      tooltip: 'Cliques únicos por campanha e destinatário ÷ e-mails enviados. Cliques repetidos no mesmo link contam uma vez.',
      lgSpan: 1,
    },
    {
      title: 'Taxa de Comprometimento',
      value: kpis ? formatPercent(kpis.compromiseRate.rate) : '',
      detail: `${formatInteger(kpis?.compromiseRate.uniqueTotal ?? 0)} credenciais submetidas`,
      progress: kpis?.compromiseRate.rate,
      icon: GppBadOutlinedIcon,
      color: theme.palette.error.main,
      tooltip: 'Submissões únicas por campanha e destinatário ÷ e-mails enviados. Apenas o evento é registrado; credenciais não são armazenadas.',
      lgSpan: 2,
    },
    {
      title: 'Aprendizado Concluído',
      value: kpis ? formatPercent(kpis.trainingRate.rate) : '',
      detail: `${formatInteger(kpis?.trainingRate.uniqueTotal ?? 0)} treinamentos concluídos`,
      progress: kpis?.trainingRate.rate,
      icon: SchoolOutlinedIcon,
      color: theme.palette.info.main,
      tooltip: 'Percentual de participantes que concluíram o treinamento entre os que clicaram na simulação.',
      lgSpan: 2,
    },
  ];

  return (
    <Box
      aria-label="Indicadores principais"
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))', lg: 'repeat(4, minmax(0, 1fr))' },
        gap: 2,
        mb: 3,
      }}
    >
      {cards.map((card) => (
        <Card key={card.title} elevation={0} sx={{ gridColumn: { lg: `span ${card.lgSpan}` } }}>
          <CardContent sx={{ p: 2.5, '&:last-child': { pb: 2.5 } }}>
            <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={2}>
              <Box sx={{ minWidth: 0 }}>
                <Stack direction="row" spacing={0.5} alignItems="center">
                  <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 650 }}>
                    {card.title}
                  </Typography>
                  {card.tooltip && (
                    <Tooltip title={card.tooltip} arrow>
                      <IconButton size="small" aria-label={`Como ${card.title} é calculado`} sx={{ p: 0.25 }}>
                        <InfoOutlinedIcon sx={{ fontSize: 15, color: 'text.secondary' }} />
                      </IconButton>
                    </Tooltip>
                  )}
                </Stack>
                {loading ? (
                  <Skeleton width={92} height={46} />
                ) : (
                  <Typography component="p" variant="h4" sx={{ mt: 0.5, fontWeight: 800, letterSpacing: '-0.04em' }}>
                    {card.value}
                  </Typography>
                )}
              </Box>
              <Box sx={{ display: 'flex', p: 1, borderRadius: 2, color: card.color, bgcolor: alpha(card.color, 0.11) }}>
                <card.icon fontSize="small" />
              </Box>
            </Stack>

            {card.progress !== undefined && (
              <LinearProgress
                variant="determinate"
                value={clampPercent(card.progress)}
                aria-label={`Progresso de ${card.title}`}
                sx={{
                  mt: 2,
                  mb: 1,
                  height: 5,
                  borderRadius: 999,
                  bgcolor: alpha(card.color, 0.12),
                  '& .MuiLinearProgress-bar': { bgcolor: card.color, borderRadius: 999 },
                }}
              />
            )}
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: card.progress === undefined ? 2 : 0 }}>
              {loading ? <Skeleton width="75%" /> : card.detail}
            </Typography>
            {card.comparison && (
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                {loading ? <Skeleton width="65%" /> : card.comparison}
              </Typography>
            )}
          </CardContent>
        </Card>
      ))}
    </Box>
  );
}
