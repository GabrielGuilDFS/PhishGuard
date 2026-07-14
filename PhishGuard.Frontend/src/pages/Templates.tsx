import { useState, useEffect, useMemo } from 'react';
import {
  Box, Button, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, IconButton, Dialog,
  DialogTitle, DialogContent, DialogActions, Stack, Snackbar, Alert,
  CircularProgress, Chip, Tabs, Tab, ToggleButton, ToggleButtonGroup,
  TextField, MenuItem, Grid, Tooltip
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import PlaylistAddCheckIcon from '@mui/icons-material/PlaylistAddCheck';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import WebIcon from '@mui/icons-material/Web';
import { templatesPredefinidos } from '../data/predefinedTemplates';
import { simulationScenarios, type SimulationScenario } from '../data/predefinedTemplates';
import { landingTemplates } from '../data/landingTemplates';
import { educationalTemplates } from '../data/educationalTemplates';

// Biblioteca de Modelos — tela unificada de gerenciamento.
//
// Fusão das antigas abas "Cenários (E-mails)" e "Páginas Falsas": um Cenário de
// Simulação amarra estritamente uma isca de e-mail à sua página falsa (ver
// simulationScenarios em predefinedTemplates.ts). A tela tem duas abas:
//   1. Cenários de Simulação  — preview emparelhado (E-mail SMTP ⇄ Página Falsa)
//      e registro do par no backend (uma linha em Templates + uma em PhishingPages).
//   2. Páginas Educativas      — listagem fixa dos moldes pedagógicos + previewer,
//      sem qualquer edição de HTML bruto.
//
// Persistência por identificador: os registros guardam APENAS o id do molde
// (corpoHtml = id da isca; conteudoHtml = id da landing/educacional). O backend
// resolve o id de volta para o HTML no disparo.

const API_TEMPLATES = 'http://localhost:5000/api/Templates';
const API_PHISHING = 'http://localhost:5000/api/PhishingPages';
const API_EDU = 'http://localhost:5000/api/EducationalPages';

// Índices auxiliares dos catálogos estáticos (id -> objeto).
const iscaPorId = new Map(templatesPredefinidos.map((i) => [i.id, i]));
const landingPorId = new Map(landingTemplates.map((l) => [l.id, l]));

interface TemplateRow { id: string; nome: string; corpoHtml: string; }
interface PageRow { id: string; nome: string; conteudoHtml: string; }

type Notify = { open: boolean; message: string; type: 'success' | 'error' | 'info' };

export default function Templates() {
  const [aba, setAba] = useState(0);
  const [loading, setLoading] = useState(false);

  const [templates, setTemplates] = useState<TemplateRow[]>([]);
  const [phishingPages, setPhishingPages] = useState<PageRow[]>([]);
  const [eduPages, setEduPages] = useState<PageRow[]>([]);

  const [notify, setNotify] = useState<Notify>({ open: false, message: '', type: 'success' });
  const showNotify = (message: string, type: Notify['type'] = 'success') =>
    setNotify({ open: true, message, type });

  const token = localStorage.getItem('phishguard_token');
  const authHeaders = { Authorization: `Bearer ${token}` };
  const jsonHeaders = { 'Content-Type': 'application/json', ...authHeaders };

  // ------- Carga inicial -------
  const fetchAll = async () => {
    setLoading(true);
    try {
      const [tRes, pRes, eRes] = await Promise.all([
        fetch(API_TEMPLATES, { headers: authHeaders }),
        fetch(API_PHISHING, { headers: authHeaders }),
        fetch(API_EDU, { headers: authHeaders }),
      ]);
      if (tRes.ok) setTemplates(await tRes.json());
      if (pRes.ok) setPhishingPages(await pRes.json());
      if (eRes.ok) setEduPages(await eRes.json());
    } catch {
      showNotify('Erro ao conectar com o servidor', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchAll(); }, []);

  // Um cenário está "registrado" quando existe uma linha de Template (isca) e uma
  // linha de PhishingPage (landing) com os identificadores do cenário.
  const templateDoCenario = (s: SimulationScenario) =>
    templates.find((t) => t.corpoHtml === s.emailTemplateId);
  const landingDoCenario = (s: SimulationScenario) =>
    phishingPages.find((p) => p.conteudoHtml === s.landingTemplateId);
  const cenarioRegistrado = (s: SimulationScenario) =>
    Boolean(templateDoCenario(s) && landingDoCenario(s));

  // ------- Registro / remoção de Cenário (par Template + PhishingPage) -------
  const registrarCenario = async (s: SimulationScenario) => {
    const isca = iscaPorId.get(s.emailTemplateId);
    const landing = landingPorId.get(s.landingTemplateId);
    if (!isca || !landing) {
      showNotify('Cenário inválido: molde não encontrado no catálogo.', 'error');
      return;
    }
    try {
      const requisicoes: Promise<Response>[] = [];
      if (!templateDoCenario(s)) {
        requisicoes.push(fetch(API_TEMPLATES, {
          method: 'POST', headers: jsonHeaders,
          body: JSON.stringify({
            nome: isca.nome, assunto: isca.assunto,
            remetenteNome: isca.remetenteNome, remetenteEmail: isca.remetenteEmail,
            corpoHtml: isca.id,
          }),
        }));
      }
      if (!landingDoCenario(s)) {
        requisicoes.push(fetch(API_PHISHING, {
          method: 'POST', headers: jsonHeaders,
          body: JSON.stringify({ nome: landing.nome, conteudoHtml: landing.id }),
        }));
      }
      const respostas = await Promise.all(requisicoes);
      if (respostas.every((r) => r.ok)) {
        showNotify('Cenário registrado! Já pode ser usado em campanhas.', 'success');
        fetchAll();
      } else {
        showNotify('Falha ao registrar o cenário.', 'error');
      }
    } catch {
      showNotify('Erro de rede ao registrar o cenário.', 'error');
    }
  };

  const removerCenario = async (s: SimulationScenario) => {
    if (!window.confirm(`Remover o cenário "${s.nome}"? As campanhas que o utilizam podem ficar sem referência.`)) return;
    const tRow = templateDoCenario(s);
    const pRow = landingDoCenario(s);
    try {
      const requisicoes: Promise<Response>[] = [];
      if (tRow) requisicoes.push(fetch(`${API_TEMPLATES}/${tRow.id}`, { method: 'DELETE', headers: authHeaders }));
      if (pRow) requisicoes.push(fetch(`${API_PHISHING}/${pRow.id}`, { method: 'DELETE', headers: authHeaders }));
      await Promise.all(requisicoes);
      showNotify('Cenário removido.', 'success');
      fetchAll();
    } catch {
      showNotify('Erro ao remover o cenário.', 'error');
    }
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 1 }}>Biblioteca de Modelos</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Cenários amarram o e-mail à página falsa correspondente. As páginas educativas são fixas do sistema.
      </Typography>

      <Tabs value={aba} onChange={(_, v) => setAba(v)} sx={{ mb: 3 }}>
        <Tab label="Cenários de Simulação" icon={<MarkEmailReadIcon />} iconPosition="start" />
        <Tab label="Páginas Educativas" icon={<WebIcon />} iconPosition="start" />
      </Tabs>

      {loading && (
        <Box display="flex" justifyContent="center" p={3}><CircularProgress /></Box>
      )}

      {!loading && aba === 0 && (
        <CenariosTab
          registrado={cenarioRegistrado}
          onRegistrar={registrarCenario}
          onRemover={removerCenario}
        />
      )}

      {!loading && aba === 1 && (
        <EducativasTab
          eduPages={eduPages}
          jsonHeaders={jsonHeaders}
          authHeaders={authHeaders}
          onChange={fetchAll}
          notify={showNotify}
        />
      )}

      <Snackbar open={notify.open} autoHideDuration={4000} onClose={() => setNotify({ ...notify, open: false })} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={notify.type} variant="filled" sx={{ width: '100%' }}>{notify.message}</Alert>
      </Snackbar>
    </Box>
  );
}

