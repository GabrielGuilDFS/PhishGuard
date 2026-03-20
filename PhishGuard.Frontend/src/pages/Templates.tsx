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
  PostAdd as PostAddIcon,
  Search as SearchIcon,
  AutoAwesome as AutoAwesomeIcon
} from '@mui/icons-material';

interface Template {
  id: string; 
  nome: string;
  assunto: string;
  remetenteNome: string;
  remetenteEmail: string;
  corpoHtml: string;
}

const marcas = [
  { id: 'microsoft', nome: 'Microsoft 365', cor: '#0078d4', remetente: 'suporte@microsoft-seguranca.com' },
  { id: 'google', nome: 'Google Workspace', cor: '#ea4335', remetente: 'alerts@google-security-noreply.com' },
  { id: 'interna', nome: 'Comunicação Interna', cor: '#2e7d32', remetente: 'ti@comunicado-interno.com' }
];

const ataques = [
  { id: 'senha_expira', titulo: 'Alerta de Senha Expirando', assunto: 'Ação Exigida: Sua senha expira em 24h' },
  { id: 'atividade_suspeita', titulo: 'Conta Invadida', assunto: 'ALERTA: Novo login não reconhecido na sua conta' },
  { id: 'documento_rh', titulo: 'Documento do RH', assunto: 'CONFIDENCIAL: Atualização de Política Salarial' }
];

const gerarHtmlDinamico = (marca: any, ataque: any, nomeEmpresaInterna: string) => {
  const nomeExibicao = marca.id === 'interna' ? nomeEmpresaInterna : marca.nome;
  
  if (ataque.id === 'senha_expira') {
    return `<div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px;">
  <h2 style="color: ${marca.cor};">${nomeExibicao}</h2>
  <p>Prezado(a) colaborador(a),</p>
  <p>Sua senha de acesso à rede expirará em <strong>24 horas</strong>.</p>
  <p>Para evitar o bloqueio da sua conta, atualize suas credenciais imediatamente no portal de segurança.</p>
  <div style="text-align: center; margin: 30px 0;">
    <a href="{{LINK_PHISHING}}" style="background-color: ${marca.cor}; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;">Manter minha senha atual</a>
  </div>
  <p style="font-size: 12px; color: #666;">Equipe de TI - ${nomeExibicao}<br>Este é um e-mail automático.</p>
</div>`;
  }
  
  if (ataque.id === 'atividade_suspeita') {
    return `<div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f44336; border-top: 5px solid #f44336;">
  <h2 style="color: #d32f2f;">Alerta de Segurança - ${nomeExibicao}</h2>
  <p>Detectamos um login suspeito na sua conta a partir de um dispositivo não reconhecido.</p>
  <p>Se não foi você, clique no botão abaixo imediatamente para bloquear o acesso e rever a atividade.</p>
  <div style="text-align: center; margin: 30px 0;">
    <a href="{{LINK_PHISHING}}" style="background-color: #d32f2f; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;">Revisar Atividade Recente</a>
  </div>
</div>`;
  }

  return `<div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;">
  <div style="background-color: ${marca.cor}; color: white; padding: 15px; text-align: center;">
    <h2>Comunicado Oficial</h2>
  </div>
  <div style="padding: 20px; background-color: white; border: 1px solid #eee;">
    <p>Olá,</p>
    <p>Um novo documento confidencial foi compartilhado com você via <strong>${nomeExibicao}</strong>.</p>
    <p>Por favor, revise o documento e assine o termo de ciência até o final do dia.</p>
    <div style="text-align: center; margin: 25px 0;">
      <a href="{{LINK_PHISHING}}" style="background-color: ${marca.cor}; color: white; padding: 10px 20px; text-decoration: none; font-weight: bold; border-radius: 5px;">Acessar Documento Seguro</a>
    </div>
  </div>
</div>`;
};
// ----------------------------------------

