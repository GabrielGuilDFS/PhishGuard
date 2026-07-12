import { useState, useEffect, useMemo } from 'react';
import {
    Container, Typography, Box, Button, TextField, Table, TableBody,
    TableCell, TableContainer, TableHead, TableRow, Paper, IconButton,
    Dialog, DialogTitle, DialogContent, DialogActions, Autocomplete, CircularProgress
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SendIcon from '@mui/icons-material/Send';
import { simulationScenarios, type SimulationScenario } from '../data/predefinedTemplates';
import { CampaignStatus } from '../data/campaignStatus';


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
    corpoHtml?: string;    // Templates: carrega o id da isca de e-mail.
    conteudoHtml?: string; // PhishingPages: carrega o id da landing (página falsa).
}

// Cenário resolvido para linhas concretas do banco (Template + PhishingPage).
// Só entra no seletor quando AMBAS as linhas existem — garantido pelo registro
// feito na Biblioteca de Modelos.
interface CenarioDisponivel extends SimulationScenario {
    emailRowId: string;   // Guid da linha em Templates.
    landingRowId: string; // Guid da linha em PhishingPages.
}

// Converte um ISO vindo do backend para o formato aceito por <input datetime-local>
// ('YYYY-MM-DDTHH:mm'). Mantém o horário como está (sem conversão de fuso).
const toDateTimeLocal = (iso?: string | null) => (iso ? iso.slice(0, 16) : '');

// Extrai uma mensagem legível de uma resposta de erro do ASP.NET Core. O corpo é lido
// UMA vez como texto (evita "body already consumed") e então tentamos interpretá-lo como
// JSON: BadRequest com string, { message }, { error }, ou o ProblemDetails de validação
// ({ title, errors: { campo: string[] } }). Se não for JSON, usa o texto puro / statusText.
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

