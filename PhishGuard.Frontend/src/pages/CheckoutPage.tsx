import { useEffect, useMemo, useState, type ReactNode, type SyntheticEvent } from 'react';
import { API_BASE } from '../config';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { clearSession } from '../auth/session';
import {
  Box,
  Button,
  Container,
  Paper,
  Typography,
  Stack,
  Divider,
  Tabs,
  Tab,
  TextField,
  InputAdornment,
  IconButton,
  CircularProgress,
  Chip,
  Backdrop,
} from '@mui/material';
// Deep imports (não o barrel `@mui/icons-material`): named imports do barrel quebram
// o Vitest no Windows com EMFILE (milhares de ícones abertos de uma vez) — gotcha já
// documentado no projeto.
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import LockIcon from '@mui/icons-material/Lock';
import PixIcon from '@mui/icons-material/Pix';
import CreditCardIcon from '@mui/icons-material/CreditCard';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useNotify } from '../context/NotificationContext';

// Paleta corporativa clara com detalhes em dourado (alinhada à HomeLandingPage / painel).
const GOLD = '#DAA520';
const GOLD_HOVER = '#c6941c';
const PAGE_BG = '#F4F5F7';
const SURFACE = '#FFFFFF';
const BORDER = '#E2E8F0';
const TEXT_DARK = '#0F172A';
const TEXT_MUTED = '#475569';
const CARD_SHADOW = '0 1px 3px rgba(15,23,42,0.08), 0 4px 12px rgba(15,23,42,0.05)';

interface PlanoCheckout {
  selo: string;
  nome: string;
  preco: string;
  pixCopiaECola: string;
}

// Apenas planos com faturamento self-service. O Enterprise (Ouro) segue pelo
// contato consultivo e não passa por esta tela de checkout.
const PLANOS: Record<string, PlanoCheckout> = {
  bronze: {
    selo: 'Bronze',
    nome: 'Plano Inicial',
    preco: 'R$ 149,00',
    pixCopiaECola:
      '00020126580014BR.GOV.BCB.PIX0136phishguard-bronze@pagamentos.com5204000053039865406149.005802BR5909PhishGuard6009Sao Paulo62070503***6304A1B2',
  },
  prata: {
    selo: 'Prata',
    nome: 'Plano Profissional',
    preco: 'R$ 590,00',
    pixCopiaECola:
      '00020126580014BR.GOV.BCB.PIX0136phishguard-prata@pagamentos.com5204000053039865406590.005802BR5909PhishGuard6009Sao Paulo62070503***6304C3D4',
  },
};

const PLANO_PADRAO = 'prata';

