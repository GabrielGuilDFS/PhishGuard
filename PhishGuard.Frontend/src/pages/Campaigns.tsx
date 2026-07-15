import { useState, useEffect, useMemo, useRef } from 'react';
import {
    Typography, Box, Button, TextField, Table, TableBody,
    TableCell, TableContainer, TableHead, TableRow, Paper, IconButton,
    Dialog, DialogTitle, DialogContent, DialogActions, Autocomplete,
    CircularProgress, Stack, Divider, Chip, Tooltip
} from '@mui/material';
import PageContainer from '../components/PageContainer';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SendIcon from '@mui/icons-material/Send';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import ClearAllIcon from '@mui/icons-material/ClearAll';
import { simulationScenarios, type SimulationScenario } from '../data/predefinedTemplates';
import { educationalTemplates } from '../data/educationalTemplates';
import { CampaignStatus } from '../data/campaignStatus';
import { useNotify } from '../context/NotificationContext';


interface Campaign {
    id: string;
    nomeCampanha: string;
    status: string;
    dataInicio: string;
    dataFim?: string;
    templateNome: string;
    landingPageNome: string;
    educationalPageNome: string;
}

interface LookupItem {
    id: string;
    nome: string;
    email?: string;
    departamento?: string;
    corpoHtml?: string;    // Templates: carrega o id da isca de e-mail.
    conteudoHtml?: string; // PhishingPages/EducationalPages: id da landing / html educativo.
}

// Cenário resolvido para linhas concretas do banco (Template + PhishingPage).
interface CenarioDisponivel extends SimulationScenario {
    emailRowId: string;
    landingRowId: string;
}

type EduMolde = (typeof educationalTemplates)[number];

// Resumo da importação de CSV (Fase 4 — feedback ao usuário).
interface ImportSummary {
    processados: number;
    importados: number;
    duplicados: number;
    invalidos: number;
}

const toDateTimeLocal = (iso?: string | null) => (iso ? iso.slice(0, 16) : '');

// Extrai uma mensagem legível de uma resposta de erro do ASP.NET Core (lê o corpo UMA vez).
const extrairMensagemDeErro = async (res: Response): Promise<string> => {
    const raw = await res.text().catch(() => '');
    try {
        const data = JSON.parse(raw);
        if (typeof data === 'string') return data;
        if (data?.errors) return Object.values(data.errors).flat().join(' ');
        return data?.message || data?.error || data?.title || raw || res.statusText;
    } catch {
        return raw || res.statusText;
    }
};

// Traduz erros crus (ex.: exceções de SMTP) em mensagens amigáveis e acionáveis.
const mensagemAmigavel = (erro: string): string => {
    const e = (erro || '').toLowerCase();
    if (/smtp|conex|connect|socket|autentic|\bauth|mailkit|\bhost\b|servidor de e-?mail|timeout/.test(e)) {
        return 'Falha na conexão com o servidor de e-mail. Verifique suas configurações de SMTP em Configurações.';
    }
    return erro || 'Ocorreu um erro inesperado. Tente novamente.';
};

const emailValido = (v: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);

