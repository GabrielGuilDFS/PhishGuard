import { useState } from 'react';
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
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useNotify } from '../context/NotificationContext';
import { setToken } from '../auth/session';
import { brandPalette } from '../theme';
// Deep import (não o barrel `@mui/icons-material`): named imports do barrel quebram o
// Vitest no Windows com EMFILE (milhares de ícones abertos de uma vez) — gotcha já
// documentado no projeto.
import SecurityIcon from '@mui/icons-material/Security';


export default function Login() {
  const navigate = useNavigate();
  const { showNotify } = useNotify();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [erro, setErro] = useState('');

  const ACCENT = brandPalette.light.accent;        // #0600c2 — CTAs, ícones, destaques
  const TEXT_DARK = brandPalette.light.text;       // #000000 — 21:1 sobre os cards brancos



  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro('');

    try {
      const response = await fetch(`${AUTH_API_BASE}/login`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (!response.ok) {
        throw new Error('Email ou senha inválidos!');
      }

      const payload = await response.json() as { accessToken: string };
      // setToken purga a sessão anterior ANTES de gravar: o novo login jamais
      // convive com resíduos (storage/cookies) do usuário anterior do navegador.
      setToken(payload.accessToken);

      showNotify("Login realizado com sucesso!", "success");

      navigate('/admin/dashboard');

    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Erro inesperado.';
      setErro(msg);

      showNotify(msg, "error");
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
            <SecurityIcon sx={{ color: ACCENT, mr: 1 }} />
            <Typography
              variant="h6"
              sx={{ fontWeight: 800, letterSpacing: 0.5, color: TEXT_DARK }}
            >
              Phish<Box component="span" sx={{ color: ACCENT }}>Guard</Box>
            </Typography>
          </Box>
          {/* Card BRANCO, não o `paper` (#6682f5) do tema: este formulário mostra erros
              de validação em vermelho, e vermelho sobre cobalto dá 1.06:1 — invisível.
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
              <Box textAlign="center">Login</Box>
            </Typography>

            <Box component="form" onSubmit={handleLogin} sx={{ mt: 1, width: '100%' }}>
              <TextField
                margin="normal"
                required
                fullWidth
                label="Endereço de Email"
                autoComplete="email"
                autoFocus
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="Senha"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />

              {erro && <Alert severity="error" sx={{ mt: 2 }}>{erro}</Alert>}

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
              >
                Entrar no Sistema
              </Button>
            </Box>
            <Link
              component={RouterLink}
              to="/Register"
              variant="body2"
              sx={{ color: 'primary.main', fontWeight: 'bold' }}
            >
              {"Sem conta? Cadastre-se"}
            </Link>
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
