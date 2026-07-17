import { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  List,
  Typography,
  Divider,
  IconButton,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  CssBaseline
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { clearSession } from '../auth/session';
// Deep imports (não o barrel `@mui/icons-material`): named imports do barrel quebram
// o Vitest no Windows com EMFILE (milhares de ícones abertos de uma vez) — gotcha já
// documentado no projeto.
import MenuIcon from '@mui/icons-material/Menu';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import SendIcon from '@mui/icons-material/Send';
import LogoutIcon from '@mui/icons-material/Logout';
import SettingsIcon from '@mui/icons-material/Settings';
import FolderIcon from '@mui/icons-material/FolderCopy';

const drawerWidth = 260;

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogout = () => {
    // Destruição completa da sessão (não só o token): ver src/auth/session.ts.
    clearSession();
    navigate('/login');
  };

  // A antiga "Biblioteca de Recursos" (Cenários + Páginas Falsas + Educacionais) foi
  // fundida numa única tela tabulada em /admin/templates (Cenários de Simulação e
  // Páginas Educativas). A aba independente de "Páginas Falsas" deixou de existir.
  const menuItems = [
    { text: 'Visão Geral', icon: <DashboardIcon />, path: '/admin/dashboard' },
    { text: 'Gestão de Alvos', icon: <PeopleIcon />, path: '/admin/targets' },
    { text: 'Biblioteca de Modelos', icon: <FolderIcon />, path: '/admin/templates' },
    { text: 'Nova Campanha', icon: <SendIcon />, path: '/admin/campaigns' },
    { text: 'Configurações', icon: <SettingsIcon />, path: '/admin/settings' }
  ];

  const drawer = (
    <div>
      {/* Cabeçalho de marca: sem fundo sólido — herda o `background.paper` do Drawer
          (branco no light, preto no dark). "Phish" usa `text.primary` (preto/branco
          automático por modo); "Guard" usa `primary.main`, que no tema deste projeto
          É o accent da paleta (não o `primary` bruto — ver themeHelper.ts), então fica
          azul nos dois modos (#0600c2 light / #443dff dark) sem hex fixo. Sem borda
          própria: o <Divider /> logo abaixo já separa a marca do menu — ter as duas
          era um traço duplicado. A troca de cor no toggle de tema já é coberta pelo
          fade global de 1s (.theme-transition no <html>, ThemeModeContext); nenhuma
          transição local é necessária aqui. */}
      <Toolbar sx={{ bgcolor: 'transparent' }}>
        <Typography
          variant="h6"
          noWrap
          component="div"
          sx={{ fontWeight: 700, color: 'text.primary' }}
        >
          Phish<Box component="span" sx={{ color: 'primary.main' }}>Guard</Box>
        </Typography>
      </Toolbar>
      <Divider sx={{ borderColor: (theme) => alpha(theme.palette.divider, 0.16) }} />
      <List>
        {menuItems.map((item) => {
          const isSelected = location.pathname === item.path;
          return (
            <ListItem key={item.text} disablePadding>
              <ListItemButton
                onClick={() => item.path && navigate(item.path)}
                selected={isSelected}
                sx={{
                  // Item ativo = fill translúcido do accent (Surface 1 de ênfase).
                  '&.Mui-selected': {
                    backgroundColor: (theme) => alpha(theme.palette.primary.main, 0.15),
                    '&:hover': { backgroundColor: (theme) => alpha(theme.palette.primary.main, 0.25) },
                  },
                }}
              >
                <ListItemIcon sx={{ color: isSelected ? 'primary.main' : 'text.secondary' }}>
                  {item.icon}
                </ListItemIcon>
                <ListItemText primary={item.text} />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>

      <Divider sx={{ borderColor: (theme) => alpha(theme.palette.divider, 0.16) }} />
      <List>
        <ListItem disablePadding>
          <ListItemButton onClick={handleLogout}>
            <ListItemIcon>
              <LogoutIcon color="error" />
            </ListItemIcon>
            <ListItemText primary="Sair do Sistema" sx={{ color: 'error.main' }} />
          </ListItemButton>
        </ListItem>
      </List>
    </div>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />

      {/* Sem ThemeProvider local: a paleta azul é o tema GLOBAL (ThemeModeProvider),
          então AppBar e Drawer herdam background.paper / text.primary / divider.
          Header "flutuante": sem borda sólida (o `divider` a 100% de opacidade contra
          o fundo neutro ficava vibrante demais) — só uma sombra muito suave (6% preto,
          neutra nos dois modos) separa o header do conteúdo abaixo. */}
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          backgroundColor: 'background.paper',
          color: 'text.primary',
          backgroundImage: 'none',
          borderBottom: 'none',
          boxShadow: (theme) => `0 1px 3px ${alpha(theme.palette.common.black, 0.06)}`,
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div">
            Painel Administrativo
          </Typography>
        </Toolbar>
      </AppBar>

      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
              // Overlay temporário já tem elevação/sombra própria do Modal — a borda
              // sólida do MUI (default `borderRight: 1px solid divider`) é redundante.
              borderRight: 'none',
            },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
              // Sidebar "flutuante": mesma lógica do header — troca a borda sólida do
              // MUI (default `borderRight: 1px solid divider`, vibrante contra o fundo
              // neutro) por uma sombra de 6% preto, sutil e neutra nos dois modos.
              borderRight: 'none',
              boxShadow: (theme) => `1px 0 3px ${alpha(theme.palette.common.black, 0.06)}`,
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          // Padding horizontal responsivo = px-4 / sm:px-6 / lg:px-8 (centralizado aqui
          // para todas as telas; PageContainer cuida de max/min width + centralização).
          px: { xs: 2, sm: 3, lg: 4 },
          py: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          // Trava a largura mínima segura no nível do main; abaixo disso, rola só o
          // conteúdo (não quebra o layout).
          overflowX: 'auto',
        }}
      >
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
}