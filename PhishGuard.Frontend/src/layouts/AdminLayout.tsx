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
  CssBaseline,
  Collapse 
} from '@mui/material';
import { 
  Web as WebIcon,
  Menu as MenuIcon, 
  Dashboard as DashboardIcon, 
  People as PeopleIcon, 
  Phishing as PhishingIcon, 
  Send as SendIcon,        
  Logout as LogoutIcon,
  Settings as SettingsIcon,
  ExpandLess,        
  ExpandMore,         
  FolderCopy as FolderIcon, 
  School as SchoolIcon      
} from '@mui/icons-material';

const drawerWidth = 260;
const primaryColor = '#DAA520'; 

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  
  const [openResources, setOpenResources] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleResourcesClick = () => {
    setOpenResources(!openResources);
  };

  const handleLogout = () => {
    localStorage.removeItem('phishguard_token');
    navigate('/login');
  };

  const menuItems = [
    { text: 'Visão Geral', icon: <DashboardIcon />, path: '/admin/dashboard' },
    { text: 'Gestão de Alvos', icon: <PeopleIcon />, path: '/admin/targets' },
    { 
      text: 'Biblioteca de Recursos', 
      icon: <FolderIcon />, 
      isGroup: true, 
      children: [
        { text: 'Cenários (E-mails)', icon: <PhishingIcon />, path: '/admin/templates' },
        { text: 'Páginas Falsas', icon: <WebIcon />, path: '/admin/phishingpages' },
        { text: 'Páginas Educacionais', icon: <SchoolIcon />, path: '/admin/educationalpages' } // Rota que faremos a seguir
      ]
    },
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
          
          if (item.isGroup) {
            return (
              <div key={item.text}>
                <ListItem disablePadding>
                  <ListItemButton onClick={handleResourcesClick}>
                    <ListItemIcon sx={{ color: primaryColor }}>
                      {item.icon}
                    </ListItemIcon>
                    <ListItemText primary={item.text} />
                    {openResources ? <ExpandLess /> : <ExpandMore />}
                  </ListItemButton>
                </ListItem>
                
                <Collapse in={openResources} timeout="auto" unmountOnExit>
                  <List component="div" disablePadding>
                    {item.children?.map((child) => {
                      const isSelected = location.pathname === child.path;
                      return (
                        <ListItemButton 
                          key={child.text}
                          onClick={() => navigate(child.path)}
                          selected={isSelected}
                          sx={{ 
                            pl: 4, // Dá um recuo (padding-left) para parecer um submenu
                            '&.Mui-selected': {
                              backgroundColor: `${primaryColor}15`,
                              '&:hover': { backgroundColor: `${primaryColor}25` },
                            },
                          }}
                        >
                          <ListItemIcon sx={{ color: primaryColor }}>
                            {child.icon}
                          </ListItemIcon>
                          <ListItemText primary={child.text} />
                        </ListItemButton>
                      );
                    })}
                  </List>
                </Collapse>
              </div>
            );
          }

          const isSelected = location.pathname === item.path;
          return (
            <ListItem key={item.text} disablePadding>
              <ListItemButton 
                onClick={() => item.path && navigate(item.path)}
                selected={isSelected}
                sx={{
                  '&.Mui-selected': {
                    backgroundColor: `${primaryColor}15`,
                    '&:hover': { backgroundColor: `${primaryColor}25` },
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