import { useState, useEffect } from 'react';
import {
  Box, Button, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, IconButton, Dialog,
  DialogTitle, DialogContent, DialogActions, TextField, Stack,
  Snackbar, Alert, CircularProgress, InputAdornment, Grid, MenuItem, Chip
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import PostAddIcon from '@mui/icons-material/PostAdd';
import SearchIcon from '@mui/icons-material/Search';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import { templatesPredefinidos } from '../data/predefinedTemplates';

// Biblioteca de Cenários (templates de e-mail) — fluxo simplificado do MVP.
//
// Mesma estratégia das Armadilhas: sem criação por código livre. Foi removido o
// "Assistente Rápido" (geração dinâmica marca+ataque) e o TextArea de HTML bruto.
// O administrador apenas seleciona uma ISCA OFICIAL homologada; a campanha persiste
// somente o IDENTIFICADOR da isca (ex.: 'amazon-notificacao-geral') no campo
// corpoHtml — nunca a string massiva de HTML. O backend (CampaignsController.
// IniciarDisparo) resolve esse ID de volta para o HTML real no momento do disparo.

const API_ENDPOINT = 'http://localhost:5000/api/Templates';

// Iscas oficiais disponíveis no seletor.
const iscasOficiais = templatesPredefinidos;

interface Template {
  id: string;
  nome: string;
  assunto: string;
  remetenteNome: string;
  remetenteEmail: string;
  corpoHtml: string; // Passa a carregar o ID da isca (não mais HTML bruto).
}

export default function Templates() {
  const [templates, setTemplates] = useState<Template[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);

  // Valor atual do seletor (ainda não confirmado).
  const [iscaSelecionada, setIscaSelecionada] = useState('');
  // Isca efetivamente carregada — dirige o preview e o salvamento.
  const [iscaCarregada, setIscaCarregada] = useState('');
  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' | 'info' });

  const iscaAtiva = iscasOficiais.find((i) => i.id === iscaCarregada);

  const filteredTemplates = templates.filter((t) =>
    t.nome.toLowerCase().includes(searchTerm.toLowerCase()) ||
    t.assunto.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const showNotify = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
    setNotify({ open: true, message, type });
  };

  const fetchTemplates = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      const response = await fetch(API_ENDPOINT, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setTemplates(data);
      }
    } catch (error) {
      showNotify('Erro ao conectar com o servidor', 'error');
    } finally {
      setLoading(false);
    }
  };

  // "Carregar Isca": apenas confirma no estado local a isca escolhida no seletor.
  const handleCarregarIsca = () => {
    if (!iscaSelecionada) {
      showNotify('Escolha uma isca oficial na lista antes de carregar.', 'info');
      return;
    }
    setIscaCarregada(iscaSelecionada);
    showNotify('Isca carregada! Confira o preview ao lado.', 'success');
  };

  const handleSave = async () => {
    const isca = iscasOficiais.find((i) => i.id === iscaCarregada);
    if (!isca) {
      showNotify('Selecione e carregue uma isca oficial antes de salvar.', 'error');
      return;
    }

    const token = localStorage.getItem('phishguard_token');
    const url = editId ? `${API_ENDPOINT}/${editId}` : API_ENDPOINT;

    // Persistência via identificador: envia os metadados curtos (nome/assunto/
    // remetente) + apenas o ID da isca em corpoHtml — nunca a string de HTML.
    const payload = {
      nome: isca.nome,
      assunto: isca.assunto,
      remetenteNome: isca.remetenteNome,
      remetenteEmail: isca.remetenteEmail,
      corpoHtml: isca.id,
    };

    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(editId ? { ...payload, id: editId } : payload)
      });

      if (response.ok) {
        showNotify(editId ? 'Cenário atualizado!' : 'Cenário salvo com sucesso!');
        fetchTemplates();
        handleClose();
      } else {
        showNotify('Falha ao salvar. Verifique os dados.', 'error');
      }
    } catch (error) {
      showNotify('Erro ao salvar cenário', 'error');
    }
  };

  const handleDelete = async (id: string) => {
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm('Deseja realmente excluir este cenário de phishing?')) {
      try {
        const response = await fetch(`${API_ENDPOINT}/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          showNotify('Cenário removido com sucesso!');
          fetchTemplates();
        }
      } catch (error) {
        showNotify('Erro de rede ao remover cenário.', 'error');
      }
    }
  };

  const handleEdit = (template: Template) => {
    setEditId(template.id);
    // Registros novos guardam o ID da isca; se casar com uma isca conhecida,
    // pré-seleciona (registros legados com HTML bruto ficam sem seleção).
    const id = iscasOficiais.some((i) => i.id === template.corpoHtml) ? template.corpoHtml : '';
    setIscaSelecionada(id);
    setIscaCarregada(id);
    setOpen(true);
  };

  const handleOpen = () => {
    setEditId(null);
    setIscaSelecionada('');
    setIscaCarregada('');
    setOpen(true);
  };

  const handleClose = () => { setOpen(false); };

  useEffect(() => { fetchTemplates(); }, []);

  // Preview do e-mail em iframe isolado (o HTML das iscas traz <head>/<style>
  // próprios). Placeholders de disparo são neutralizados para a visualização.
  const previewSrcDoc = iscaAtiva
    ? iscaAtiva.corpoHtml.replaceAll('{{LINK_PHISHING}}', '#').replaceAll('{{LINK}}', '#').replaceAll('{{NOME}}', 'Colaborador(a)')
    : '<div style="padding: 24px; color: #999; font-family: sans-serif;">Selecione uma isca oficial e clique em "Carregar Isca" para visualizar o e-mail.</div>';

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

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="lg">
        <DialogTitle>{editId ? 'Editar Cenário' : 'Novo Cenário de Phishing'}</DialogTitle>
        <DialogContent dividers>

          <Grid container spacing={3}>
            {/* Coluna esquerda: seleção da isca oficial (fluxo único). */}
            <Grid size={{ xs: 12, md: 5 }}>
              <Box sx={{ p: 3, backgroundColor: '#f0f4f8', borderRadius: 2, border: '1px dashed #b0bec5' }}>
                <Typography variant="subtitle1" color="primary" sx={{ fontWeight: 'bold', mb: 0.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <MenuBookIcon /> Carregar Isca Oficial
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Escolha uma isca homologada da plataforma. A campanha persiste apenas o identificador da isca.
                </Typography>

                <TextField
                  select fullWidth label="Escolha a Isca"
                  value={iscaSelecionada}
                  onChange={(e) => setIscaSelecionada(e.target.value)}
                  size="small"
                  sx={{ mb: 2, backgroundColor: 'white' }}
                >
                  {iscasOficiais.map((i) => (
                    <MenuItem key={i.id} value={i.id}>{i.categoria} — {i.nome}</MenuItem>
                  ))}
                </TextField>

                <Button variant="contained" color="secondary" fullWidth onClick={handleCarregarIsca}>
                  Carregar Isca
                </Button>
              </Box>

              {iscaAtiva && (
                <Paper variant="outlined" sx={{ mt: 3, p: 2.5, borderRadius: 2 }}>
                  <Stack direction="row" spacing={1} alignItems="center" sx={{ flexWrap: 'wrap', gap: 1, mb: 1.5 }}>
                    <Typography variant="body1" sx={{ fontWeight: 600 }}>{iscaAtiva.nome}</Typography>
                    <Chip label={iscaAtiva.categoria} size="small" color="primary" variant="outlined" />
                  </Stack>
                  <Typography variant="body2" color="text.secondary"><strong>Assunto:</strong> {iscaAtiva.assunto}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                    <strong>Remetente:</strong> {iscaAtiva.remetenteNome} &lt;{iscaAtiva.remetenteEmail}&gt;
                  </Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1.5 }}>
                    Identificador enviado ao backend: <strong>{iscaAtiva.id}</strong>
                  </Typography>
                </Paper>
              )}
            </Grid>

            {/* Coluna direita: preview do e-mail (visão do alvo). */}
            <Grid size={{ xs: 12, md: 7 }}>
              <Typography variant="subtitle2" gutterBottom sx={{ color: 'text.secondary' }}>
                Visualização Prévia (Como o alvo verá o e-mail):
              </Typography>
              <Paper
                variant="outlined"
                sx={{ height: '520px', backgroundColor: '#fff', border: '2px solid #e0e0e0', borderRadius: 2, overflow: 'hidden' }}
              >
                <iframe
                  title="Email Preview"
                  srcDoc={previewSrcDoc}
                  style={{ width: '100%', height: '100%', border: 'none' }}
                />
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
