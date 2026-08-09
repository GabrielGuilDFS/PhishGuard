import { useState, useEffect } from 'react';
import { API_BASE, AUTH_API_BASE } from '../config';
import { authFetch, getToken } from '../auth/session';
import {
  Box,
  Typography,
  Paper,
  Tabs,
  Tab,
  TextField,
  Button,
  Stack,
  InputAdornment,
  Divider,
  ToggleButton,
  ToggleButtonGroup,
  Alert,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import LockIcon from '@mui/icons-material/Lock';
import EmailIcon from '@mui/icons-material/Email';
import SettingsIcon from '@mui/icons-material/Settings';
import SendIcon from '@mui/icons-material/Send';
import PaletteIcon from '@mui/icons-material/Palette';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import VpnKeyIcon from '@mui/icons-material/VpnKey';
import CloudQueueIcon from '@mui/icons-material/CloudQueue';
import { alpha } from '@mui/material/styles';
import { useNotify } from '../context/NotificationContext';
import { useThemeMode } from '../context/ThemeModeContext';
import type { AppThemeMode } from '../theme';
import PageContainer from '../components/PageContainer';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

interface SmtpStatus {
  configurado: boolean;
  providerType?: number;
  apiProvider?: number;
  senhaConfigurada: boolean;
  apiKeyConfigured?: boolean;
  transporteDisponivel: boolean;
  transporteIndisponivelMotivo?: string;
  ultimoTesteEmUtc?: string;
  ultimoTesteSucesso?: boolean | null;
  ultimoErroCodigo?: string;
}

interface EmailDeliveryConfigResponse {
  providerType: number;
  apiProvider: number;
  senderEmail: string;
  senderName: string;
  apiAccountIdentifier: string;
  apiRegion: string;
  host: string;
  porta: number;
  usuario: string;
}

interface ProfileResponse {
  nome?: string;
  email?: string;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

export default function Settings() {
  const { showNotify } = useNotify();
  const { mode, setMode } = useThemeMode();
  const [tabValue, setTabValue] = useState(
    () => new URLSearchParams(window.location.search).get('tab') === 'smtp' ? 1 : 0
  );
  const [smtpStatus, setSmtpStatus] = useState<SmtpStatus | null>(null);
  const [smtpDirty, setSmtpDirty] = useState(false);
  const [savingSmtp, setSavingSmtp] = useState(false);
  const [testingSmtp, setTestingSmtp] = useState(false);

  const handleChangeMode = (_event: React.MouseEvent<HTMLElement>, novoModo: AppThemeMode | null) => {
    if (!novoModo || novoModo === mode) return;
    setMode(novoModo);
    showNotify(`Modo ${novoModo === 'dark' ? 'escuro' : 'claro'} ativado.`, 'success');
  };

  const [profile, setProfile] = useState({
    nome: '',
    email: '',
    senhaAtual: '',
    novaSenha: ''
  });

  useEffect(() => {
    const fetchProfile = async () => {
      const token = getToken();
      if (!token) return;

      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const nameClaim = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload.unique_name || payload.name;
        const emailClaim = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || payload.email;
        if (nameClaim || emailClaim) {
          setProfile(prev => ({
            ...prev,
            nome: nameClaim || '',
            email: emailClaim || ''
          }));
        }
      } catch (e) {
        console.error("Erro ao decodificar token JWT", e);
      }

      try {
        const response = await authFetch(`${AUTH_API_BASE}/profile`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
        if (response.ok) {
          const data = await response.json() as ProfileResponse;
          setProfile(prev => ({
            ...prev,
            nome: data.nome || prev.nome,
            email: data.email || prev.email
          }));
        }
      } catch {
        console.warn("Rota GET /api/auth/profile não implementada. Usando claims do JWT.");
      }
    };

    fetchProfile();
  }, []);

  const [smtp, setSmtp] = useState({
    providerType: 0,
    apiProvider: 0,
    senderEmail: '',
    senderName: '',
    apiKey: '',
    apiAccountIdentifier: '',
    apiRegion: 'us-east-1',
    host: 'smtp.gmail.com',
    port: '587',
    user: '',
    password: '',
  });

  useEffect(() => {
    const fetchSmtpConfig = async () => {
      try {
        const token = getToken();
        if (!token) {
          showNotify("Sessão expirada. Faça login novamente.", "error");
          return;
        }

        const response = await authFetch(`${API_BASE}/email-delivery/config`, {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (response.ok) {
          const data = await response.json() as EmailDeliveryConfigResponse;
          setSmtp(prev => ({
            ...prev,
            providerType: data.providerType ?? 0,
            apiProvider: data.apiProvider ?? 0,
            senderEmail: data.senderEmail || '',
            senderName: data.senderName || '',
            host: data.host || '',
            port: data.porta ? data.porta.toString() : '587',
            user: data.usuario || '',
            password: prev.password,
            apiKey: prev.apiKey,
            apiAccountIdentifier: data.apiAccountIdentifier || '',
            apiRegion: data.apiRegion || 'us-east-1'
          }));
        }

        const statusResponse = await authFetch(`${API_BASE}/email-delivery/status`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (statusResponse.ok) setSmtpStatus(await statusResponse.json() as SmtpStatus);
      } catch (error) {
        console.error("Erro ao carregar configurações de e-mail", error);
      }
    };

    fetchSmtpConfig();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleChangeTab = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (profile.novaSenha && profile.novaSenha.length < 6) {
      showNotify("A nova senha deve ter no mínimo 6 caracteres", "error");
      return;
    }

    try {
      const token = getToken();
      if (!token) {
        showNotify("Sessão expirada. Faça login novamente.", "error");
        return;
      }

      const response = await authFetch(`${AUTH_API_BASE}/profile`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          nome: profile.nome,
          senhaAtual: profile.senhaAtual,
          novaSenha: profile.novaSenha
        })
      });

      if (response.ok) {
        showNotify("Perfil atualizado com sucesso!", "success");
        setProfile(prev => ({ ...prev, senhaAtual: '', novaSenha: '' }));
      } else {
        const errorText = await response.text();
        showNotify(`Erro ao atualizar perfil: ${errorText}`, "error");
      }
    } catch {
      showNotify("Erro de conexão ao salvar perfil.", "error");
    }
  };

  const handleSaveSmtp = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setSavingSmtp(true);
      const token = getToken();
      if (!token) {
        showNotify("Sessão expirada. Faça login novamente.", "error");
        return;
      }

      const porta = Number(smtp.port);
      if (smtp.providerType === 0 && (!Number.isInteger(porta) || porta <= 0 || porta > 65535)) {
        showNotify("A porta SMTP deve ser um número válido (1-65535).", "error");
        return;
      }
      if (smtp.providerType === 1 && !/^\S+@\S+\.\S+$/.test(smtp.senderEmail.trim())) {
        showNotify("Informe um e-mail de remetente autorizado válido.", "error");
        return;
      }
      if (smtp.providerType === 1
        && !smtp.apiKey
        && (!smtpStatus?.apiKeyConfigured
          || smtpStatus.providerType !== 1
          || smtpStatus.apiProvider !== smtp.apiProvider)) {
        showNotify("Informe a credencial do provedor selecionado.", "error");
        return;
      }
      if (smtp.providerType === 1 && smtp.apiProvider === 0 && !smtp.apiAccountIdentifier.trim()) {
        showNotify("Informe o AWS Access Key ID.", "error");
        return;
      }
      if (smtp.providerType === 1 && smtp.apiProvider === 4 && !/^\d+$/.test(smtp.apiAccountIdentifier.trim())) {
        showNotify("Informe o Sandbox ID numérico do Mailtrap.", "error");
        return;
      }

      const payload = {
        ProviderType: smtp.providerType,
        ApiProvider: smtp.apiProvider,
        SenderEmail: smtp.senderEmail,
        SenderName: smtp.senderName,
        ApiKey: smtp.apiKey,
        ApiAccountIdentifier: smtp.apiAccountIdentifier,
        ApiRegion: smtp.apiRegion,
        Host: smtp.host,
        Porta: porta,
        Usuario: smtp.user,
        Senha: smtp.password
      };

      const response = await authFetch(`${API_BASE}/email-delivery/config`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        const data = await response.json() as { status?: SmtpStatus };
        showNotify("Configurações de envio de e-mail salvas com sucesso!", "success");
        setSmtp(prev => ({ ...prev, password: '', apiKey: '' }));
        if (data.status) setSmtpStatus(data.status);
        setSmtpDirty(false);
      } else {
        const data = await response.json().catch(() => null) as { message?: string } | null;
        showNotify(data?.message ?? "Falha ao salvar as configurações. Verifique os dados.", "error");
      }
    } catch {
      showNotify("Erro de conexão com o servidor.", "error");
    } finally {
      setSavingSmtp(false);
    }
  };

  const handleTestEmail = async () => {
    if (smtpDirty) {
      showNotify("Salve as alterações antes de executar o teste.", "warning");
      return;
    }

    const emailDestino = window.prompt(
      smtp.apiProvider === 4
        ? "Digite o destinatário simulado que aparecerá no Mailtrap Sandbox:"
        : "Digite o e-mail que receberá a mensagem de teste do PhishGuard:"
    );

    if (!emailDestino) return;

    showNotify("Tentando enviar e-mail de teste...", "info");

    try {
      setTestingSmtp(true);
      const token = getToken();

      const response = await authFetch(`${API_BASE}/email-delivery/test`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          emailDestino: emailDestino
        })
      });

      if (response.ok) {
        const data = await response.json().catch(() => null) as { message?: string } | null;
        showNotify(
          data?.message ?? (smtp.apiProvider === 4
            ? "Mensagem capturada! Verifique o Mailtrap Sandbox."
            : "Teste de conexão bem-sucedido! Verifique a caixa de entrada."),
          "success"
        );
        setSmtpStatus(prev => prev ? { ...prev, ultimoTesteSucesso: true, ultimoTesteEmUtc: new Date().toISOString(), ultimoErroCodigo: undefined } : prev);
      } else {
        const data = await response.json().catch(() => null) as { code?: string; message?: string } | null;
        showNotify(data?.message ?? "Não foi possível concluir o teste de envio.", "error");
        setSmtpStatus(prev => prev ? { ...prev, ultimoTesteSucesso: false, ultimoTesteEmUtc: new Date().toISOString(), ultimoErroCodigo: data?.code } : prev);
      }
    } catch {
      showNotify("Erro de rede ao tentar contatar o servidor.", "error");
    } finally {
      setTestingSmtp(false);
    }
  };

  return (
    <PageContainer>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 3 }}>
        Configurações do Sistema
      </Typography>

      <Paper elevation={2}>
        <Box sx={{ borderBottom: 1, borderColor: (theme) => alpha(theme.palette.divider, 0.16) }}>
          <Tabs
            value={tabValue}
            onChange={handleChangeTab}
            aria-label="config tabs"
            textColor="primary"
            indicatorColor="primary"
          >
            <Tab icon={<SettingsIcon />} iconPosition="start" label="Meu Perfil" />
            <Tab icon={<EmailIcon />} iconPosition="start" label="Entrega de E-mail" />
            <Tab icon={<PaletteIcon />} iconPosition="start" label="Aparência" />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <Box component="form" onSubmit={handleSaveProfile} sx={{ maxWidth: 500 }}>
            <Typography variant="h6" gutterBottom>Dados de Acesso</Typography>
            <Typography variant="body2" color="textSecondary" mb={3}>
              Mantenha seus dados atualizados para garantir a segurança do painel.
            </Typography>

            <TextField
              fullWidth
              label="Nome do Administrador"
              margin="normal"
              value={profile.nome}
              onChange={(e) => setProfile({ ...profile, nome: e.target.value })}
            />
            <TextField
              fullWidth
              label="E-mail de Login"
              margin="normal"
              disabled
              value={profile.email}
            />

            <Divider sx={{ my: 3 }} />

            <Typography variant="h6" gutterBottom>Alterar Senha</Typography>
            <TextField
              fullWidth
              type="password"
              label="Senha Atual"
              margin="normal"
              value={profile.senhaAtual}
              onChange={(e) => setProfile({ ...profile, senhaAtual: e.target.value })}
            />
            <TextField
              fullWidth
              type="password"
              label="Nova Senha"
              margin="normal"
              value={profile.novaSenha}
              onChange={(e) => setProfile({ ...profile, novaSenha: e.target.value })}
            />

            <Box sx={{ mt: 3 }}>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />}>
                Salvar Alterações
              </Button>
            </Box>
          </Box>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Box component="form" onSubmit={handleSaveSmtp} sx={{ maxWidth: 600 }}>
            {smtpStatus && !smtpStatus.transporteDisponivel && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {smtpStatus.transporteIndisponivelMotivo}
              </Alert>
            )}
            {smtpStatus?.ultimoErroCodigo === 'SMTP_CREDENTIAL_UNREADABLE' && (
              <Alert severity="warning" sx={{ mb: 2 }}>
                A credencial salva pertence a uma chave antiga. Digite os dados novamente e salve a configuração.
              </Alert>
            )}
            <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }} useFlexGap>
              <Chip
                label={smtpStatus?.configurado ? 'Configuração salva' : 'Não configurado'}
                color={smtpStatus?.configurado ? 'success' : 'warning'}
                size="small"
              />
              {smtp.providerType === 0 && smtpStatus?.senhaConfigurada && <Chip label="Senha SMTP cadastrada" color="info" size="small" />}
              {smtp.providerType === 1 && smtpStatus?.apiKeyConfigured && <Chip label="API Key cadastrada" color="info" size="small" />}
              {smtp.providerType === 1 && smtpStatus?.transporteDisponivel && <Chip label="HTTPS disponível" color="success" size="small" />}
              {smtpDirty && <Chip label="Alterações não salvas" color="warning" size="small" />}
              {smtpStatus?.ultimoTesteSucesso === true && <Chip label="Último teste aprovado" color="success" size="small" />}
              {smtpStatus?.ultimoTesteSucesso === false && <Chip label="Último teste falhou" color="error" size="small" />}
            </Stack>

            <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
              <Box>
                <Typography variant="h6">Configuração de Disparo</Typography>
                <Typography variant="body2" color="textSecondary">
                  Defina o método de envio utilizado para transmitir as simulações de e-mail.
                </Typography>
              </Box>
              <Button
                onClick={handleTestEmail}
                color="secondary"
                disabled={testingSmtp || smtpDirty || !smtpStatus?.configurado || !smtpStatus?.transporteDisponivel}
                startIcon={testingSmtp ? <CircularProgress size={16} color="inherit" /> : <SendIcon />}
              >
                TESTAR CONEXÃO
              </Button>
            </Stack>

            <Box sx={{ mb: 3 }}>
              <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 'bold' }}>Método de Transporte</Typography>
              <ToggleButtonGroup
                value={smtp.providerType}
                exclusive
                onChange={(_e, val) => {
                  if (val !== null) {
                    setSmtp({ ...smtp, providerType: val });
                    setSmtpDirty(true);
                  }
                }}
                color="primary"
                fullWidth
              >
                <ToggleButton value={0}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <EmailIcon fontSize="small" />
                    <span>Servidor SMTP (Local / Relay)</span>
                  </Stack>
                </ToggleButton>
                <ToggleButton value={1}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <CloudQueueIcon fontSize="small" />
                    <span>API HTTPS (SaaS / Sandbox)</span>
                  </Stack>
                </ToggleButton>
              </ToggleButtonGroup>
            </Box>

            {smtp.providerType === 0 ? (
              <>
                <Stack direction="row" spacing={2}>
                  <TextField
                    fullWidth
                    label="Host SMTP"
                    placeholder="smtp.gmail.com"
                    margin="normal"
                    value={smtp.host}
                    onChange={(e) => { setSmtp({ ...smtp, host: e.target.value }); setSmtpDirty(true); }}
                  />
                  <TextField
                    sx={{ width: 150 }}
                    label="Porta"
                    placeholder="587"
                    type="number"
                    margin="normal"
                    value={smtp.port}
                    inputProps={{ min: 1, max: 65535, step: 1 }}
                    onChange={(e) => { setSmtp({ ...smtp, port: e.target.value }); setSmtpDirty(true); }}
                  />
                </Stack>

                <TextField
                  fullWidth
                  label="Usuário SMTP / E-mail"
                  margin="normal"
                  value={smtp.user}
                  onChange={(e) => { setSmtp({ ...smtp, user: e.target.value }); setSmtpDirty(true); }}
                />
                <TextField
                  fullWidth
                  type="password"
                  label="Senha / App Password"
                  margin="normal"
                  value={smtp.password}
                  placeholder={smtpStatus?.senhaConfigurada ? 'Deixe em branco para manter a senha atual' : undefined}
                  onChange={(e) => { setSmtp({ ...smtp, password: e.target.value }); setSmtpDirty(true); }}
                  InputProps={{
                    startAdornment: <InputAdornment position="start"><LockIcon fontSize="small" /></InputAdornment>,
                  }}
                />
              </>
            ) : (
              <>
                <FormControl fullWidth margin="normal">
                  <InputLabel id="api-provider-select-label">Provedor por API</InputLabel>
                  <Select
                    labelId="api-provider-select-label"
                    value={smtp.apiProvider}
                    label="Provedor por API"
                    onChange={(e) => {
                      setSmtp({
                        ...smtp,
                        apiProvider: Number(e.target.value),
                        apiKey: '',
                        apiAccountIdentifier: ''
                      });
                      setSmtpDirty(true);
                    }}
                  >
                    <MenuItem value={0}>AWS SES (HTTPS / Porto 443)</MenuItem>
                    <MenuItem value={1}>Postmark (HTTPS)</MenuItem>
                    <MenuItem value={2}>Brevo (HTTPS)</MenuItem>
                    <MenuItem value={3}>SendGrid (HTTPS)</MenuItem>
                    <MenuItem value={4}>Mailtrap Sandbox (HTTPS / não entrega externamente)</MenuItem>
                  </Select>
                </FormControl>

                {smtp.apiProvider === 4 && (
                  <Alert severity="info" sx={{ mt: 1, mb: 1 }}>
                    O Mailtrap Sandbox captura as mensagens para inspeção. Os endereços informados
                    são destinatários simulados e não receberão e-mails em suas caixas reais.
                  </Alert>
                )}

                <TextField
                  fullWidth
                  label={smtp.apiProvider === 4
                    ? 'E-mail do Remetente Simulado'
                    : 'E-mail do Remetente Autorizado'}
                  placeholder={smtp.apiProvider === 4
                    ? 'simulacoes@example.com'
                    : 'simulacoes@dominio-verificado.com'}
                  margin="normal"
                  value={smtp.senderEmail}
                  onChange={(e) => { setSmtp({ ...smtp, senderEmail: e.target.value }); setSmtpDirty(true); }}
                />

                <TextField
                  fullWidth
                  label="Nome de Exibição Padrão do Remetente"
                  placeholder="PhishGuard Security"
                  margin="normal"
                  value={smtp.senderName}
                  onChange={(e) => { setSmtp({ ...smtp, senderName: e.target.value }); setSmtpDirty(true); }}
                />

                {smtp.apiProvider === 0 && (
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    <TextField
                      fullWidth
                      label="AWS Access Key ID"
                      margin="normal"
                      value={smtp.apiAccountIdentifier}
                      onChange={(e) => { setSmtp({ ...smtp, apiAccountIdentifier: e.target.value }); setSmtpDirty(true); }}
                    />
                    <FormControl fullWidth margin="normal">
                      <InputLabel id="aws-region-select-label">Região AWS SES</InputLabel>
                      <Select
                        labelId="aws-region-select-label"
                        value={smtp.apiRegion}
                        label="Região AWS SES"
                        onChange={(e) => { setSmtp({ ...smtp, apiRegion: e.target.value }); setSmtpDirty(true); }}
                      >
                        <MenuItem value="us-east-1">US East (N. Virginia)</MenuItem>
                        <MenuItem value="us-east-2">US East (Ohio)</MenuItem>
                        <MenuItem value="us-west-2">US West (Oregon)</MenuItem>
                        <MenuItem value="sa-east-1">South America (São Paulo)</MenuItem>
                        <MenuItem value="eu-west-1">Europe (Ireland)</MenuItem>
                      </Select>
                    </FormControl>
                  </Stack>
                )}

                {smtp.apiProvider === 4 && (
                  <TextField
                    fullWidth
                    label="Mailtrap Sandbox ID"
                    helperText="Número exibido na URL da caixa: mailtrap.io/inboxes/{sandboxId}/messages"
                    inputMode="numeric"
                    margin="normal"
                    value={smtp.apiAccountIdentifier}
                    onChange={(e) => {
                      setSmtp({ ...smtp, apiAccountIdentifier: e.target.value.replace(/\D/g, '') });
                      setSmtpDirty(true);
                    }}
                  />
                )}

                <TextField
                  fullWidth
                  type="password"
                  label={smtp.apiProvider === 0
                    ? 'AWS Secret Access Key'
                    : smtp.apiProvider === 4
                      ? 'Mailtrap API Token'
                      : 'API Key do Provedor'}
                  margin="normal"
                  value={smtp.apiKey}
                  placeholder={smtpStatus?.apiKeyConfigured ? 'Deixe em branco para manter a API Key atual' : undefined}
                  onChange={(e) => { setSmtp({ ...smtp, apiKey: e.target.value }); setSmtpDirty(true); }}
                  InputProps={{
                    startAdornment: <InputAdornment position="start"><VpnKeyIcon fontSize="small" /></InputAdornment>,
                  }}
                />
              </>
            )}

            <Box sx={{ mt: 3 }}>
              <Button type="submit" variant="contained" disabled={savingSmtp || !smtpDirty} startIcon={savingSmtp ? <CircularProgress size={16} color="inherit" /> : <SaveIcon />}>
                Salvar Configurações de Envio
              </Button>
            </Box>
          </Box>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Box sx={{ maxWidth: 560 }}>
            <Typography variant="h6" gutterBottom>Tema do Painel</Typography>
            <Typography variant="body2" color="textSecondary" mb={3}>
              Escolha entre o modo claro e o modo escuro. A preferência é salva neste
              navegador e aplicada em todo o sistema com uma transição suave.
            </Typography>

            <ToggleButtonGroup
              value={mode}
              exclusive
              onChange={handleChangeMode}
              aria-label="modo de cor do painel"
              color="primary"
              sx={{ gap: 2, flexWrap: 'wrap' }}
            >
              <ToggleButton
                value="light"
                aria-label="modo claro"
                sx={{ px: 3, py: 2, borderRadius: 2, flexDirection: 'column', gap: 1, minWidth: 160 }}
              >
                <LightModeIcon />
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="subtitle2" sx={{ fontWeight: 700, textTransform: 'none' }}>
                    Modo Claro
                  </Typography>
                  <Typography variant="caption" sx={{ display: 'block', textTransform: 'none' }}>
                    Branco e azul
                  </Typography>
                </Box>
              </ToggleButton>
              <ToggleButton
                value="dark"
                aria-label="modo escuro"
                sx={{ px: 3, py: 2, borderRadius: 2, flexDirection: 'column', gap: 1, minWidth: 160 }}
              >
                <DarkModeIcon />
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="subtitle2" sx={{ fontWeight: 700, textTransform: 'none' }}>
                    Modo Escuro
                  </Typography>
                  <Typography variant="caption" sx={{ display: 'block', textTransform: 'none' }}>
                    Preto e azul neon
                  </Typography>
                </Box>
              </ToggleButton>
            </ToggleButtonGroup>
          </Box>
        </TabPanel>
      </Paper>
    </PageContainer>
  );
}
