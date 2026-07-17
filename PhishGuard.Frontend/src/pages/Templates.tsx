import { useState, useMemo } from 'react';
import {
  Button, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Dialog,
  DialogTitle, DialogContent, DialogActions, Stack,
  Chip, Tabs, Tab, ToggleButton, ToggleButtonGroup,
  TextField, MenuItem, Grid
} from '@mui/material';
import VisibilityIcon from '@mui/icons-material/Visibility';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import WebIcon from '@mui/icons-material/Web';
import { templatesPredefinidos } from '../data/predefinedTemplates';
import { simulationScenarios, type SimulationScenario } from '../data/predefinedTemplates';
import { landingTemplates } from '../data/landingTemplates';
import { educationalTemplates } from '../data/educationalTemplates';
import PageContainer from '../components/PageContainer';

// Biblioteca de Modelos — catálogo NAVEGÁVEL (somente leitura) dos moldes do sistema.
//
// Duas abas, ambas somente-leitura:
//   1. Cenários de Simulação  — preview emparelhado (E-mail SMTP ⇄ Página Falsa).
//   2. Páginas Educativas      — moldes pedagógicos + previewer.
//
// Um Cenário amarra estritamente uma isca de e-mail à sua página falsa (ver
// simulationScenarios em predefinedTemplates.ts). O admin NÃO registra nem descarta
// cenários por aqui: as linhas de Templates/PhishingPages que um cenário representa
// são provisionadas sob demanda (find-or-create) ao SALVAR a campanha que o usa
// (garantirCenario em Campaigns.tsx) — o mesmo padrão já adotado para as páginas
// educativas. Isso remove fricção sem propósito no modelo mental do usuário e evita
// exclusões acidentais direto desta listagem principal.
//
// Persistência por identificador (no fluxo de campanha): os registros guardam APENAS
// o id do molde (corpoHtml = id da isca; conteudoHtml = id da landing/educacional).
// O backend resolve o id de volta para o HTML no disparo.

// Índices auxiliares dos catálogos estáticos (id -> objeto).
const iscaPorId = new Map(templatesPredefinidos.map((i) => [i.id, i]));
const landingPorId = new Map(landingTemplates.map((l) => [l.id, l]));

export default function Templates() {
  const [aba, setAba] = useState(0);

  return (
    <PageContainer>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 1 }}>Biblioteca de Modelos</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Cenários amarram o e-mail à página falsa correspondente. As páginas educativas são fixas do sistema.
      </Typography>

      <Tabs value={aba} onChange={(_, v) => setAba(v)} sx={{ mb: 3 }}>
        <Tab label="Cenários de Simulação" icon={<MarkEmailReadIcon />} iconPosition="start" />
        <Tab label="Páginas Educativas" icon={<WebIcon />} iconPosition="start" />
      </Tabs>

      {aba === 0 && <CenariosTab />}
      {aba === 1 && <EducativasTab />}
    </PageContainer>
  );
}

// ===========================================================================
// Aba 1 — Cenários de Simulação (catálogo SOMENTE LEITURA)
// ===========================================================================
//
// Só a ação de Visualizar (preview emparelhado) sobrevive. Registrar e Descartar
// cenário foram removidos: a listagem principal é um catálogo, não um painel de
// escrita/exclusão. O par Template+PhishingPage nasce sob demanda ao salvar a
// campanha (ver garantirCenario em Campaigns.tsx).
function CenariosTab() {
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
      {/* Dica de interação sutil: a linha inteira é clicável (não há mais botão de ação). */}
      <Stack direction="row" alignItems="center" spacing={0.5} sx={{ mb: 1, color: 'text.secondary' }}>
        <VisibilityIcon sx={{ fontSize: 16 }} />
        <Typography variant="caption">
          Clique em um cenário para visualizar o preview (e-mail e página falsa).
        </Typography>
      </Stack>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          {/* O fundo do cabeçalho (Surface 2) vem do tema global — MuiTableCell.head. */}
          <TableHead>
            <TableRow>
              <TableCell align="center"><strong>Cenário</strong></TableCell>
              <TableCell align="center"><strong>Categoria</strong></TableCell>
              <TableCell align="center"><strong>Amarração (E-mail ⇄ Página Falsa)</strong></TableCell>
              {/* Coluna "Ações" removida — a affordance de clique vive na própria linha. */}
              <TableCell aria-hidden sx={{ width: 56 }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {simulationScenarios.map((s) => {
              const isca = iscaPorId.get(s.emailTemplateId);
              const landing = landingPorId.get(s.landingTemplateId);
              return (
                // Linha inteira clicável (mouse) + acessível por teclado (Enter/Espaço).
                // Affordance via Tailwind: cursor-pointer + hover suave no fundo (claro/escuro),
                // e o grupo nomeado `group/row` revela o ícone-olho e realça o título no hover.
                // Sem a prop `hover` do MUI de propósito: a emotion do MUI (sem @layer) venceria
                // o `hover:bg-*` do Tailwind (camada utilities, menor precedência).
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
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                    <Typography
                      variant="body2"
                      sx={{ fontWeight: 600 }}
                      className="transition-colors underline-offset-4 group-hover/row:text-accent group-hover/row:underline"
                    >
                      {s.nome}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">{s.descricao}</Typography>
                  </TableCell>
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                    <Chip label={s.categoria} size="small" variant="outlined" color="primary" />
                  </TableCell>
                  <TableCell align="center" sx={{ verticalAlign: 'middle' }}>
                    <Typography variant="caption" display="block">{isca?.nome ?? s.emailTemplateId}</Typography>
                    <Typography variant="caption" color="text.secondary" display="block">{landing?.nome ?? s.landingTemplateId}</Typography>
                  </TableCell>
                  {/* Ícone-olho discreto: invisível até o hover/focus da linha (group/row). */}
                  <TableCell align="right" sx={{ verticalAlign: 'middle', color: 'text.secondary' }}>
                    <VisibilityIcon
                      fontSize="small"
                      className="opacity-0 transition-opacity duration-200 group-hover/row:opacity-100 group-focus-visible/row:opacity-100"
                    />
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
        </DialogActions>
      </Dialog>
    </>
  );
}

// ===========================================================================
// Aba 2 — Páginas Educativas (catálogo fixo do sistema, SOMENTE LEITURA)
// ===========================================================================
//
// Decisão de UX: os moldes educativos são conteúdo FIXO do sistema (idênticos para
// todo tenant, não editáveis). Não há motivo para o admin "registrar" um molde: isso
// só criava uma linha em EducationalPages para servir de alvo da FK da campanha —
// fricção sem propósito no modelo mental do usuário. Removemos os botões de Registrar/
// Remover; a linha no banco passou a ser provisionada de forma transparente no momento
// da criação da campanha (find-or-create em Campaigns.tsx). Esta aba é agora apenas um
// catálogo navegável com preview.
function EducativasTab() {
  const [selecionado, setSelecionado] = useState(educationalTemplates[0]?.id ?? '');
  const molde = educationalTemplates.find((m) => m.id === selecionado);

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 5 }}>
        <Paper variant="outlined" sx={{ p: 3, borderRadius: 2 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 'bold', mb: 1 }}>Moldes Pedagógicos</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Catálogo fixo do sistema. Selecione uma abordagem para ler e visualizar. A página é
            associada automaticamente à campanha ao criá-la — não é preciso registrar nada aqui.
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

          {molde?.categoria && (
            <Stack direction="row" spacing={1} alignItems="center" sx={{ flexWrap: 'wrap', gap: 1 }}>
              <Chip label={molde.categoria} size="small" variant="outlined" color="primary" />
            </Stack>
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
