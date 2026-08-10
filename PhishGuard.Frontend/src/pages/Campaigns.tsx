import { useState, useEffect, useMemo, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { API_BASE } from '../config';
import { authFetch, getToken } from '../auth/session';
import {
    Typography, Box, Button, TextField, Table, TableBody,
    TableCell, TableContainer, TableHead, TableRow, Paper, IconButton,
    Dialog, DialogTitle, DialogContent, DialogActions, Autocomplete,
    CircularProgress, Stack, Divider, Chip, Tooltip, Alert
} from '@mui/material';
import PageContainer from '../components/PageContainer';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SendIcon from '@mui/icons-material/Send';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import ClearAllIcon from '@mui/icons-material/ClearAll';
import ReplayIcon from '@mui/icons-material/Replay';
import { simulationScenarios, templatesPredefinidos, type SimulationScenario } from '../data/predefinedTemplates';
import { landingTemplates } from '../data/landingTemplates';
import { paginaEducativaPrecisaReconciliar, resolverPaginaEducativaDoCenario } from '../data/feedbackTrainings';
import { CampaignStatus } from '../data/campaignStatus';
import { useNotify } from '../context/NotificationContext';
import { toDateTimeLocal } from '../utils/dateTime';

// Índices dos catálogos estáticos (id -> objeto), para provisionar sob demanda as
// linhas (Template + PhishingPage) que um cenário representa — ver garantirCenario.
const iscaPorId = new Map(templatesPredefinidos.map((i) => [i.id, i]));
const landingPorId = new Map(landingTemplates.map((l) => [l.id, l]));


interface Campaign {
    id: string;
    nomeCampanha: string;
    status: string;
    dataInicio: string;
    dataFim?: string;
    templateNome: string;
    landingPageNome: string;
    educationalPageNome: string;
    dispatchErrorCode?: string;
    dispatchErrorMessage?: string;
    dispatchFailedAtUtc?: string;
}

interface SmtpStatus {
    configurado: boolean;
    transporteDisponivel: boolean;
    transporteIndisponivelMotivo?: string;
    ultimoErroCodigo?: string;
}

interface LookupItem {
    id: string;
    nome: string;
    email?: string;
    departamento?: string;
    corpoHtml?: string;    // Templates: carrega o id da isca de e-mail.
    conteudoHtml?: string; // PhishingPages/EducationalPages: id da landing / html educativo.
}

// Resumo da importação de CSV (Fase 4 — feedback ao usuário).
interface ImportSummary {
    processados: number;
    importados: number;
    duplicados: number;
    invalidos: number;
}

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

// Traduz erros crus de transporte em mensagens amigáveis sem presumir SMTP: o tenant
// também pode estar usando uma API HTTPS (Mailtrap, SES, Brevo etc.).
const mensagemAmigavel = (erro: string): string => {
    const e = (erro || '').toLowerCase();
    if (/smtp|conex|connect|socket|autentic|\bauth|mailkit|\bhost\b|servidor de e-?mail|timeout/.test(e)) {
        return 'Falha no serviço de entrega de e-mail. Verifique o provedor configurado em Configurações.';
    }
    return erro || 'Ocorreu um erro inesperado. Tente novamente.';
};

const emailValido = (v: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);

// ---- Atualização reativa do status (short polling inteligente) ----
// Intervalo de checagem enquanto houver campanha "em trânsito" de status. 8s fica na
// faixa pedida (5–10s): responsivo o suficiente para o disparo (worker roda a cada 1min)
// sem martelar a API.
const POLL_INTERVAL_MS = 8000;
// Janela em que a badge "pisca" após uma transição de status (casa com o fade de 1s do tema).
const FLASH_MS = 1800;

// Status que mantêm o polling LIGADO — a campanha ainda vai mudar sozinha:
//   Agendada  → o worker dispara ao chegar a DataInicio (vira Processando);
//   Processando → o lote está sendo enviado (vira Em Andamento ao concluir).
// Em Andamento/Finalizada/Rascunho são estáveis do ponto de vista do timer: não geram polling.
const STATUS_PENDENTES: string[] = [CampaignStatus.Agendada, CampaignStatus.Processando];

// Cor semântica da badge por status (paleta do tema; ver theme/themeHelper.ts).
const STATUS_CHIP_COLOR: Record<string, 'default' | 'warning' | 'info' | 'success'> = {
    [CampaignStatus.Rascunho]: 'default',
    [CampaignStatus.Agendada]: 'warning',    // aguardando o horário
    [CampaignStatus.Processando]: 'info',     // disparando o lote
    [CampaignStatus.FalhaNoDisparo]: 'warning',
    [CampaignStatus.EmAndamento]: 'success',  // disparado / coletando
    [CampaignStatus.Finalizada]: 'default',   // encerrada
};

// Badge de status com transição suave de cor (1s, padrão do tema) e um "flash" discreto
// quando o status ACABOU de mudar — feedback claro de que a tela atualizou em tempo real.
function StatusBadge({ status, flashing }: { status: string; flashing: boolean }) {
    const color = STATUS_CHIP_COLOR[status] ?? 'default';
    return (
        <Chip
            label={status}
            color={color}
            size="small"
            variant={status === CampaignStatus.Finalizada ? 'outlined' : 'filled'}
            sx={{
                fontWeight: 600,
                // Cor/borda migram no mesmo 1s do fade global de tema (cubic-bezier premium).
                transition: 'background-color 1s cubic-bezier(0.4,0,0.2,1), color 1s cubic-bezier(0.4,0,0.2,1), border-color 1s cubic-bezier(0.4,0,0.2,1)',
                // Pisca 2x (halo + brilho) só no instante da transição; removido após FLASH_MS.
                ...(flashing && {
                    animation: 'statusFlash 800ms ease-in-out 2',
                    '@keyframes statusFlash': {
                        '0%, 100%': { boxShadow: '0 0 0 0 rgba(102,130,245,0)', filter: 'brightness(1)' },
                        '50%': { boxShadow: '0 0 0 4px rgba(102,130,245,0.45)', filter: 'brightness(1.18)' },
                    },
                }),
            }}
        />
    );
}

// Célula "Encerramento da Coleta". Três casos, para nunca ficar vazia/quebrada:
//  - com prazo + já coletando (Em Andamento/Finalizada) → só a data real;
//  - com prazo + ainda não ativada (Rascunho/Agendada)  → data + tag "prevista" (o prazo
//    só começa a valer ao ativar) — é a "previsão" de quando a coleta expirará;
//  - sem prazo (DataFim nula) → indicador sutil "Sem prazo" (coleta indefinida), não '-'.
function ColetaFimCell({ campanha }: { campanha: Campaign }) {
    // Aceita QUALQUER data válida vinda da listagem (string ISO do GET /api/Campaigns).
    // A condicional é frouxa de propósito: qualquer valor que o `new Date` consiga
    // interpretar é tratado como prazo definido. Só cai em "Sem prazo" quando o campo é
    // genuinamente ausente/nulo/vazio OU uma string inválida (nunca renderiza "Invalid Date").
    // ⚠️ Se o campo vier `undefined` (ausente do payload da LISTA), isto também exibirá
    //    "Sem prazo" — nesse caso o problema é o endpoint não estar mandando `dataFim`,
    //    não esta renderização (o GET /api/Campaigns/{id} do modal manda; a lista precisa
    //    mandar igual). Ver CampaignsController.GetCampaigns.
    const data = campanha.dataFim ? new Date(campanha.dataFim) : null;
    const temPrazoValido = data !== null && !Number.isNaN(data.getTime());

    if (!temPrazoValido) {
        return (
            <Tooltip title="Sem data de encerramento: a coleta fica ativa por tempo indeterminado até ser encerrada manualmente.">
                <Chip
                    label="Sem prazo"
                    size="small"
                    variant="outlined"
                    sx={{ color: 'text.secondary', borderStyle: 'dashed' }}
                />
            </Tooltip>
        );
    }

    const quando = data.toLocaleString();
    const aindaNaoColeta =
        campanha.status === CampaignStatus.Rascunho || campanha.status === CampaignStatus.Agendada;

    return (
        <Stack direction="row" spacing={0.75} alignItems="center" sx={{ flexWrap: 'wrap' }}>
            <Typography variant="body2">{quando}</Typography>
            {aindaNaoColeta && (
                <Tooltip title="Data prevista: a coleta será encerrada neste horário assim que a campanha for ativada.">
                    <Chip
                        label="prevista"
                        size="small"
                        color="warning"
                        variant="outlined"
                        sx={{ height: 20, fontSize: '0.68rem' }}
                    />
                </Tooltip>
            )}
        </Stack>
    );
}

export default function Campaigns() {
    const { showNotify } = useNotify();
    const [searchParams, setSearchParams] = useSearchParams();

    const [campaigns, setCampaigns] = useState<Campaign[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(false);
    const [campaignsLoaded, setCampaignsLoaded] = useState(false);
    const [activatingId, setActivatingId] = useState<string | null>(null);
    const [smtpStatus, setSmtpStatus] = useState<SmtpStatus | null>(null);

    const [open, setOpen] = useState(false);
    const [currentId, setCurrentId] = useState<string | null>(null);

    const [formData, setFormData] = useState({
        nomeCampanha: '',
        dataInicio: '',
        dataFim: '',
        cenario: null as SimulationScenario | null,
        targetsSelecionados: [] as LookupItem[]
    });

    const [templates, setTemplates] = useState<LookupItem[]>([]);
    const [phishingPages, setPhishingPages] = useState<LookupItem[]>([]);
    const [educationalPages, setEducationalPages] = useState<LookupItem[]>([]);
    const [targets, setTargets] = useState<LookupItem[]>([]);
    const [loadingLookups, setLoadingLookups] = useState(false);
    const [importSummary, setImportSummary] = useState<ImportSummary | null>(null);
    const csvInputRef = useRef<HTMLInputElement>(null);

    // Reatividade do status: ids que devem "piscar" (transição recém-detectada) e o mapa
    // do último status conhecido por campanha (para diferenciar mudanças entre polls).
    const [flashingIds, setFlashingIds] = useState<Set<string>>(new Set());
    const statusAnteriorRef = useRef<Map<string, string>>(new Map());
    const routeActionRef = useRef<string | null>(null);

    const token = getToken();
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    // Departamentos distintos cadastrados no tenant (para seleção por setor).
    const departamentos = useMemo<string[]>(() => {
        const set = new Set<string>();
        targets.forEach((t) => { if (t.departamento) set.add(t.departamento); });
        return Array.from(set).sort((a, b) => a.localeCompare(b, 'pt-BR'));
    }, [targets]);

    // Carga inicial: roda uma unica vez na montagem. `fetchCampaigns` e' recriada a cada
    // render (fecha sobre `headers`), entao inclui-la nas deps refaria a busca em loop.
    useEffect(() => {
        fetchCampaigns();
        fetchSmtpStatus();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const fetchSmtpStatus = async () => {
        try {
            const res = await authFetch(`${API_BASE}/email-delivery/status`, { headers });
            if (res.ok) setSmtpStatus(await res.json() as SmtpStatus);
        } catch {
            setSmtpStatus(null);
        }
    };

    // Existe alguma campanha "em trânsito" (Agendada/Processando)? Controla o polling.
    const temPendentes = useMemo(
        () => campaigns.some(c => STATUS_PENDENTES.includes(c.status)),
        [campaigns]
    );

    // POLLING INTELIGENTE: só cria o intervalo quando há campanhas pendentes; quando a
    // última pendente resolve, `temPendentes` vira false, este efeito re-executa e o
    // cleanup limpa o intervalo imediatamente (não fica batendo na API à toa). Também é
    // limpo ao desmontar a tela.
    useEffect(() => {
        if (!temPendentes) return;
        const intervalo = window.setInterval(() => { fetchCampaigns(); }, POLL_INTERVAL_MS);
        return () => window.clearInterval(intervalo);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [temPendentes]);

    const fetchCampaigns = async () => {
        try {
            const res = await authFetch(`${API_BASE}/Campaigns`, { headers });
            if (!res.ok) return;
            const novas: Campaign[] = await res.json();

            // Detecta transições de status vs. o último snapshot conhecido. Só conta como
            // mudança quando já havia um status anterior (não pisca tudo na 1ª carga).
            const mudaram = novas
                .filter(c => {
                    const anterior = statusAnteriorRef.current.get(c.id);
                    return anterior !== undefined && anterior !== c.status;
                })
                .map(c => c.id);

            statusAnteriorRef.current = new Map(novas.map(c => [c.id, c.status]));
            setCampaigns(novas);

            // Dispara o "flash" nas linhas que mudaram e o remove após a janela de animação.
            if (mudaram.length > 0) {
                setFlashingIds(prev => new Set([...prev, ...mudaram]));
                window.setTimeout(() => {
                    setFlashingIds(prev => {
                        const s = new Set(prev);
                        mudaram.forEach(id => s.delete(id));
                        return s;
                    });
                }, FLASH_MS);
            }
        } catch (error) {
            console.error('Erro ao buscar campanhas', error);
            showNotify('Não foi possível carregar as campanhas.', 'error');
        } finally {
            setCampaignsLoaded(true);
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
                    authFetch(`${API_BASE}/Templates`, { headers }),
                    authFetch(`${API_BASE}/PhishingPages`, { headers }),
                    authFetch(`${API_BASE}/EducationalPages`, { headers }),
                    authFetch(`${API_BASE}/Targets`, { headers })
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
                const res = await authFetch(`${API_BASE}/Campaigns/${campaign.id}`, { headers });
                if (res.ok) {
                    const data = await res.json();
                    setCurrentId(data.id);

                    const alvosPreSelecionados = loadedTargets.filter(t => data.targetIds?.includes(t.id));

                    // A campanha persiste o id da LINHA de Template; mapeia-o de volta ao
                    // cenário estático pelo corpoHtml (= id da isca) que a linha carrega.
                    const emailRowDaCampanha = loadedTemplates.find((t) => t.id === data.emailTemplateId);
                    const cenarioReconstruido = emailRowDaCampanha
                        ? simulationScenarios.find((s) => s.emailTemplateId === emailRowDaCampanha.corpoHtml) ?? null
                        : null;

                    // A página educativa NÃO é mais escolhida manualmente: ela deriva 1:1 do
                    // cenário (feedbackTemplateId) e é provisionada no salvar — nada a reconstruir aqui.
                    setFormData({
                        nomeCampanha: data.nomeCampanha || '',
                        dataInicio: toDateTimeLocal(data.dataInicio),
                        dataFim: toDateTimeLocal(data.dataFim),
                        cenario: cenarioReconstruido,
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
                targetsSelecionados: []
            });
        }
        setOpen(true);
    };

    const closeModal = () => setOpen(false);

    // Deep-link vindo do dashboard. A query é consumida uma única vez e removida com
    // replace: o histórico não ganha uma entrada extra e a listagem não é recarregada.
    useEffect(() => {
        const nova = searchParams.get('nova') === '1';
        const editId = searchParams.get('editar')?.trim() || null;
        const actionKey = nova ? 'nova' : editId ? `editar:${editId}` : null;
        if (!actionKey || routeActionRef.current === actionKey) return;
        if (editId && !campaignsLoaded) return;

        const limparQueryDaAcao = () => {
            const next = new URLSearchParams(searchParams);
            next.delete('nova');
            next.delete('editar');
            setSearchParams(next, { replace: true });
        };

        if (editId) {
            const campaign = campaigns.find((item) => item.id === editId);
            if (!campaign) {
                routeActionRef.current = actionKey;
                showNotify('Campanha não encontrada.', 'error');
                limparQueryDaAcao();
                return;
            }
            routeActionRef.current = actionKey;
            void openModal(campaign).finally(limparQueryDaAcao);
            return;
        }

        routeActionRef.current = actionKey;
        void openModal().finally(limparQueryDaAcao);
        // openModal fecha sobre os lookups atuais; incluir a função nas dependências
        // recriaria o efeito a cada render. actionKey+routeActionRef garantem consumo único.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [campaigns, campaignsLoaded, searchParams, setSearchParams, showNotify]);

    // Provisiona sob demanda o PAR de linhas (Template + PhishingPage) que o cenário
    // estático representa — find-or-create, espelhando garantirPaginaEducativa. Substitui
    // o antigo botão "Registrar" da Biblioteca de Modelos: o admin escolhe o cenário no
    // catálogo e as linhas nascem de forma transparente ao salvar a campanha.
    // Persistência por identificador: corpoHtml = id da isca, conteudoHtml = id da landing.
    const garantirCenario = async (s: SimulationScenario): Promise<{ emailRowId: string; landingRowId: string }> => {
        const isca = iscaPorId.get(s.emailTemplateId);
        const landing = landingPorId.get(s.landingTemplateId);
        if (!isca || !landing) throw new Error('Cenário inválido: molde não encontrado no catálogo.');

        let emailRow = templates.find((t) => t.corpoHtml === s.emailTemplateId);
        if (!emailRow) {
            const res = await authFetch(`${API_BASE}/Templates`, {
                method: 'POST', headers,
                body: JSON.stringify({
                    nome: isca.nome, assunto: isca.assunto,
                    remetenteNome: isca.remetenteNome, remetenteEmail: isca.remetenteEmail,
                    corpoHtml: isca.id,
                }),
            });
            if (!res.ok) throw new Error(await extrairMensagemDeErro(res));
            const nova = await res.json();
            emailRow = { id: nova.id, nome: isca.nome, corpoHtml: isca.id };
            setTemplates(prev => [...prev, emailRow!]);
        }

        let landingRow = phishingPages.find((p) => p.conteudoHtml === s.landingTemplateId);
        if (!landingRow) {
            const res = await authFetch(`${API_BASE}/PhishingPages`, {
                method: 'POST', headers,
                body: JSON.stringify({ nome: landing.nome, conteudoHtml: landing.id }),
            });
            if (!res.ok) throw new Error(await extrairMensagemDeErro(res));
            const nova = await res.json();
            landingRow = { id: nova.id, nome: landing.nome, conteudoHtml: landing.id };
            setPhishingPages(prev => [...prev, landingRow!]);
        }

        return { emailRowId: emailRow.id, landingRowId: landingRow.id };
    };

    // Provisiona a linha de EducationalPages sob demanda (find-or-create), derivando o
    // conteúdo do CENÁRIO (vínculo 1:1 via feedbackTemplateId) — não há mais escolha manual.
    // A FK educationalPageId é obrigatória no backend; esta linha a satisfaz de forma
    // transparente. Idempotência pela chave estável `descritor.html` (uma linha por treinamento).
    const garantirPaginaEducativa = async (cenario: SimulationScenario): Promise<string> => {
        const descritor = resolverPaginaEducativaDoCenario(cenario);

        const existente = educationalPages.find(p => p.conteudoHtml === descritor.html);
        if (existente) {
            // O catálogo é a fonte canônica do rótulo. Páginas antigas podem manter
            // nomes de migrations de descontinuação mesmo após o cenário ser reativado.
            // Reconcilia no mesmo registro para preservar FKs, histórico e idempotência.
            if (paginaEducativaPrecisaReconciliar(
                { nome: existente.nome, html: existente.conteudoHtml ?? '' },
                descritor,
            )) {
                const res = await authFetch(`${API_BASE}/EducationalPages/${existente.id}`, {
                    method: 'PUT', headers,
                    body: JSON.stringify({ nome: descritor.nome, conteudoHtml: descritor.html }),
                });
                if (!res.ok) throw new Error(await extrairMensagemDeErro(res));

                setEducationalPages(prev => prev.map(p =>
                    p.id === existente.id ? { ...p, nome: descritor.nome } : p));
            }
            return existente.id;
        }

        const res = await authFetch(`${API_BASE}/EducationalPages`, {
            method: 'POST', headers,
            body: JSON.stringify({ nome: descritor.nome, conteudoHtml: descritor.html }),
        });
        if (!res.ok) throw new Error(await extrairMensagemDeErro(res));
        const nova = await res.json();
        setEducationalPages(prev => [...prev, { id: nova.id, nome: nova.nome, conteudoHtml: descritor.html }]);
        return nova.id;
    };

    const handleSave = async () => {
        if (!formData.nomeCampanha || !formData.dataInicio || !formData.cenario ||
            formData.targetsSelecionados.length === 0) {
            showNotify('Preencha os campos obrigatórios (Cenário) e selecione ao menos um alvo.', 'error');
            return;
        }

        setLoading(true);
        try {
            // Provisiona (find-or-create) o par de linhas do cenário e a página educativa
            // ANTES de montar o payload — substitui o registro manual da Biblioteca. A página
            // educativa deriva 1:1 do próprio cenário (feedbackTemplateId), sem escolha manual.
            const { emailRowId, landingRowId } = await garantirCenario(formData.cenario);
            const educationalPageId = await garantirPaginaEducativa(formData.cenario);

            const payload = {
                nomeCampanha: formData.nomeCampanha,
                dataInicio: new Date(formData.dataInicio).toISOString(),
                dataFim: formData.dataFim ? new Date(formData.dataFim).toISOString() : null,
                emailTemplateId: emailRowId,
                landingPageId: landingRowId,
                educationalPageId,
                targetIds: formData.targetsSelecionados.map(t => t.id)
            };

            const method = currentId ? 'PUT' : 'POST';
            const url = currentId ? `${API_BASE}/Campaigns/${currentId}` : `${API_BASE}/Campaigns`;

            const res = await authFetch(url, { method, headers, body: JSON.stringify(payload) });
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
            const res = await authFetch(`${API_BASE}/Campaigns/${id}`, { method: 'DELETE', headers });
            if (res.ok) { showNotify('Campanha excluída.', 'success'); fetchCampaigns(); }
            else showNotify(mensagemAmigavel(await extrairMensagemDeErro(res)), 'error');
        } catch (error) {
            console.error('Erro ao deletar', error);
            showNotify('Falha na comunicação com o servidor.', 'error');
        }
    };

    const filtered = campaigns.filter(c => c.nomeCampanha.toLowerCase().includes(searchTerm.toLowerCase()));
    const smtpBlockingMessage = smtpStatus?.transporteIndisponivelMotivo
        ?? (smtpStatus?.ultimoErroCodigo === 'SMTP_CREDENTIAL_UNREADABLE'
            ? 'A credencial de entrega salva não pode ser decifrada após o deploy. Informe-a novamente e salve a configuração.'
            : 'A entrega de e-mail ainda não foi configurada. Configure SMTP ou uma API HTTPS antes de ativar uma campanha.');

    const handleActivateCampaign = async (id: string) => {
        if (!smtpStatus?.configurado || !smtpStatus.transporteDisponivel) {
            showNotify(
                smtpBlockingMessage,
                'error'
            );
            return;
        }
        if (!window.confirm('Deseja ativar esta campanha? Se a data de início já chegou, os e-mails serão disparados agora; caso contrário, ela ficará Agendada até o horário.')) return;

        setActivatingId(id);
        try {
            const res = await authFetch(`${API_BASE}/Campaigns/${id}/ativar`, { method: 'POST', headers });
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

    const handleRetryDispatch = async (id: string) => {
        setActivatingId(id);
        try {
            const res = await authFetch(`${API_BASE}/Campaigns/${id}/retry-dispatch`, { method: 'POST', headers });
            if (!res.ok) {
                showNotify(mensagemAmigavel(await extrairMensagemDeErro(res)), 'error');
                return;
            }
            const data = await res.json().catch(() => null);
            showNotify(data?.message ?? 'Campanha reenfileirada.', 'success');
            await fetchCampaigns();
        } catch {
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

            {smtpStatus && (!smtpStatus.configurado || !smtpStatus.transporteDisponivel) && (
                <Alert
                    severity="error"
                    action={<Button color="inherit" size="small" href="/admin/settings?tab=smtp">Configurar entrega</Button>}
                    sx={{ mb: 2 }}
                >
                    {smtpBlockingMessage}
                </Alert>
            )}

            <TextField
                label="Buscar"
                variant="outlined"
                fullWidth
                margin="normal"
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                sx={{ mb: 3 }}
            />

            {/* Aviso: a exclusão de campanhas é destrutiva e retira os logs históricos que
                alimentam os gráficos/KPIs do dashboard (cliques únicos, funil, etc.). */}
            <Alert severity="warning" sx={{ mb: 3 }}>
                Atenção: A exclusão permanente de campanhas antigas impactará diretamente os dados e o histórico exibidos nos gráficos da dashboard.
            </Alert>

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
                                <TableCell>
                                    <Stack spacing={0.5} alignItems="flex-start">
                                        <StatusBadge status={c.status} flashing={flashingIds.has(c.id)} />
                                        {c.status === CampaignStatus.FalhaNoDisparo && c.dispatchErrorMessage && (
                                            <Tooltip title={c.dispatchErrorMessage}>
                                                <Typography variant="caption" color="error" sx={{ maxWidth: 180 }} noWrap>
                                                    {c.dispatchErrorMessage}
                                                </Typography>
                                            </Tooltip>
                                        )}
                                    </Stack>
                                </TableCell>
                                <TableCell>{new Date(c.dataInicio).toLocaleString()}</TableCell>
                                <TableCell><ColetaFimCell campanha={c} /></TableCell>
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
                                    {c.status === CampaignStatus.FalhaNoDisparo && (
                                        <Tooltip title="Tentar o disparo novamente">
                                            <span>
                                                <IconButton
                                                    onClick={() => handleRetryDispatch(c.id)}
                                                    color="warning"
                                                    disabled={activatingId === c.id || !smtpStatus?.configurado || !smtpStatus?.transporteDisponivel}
                                                >
                                                    {activatingId === c.id ? <CircularProgress size={20} /> : <ReplayIcon />}
                                                </IconButton>
                                            </span>
                                        </Tooltip>
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
                                options={simulationScenarios}
                                getOptionLabel={(option) => option.nome || ''}
                                value={formData.cenario}
                                onChange={(_, newValue) => setFormData({ ...formData, cenario: newValue })}
                                isOptionEqualToValue={(option, value) => option.id === value?.id}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Cenário de Simulação"
                                        required
                                        helperText="Amarra o e-mail, a página falsa e a tela educacional (associada automaticamente ao cenário)."
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
