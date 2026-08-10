import { useEffect, useState } from 'react';
import { AUTH_API_BASE } from '../config';
import {
  Box,
  Button,
  Container,
  TextField,
  Typography,
  Paper,
  Alert,
  Link
} from '@mui/material';
import { useNavigate, useSearchParams, Link as RouterLink } from 'react-router-dom';
import { clearSession } from '../auth/session';
import { brandPalette } from '../theme';
import PhishGuardMark from '../components/PhishGuardMark';

export default function Register() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  // Plano escolhido na landing (?plano=bronze|prata|ouro). Encaminhado ao checkout.
  const planoSelecionado = searchParams.get('plano');

  const ACCENT = brandPalette.light.accent;
  const TEXT_DARK = brandPalette.light.text;

  // Isolamento de sessão: entrar no fluxo de NOVA conta destrói qualquer sessão
  // residual do navegador (token de um login anterior). Sem isto, o checkout
  // desembocava no painel da conta ANTIGA — o token velho passava no PrivateRoute
  // e todas as telas requisitavam a API com o tenant errado.
  useEffect(() => {
    clearSession();
  }, []);

  const [nomeEmpresa, setNomeEmpresa] = useState('');
  const [cnpj, setCNPJ] = useState('');
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmarSenha, setConfirmarSenha] = useState('');

  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(false);
  const [loading, setLoading] = useState(false);

  // 'confirmarSenha' é validação client-side pura — nunca é enviada à API nem consta
  // no mapaCamposApi (o backend não conhece esse campo).
  type CampoFormulario = 'nomeEmpresa' | 'cnpj' | 'nome' | 'email' | 'password' | 'confirmarSenha';
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<CampoFormulario, string>>>({});

  // Chaves em minúsculo para permitir busca case-insensitive.
  // O ASP.NET Core pode serializar as chaves do ModelState em PascalCase (Email)
  // ou camelCase (email) dependendo das JsonOptions, então normalizamos ambos.
  const mapaCamposApi: Record<string, CampoFormulario> = {
    nomeempresa: 'nomeEmpresa',
    cnpj: 'cnpj',
    nome: 'nome',
    email: 'email',
    password: 'password'
  };

  const formatarCNPJ = (valor: string) => {
    let v = valor.replace(/\D/g, '');

    v = v.replace(/^(\d{2})(\d)/, '$1.$2');
    v = v.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
    v = v.replace(/\.(\d{3})(\d)/, '.$1/$2');
    v = v.replace(/(\d{4})(\d)/, '$1-$2');

    return v.slice(0, 18);
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro('');
    setSucesso(false);
    setFieldErrors({});

    const cnpjLimpo = cnpj.replace(/\D/g, '');
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    const errosCliente: Partial<Record<CampoFormulario, string>> = {};

    if (!nomeEmpresa.trim()) {
      errosCliente.nomeEmpresa = 'O nome da empresa é obrigatório.';
    }

    if (cnpjLimpo.length !== 14) {
      errosCliente.cnpj = 'O CNPJ deve conter exatamente 14 dígitos.';
    }

    if (!nome.trim()) {
      errosCliente.nome = 'O nome é obrigatório.';
    }

    if (!emailRegex.test(email)) {
      errosCliente.email = 'O formato do e-mail é inválido.';
    }

    if (password.length < 3) {
      errosCliente.password = 'A senha deve ter pelo menos 3 caracteres.';
    }

    if (confirmarSenha !== password) {
      errosCliente.confirmarSenha = 'As senhas não coincidem.';
    }

    if (Object.keys(errosCliente).length > 0) {
      setFieldErrors(errosCliente);
      return;
    }

    try {
      setLoading(true);

      const response = await fetch(`${AUTH_API_BASE}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          NomeEmpresa: nomeEmpresa,
          Cnpj: cnpjLimpo,
          Nome: nome,
          Email: email,
          Password: password,
          Plano: planoSelecionado ?? undefined
        })
      });

      if (!response.ok) {
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
          const errData = await response.json();

          if (errData.errors) {
            const errosApi: Partial<Record<CampoFormulario, string>> = {};
            for (const [campo, mensagens] of Object.entries(errData.errors)) {
              const chave = mapaCamposApi[campo.toLowerCase()];
              if (chave && Array.isArray(mensagens) && mensagens.length > 0) {
                errosApi[chave] = mensagens[0] as string;
              }
            }
            setFieldErrors(errosApi);
            if (Object.keys(errosApi).length === 0) {
              throw new Error(errData.title || errData.message || 'Falha ao validar os dados.');
            }
          } else {
            throw new Error(errData.title || errData.message || 'Falha ao validar os dados.');
          }
        } else {
          const mensagemErro = await response.text();
          throw new Error(mensagemErro || 'Falha ao registrar.');
        }
      } else {
        setSucesso(true);

        setTimeout(() => {
          // Planos self-service (Bronze/Prata) seguem para o checkout de faturamento.
          // Sem plano (ou Enterprise/Ouro), mantém o fluxo padrão de login.
          if (planoSelecionado === 'bronze' || planoSelecionado === 'prata') {
            navigate(`/checkout?plano=${planoSelecionado}`);
          } else {
            navigate('/login');
          }
        }, 2000);
      }

    } catch (err) {
      setErro(err instanceof Error ? err.message : 'Erro inesperado.');
    } finally {
      setLoading(false);
    }
  };

  return (
    // Tela travada no light pelo <ForcedLightScope> (ver App.tsx). A marca entra pelo
    // tint radial de Surface 1 (#6682f5) sobre o fundo branco.
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background:
          'radial-gradient(1000px 460px at 50% -10%, rgba(102,130,245,0.28), transparent 60%)',
      }}
    >
      <Container component="main" maxWidth="xs">
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            width: '100%',
          }}
        >
          {/* Logo clicável: volta para a LandingHome via SPA (RouterLink), sem reload.
              `component={RouterLink}` no lugar do <div> padrão do Box preserva o
              posicionamento absoluto existente — só a semântica/interatividade muda. */}
          <Box
            component={RouterLink}
            to="/"
            aria-label="Voltar para a página inicial do PhishGuard"
            sx={{
              position: 'absolute',
              top: 24,
              left: 24,
              display: 'flex',
              alignItems: 'center',
              textDecoration: 'none',
              color: 'inherit',
              borderRadius: 1,
              transition: 'opacity 300ms ease',
              '&:hover': { opacity: 0.9 },
              '&:focus-visible': {
                outline: 'none',
                // Anel de foco em Surface 1 (var(--primary) = #6682f5), distinto do
                // accent (#0600c2) usado no texto/ícone — só existe dentro do escopo
                // `.forced-light-theme` (ForcedLightScope), que define essa variável.
                boxShadow: '0 0 0 2px var(--primary)',
              },
            }}
          >
            <PhishGuardMark size={34} marginRight={0.5} />
            <Typography
              variant="h6"
              sx={{ fontWeight: 800, letterSpacing: 0.5, color: TEXT_DARK }}
            >
              Phish<Box component="span" sx={{ color: ACCENT }}>Guard</Box>
            </Typography>
          </Box>
          {/* Card BRANCO, não o `paper` (#6682f5) do tema: os erros de validação por
              campo (helperText vermelho) dariam 1.06:1 sobre cobalto — invisíveis.
              Ver a nota "cards de autenticação" em src/theme/forcedLight.ts. */}
          <Paper
            elevation={3}
            sx={{
              p: 4,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              width: '100%',
              bgcolor: 'background.default',
              border: 1,
              borderColor: 'divider',
            }}
          >
            <Typography component="h1" variant="h5">
              Cadastre-se
            </Typography>

            <Box component="form" onSubmit={handleRegister} noValidate sx={{ mt: 1, width: '100%' }}>

              <TextField
                margin="normal"
                required
                fullWidth
                label="Nome da Empresa"
                autoFocus
                value={nomeEmpresa}
                onChange={(e) => setNomeEmpresa(e.target.value)}
                error={!!fieldErrors.nomeEmpresa}
                helperText={fieldErrors.nomeEmpresa || ''}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                label="CNPJ"
                value={cnpj}
                onChange={(e) => setCNPJ(formatarCNPJ(e.target.value))}
                error={!!fieldErrors.cnpj}
                helperText={fieldErrors.cnpj || ''}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                label="Nome Completo"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                error={!!fieldErrors.nome}
                helperText={fieldErrors.nome || ''}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                label="Endereço de Email"
                type="text"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                error={!!fieldErrors.email}
                helperText={fieldErrors.email || ''}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                label="Senha"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                error={!!fieldErrors.password}
                helperText={fieldErrors.password || ''}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                label="Confirmar Senha"
                type="password"
                value={confirmarSenha}
                onChange={(e) => setConfirmarSenha(e.target.value)}
                error={!!fieldErrors.confirmarSenha}
                helperText={fieldErrors.confirmarSenha || ''}
              />

              {erro && <Alert severity="error" sx={{ mt: 2 }}>{erro}</Alert>}
              {sucesso && <Alert severity="success" sx={{ mt: 2 }}>Cadastro realizado! Redirecionando...</Alert>}

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
                disabled={sucesso || loading}
              >
                {loading ? 'Carregando...' : 'CADASTRAR'}
              </Button>

              <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                <Link
                  component={RouterLink}
                  to="/login"
                  variant="body2"
                  sx={{ color: 'primary.main', fontWeight: 'bold' }}
                >
                  Já tem uma conta? Faça Login
                </Link>
              </Box>

            </Box>
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
