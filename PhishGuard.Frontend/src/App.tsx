import { BrowserRouter, useSearchParams } from 'react-router-dom';
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
import AppRoutes from './routing/AppRoutes';

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
          <AppRoutes
            home={<ForcedLightScope><HomeLandingPage /></ForcedLightScope>}
            login={<ForcedLightScope><Login /></ForcedLightScope>}
            register={<ForcedLightScope><Register /></ForcedLightScope>}
            checkout={<CheckoutPage />}
            landing={<LandingPage />}
            educationalFeedback={<EducationalFeedback />}
            adminLayout={<AdminLayout />}
            dashboard={<AdminDashboard />}
            targets={<Targets />}
            campaigns={<Campaigns />}
            templates={<Templates />}
            settings={<Settings />}
          />
        </BrowserRouter>
      </NotificationProvider>
    </ThemeModeProvider>
  );
}

export default App;
