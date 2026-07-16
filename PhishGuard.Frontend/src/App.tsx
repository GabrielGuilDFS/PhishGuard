import { BrowserRouter, Routes, Route, Navigate, useSearchParams } from 'react-router-dom';
import { type ReactNode } from 'react';
import Login from './pages/Login';
import Register from './pages/Register';
import AdminLayout from './layouts/AdminLayout';
import AdminDashboard from './pages/AdminDashboard';
import Targets from './pages/Targets';
import Campaigns from './pages/Campaigns';
import Settings from './pages/Settings';
import Templates from './pages/Templates';
import { ThemeModeProvider } from './context/ThemeModeContext';
import { NotificationProvider } from './context/NotificationContext';
import ForcedLightScope from './theme/ForcedLightScope';

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

function App() {
  return (
    <ThemeModeProvider>
      <NotificationProvider>
        <BrowserRouter>
          <Routes>
            {/* Vitrine pública: travada no modo light, ignora o toggle do painel.
                O <ForcedLightScope> fica AQUI (e não dentro de cada página) para que
                exista um único ponto de verdade sobre quais rotas são travadas. */}
            <Route path="/" element={<ForcedLightScope><HomeLandingPage /></ForcedLightScope>} />
            <Route path="/login" element={<ForcedLightScope><Login /></ForcedLightScope>} />
            <Route path="/register" element={<ForcedLightScope><Register /></ForcedLightScope>} />
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
              <Route path="dashboard" element={<AdminDashboard />} />
              <Route path="targets" element={<Targets />} />
              <Route path="campaigns" element={<Campaigns />} />
              <Route path="templates" element={<Templates />} />
              <Route path="settings" element={<Settings />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </NotificationProvider>
    </ThemeModeProvider>
  );
}

export default App;