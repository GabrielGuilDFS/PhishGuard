import { BrowserRouter, Routes, Route, Navigate, useSearchParams } from 'react-router-dom';
import { type ReactNode } from 'react';
import Login from './pages/Login';
import Register from './pages/Register';
import AdminLayout from './layouts/AdminLayout';
import DashboardHome from './pages/DashboardHome';
import Targets from './pages/Targets';
import Campaigns from './pages/Campaigns';
import Settings from './pages/Settings';
import Templates from './pages/Templates';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { NotificationProvider } from './context/NotificationContext';

import LandingPage from './pages/LandingPage';
import HomeLandingPage from './pages/HomeLandingPage';
import CheckoutPage from './pages/CheckoutPage';
import { educationalTemplates } from './data/educationalTemplates';

const PrivateRoute = ({ children }: { children: ReactNode }) => {
  const token = localStorage.getItem('phishguard_token');
  return token ? children : <Navigate to="/login" replace />;
};

// Rota padrão de feedback educacional: renderiza o molde educacional indicado por
// ?template= (ex.: 'basico_phishing'), reutilizando o catálogo central estático em
// data/educationalTemplates.ts — sem duplicar lógica de renderização.
const EducationalFeedback = () => {
  const [searchParams] = useSearchParams();
  const templateId = searchParams.get('template') || 'basico_phishing';
  const molde = educationalTemplates.find((t) => t.id === templateId);
  const html = molde
    ? molde.html
    : '<div style="padding:2rem;text-align:center;font-family:sans-serif;color:#b00;">Treinamento não encontrado.</div>';
  return <div dangerouslySetInnerHTML={{ __html: html }} />;
};

const theme = createTheme({
  palette: {
    primary: { main: '#DAA520', contrastText: '#ffffff' },
    background: { default: '#f4f6f8' },
  },
  shape: { borderRadius: 8 },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NotificationProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<HomeLandingPage />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/landing/:id" element={<LandingPage />} />
            <Route path="/educational-feedback" element={<EducationalFeedback />} />
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
              <Route path="campaigns" element={<Campaigns />} />
              <Route path="templates" element={<Templates />} />
              <Route path="settings" element={<Settings />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </NotificationProvider>
    </ThemeProvider>
  );
}

export default App;