export default function Campaigns() {
    const [campaigns, setCampaigns] = useState<Campaign[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(false);

    const [open, setOpen] = useState(false);
    const [currentId, setCurrentId] = useState<string | null>(null);

    const [formData, setFormData] = useState({
        nomeCampanha: '',
        dataInicio: '',
        dataFim: '',
        cenario: null as CenarioDisponivel | null,
        educationalPageId: null as LookupItem | null,
        targetsSelecionados: [] as LookupItem[]
    });

    const [templates, setTemplates] = useState<LookupItem[]>([]);
    const [phishingPages, setPhishingPages] = useState<LookupItem[]>([]);
    const [educationalPages, setEducationalPages] = useState<LookupItem[]>([]);
    const [targets, setTargets] = useState<LookupItem[]>([]);
    const [loadingLookups, setLoadingLookups] = useState(false);

    const API_BASE = 'http://localhost:5000/api';

    const token = localStorage.getItem('phishguard_token');
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    // Cenários disponíveis: um cenário do catálogo só aparece quando existe uma linha
    // de Template (isca) e uma de PhishingPage (landing) com os identificadores dele.
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

    useEffect(() => {
        fetchCampaigns();
    }, []);

    const fetchCampaigns = async () => {
        try {
            const res = await fetch(`${API_BASE}/Campaigns`, { headers });
            if (res.ok) {
                const data = await res.json();
                setCampaigns(data);
            }
        } catch (error) {
            console.error('Erro ao buscar campanhas', error);
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

                    // Reconstrói o Cenário a partir dos ids persistidos (casando a linha
                    // de Template salva na campanha com o cenário do catálogo).
                    const cenarioReconstruido = simulationScenarios
                        .map((s) => {
                            const emailRow = loadedTemplates.find((t) => t.corpoHtml === s.emailTemplateId);
                            const landingRow = loadedPhishingPages.find((p) => p.conteudoHtml === s.landingTemplateId);
                            if (!emailRow || !landingRow) return null;
                            return { ...s, emailRowId: emailRow.id, landingRowId: landingRow.id } as CenarioDisponivel;
                        })
                        .find((c) => c?.emailRowId === data.emailTemplateId) ?? null;

                    setFormData({
                        nomeCampanha: data.nomeCampanha || '',
                        dataInicio: toDateTimeLocal(data.dataInicio),
                        dataFim: toDateTimeLocal(data.dataFim),
                        cenario: cenarioReconstruido,
                        educationalPageId: loadedEduPages.find(e => e.id === data.educationalPageId) || { id: data.educationalPageId, nome: data.educationalPageNome },
                        targetsSelecionados: alvosPreSelecionados
                    });
                }
            } catch (error) {
                console.error('Failed to load campaign details', error);
            }
        } else {
            setCurrentId(null);
            setFormData({
                nomeCampanha: '',
                dataInicio: toDateTimeLocal(new Date().toISOString()),
                dataFim: '',
                cenario: null,
                educationalPageId: null,
                targetsSelecionados: []
            });
        }
        setOpen(true);
    };

    const closeModal = () => setOpen(false);

    const handleSave = async () => {
        if (!formData.nomeCampanha || !formData.dataInicio || !formData.cenario || !formData.educationalPageId || formData.targetsSelecionados.length === 0) {
            alert('Preencha os campos obrigatórios (incluindo o Cenário de Simulação) e selecione pelo menos um alvo.');
            return;
        }

        // O Cenário resolve os dois ids que o backend espera: a isca de e-mail
        // (EmailTemplateId) e a página falsa amarrada (LandingPageId).
        const payload = {
            nomeCampanha: formData.nomeCampanha,
            dataInicio: new Date(formData.dataInicio).toISOString(),
            dataFim: formData.dataFim ? new Date(formData.dataFim).toISOString() : null,
            emailTemplateId: formData.cenario.emailRowId,
            landingPageId: formData.cenario.landingRowId,
            educationalPageId: formData.educationalPageId.id,
            targetIds: formData.targetsSelecionados.map(t => t.id)
        };

        const method = currentId ? 'PUT' : 'POST';
        const url = currentId ? `${API_BASE}/Campaigns/${currentId}` : `${API_BASE}/Campaigns`;

        try {
            setLoading(true);
            const res = await fetch(url, {
                method,
                headers,
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                closeModal();
                fetchCampaigns();
            } else {
                const err = await res.text();
                alert(`Erro: ${err}`);
            }
        } catch (error) {
            console.error('Erro ao salvar', error);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (!window.confirm('Excluir esta campanha?')) return;

        try {
            const res = await fetch(`${API_BASE}/Campaigns/${id}`, {
                method: 'DELETE',
                headers
            });
            if (res.ok) fetchCampaigns();
        } catch (error) {
            console.error('Erro ao deletar', error);
        }
    };

    const filtered = campaigns.filter(c => c.nomeCampanha.toLowerCase().includes(searchTerm.toLowerCase()));

    const handleActivateCampaign = async (id: string) => {
        if (!window.confirm('Deseja ativar esta campanha? Se a data de início já chegou, os e-mails serão disparados agora; caso contrário, ela ficará Agendada até o horário.')) return;

        try {
            setLoading(true);
            const res = await fetch(`${API_BASE}/Campaigns/${id}/ativar`, {
                method: 'POST',
                headers
            });

            if (res.ok) {
                const data = await res.json().catch(() => null);
                alert(data?.message ?? 'Campanha ativada com sucesso!');
                fetchCampaigns();
            } else {
                alert(`Erro ao ativar: ${await extrairMensagemDeErro(res)}`);
            }
        } catch (error) {
            console.error('Erro ao ativar campanha', error);
            alert('Falha na comunicação com o servidor.');
        } finally {
            setLoading(false);
        }
    };
    return (
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
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
                                        <IconButton onClick={() => handleActivateCampaign(c.id)} color="success" title="Ativar Campanha">
                                            <SendIcon />
                                        </IconButton>
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
                                options={educationalPages}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.educationalPageId}
                                onChange={(_, newValue) => setFormData({ ...formData, educationalPageId: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => <TextField {...params} label="Página Educativa" required />}
                            />

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
                        </Box>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeModal} color="inherit">Cancelar</Button>
                    <Button onClick={handleSave} color="primary" variant="contained" disabled={loading || loadingLookups}>
                        {loading ? 'Salvando...' : 'Salvar'}
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}
