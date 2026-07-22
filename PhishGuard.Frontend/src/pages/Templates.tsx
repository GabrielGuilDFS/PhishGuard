import { useState, useMemo } from 'react';
import {
  Button, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Dialog,
  DialogTitle, DialogContent, DialogActions, Stack,
  Chip, Tabs, Tab,
} from '@mui/material';
import VisibilityIcon from '@mui/icons-material/Visibility';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import WebIcon from '@mui/icons-material/Web';
import SchoolIcon from '@mui/icons-material/School';
import { templatesPredefinidos } from '../data/predefinedTemplates';
import { simulationScenarios, type SimulationScenario } from '../data/predefinedTemplates';
import { landingTemplates } from '../data/landingTemplates';
import { educationalTemplates } from '../data/educationalTemplates';
import { feedbackTrainings } from '../data/feedbackTrainings';
import FeedbackTraining from '../components/FeedbackTraining';
import PageContainer from '../components/PageContainer';
import { formatarExpiracaoLink, formatarDataAcessoBRT } from '../utils/emailExpiration';

// Biblioteca de Modelos — catálogo NAVEGÁVEL (somente leitura) dos moldes do sistema.
//
// FLUXO ÚNICO E DIRETO (refatoração de navegação): não há mais abas de topo. A tela é
// uma tabela única de Cenários; clicar num cenário abre um diálogo com TRÊS abas de
// preview emparelhado — E-mail, Página Falsa e Página Educacional (o treinamento que
// o alvo vê ao final da simulação). As antigas abas "Cenários de Simulação" e "Páginas
// Educativas" foram fundidas neste fluxo, eliminando redundância de navegação.
//
// Um Cenário amarra estritamente uma isca de e-mail à sua página falsa e ao seu molde
// educacional (ver simulationScenarios em predefinedTemplates.ts). O admin NÃO registra
// nem descarta cenários por aqui: as linhas de Templates/PhishingPages/EducationalPages
// que um cenário representa são provisionadas sob demanda (find-or-create) ao SALVAR a
// campanha que o usa (garantirCenario/garantirPaginaEducativa em Campaigns.tsx).
//
// Persistência por identificador (no fluxo de campanha): os registros guardam APENAS o
// id do molde. O backend resolve o id de volta para o HTML no disparo.

// Índices auxiliares dos catálogos estáticos (id -> objeto).
const iscaPorId = new Map(templatesPredefinidos.map((i) => [i.id, i]));
const landingPorId = new Map(landingTemplates.map((l) => [l.id, l]));

// Molde educacional padrão quando o cenário não declara um (paridade com o
// redirecionamento das landings ainda não migradas, que apontam para este molde).
const FEEDBACK_PADRAO = 'basico_phishing';

type PreviewTab = 'email' | 'landing' | 'educational';

