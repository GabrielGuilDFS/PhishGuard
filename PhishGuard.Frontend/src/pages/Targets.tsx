import { useState, useEffect, useRef } from 'react';
import { API_BASE } from '../config';
import { authFetch, getToken } from '../auth/session';
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
import PageContainer from '../components/PageContainer';

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
  const [emailError, setEmailError] = useState<string | null>(null);
  // Cota de alvos do plano ativo do Tenant (Bronze: 50, Prata: 500).
  const [limiteAlvos, setLimiteAlvos] = useState<number | null>(null);
  const [planoNome, setPlanoNome] = useState<string>('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Trava de UI: bloqueia o cadastro quando o total de alvos atinge o limite do plano.
  const limiteAtingido = limiteAlvos !== null && targets.length >= limiteAlvos;

  const filteredTargets = targets.filter((target) => 
    target.nome.toLowerCase().includes(searchTerm.toLowerCase()) ||
    target.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (target.departamento || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const fetchTargets = async () => {
    setLoading(true);
    const token = getToken();
    try {
      // 1. URL ATUALIZADA PARA TARGETS
      const response = await authFetch(`${API_BASE}/Targets`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setTargets(data);
      }
    } catch {
      showNotify("Erro ao conectar com o servidor", "error");
    } finally {
      setLoading(false);
    }
  };

  const showNotify = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
    setNotify({ open: true, message, type });
  };

  // Consulta a cota do plano ativo para saber o limite de alvos permitido.
  const fetchQuota = async () => {
    const token = getToken();
    try {
      const response = await authFetch(`${API_BASE}/Tenant/quota`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setLimiteAlvos(data.limiteAlvos);
        setPlanoNome(data.plano);
      }
    } catch {
      // Silencioso: sem a cota, a UI apenas não aplica a trava preventiva.
    }
  };

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    Papa.parse(file, {
      header: true, 
      skipEmptyLines: true,
      complete: async (results: Papa.ParseResult<Record<string, string>>) => {
        const dadosCSV = results.data;
        
        if (!dadosCSV || dadosCSV.length === 0 || !dadosCSV[0].nome || !dadosCSV[0].email) {
          showNotify("CSV Inválido ou Vazio. Colunas necessárias: nome, email", "error");
          return;
        }

        let sucessos = 0;
        const token = getToken();

        setLoading(true);
        for (const item of dadosCSV) {
          try {
            // 2. URL ATUALIZADA PARA TARGETS
            await authFetch(`${API_BASE}/Targets`, {
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
          } catch {
            console.error("Falha ao importar linha", item);
          }
        }
        setLoading(false);
        showNotify(`${sucessos} alvos importados com sucesso!`);
        fetchTargets();
        fetchQuota();
      }
    });

    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handleSave = async () => {
    setEmailError(null);
    const token = getToken();
    // 3. URLs ATUALIZADAS PARA TARGETS
    const url = editId
      ? `${API_BASE}/Targets/${editId}`
      : `${API_BASE}/Targets`;

    try {
      const response = await authFetch(url, {
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
        fetchQuota();
        handleClose();
      } else if (response.status === 400) {
        const raw = await response.text();
        let mensagem = "Falha ao salvar. Verifique os dados.";
        try {
          // Erros de validação de modelo chegam como JSON ({ errors: { Email: [...] } }).
          const data = JSON.parse(raw);
          const errosEmail = data?.errors?.Email ?? data?.Email;
          if (Array.isArray(errosEmail) && errosEmail.length > 0) {
            setEmailError(errosEmail[0]);
          } else if (typeof data === 'string') {
            mensagem = data;
          }
        } catch {
          // Corpo em texto puro (ex.: mensagem de limite de plano atingido).
          if (raw) mensagem = raw;
        }
        showNotify(mensagem, "error");
      } else {
        showNotify("Falha ao salvar. Verifique os dados.", "error");
      }
    } catch {
      showNotify("Erro ao salvar alvo", "error");
    }
  };

  const handleDelete = async (id: string) => { 
    const token = getToken();
    if (window.confirm("Deseja realmente excluir este alvo?")) {
      try {
        const response = await authFetch(`${API_BASE}/Targets/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (response.ok) {
          showNotify("Alvo removido com sucesso!");
          fetchTargets();
          fetchQuota();
        } else {
          showNotify("Falha ao remover o alvo.", "error");
        }
      } catch {
        showNotify("Erro de rede ao remover alvo.", "error");
      }
    }
  };

  const handleEdit = (target: Target) => {
    setEditId(target.id);
    setNewTarget({ nome: target.nome, email: target.email, departamento: target.departamento });
    setEmailError(null);
    setOpen(true);
  };

  const handleOpen = () => {
    if (limiteAtingido) {
      showNotify(`Limite de alvos do plano ${planoNome} atingido.`, "error");
      return;
    }
    setEditId(null); setEmailError(null); setOpen(true);
  };
  const handleClose = () => {
    setOpen(false);
    setNewTarget({ nome: '', email: '', departamento: '' });
    setEmailError(null);
  };

  const handleTestTarget = async (targetId: string, emailDestino: string) => {
    showNotify(`Enviando e-mail de teste para ${emailDestino}...`, "info");
    const token = getToken();
    
    try {
      const response = await authFetch(`${API_BASE}/SmtpConfig/Testar`, {
        method: 'POST',
        headers: { 
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ targetId }) 
      });

      if (response.ok) {
        showNotify(`Disparo bem-sucedido para ${emailDestino}!`, "success");
      } else {
        const errorText = await response.text();
        showNotify(`Falha no envio: ${errorText}`, "error");
      }
    } catch {
      showNotify("Erro de rede ao tentar contatar o servidor.", "error");
    }
  };

  // Carga inicial: roda uma unica vez na montagem. `fetchTargets`/`fetchQuota` sao
  // recriadas a cada render, entao inclui-las nas deps refaria a busca em loop.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { fetchTargets(); fetchQuota(); }, []);

  return (
    <PageContainer>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={limiteAtingido ? 1 : 3}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 'bold' }}>Gestão de Alvos</Typography>
          {limiteAlvos !== null && (
            <Typography variant="caption" sx={{ color: 'text.secondary' }}>
              {targets.length} de {limiteAlvos} alvos · Plano {planoNome}
            </Typography>
          )}
        </Box>

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
            sx={{ backgroundColor: 'background.paper', borderRadius: 1 }}
          />

          <input
            type="file"
            accept=".csv"
            hidden
            ref={fileInputRef}
            onChange={handleFileUpload}
          />
          
          <Tooltip title="CSV deve conter as colunas: nome, email, departamento">
            <span>
              <Button
                variant="outlined"
                startIcon={<UploadIcon />}
                onClick={() => fileInputRef.current?.click()}
                disabled={limiteAtingido}
              >
                Importar CSV
              </Button>
            </span>
          </Tooltip>

          <Tooltip title={limiteAtingido ? `Limite do plano ${planoNome} atingido` : 'Cadastrar novo alvo'}>
            <span>
              <Button
                variant="contained"
                startIcon={<PersonAddIcon />}
                onClick={handleOpen}
                disabled={limiteAtingido}
              >
                Novo Alvo
              </Button>
            </span>
          </Tooltip>
        </Box>
      </Stack>

      {limiteAtingido && (
        <Typography variant="body2" sx={{ color: 'error.main', fontWeight: 500, mb: 3 }}>
          Limite de alvos do seu plano atingido. Faça um upgrade para o Plano Prata.
        </Typography>
      )}

      <TableContainer component={Paper} elevation={2}>
        <Table>
          {/* O fundo do cabeçalho (Surface 2) vem do tema global — MuiTableCell.head. */}
          <TableHead>
            <TableRow>
              <TableCell align="center"><strong>Nome</strong></TableCell>
              <TableCell align="center"><strong>E-mail</strong></TableCell>
              <TableCell align="center"><strong>Departamento</strong></TableCell>
              <TableCell align="center"><strong>Ações</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={4} align="center"><CircularProgress sx={{ m: 2 }} /></TableCell></TableRow>
            ) : (
              filteredTargets.map((target) => (
                <TableRow key={target.id} hover>
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>{target.nome}</TableCell>
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>{target.email}</TableCell>
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>{target.departamento}</TableCell>

                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                    <Tooltip title="Testar Disparo para este Alvo">
                      <IconButton color="secondary" onClick={() => handleTestTarget(target.id, target.email)}>
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
          {emailError && (
            <Typography variant="caption" color="error" sx={{ fontWeight: 500, display: 'block', mt: 0.5 }}>
              {emailError}
            </Typography>
          )}
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
    </PageContainer>
  );
}
