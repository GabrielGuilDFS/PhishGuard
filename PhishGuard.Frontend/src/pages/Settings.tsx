import { useState, useEffect } from 'react';
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
  Divider
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
  });

  useEffect(() => {
    const fetchSmtpConfig = async () => {
      try {
        const token = localStorage.getItem('phishguard_token'); 
        if (!token) {
          showNotify("Sessão expirada. Faça login novamente.", "error");
          return;
        }
        
        const response = await fetch('http://localhost:5000/api/SmtpConfig', {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
  
        if (response.ok) {
          const data = await response.json();
          setSmtp(prev => ({
            ...prev,
            host: data.Host || '',
            port: data.Porta ? data.Porta.toString() : '587',
            user: data.Usuario || '',
            password: prev.password
          }));
        }
      } catch (error) {
        console.error("Erro ao carregar configurações de SMTP", error);
      }
    };

    fetchSmtpConfig();
  }, []);

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

  const handleSaveSmtp = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const token = localStorage.getItem('phishguard_token');
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
  
      const response = await fetch('http://localhost:5000/api/SmtpConfig', {
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
    } catch (error) {
      showNotify("Erro de conexão com o servidor.", "error");
    }
  };

  const handleTestEmail = async () => {

    const emailDestino = window.prompt("Digite o e-mail que receberá a mensagem de teste do PhishGuard:");
    
    if (!emailDestino) return; 
  
    showNotify("Tentando enviar e-mail de teste...", "info");
  
    try {
      const token = localStorage.getItem('phishguard_token'); 
      
      const response = await fetch('http://localhost:5000/api/SmtpConfig/Testar', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ emailDestino: emailDestino }) 
      });
  
      if (response.ok) {
        showNotify("Teste de conexão bem-sucedido! Verifique a caixa de entrada.", "success");
      } else {
        // Se der erro (ex: senha errada), o backend vai mandar a mensagem no BadRequest
        const errorText = await response.text(); 
        showNotify(`Falha no envio: ${errorText}`, "error");
      }
    } catch (error) {
      showNotify("Erro de rede ao tentar contatar o servidor.", "error");
    }
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
                onChange={(e) => setSmtp({...smtp, host: e.target.value})}
              />
              <TextField
                sx={{ width: 150 }}
                label="Porta"
                placeholder="587"
                type="number"
                margin="normal"
                value={smtp.port}
                inputProps={{ min: 1, max: 65535, step: 1 }}
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