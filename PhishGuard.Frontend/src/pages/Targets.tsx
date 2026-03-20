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
  Search as SearchIcon,
  Send as SendIcon
} from '@mui/icons-material';
import Papa from 'papaparse'; 

interface Target {
  id: string; 
  nome: string;
  email: string;
  departamento: string; 
}

export default function Targets() {
  const [targets, setTargets] = useState<Target[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null); 
  const [novoAlvo, setNewTarget] = useState({ nome: '', email: '', departamento: '' });
  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as 'success' | 'error' | 'info' });  
  const fileInputRef = useRef<HTMLInputElement>(null);

  const filteredTargets = targets.filter((target) => 
    target.nome.toLowerCase().includes(searchTerm.toLowerCase()) ||
    target.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (target.departamento || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const fetchTargets = async () => {
    setLoading(true);
    const token = localStorage.getItem('phishguard_token');
    try {
      // 1. URL ATUALIZADA PARA TARGETS
      const response = await fetch('http://localhost:5000/api/Targets', {
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

  const showNotify = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
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
            // 2. URL ATUALIZADA PARA TARGETS
            await fetch('http://localhost:5000/api/Targets', {
              method: 'POST',
              headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` 
              },
              body: JSON.stringify({
                nome: item.nome,
                email: item.email,
                departamento: item.departamento || item.setor || 'Geral' 
              })
            });
            sucessos++;
          } catch (err) {
            console.error("Falha ao importar linha", item);
          }
        }
        setLoading(false);
        showNotify(`${sucessos} alvos importados com sucesso!`);
        fetchTargets(); 
      }
    });

    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handleSave = async () => {
    const token = localStorage.getItem('phishguard_token');
    // 3. URLs ATUALIZADAS PARA TARGETS
    const url = editId 
      ? `http://localhost:5000/api/Targets/${editId}` 
      : 'http://localhost:5000/api/Targets';
    
    try {
      const response = await fetch(url, {
        method: editId ? 'PUT' : 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify(editId ? { ...novoAlvo, id: editId } : novoAlvo)
      });

      if (response.ok) {
        showNotify(editId ? "Alvo atualizado!" : "Alvo cadastrado com sucesso!");
        fetchTargets();
        handleClose();
      } else {
        showNotify("Falha ao salvar. Verifique os dados.", "error");
      }
    } catch (error) {
      showNotify("Erro ao salvar alvo", "error");
    }
  };

  const handleDelete = async (id: string) => { 
    const token = localStorage.getItem('phishguard_token');
    if (window.confirm("Deseja realmente excluir este alvo?")) {
      try {
        const response = await fetch(`http://localhost:5000/api/Targets/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (response.ok) {
          showNotify("Alvo removido com sucesso!");
          fetchTargets();
        } else {
          showNotify("Falha ao remover o alvo.", "error");
        }
      } catch (error) {
        showNotify("Erro de rede ao remover alvo.", "error");
      }
    }
  };

  const handleEdit = (target: Target) => {
    setEditId(target.id);
    setNewTarget({ nome: target.nome, email: target.email, departamento: target.departamento });
    setOpen(true);
  };

  const handleOpen = () => { setEditId(null); setOpen(true); };
  const handleClose = () => { setOpen(false); setNewTarget({ nome: '', email: '', departamento: '' }); };

  const handleTestTarget = async (emailDestino: string) => {
    showNotify(`Enviando e-mail de teste para ${emailDestino}...`, "info");
    const token = localStorage.getItem('phishguard_token');
    
    try {
      const response = await fetch('http://localhost:5000/api/SmtpConfig/Testar', {
        method: 'POST',
        headers: { 
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ emailDestino }) 
      });

      if (response.ok) {
        showNotify(`Disparo bem-sucedido para ${emailDestino}!`, "success");
      } else {
        const errorText = await response.text();
        showNotify(`Falha no envio: ${errorText}`, "error");
      }
    } catch (error) {
      showNotify("Erro de rede ao tentar contatar o servidor.", "error");
    }
  };

  useEffect(() => { fetchTargets(); }, []);

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
          
          <Tooltip title="CSV deve conter as colunas: nome, email, departamento">
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
              <TableCell><strong>Departamento</strong></TableCell>
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
                  <TableCell>{target.departamento}</TableCell>
                  
                  <TableCell align="right">
                    <Tooltip title="Testar Disparo para este Alvo">
                      <IconButton color="secondary" onClick={() => handleTestTarget(target.email)}>
                        <SendIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
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
          <TextField margin="normal" fullWidth label="Nome Completo" value={novoAlvo.nome} onChange={(e) => setNewTarget({...novoAlvo, nome: e.target.value})} />
          <TextField margin="normal" fullWidth label="E-mail Corporativo" value={novoAlvo.email} onChange={(e) => setNewTarget({...novoAlvo, email: e.target.value})} />
          <TextField margin="normal" fullWidth label="Departamento" value={novoAlvo.departamento} onChange={(e) => setNewTarget({...novoAlvo, departamento: e.target.value})} />
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