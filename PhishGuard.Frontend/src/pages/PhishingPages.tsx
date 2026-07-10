import { useState, useEffect } from 'react';
import {
  Box, Button, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, IconButton, Dialog,
  DialogTitle, DialogContent, DialogActions, TextField, Stack,
  Snackbar, Alert, CircularProgress, InputAdornment, Grid, MenuItem, Chip
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SearchIcon from '@mui/icons-material/Search';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import AddIcon from '@mui/icons-material/Add';
import { landingTemplates } from '../data/landingTemplates';

// Tela de Páginas Simuladas (Armadilhas) — fluxo simplificado do MVP.
//
// Diferente das Páginas Educacionais (que ainda usam o editor livre em
// HtmlEditorView), aqui NÃO existe entrada de código livre. O administrador
// apenas seleciona um Molde Oficial Homologado e a campanha persiste somente o
// IDENTIFICADOR do molde (ex.: 'hbomax-redefinicao-senha') — nunca a string
// massiva de HTML. O LandingPage.tsx resolve esse ID de volta para o HTML no
// momento em que a armadilha é servida (/landing/:id).

const API_ENDPOINT = 'http://localhost:5000/api/PhishingPages';

// Moldes oficiais homologados disponíveis no dropdown "Carregar Molde Oficial".
const moldesOficiais = landingTemplates;

interface PageRecord {
  id: string;
  nome: string;
  conteudoHtml: string; // Passa a carregar o ID do molde (não mais HTML bruto).
}

export default function PhishingPages() {
  const [pages, setPages] = useState<PageRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);

  // Valor atual do dropdown (ainda não confirmado).
  const [moldeSelecionado, setMoldeSelecionado] = useState('');
  // Molde efetivamente carregado — dirige o preview e o salvamento.
  const [moldeCarregado, setMoldeCarregado] = useState('');
  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' | 'info' });

  const filteredPages = pages.filter((p) =>
    p.nome.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const moldeAtivo = moldesOficiais.find((t) => t.id === moldeCarregado);

  const showNotify = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
    setNotify({ open: true, message, type });
  };

  const fetchPages = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      const response = await fetch(API_ENDPOINT, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setPages(data);
      }
    } catch (error) {
      showNotify('Erro ao conectar com o servidor', 'error');
    } finally {
      setLoading(false);
    }
  };

  // "Carregar HTML": apenas confirma no estado local o molde escolhido no dropdown.
  const handleCarregarMolde = () => {
    if (!moldeSelecionado) {
      showNotify('Escolha um molde oficial na lista antes de carregar.', 'info');
      return;
    }
    setMoldeCarregado(moldeSelecionado);
    showNotify('Molde carregado! Confira o preview ao lado.', 'success');
  };

  const handleSave = async () => {
    // Sem molde carregado não há o que persistir.
    const molde = moldesOficiais.find((t) => t.id === moldeCarregado);
    if (!molde) {
      showNotify('Selecione e carregue um molde oficial antes de salvar.', 'error');
      return;
    }

    const token = localStorage.getItem('phishguard_token');
    const url = editId ? `${API_ENDPOINT}/${editId}` : API_ENDPOINT;

    // Persistência simplificada: envia apenas o identificador do molde, não o HTML.
    const payload = {
      nome: molde.nome,
      conteudoHtml: molde.id,
    };

    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(editId ? { ...payload, id: editId } : payload)
      });

      if (response.ok) {
        showNotify('Armadilha salva com sucesso!');
        fetchPages();
        handleClose();
      } else {
        showNotify('Falha ao salvar. Verifique os dados.', 'error');
      }
    } catch (error) {
      showNotify('Erro ao salvar registro', 'error');
    }
  };

  const handleDelete = async (id: string) => {
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm('Deseja realmente excluir esta armadilha?')) {
      try {
        const response = await fetch(`${API_ENDPOINT}/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          showNotify('Registro removido com sucesso!');
          fetchPages();
        }
      } catch (error) {
        showNotify('Erro ao remover.', 'error');
      }
    }
  };

  const handleEdit = (page: PageRecord) => {
    setEditId(page.id);
    // Registros novos guardam o ID do molde; se casar com um molde conhecido,
    // pré-seleciona no dropdown (registros legados de HTML bruto ficam vazios).
    const id = moldesOficiais.some((t) => t.id === page.conteudoHtml) ? page.conteudoHtml : '';
    setMoldeSelecionado(id);
    setMoldeCarregado(id);
    setOpen(true);
  };

  const handleOpen = () => {
    setEditId(null);
    setMoldeSelecionado('');
    setMoldeCarregado('');
    setOpen(true);
  };

  const handleClose = () => { setOpen(false); };

  useEffect(() => { fetchPages(); }, []);

  // Preview: renderiza a captura estática do molde. Os placeholders de telemetria
  // ({{CAMPAIGN_ID}}/{{TARGET_ID}}) são neutralizados para não vazarem na visão.
  const previewSrcDoc = moldeAtivo
    ? moldeAtivo.html.replace(/{{CAMPAIGN_ID}}/g, '').replace(/{{TARGET_ID}}/g, '')
    : '<div style="padding: 24px; color: #999; font-family: sans-serif;">Selecione um molde oficial e clique em "Carregar HTML" para visualizar a armadilha.</div>';

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>Páginas Simuladas (Armadilhas)</Typography>

        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Buscar..." size="small" value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action" /></InputAdornment>) }}
            sx={{ backgroundColor: 'white', borderRadius: 1 }}
          />
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpen}>
            Novo Registro
          </Button>
        </Box>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead sx={{ backgroundColor: '#f8f9fa' }}>
            <TableRow>
              <TableCell><strong>Identificação</strong></TableCell>
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
                    Nenhuma página de captura cadastrada. Crie a sua primeira armadilha!
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="lg">
        <DialogTitle>{editId ? 'Editar Armadilha' : 'Nova Armadilha'}</DialogTitle>
        <DialogContent dividers>

          <Grid container spacing={3}>
            {/* Coluna esquerda: seleção do molde oficial (fluxo único). */}
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ p: 3, backgroundColor: '#f0f4f8', borderRadius: 2, border: '1px dashed #b0bec5' }}>
                <Typography variant="subtitle1" color="primary" sx={{ fontWeight: 'bold', mb: 0.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AutoAwesomeIcon /> Carregar Molde Oficial
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Escolha um molde homologado do ecossistema. A campanha persiste apenas o identificador do molde.
                </Typography>

                <TextField
                  select fullWidth label="Escolha a Interface"
                  value={moldeSelecionado}
                  onChange={(e) => setMoldeSelecionado(e.target.value)}
                  size="small"
                  sx={{ mb: 2, backgroundColor: 'white' }}
                >
                  {moldesOficiais.map((t) => (<MenuItem key={t.id} value={t.id}>{t.nome}</MenuItem>))}
                </TextField>

                <Button variant="contained" color="secondary" fullWidth onClick={handleCarregarMolde}>
                  Carregar HTML
                </Button>
              </Box>

              {moldeAtivo && (
                <Paper variant="outlined" sx={{ mt: 3, p: 2.5, borderRadius: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                    Molde selecionado
                  </Typography>
                  <Stack direction="row" spacing={1} alignItems="center" sx={{ flexWrap: 'wrap', gap: 1 }}>
                    <Typography variant="body1" sx={{ fontWeight: 600 }}>{moldeAtivo.nome}</Typography>
                    {moldeAtivo.categoria && <Chip label={moldeAtivo.categoria} size="small" color="primary" variant="outlined" />}
                  </Stack>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                    Identificador enviado ao backend: <strong>{moldeAtivo.id}</strong>
                  </Typography>
                </Paper>
              )}
            </Grid>

            {/* Coluna direita: preview em tempo real da armadilha. */}
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle2" gutterBottom sx={{ color: 'text.secondary' }}>
                Preview em Tempo Real (Visão do Usuário):
              </Typography>
              <Paper
                variant="outlined"
                sx={{ height: '560px', backgroundColor: '#fff', border: '2px solid #e0e0e0', borderRadius: 2, overflow: 'hidden' }}
              >
                <iframe
                  title="Live Preview"
                  srcDoc={previewSrcDoc}
                  style={{ width: '100%', height: '100%', border: 'none' }}
                />
              </Paper>
            </Grid>
          </Grid>

        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleClose} color="inherit">Cancelar</Button>
          <Button onClick={handleSave} variant="contained" size="large">Salvar Registro</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={notify.open} autoHideDuration={4000} onClose={() => setNotify({ ...notify, open: false })} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={notify.type} variant="filled" sx={{ width: '100%' }}>{notify.message}</Alert>
      </Snackbar>
    </Box>
  );
}
