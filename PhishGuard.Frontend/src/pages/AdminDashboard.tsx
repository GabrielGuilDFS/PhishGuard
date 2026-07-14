import { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  ThemeProvider,
  createTheme,
  CssBaseline,
  CircularProgress
} from '@mui/material';
import {
  PeopleAlt as PeopleIcon,
  Campaign as CampaignIcon,
  MailOutline as MailIcon,
  Security as SecurityIcon,
  WarningAmber as WarningIcon
} from '@mui/icons-material';

// Minimalist Light Theme with Gold Accents
const lightGoldTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#daa520', // Gold
    },
    background: {
      default: '#f4f6f8', // Pure white background
      paper: '#FFFFFF',
    },
    text: {
      primary: '#111827', // Dark slate for better contrast on white
      secondary: '#6b7280', // Medium gray for secondary text
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: '16px', // Slightly rounded as requested
          border: '1px solid #f3f4f6', // Very subtle border for minimalism
          boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.05), 0 2px 4px -2px rgb(0 0 0 / 0.05)', // Minimalist soft shadow
        }
      }
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderBottom: '1px solid #f3f4f6', // Clean table borders
        }
      }
    }
  }
});

const API_BASE = 'http://localhost:5000/api';

export default function AdminDashboard() {
  const [metrics, setMetrics] = useState({
    totalColaboradores: 0,
    campanhasAtivas: 0,
    emailsDisparados: 0,
    riscoGlobal: 0
  });
  const [departments, setDepartments] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      const token = localStorage.getItem('phishguard_token');
      const headers = { 
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      };

      try {
        const [metricsRes, deptRes] = await Promise.all([
          fetch(`${API_BASE}/Dashboard/metrics`, { headers }),
          fetch(`${API_BASE}/Dashboard/departments`, { headers })
        ]);

        if (metricsRes.ok && deptRes.ok) {
          const metricsData = await metricsRes.json();
          const deptData = await deptRes.json();
          setMetrics(metricsData);
          setDepartments(deptData);
        }
      } catch (err) {
        console.error("Erro ao buscar dados do dashboard", err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  const overviewCards = [
    { title: 'Total de Colaboradores', value: metrics.totalColaboradores.toLocaleString('pt-BR'), icon: PeopleIcon },
    { title: 'Campanhas Ativas', value: metrics.campanhasAtivas.toString(), icon: CampaignIcon },
    { title: 'E-mails Disparados', value: metrics.emailsDisparados.toLocaleString('pt-BR'), icon: MailIcon },
    { title: 'Risco Global', value: `${metrics.riscoGlobal}%`, icon: SecurityIcon },
  ];
  return (
    <ThemeProvider theme={lightGoldTheme}>
      <CssBaseline />
      <Box sx={{ p: { xs: 3, md: 6 }, minHeight: '100vh', bgcolor: 'background.default' }}>

        {/* Header Section */}
        <Box sx={{ mb: 6 }}>
          <Typography variant="h5" sx={{ fontWeight: 700, color: 'text.primary', mb: 0.5 }}>
            Dashboard Administrativo
          </Typography>
          <Typography variant="body2" sx={{ color: 'text.secondary' }}>
            Visão geral de cibersegurança e métricas de vulnerabilidade.
          </Typography>
        </Box>

        {/* 4 Horizontal Small Rounded Cards */}
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' },
            gap: 3,
            mb: 8
          }}
        >
                    {overviewCards.map((item, index) => (
            <Card key={index} elevation={0}>
              <CardContent sx={{ p: 3, '&:last-child': { pb: 3 }, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography
                    variant="caption"
                    sx={{
                      color: 'text.secondary',
                      fontWeight: 600,
                      textTransform: 'uppercase',
                      letterSpacing: '0.5px',
                      display: 'block',
                      mb: 0.5
                    }}
                  >
                    {item.title}
                  </Typography>
                  <Typography variant="h5" sx={{ fontWeight: 800, color: 'text.primary' }}>
                    {item.value}
                  </Typography>
                </Box>
                {/* Gold Accent Icon Background */}
                <Box sx={{ bgcolor: 'rgba(218, 165, 32, 0.1)', p: 1.5, borderRadius: '50%', display: 'flex' }}>
                  <item.icon sx={{ color: '#daa520' }} />
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* Minimalist Risk Table */}
        <Box sx={{ maxWidth: '900px' }}>
          <Card elevation={0}>
            <CardContent sx={{ p: { xs: 2, sm: 3 }, '&:last-child': { pb: 3 } }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <WarningIcon sx={{ color: '#daa520', mr: 1, fontSize: 22 }} />
                <Typography variant="h6" sx={{ fontWeight: 600, color: 'text.primary' }}>
                  Vulnerabilidade por Departamento
                </Typography>
              </Box>

              <TableContainer sx={{ bgcolor: 'transparent', borderRadius: 0 }}>
                <Table size="medium" aria-label="tabela de riscos minimalista">
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2 }}>
                        Departamento
                      </TableCell>
                      <TableCell align="right" sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2 }}>
                        Colaboradores
                      </TableCell>
                      <TableCell align="right" sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2 }}>
                        E-mails Recebidos
                      </TableCell>
                      <TableCell align="right" sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2 }}>
                        Cliques
                      </TableCell>
                      <TableCell align="right" sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2 }}>
                        Risco
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {loading ? (
                      <TableRow>
                        <TableCell colSpan={5} align="center">
                          <CircularProgress sx={{ my: 2 }} />
                        </TableCell>
                      </TableRow>
                    ) : departments.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={5} align="center">
                          <Typography variant="body2" sx={{ color: 'text.secondary', my: 2 }}>
                            Nenhum dado encontrado.
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ) : (
                      departments.map((row) => (
                        <TableRow
                          key={row.id}
                          sx={{ '&:hover': { bgcolor: '#f9fafb' }, transition: 'background-color 0.2s' }}
                        >
                          <TableCell component="th" scope="row" sx={{ fontWeight: 500, color: 'text.primary' }}>
                            {row.name}
                          </TableCell>
                          <TableCell align="right" sx={{ color: 'text.secondary' }}>{row.employees}</TableCell>
                          <TableCell align="right" sx={{ color: 'text.secondary' }}>{row.emails}</TableCell>
                          <TableCell align="right" sx={{ color: 'text.secondary' }}>{row.clicks}</TableCell>
                          {/* Gold Accent for Risk Percentage */}
                          <TableCell align="right" sx={{ fontWeight: 700, color: '#daa520' }}>
                            {row.risk}
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Box>

      </Box>
    </ThemeProvider>
  );
}
