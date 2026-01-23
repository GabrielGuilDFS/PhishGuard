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
  
  // Estados para os campos do formulário
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  
  // Estados para feedback visual
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(false);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro('');
    setSucesso(false);

    // Validação simples antes de enviar
    if (senha.length < 3) {
      setErro('A senha deve ter pelo menos 3 caracteres.');
      return;
    }

    try {
      // 1. Envia para o Backend na porta 5000
      const response = await fetch('http://localhost:5000/api/Auth/registrar', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nome, email, senha })
      });

      // 2. Se a API retornar erro (ex: email já existe), lança exceção
      if (!response.ok) {
        const mensagemErro = await response.text();
        throw new Error(mensagemErro || 'Falha ao registrar.');
      }

      // 3. Sucesso!
      setSucesso(true);
      
      // Espera 2 segundos para o usuário ler a mensagem e redireciona para o Login
      setTimeout(() => {
        navigate('/login');
      }, 2000);

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
            Crie sua conta Admin
          </Typography>
          
          <Box component="form" onSubmit={handleRegister} sx={{ mt: 1, width: '100%' }}>
            <TextField
              margin="normal"
              required
              fullWidth
              label="Nome Completo"
              autoFocus
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
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
            />
            
            {/* Mensagens de Erro ou Sucesso */}
            {erro && <Alert severity="error" sx={{ mt: 2 }}>{erro}</Alert>}
            {sucesso && <Alert severity="success" sx={{ mt: 2 }}>Cadastro realizado! Redirecionando...</Alert>}

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={sucesso} // Bloqueia o botão se já deu certo
            >
              CADASTRAR
            </Button>

            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
              <Link component={RouterLink} to="/login" variant="body2">
                {"Já tem uma conta? Faça Login"}
              </Link>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
}