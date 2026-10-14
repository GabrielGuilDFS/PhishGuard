import { useRef, useState } from 'react';
import { AUTH_API_BASE, REGISTRATION_ENABLED } from '../config';
import {
  Box,
  Button,
  Container,
  TextField,
  Typography,
  Paper,
  Alert,
  Link,
  CircularProgress,
  IconButton,
  InputAdornment,
} from '@mui/material';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import { useLocation, useNavigate, Link as RouterLink } from 'react-router-dom';
import { useNotify } from '../context/NotificationContext';
import { setToken } from '../auth/session';
import { brandPalette } from '../theme';
import PhishGuardMark from '../components/PhishGuardMark';

function loginDestination(state: unknown): string {
  const fallback = '/admin/dashboard';
  if (!state || typeof state !== 'object' || !('from' in state)
    || typeof state.from !== 'string') return fallback;

  // Somente rotas administrativas locais: nunca aceitar redirecionamentos externos.
  if (!/^\/admin(?:\/(?:dashboard|targets|campaigns|templates|settings))?(?:[?#]|$)/.test(state.from)
    || state.from.includes('\\')) return fallback;
  return state.from;
}

function loginError(status: number): string {
  if (status === 400 || status === 401) return 'E-mail ou senha inválidos.';
  if (status === 429) return 'Muitas tentativas. Aguarde um momento antes de tentar novamente.';
  return 'Não foi possível entrar agora. Tente novamente em instantes.';
}

export default function Login() {
  const navigate = useNavigate();
  const location = useLocation();
  const { showNotify } = useNotify();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [capsLock, setCapsLock] = useState(false);
  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const emailRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);
  const submittingRef = useRef(false);

  const ACCENT = brandPalette.light.accent;        // #0600c2 — CTAs, ícones, destaques
  const TEXT_DARK = brandPalette.light.text;       // #000000 — 21:1 sobre os cards brancos



  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (submittingRef.current) return;
    setErro('');
    const normalizedEmail = email.trim();
    const invalidEmail = !normalizedEmail
      ? 'Informe seu e-mail.'
      : emailRef.current?.validity.typeMismatch ? 'Informe um e-mail válido.' : '';
    const invalidPassword = !password ? 'Informe sua senha.' : '';
    setEmailError(invalidEmail);
    setPasswordError(invalidPassword);
    if (invalidEmail || invalidPassword) {
      (invalidEmail ? emailRef : passwordRef).current?.focus();
      return;
    }

    submittingRef.current = true;
    setLoading(true);
    setShowPassword(false);
    setCapsLock(false);
    setEmail(normalizedEmail);

    try {
      const response = await fetch(`${AUTH_API_BASE}/login`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: normalizedEmail, password })
      });

      if (!response.ok) {
        const message = loginError(response.status);
        setErro(message);
        showNotify(message, 'error');
        return;
      }

      const payload: unknown = await response.json();
      if (!payload || typeof payload !== 'object' || !('accessToken' in payload)
        || typeof payload.accessToken !== 'string' || !payload.accessToken.trim()) {
        throw new Error('Resposta de autenticação inválida.');
      }
      // O bearer permanece em memória; setToken remove tokens legados do storage.
      setToken(payload.accessToken);

      showNotify("Login realizado com sucesso!", "success");

      navigate(loginDestination(location.state), { replace: true });

    } catch (err) {
      const msg = err instanceof TypeError
        ? 'Não foi possível conectar. Verifique sua conexão e tente novamente.'
        : 'Não foi possível entrar agora. Tente novamente em instantes.';
      setErro(msg);

      showNotify(msg, "error");
    } finally {
      submittingRef.current = false;
      setLoading(false);
    }
  };

  return (
    // Tela travada no light pelo <ForcedLightScope> (ver App.tsx). A marca entra pelo
    // tint radial de Surface 1 (#6682f5) sobre o fundo branco.
    <Box
      sx={{
        minHeight: '100vh',
        '@supports (height: 100dvh)': { minHeight: '100dvh' },
        py: { xs: 3, sm: 10 },
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
              position: { xs: 'static', sm: 'absolute' },
              top: { sm: 24 },
              left: { sm: 24 },
              alignSelf: 'flex-start',
              mb: { xs: 4, sm: 0 },
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
          {/* Card BRANCO, não o `paper` (#6682f5) do tema: este formulário mostra erros
              de validação em vermelho, e vermelho sobre cobalto dá 1.06:1 — invisível.
              Ver a nota "cards de autenticação" em src/theme/forcedLight.ts. */}
          <Paper
            elevation={3}
            sx={{
              p: { xs: 3, sm: 4 },
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

            <Box component="form" noValidate onSubmit={handleLogin} aria-busy={loading} sx={{ mt: 1, width: '100%' }}>
              <TextField
                margin="normal"
                required
                fullWidth
                id="login-email"
                name="email"
                label="Endereço de E-mail"
                type="email"
                autoComplete="email"
                autoFocus
                value={email}
                inputRef={emailRef}
                disabled={loading}
                error={Boolean(emailError)}
                helperText={emailError}
                slotProps={{ htmlInput: { autoCapitalize: 'none', spellCheck: false } }}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setEmailError('');
                  setErro('');
                }}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="Senha"
                id="login-password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                value={password}
                inputRef={passwordRef}
                disabled={loading}
                error={Boolean(passwordError)}
                helperText={passwordError}
                onKeyDown={(e) => setCapsLock(e.getModifierState('CapsLock'))}
                onKeyUp={(e) => setCapsLock(e.getModifierState('CapsLock'))}
                onBlur={() => setCapsLock(false)}
                slotProps={{
                  htmlInput: {
                    'aria-describedby': [passwordError ? 'login-password-helper-text' : '', capsLock ? 'login-caps-lock' : '']
                      .filter(Boolean).join(' ') || undefined,
                  },
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          type="button"
                          aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
                          aria-pressed={showPassword}
                          disabled={loading}
                          onMouseDown={(e) => e.preventDefault()}
                          onClick={() => setShowPassword((visible) => !visible)}
                          edge="end"
                          sx={{ minWidth: 44, minHeight: 44 }}
                        >
                          {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  },
                }}
                onChange={(e) => {
                  setPassword(e.target.value);
                  setPasswordError('');
                  setErro('');
                }}
              />

              {capsLock && <Typography id="login-caps-lock" role="status" variant="body2" color="text.secondary">Caps Lock está ativado.</Typography>}
              {erro && <Alert severity="error" sx={{ mt: 2 }}>{erro}</Alert>}

              <Button
                type="submit"
                fullWidth
                variant="contained"
                disabled={loading}
                startIcon={loading ? <CircularProgress size={18} color="inherit" aria-hidden="true" /> : undefined}
                sx={{ mt: 3, mb: 2 }}
              >
                {loading ? 'Entrando…' : 'Entrar no Sistema'}
              </Button>
            </Box>
            {REGISTRATION_ENABLED && (
              <Link
                component={RouterLink}
                to="/register"
                variant="body2"
                sx={{ color: 'primary.main', fontWeight: 'bold' }}
              >
                {"Sem conta? Cadastre-se"}
              </Link>
            )}
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