export default function Templates() {
  const [templates, setTemplates] = useState<Template[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null); 
  
  // Estados do Formulário e do Gerador
  const [novoTemplate, setNovoTemplate] = useState({ nome: '', assunto: '', remetenteNome: '', remetenteEmail: '', corpoHtml: '' });
  const [marcaSelecionada, setMarcaSelecionada] = useState('');
  const [ataqueSelecionado, setAtaqueSelecionado] = useState('');
  const [nomeInterno, setNomeInterno] = useState('Departamento de TI');

  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' | 'info' });

  const filteredTemplates = templates.filter((t) => 
    t.nome.toLowerCase().includes(searchTerm.toLowerCase()) ||
    t.assunto.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const fetchTemplates = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      const response = await fetch('http://localhost:5000/api/Templates', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setTemplates(data);
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

  const handleGerarTemplateDinamico = () => {
    if (!marcaSelecionada || !ataqueSelecionado) {
      showNotify("Selecione uma Marca e um Tipo de Isca primeiro!", "info");
      return;
    }
    
    const marca = marcas.find(m => m.id === marcaSelecionada);
    const ataque = ataques.find(a => a.id === ataqueSelecionado);
    
    if (marca && ataque) {
      const htmlGerado = gerarHtmlDinamico(marca, ataque, nomeInterno);
      
      setNovoTemplate({
        nome: `[Gerador] ${marca.nome} - ${ataque.titulo}`,
        assunto: ataque.assunto,
        remetenteNome: marca.id === 'interna' ? nomeInterno : `Suporte ${marca.nome}`,
        remetenteEmail: marca.remetente,
        corpoHtml: htmlGerado
      });
      showNotify("Isca gerada! Você pode ajustar os detalhes abaixo se quiser.", "success");
    }
  };

  const handleSave = async () => {
    const token = localStorage.getItem('phishguard_token');
    const url = editId ? `http://localhost:5000/api/Templates/${editId}` : 'http://localhost:5000/api/Templates';
    
    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(editId ? { ...novoTemplate, id: editId } : novoTemplate)
      });

      if (response.ok) {
        showNotify(editId ? "Cenário atualizado!" : "Cenário salvo com sucesso!");
        fetchTemplates();
        handleClose();
      } else {
        showNotify("Falha ao salvar. Verifique os dados.", "error");
      }
    } catch (error) {
      showNotify("Erro ao salvar cenário", "error");
    }
  };

  const handleDelete = async (id: string) => { 
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm("Deseja realmente excluir este cenário de phishing?")) {
      try {
        const response = await fetch(`http://localhost:5000/api/Templates/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          showNotify("Cenário removido com sucesso!");
          fetchTemplates();
        }
      } catch (error) {
        showNotify("Erro de rede ao remover cenário.", "error");
      }
    }
  };

  const handleEdit = (template: Template) => {
    setEditId(template.id);
    setNovoTemplate({ 
      nome: template.nome, assunto: template.assunto, remetenteNome: template.remetenteNome,
      remetenteEmail: template.remetenteEmail, corpoHtml: template.corpoHtml
    });
    setOpen(true);
  };

  const handleOpen = () => { 
    setEditId(null); 
    setNovoTemplate({ nome: '', assunto: '', remetenteNome: '', remetenteEmail: '', corpoHtml: '' });
    setMarcaSelecionada('');
    setAtaqueSelecionado('');
    setOpen(true); 
  };
  
  const handleClose = () => { setOpen(false); };

  useEffect(() => { fetchTemplates(); }, []);

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>Biblioteca de Cenários</Typography>
        
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Buscar cenário..."
            size="small"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action" /></InputAdornment>) }}
            sx={{ backgroundColor: 'white', borderRadius: 1 }}
          />

          <Button variant="contained" startIcon={<PostAddIcon />} onClick={handleOpen}>
            Novo Cenário
          </Button>
        </Box>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead sx={{ backgroundColor: '#f8f9fa' }}>
            <TableRow>
              <TableCell><strong>Identificação</strong></TableCell>
              <TableCell><strong>Assunto da Isca</strong></TableCell>
              <TableCell><strong>Remetente Falso</strong></TableCell>
              <TableCell align="right"><strong>Ações</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={4} align="center"><CircularProgress sx={{ m: 2 }} /></TableCell></TableRow>
            ) : (
              filteredTemplates.map((template) => (
                <TableRow key={template.id} hover>
                  <TableCell>{template.nome}</TableCell>
                  <TableCell>{template.assunto}</TableCell>
                  <TableCell>{template.remetenteNome} &lt;{template.remetenteEmail}&gt;</TableCell>
                  <TableCell align="right">
                    <IconButton color="primary" onClick={() => handleEdit(template)}><EditIcon fontSize="small" /></IconButton>
                    <IconButton color="error" onClick={() => handleDelete(template.id)}><DeleteIcon fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
            {!loading && filteredTemplates.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} align="center" sx={{ py: 3 }}>
                  <Typography variant="body1" color="textSecondary">
                    Nenhum cenário cadastrado. Crie sua primeira isca!
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="md">
        <DialogTitle>{editId ? "Editar Cenário" : "Construir Novo Cenário de Phishing"}</DialogTitle>
        <DialogContent dividers>
          
          {!editId && (
            <Box sx={{ mb: 4, p: 3, backgroundColor: '#f0f4f8', borderRadius: 2, border: '1px dashed #b0bec5' }}>
              <Typography variant="subtitle1" color="primary" sx={{ fontWeight: 'bold', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <AutoAwesomeIcon /> Assistente de Criação Rápida
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, sm: marcaSelecionada === 'interna' ? 4 : 5 }}>
                  <TextField select fullWidth label="1. Identidade (Marca)" value={marcaSelecionada} onChange={(e) => setMarcaSelecionada(e.target.value)} size="small">
                    {marcas.map((m) => (<MenuItem key={m.id} value={m.id}>{m.nome}</MenuItem>))}
                  </TextField>
                </Grid>
                
                {marcaSelecionada === 'interna' && (
                  <Grid size={{ xs: 12, sm: 3 }}>
                    <TextField fullWidth label="Nome do Setor" value={nomeInterno} onChange={(e) => setNomeInterno(e.target.value)} size="small" />
                  </Grid>
                )}

                <Grid size={{ xs: 12, sm: marcaSelecionada === 'interna' ? 3 : 5 }}>
                  <TextField select fullWidth label="2. Gatilho Mental (Ataque)" value={ataqueSelecionado} onChange={(e) => setAtaqueSelecionado(e.target.value)} size="small">
                    {ataques.map((a) => (<MenuItem key={a.id} value={a.id}>{a.titulo}</MenuItem>))}
                  </TextField>
                </Grid>
                
                <Grid size={{ xs: 12, sm: 2 }}>
                  <Button variant="contained" color="secondary" fullWidth onClick={handleGerarTemplateDinamico}>
                    Gerar
                  </Button>
                </Grid>
              </Grid>
            </Box>
          )}

          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField fullWidth label="Nome de Identificação Interna" value={novoTemplate.nome} onChange={(e) => setNovoTemplate({...novoTemplate, nome: e.target.value})} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Nome do Remetente Falso" value={novoTemplate.remetenteNome} onChange={(e) => setNovoTemplate({...novoTemplate, remetenteNome: e.target.value})} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="E-mail do Remetente Falso" value={novoTemplate.remetenteEmail} onChange={(e) => setNovoTemplate({...novoTemplate, remetenteEmail: e.target.value})} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField fullWidth label="Assunto do E-mail" value={novoTemplate.assunto} onChange={(e) => setNovoTemplate({...novoTemplate, assunto: e.target.value})} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField 
                fullWidth label="Corpo do E-mail (HTML)" multiline rows={8} 
                value={novoTemplate.corpoHtml} 
                onChange={(e) => setNovoTemplate({...novoTemplate, corpoHtml: e.target.value})} 
                helperText="O placeholder {{LINK_PHISHING}} será substituído automaticamente pela URL maliciosa na hora do disparo."
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle2" gutterBottom sx={{ mt: 2, color: 'text.secondary' }}>
                Visualização Prévia (Como o alvo verá o e-mail):
              </Typography>
              <Paper 
                variant="outlined" 
                sx={{ 
                  p: 2, backgroundColor: '#fff', 
                  minHeight: '200px', maxHeight: '400px', overflowY: 'auto',
                  border: '2px solid #e0e0e0', borderRadius: 2
                }}
              >
                <div dangerouslySetInnerHTML={{ __html: novoTemplate.corpoHtml.replace('{{LINK_PHISHING}}', '#') }} />
              </Paper>
            </Grid>
          </Grid>

        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleClose} color="inherit">Cancelar</Button>
          <Button onClick={handleSave} variant="contained" size="large">Salvar Cenário</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={notify.open} autoHideDuration={4000} onClose={() => setNotify({ ...notify, open: false })} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={notify.type} variant="filled" sx={{ width: '100%' }}>{notify.message}</Alert>
      </Snackbar>
    </Box>
  );
}