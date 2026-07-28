import { BrowserRouter, Routes, Route, useSearchParams } from 'react-router-dom';
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
import PrivateRoute from './auth/PrivateRoute';

import LandingPage from './pages/LandingPage';
import HomeLandingPage from './pages/HomeLandingPage';
import CheckoutPage from './pages/CheckoutPage';
import { educationalTemplates } from './data/educationalTemplates';
import { feedbackTrainings } from './data/feedbackTrainings';
import FeedbackTraining from './components/FeedbackTraining';
import { EDU_FEEDBACK_QUERY } from './shared/trackingContract';

// Rota de feedback educacional resolvida por ?template=:
//   1) se houver um treinamento INTERATIVO (Just-in-Time Training) para o id,
//      renderiza o componente reutilizável <FeedbackTraining> (amzprime, etc.);
//   2) senão, cai no catálogo estático legado (data/educationalTemplates.ts),
//      mantendo compatibilidade com as iscas ainda não migradas.
const EducationalFeedback = () => {
  const [searchParams] = useSearchParams();
  // Nome do parâmetro vem do contrato compartilhado (não hardcode 'template').
  const templateId = searchParams.get(EDU_FEEDBACK_QUERY.template) || 'basico_phishing';

  const training = feedbackTrainings[templateId];
  if (training) {
    return <FeedbackTraining config={training} />;
  }

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