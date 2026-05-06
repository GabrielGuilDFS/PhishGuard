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

export default function Register() {
  const navigate = useNavigate();

  const [nomeEmpresa, setNomeEmpresa] = useState('');
  const [cnpj, setCNPJ] = useState('');
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(false);
  const [loading, setLoading] = useState(false);

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

    const cnpjLimpo = cnpj.replace(/\D/g, '');

    // validação CNPJ
    if (cnpjLimpo.length !== 14) {
      setErro('CNPJ inválido.');
      return;
    }

    // validação senha
    if (password.length < 3) {
      setErro('A senha deve ter pelo menos 3 caracteres.');
      return;
    }

    try {
      setLoading(true);

      const response = await fetch('http://localhost:5000/api/Auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          NomeEmpresa: nomeEmpresa,
          Cnpj: cnpjLimpo,
          Nome: nome,
          Email: email,
          Password: password
        })
      });

      if (!response.ok) {
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
          const errData = await response.json();
          let errorMessage = errData.title || errData.message || 'Falha ao validar os dados.';
          if (errData.errors) {
            const validationErrors = Object.values(errData.errors).flat().join(' ');
            errorMessage += ` ${validationErrors}`;
          }
          throw new Error(errorMessage);
        } else {
          const mensagemErro = await response.text();
          throw new Error(mensagemErro || 'Falha ao registrar.');
        }
      }

      setSucesso(true);

      setTimeout(() => {
        navigate('/login');
      }, 2000);

    } catch (err: any) {
      setErro(err.message);
    } finally {
      setLoading(false);
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
            Crie sua conta Admin
          </Typography>

          <Box component="form" onSubmit={handleRegister} sx={{ mt: 1, width: '100%' }}>

            <TextField
              margin="normal"
              required
              fullWidth
              label="Nome da Empresa"
              autoFocus
              value={nomeEmpresa}
              onChange={(e) => setNomeEmpresa(e.target.value)}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              label="CNPJ"
              value={cnpj}
              onChange={(e) => setCNPJ(formatarCNPJ(e.target.value))}
              error={!!erro && erro.includes('CNPJ')}
              helperText={erro && erro.includes('CNPJ') ? erro : ''}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              label="Nome Completo"
              value={nome}
              onChange={(e) => setNome(e.target.value)}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              label="Endereço de Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              label="Senha"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
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
  );
}