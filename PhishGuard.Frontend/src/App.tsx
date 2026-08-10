import { lazy, Suspense, useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeModeProvider } from './context/ThemeModeContext';
import { NotificationProvider } from './context/NotificationContext';
import ForcedLightScope from './theme/ForcedLightScope';
import AppRoutes from './routing/AppRoutes';
import { REGISTRATION_ENABLED } from './config';

import { retryPendingTrackingEvents } from './shared/trackingRetryQueue';

const Login = lazy(() => import('./pages/Login'));
const Register = lazy(() => import('./pages/Register'));
const RegistrationUnavailable = lazy(() => import('./pages/RegistrationUnavailable'));
const AdminLayout = lazy(() => import('./layouts/AdminLayout'));
const AdminDashboard = lazy(() => import('./pages/AdminDashboard'));
const Targets = lazy(() => import('./pages/Targets'));
const Campaigns = lazy(() => import('./pages/Campaigns'));
const Settings = lazy(() => import('./pages/Settings'));
const Templates = lazy(() => import('./pages/Templates'));
const LandingPage = lazy(() => import('./pages/LandingPage'));
const HomeLandingPage = lazy(() => import('./pages/HomeLandingPage'));
const CheckoutPage = lazy(() => import('./pages/CheckoutPage'));
const EducationalFeedbackPage = lazy(() => import('./pages/EducationalFeedbackPage'));

function TrackingRetryProcessor() {
  useEffect(() => {
    void retryPendingTrackingEvents();
    const retryQuandoOnline = () => { void retryPendingTrackingEvents(); };
    window.addEventListener('online', retryQuandoOnline);
    return () => window.removeEventListener('online', retryQuandoOnline);
  }, []);

  return null;
}

function App() {
  return (
    <ThemeModeProvider>
      <NotificationProvider>
        <TrackingRetryProcessor />
        <BrowserRouter>
          <Suspense fallback={<div role="status" aria-label="Carregando página" />}>
            <AppRoutes
              home={<ForcedLightScope><HomeLandingPage /></ForcedLightScope>}
              login={<ForcedLightScope><Login /></ForcedLightScope>}
              register={
                <ForcedLightScope>
                  {REGISTRATION_ENABLED ? <Register /> : <RegistrationUnavailable />}
                </ForcedLightScope>
              }
              checkout={<CheckoutPage />}
              landing={<LandingPage />}
              educationalFeedback={<EducationalFeedbackPage />}
              adminLayout={<AdminLayout />}
              dashboard={<AdminDashboard />}
              targets={<Targets />}
              campaigns={<Campaigns />}
              templates={<Templates />}
              settings={<Settings />}
            />
          </Suspense>
        </BrowserRouter>
      </NotificationProvider>
    </ThemeModeProvider>
  );
}

export default App;
