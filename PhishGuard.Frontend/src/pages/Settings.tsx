import { useState } from 'react';
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
  Alert
} from '@mui/material';
import { 
  Save as SaveIcon, 
  Lock as LockIcon, 
  Email as EmailIcon, 
  Settings as SettingsIcon,
  Send as SendIcon
} from '@mui/icons-material';
import { useNotify } from '../context/NotificationContext'; 

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
  const [tabValue, setTabValue] = useState(0);

  const [profile, setProfile] = useState({
    nome: 'Administrador',
    email: 'admin@phishguard.com',
    senhaAtual: '',
    novaSenha: ''
  });

  const [smtp, setSmtp] = useState({
    host: 'smtp.gmail.com',
    port: '587',
    user: '',
    password: '',
    senderName: 'Suporte TI (Falso)',
    senderEmail: 'seguranca@empresa-alvo.com'
  });

  const handleChangeTab = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleSaveProfile = (e: React.FormEvent) => {
    e.preventDefault();
    if (profile.novaSenha && profile.novaSenha.length < 6) {
      showNotify("A nova senha deve ter no mínimo 6 caracteres", "error");
      return;
    }
    showNotify("Perfil atualizado com sucesso!");
  };

  const handleSaveSmtp = (e: React.FormEvent) => {
    e.preventDefault();
    showNotify("Configurações de SMTP salvas com sucesso!");
  };

  const handleTestEmail = () => {
    if (!smtp.user || !smtp.password) {
      showNotify("Preencha usuário e senha para testar", "warning");
      return;
    }
    showNotify("E-mail de teste enviado para seu admin!", "info");
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 3 }}>
        Configurações do Sistema
      </Typography>

      <Paper elevation={2}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs 
            value={tabValue} 
            onChange={handleChangeTab} 
            aria-label="config tabs"
            textColor="primary"
            indicatorColor="primary"
          >
            <Tab icon={<SettingsIcon />} iconPosition="start" label="Meu Perfil" />
            <Tab icon={<EmailIcon />} iconPosition="start" label="Servidor de E-mail (SMTP)" />
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
              onChange={(e) => setProfile({...profile, nome: e.target.value})}
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
              onChange={(e) => setProfile({...profile, senhaAtual: e.target.value})}
            />
            <TextField
              fullWidth
              type="password"
              label="Nova Senha"
              margin="normal"
              value={profile.novaSenha}
              onChange={(e) => setProfile({...profile, novaSenha: e.target.value})}
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
              <Button color="secondary" onClick={handleTestEmail} startIcon={<SendIcon />}>
                Testar Conexão
              </Button>
            </Stack>

            <Alert severity="info" sx={{ mb: 3 }}>
              Recomendamos usar um servidor SMTP dedicado ou uma conta Gmail configurada com "App Password".
            </Alert>

            <Stack direction="row" spacing={2}>
              <TextField
                fullWidth
                label="Host SMTP"
                placeholder="smtp.gmail.com"
                margin="normal"
                value={smtp.host}
                onChange={(e) => setSmtp({...smtp, host: e.target.value})}
              />
              <TextField
                sx={{ width: 150 }}
                label="Porta"
                placeholder="587"
                margin="normal"
                value={smtp.port}
                onChange={(e) => setSmtp({...smtp, port: e.target.value})}
              />
            </Stack>

            <TextField
              fullWidth
              label="Usuário SMTP / E-mail"
              margin="normal"
              value={smtp.user}
              onChange={(e) => setSmtp({...smtp, user: e.target.value})}
            />
            <TextField
              fullWidth
              type="password"
              label="Senha / App Password"
              margin="normal"
              value={smtp.password}
              onChange={(e) => setSmtp({...smtp, password: e.target.value})}
              InputProps={{
                startAdornment: <InputAdornment position="start"><LockIcon fontSize="small" /></InputAdornment>,
              }}
            />

            <Divider sx={{ my: 3 }} />

            <Typography variant="h6" gutterBottom>Identidade do Remetente (Spoofing)</Typography>
            <Typography variant="body2" color="textSecondary" mb={2}>
              Como o e-mail aparecerá na caixa de entrada da vítima.
            </Typography>

            <TextField
              fullWidth
              label="Nome do Remetente"
              placeholder="Ex: Suporte Microsoft"
              margin="normal"
              value={smtp.senderName}
              onChange={(e) => setSmtp({...smtp, senderName: e.target.value})}
            />
            <TextField
              fullWidth
              label="E-mail do Remetente (Reply-To)"
              placeholder="Ex: nao-responda@microsoft-security.com"
              margin="normal"
              value={smtp.senderEmail}
              onChange={(e) => setSmtp({...smtp, senderEmail: e.target.value})}
              helperText="Alguns servidores SMTP forçam o remetente real para evitar spam."
            />

            <Box sx={{ mt: 3 }}>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />}>
                Salvar Configurações SMTP
              </Button>
            </Box>
          </Box>
        </TabPanel>
      </Paper>
    </Box>
  );
}