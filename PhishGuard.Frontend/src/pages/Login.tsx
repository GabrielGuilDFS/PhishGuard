import { useState } from 'react';
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


export default function Login() {
  const navigate = useNavigate();
  
  // Estados para guardar o que o usuário digita
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault(); // Evita recarregar a página
    setErro('');

    try {
      // 1. Chamada para a sua API (ajuste a porta se não for 5000)
      const response = await fetch('http://localhost:5000/api/Auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, senha })
      });

      if (!response.ok) {
        throw new Error('Email ou senha inválidos!');
      }

      // 2. Pegar o Token (que vem como texto puro da API)
      const token = await response.text();

      // 3. Salvar no Navegador (O passo mais importante!)
      localStorage.setItem('phishguard_token', token);

      // 4. Redirecionar para o Painel
      alert("Login realizado com sucesso!");
      navigate('/admin/dashboard');

    } catch (err: any) {
      setErro(err.message);
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper elevation={3} sx={{ p: 4, display: 'flex', flexDirection: 'column', alignItems: 'center', width: '100%' }}>
          <Typography component="h1" variant="h5">
            PhishGuard Admin
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
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
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
          <Link component={RouterLink} to="/Register" variant="body2">
                {"Sem conta? Cadastre-se"}
              </Link>
        </Paper>
      </Box>
    </Container>
  );
}