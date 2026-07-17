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
  Skeleton
} from '@mui/material';
import {
  PeopleAlt as PeopleIcon,
  Campaign as CampaignIcon,
  MailOutline as MailIcon,
  Security as SecurityIcon,
  WarningAmber as WarningIcon,
  Insights as InsightsIcon
} from '@mui/icons-material';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RTooltip,
  ResponsiveContainer, Cell
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

interface Metrics {
  totalColaboradores: number;
  campanhasAtivas: number;
  emailsDisparados: number;
  riscoGlobal: number;
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
  // Tooltip = Surface 1 (`primary`), a surface de ÊNFASE da paleta: destaca a bolha
  // sobre o fundo neutro da página. O texto acompanha o `text` do modo — preto sobre
  // #6682f5 (5.6:1) e branco sobre #0a2799 (12:1), ambos acima do mínimo AA.
  const chartTooltipStyle = {
    backgroundColor: C.primary,
    border: `1px solid ${alpha(C.secondary, SOFT_BORDER_ALPHA)}`,
    borderRadius: 8,
    color: C.text,
  } as const;
  // Fills translúcidos do accent do modo (cursor de gráfico, bolha de ícone dos KPIs,
  // hover das linhas da tabela) — canais derivados do hex, sem valor repetido à mão.
  const accentRgb = rgbChannelsOf(C.accent);
  const chartCursorFill = { fill: `rgba(${accentRgb}, 0.10)` };
  const iconBubbleBg = `rgba(${accentRgb}, 0.14)`;
  const rowHoverBg = `rgba(${accentRgb}, 0.08)`;

  const [metrics, setMetrics] = useState<Metrics>({
    totalColaboradores: 0,
    campanhasAtivas: 0,
    emailsDisparados: 0,
    riscoGlobal: 0
  });
  const [departments, setDepartments] = useState<DepartmentRow[]>([]);
  const [funnel, setFunnel] = useState<Funnel | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      const token = localStorage.getItem('phishguard_token');
      const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      };

      try {
        const [metricsRes, deptRes, funnelRes] = await Promise.all([
          fetch(`${API_BASE}/Dashboard/metrics`, { headers }),
          fetch(`${API_BASE}/Dashboard/departments`, { headers }),
          fetch(`${API_BASE}/Dashboard/funnel`, { headers })
        ]);

        if (metricsRes.ok) setMetrics(await metricsRes.json());
        if (deptRes.ok) setDepartments(await deptRes.json());
        if (funnelRes.ok) setFunnel(await funnelRes.json());
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

  // Sem NENHUM disparo ainda → não há métrica de comportamento para plotar.
  const semDisparos = (funnel?.disparosFeitos ?? 0) === 0;

  // Dados derivados dos gráficos.
  const dadosFunil = funnel
    ? [
      { nome: 'Disparos Feitos', valor: funnel.disparosFeitos, cor: ACCENT },
      { nome: 'Entregues (SMTP)', valor: funnel.entregues, cor: SUCCESS },
    ]
    : [];
  // Ambos são falhas críticas (vermelho); a intensidade distingue a severidade.
  const dadosRisco = funnel
    ? [
      { nome: 'Cliques no Link', valor: funnel.cliques, cor: DANGER_SOFT },
      { nome: 'Submeteram Dados', valor: funnel.submissoes, cor: DANGER },
    ]
    : [];
  const dadosDepto = departments
    .map((d) => ({
      nome: d.name,
      taxa: d.emails > 0 ? Math.round((100 * d.clicks) / d.emails) : 0,
    }))
    .sort((a, b) => b.taxa - a.taxa);

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
                <RTooltip cursor={chartCursorFill} contentStyle={chartTooltipStyle} />
                <Bar dataKey="valor" name="E-mails" radius={[6, 6, 0, 0]} maxBarSize={90}>
                  {dadosFunil.map((d, i) => <Cell key={i} fill={d.cor} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>

          {/* 2. Gráfico de Comportamento de Risco (cliques vs submissões) */}
          <ChartCard
            title="Comportamento de Risco"
            subtitle="Alvos que clicaram na isca vs. os que chegaram a inserir credenciais."
          >
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={dadosRisco} margin={{ top: 8, right: 16, left: -12, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke={chartGridStroke} vertical={false} />
                <XAxis dataKey="nome" tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                <YAxis allowDecimals={false} tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                <RTooltip cursor={chartCursorFill} contentStyle={chartTooltipStyle} />
                <Bar dataKey="valor" name="Alvos" radius={[6, 6, 0, 0]} maxBarSize={90}>
                  {dadosRisco.map((d, i) => <Cell key={i} fill={d.cor} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>

          {/* 3. Risco por Departamento (taxa de cliques / envios) */}
          <ChartCard
            title="Risco por Departamento"
            subtitle="Taxa de vulnerabilidade (cliques ÷ e-mails recebidos) por setor."
            full
          >
            {dadosDepto.length === 0 ? (
              <EmptyInline texto="Sem departamentos com dados de simulação ainda." />
            ) : (
              <>
              <ResponsiveContainer width="100%" height={Math.max(220, dadosDepto.length * 48)}>
                <BarChart
                  layout="vertical"
                  data={dadosDepto}
                  /* left:0 — o vão à esquerda era, na verdade, o `width={140}` do YAxis
                     (calha larga demais para rótulos curtos como RH/TI). Zeramos a margem
                     e enxugamos a calha; os rótulos passam a ser ancorados à ESQUERDA
                     (x=0), alinhados com o início do título do card. */
                  margin={{ top: 8, right: 32, left: -40, bottom: 0 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke={chartGridStroke} horizontal={false} />
                  <XAxis type="number" domain={[0, 100]} unit="%" tick={chartTick} axisLine={{ stroke: chartAxisStroke }} tickLine={false} />
                  <YAxis
                    type="category"
                    dataKey="nome"
                    width={88}
                    tickLine={false}
                    axisLine={{ stroke: chartAxisStroke }}
                    tick={(props) => {
                      const { y, payload } = props;
                      return (
                        <text x={10} y={y} dy={4} textAnchor="start" fontSize={12} fill={C.text}>
                          {payload.value}
                        </text>
                      );
                    }}
                  />
                  <RTooltip cursor={chartCursorFill} contentStyle={chartTooltipStyle} formatter={(value) => [`${value}%`, 'Taxa de risco']} />
                  {/* Semáforo semântico: ≥50% falha crítica, ≥25% alerta, abaixo = setor resistindo bem. */}
                  <Bar dataKey="taxa" name="Taxa de risco" radius={[0, 6, 6, 0]} maxBarSize={28}>
                    {dadosDepto.map((d, i) => (
                      <Cell key={i} fill={d.taxa >= 50 ? DANGER : d.taxa >= 25 ? WARNING : SUCCESS} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
              {/* Legenda do semáforo FORA do SVG do Recharts: o <Legend> desta versão omite
                  a prop `payload` no tsc -b. Aqui usa tokens do tema (text.secondary) →
                  texto legível nos dois modos (nunca preto no dark) + swatches de status. */}
              <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap', mt: 1.5 }}>
                {[{ l: 'Alto (≥ 50%)', c: DANGER }, { l: 'Médio (25–49%)', c: WARNING }, { l: 'Baixo (< 25%)', c: SUCCESS }].map((it) => (
                  <Box key={it.l} sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                    <Box sx={{ width: 10, height: 10, borderRadius: '2px', bgcolor: it.c, flexShrink: 0 }} />
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>{it.l}</Typography>
                  </Box>
                ))}
              </Box>
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