export default function Campaigns() {
    const { showNotify } = useNotify();

    const [campaigns, setCampaigns] = useState<Campaign[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(false);
    const [activatingId, setActivatingId] = useState<string | null>(null);

    const [open, setOpen] = useState(false);
    const [currentId, setCurrentId] = useState<string | null>(null);

    const [formData, setFormData] = useState({
        nomeCampanha: '',
        dataInicio: '',
        dataFim: '',
        cenario: null as CenarioDisponivel | null,
        educationalMolde: null as EduMolde | null,
        targetsSelecionados: [] as LookupItem[]
    });

    const [templates, setTemplates] = useState<LookupItem[]>([]);
    const [phishingPages, setPhishingPages] = useState<LookupItem[]>([]);
    const [educationalPages, setEducationalPages] = useState<LookupItem[]>([]);
    const [targets, setTargets] = useState<LookupItem[]>([]);
    const [loadingLookups, setLoadingLookups] = useState(false);
    const [importSummary, setImportSummary] = useState<ImportSummary | null>(null);
    const csvInputRef = useRef<HTMLInputElement>(null);

    const API_BASE = 'http://localhost:5000/api';

    const token = localStorage.getItem('phishguard_token');
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    const cenariosDisponiveis = useMemo<CenarioDisponivel[]>(() => {
        return simulationScenarios
            .map((s) => {
                const emailRow = templates.find((t) => t.corpoHtml === s.emailTemplateId);
                const landingRow = phishingPages.find((p) => p.conteudoHtml === s.landingTemplateId);
                if (!emailRow || !landingRow) return null;
                return { ...s, emailRowId: emailRow.id, landingRowId: landingRow.id };
            })
            .filter((c): c is CenarioDisponivel => c !== null);
    }, [templates, phishingPages]);

    // Departamentos distintos cadastrados no tenant (para seleção por setor).
    const departamentos = useMemo<string[]>(() => {
        const set = new Set<string>();
        targets.forEach((t) => { if (t.departamento) set.add(t.departamento); });
        return Array.from(set).sort((a, b) => a.localeCompare(b, 'pt-BR'));
    }, [targets]);

    useEffect(() => {
        fetchCampaigns();
    }, []);

    const fetchCampaigns = async () => {
        try {
            const res = await fetch(`${API_BASE}/Campaigns`, { headers });
            if (res.ok) setCampaigns(await res.json());
        } catch (error) {
            console.error('Erro ao buscar campanhas', error);
            showNotify('Não foi possível carregar as campanhas.', 'error');
        }
    };

    const openModal = async (campaign?: Campaign) => {
        let loadedTemplates = templates;
        let loadedPhishingPages = phishingPages;
        let loadedEduPages = educationalPages;
        let loadedTargets = targets;

        if (templates.length === 0) {
            setLoadingLookups(true);
            try {
                const [tempRes, phishRes, eduRes, targetRes] = await Promise.all([
                    fetch(`${API_BASE}/Templates`, { headers }),
                    fetch(`${API_BASE}/PhishingPages`, { headers }),
                    fetch(`${API_BASE}/EducationalPages`, { headers }),
                    fetch(`${API_BASE}/Targets`, { headers })
                ]);

                if (tempRes.ok && phishRes.ok && eduRes.ok && targetRes.ok) {
                    loadedTemplates = await tempRes.json();
                    loadedPhishingPages = await phishRes.json();
                    loadedEduPages = await eduRes.json();
                    loadedTargets = await targetRes.json();

                    setTemplates(loadedTemplates);
                    setPhishingPages(loadedPhishingPages);
                    setEducationalPages(loadedEduPages);
                    setTargets(loadedTargets);
                }
            } catch (error) {
                console.error('Erro ao buscar lookups', error);
                showNotify('Erro ao carregar dados do formulário.', 'error');
            } finally {
                setLoadingLookups(false);
            }
        }

        if (campaign) {
            try {
                const res = await fetch(`${API_BASE}/Campaigns/${campaign.id}`, { headers });
                if (res.ok) {
                    const data = await res.json();
                    setCurrentId(data.id);

                    const alvosPreSelecionados = loadedTargets.filter(t => data.targetIds?.includes(t.id));

                    const cenarioReconstruido = simulationScenarios
                        .map((s) => {
                            const emailRow = loadedTemplates.find((t) => t.corpoHtml === s.emailTemplateId);
                            const landingRow = loadedPhishingPages.find((p) => p.conteudoHtml === s.landingTemplateId);
                            if (!emailRow || !landingRow) return null;
                            return { ...s, emailRowId: emailRow.id, landingRowId: landingRow.id } as CenarioDisponivel;
                        })
                        .find((c) => c?.emailRowId === data.emailTemplateId) ?? null;

                    // Reconstrói o molde educativo (catálogo estático) a partir do nome persistido.
                    const moldeReconstruido = educationalTemplates.find(m => m.nome === data.educationalPageNome) ?? null;

                    setFormData({
                        nomeCampanha: data.nomeCampanha || '',
                        dataInicio: toDateTimeLocal(data.dataInicio),
                        dataFim: toDateTimeLocal(data.dataFim),
                        cenario: cenarioReconstruido,
                        educationalMolde: moldeReconstruido,
                        targetsSelecionados: alvosPreSelecionados
                    });
                }
            } catch (error) {
                console.error('Failed to load campaign details', error);
                showNotify('Não foi possível carregar os detalhes da campanha.', 'error');
            }
        } else {
            setCurrentId(null);
            setFormData({
                nomeCampanha: '',
                dataInicio: toDateTimeLocal(new Date().toISOString()),
                dataFim: '',
                cenario: null,
                educationalMolde: null,
                targetsSelecionados: []
            });
        }
        setOpen(true);
    };

    const closeModal = () => setOpen(false);

    // Fase 1: provisiona a linha de EducationalPages sob demanda (find-or-create). O admin
    // não precisa mais "registrar" o molde manualmente na Biblioteca de Modelos.
    const garantirPaginaEducativa = async (molde: EduMolde): Promise<string> => {
        const existente = educationalPages.find(p => p.conteudoHtml === molde.html);
        if (existente) return existente.id;

        const res = await fetch(`${API_BASE}/EducationalPages`, {
            method: 'POST', headers,
            body: JSON.stringify({ nome: molde.nome, conteudoHtml: molde.html }),
        });
        if (!res.ok) throw new Error(await extrairMensagemDeErro(res));
        const nova = await res.json();
        setEducationalPages(prev => [...prev, { id: nova.id, nome: nova.nome, conteudoHtml: molde.html }]);
        return nova.id;
    };

    const handleSave = async () => {
        if (!formData.nomeCampanha || !formData.dataInicio || !formData.cenario ||
            !formData.educationalMolde || formData.targetsSelecionados.length === 0) {
            showNotify('Preencha os campos obrigatórios (Cenário e Página Educativa) e selecione ao menos um alvo.', 'error');
            return;
        }

        setLoading(true);
        try {
            const educationalPageId = await garantirPaginaEducativa(formData.educationalMolde);

            const payload = {
                nomeCampanha: formData.nomeCampanha,
                dataInicio: new Date(formData.dataInicio).toISOString(),
                dataFim: formData.dataFim ? new Date(formData.dataFim).toISOString() : null,
                emailTemplateId: formData.cenario.emailRowId,
                landingPageId: formData.cenario.landingRowId,
                educationalPageId,
                targetIds: formData.targetsSelecionados.map(t => t.id)
            };

            const method = currentId ? 'PUT' : 'POST';
            const url = currentId ? `${API_BASE}/Campaigns/${currentId}` : `${API_BASE}/Campaigns`;

            const res = await fetch(url, { method, headers, body: JSON.stringify(payload) });
            if (res.ok) {
                showNotify(currentId ? 'Campanha atualizada com sucesso.' : 'Campanha criada com sucesso.', 'success');
                closeModal();
                fetchCampaigns();
            } else {
                showNotify(mensagemAmigavel(await extrairMensagemDeErro(res)), 'error');
            }
        } catch (error) {
            console.error('Erro ao salvar', error);
            showNotify('Erro ao salvar a campanha. Tente novamente.', 'error');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (!window.confirm('Excluir esta campanha?')) return;
        try {
            const res = await fetch(`${API_BASE}/Campaigns/${id}`, { method: 'DELETE', headers });
            if (res.ok) { showNotify('Campanha excluída.', 'success'); fetchCampaigns(); }
            else showNotify(mensagemAmigavel(await extrairMensagemDeErro(res)), 'error');
        } catch (error) {
            console.error('Erro ao deletar', error);
            showNotify('Falha na comunicação com o servidor.', 'error');
        }
    };

    const filtered = campaigns.filter(c => c.nomeCampanha.toLowerCase().includes(searchTerm.toLowerCase()));

    const handleActivateCampaign = async (id: string) => {
        if (!window.confirm('Deseja ativar esta campanha? Se a data de início já chegou, os e-mails serão disparados agora; caso contrário, ela ficará Agendada até o horário.')) return;

        setActivatingId(id);
        try {
            const res = await fetch(`${API_BASE}/Campaigns/${id}/ativar`, { method: 'POST', headers });
            if (res.ok) {
                const data = await res.json().catch(() => null);
                showNotify(
                    data?.message ??
                    'Campanha agendada/iniciada com sucesso! O serviço em segundo plano está processando os disparos.',
                    'success'
                );
                fetchCampaigns();
            } else {
                showNotify(mensagemAmigavel(await extrairMensagemDeErro(res)), 'error');
            }
        } catch (error) {
            console.error('Erro ao ativar campanha', error);
            showNotify('Falha na comunicação com o servidor.', 'error');
        } finally {
            setActivatingId(null);
        }
    };

    // ---- Seleção de alvos: por setor, em lote e por CSV (Fase 3) ----
    const adicionarAlvos = (novos: LookupItem[]) => {
        setFormData(prev => {
            const ids = new Set(prev.targetsSelecionados.map(t => t.id));
            const adicionar = novos.filter(t => !ids.has(t.id));
            return { ...prev, targetsSelecionados: [...prev.targetsSelecionados, ...adicionar] };
        });
    };

    const adicionarSetor = (dep: string | null) => {
        if (!dep) return;
        const doSetor = targets.filter(t => (t.departamento || '') === dep);
        adicionarAlvos(doSetor);
        showNotify(`Setor "${dep}": ${doSetor.length} alvo(s) adicionados à seleção.`, 'info');
    };

    const selecionarTodos = () => {
        setFormData(prev => ({ ...prev, targetsSelecionados: [...targets] }));
        showNotify(`Todos os ${targets.length} alvos foram selecionados.`, 'info');
    };

    const limparSelecao = () => setFormData(prev => ({ ...prev, targetsSelecionados: [] }));

    // Importa um CSV temporário e MAPEIA os e-mails para alvos JÁ cadastrados (não cria
    // novos alvos — respeita a cota de plano). Gera um resumo: importados/duplicados/inválidos.
    const handleCsvSelecionado = async (file: File) => {
        try {
            const texto = await file.text();
            const linhas = texto.split(/\r?\n/).map(l => l.trim()).filter(Boolean);

            const targetPorEmail = new Map(targets.map(t => [(t.email || '').toLowerCase(), t]));
            const jaSelecionados = new Set(formData.targetsSelecionados.map(t => t.id));
            const vistosNoArquivo = new Set<string>();
            const novos: LookupItem[] = [];
            let processados = 0, importados = 0, duplicados = 0, invalidos = 0;

            for (const linha of linhas) {
                const campos = linha.split(/[,;\t]/).map(c => c.trim());
                const email = campos.map(c => c.toLowerCase()).find(emailValido);

                if (!email) {
                    // Linha de cabeçalho (email/nome/departamento) → ignora sem contar.
                    if (/e-?mail|nome|departamento/i.test(linha)) continue;
                    processados++; invalidos++; continue;
                }

                processados++;
                if (vistosNoArquivo.has(email)) { duplicados++; continue; }
                vistosNoArquivo.add(email);

                const alvo = targetPorEmail.get(email);
                if (!alvo) { invalidos++; continue; }          // e-mail não cadastrado como alvo
                if (jaSelecionados.has(alvo.id)) { duplicados++; continue; }

                jaSelecionados.add(alvo.id);
                novos.push(alvo);
                importados++;
            }

            adicionarAlvos(novos);
            setImportSummary({ processados, importados, duplicados, invalidos });
            showNotify(`Importação concluída: ${importados} alvo(s) adicionados.`, importados > 0 ? 'success' : 'info');
        } catch {
            showNotify('Não foi possível ler o arquivo CSV. Verifique o formato.', 'error');
        } finally {
            if (csvInputRef.current) csvInputRef.current.value = '';
        }
    };

    return (
        <PageContainer>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                <Typography variant="h4">Gerenciamento de Campanhas</Typography>
                <Button variant="contained" color="primary" onClick={() => openModal()}>
                    Nova Campanha
                </Button>
            </Box>

            <TextField
                label="Buscar"
                variant="outlined"
                fullWidth
                margin="normal"
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                sx={{ mb: 3 }}
            />

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Nome</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Início</TableCell>
                            <TableCell>Encerramento da Coleta</TableCell>
                            <TableCell>E-mail (Cenário)</TableCell>
                            <TableCell>Página Falsa</TableCell>
                            <TableCell>Página Educativa</TableCell>
                            <TableCell align="center">Ações</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {filtered.map(c => (
                            <TableRow key={c.id}>
                                <TableCell>{c.nomeCampanha}</TableCell>
                                <TableCell>{c.status}</TableCell>
                                <TableCell>{new Date(c.dataInicio).toLocaleString()}</TableCell>
                                <TableCell>{c.dataFim ? new Date(c.dataFim).toLocaleString() : '-'}</TableCell>
                                <TableCell>{c.templateNome}</TableCell>
                                <TableCell>{c.landingPageNome}</TableCell>
                                <TableCell>{c.educationalPageNome}</TableCell>
                                <TableCell align="center">
                                    {c.status === CampaignStatus.Rascunho && (
                                        activatingId === c.id ? (
                                            <Tooltip title="Processando disparo...">
                                                <span>
                                                    <IconButton color="success" disabled>
                                                        <CircularProgress size={20} color="inherit" />
                                                    </IconButton>
                                                </span>
                                            </Tooltip>
                                        ) : (
                                            <IconButton onClick={() => handleActivateCampaign(c.id)} color="success" title="Ativar Campanha">
                                                <SendIcon />
                                            </IconButton>
                                        )
                                    )}
                                    <IconButton onClick={() => openModal(c)} color="primary" title="Editar">
                                        <EditIcon />
                                    </IconButton>
                                    <IconButton onClick={() => handleDelete(c.id)} color="error" title="Excluir">
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Dialog open={open} onClose={closeModal} maxWidth="sm" fullWidth>
                <DialogTitle>{currentId ? 'Editar Campanha' : 'Nova Campanha'}</DialogTitle>
                <DialogContent>
                    {loadingLookups ? (
                        <Box display="flex" justifyContent="center" p={3}>
                            <CircularProgress />
                        </Box>
                    ) : (
                        <Box component="form" sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
                            <TextField
                                label="Nome da Campanha"
                                fullWidth
                                required
                                value={formData.nomeCampanha}
                                onChange={e => setFormData({ ...formData, nomeCampanha: e.target.value })}
                            />

                            <Box display="flex" gap={2}>
                                <TextField
                                    label="Data de Início"
                                    type="datetime-local"
                                    fullWidth
                                    required
                                    InputLabelProps={{ shrink: true }}
                                    value={formData.dataInicio}
                                    onChange={e => setFormData({ ...formData, dataInicio: e.target.value })}
                                />
                                <TextField
                                    label="Data de Encerramento da Coleta"
                                    type="datetime-local"
                                    fullWidth
                                    InputLabelProps={{ shrink: true }}
                                    helperText="Limite temporal para registrar métricas e cliques desta simulação."
                                    value={formData.dataFim}
                                    onChange={e => setFormData({ ...formData, dataFim: e.target.value })}
                                />
                            </Box>

                            <Autocomplete
                                options={cenariosDisponiveis}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.cenario}
                                onChange={(_, newValue) => setFormData({ ...formData, cenario: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                noOptionsText="Nenhum cenário registrado. Registre um na Biblioteca de Modelos."
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Cenário de Simulação"
                                        required
                                        helperText="Amarra o e-mail à página falsa correspondente (ex.: Amazon com Amazon)."
                                    />
                                )}
                            />

                            <Autocomplete
                                options={educationalTemplates}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.educationalMolde}
                                onChange={(_, newValue) => setFormData({ ...formData, educationalMolde: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Página Educativa"
                                        required
                                        helperText="Molde exibido ao alvo ao final da simulação (associado automaticamente)."
                                    />
                                )}
                            />

                            <Divider textAlign="left">
                                <Typography variant="caption" color="text.secondary">Seleção de Alvos</Typography>
                            </Divider>

                            {/* Fase 3: atalhos de seleção — por setor, em lote e por CSV. */}
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems="center">
                                <Autocomplete
                                    sx={{ flex: 1, width: '100%' }}
                                    options={departamentos}
                                    value={null}
                                    blurOnSelect
                                    disabled={departamentos.length === 0}
                                    onChange={(_, dep) => adicionarSetor(dep)}
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            size="small"
                                            label="Adicionar por Departamento"
                                            placeholder={departamentos.length ? 'Ex.: Financeiro' : 'Nenhum departamento'}
                                        />
                                    )}
                                />
                                <Button
                                    size="small"
                                    variant="outlined"
                                    startIcon={<DoneAllIcon />}
                                    onClick={selecionarTodos}
                                    disabled={targets.length === 0}
                                >
                                    Todos ({targets.length})
                                </Button>
                                <Button
                                    size="small"
                                    variant="outlined"
                                    component="label"
                                    startIcon={<UploadFileIcon />}
                                >
                                    CSV
                                    <input
                                        ref={csvInputRef}
                                        hidden
                                        type="file"
                                        accept=".csv,text/csv"
                                        onChange={(e) => {
                                            const file = e.target.files?.[0];
                                            if (file) handleCsvSelecionado(file);
                                        }}
                                    />
                                </Button>
                            </Stack>

                            <Autocomplete
                                multiple
                                options={targets}
                                getOptionLabel={(option) => option.nome || option.email || ''}
                                value={formData.targetsSelecionados}
                                onChange={(_, newValue) => setFormData({ ...formData, targetsSelecionados: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                filterSelectedOptions
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Funcionários Alvo (Targets)"
                                        placeholder="Selecione 1 ou mais..."
                                        required={formData.targetsSelecionados.length === 0}
                                    />
                                )}
                            />

                            <Stack direction="row" justifyContent="space-between" alignItems="center">
                                <Chip
                                    size="small"
                                    color={formData.targetsSelecionados.length > 0 ? 'primary' : 'default'}
                                    label={`${formData.targetsSelecionados.length} de ${targets.length} alvos selecionados`}
                                />
                                {formData.targetsSelecionados.length > 0 && (
                                    <Button size="small" color="inherit" startIcon={<ClearAllIcon />} onClick={limparSelecao}>
                                        Limpar seleção
                                    </Button>
                                )}
                            </Stack>
                        </Box>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeModal} color="inherit">Cancelar</Button>
                    <Button
                        onClick={handleSave}
                        color="primary"
                        variant="contained"
                        disabled={loading || loadingLookups}
                        startIcon={loading ? <CircularProgress size={16} color="inherit" /> : undefined}
                    >
                        {loading ? 'Salvando...' : 'Salvar'}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Fase 4: resumo da importação de CSV. */}
            <Dialog open={Boolean(importSummary)} onClose={() => setImportSummary(null)} maxWidth="xs" fullWidth>
                <DialogTitle>Resumo da Importação</DialogTitle>
                <DialogContent dividers>
                    {importSummary && (
                        <Stack spacing={1.5}>
                            <Typography variant="body2">
                                <strong>{importSummary.processados}</strong> linha(s) processada(s).
                            </Typography>
                            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                                <Chip color="success" label={`${importSummary.importados} importados`} />
                                <Chip color="warning" label={`${importSummary.duplicados} duplicados`} />
                                <Chip color="error" label={`${importSummary.invalidos} inválidos`} />
                            </Stack>
                            <Typography variant="caption" color="text.secondary">
                                Apenas e-mails já cadastrados como alvos deste tenant são importados. Linhas sem
                                correspondência entram como "inválidos" (cadastre-as antes em Alvos).
                            </Typography>
                        </Stack>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setImportSummary(null)} variant="contained">Entendi</Button>
                </DialogActions>
            </Dialog>
        </PageContainer>
    );
}
