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
import { brandPalette, mutedTextFor } from '../theme';
import { REGISTRATION_ENABLED } from '../config';
// Importada (e não referenciada por caminho solto tipo '/assets/images/...'): o Vite
// versiona o arquivo com hash e, se ele sumir, o BUILD quebra em vez de virar um 404
// silencioso em produção. A netflix-bg.png vive em public/ por outro motivo — é usada
// dentro de uma string de HTML cru (landingTemplates.ts), onde não existe import.
import fotoAnalista from '../assets/templates/pexels-arina-krasnikova-7005399.jpg';
// Versão de 1800px (147 KB) do original de 7952x5304 (5.1 MB, ainda no repo). O hero é
// a primeira dobra da porta de entrada do produto: 5 MB destruiriam o LCP. A 1800px a
// foto ainda tem o DOBRO do necessário — o container ocupa 45% de ~1920 = 864 CSS px
// (1728 em telas 2x). Para reverter, aponte o import de volta para o arquivo original.
import fotoTecnologia from '../assets/templates/pexels-thisisengineering-3861957-1800w.jpg';

// Identidade visual: paleta AZUL travada no modo light (a página é envolvida pelo
// <ForcedLightScope> em App.tsx e nunca responde ao toggle dark do painel).
//
// As cores vivem em constantes de módulo, e não no tema do MUI, porque esta tela pinta
// quase tudo via `sx` explícito — um ThemeProvider sozinho não a alcançaria. Os valores
// abaixo são os mesmos de `brandPalette.light` / `forcedLightTheme`; importados de lá
// para não haver um segundo lugar onde a paleta possa divergir.
const ACCENT = brandPalette.light.accent;        // #0600c2 — CTAs, ícones, destaques
const ACCENT_HOVER = '#04008f';                  // ultramar mais fechado p/ hover
const CARD_BG = '#ffffffff';                     // cards brancos
const BAND_BG = '#F0F0F0';                       // faixas de seção / rodapé
const PAGE_BG = '#F0F0F0';                       // fundo geral da página
const FOOTER_BG = BAND_BG;
const BORDER = '#c8c8c8ff';                      // borda neutra padrão da página
// Borda dos cards de recurso: Surface 2 da paleta (#b2c1fa) — mais suave e com a cor
// da marca, em vez do cinza neutro usado no resto da página.
const CARD_BORDER = brandPalette.light.secondary; // #b2c1fa
const ACCENT_BORDER = 'rgba(6,0,194,0.35)';      // borda de destaque sutil
const TEXT_DARK = brandPalette.light.text;       // #000000 — 21:1 sobre os cards brancos
const TEXT_MUTED = mutedTextFor('light');        // #05134d — 17.4:1 no branco
const CARD_SHADOW = '0 1px 3px rgba(6,0,194,0.10), 0 4px 12px rgba(6,0,194,0.06)';

// Fade da foto da seção "Recursos": apaga a IMAGEM da esquerda para a direita. Usamos
// máscara em vez de um overlay de cor sólida (como um `from-[#ffffff]`) porque o fundo
// da página é #F0F0F0, não branco — um overlay branco deixaria um halo claro visível
// por cima do cinza. A máscara funde com QUALQUER fundo. Vai a 0% na borda esquerda e
// fica opaca a 45%: o notebook (canto esquerdo da foto) esmaece para dentro da página
// e a profissional, que está no centro-direita, permanece nítida.
const FADE_FOTO = 'linear-gradient(to right, transparent 0%, rgba(0,0,0,0.55) 22%, #000 45%)';

// Fade da foto de FUNDO do hero. Aqui ele não é enfeite, é o que mantém o "alto
// contraste" pedido: o conteúdo do hero é centralizado (max-w-4xl) e a foto ocupa os
// 45% da direita, então os dois SE SOBREPÕEM em qualquer viewport — 100% da foto fica
// sob o texto em 900px, e 41% ainda em 1920px. Sem o fade, o texto preto cai para
// 1.85:1 sobre a tampa escura do notebook (o mínimo AA é 4.5:1). Com ele, a foto só
// ganha força no trecho à direita onde o texto centralizado não alcança.
const FADE_HERO = 'linear-gradient(to right, transparent 0%, rgba(0,0,0,0.35) 40%, #000 75%)';

