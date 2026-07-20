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
  Skeleton,
  ToggleButton,
  ToggleButtonGroup
} from '@mui/material';
import {
  Campaign as CampaignIcon,
  WarningAmber as WarningIcon,
  Insights as InsightsIcon,
  TouchApp as TouchAppIcon,
  QueryStats as QueryStatsIcon
} from '@mui/icons-material';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RTooltip,
  ResponsiveContainer, Cell, LineChart, Line, Legend
} from 'recharts';
import { alpha } from '@mui/material/styles';
import PageContainer from '../components/PageContainer';
import { brandPalette, statusColors, mutedTextFor, rgbChannelsOf, SOFT_BORDER_ALPHA } from '../theme';
import { useThemeMode } from '../context/ThemeModeContext';

const API_BASE = 'http://localhost:5000/api';

// Cores semânticas de status — papéis EXCLUSIVOS e iguais nos dois modos:
// danger = falha crítica (clicou/inseriu dados), warning = alerta/pendência,
// success = entregue/resistiu.
const SUCCESS = statusColors.success;
const WARNING = statusColors.warning;
const DANGER = statusColors.danger;
// Falha crítica em 2 intensidades (mesma família rose): clique < submissão de dados.
const DANGER_SOFT = '#fb7185'; // rose-400

// row.risk chega como percentual em string (ex.: "37%") — mesmas faixas do
// semáforo do gráfico de departamentos: ≥50% danger, ≥25% warning, senão success.
function corDoRisco(risk: string): string {
  const pct = parseInt(risk, 10);
  if (Number.isNaN(pct)) return WARNING;
  return pct >= 50 ? DANGER : pct >= 25 ? WARNING : SUCCESS;
}

// Paleta de cores harmoniosa para até 8 departamentos nas linhas do gráfico temporal.
const DEPT_COLORS = ['#6366f1', '#f59e0b', '#10b981', '#ef4444', '#8b5cf6', '#06b6d4', '#ec4899', '#84cc16'];

interface DeptRiskTimeline {
  departamentos: string[];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  timeline: Record<string, any>[];
}

interface Metrics {
  // KPIs consolidados exibidos no topo.
  totalCampanhas: number;
  cliquesUnicosAcumulados: number;
  volumeBrutoInteracoes: number;
}
// Ponto do gráfico de barras por campanha (rótulo "Nome (Mês)" + cliques únicos).
interface CampaignClicks {
  label: string;
  clicks: number;
}
interface DepartmentRow {
  id: string;
  name: string;
  employees: number;
  emails: number;
  clicks: number;
  risk: string;
}
interface Funnel {
  disparosFeitos: number;
  entregues: number;
  falhas: number;
  cliques: number;
  submissoes: number;
}

