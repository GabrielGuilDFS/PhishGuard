import {
  Box,
  Button,
  CircularProgress,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Menu,
  ListItemIcon,
  Select,
  Stack,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined';
import DownloadOutlinedIcon from '@mui/icons-material/DownloadOutlined';
import PictureAsPdfOutlinedIcon from '@mui/icons-material/PictureAsPdfOutlined';
import TableViewOutlinedIcon from '@mui/icons-material/TableViewOutlined';
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import { useState } from 'react';
import type { DashboardExportFormat } from './dashboard.export';
import type { DashboardPeriod } from './dashboard.types';

interface DashboardHeaderProps {
  tenantName: string;
  period: DashboardPeriod;
  department: string;
  departments: string[];
  refreshing: boolean;
  exportDisabled: boolean;
  exportingFormat: DashboardExportFormat | null;
  onPeriodChange: (period: DashboardPeriod) => void;
  onDepartmentChange: (department: string) => void;
  onNewCampaign: () => void;
  onExport: (format: DashboardExportFormat) => void;
}

export default function DashboardHeader({
  tenantName,
  period,
  department,
  departments,
  refreshing,
  exportDisabled,
  exportingFormat,
  onPeriodChange,
  onDepartmentChange,
  onNewCampaign,
  onExport,
}: DashboardHeaderProps) {
  const [exportAnchor, setExportAnchor] = useState<HTMLElement | null>(null);

  const selectExport = (format: DashboardExportFormat) => {
    setExportAnchor(null);
    onExport(format);
  };

  return (
    <Box
      component="header"
      sx={{
        display: 'flex',
        flexDirection: { xs: 'column', lg: 'row' },
        alignItems: { xs: 'stretch', lg: 'flex-end' },
        justifyContent: 'space-between',
        gap: 2.5,
        mb: 3,
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="h5" component="h1" sx={{ fontWeight: 750, letterSpacing: '-0.02em' }}>
          Visão Geral de Segurança
        </Typography>
        <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1, minWidth: 0 }}>
          <Chip
            size="small"
            icon={<BusinessOutlinedIcon />}
            label={tenantName || 'Tenant atual'}
            variant="outlined"
            sx={{ maxWidth: '100%', '& .MuiChip-label': { overflow: 'hidden', textOverflow: 'ellipsis' } }}
          />
          <Typography variant="caption" color="text.secondary" sx={{ display: { xs: 'none', sm: 'block' } }}>
            Métricas consolidadas do ambiente
          </Typography>
        </Stack>
      </Box>

      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1.25}
        sx={{ width: { xs: '100%', lg: 'auto' } }}
      >
        <FormControl size="small" sx={{ minWidth: { sm: 170 } }} disabled={refreshing}>
          <InputLabel id="dashboard-period-label">Período</InputLabel>
          <Select
            labelId="dashboard-period-label"
            label="Período"
            value={period}
            onChange={(event) => onPeriodChange(event.target.value as DashboardPeriod)}
          >
            <MenuItem value="7d">Últimos 7 dias</MenuItem>
            <MenuItem value="30d">Últimos 30 dias</MenuItem>
            <MenuItem value="90d">Últimos 90 dias</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: { sm: 190 } }} disabled={refreshing}>
          <InputLabel id="dashboard-department-label">Departamento</InputLabel>
          <Select
            labelId="dashboard-department-label"
            label="Departamento"
            value={department}
            onChange={(event) => onDepartmentChange(event.target.value)}
          >
            <MenuItem value="">Todos</MenuItem>
            {departments.map((item) => <MenuItem key={item} value={item}>{item}</MenuItem>)}
          </Select>
        </FormControl>

        <Button
          variant="outlined"
          startIcon={exportingFormat
            ? <CircularProgress size={16} color="inherit" />
            : <DownloadOutlinedIcon />}
          endIcon={!exportingFormat ? <ArrowDropDownIcon /> : undefined}
          disabled={exportDisabled || exportingFormat !== null}
          onClick={(event) => setExportAnchor(event.currentTarget)}
          aria-haspopup="menu"
          aria-expanded={Boolean(exportAnchor)}
          sx={{ minHeight: 40, whiteSpace: 'nowrap', textTransform: 'none', fontWeight: 700 }}
        >
          {exportingFormat ? `Exportando ${exportingFormat.toUpperCase()}...` : 'Exportar'}
        </Button>
        <Menu
          anchorEl={exportAnchor}
          open={Boolean(exportAnchor)}
          onClose={() => setExportAnchor(null)}
          slotProps={{ list: { 'aria-label': 'Formatos de exportação do dashboard' } }}
        >
          <MenuItem onClick={() => selectExport('pdf')}>
            <ListItemIcon><PictureAsPdfOutlinedIcon fontSize="small" /></ListItemIcon>
            Relatório PDF
          </MenuItem>
          <MenuItem onClick={() => selectExport('csv')}>
            <ListItemIcon><TableViewOutlinedIcon fontSize="small" /></ListItemIcon>
            CSV de campanhas
          </MenuItem>
        </Menu>

        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={onNewCampaign}
          sx={{ minHeight: 40, px: 2.25, whiteSpace: 'nowrap', textTransform: 'none', fontWeight: 700 }}
        >
          Nova Campanha
        </Button>
      </Stack>
    </Box>
  );
}