export default function Templates() {
  const [preview, setPreview] = useState<SimulationScenario | null>(null);
  const [tab, setTab] = useState<PreviewTab>('email');

  const abrirPreview = (s: SimulationScenario) => { setTab('email'); setPreview(s); };

  // srcDoc dos previews de E-mail e Página Falsa (HTML estático isolado no iframe).
  const previewSrcDoc = useMemo(() => {
    if (!preview) return '';
    if (tab === 'email') {
      const isca = iscaPorId.get(preview.emailTemplateId);
      if (!isca) return '<div style="padding:24px;font-family:sans-serif;color:#999;">Isca não encontrada.</div>';
      return isca.corpoHtml
        .replaceAll('{{LINK_PHISHING}}', '#')
        .replaceAll('{{LINK}}', '#')
        .replaceAll('{{NOME}}', 'Colaborador(a)')
        .replaceAll('{{DATA_EXPIRACAO}}', formatarExpiracaoLink())
        .replaceAll('{{DATA_ACESSO}}', formatarDataAcessoBRT());
    }
    if (tab === 'landing') {
      const landing = landingPorId.get(preview.landingTemplateId);
      if (!landing) return '<div style="padding:24px;font-family:sans-serif;color:#999;">Página falsa não encontrada.</div>';
      return landing.html.replace(/{{CAMPAIGN_ID}}/g, '').replace(/{{TARGET_ID}}/g, '');
    }
    return '';
  }, [preview, tab]);

  // A Página Educacional resolve o treinamento do cenário:
  //  - se houver um treinamento INTERATIVO (Just-in-Time Training), renderiza o
  //    componente <FeedbackTraining> DIRETO (modo preview) — sem recarregar o app
  //    num iframe de rota (frágil e dependente de fallback de SPA no host);
  //  - senão, cai no molde educacional ESTÁTICO correspondente (iframe srcDoc).
  const feedbackConfig = useMemo(() => {
    if (!preview) return undefined;
    const tid = preview.feedbackTemplateId ?? FEEDBACK_PADRAO;
    return feedbackTrainings[tid];
  }, [preview]);

  const educationalStaticSrcDoc = useMemo(() => {
    if (!preview || feedbackConfig) return '';
    const tid = preview.feedbackTemplateId ?? FEEDBACK_PADRAO;
    const molde = educationalTemplates.find((m) => m.id === tid) ?? educationalTemplates[0];
    return molde?.html ?? '<div style="padding:24px;font-family:sans-serif;color:#999;">Treinamento não encontrado.</div>';
  }, [preview, feedbackConfig]);

  return (
    <PageContainer>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 1 }}>Biblioteca de Modelos</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Cada cenário amarra o e-mail, a página falsa e o treinamento educacional correspondentes.
      </Typography>

      {/* Dica de interação: a linha inteira é clicável (sem botão de ação dedicado). */}
      <Stack direction="row" alignItems="center" spacing={0.5} sx={{ mb: 1, color: 'text.secondary' }}>
        <VisibilityIcon sx={{ fontSize: 16 }} />
        <Typography variant="caption">
          Clique em um cenário para visualizar o e-mail, a página falsa e o treinamento.
        </Typography>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          {/* O fundo do cabeçalho (Surface 2) vem do tema global — MuiTableCell.head. */}
          <TableHead>
            <TableRow>
              <TableCell align="center"><strong>Cenário</strong></TableCell>
              <TableCell align="center"><strong>Categoria</strong></TableCell>
              <TableCell align="center"><strong>Observação</strong></TableCell>
              {/* Coluna do ícone-olho — a affordance de clique vive na própria linha. */}
              <TableCell aria-hidden sx={{ width: 56 }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {simulationScenarios.map((s) => (
              // Linha inteira clicável (mouse) + acessível por teclado (Enter/Espaço).
              // Affordance via Tailwind: cursor-pointer + hover suave no fundo (claro/escuro);
              // o grupo `group/row` revela o ícone-olho no hover. Sem a prop `hover` do MUI
              // de propósito: a emotion do MUI (sem @layer) venceria o `hover:bg-*` do
              // Tailwind (camada utilities, menor precedência).
              <TableRow
                key={s.id}
                onClick={() => abrirPreview(s)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); abrirPreview(s); }
                }}
                role="button"
                tabIndex={0}
                aria-label={`Visualizar ${s.nome}`}
                className="group/row cursor-pointer transition-colors hover:bg-neutral-100/50 dark:hover:bg-neutral-900/50 focus-visible:outline-none focus-visible:bg-neutral-100/50 dark:focus-visible:bg-neutral-900/50"
              >
                {/* Coluna Cenário: EXCLUSIVAMENTE o título (sem descrição/subtexto).
                    Hover apenas realça a cor — sem underline. */}
                <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                  <Typography
                    variant="body2"
                    sx={{ fontWeight: 600 }}
                    className="transition-colors group-hover/row:text-accent"
                  >
                    {s.nome}
                  </Typography>
                </TableCell>
                <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                  <Chip label={s.categoria} size="small" variant="outlined" color="primary" />
                </TableCell>
                {/* Observação: nota descritiva do cenário (substitui a antiga "Amarração"). */}
                <TableCell align="center" sx={{ verticalAlign: 'middle', maxWidth: 420 }}>
                  <Typography variant="caption" color="text.secondary">{s.descricao}</Typography>
                </TableCell>
                {/* Ícone-olho discreto: invisível até o hover/focus da linha (group/row). */}
                <TableCell align="right" sx={{ verticalAlign: 'middle', color: 'text.secondary' }}>
                  <VisibilityIcon
                    fontSize="small"
                    className="opacity-0 transition-opacity duration-200 group-hover/row:opacity-100 group-focus-visible/row:opacity-100"
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={Boolean(preview)} onClose={() => setPreview(null)} fullWidth maxWidth="lg">
        <DialogTitle>{preview?.nome}</DialogTitle>
        <DialogContent dividers>
          <Tabs
            value={tab}
            onChange={(_, v: PreviewTab) => setTab(v)}
            variant="fullWidth"
            sx={{ mb: 2 }}
          >
            <Tab value="email" label="E-mail" icon={<MarkEmailReadIcon />} iconPosition="start" />
            <Tab value="landing" label="Página Falsa" icon={<WebIcon />} iconPosition="start" />
            <Tab value="educational" label="Página Educacional" icon={<SchoolIcon />} iconPosition="start" />
          </Tabs>

          <Paper
            variant="outlined"
            sx={{ height: '560px', overflow: tab === 'educational' && feedbackConfig ? 'auto' : 'hidden', borderRadius: 2 }}
          >
            {tab === 'educational' ? (
              feedbackConfig ? (
                // Treinamento INTERATIVO renderizado DIRETO (modo preview): sem recarregar
                // o app num iframe de rota. As iscas exibidas dentro dele já ficam em
                // iframes `sandbox=""` internos, então o HTML das armadilhas segue isolado.
                <FeedbackTraining config={feedbackConfig} preview />
              ) : (
                // Fallback: molde educacional estático (HTML) isolado no iframe.
                <iframe
                  title="Educational Preview"
                  srcDoc={educationalStaticSrcDoc}
                  sandbox="allow-same-origin"
                  style={{ width: '100%', height: '100%', border: 'none' }}
                />
              )
            ) : (
              <iframe
                title={tab === 'email' ? 'Email Preview' : 'Landing Preview'}
                srcDoc={previewSrcDoc}
                // Segurança (XSS): 'allow-same-origin' SEM 'allow-scripts' isola o HTML
                // clonado — scripts injetados não executam e não conseguem ler o JWT em
                // localStorage do painel administrativo.
                sandbox="allow-same-origin"
                style={{ width: '100%', height: '100%', border: 'none' }}
              />
            )}
          </Paper>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setPreview(null)} color="inherit">Fechar</Button>
        </DialogActions>
      </Dialog>
    </PageContainer>
  );
}
