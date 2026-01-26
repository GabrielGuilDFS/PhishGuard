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
import { 
  Menu as MenuIcon, 
  Dashboard as DashboardIcon, 
  People as PeopleIcon, 
  Phishing as PhishingIcon, 
  Send as SendIcon,         
  Logout as LogoutIcon,
  Settings as SettingsIcon
} from '@mui/icons-material';

const drawerWidth = 260;
const primaryColor = '#DAA520'; 

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

  const menuItems = [
    { text: 'Visão Geral', icon: <DashboardIcon />, path: '/admin/dashboard' },
    { text: 'Gestão de Alvos', icon: <PeopleIcon />, path: '/admin/targets' },
    { text: 'Biblioteca de Cenários', icon: <PhishingIcon />, path: '/admin/scenarios' },
    { text: 'Nova Campanha', icon: <SendIcon />, path: '/admin/campaigns' },
    { text: 'Configurações', icon: <SettingsIcon />, path: '/admin/settings' }
  ];

  const drawer = (
    <div>
      <Toolbar sx={{ backgroundColor: primaryColor, color: 'white' }}>
        <Typography variant="h6" noWrap component="div">
          PhishGuard
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {menuItems.map((item) => {
            const isSelected = location.pathname === item.path;
            
            return (
            <ListItem key={item.text} disablePadding>
                <ListItemButton 
                onClick={() => navigate(item.path)}
                selected={isSelected}
                sx={{
                    '&.Mui-selected': {
                    backgroundColor: `${primaryColor}15`,
                    '&:hover': {
                        backgroundColor: `${primaryColor}25`,
                    },
                    },
                }}
                >
                <ListItemIcon sx={{ color: primaryColor }}>
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
      
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          backgroundColor: primaryColor, 
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
        sx={{ flexGrow: 1, p: 3, width: { sm: `calc(100% - ${drawerWidth}px)` } }}
      >
        <Toolbar /> 
        <Outlet /> 
      </Box>
    </Box>
  );
}