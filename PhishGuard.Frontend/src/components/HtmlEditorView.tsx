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
  Code as CodeIcon
} from '@mui/icons-material';

export interface TemplateModel {
  id: string;
  nome: string;
  html: string;
}

interface HtmlEditorViewProps {
  title: string;
  apiEndpoint: string;
  emptyMessage: string;
  defaultTemplates: TemplateModel[];
}

interface PageRecord {
  id: string;
  nome: string;
  conteudoHtml: string;
}

export default function HtmlEditorView({ title, apiEndpoint, emptyMessage, defaultTemplates }: HtmlEditorViewProps) {
  const [pages, setPages] = useState<PageRecord[]>([]);
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
      const response = await fetch(apiEndpoint, {
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
    if (!templateSelecionado) return;
    const tpl = defaultTemplates.find(t => t.id === templateSelecionado);
    if (tpl) {
      setNovaPagina({ nome: `[Molde] ${tpl.nome}`, conteudoHtml: tpl.html });
      showNotify("Código carregado! Você pode ajustar o HTML abaixo.", "success");
    }
  };

  const handleSave = async () => {
    const token = localStorage.getItem('phishguard_token');
    const url = editId ? `${apiEndpoint}/${editId}` : apiEndpoint;

    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(editId ? { ...novaPagina, id: editId } : novaPagina)
      });

      if (response.ok) {
        showNotify("Página salva com sucesso!");
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
    if (window.confirm("Deseja realmente excluir este registro?")) {
      try {
        const response = await fetch(`${apiEndpoint}/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          showNotify("Registro removido com sucesso!");
          fetchPages();
        }
      } catch (error) {
        showNotify("Erro ao remover.", "error");
      }
    }
  };

  const handleEdit = (page: PageRecord) => {
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

  useEffect(() => { fetchPages(); }, [apiEndpoint]);

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>{title}</Typography>

        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Buscar..." size="small" value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action" /></InputAdornment>) }}
            sx={{ backgroundColor: 'white', borderRadius: 1 }}
          />
          <Button variant="contained" startIcon={<CodeIcon />} onClick={handleOpen}>
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
                  <Typography variant="body1" color="textSecondary">{emptyMessage}</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="lg">
        <DialogTitle>{editId ? `Editar - ${title}` : `Construir - ${title}`}</DialogTitle>
        <DialogContent dividers>

          {!editId && defaultTemplates.length > 0 && (
            <Box sx={{ mb: 4, p: 3, backgroundColor: '#f0f4f8', borderRadius: 2, border: '1px dashed #b0bec5' }}>
              <Typography variant="subtitle1" color="primary" sx={{ fontWeight: 'bold', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <AutoAwesomeIcon /> Carregar Molde Oficial
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, sm: 9 }}>
                  <TextField select fullWidth label="Escolha a Interface" value={templateSelecionado} onChange={(e) => setTemplateSelecionado(e.target.value)} size="small">
                    {defaultTemplates.map((t) => (<MenuItem key={t.id} value={t.id}>{t.nome}</MenuItem>))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm:3}}>
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
                sx={{ fontFamily: 'monospace' }}
              />
            </Grid>

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
                  srcDoc={novaPagina.conteudoHtml || '<div style="padding: 20px; color: #999; font-family: sans-serif;">O preview aparecerá aqui...</div>'}
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