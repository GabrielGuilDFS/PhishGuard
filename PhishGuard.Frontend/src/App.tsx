import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { type ReactNode } from 'react';
import Login from './pages/Login';
import Register from './pages/Register';
import AdminLayout from './layouts/AdminLayout'; 
import DashboardHome from './pages/DashboardHome';
import Targets from './pages/Targets';
import Scenarios from './pages/Scenarios';
import Campaigns from './pages/Campaigns';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { NotificationProvider } from './context/NotificationContext';

const PrivateRoute = ({ children }: { children: ReactNode }) => {
  const token = localStorage.getItem('phishguard_token');
  return token ? children : <Navigate to="/login" replace />;
};

const theme = createTheme({
  palette: {
    primary: {
      main: '#DAA520', 
      contrastText: '#ffffff',
    },
    background: {
      default: '#f4f6f8',
    },
  },
  shape: {
    borderRadius: 8,
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NotificationProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route 
            path="/admin" 
            element={
              <PrivateRoute>
                <AdminLayout />
              </PrivateRoute>
            }
          >
            <Route path="dashboard" element={<DashboardHome />} />
            <Route path="targets" element={<Targets />} />
            <Route path="scenarios" element={<Scenarios />} />
            <Route path="campaigns" element={<Campaigns />} />
          </Route>
        </Routes>
      </BrowserRouter>
      </NotificationProvider>
    </ThemeProvider>
  );
}

export default App;