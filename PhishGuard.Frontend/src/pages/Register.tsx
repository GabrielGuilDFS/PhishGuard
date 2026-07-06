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

  type CampoFormulario = 'nomeEmpresa' | 'cnpj' | 'nome' | 'email' | 'password';
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

    if (Object.keys(errosCliente).length > 0) {
      setFieldErrors(errosCliente);
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
          navigate('/login');
        }, 2000);
      }

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