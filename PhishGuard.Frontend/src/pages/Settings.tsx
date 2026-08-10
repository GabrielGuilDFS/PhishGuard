import { useState, useEffect } from 'react';
import { API_BASE, AUTH_API_BASE } from '../config';
import { authFetch, getSessionIdentity, getToken, setToken } from '../auth/session';
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
  nome: string;
  email: string;
  empresa: string;
}

interface ProfileUpdateResponse extends ProfileResponse {
  accessToken: string;
  expiresAtUtc: string;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: { xs: 2, md: 3 } }}>{children}</Box>}
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
  const [savingPersonalInfo, setSavingPersonalInfo] = useState(false);
  const [savingPassword, setSavingPassword] = useState(false);

  const handleChangeMode = (_event: React.MouseEvent<HTMLElement>, novoModo: AppThemeMode | null) => {
    if (!novoModo || novoModo === mode) return;
    setMode(novoModo);
    showNotify(`Modo ${novoModo === 'dark' ? 'escuro' : 'claro'} ativado.`, 'success');
  };

  const [profile, setProfile] = useState({
    nome: '',
    email: '',
    empresa: ''
  });
  const [password, setPassword] = useState({
    atual: '',
    nova: '',
    confirmacao: ''
  });

  useEffect(() => {
    const fetchProfile = async () => {
      const token = getToken();
      if (!token) return;

      try {
        const identity = getSessionIdentity(token);
        if (!identity) throw new Error('Token inválido.');
        if (identity.name || identity.email) {
          setProfile(prev => ({
            ...prev,
            nome: identity.name,
            email: identity.email
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
            email: data.email || prev.email,
            empresa: data.empresa || prev.empresa
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

  const updateProfile = async (payload: { nome?: string; senhaAtual?: string; novaSenha?: string }) => {
    try {
      const token = getToken();
      if (!token) {
        showNotify("Sessão expirada. Faça login novamente.", "error");
        return null;
      }

      const response = await authFetch(`${AUTH_API_BASE}/profile`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      if (!response.ok) {
        const data = await response.json().catch(() => null) as { message?: string; title?: string } | null;
        showNotify(data?.message ?? data?.title ?? "Não foi possível atualizar o perfil.", "error");
        return null;
      }

      const data = await response.json() as ProfileUpdateResponse;
      setToken(data.accessToken);
      setProfile(prev => ({
        ...prev,
        nome: data.nome,
        email: data.email,
        empresa: data.empresa || prev.empresa
      }));
      return data;
    } catch {
      showNotify("Erro de conexão ao salvar perfil.", "error");
      return null;
    }
  };

  const handleSavePersonalInfo = async (e: React.FormEvent) => {
    e.preventDefault();
    const nome = profile.nome.trim();
    if (nome.length < 2 || nome.length > 150) {
      showNotify("O nome deve ter entre 2 e 150 caracteres.", "error");
      return;
    }

    try {
      setSavingPersonalInfo(true);
      const updated = await updateProfile({ nome });
      if (updated) showNotify("Informações pessoais atualizadas com sucesso!", "success");
    } finally {
      setSavingPersonalInfo(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!password.atual) {
      showNotify("Informe a senha atual.", "error");
      return;
    }
    if (password.nova.length < 6 || password.nova.length > 100) {
      showNotify("A nova senha deve ter entre 6 e 100 caracteres.", "error");
      return;
    }
    if (password.nova !== password.confirmacao) {
      showNotify("A confirmação deve ser idêntica à nova senha.", "error");
      return;
    }

    try {
      setSavingPassword(true);
      const updated = await updateProfile({
        senhaAtual: password.atual,
        novaSenha: password.nova
      });
      if (updated) {
        setPassword({ atual: '', nova: '', confirmacao: '' });
        showNotify("Senha alterada. As outras sessões foram encerradas.", "success");
      }
    } finally {
      setSavingPassword(false);
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

  const passwordLengthValid = password.nova.length >= 6 && password.nova.length <= 100;
  const passwordConfirmationMatches = password.confirmacao.length > 0
    && password.nova === password.confirmacao;

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
            <Tab icon={<SettingsIcon />} iconPosition="start" label="Editar Perfil" />
            <Tab icon={<EmailIcon />} iconPosition="start" label="Entrega de E-mail" />
            <Tab icon={<PaletteIcon />} iconPosition="start" label="Aparência" />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
              gap: 2.5,
              alignItems: 'stretch'
            }}
          >
            <Paper
              component="form"
              variant="outlined"
              aria-labelledby="personal-info-title"
              onSubmit={handleSavePersonalInfo}
              sx={{ p: { xs: 2, sm: 3 }, display: 'flex', flexDirection: 'column', minWidth: 0 }}
            >
              <Box sx={{ mb: 2.5 }}>
                <Typography id="personal-info-title" variant="h6" sx={{ fontWeight: 750 }}>
                  Editar informações pessoais
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Atualize os dados do seu perfil administrativo. E-mail e empresa são somente leitura.
                </Typography>
              </Box>

              <Stack spacing={2} sx={{ flexGrow: 1 }}>
                <TextField
                  fullWidth
                  required
                  label="Nome do Administrador"
                  value={profile.nome}
                  inputProps={{ maxLength: 150 }}
                  onChange={(e) => setProfile({ ...profile, nome: e.target.value })}
                />
                <TextField
                  fullWidth
                  label="E-mail de Login"
                  value={profile.email}
                  helperText="O e-mail de acesso não pode ser alterado nesta tela."
                  slotProps={{ htmlInput: { readOnly: true } }}
                />
                <TextField
                  fullWidth
                  label="Empresa"
                  value={profile.empresa}
                  slotProps={{ htmlInput: { readOnly: true } }}
                />
              </Stack>

              <Button
                type="submit"
                variant="contained"
                disabled={savingPersonalInfo}
                startIcon={savingPersonalInfo ? <CircularProgress size={16} color="inherit" /> : <SaveIcon />}
                sx={{ mt: 3, alignSelf: { xs: 'stretch', sm: 'flex-start' } }}
              >
                {savingPersonalInfo ? 'Salvando...' : 'Salvar informações'}
              </Button>
            </Paper>

            <Paper
              component="form"
              variant="outlined"
              aria-labelledby="security-title"
              onSubmit={handleChangePassword}
              sx={{ p: { xs: 2, sm: 3 }, display: 'flex', flexDirection: 'column', minWidth: 0 }}
            >
              <Box sx={{ mb: 2.5 }}>
                <Typography id="security-title" variant="h6" sx={{ fontWeight: 750 }}>
                  Segurança
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Use a senha atual para confirmar uma alteração segura.
                </Typography>
              </Box>

              <Stack spacing={2}>
                <TextField
                  fullWidth
                  required
                  type="password"
                  label="Senha Atual"
                  autoComplete="current-password"
                  value={password.atual}
                  inputProps={{ maxLength: 100 }}
                  onChange={(e) => setPassword({ ...password, atual: e.target.value })}
                />
                <TextField
                  fullWidth
                  required
                  type="password"
                  label="Nova Senha"
                  autoComplete="new-password"
                  value={password.nova}
                  inputProps={{ minLength: 6, maxLength: 100 }}
                  onChange={(e) => setPassword({ ...password, nova: e.target.value })}
                />
                <TextField
                  fullWidth
                  required
                  type="password"
                  label="Confirmar Nova Senha"
                  autoComplete="new-password"
                  value={password.confirmacao}
                  error={password.confirmacao.length > 0 && !passwordConfirmationMatches}
                  helperText={password.confirmacao.length > 0 && !passwordConfirmationMatches
                    ? 'As senhas não coincidem.'
                    : 'Digite novamente a nova senha.'}
                  inputProps={{ minLength: 6, maxLength: 100 }}
                  onChange={(e) => setPassword({ ...password, confirmacao: e.target.value })}
                />
              </Stack>

              <Box
                aria-live="polite"
                sx={{ mt: 2.5, p: 2, borderRadius: 2, bgcolor: (theme) => alpha(theme.palette.primary.main, 0.06) }}
              >
                <Typography variant="subtitle2" sx={{ mb: 1 }}>Critérios de segurança</Typography>
                <Stack component="ul" spacing={0.75} sx={{ p: 0, m: 0, listStyle: 'none' }}>
                  <Typography
                    component="li"
                    variant="body2"
                    color={passwordLengthValid ? 'success.main' : 'text.secondary'}
                  >
                    {passwordLengthValid ? '✓' : '•'} Entre 6 e 100 caracteres
                  </Typography>
                  <Typography
                    component="li"
                    variant="body2"
                    color={passwordConfirmationMatches ? 'success.main' : 'text.secondary'}
                  >
                    {passwordConfirmationMatches ? '✓' : '•'} Confirmação idêntica à nova senha
                  </Typography>
                  <Typography component="li" variant="body2" color="text.secondary">
                    • Outras sessões serão encerradas após a alteração
                  </Typography>
                </Stack>
              </Box>

              <Button
                type="submit"
                variant="contained"
                disabled={savingPassword}
                startIcon={savingPassword ? <CircularProgress size={16} color="inherit" /> : <LockIcon />}
                sx={{ mt: 3, alignSelf: { xs: 'stretch', sm: 'flex-start' } }}
              >
                {savingPassword ? 'Alterando...' : 'Alterar senha'}
              </Button>
            </Paper>
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