// Selos de PLANO: são medalhas (Bronze/Prata/Ouro) — cor semântica do nível, não
// identidade de marca. Ficam fora da paleta azul pelo mesmo motivo dos status colors:
// um selo "Ouro" azul perde o significado. Vão sobre chip branco para manter contraste.
const BRONZE = '#CD7F32';
const SILVER = '#878787';
const GOLD = '#DAA520';

interface Recurso {
  icon: ReactNode;
  titulo: string;
  descricao: string;
}

const recursos: Recurso[] = [
  {
    icon: <SendIcon sx={{ fontSize: 22 }} />,
    titulo: 'Disparos de E-mails Simulados',
    descricao:
      'Um catálogo pronto de cenários que imitam marcas populares como NetsFlix e amzprime. Programe campanhas e meça quem realmente cai no anzol — sem risco para a operação.',
  },
  {
    icon: <WebIcon sx={{ fontSize: 22 }} />,
    titulo: 'Páginas de Captura Realistas',
    descricao:
      'Clones fiéis de telas de login, incluindo cenários como o do bho MAX. Capturamos a interação do colaborador de forma segura para dimensionar a exposição da empresa.',
  },
  {
    icon: <SchoolIcon sx={{ fontSize: 22 }} />,
    titulo: 'Feedback Educacional Imediato',
    descricao:
      'O "Momento Ensinável": ao cair na simulação, o colaborador é direcionado na hora para um treinamento pedagógico com o template básico de phishing. Erro vira aprendizado.',
  },
  {
    icon: <InsightsIcon sx={{ fontSize: 22 }} />,
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
    preco: 'R$ 59',
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
    preco: 'R$ 119',
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
    navigate(REGISTRATION_ENABLED ? `/register?plano=${planoId}` : '/login');
  };

  const irParaConsultor = () => {
    navigate(REGISTRATION_ENABLED ? '/register?plano=ouro&contato=consultor' : '/login');
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
          backgroundColor: PAGE_BG,
          backdropFilter: 'blur(8px)',
          borderBottom: `1px solid ${BORDER}`,
        }}
      >
        <Container maxWidth="lg">
          <Toolbar>
            <SecurityIcon sx={{ color: ACCENT, mr: 1 }} />
            <Typography
              variant="h6"
              sx={{ fontWeight: 800, letterSpacing: 0.5, flexGrow: 1, color: TEXT_DARK }}
            >
              Phish<Box component="span" sx={{ color: ACCENT }}>Guard</Box>
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
                sx={{ color: TEXT_DARK, fontWeight: 500, '&:hover': { color: ACCENT } }}
              >
                Recursos
              </Link>
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('precos')}
                sx={{ color: TEXT_DARK, fontWeight: 500, '&:hover': { color: ACCENT } }}
              >
                Preços
              </Link>
            </Stack>

            <Button
              component={RouterLink}
              to="/login"
              variant="outlined"
              sx={{
                color: ACCENT,
                borderColor: ACCENT_BORDER,
                fontWeight: 700,
                '&:hover': { borderColor: ACCENT, backgroundColor: 'rgba(6,0,194,0.06)' },
              }}
            >
              Login
            </Button>
          </Toolbar>
        </Container>
      </AppBar>

      {/* ---------- HERO ---------- */}
      {/* Pai já é `relative overflow-hidden`: a foto abaixo é posicionada contra ele. */}
      <Box
        sx={{
          position: 'relative',
          overflow: 'hidden',
          background: `radial-gradient(1200px 500px at 50% -10%, rgba(102,130,245,0.30), transparent 60%), ${PAGE_BG}`,
        }}
      >
        {/* Foto de fundo, ancorada à direita (z-0). Decorativa → background-image, sem
            alt. `pointerEvents: none` + `userSelect: none` garantem que ela nunca
            intercepte clique nos CTAs nem entre numa seleção de texto. */}
        <Box
          sx={{
            position: 'absolute',
            right: 0,
            top: 0,
            bottom: 0,
            width: { xs: '100%', md: '45%' },
            zIndex: 0,
            pointerEvents: 'none',
            userSelect: 'none',
            opacity: { xs: 0.4, md: 0.85 },
            transition: 'opacity 1s',
            backgroundImage: `url(${fotoTecnologia})`,
            backgroundSize: 'cover',
            backgroundPosition: { xs: 'left', md: 'center' },
            maskImage: FADE_HERO,
            WebkitMaskImage: FADE_HERO,
          }}
        />

        {/* Conteúdo centralizado (z-10, acima da foto). `Container maxWidth="md"` = 900px
            ≈ max-w-4xl (896px), com `mx: auto` e `text-align: center` já embutidos. */}
        <Container
          maxWidth="md"
          sx={{
            position: 'relative',
            zIndex: 1,
            pointerEvents: 'auto',
            textAlign: 'center',
            py: { xs: 8, md: 14 },
          }}
        >
          <Chip
            icon={<SecurityIcon sx={{ color: `${ACCENT} !important` }} />}
            label="Conscientização em Segurança da Informação"
            sx={{
              mb: 3,
              color: ACCENT,
              backgroundColor: 'rgba(102,130,245,0.16)',
              border: `1px solid ${ACCENT_BORDER}`,
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
            <Box component="span" sx={{ color: ACCENT }}>
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
              onClick={() => REGISTRATION_ENABLED ? scrollPara('precos') : navigate('/login')}
              sx={{
                backgroundColor: ACCENT,
                color: '#ffffff',
                fontWeight: 700,
                px: 4,
                py: 1.5,
                boxShadow: '0 4px 14px rgba(6,0,194,0.35)',
                '&:hover': { backgroundColor: ACCENT_HOVER },
              }}
            >
              {REGISTRATION_ENABLED ? 'Começar Agora' : 'Acessar demonstração'}
            </Button>
            <Button
              variant="outlined"
              size="large"
              onClick={() => navigate(REGISTRATION_ENABLED ? '/register?intencao=demonstracao' : '/login')}
              sx={{
                color: TEXT_DARK,
                borderColor: BORDER,
                backgroundColor: PAGE_BG,
                fontWeight: 700,
                px: 4,
                py: 1.5,
                '&:hover': { borderColor: ACCENT, backgroundColor: 'rgba(6,0,194,0.06)' },
              }}
            >
              {REGISTRATION_ENABLED ? 'Agendar Demonstração' : 'Entrar'}
            </Button>
          </Stack>
        </Container>
      </Box>

      {/* ---------- PROBLEMÁTICAS E RECURSOS ---------- */}
      {/* Título centralizado no topo; abaixo, grid de 2 colunas no desktop:
          cards compactos à esquerda + foto com fade-out à direita. */}
      <Box id="recursos" sx={{ py: { xs: 8, md: 12 }, scrollMarginTop: 80 }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="overline" sx={{ color: ACCENT, fontWeight: 700, letterSpacing: 2 }}>
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
        </Container>

        {/* max-w-7xl (1280) centralizado — mais largo que o Container "lg" do título, o
            que puxa os cards para mais perto da borda esquerda da tela. */}
        <Box
          sx={{
            maxWidth: 1280,
            mx: 'auto',
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
            gap: { xs: 4, md: 6 },
            alignItems: 'stretch',
          }}
        >
          {/* ----- COLUNA ESQUERDA: cards compactos em coluna única ----- */}
          <Stack
            spacing={2}
            sx={{ pl: { xs: 2, sm: 3, lg: 4 }, pr: { xs: 2, sm: 3, md: 0 } }}
          >
            {recursos.map((r) => (
              <Paper
                key={r.titulo}
                elevation={0}
                sx={{
                  // Compacto: ícone AO LADO do texto (e não acima), padding menor.
                  p: 2.5,
                  display: 'flex',
                  alignItems: 'flex-start',
                  gap: 2,
                  backgroundColor: CARD_BG,
                  border: `1px solid ${CARD_BORDER}`,
                  borderRadius: 3,
                  boxShadow: CARD_SHADOW,
                  transition: 'transform .2s, border-color .2s, box-shadow .2s',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    borderColor: ACCENT_BORDER,
                    boxShadow: '0 8px 20px rgba(6,0,194,0.16)',
                  },
                }}
              >
                <Box
                  sx={{
                    width: 44,
                    height: 44,
                    borderRadius: 2,
                    flexShrink: 0,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: ACCENT,
                    backgroundColor: 'rgba(102,130,245,0.16)',
                  }}
                >
                  {r.icon}
                </Box>
                <Box>
                  <Typography variant="subtitle1" sx={{ fontWeight: 700, color: TEXT_DARK, mb: 0.5 }}>
                    {r.titulo}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ color: TEXT_MUTED, lineHeight: 1.6, fontSize: '0.85rem' }}
                  >
                    {r.descricao}
                  </Typography>
                </Box>
              </Paper>
            ))}
          </Stack>

          {/* ----- COLUNA DIREITA: foto com fade-out para dentro da página -----
              Decorativa (a informação está nos cards) → entra como background-image,
              sem alt, e some no mobile: em coluna única o fade lateral não faz sentido
              e o browser nem baixa a imagem de um elemento display:none. */}
          <Box
            sx={{
              display: { xs: 'none', md: 'block' },
              minHeight: 450,
              height: '100%',
              borderRadius: '0 16px 16px 0',
              backgroundImage: `url(${fotoAnalista})`,
              backgroundSize: 'cover',
              backgroundPosition: 'center',
              maskImage: FADE_FOTO,
              WebkitMaskImage: FADE_FOTO,
            }}
          />
        </Box>
      </Box>

      {/* ---------- PRICING ---------- */}
      <Box id="precos" sx={{ backgroundColor: BAND_BG, py: { xs: 8, md: 12 }, scrollMarginTop: 80 }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="overline" sx={{ color: ACCENT, fontWeight: 700, letterSpacing: 2 }}>
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
                  color: TEXT_DARK,
                  position: 'relative',
                  p: 4,
                  display: 'flex',
                  flexDirection: 'column',
                  backgroundColor: CARD_BG,
                  border: `${plano.destaque ? '2px' : '1px'} solid ${plano.destaque ? ACCENT : BORDER}`,
                  borderRadius: 3,
                  boxShadow: plano.destaque
                    ? '0 12px 32px rgba(6,0,194,0.28)'
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
                      backgroundColor: ACCENT,
                      color: '#ffffff',
                      fontWeight: 800,
                      letterSpacing: 1,
                    }}
                  />
                )}

                <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
                  <Typography variant="h5" sx={{ fontWeight: 800 }}>
                    {plano.nome}
                  </Typography>
                  {/* Selo de medalha sobre chip BRANCO: bronze/prata/ouro sobre o card
                      cobalto ficariam encardidos e sem contraste. */}
                  <Chip
                    label={plano.selo}
                    size="small"
                    sx={{
                      backgroundColor: PAGE_BG,
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
                      <CheckCircleIcon sx={{ fontSize: 20, color: ACCENT, mt: '2px' }} />
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
                        backgroundColor: ACCENT,
                        color: '#ffffff',
                        fontWeight: 800,
                        boxShadow: '0 4px 14px ACCENT',
                        '&:hover': { backgroundColor: ACCENT_HOVER },
                      }
                      : {
                        color: '#ffffff',
                        borderColor: 'rgba(0, 0, 0, 0.19)',
                        backgroundColor: ACCENT,
                        fontWeight: 700,
                        '&:hover': { borderColor: PAGE_BG, backgroundColor: ACCENT_HOVER },
                      }
                  }
                >
                  {REGISTRATION_ENABLED ? plano.cta : 'Acessar conta'}
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
                <SecurityIcon sx={{ color: ACCENT }} />
                <Typography variant="h6" sx={{ fontWeight: 800, color: TEXT_DARK }}>
                  Phish<span style={{ color: ACCENT }}>Guard</span>
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
                sx={{ color: TEXT_MUTED, '&:hover': { color: ACCENT } }}
              >
                Recursos
              </Link>
              <Link
                component="button"
                underline="none"
                onClick={() => scrollPara('precos')}
                sx={{ color: TEXT_MUTED, '&:hover': { color: ACCENT } }}
              >
                Preços
              </Link>
              <Link
                component={RouterLink}
                to="/login"
                underline="none"
                sx={{ color: TEXT_MUTED, '&:hover': { color: ACCENT } }}
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
