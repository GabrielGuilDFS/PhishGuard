import { type ReactNode } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Box,
  Button,
  Container,
  Typography,
  Paper,
  Stack,
  Chip,
  Divider,
  Link,
} from '@mui/material';
import {
  Send as SendIcon,
  Web as WebIcon,
  School as SchoolIcon,
  Insights as InsightsIcon,
  CheckCircle as CheckCircleIcon,
  Security as SecurityIcon,
  ArrowForward as ArrowForwardIcon,
} from '@mui/icons-material';

// Identidade visual: paleta clara e corporativa alinhada ao painel interno.
// O dourado (#DAA520) é a cor primária do tema (ver App.tsx / AdminLayout),
// reservado para pontos estratégicos de conversão e destaque.
const GOLD = '#DAA520';
const SILVER = '#878787ff';
const BRONZE = '#CD7F32';
const GOLD_HOVER = '#c6941c';
const FOOTER_BG = "#e1e1e1ff"
const PAGE_BG = '#F4F5F7'; // cinza bem suave para o fundo geral
const SURFACE = '#F4F5F7'; // cards e superfícies elevadas
const BORDER = '#c5c5c5ff'; // borda fina e discreta
const GOLD_BORDER = 'rgba(218,165,32,0.35)'; // borda dourada sutil (destaques)
const TEXT_DARK = '#0F172A'; // títulos e textos de alta legibilidade
const TEXT_MUTED = '#475569'; // textos de apoio / subtítulos
const CARD_SHADOW = '0 1px 3px rgba(15,23,42,0.08), 0 4px 12px rgba(15,23,42,0.05)';

interface Recurso {
  icon: ReactNode;
  titulo: string;
  descricao: string;
}

const recursos: Recurso[] = [
  {
    icon: <SendIcon sx={{ fontSize: 34 }} />,
    titulo: 'Disparos de E-mails Simulados',
    descricao:
      'Um catálogo pronto de cenários que imitam marcas reais como Netflix e Amazon. Programe campanhas e meça quem realmente cai no anzol — sem risco para a operação.',
  },
  {
    icon: <WebIcon sx={{ fontSize: 34 }} />,
    titulo: 'Páginas de Captura Realistas',
    descricao:
      'Clones fiéis de telas de login, incluindo cenários como o da HBO Max. Capturamos a interação do colaborador de forma segura para dimensionar a exposição da empresa.',
  },
  {
    icon: <SchoolIcon sx={{ fontSize: 34 }} />,
    titulo: 'Feedback Educacional Imediato',
    descricao:
      'O "Momento Ensinável": ao cair na simulação, o colaborador é direcionado na hora para um treinamento pedagógico com o template básico de phishing. Erro vira aprendizado.',
  },
  {
    icon: <InsightsIcon sx={{ fontSize: 34 }} />,
    titulo: 'Relatórios de Vulnerabilidade',
    descricao:
      'Métricas de vulnerabilidade organizacional para gestores: taxas de clique, dados inseridos e evolução da maturidade em segurança ao longo das campanhas.',
  },
];

interface Plano {
  id: string;
  nome: string;
  selo: string;
  preco: string;
  periodo: string;
  publico: string;
  recursos: string[];
  cta: string;
  destaque?: boolean;
  cor: string;
}

const planos: Plano[] = [
  {
    id: 'bronze',
    nome: 'Inicial',
    selo: 'Bronze',
    preco: 'R$ 149',
    periodo: '/mês',
    publico: 'Pequenas empresas começando a estruturar a cultura de segurança.',
    recursos: [
      'Até 50 alvos simulados',
      'Templates de e-mail padrão',
      'Disparos de campanhas simuladas',
      'Relatório básico de cliques',
    ],
    cta: 'Escolher Plano',
    cor: BRONZE,
  },
  {
    id: 'prata',
    nome: 'Profissional',
    selo: 'Prata',
    preco: 'R$ 590',
    periodo: '/mês',
    publico: 'Médias empresas que precisam de treinamento contínuo e mensurável.',
    recursos: [
      'Até 500 alvos simulados',
      'Suporte a landing pages falsas',
      'Feedback educacional automático',
      'Relatórios avançados e métricas',
      'Catálogo completo de cenários',
    ],
    cta: 'Escolher Plano',
    destaque: true,
    cor: SILVER,
  },
  {
    id: 'ouro',
    nome: 'Enterprise',
    selo: 'Ouro',
    preco: 'Sob consulta',
    periodo: '',
    publico: 'Grandes operações com simulações customizadas em larga escala.',
    recursos: [
      'Alvos ilimitados',
      'Cenários e landing pages sob medida',
      'Integrações e SSO corporativo',
      'Gerente de sucesso dedicado',
      'SLA e suporte prioritário',
    ],
    cta: 'Falar com Consultor',
    cor: GOLD,
  },
];

