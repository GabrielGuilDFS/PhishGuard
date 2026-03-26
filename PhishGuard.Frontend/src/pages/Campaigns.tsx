import { useState, useEffect } from 'react';
import {
    Container, Typography, Box, Button, TextField, Table, TableBody,
    TableCell, TableContainer, TableHead, TableRow, Paper, IconButton,
    Dialog, DialogTitle, DialogContent, DialogActions, Autocomplete, CircularProgress
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SendIcon from '@mui/icons-material/Send';


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
    email?: string; // Adicionado email para os targets
}

export default function Campaigns() {
    const [campaigns, setCampaigns] = useState<Campaign[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(false);

    const [open, setOpen] = useState(false);
    const [currentId, setCurrentId] = useState<string | null>(null);

    // 1. ESTADO ATUALIZADO: Adicionado targetsSelecionados como array
    const [formData, setFormData] = useState({
        nomeCampanha: '',
        dataInicio: '',
        dataFim: '',
        emailTemplateId: null as LookupItem | null,
        landingPageId: null as LookupItem | null,
        educationalPageId: null as LookupItem | null,
        targetsSelecionados: [] as LookupItem[]
    });

    const [templates, setTemplates] = useState<LookupItem[]>([]);
    const [phishingPages, setPhishingPages] = useState<LookupItem[]>([]);
    const [educationalPages, setEducationalPages] = useState<LookupItem[]>([]);
    const [targets, setTargets] = useState<LookupItem[]>([]); // Estado para os funcionários
    const [loadingLookups, setLoadingLookups] = useState(false);

    const API_BASE = 'http://localhost:5000/api';

    const token = localStorage.getItem('phishguard_token');
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

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

    const openModal = async (campaign?: any) => {
        let loadedTemplates = templates;
        let loadedPhishingPages = phishingPages;
        let loadedEduPages = educationalPages;
        let loadedTargets = targets;

        if (templates.length === 0) {
            setLoadingLookups(true);
            try {
                // 2. BUSCA: Adicionado fetch de Targets no Promise.all
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

                    // 3. PREENCHER NA EDIÇÃO: Transforma os IDs que vieram do banco nos objetos selecionados
                    const alvosPreSelecionados = loadedTargets.filter(t => data.targetIds?.includes(t.id));

                    setFormData({
                        nomeCampanha: data.nomeCampanha || '',
                        dataInicio: data.dataInicio ? data.dataInicio.split('T')[0] : '',
                        dataFim: data.dataFim ? data.dataFim.split('T')[0] : '',
                        emailTemplateId: loadedTemplates.find(t => t.id === data.emailTemplateId) || { id: data.emailTemplateId, nome: data.templateNome },
                        landingPageId: loadedPhishingPages.find(p => p.id === data.landingPageId) || { id: data.landingPageId, nome: data.landingPageNome },
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
                dataInicio: new Date().toISOString().split('T')[0],
                dataFim: '',
                emailTemplateId: null,
                landingPageId: null,
                educationalPageId: null,
                targetsSelecionados: []
            });
        }
        setOpen(true);
    };

    const closeModal = () => setOpen(false);

    const handleSave = async () => {
        // Validando se selecionou pelo menos um alvo
        if (!formData.nomeCampanha || !formData.dataInicio || !formData.emailTemplateId || !formData.landingPageId || !formData.educationalPageId || formData.targetsSelecionados.length === 0) {
            alert('Preencha os campos obrigatórios e selecione pelo menos um alvo.');
            return;
        }

        // 4. MONTAR PAYLOAD: Extrai apenas os IDs do array de objetos
        const payload = {
            nomeCampanha: formData.nomeCampanha,
            dataInicio: new Date(formData.dataInicio).toISOString(),
            dataFim: formData.dataFim ? new Date(formData.dataFim).toISOString() : null,
            emailTemplateId: formData.emailTemplateId.id,
            landingPageId: formData.landingPageId.id,
            educationalPageId: formData.educationalPageId.id,
            targetIds: formData.targetsSelecionados.map(t => t.id) // <-- Isso é o que o C# espera!
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

    const handleStartCampaign = async (id: string) => {
        if (!window.confirm('Deseja realmente iniciar o disparo desta campanha? Os e-mails serão enviados.')) return;

        try {
            setLoading(true);
            const res = await fetch(`${API_BASE}/Campaigns/${id}/iniciar`, {
                method: 'POST',
                headers
            });

            if (res.ok) {
                alert('Disparo iniciado com sucesso! Verifique o Mailtrap.');
                fetchCampaigns(); // Atualiza a tabela para mostrar o status "Em Andamento"
            } else {
                const err = await res.text();
                alert(`Erro ao iniciar: ${err}`);
            }
        } catch (error) {
            console.error('Erro ao iniciar disparo', error);
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
                            <TableCell>Data Início</TableCell>
                            <TableCell>Data Fim</TableCell>
                            <TableCell>Template</TableCell>
                            <TableCell>Página Falsa</TableCell>
                            <TableCell>Página Educacional</TableCell>
                            <TableCell align="center">Ações</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {filtered.map(c => (
                            <TableRow key={c.id}>
                                <TableCell>{c.nomeCampanha}</TableCell>
                                <TableCell>{c.status}</TableCell>
                                <TableCell>{new Date(c.dataInicio).toLocaleDateString()}</TableCell>
                                <TableCell>{c.dataFim ? new Date(c.dataFim).toLocaleDateString() : '-'}</TableCell>
                                <TableCell>{c.templateNome}</TableCell>
                                <TableCell>{c.landingPageNome}</TableCell>
                                <TableCell>{c.educationalPageNome}</TableCell>
                                <TableCell align="center">
                                    {c.status === 'Rascunho' && (
                                        <IconButton onClick={() => handleStartCampaign(c.id)} color="success" title="Iniciar Disparo">
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
                                    label="Data Início"
                                    type="date"
                                    fullWidth
                                    required
                                    InputLabelProps={{ shrink: true }}
                                    value={formData.dataInicio}
                                    onChange={e => setFormData({ ...formData, dataInicio: e.target.value })}
                                />
                                <TextField
                                    label="Data Fim"
                                    type="date"
                                    fullWidth
                                    InputLabelProps={{ shrink: true }}
                                    value={formData.dataFim}
                                    onChange={e => setFormData({ ...formData, dataFim: e.target.value })}
                                />
                            </Box>

                            <Autocomplete
                                options={templates}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.emailTemplateId}
                                onChange={(_, newValue) => setFormData({ ...formData, emailTemplateId: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => <TextField {...params} label="Template de E-mail" required />}
                            />

                            <Autocomplete
                                options={phishingPages}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.landingPageId}
                                onChange={(_, newValue) => setFormData({ ...formData, landingPageId: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => <TextField {...params} label="Página Falsa (Landing Page)" required />}
                            />

                            <Autocomplete
                                options={educationalPages}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.educationalPageId}
                                onChange={(_, newValue) => setFormData({ ...formData, educationalPageId: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => <TextField {...params} label="Página Educacional" required />}
                            />

                            {/* O seu componente Múltiplo agora tem todos os dados que precisa! */}
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