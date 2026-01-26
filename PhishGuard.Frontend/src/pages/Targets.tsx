import { useState, useEffect, useRef } from 'react';
import { 
  Box, Button, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, IconButton, Dialog, 
  DialogTitle, DialogContent, DialogActions, TextField, Stack,
  Snackbar, Alert, CircularProgress, Tooltip, InputAdornment
} from '@mui/material';
import { 
  Delete as DeleteIcon, 
  Edit as EditIcon, 
  PersonAdd as PersonAddIcon,
  FileUpload as UploadIcon,
  Search as SearchIcon 
} from '@mui/icons-material';
import Papa from 'papaparse'; 

interface Target {
  id: number;
  nome: string;
  email: string;
  setor: string;
}

export default function Targets() {
  const [targets, setTargets] = useState<Target[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [novoAlvo, setNovoAlvo] = useState({ nome: '', email: '', setor: '' });
  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' });
  const fileInputRef = useRef<HTMLInputElement>(null);

  const filteredTargets = targets.filter((target) => 
    target.nome.toLowerCase().includes(searchTerm.toLowerCase()) ||
    target.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (target.setor || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const fetchAlvos = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      const response = await fetch('http://localhost:5000/api/Alvos', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setTargets(data);
      }
    } catch (error) {
      showNotify("Erro ao conectar com o servidor", "error");
    } finally {
      setLoading(false);
    }
  };

  const showNotify = (message: string, type: 'success' | 'error' = 'success') => {
    setNotify({ open: true, message, type });
  };

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    Papa.parse(file, {
      header: true, 
      skipEmptyLines: true,
      complete: async (results: any) => {
        const dadosCSV = results.data;
        
        if (!dadosCSV || dadosCSV.length === 0 || !dadosCSV[0].nome || !dadosCSV[0].email) {
          showNotify("CSV Inválido ou Vazio. Colunas necessárias: nome, email", "error");
          return;
        }

        let sucessos = 0;
        const token = localStorage.getItem('phishguard_token');

        setLoading(true);
        for (const item of dadosCSV) {
          try {
            await fetch('http://localhost:5000/api/Alvos', {
              method: 'POST',
              headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` 
              },
              body: JSON.stringify({
                nome: item.nome,
                email: item.email,
                setor: item.setor || 'Geral' 
              })
            });
            sucessos++;
          } catch (err) {
            console.error("Falha ao importar linha", item);
          }
        }
        setLoading(false);
        showNotify(`${sucessos} alvos importados com sucesso!`);
        fetchAlvos(); 
      }
    });

    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handleSave = async () => {
    const token = localStorage.getItem('phishguard_token');
    const url = editId 
      ? `http://localhost:5000/api/Alvos/${editId}` 
      : 'http://localhost:5000/api/Alvos';
    
    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify(novoAlvo)
      });

      if (response.ok) {
        showNotify(editId ? "Alvo atualizado!" : "Alvo cadastrado com sucesso!");
        fetchAlvos();
        handleClose();
      }
    } catch (error) {
      showNotify("Erro ao salvar alvo", "error");
    }
  };

  const handleDelete = async (id: number) => {
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm("Deseja realmente excluir este alvo?")) {
      await fetch(`http://localhost:5000/api/Alvos/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      showNotify("Alvo removido");
      fetchAlvos();
    }
  };

  const handleEdit = (target: Target) => {
    setEditId(target.id);
    setNovoAlvo({ nome: target.nome, email: target.email, setor: target.setor });
    setOpen(true);
  };

  const handleOpen = () => { setEditId(null); setOpen(true); };
  const handleClose = () => { setOpen(false); setNovoAlvo({ nome: '', email: '', setor: '' }); };

  useEffect(() => { fetchAlvos(); }, []);

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>Gestão de Alvos</Typography>
        
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          
          <TextField
            placeholder="Buscar..."
            size="small"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon color="action" />
                </InputAdornment>
              ),
            }}
            sx={{ backgroundColor: 'white', borderRadius: 1 }}
          />

          <input
            type="file"
            accept=".csv"
            hidden
            ref={fileInputRef}
            onChange={handleFileUpload}
          />
          
          <Tooltip title="CSV deve conter: nome, email, setor">
            <Button 
              variant="outlined" 
              startIcon={<UploadIcon />}
              onClick={() => fileInputRef.current?.click()}
            >
              Importar CSV
            </Button>
          </Tooltip>

          <Button variant="contained" startIcon={<PersonAddIcon />} onClick={handleOpen}>
            Novo Alvo
          </Button>
        </Box>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead sx={{ backgroundColor: '#f8f9fa' }}>
            <TableRow>
              <TableCell><strong>Nome</strong></TableCell>
              <TableCell><strong>E-mail</strong></TableCell>
              <TableCell><strong>Setor</strong></TableCell>
              <TableCell align="right"><strong>Ações</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={4} align="center"><CircularProgress sx={{ m: 2 }} /></TableCell></TableRow>
            ) : (
              filteredTargets.map((target) => (
                <TableRow key={target.id} hover>
                  <TableCell>{target.nome}</TableCell>
                  <TableCell>{target.email}</TableCell>
                  <TableCell>{target.setor}</TableCell>
                  <TableCell align="right">
                    <IconButton color="primary" onClick={() => handleEdit(target)}><EditIcon fontSize="small" /></IconButton>
                    <IconButton color="error" onClick={() => handleDelete(target.id)}><DeleteIcon fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
            {!loading && filteredTargets.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} align="center" sx={{ py: 3 }}>
                  <Typography variant="body1" color="textSecondary">
                    Nenhum alvo encontrado.
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose} fullWidth maxWidth="xs">
        <DialogTitle>{editId ? "Editar Alvo" : "Adicionar Novo Alvo"}</DialogTitle>
        <DialogContent>
          <TextField margin="normal" fullWidth label="Nome Completo" value={novoAlvo.nome} onChange={(e) => setNovoAlvo({...novoAlvo, nome: e.target.value})} />
          <TextField margin="normal" fullWidth label="E-mail Corporativo" value={novoAlvo.email} onChange={(e) => setNovoAlvo({...novoAlvo, email: e.target.value})} />
          <TextField margin="normal" fullWidth label="Setor" value={novoAlvo.setor} onChange={(e) => setNovoAlvo({...novoAlvo, setor: e.target.value})} />
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={handleClose} color="inherit">Cancelar</Button>
          <Button onClick={handleSave} variant="contained">Salvar</Button>
        </DialogActions>
      </Dialog>

      <Snackbar 
        open={notify.open} 
        autoHideDuration={4000} 
        onClose={() => setNotify({ ...notify, open: false })}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert severity={notify.type} variant="filled" sx={{ width: '100%' }}>
          {notify.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}