import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
  Typography,
} from '@mui/material';

interface MetricsGlossaryDialogProps {
  open: boolean;
  onClose: () => void;
}

const METRICS = [
  {
    name: 'E-mails Enviados',
    formula: 'Pares únicos de campanha + destinatário com envio SMTP bem-sucedido.',
    note: 'O mesmo destinatário em campanhas diferentes conta uma vez em cada campanha. Reenvios da mesma campanha não duplicam a métrica.',
  },
  {
    name: 'Taxa de Abertura',
    formula: 'Aberturas efetivas ÷ e-mails enviados × 100.',
    note: 'Aberturas efetivas unem pixels observados e aberturas inferidas por ações posteriores. Na tabela, “abriram sem clicar” representa a diferença entre aberturas efetivas e cliques únicos da campanha; o resultado é agregado e não identifica pessoas.',
  },
  {
    name: 'Taxa de Clique',
    formula: 'Cliques únicos por campanha e destinatário ÷ e-mails enviados × 100.',
    note: 'Cliques repetidos no mesmo link não aumentam o indicador.',
  },
  {
    name: 'Taxa de Comprometimento',
    formula: 'Submissões únicas ÷ e-mails enviados × 100.',
    note: 'Somente o evento é registrado; e-mail e senha digitados não são armazenados.',
  },
  {
    name: 'Aprendizado Concluído',
    formula: 'Treinamentos concluídos ÷ participantes que clicaram × 100.',
    note: 'Conclusões repetidas para a mesma campanha e destinatário contam uma única vez.',
  },
  {
    name: 'Acesso Educacional',
    formula: 'Visualizações únicas da página educacional ÷ e-mails enviados × 100.',
    note: 'A visualização é registrada separadamente do clique no botão de conclusão.',
  },
  {
    name: 'Abandono Educacional',
    formula: 'Pessoas que visualizaram a página e não concluíram ÷ visualizações educacionais × 100.',
    note: 'É uma diferença entre conjuntos agregados; não expõe a identidade dos participantes.',
  },
  {
    name: 'Recuperação após Comprometimento',
    formula: 'Participantes comprometidos que concluíram o treinamento ÷ comprometidos × 100.',
    note: 'Compara os eventos da mesma campanha e do mesmo destinatário, sempre no recorte selecionado.',
  },
];

export default function MetricsGlossaryDialog({ open, onClose }: MetricsGlossaryDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="metrics-glossary-title"
    >
      <DialogTitle id="metrics-glossary-title">Como as métricas são calculadas?</DialogTitle>
      <DialogContent dividers>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2.5 }}>
          Período e departamento são aplicados a todas as métricas. O gráfico agrega todas as campanhas que tiveram envios dentro do recorte selecionado.
        </Typography>
        <Stack divider={<Divider flexItem />} spacing={2}>
          {METRICS.map((metric) => (
            <Box key={metric.name}>
              <Typography component="h3" variant="subtitle2" sx={{ fontWeight: 750 }}>
                {metric.name}
              </Typography>
              <Typography variant="body2" sx={{ mt: 0.5 }}>{metric.formula}</Typography>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                {metric.note}
              </Typography>
            </Box>
          ))}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Fechar</Button>
      </DialogActions>
    </Dialog>
  );
}