export default function HomeLandingPage() {
  const navigate = useNavigate();

  // Inicia o fluxo de onboarding do novo Tenant (cadastro da empresa).
  const iniciarOnboarding = (planoId: string) => {
    navigate(`/register?plano=${planoId}`);
  };

  const irParaConsultor = () => {
    navigate('/register?plano=ouro&contato=consultor');
  };

  const scrollPara = (id: string) => {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
  };

  return (
    <Box sx={{ backgroundColor: PAGE_BG, color: TEXT_DARK, minHeight: '100vh' }}>
      {/* ---------- NAVBAR PÚBLICA ---------- */}
      <AppBar
        position="sticky"
        elevation={0}
        sx={{
          backgroundColor: '#c6941c',
          backdropFilter: 'blur(8px)',
          borderBottom: `1px solid ${BORDER}`,
        }}
      >
        <Container maxWidth="lg">
          <Toolbar>
            <SecurityIcon sx={{ mr: 1 }} />
            <Typography
              variant="h6"
              sx={{ fontWeight: 800, letterSpacing: 0.5, flexGrow: 1, color: TEXT_DARK }}
            >
              <span>PhishGuard</span>
            </Typography>

            <Stack
              direction="row"
              spacing={3}
              sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center', mr: 2 }}
            >
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('recursos')}
                sx={{ color: 'Black', fontWeight: 500, '&:hover': { color: '#000000' } }}
              >
                Recursos
              </Link>
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('precos')}
                sx={{ color: 'Black', fontWeight: 500, '&:hover': { color: GOLD } }}
              >
                Preços
              </Link>
            </Stack>

            <Button
              component={RouterLink}
              to="/login"
              variant="outlined"
              sx={{
                color: 'black',
                border: 'none',
              }}
            >
              Login
            </Button>
          </Toolbar>
        </Container>
      </AppBar>

      {/* ---------- HERO ---------- */}
      <Box
        sx={{
          position: 'relative',
          overflow: 'hidden',
          background: `radial-gradient(1200px 500px at 50% -10%, rgba(218,165,32,0.16), transparent 60%), ${SURFACE}`,
        }}
      >
        <Container maxWidth="md" sx={{ textAlign: 'center', py: { xs: 8, md: 14 } }}>
          <Chip
            icon={<SecurityIcon sx={{ color: `${GOLD} !important` }} />}
            label="Conscientização em Segurança da Informação"
            sx={{
              mb: 3,
              color: '#8a6d13',
              backgroundColor: 'rgba(218,165,32,0.12)',
              border: `1px solid ${GOLD_BORDER}`,
              fontWeight: 600,
            }}
          />
          <Typography
            variant="h2"
            sx={{
              fontWeight: 900,
              lineHeight: 1.1,
              fontSize: { xs: '2.2rem', md: '3.4rem' },
              mb: 3,
            }}
          >
            Transforme a cultura de segurança da sua empresa contra ataques de{' '}
            <Box component="span" sx={{ color: GOLD }}>
              Phishing
            </Box>
          </Typography>
          <Typography
            variant="h6"
            sx={{
              color: TEXT_MUTED,
              fontWeight: 400,
              maxWidth: 720,
              mx: 'auto',
              mb: 5,
              fontSize: { xs: '1rem', md: '1.2rem' },
            }}
          >
            O PhishGuard automatiza simulações de phishing e treinamentos pedagógicos para os seus
            colaboradores. Identifique vulnerabilidades reais e transforme cada erro em um momento
            de aprendizado — de forma segura, contínua e mensurável.
          </Typography>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
            <Button
              variant="contained"
              size="large"
              endIcon={<ArrowForwardIcon />}
              onClick={() => scrollPara('precos')}
              sx={{
                backgroundColor: GOLD,
                color: '#111',
                fontWeight: 700,
                px: 4,
                py: 1.5,
                boxShadow: '0 4px 14px rgba(218,165,32,0.4)',
                '&:hover': { backgroundColor: GOLD_HOVER },
              }}
            >
              Começar Agora
            </Button>
            <Button
              variant="outlined"
              size="large"
              onClick={() => navigate('/register?intencao=demonstracao')}
              sx={{
                color: TEXT_DARK,
                borderColor: BORDER,
                backgroundColor: SURFACE,
                fontWeight: 700,
                px: 4,
                py: 1.5,
                '&:hover': { borderColor: GOLD, backgroundColor: 'rgba(218,165,32,0.08)' },
              }}
            >
              Agendar Demonstração
            </Button>
          </Stack>
        </Container>
      </Box>

      {/* ---------- PROBLEMÁTICAS E RECURSOS ---------- */}
      <Container id="recursos" maxWidth="lg" sx={{ py: { xs: 8, md: 12 }, scrollMarginTop: 80 }}>
        <Box sx={{ textAlign: 'center', mb: 6 }}>
          <Typography variant="overline" sx={{ color: GOLD, fontWeight: 700, letterSpacing: 2 }}>
            O problema é humano — a defesa também
          </Typography>
          <Typography variant="h3" sx={{ fontWeight: 800, mt: 1, fontSize: { xs: '1.8rem', md: '2.6rem' } }}>
            Engenharia social não se resolve só com firewall
          </Typography>
          <Typography variant="body1" sx={{ color: TEXT_MUTED, maxWidth: 680, mx: 'auto', mt: 2 }}>
            A maioria dos incidentes começa com um clique. O PhishGuard trata a causa raiz treinando
            pessoas com simulações realistas e feedback imediato.
          </Typography>
        </Box>

        <Box
          sx={{
            display: 'grid',
            gap: 3,
            gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
          }}
        >
          {recursos.map((r) => (
            <Paper
              key={r.titulo}
              elevation={0}
              sx={{
                p: 4,
                height: '100%',
                backgroundColor: SURFACE,
                border: `1px solid ${BORDER}`,
                borderRadius: 3,
                boxShadow: CARD_SHADOW,
                transition: 'transform .2s, border-color .2s, box-shadow .2s',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  borderColor: GOLD_BORDER,
                  boxShadow: '0 10px 24px rgba(15,23,42,0.1)',
                },
              }}
            >
              <Box
                sx={{
                  width: 60,
                  height: 60,
                  borderRadius: 2,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: GOLD,
                  backgroundColor: 'rgba(218,165,32,0.12)',
                  mb: 2.5,
                }}
              >
                {r.icon}
              </Box>
              <Typography variant="h6" sx={{ fontWeight: 700, color: TEXT_DARK, mb: 1 }}>
                {r.titulo}
              </Typography>
              <Typography variant="body2" sx={{ color: TEXT_MUTED, lineHeight: 1.7 }}>
                {r.descricao}
              </Typography>
            </Paper>
          ))}
        </Box>
      </Container>

      {/* ---------- PRICING ---------- */}
      <Box id="precos" sx={{ backgroundColor: SURFACE, py: { xs: 8, md: 12 }, scrollMarginTop: 80 }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="overline" sx={{ color: GOLD, fontWeight: 700, letterSpacing: 2 }}>
              Planos e Preços
            </Typography>
            <Typography variant="h3" sx={{ fontWeight: 800, mt: 1, fontSize: { xs: '1.8rem', md: '2.6rem' } }}>
              Escolha o plano ideal para a sua operação
            </Typography>
            <Typography variant="body1" sx={{ color: TEXT_MUTED, maxWidth: 620, mx: 'auto', mt: 2 }}>
              Comece pequeno e escale conforme a maturidade de segurança da sua empresa cresce.
            </Typography>
          </Box>

          <Box
            sx={{
              display: 'grid',
              gap: 4,
              alignItems: 'stretch',
              gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
            }}
          >
            {planos.map((plano) => (
              <Paper
                key={plano.id}
                elevation={0}
                sx={{
                  position: 'relative',
                  p: 4,
                  display: 'flex',
                  flexDirection: 'column',
                  backgroundColor: SURFACE,
                  border: `${plano.destaque ? '2px' : '1px'} solid ${plano.destaque ? GOLD : BORDER}`,
                  borderRadius: 3,
                  background: plano.destaque
                    ? `linear-gradient(180deg, rgba(218,165,32,0.06), ${SURFACE} 45%)`
                    : SURFACE,
                  boxShadow: plano.destaque
                    ? '0 12px 32px rgba(218,165,32,0.22)'
                    : CARD_SHADOW,
                  transform: { md: plano.destaque ? 'scale(1.05)' : 'none' },
                  zIndex: plano.destaque ? 1 : 0,
                }}
              >
                {plano.destaque && (
                  <Chip
                    label="MAIS POPULAR"
                    size="small"
                    sx={{
                      position: 'absolute',
                      top: -14,
                      left: '50%',
                      transform: 'translateX(-50%)',
                      backgroundColor: GOLD,
                      color: '#111',
                      fontWeight: 800,
                      letterSpacing: 1,
                    }}
                  />
                )}

                <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
                  <Typography variant="h5" sx={{ fontWeight: 800 }}>
                    {plano.nome}
                  </Typography>
                  <Chip
                    label={plano.selo}
                    size="small"
                    sx={{
                      backgroundColor: 'transparent',
                      color: plano.cor,
                      border: `1px solid ${plano.cor}`,
                      fontWeight: 700,
                    }}
                  />
                </Stack>

                <Typography variant="body2" sx={{ color: TEXT_MUTED, minHeight: 44, mb: 2 }}>
                  {plano.publico}
                </Typography>

                <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5, mb: 3 }}>
                  <Typography variant="h3" sx={{ fontWeight: 900, color: TEXT_DARK }}>
                    {plano.preco}
                  </Typography>
                  <Typography variant="body1" sx={{ color: TEXT_MUTED }}>
                    {plano.periodo}
                  </Typography>
                </Box>

                <Divider sx={{ borderColor: BORDER, mb: 2 }} />

                <Stack spacing={1.5} sx={{ mb: 4, flexGrow: 1 }}>
                  {plano.recursos.map((item) => (
                    <Stack key={item} direction="row" spacing={1.2} alignItems="flex-start">
                      <CheckCircleIcon sx={{ fontSize: 20, color: GOLD, mt: '2px' }} />
                      <Typography variant="body2" sx={{ color: TEXT_DARK }}>
                        {item}
                      </Typography>
                    </Stack>
                  ))}
                </Stack>

                <Button
                  fullWidth
                  variant={plano.destaque ? 'contained' : 'outlined'}
                  size="large"
                  onClick={() =>
                    plano.id === 'ouro' ? irParaConsultor() : iniciarOnboarding(plano.id)
                  }
                  sx={
                    plano.destaque
                      ? {
                        backgroundColor: GOLD,
                        color: '#111',
                        fontWeight: 800,
                        boxShadow: '0 4px 14px rgba(218,165,32,0.4)',
                        '&:hover': { backgroundColor: GOLD_HOVER },
                      }
                      : {
                        color: TEXT_DARK,
                        borderColor: BORDER,
                        fontWeight: 700,
                        '&:hover': { borderColor: GOLD, backgroundColor: 'rgba(218,165,32,0.08)' },
                      }
                  }
                >
                  {plano.cta}
                </Button>
              </Paper>
            ))}
          </Box>
        </Container>
      </Box>

      {/* ---------- RODAPÉ ---------- */}
      <Box component="footer" sx={{ backgroundColor: FOOTER_BG, borderTop: `1px solid ${BORDER}`, py: 2 }}>
        <Container maxWidth="lg">
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            justifyContent="space-between"
            alignItems={{ xs: 'flex-start', md: 'center' }}
            spacing={2}
          >
            <Box>
              <Stack direction="row" alignItems="center" spacing={1}>
                <SecurityIcon sx={{ color: GOLD }} />
                <Typography variant="h6" sx={{ fontWeight: 800 }}>
                  Phish<span style={{ color: GOLD }}>Guard</span>
                </Typography>
              </Stack>
              <Typography variant="body2" sx={{ color: TEXT_MUTED, mt: 1, maxWidth: 360 }}>
                Plataforma de conscientização em segurança da informação contra engenharia social.
              </Typography>
            </Box>

            <Stack direction="row" spacing={3}>
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('recursos')}
                sx={{ color: TEXT_MUTED, '&:hover': { color: GOLD } }}
              >
                Recursos
              </Link>
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('precos')}
                sx={{ color: TEXT_MUTED, '&:hover': { color: GOLD } }}
              >
                Preços
              </Link>
              <Link
                component={RouterLink}
                to="/login"
                underline="none"
                sx={{ color: TEXT_MUTED, '&:hover': { color: GOLD } }}
              >
                Login
              </Link>
            </Stack>
          </Stack>

          <Divider sx={{ borderColor: BORDER, my: 3 }} />
          <Typography variant="caption" sx={{ color: TEXT_MUTED }}>
            © {new Date().getFullYear()} PhishGuard. Todos os direitos reservados. Uso destinado
            exclusivamente a treinamentos autorizados de segurança da informação.
          </Typography>
        </Container>
      </Box>
    </Box >
  );
}