export default function AdminDashboard() {
  // Os gráficos Recharts não leem o tema do MUI — puxamos as cores da mesma paleta
  // central, no modo ATIVO, para acompanharem o toggle dark/light.
  const { mode } = useThemeMode();
  const C = brandPalette[mode];
  const ACCENT = C.accent;
  // Recharts é SVG puro — não lê o tema do MUI. Grade e eixos usam um NEUTRO derivado do
  // TEXTO do modo (branco no dark, preto no light) em baixa opacidade: sutil, porém visível
  // sobre o grafite fosco (#0a0a0a). O antigo alpha(C.secondary) dava #05134d a 0.35 no dark
  // — azul-quase-preto sobre preto = invisível, e o usuário perdia a referência espacial
  // (pior com o gráfico zerado). O eixo usa a mesma cor um pouco mais forte que a grade, para
  // ancorar a base sem poluir. Ver rgbChannelsOf em themeHelper.ts.
  const textRgb = rgbChannelsOf(C.text);
  // Opacidade AJUSTADA POR MODO: a mesma alpha "pesa" menos sobre fundo claro do que
  // sobre o grafite (#0a0a0a), então o preto a 0.10 sumia no light — o light recebe um
  // pouco mais. Eixo = grade reforçada, para ancorar a base sem poluir.
  const gridAlpha = mode === 'dark' ? 0.16 : 0.22;
  const axisAlpha = mode === 'dark' ? 0.34 : 0.40;
  const chartGridStroke = `rgba(${textRgb}, ${gridAlpha})`;
  const chartAxisStroke = `rgba(${textRgb}, ${axisAlpha})`;
  const chartTick = { fontSize: 12, fill: mutedTextFor(mode) };
  // Tooltip = Fundo cinza e texto preto para alta legibilidade (conforme solicitação)
  const chartTooltipStyle = {
    backgroundColor: '#f3f4f6', // cinza claro
    border: `1px solid ${alpha(C.secondary, SOFT_BORDER_ALPHA)}`,
    borderRadius: 8,
  } as const;
  const chartTooltipLabelStyle = { color: '#000000', fontWeight: 600 } as const;
  const chartTooltipItemStyle = { color: '#000000' } as const;
  // Fills translúcidos do accent do modo (cursor de gráfico, bolha de ícone dos KPIs,
  // hover das linhas da tabela) — canais derivados do hex, sem valor repetido à mão.
  const accentRgb = rgbChannelsOf(C.accent);
  const chartCursorFill = { fill: `rgba(${accentRgb}, 0.10)` };
  const iconBubbleBg = `rgba(${accentRgb}, 0.14)`;
  const rowHoverBg = `rgba(${accentRgb}, 0.08)`;

  const [metrics, setMetrics] = useState<Metrics>({
    totalCampanhas: 0,
    cliquesUnicosAcumulados: 0,
    volumeBrutoInteracoes: 0
  });
  const [departments, setDepartments] = useState<DepartmentRow[]>([]);
  const [funnel, setFunnel] = useState<Funnel | null>(null);
  const [campaignClicks, setCampaignClicks] = useState<CampaignClicks[]>([]);
  const [deptTimeline, setDeptTimeline] = useState<DeptRiskTimeline | null>(null);
  const [riskMetric, setRiskMetric] = useState<'cliques' | 'submissoes'>('cliques');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      const token = localStorage.getItem('phishguard_token');
      const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      };

      try {
        const [metricsRes, deptRes, funnelRes, campaignRes, timelineRes] = await Promise.all([
          fetch(`${API_BASE}/Dashboard/metrics`, { headers }),
          fetch(`${API_BASE}/Dashboard/departments`, { headers }),
          fetch(`${API_BASE}/Dashboard/funnel`, { headers }),
          fetch(`${API_BASE}/Dashboard/campaign-clicks`, { headers }),
          fetch(`${API_BASE}/Dashboard/dept-risk-timeline`, { headers })
        ]);

        if (metricsRes.ok) setMetrics(await metricsRes.json());
        if (deptRes.ok) setDepartments(await deptRes.json());
        if (funnelRes.ok) setFunnel(await funnelRes.json());
        if (campaignRes.ok) setCampaignClicks(await campaignRes.json());
        if (timelineRes.ok) setDeptTimeline(await timelineRes.json());
      } catch (err) {
        console.error("Erro ao buscar dados do dashboard", err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  // 3 KPIs consolidados: campanhas disparadas, vulnerabilidades reais (cliques únicos)
  // e o volume bruto de telemetria (para auditoria de rede).
  const overviewCards = [
    { title: 'Total de Campanhas', value: metrics.totalCampanhas.toLocaleString('pt-BR'), icon: CampaignIcon },
    { title: 'Cliques Únicos Acumulados', value: metrics.cliquesUnicosAcumulados.toLocaleString('pt-BR'), icon: TouchAppIcon },
    { title: 'Volume Bruto de Interações', value: metrics.volumeBrutoInteracoes.toLocaleString('pt-BR'), icon: QueryStatsIcon },
  ];

  // Sem NENHUM disparo ainda → não há métrica de comportamento para plotar.
  const semDisparos = (funnel?.disparosFeitos ?? 0) === 0;

  // Dados derivados dos gráficos.
  const dadosFunil = funnel
    ? [
      { nome: 'Disparos Feitos', valor: funnel.disparosFeitos, cor: ACCENT },
      { nome: 'Entregues (SMTP)', valor: funnel.entregues, cor: SUCCESS },
    ]
    : [];


  return (
    <PageContainer sx={{ py: { xs: 1, md: 2 } }}>

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
          // Cada card tem largura mínima de 190px (> 170px, limite de segurança contra a
          // sobreposição ícone×título) e o grid QUEBRA (auto-fit) em vez de espremer os
          // cards — resolve inclusive o md, onde o drawer de 260px comprimia 4 colunas.
          gridTemplateColumns: 'repeat(auto-fit, minmax(190px, 1fr))',
          gap: 3,
          mb: 6
        }}
      >
        {overviewCards.map((item, index) => (
          <Card key={index} elevation={0}>
            {/* Anti-sobreposição: empilha (ícone acima) em telas estreitas e volta ao
                  layout horizontal no md+. gap garante respiro; o ícone tem flexShrink:0
                  e tamanho reduzido para nunca colidir com o título. */}
            <CardContent
              sx={{
                p: 3, '&:last-child': { pb: 3 },
                display: 'flex',
                flexDirection: { xs: 'column', md: 'row' },
                alignItems: { xs: 'flex-start', md: 'center' },
                justifyContent: 'space-between',
                gap: 2
              }}
            >
              <Box sx={{ minWidth: 0, order: { xs: 2, md: 1 } }}>
                <Typography
                  variant="caption"
                  sx={{
                    color: 'text.secondary', fontWeight: 600, textTransform: 'uppercase',
                    letterSpacing: '0.5px', display: 'block', mb: 0.5
                  }}
                >
                  {item.title}
                </Typography>
                {loading ? (
                  <Skeleton variant="text" width={64} height={36} />
                ) : (
                  <Typography variant="h5" sx={{ fontWeight: 800, color: 'text.primary', lineHeight: 1.2 }}>
                    {item.value}
                  </Typography>
                )}
              </Box>
              <Box sx={{
                bgcolor: iconBubbleBg, p: 1.25, borderRadius: '50%',
                display: 'flex', flexShrink: 0, order: { xs: 1, md: 2 }
              }}>
                <item.icon sx={{ color: ACCENT, fontSize: 20 }} />
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>

      {/* Charts Section */}
      {loading ? (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' }, gap: 3, mb: 6 }}>
          {[0, 1].map((i) => (
            <Card key={i} elevation={0}>
              <CardContent sx={{ p: 3 }}>
                <Skeleton variant="text" width="40%" height={28} />
                <Skeleton variant="rounded" height={260} sx={{ mt: 2, borderRadius: 2 }} />
              </CardContent>
            </Card>
          ))}
        </Box>
      ) : semDisparos ? (
        <Card elevation={0} sx={{ mb: 6 }}>
          <CardContent sx={{ p: 6, textAlign: 'center' }}>
            <InsightsIcon sx={{ fontSize: 48, color: `rgba(${accentRgb}, 0.55)`, mb: 2 }} />
            <Typography variant="h6" sx={{ fontWeight: 600, color: 'text.primary', mb: 1 }}>
              Nenhuma campanha rodando ainda
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', maxWidth: 460, mx: 'auto' }}>
              Assim que você ativar uma campanha e os e-mails começarem a ser disparados, os gráficos de
              entregabilidade e comportamento de risco aparecerão aqui.
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' }, gap: 3, mb: 6 }}>
          {/* 1. Funil de Conversão de Disparos (entregabilidade SMTP) */}
          <ChartCard
            title="Funil de Entregabilidade (SMTP)"
            subtitle="Disparos feitos vs. efetivamente entregues pelo servidor de e-mail."
          >
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={dadosFunil} margin={{ top: 8, right: 16, left: -12, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke={chartGridStroke} vertical={false} />
                <XAxis dataKey="nome" tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                <YAxis allowDecimals={false} tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                <RTooltip cursor={chartCursorFill} contentStyle={chartTooltipStyle} labelStyle={chartTooltipLabelStyle} itemStyle={chartTooltipItemStyle} />
                <Bar dataKey="valor" name="E-mails" radius={[6, 6, 0, 0]} maxBarSize={90}>
                  {dadosFunil.map((d, i) => <Cell key={i} fill={d.cor} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>

          {/* 2. Cliques únicos por campanha (barras verticais; X = "Nome | Mês") */}
          <ChartCard
            title="Cliques Únicos por Campanha"
            subtitle="Vulnerabilidades reais por campanha (1 clique por alvo), com o mês de disparo."
          >
            {campaignClicks.length === 0 ? (
              <EmptyInline texto="Nenhuma campanha disparada com dados de clique ainda." />
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={campaignClicks} margin={{ top: 8, right: 16, left: -12, bottom: 44 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke={chartGridStroke} vertical={false} />
                  <XAxis
                    dataKey="label"
                    tick={chartTick}
                    axisLine={{ stroke: chartAxisStroke }}
                    tickLine={false}
                    interval={0}
                    angle={-20}
                    textAnchor="end"
                    height={64}
                  />
                  <YAxis allowDecimals={false} tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                  <RTooltip cursor={chartCursorFill} contentStyle={chartTooltipStyle} labelStyle={chartTooltipLabelStyle} itemStyle={chartTooltipItemStyle} formatter={(value) => [value, 'Cliques únicos']} />
                  <Bar dataKey="clicks" name="Cliques únicos" fill={DANGER_SOFT} radius={[6, 6, 0, 0]} maxBarSize={64} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>

          {/* 3. Evolução Temporal de Risco por Departamento (LineChart + Toggle) */}
          <ChartCard
            title="Evolução de Risco por Departamento"
            subtitle="Acompanhe a taxa de cliques ou submissões de cada setor ao longo dos meses."
            full
          >
            {!deptTimeline || deptTimeline.timeline.length === 0 ? (
              <EmptyInline texto="Sem dados temporais de risco por departamento ainda." />
            ) : (
              <>
              {/* Toggle: Taxa de Cliques vs Taxa de Submissões */}
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
                <ToggleButtonGroup
                  value={riskMetric}
                  exclusive
                  onChange={(_, v) => { if (v) setRiskMetric(v); }}
                  size="small"
                  sx={{
                    '& .MuiToggleButton-root': {
                      textTransform: 'none',
                      fontWeight: 600,
                      fontSize: 13,
                      px: 2.5,
                      py: 0.75,
                      color: 'text.secondary',
                      borderColor: `rgba(${accentRgb}, 0.25)`,
                      '&.Mui-selected': {
                        bgcolor: `rgba(${accentRgb}, 0.14)`,
                        color: C.accent,
                        borderColor: `rgba(${accentRgb}, 0.4)`,
                      },
                    },
                  }}
                >
                  <ToggleButton value="cliques">Taxa de Cliques</ToggleButton>
                  <ToggleButton value="submissoes">Taxa de Submissões</ToggleButton>
                </ToggleButtonGroup>
              </Box>

              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={deptTimeline.timeline} margin={{ top: 8, right: 24, left: -8, bottom: 8 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke={chartGridStroke} />
                  <XAxis
                    dataKey="mes"
                    tick={chartTick}
                    axisLine={{ stroke: chartAxisStroke }}
                    tickLine={false}
                  />
                  <YAxis
                    domain={[0, 100]}
                    unit="%"
                    allowDecimals={false}
                    tick={chartTick}
                    axisLine={{ stroke: chartAxisStroke }}
                    tickLine={false}
                  />
                  <RTooltip
                    cursor={chartCursorFill}
                    contentStyle={chartTooltipStyle}
                    labelStyle={chartTooltipLabelStyle}
                    itemStyle={chartTooltipItemStyle}
                    formatter={(value) => [`${value}%`]}
                  />
                  <Legend
                    verticalAlign="bottom"
                    iconType="circle"
                    wrapperStyle={{ fontSize: 12, color: mutedTextFor(mode), paddingTop: 8 }}
                  />
                  {deptTimeline.departamentos.map((dept, i) => (
                    <Line
                      key={`${dept}_${riskMetric}`}
                      type="monotone"
                      dataKey={`${dept}_${riskMetric}`}
                      name={dept}
                      stroke={DEPT_COLORS[i % DEPT_COLORS.length]}
                      strokeWidth={2.5}
                      dot={{ r: 4, strokeWidth: 2 }}
                      activeDot={{ r: 6, strokeWidth: 2 }}
                      animationDuration={400}
                      connectNulls
                    />
                  ))}
                </LineChart>
              </ResponsiveContainer>
              </>
            )}
          </ChartCard>
        </Box>
      )}

      {/* Risk Table — dados centralizados horizontal e verticalmente nas células */}
      <Box>
        <Card elevation={0}>
          <CardContent sx={{ p: { xs: 2, sm: 3 }, '&:last-child': { pb: 3 } }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 3 }}>
              <WarningIcon sx={{ color: WARNING, mr: 1, fontSize: 22 }} />
              <Typography variant="h6" sx={{ fontWeight: 600, color: 'text.primary' }}>
                Vulnerabilidade por Departamento
              </Typography>
            </Box>

            <TableContainer sx={{ bgcolor: 'transparent', borderRadius: 0 }}>
              <Table size="medium" aria-label="tabela de riscos minimalista">
                <TableHead>
                  <TableRow>
                    {['Departamento', 'Colaboradores', 'E-mails Recebidos', 'Cliques', 'Risco'].map((h) => (
                      <TableCell key={h} align="center" sx={{ color: 'text.secondary', fontWeight: 600, borderBottomWidth: 2, verticalAlign: 'middle' }}>
                        {h}
                      </TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {loading ? (
                    Array.from({ length: 3 }).map((_, i) => (
                      <TableRow key={i}>
                        <TableCell colSpan={5} align="center"><Skeleton variant="text" height={28} /></TableCell>
                      </TableRow>
                    ))
                  ) : departments.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography variant="body2" sx={{ color: 'text.secondary', my: 2 }}>
                          Nenhum departamento cadastrado ainda.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    departments.map((row) => (
                      <TableRow key={row.id} sx={{ '&:hover': { bgcolor: rowHoverBg }, transition: 'background-color 0.2s' }}>
                        <TableCell component="th" scope="row" align="center" sx={{ fontWeight: 500, color: 'text.primary', verticalAlign: 'middle' }}>{row.name}</TableCell>
                        <TableCell align="center" sx={{ color: 'text.secondary', verticalAlign: 'middle' }}>{row.employees}</TableCell>
                        <TableCell align="center" sx={{ color: 'text.secondary', verticalAlign: 'middle' }}>{row.emails}</TableCell>
                        <TableCell align="center" sx={{ color: 'text.secondary', verticalAlign: 'middle' }}>{row.clicks}</TableCell>
                        <TableCell align="center" sx={{ fontWeight: 700, color: corDoRisco(row.risk), verticalAlign: 'middle' }}>{row.risk}</TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Box>

    </PageContainer>
  );
}

// Cartão-container padrão de um gráfico (título + subtítulo + área do gráfico).
function ChartCard({ title, subtitle, children, full }: {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  full?: boolean;
}) {
  return (
    <Card elevation={0} sx={{ gridColumn: full ? { xs: 'auto', md: '1 / -1' } : 'auto' }}>
      <CardContent sx={{ p: 3, '&:last-child': { pb: 3 } }}>
        <Typography variant="subtitle1" sx={{ fontWeight: 700, color: 'text.primary' }}>{title}</Typography>
        <Typography variant="caption" sx={{ color: 'text.secondary', display: 'block', mb: 2 }}>{subtitle}</Typography>
        {children}
      </CardContent>
    </Card>
  );
}

function EmptyInline({ texto }: { texto: string }) {
  return (
    <Box sx={{ py: 6, textAlign: 'center' }}>
      <Typography variant="body2" sx={{ color: 'text.secondary' }}>{texto}</Typography>
    </Box>
  );
}
