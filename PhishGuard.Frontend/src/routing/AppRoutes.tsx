import type { ReactNode } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import PrivateRoute from '../auth/PrivateRoute';

interface AppRoutesProps {
  home: ReactNode;
  login: ReactNode;
  register: ReactNode;
  checkout: ReactNode;
  landing: ReactNode;
  educationalFeedback: ReactNode;
  adminLayout: ReactNode;
  dashboard: ReactNode;
  targets: ReactNode;
  campaigns: ReactNode;
  templates: ReactNode;
  settings: ReactNode;
}

/**
 * Árvore declarativa de rotas da aplicação.
 *
 * As páginas são injetadas pelo App para que o roteamento permaneça independente
 * da implementação visual de cada tela e possa ser validado isoladamente.
 */
export default function AppRoutes({
  home,
  login,
  register,
  checkout,
  landing,
  educationalFeedback,
  adminLayout,
  dashboard,
  targets,
  campaigns,
  templates,
  settings,
}: AppRoutesProps) {
  return (
    <Routes>
      <Route path="/" element={home} />
      <Route path="/login" element={login} />
      <Route path="/register" element={register} />
      <Route path="/checkout" element={checkout} />
      <Route path="/landing/:id" element={landing} />
      <Route path="/educational-feedback" element={educationalFeedback} />

      <Route
        path="/admin"
        element={<PrivateRoute>{adminLayout}</PrivateRoute>}
      >
        {/* /admin sozinho não possui conteúdo próprio: após validar a sessão,
            leva o usuário à página inicial do painel. */}
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={dashboard} />
        <Route path="targets" element={targets} />
        <Route path="campaigns" element={campaigns} />
        <Route path="templates" element={templates} />
        <Route path="settings" element={settings} />
        {/* Mantém URLs administrativas desconhecidas dentro da barreira do
            PrivateRoute: sem sessão vão ao login; com sessão, ao dashboard. */}
        <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
      </Route>

      {/* Sem fallback, o React Router renderiza null e produz uma tela vazia. */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