// Gera uma matriz determinística de módulos para um QR Code fictício (apenas
// visual — não codifica dados reais). O seed garante um padrão estável por plano.
function gerarModulosQr(seed: string, tamanho = 25): boolean[][] {
  let h = 2166136261;
  for (let i = 0; i < seed.length; i++) {
    h ^= seed.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  const rand = () => {
    h = (Math.imul(h, 1103515245) + 12345) & 0x7fffffff;
    return h / 0x7fffffff;
  };
  const dentroDoFinder = (r: number, c: number) =>
    (r < 8 && c < 8) || (r < 8 && c >= tamanho - 8) || (r >= tamanho - 8 && c < 8);
  return Array.from({ length: tamanho }, (_, r) =>
    Array.from({ length: tamanho }, (_, c) => (dentroDoFinder(r, c) ? false : rand() > 0.55)),
  );
}

function QrCodeFicticio({ seed }: { seed: string }) {
  const tamanho = 25;
  const modulos = useMemo(() => gerarModulosQr(seed, tamanho), [seed]);

  const finder = (x: number, y: number) => (
    <g key={`f-${x}-${y}`} fill={TEXT_DARK}>
      <rect x={x} y={y} width={7} height={7} rx={1} />
      <rect x={x + 1} y={y + 1} width={5} height={5} rx={1} fill={SURFACE} />
      <rect x={x + 2} y={y + 2} width={3} height={3} rx={0.5} />
    </g>
  );

  return (
    <Box
      sx={{
        width: 200,
        height: 200,
        p: 1.5,
        backgroundColor: SURFACE,
        border: `1px solid ${BORDER}`,
        borderRadius: 2,
        boxShadow: CARD_SHADOW,
      }}
    >
      <svg viewBox={`0 0 ${tamanho} ${tamanho}`} width="100%" height="100%" role="img" aria-label="QR Code Pix fictício">
        <rect x={0} y={0} width={tamanho} height={tamanho} fill={SURFACE} />
        {modulos.map((linha, r) =>
          linha.map((ativo, c) =>
            ativo ? <rect key={`${r}-${c}`} x={c} y={r} width={1} height={1} fill={TEXT_DARK} /> : null,
          ),
        )}
        {finder(0, 0)}
        {finder(tamanho - 7, 0)}
        {finder(0, tamanho - 7)}
      </svg>
    </Box>
  );
}

interface TabPanelProps {
  children: ReactNode;
  value: number;
  index: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  if (value !== index) return null;
  return (
    <Box role="tabpanel" sx={{ pt: 4 }}>
      {children}
    </Box>
  );
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const { showNotify } = useNotify();
  const [searchParams] = useSearchParams();

  const planoId = (searchParams.get('plano') || PLANO_PADRAO).toLowerCase();
  const plano = PLANOS[planoId] ?? PLANOS[PLANO_PADRAO];

  const [aba, setAba] = useState(0);
  const [processando, setProcessando] = useState(false);

  // Isolamento de sessão: o checkout SÓ é alcançável pelo fluxo de nova conta
  // (Register → /checkout). Qualquer token residual aqui é de OUTRA conta do mesmo
  // navegador — destruído na entrada, para que nada deste fluxo (nem a ativação de
  // plano abaixo, nem o redirecionamento final) execute no escopo do tenant errado.
  useEffect(() => {
    clearSession();
  }, []);

  // Campos do cartão (simulados).
  const [numeroCartao, setNumeroCartao] = useState('');
  const [titular, setTitular] = useState('');
  const [validade, setValidade] = useState('');
  const [cvv, setCvv] = useState('');

  const formatarNumeroCartao = (v: string) =>
    v.replace(/\D/g, '').slice(0, 16).replace(/(\d{4})(?=\d)/g, '$1 ').trim();

  const formatarValidade = (v: string) => {
    const d = v.replace(/\D/g, '').slice(0, 4);
    return d.length > 2 ? `${d.slice(0, 2)}/${d.slice(2)}` : d;
  };

  const copiarPix = async () => {
    try {
      await navigator.clipboard.writeText(plano.pixCopiaECola);
      showNotify('Código Pix copiado!', 'success');
    } catch {
      showNotify('Não foi possível copiar automaticamente.', 'warning');
    }
  };

  // Simula a resposta da API financeira e, em seguida, o envio de um payload ao
  // backend C# .NET que ativa o Tenant. O endpoint real ainda não existe, então a
  // chamada é best-effort (a simulação prossegue mesmo se indisponível).
  //
  // ⚠️ Segurança (correção de isolamento): a chamada é ANÔNIMA de propósito. A conta
  // recém-criada ainda não tem token (o register não emite JWT), e a versão anterior
  // anexava o Bearer RESIDUAL da conta antiga do navegador — o que ativaria o plano
  // no tenant ERRADO. Quando o endpoint real existir, a amarração conta↔pagamento
  // deve vir de um token de ativação de uso único emitido pelo próprio register,
  // nunca da sessão que estiver caída no storage.
  const finalizarAssinatura = () => {
    setProcessando(true);

    setTimeout(async () => {
      const payload = {
        plano: planoId,
        metodoPagamento: aba === 0 ? 'pix' : 'cartao_credito',
        statusTenant: 'Ativo',
      };

      try {
        await fetch(`${API_BASE}/Tenants/ativar`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload),
        });
      } catch {
        // Simulação: ignora a indisponibilidade do endpoint financeiro.
      }

      // A conta nova nunca autenticou: o único caminho legítimo para o painel é um
      // login limpo. Redirecionar direto a /admin abriria o painel da conta que
      // estivesse logada antes neste navegador (o bug original).
      showNotify('Pagamento aprovado! Assinatura ativada — faça login para acessar o painel.', 'success');
      navigate('/login');
    }, 2000);
  };

  const cartaoIncompleto =
    numeroCartao.replace(/\s/g, '').length < 16 ||
    titular.trim().length < 3 ||
    validade.length < 5 ||
    cvv.length < 3;

  return (
    <Box sx={{ backgroundColor: PAGE_BG, minHeight: '100vh', color: TEXT_DARK, py: { xs: 4, md: 8 } }}>
      <Container maxWidth="md">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ color: TEXT_MUTED, mb: 3, textTransform: 'none', '&:hover': { color: GOLD } }}
        >
          Voltar
        </Button>

        <Box
          sx={{
            display: 'grid',
            gap: 4,
            alignItems: 'start',
            gridTemplateColumns: { xs: '1fr', md: '1fr 1.4fr' },
          }}
        >
          {/* ---------- RESUMO DO PEDIDO ---------- */}
          <Paper
            elevation={0}
            sx={{
              p: 4,
              backgroundColor: SURFACE,
              border: `1px solid ${BORDER}`,
              borderRadius: 3,
              boxShadow: CARD_SHADOW,
              position: { md: 'sticky' },
              top: { md: 32 },
            }}
          >
            <Typography variant="overline" sx={{ color: TEXT_MUTED, fontWeight: 700, letterSpacing: 1.5 }}>
              Resumo do pedido
            </Typography>

            <Stack direction="row" alignItems="center" spacing={1} sx={{ mt: 1.5, mb: 0.5 }}>
              <Typography variant="h5" sx={{ fontWeight: 800 }}>
                {plano.nome}
              </Typography>
              <Chip
                label={plano.selo}
                size="small"
                sx={{ color: GOLD, border: `1px solid ${GOLD}`, backgroundColor: 'transparent', fontWeight: 700 }}
              />
            </Stack>
            <Typography variant="body2" sx={{ color: TEXT_MUTED, mb: 3 }}>
              Assinatura mensal · renovação automática
            </Typography>

            <Divider sx={{ borderColor: BORDER, mb: 2 }} />

            <Stack direction="row" justifyContent="space-between" sx={{ mb: 1 }}>
              <Typography variant="body2" sx={{ color: TEXT_MUTED }}>
                Subtotal
              </Typography>
              <Typography variant="body2" sx={{ color: TEXT_DARK, fontWeight: 600 }}>
                {plano.preco}
              </Typography>
            </Stack>
            <Stack direction="row" justifyContent="space-between" sx={{ mb: 2 }}>
              <Typography variant="body2" sx={{ color: TEXT_MUTED }}>
                Impostos
              </Typography>
              <Typography variant="body2" sx={{ color: TEXT_MUTED }}>
                Inclusos
              </Typography>
            </Stack>

            <Divider sx={{ borderColor: BORDER, mb: 2 }} />

            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                Total mensal
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 900, color: TEXT_DARK }}>
                {plano.preco}
              </Typography>
            </Stack>
          </Paper>

          {/* ---------- PAGAMENTO ---------- */}
          <Paper
            elevation={0}
            sx={{
              p: { xs: 3, md: 4 },
              backgroundColor: SURFACE,
              border: `1px solid ${BORDER}`,
              borderRadius: 3,
              boxShadow: CARD_SHADOW,
            }}
          >
            <Typography variant="h5" sx={{ fontWeight: 800, mb: 0.5 }}>
              Dados de faturamento
            </Typography>
            <Typography variant="body2" sx={{ color: TEXT_MUTED, mb: 3 }}>
              Escolha a forma de pagamento para ativar sua conta.
            </Typography>

            <Tabs
              value={aba}
              onChange={(_e: SyntheticEvent, novo: number) => setAba(novo)}
              variant="fullWidth"
              sx={{
                borderBottom: `1px solid ${BORDER}`,
                '& .MuiTab-root': { textTransform: 'none', fontWeight: 700, color: TEXT_MUTED },
                '& .Mui-selected': { color: `${GOLD} !important` },
                '& .MuiTabs-indicator': { backgroundColor: GOLD },
              }}
            >
              <Tab icon={<PixIcon />} iconPosition="start" label="Pix" />
              <Tab icon={<CreditCardIcon />} iconPosition="start" label="Cartão de Crédito" />
            </Tabs>

            {/* ----- ABA PIX ----- */}
            <TabPanel value={aba} index={0}>
              <Stack alignItems="center" spacing={2}>
                <QrCodeFicticio seed={`pix-${planoId}`} />
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="body2" sx={{ color: TEXT_MUTED }}>
                    Valor a pagar
                  </Typography>
                  <Typography variant="h5" sx={{ fontWeight: 900, color: TEXT_DARK }}>
                    {plano.preco}
                  </Typography>
                </Box>

                <TextField
                  fullWidth
                  label="Pix Copia e Cola"
                  value={plano.pixCopiaECola}
                  InputProps={{
                    readOnly: true,
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton onClick={copiarPix} edge="end" aria-label="Copiar código Pix" sx={{ color: GOLD }}>
                          <ContentCopyIcon />
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />

                <Button
                  fullWidth
                  size="large"
                  variant="contained"
                  disabled={processando}
                  onClick={finalizarAssinatura}
                  startIcon={<CheckCircleIcon />}
                  sx={{
                    backgroundColor: GOLD,
                    color: '#111',
                    fontWeight: 800,
                    py: 1.4,
                    boxShadow: '0 4px 14px rgba(218,165,32,0.4)',
                    '&:hover': { backgroundColor: GOLD_HOVER },
                  }}
                >
                  Confirmar Pagamento
                </Button>
              </Stack>
            </TabPanel>

            {/* ----- ABA CARTÃO ----- */}
            <TabPanel value={aba} index={1}>
              <Stack spacing={2.5}>
                <TextField
                  fullWidth
                  label="Número do Cartão"
                  placeholder="0000 0000 0000 0000"
                  value={numeroCartao}
                  onChange={(e) => setNumeroCartao(formatarNumeroCartao(e.target.value))}
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <CreditCardIcon sx={{ color: TEXT_MUTED }} />
                      </InputAdornment>
                    ),
                  }}
                />
                <TextField
                  fullWidth
                  label="Nome do Titular"
                  placeholder="Como está impresso no cartão"
                  value={titular}
                  onChange={(e) => setTitular(e.target.value.toUpperCase())}
                />
                <Stack direction="row" spacing={2}>
                  <TextField
                    fullWidth
                    label="Validade (MM/AA)"
                    placeholder="MM/AA"
                    value={validade}
                    onChange={(e) => setValidade(formatarValidade(e.target.value))}
                  />
                  <TextField
                    fullWidth
                    label="CVV"
                    placeholder="123"
                    value={cvv}
                    onChange={(e) => setCvv(e.target.value.replace(/\D/g, '').slice(0, 4))}
                  />
                </Stack>

                <Button
                  fullWidth
                  size="large"
                  variant="contained"
                  disabled={processando || cartaoIncompleto}
                  onClick={finalizarAssinatura}
                  startIcon={<LockIcon />}
                  sx={{
                    backgroundColor: GOLD,
                    color: '#111',
                    fontWeight: 800,
                    py: 1.4,
                    boxShadow: '0 4px 14px rgba(218,165,32,0.4)',
                    '&:hover': { backgroundColor: GOLD_HOVER },
                    '&.Mui-disabled': { backgroundColor: '#E7DCC0', color: '#9A8B66' },
                  }}
                >
                  Finalizar Assinatura
                </Button>
              </Stack>
            </TabPanel>

            <Stack direction="row" alignItems="center" justifyContent="center" spacing={0.8} sx={{ mt: 3 }}>
              <LockIcon sx={{ fontSize: 16, color: TEXT_MUTED }} />
              <Typography variant="caption" sx={{ color: TEXT_MUTED }}>
                Ambiente de pagamento simulado · dados não são processados
              </Typography>
            </Stack>
          </Paper>
        </Box>
      </Container>

      {/* ---------- OVERLAY DE PROCESSAMENTO ---------- */}
      <Backdrop open={processando} sx={{ color: '#fff', zIndex: (t) => t.zIndex.modal + 1, flexDirection: 'column', gap: 2 }}>
        <CircularProgress sx={{ color: GOLD }} />
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Processando pagamento...
        </Typography>
      </Backdrop>
    </Box>
  );
}