// ===========================================================================
// Aba 1 — Cenários de Simulação
// ===========================================================================
function CenariosTab({ registrado, onRegistrar, onRemover }: {
  registrado: (s: SimulationScenario) => boolean;
  onRegistrar: (s: SimulationScenario) => void;
  onRemover: (s: SimulationScenario) => void;
}) {
  const [preview, setPreview] = useState<SimulationScenario | null>(null);
  const [modo, setModo] = useState<'email' | 'landing'>('email');

  const abrirPreview = (s: SimulationScenario) => { setModo('email'); setPreview(s); };

  // Monta o srcDoc do preview conforme o modo (E-mail SMTP ou Página Falsa).
  const previewSrcDoc = useMemo(() => {
    if (!preview) return '';
    if (modo === 'email') {
      const isca = iscaPorId.get(preview.emailTemplateId);
      if (!isca) return '<div style="padding:24px;font-family:sans-serif;color:#999;">Isca não encontrada.</div>';
      return isca.corpoHtml
        .replaceAll('{{LINK_PHISHING}}', '#')
        .replaceAll('{{LINK}}', '#')
        .replaceAll('{{NOME}}', 'Colaborador(a)');
    }
    const landing = landingPorId.get(preview.landingTemplateId);
    if (!landing) return '<div style="padding:24px;font-family:sans-serif;color:#999;">Página falsa não encontrada.</div>';
    return landing.html.replace(/{{CAMPAIGN_ID}}/g, '').replace(/{{TARGET_ID}}/g, '');
  }, [preview, modo]);

  return (
    <>
      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead sx={{ backgroundColor: '#f8f9fa' }}>
            <TableRow>
              <TableCell><strong>Cenário</strong></TableCell>
              <TableCell><strong>Categoria</strong></TableCell>
              <TableCell><strong>Amarração (E-mail ⇄ Página Falsa)</strong></TableCell>
              <TableCell><strong>Status</strong></TableCell>
              <TableCell align="right"><strong>Ações</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {simulationScenarios.map((s) => {
              const isca = iscaPorId.get(s.emailTemplateId);
              const landing = landingPorId.get(s.landingTemplateId);
              const jaRegistrado = registrado(s);
              return (
                <TableRow key={s.id} hover>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>{s.nome}</Typography>
                    <Typography variant="caption" color="text.secondary">{s.descricao}</Typography>
                  </TableCell>
                  <TableCell><Chip label={s.categoria} size="small" variant="outlined" color="primary" /></TableCell>
                  <TableCell>
                    <Typography variant="caption" display="block">{isca?.nome ?? s.emailTemplateId}</Typography>
                    <Typography variant="caption" color="text.secondary" display="block">{landing?.nome ?? s.landingTemplateId}</Typography>
                  </TableCell>
                  <TableCell>
                    {jaRegistrado
                      ? <Chip label="Registrado" size="small" color="success" />
                      : <Chip label="Não registrado" size="small" variant="outlined" />}
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Visualizar previews (E-mail e Página Falsa)">
                      <IconButton aria-label={`Visualizar ${s.nome}`} color="primary" onClick={() => abrirPreview(s)}><VisibilityIcon fontSize="small" /></IconButton>
                    </Tooltip>
                    {jaRegistrado ? (
                      <Tooltip title="Remover cenário">
                        <IconButton aria-label={`Remover ${s.nome}`} color="error" onClick={() => onRemover(s)}><DeleteIcon fontSize="small" /></IconButton>
                      </Tooltip>
                    ) : (
                      <Tooltip title="Registrar cenário para uso em campanhas">
                        <IconButton aria-label={`Registrar ${s.nome}`} color="success" onClick={() => onRegistrar(s)}><PlaylistAddCheckIcon fontSize="small" /></IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={Boolean(preview)} onClose={() => setPreview(null)} fullWidth maxWidth="lg">
        <DialogTitle>{preview?.nome}</DialogTitle>
        <DialogContent dividers>
          <Stack direction="row" justifyContent="center" sx={{ mb: 2 }}>
            <ToggleButtonGroup
              exclusive size="small" color="primary"
              value={modo}
              onChange={(_, v) => { if (v) setModo(v); }}
            >
              <ToggleButton value="email">E-mail (SMTP)</ToggleButton>
              <ToggleButton value="landing">Página Falsa (Tailwind v4)</ToggleButton>
            </ToggleButtonGroup>
          </Stack>
          <Paper variant="outlined" sx={{ height: '560px', overflow: 'hidden', borderRadius: 2 }}>
            <iframe
              title={modo === 'email' ? 'Email Preview' : 'Landing Preview'}
              srcDoc={previewSrcDoc}
              // Segurança (XSS): 'allow-same-origin' SEM 'allow-scripts' isola o HTML
              // clonado — scripts injetados não executam e não conseguem ler o JWT em
              // localStorage do painel administrativo.
              sandbox="allow-same-origin"
              style={{ width: '100%', height: '100%', border: 'none' }}
            />
          </Paper>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setPreview(null)} color="inherit">Fechar</Button>
          {preview && !registrado(preview) && (
            <Button variant="contained" onClick={() => { onRegistrar(preview); setPreview(null); }}>
              Registrar Cenário
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </>
  );
}

// ===========================================================================
// Aba 2 — Páginas Educativas (fixas, somente leitura + previewer)
// ===========================================================================
function EducativasTab({ eduPages, jsonHeaders, authHeaders, onChange, notify }: {
  eduPages: PageRow[];
  jsonHeaders: Record<string, string>;
  authHeaders: Record<string, string>;
  onChange: () => void;
  notify: (m: string, t?: Notify['type']) => void;
}) {
  const [selecionado, setSelecionado] = useState(educationalTemplates[0]?.id ?? '');
  const molde = educationalTemplates.find((m) => m.id === selecionado);

  // Uma página educativa está registrada quando existe uma linha cujo conteudoHtml
  // casa com o HTML do molde fixo (persistência do molde escolhido).
  const linhaDoMolde = (html: string) => eduPages.find((p) => p.conteudoHtml === html);
  const registrado = molde ? Boolean(linhaDoMolde(molde.html)) : false;

  const registrar = async () => {
    if (!molde) return;
    try {
      const res = await fetch(API_EDU, {
        method: 'POST', headers: jsonHeaders,
        body: JSON.stringify({ nome: molde.nome, conteudoHtml: molde.html }),
      });
      if (res.ok) { notify('Página educativa registrada!', 'success'); onChange(); }
      else notify('Falha ao registrar a página educativa.', 'error');
    } catch { notify('Erro de rede ao registrar.', 'error'); }
  };

  const remover = async () => {
    if (!molde) return;
    const linha = linhaDoMolde(molde.html);
    if (!linha) return;
    try {
      const res = await fetch(`${API_EDU}/${linha.id}`, { method: 'DELETE', headers: authHeaders });
      if (res.ok) { notify('Página educativa removida.', 'success'); onChange(); }
    } catch { notify('Erro ao remover.', 'error'); }
  };

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 5 }}>
        <Paper variant="outlined" sx={{ p: 3, borderRadius: 2 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 'bold', mb: 1 }}>Moldes Pedagógicos</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Catálogo fixo do sistema. Selecione uma abordagem para ler e visualizar. As campanhas escolhem
            livremente qual molde aplicar ao final do fluxo.
          </Typography>

          <TextField
            select fullWidth size="small" label="Escolha o molde educativo"
            value={selecionado}
            onChange={(e) => setSelecionado(e.target.value)}
            sx={{ mb: 2 }}
          >
            {educationalTemplates.map((m) => (
              <MenuItem key={m.id} value={m.id}>{m.nome}</MenuItem>
            ))}
          </TextField>

          {molde && (
            <Stack direction="row" spacing={1} alignItems="center" sx={{ flexWrap: 'wrap', gap: 1, mb: 2 }}>
              {molde.categoria && <Chip label={molde.categoria} size="small" variant="outlined" color="primary" />}
              {registrado
                ? <Chip label="Registrado" size="small" color="success" />
                : <Chip label="Não registrado" size="small" variant="outlined" />}
            </Stack>
          )}

          {registrado ? (
            <Button variant="outlined" color="error" fullWidth startIcon={<DeleteIcon />} onClick={remover}>
              Remover registro
            </Button>
          ) : (
            <Button variant="contained" fullWidth startIcon={<PlaylistAddCheckIcon />} onClick={registrar}>
              Registrar molde
            </Button>
          )}
        </Paper>
      </Grid>

      <Grid size={{ xs: 12, md: 7 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ color: 'text.secondary' }}>
          Preview do molde educativo (visão do alvo ao final da simulação):
        </Typography>
        <Paper variant="outlined" sx={{ height: '560px', overflow: 'hidden', borderRadius: 2 }}>
          <iframe
            title="Educational Preview"
            srcDoc={molde?.html ?? '<div style="padding:24px;font-family:sans-serif;color:#999;">Selecione um molde.</div>'}
            // Segurança (XSS): isola o preview sem permitir execução de scripts.
            sandbox="allow-same-origin"
            style={{ width: '100%', height: '100%', border: 'none' }}
          />
        </Paper>
      </Grid>
    </Grid>
  );
}
