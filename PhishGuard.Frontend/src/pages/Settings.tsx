import { useState, useEffect } from 'react';
import { API_BASE } from '../config';
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
  ToggleButtonGroup
} from '@mui/material';
// Deep imports (não o barrel `@mui/icons-material`): named imports do barrel quebram
// o Vitest no Windows com EMFILE (milhares de ícones abertos de uma vez) — gotcha já
// documentado no projeto.
import SaveIcon from '@mui/icons-material/Save';
import LockIcon from '@mui/icons-material/Lock';
import EmailIcon from '@mui/icons-material/Email';
import SettingsIcon from '@mui/icons-material/Settings';
import SendIcon from '@mui/icons-material/Send';
import PaletteIcon from '@mui/icons-material/Palette';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
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

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

export default function Settings() {
  const { showNotify } = useNotify();
  const { mode, setMode } = useThemeMode();
  const [tabValue, setTabValue] = useState(0);

  const handleChangeMode = (_event: React.MouseEvent<HTMLElement>, novoModo: AppThemeMode | null) => {
    // ToggleButtonGroup emite null quando o usuário clica no botão já ativo — ignora
    // para nunca ficar sem tema selecionado.
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
        const response = await authFetch(`${API_BASE}/Auth/profile`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
        if (response.ok) {
          const data = await response.json();
          setProfile(prev => ({
            ...prev,
            nome: data.nome || prev.nome,
            email: data.email || prev.email
          }));
        }
      } catch {
        console.warn("Rota GET /api/Auth/profile não implementada. Usando claims do JWT.");
      }
    };

    fetchProfile();
  }, []);

  const [smtp, setSmtp] = useState({
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

        const response = await authFetch(`${API_BASE}/SmtpConfig`, {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (response.ok) {
          const data = await response.json();
          // A API serializa em camelCase (host/porta/usuario), não PascalCase.
          // A senha nunca volta do backend (por segurança), então preserva a digitada.
          setSmtp(prev => ({
            ...prev,
            host: data.host || '',
            port: data.porta ? data.porta.toString() : '587',
            user: data.usuario || '',
            password: prev.password
          }));
        }
      } catch (error) {
        console.error("Erro ao carregar configurações de SMTP", error);
      }
    };

    fetchSmtpConfig();
    // Carga inicial: roda uma unica vez na montagem. `showNotify` vem do contexto e nao
    // deve reexecutar a busca de configuracao de SMTP quando a identidade dela mudar.
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

      const response = await authFetch(`${API_BASE}/Auth/profile`, {
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
      const token = getToken();
      if (!token) {
        showNotify("Sessão expirada. Faça login novamente.", "error");
        return;
      }

      const porta = Number(smtp.port);
      if (!Number.isInteger(porta) || porta <= 0 || porta > 65535) {
        showNotify("A porta SMTP deve ser um número válido (1-65535).", "error");
        return;
      }

      const payload = {
        Host: smtp.host,
        Porta: porta,
        Usuario: smtp.user,
        Senha: smtp.password
      };

      const response = await authFetch(`${API_BASE}/SmtpConfig`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        showNotify("Configurações de SMTP salvas com sucesso!", "success");
        // Opcional: limpar o campo de senha da tela após salvar, já que o C# processou
        setSmtp(prev => ({ ...prev, password: '' }));
      } else {
        showNotify("Falha ao salvar as configurações. Verifique os dados.", "error");
      }
    } catch {
      showNotify("Erro de conexão com o servidor.", "error");
    }
  };

  const handleTestEmail = async () => {

    const emailDestino = window.prompt("Digite o e-mail que receberá a mensagem de teste do PhishGuard:");

    if (!emailDestino) return;

    showNotify("Tentando enviar e-mail de teste...", "info");

    try {
      const token = getToken();

      const response = await authFetch(`${API_BASE}/SmtpConfig/Testar`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          host: smtp.host,
          porta: Number(smtp.port) || 0,
          usuario: smtp.user,
          senha: smtp.password,
          emailDestino: emailDestino
        })
      });

      if (response.ok) {
        showNotify("Teste de conexão bem-sucedido! Verifique a caixa de entrada.", "success");
      } else {
        // Se der erro (ex: senha errada), o backend vai mandar a mensagem no BadRequest
        const errorText = await response.text();
        showNotify(`Falha no envio: ${errorText}`, "error");
      }
    } catch {
      showNotify("Erro de rede ao tentar contatar o servidor.", "error");
    }
  };

  return (
    <PageContainer>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 3 }}>
        Configurações do Sistema
      </Typography>

      {/* Sem borda: `elevation={2}` já dá ao painel uma sombra suave, suficiente para
          delimitá-lo do fundo (mesmo neutro que a página) sem precisar de um contorno
          — a mesma lógica "flutuante" aplicada ao header/sidebar do AdminLayout. */}
      <Paper elevation={2}>
        {/* Separador funcional (faixa de abas → conteúdo): mantém a borda, mas diluída
            — mesmo alpha usado nos <Divider /> do AdminLayout, já que aqui ela também
            está cercada de conteúdo e não precisa competir por atenção como o contorno
            externo do painel. */}
        <Box sx={{ borderBottom: 1, borderColor: (theme) => alpha(theme.palette.divider, 0.16) }}>
          <Tabs
            value={tabValue}
            onChange={handleChangeTab}
            aria-label="config tabs"
            textColor="primary"
            indicatorColor="primary"
          >
            <Tab icon={<SettingsIcon />} iconPosition="start" label="Meu Perfil" />
            <Tab icon={<EmailIcon />} iconPosition="start" label="Servidor de E-mail (SMTP)" />
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
            <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
              <Box>
                <Typography variant="h6">Configuração de Disparo</Typography>
                <Typography variant="body2" color="textSecondary">
                  Defina qual servidor será usado para enviar os ataques simulados.
                </Typography>
              </Box>
              <Button
                onClick={handleTestEmail}
                color="secondary"
                startIcon={<SendIcon />} // Ou o ícone que você estiver usando
              >
                TESTAR CONEXÃO
              </Button>
            </Stack>

            <Stack direction="row" spacing={2}>
              <TextField
                fullWidth
                label="Host SMTP"
                placeholder="smtp.gmail.com"
                margin="normal"
                value={smtp.host}
                onChange={(e) => setSmtp({ ...smtp, host: e.target.value })}
              />
              <TextField
                sx={{ width: 150 }}
                label="Porta"
                placeholder="587"
                type="number"
                margin="normal"
                value={smtp.port}
                inputProps={{ min: 1, max: 65535, step: 1 }}
                onChange={(e) => setSmtp({ ...smtp, port: e.target.value })}
              />
            </Stack>

            <TextField
              fullWidth
              label="Usuário SMTP / E-mail"
              margin="normal"
              value={smtp.user}
              onChange={(e) => setSmtp({ ...smtp, user: e.target.value })}
            />
            <TextField
              fullWidth
              type="password"
              label="Senha / App Password"
              margin="normal"
              value={smtp.password}
              onChange={(e) => setSmtp({ ...smtp, password: e.target.value })}
              InputProps={{
                startAdornment: <InputAdornment position="start"><LockIcon fontSize="small" /></InputAdornment>,
              }}
            />

            <Box sx={{ mt: 3 }}>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />}>
                Salvar Configurações SMTP
              </Button>
            </Box>
          </Box>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Box sx={{ maxWidth: 560 }}>
            {/* Card de destaque com um dos gradientes da paleta (azul-escuro nos dois
                modos → conteúdo em branco fixo, não no `text` do modo). */}
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
