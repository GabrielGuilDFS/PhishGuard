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
import {
  Menu as MenuIcon,
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  Send as SendIcon,
  Logout as LogoutIcon,
  Settings as SettingsIcon,
  FolderCopy as FolderIcon
} from '@mui/icons-material';
import { brandPalette } from '../theme';

const drawerWidth = 260;

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogout = () => {
    localStorage.removeItem('phishguard_token');
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
      {/* Cabeçalho de marca: gradiente de destaque da paleta. É azul-escuro nos dois
          modos, então o conteúdo por cima vai de branco + periwinkle fixos (o `text`
          do modo light — preto — teria contraste ruim sobre ele). */}
      <Toolbar
        sx={{
          background: 'var(--linearPrimaryAccent)',
          color: '#ffffff',
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Typography variant="h6" noWrap component="div" sx={{ fontWeight: 700 }}>
          Phish<Box component="span" sx={{ color: brandPalette.light.secondary }}>Guard</Box>
        </Typography>
      </Toolbar>
      <Divider />
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
      
      <Divider />
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
          então AppBar e Drawer herdam background.paper / text.primary / divider. */}
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          backgroundColor: 'background.paper',
          color: 'text.primary',
          backgroundImage: 'none',
          boxShadow: 'none',
          borderBottom: 1,
          borderColor: 'divider',
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
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
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