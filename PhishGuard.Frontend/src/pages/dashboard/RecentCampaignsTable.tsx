import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  IconButton,
  LinearProgress,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import AddIcon from '@mui/icons-material/Add';
import { alpha, useTheme } from '@mui/material/styles';
import { CampaignStatus } from '../../data/campaignStatus';
import { clampPercent, formatDate, formatInteger, formatPercent } from './dashboard.formatters';
import type { DashboardRecentCampaign } from './dashboard.types';

interface RecentCampaignsTableProps {
  campaigns: DashboardRecentCampaign[];
  loading: boolean;
  onViewCampaign: (id: string) => void;
  onNewCampaign: () => void;
}

const STATUS_COLOR: Record<string, 'default' | 'warning' | 'info' | 'success'> = {
  [CampaignStatus.Rascunho]: 'default',
  [CampaignStatus.Agendada]: 'warning',
  [CampaignStatus.Processando]: 'info',
  [CampaignStatus.EmAndamento]: 'success',
  [CampaignStatus.Finalizada]: 'default',
};

function InlineRate({ value, color }: { value: number; color: string }) {
  return (
    <Stack direction="row" spacing={1} alignItems="center" sx={{ minWidth: 130 }}>
      <Typography variant="body2" sx={{ width: 46, textAlign: 'right', fontVariantNumeric: 'tabular-nums' }}>
        {formatPercent(value)}
      </Typography>
      <LinearProgress
        variant="determinate"
        value={clampPercent(value)}
        aria-label={`${formatPercent(value)} de taxa`}
        sx={{
          width: 68,
          height: 5,
          borderRadius: 999,
          bgcolor: alpha(color, 0.13),
          '& .MuiLinearProgress-bar': { bgcolor: color, borderRadius: 999 },
        }}
      />
    </Stack>
  );
}

function InlineOpenRate({ campaign }: { campaign: DashboardRecentCampaign }) {
  const theme = useTheme();
  const semClique = campaign.openedWithoutClickTotal;
  const detalhe = campaign.openedTotal === 0
    ? 'Nenhuma abertura registrada'
    : semClique === 0
      ? 'Todas as aberturas tiveram clique'
      : `${formatInteger(semClique)} ${semClique === 1 ? 'abriu' : 'abriram'} sem clicar`;
  const label = `${formatPercent(campaign.openRate)} de abertura; ${detalhe}`;

  return (
    <Tooltip
      arrow
      title={campaign.openedTotal === 0
        ? 'Nenhuma abertura foi observada ou inferida nesta campanha.'
        : semClique === 0
          ? 'Todas as aberturas efetivas desta campanha também tiveram clique.'
          : `Aberturas efetivas ÷ e-mails enviados. ${formatInteger(semClique)} ${semClique === 1 ? 'destinatário abriu' : 'destinatários abriram'} sem clicar (${formatPercent(campaign.openedWithoutClickRate)} dos enviados).`}
    >
      <Box tabIndex={0} aria-label={label} sx={{ width: 'fit-content', outlineOffset: 2 }}>
        <InlineRate value={campaign.openRate} color={theme.palette.success.main} />
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', pl: 1, mt: 0.25 }}>
          {detalhe}
        </Typography>
      </Box>
    </Tooltip>
  );
}

export default function RecentCampaignsTable({
  campaigns,
  loading,
  onViewCampaign,
  onNewCampaign,
}: RecentCampaignsTableProps) {
  const theme = useTheme();

  return (
    <Card elevation={0}>
      <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
        <Box sx={{ px: { xs: 2, sm: 3 }, pt: 2.5, pb: 2 }}>
          <Typography component="h2" variant="h6" sx={{ fontWeight: 750 }}>Campanhas Recentes</Typography>
          <Typography variant="body2" color="text.secondary">
            Desempenho das campanhas com envios dentro do período selecionado.
          </Typography>
        </Box>

        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small" aria-label="Campanhas recentes" sx={{ minWidth: 1240 }}>
            <TableHead>
              <TableRow>
                <TableCell>Nome da Campanha</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell align="center">Data</TableCell>
                <TableCell align="center">Enviados</TableCell>
                <TableCell>Taxa de Abertura</TableCell>
                <TableCell>Taxa de Clique</TableCell>
                <TableCell>Comprometimento</TableCell>
                <TableCell>Aprendizado</TableCell>
                <TableCell align="center">Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? Array.from({ length: 4 }).map((_, index) => (
                <TableRow key={index}>
                  <TableCell colSpan={9}><Skeleton height={34} /></TableCell>
                </TableRow>
              )) : campaigns.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} align="center">
                    <Stack alignItems="center" spacing={1.5} sx={{ py: 5 }}>
                      <Typography sx={{ fontWeight: 700 }}>Nenhuma campanha com envios neste período</Typography>
                      <Typography variant="body2" color="text.secondary">
                        Ajuste os filtros ou inicie uma nova simulação.
                      </Typography>
                      <Button variant="outlined" startIcon={<AddIcon />} onClick={onNewCampaign}>
                        Criar campanha
                      </Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              ) : campaigns.map((campaign) => (
                <TableRow key={campaign.id} hover>
                  <TableCell component="th" scope="row" sx={{ fontWeight: 650, maxWidth: 260 }}>
                    <Typography variant="body2" noWrap title={campaign.name}>{campaign.name}</Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Chip
                      size="small"
                      label={campaign.status}
                      color={STATUS_COLOR[campaign.status] ?? 'default'}
                      variant={campaign.status === CampaignStatus.Finalizada ? 'outlined' : 'filled'}
                    />
                  </TableCell>
                  <TableCell align="center" sx={{ whiteSpace: 'nowrap' }}>{formatDate(campaign.date)}</TableCell>
                  <TableCell align="center" sx={{ fontVariantNumeric: 'tabular-nums' }}>{formatInteger(campaign.sent)}</TableCell>
                  <TableCell><InlineOpenRate campaign={campaign} /></TableCell>
                  <TableCell><InlineRate value={campaign.clickRate} color={theme.palette.warning.main} /></TableCell>
                  <TableCell><InlineRate value={campaign.compromiseRate} color={theme.palette.error.main} /></TableCell>
                  <TableCell><InlineRate value={campaign.trainingRate} color={theme.palette.info.main} /></TableCell>
                  <TableCell align="center">
                    <Tooltip title="Ver campanha">
                      <IconButton size="small" onClick={() => onViewCampaign(campaign.id)} aria-label={`Ver campanha ${campaign.name}`}>
                        <VisibilityOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );
}
