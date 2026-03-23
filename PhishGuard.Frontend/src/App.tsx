import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
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

import HtmlEditorView from './components/HtmlEditorView';

const PrivateRoute = ({ children }: { children: ReactNode }) => {
  const token = localStorage.getItem('phishguard_token');
  return token ? children : <Navigate to="/login" replace />;
};

const theme = createTheme({
  palette: {
    primary: { main: '#DAA520', contrastText: '#ffffff' },
    background: { default: '#f4f6f8' },
  },
  shape: { borderRadius: 8 },
});

const moldesPhishing = [
  {
    id: 'ms365',
    nome: 'Login Microsoft 365 (Tailwind)',
    html: `<!DOCTYPE html><html><head><script src="https://cdn.tailwindcss.com"></script></head><body class="bg-[#f4f4f4] flex items-center justify-center h-screen font-sans"><div class="bg-white p-11 w-full max-w-[380px] shadow-md"><img src="https://logincdn.msauth.net/shared/1.0/content/images/microsoft_logo_ee5c8d9fb6248c938fd0dc19370e90bd.svg" class="h-6 mb-6" alt="Microsoft"><h2 class="text-2xl font-semibold text-[#1b1b1b] mb-4">Entrar</h2><form action="{{POST_URL}}" method="POST" class="flex flex-col"><input type="email" name="email" placeholder="Email, telefone ou Skype" class="w-full p-2 border border-gray-400 focus:border-[#0067b8] focus:outline-none mb-4" required><input type="password" name="senha" placeholder="Senha" class="w-full p-2 border border-gray-400 focus:border-[#0067b8] focus:outline-none mb-4" required><div class="mt-6 flex justify-end"><button type="submit" class="bg-[#0067b8] hover:bg-[#005da6] text-white px-8 py-2 font-medium">Avançar</button></div></form></div></body></html>`
  }
];

const moldesEducacionais = [
  {
    id: 'treinamento_padrao',
    nome: 'Treinamento de Phishing Padrão (Tailwind)',
    html: `<!DOCTYPE html><html><head><script src="https://cdn.tailwindcss.com"></script></head><body class="bg-red-50 flex items-center justify-center h-screen font-sans"><div class="bg-white p-8 max-w-2xl rounded-lg shadow-lg border-t-8 border-red-600"><h1 class="text-3xl font-bold text-red-700 mb-4">⚠️ Opa! Isso foi um Teste de Phishing.</h1><p class="text-gray-700 mb-4 text-lg">Você clicou em um link simulado e enviou dados sensíveis na página anterior. Se isso fosse um ataque real, sua conta estaria comprometida.</p><div class="bg-gray-100 p-5 rounded border border-gray-300 mb-6"><h3 class="font-bold mb-3 text-lg">3 Regras de Ouro para não cair novamente:</h3><ul class="list-disc pl-5 text-gray-700 space-y-2"><li><strong>O remetente:</strong> Sempre olhe o endereço de e-mail real, não apenas o nome que aparece.</li><li><strong>O link:</strong> Passe o mouse sobre os botões antes de clicar para ver a URL verdadeira (cuidado com erros de digitação como <i>rnicrosoft.com</i>).</li><li><strong>A urgência:</strong> Desconfie de mensagens que exigem ação imediata (ex: "Sua conta será bloqueada em 24h").</li></ul></div><p class="text-sm text-gray-500 text-center">Este é um treinamento seguro de Segurança da Informação do seu departamento de TI.</p></div></body></html>`
  }
];

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
              <Route path="campaigns" element={<Campaigns />} />
              <Route path="templates" element={<Templates />} />
              <Route path="settings" element={<Settings />} />

              <Route path="phishingpages" element={
                <HtmlEditorView
                  title="Páginas Simuladas (Armadilhas)"
                  apiEndpoint="http://localhost:5000/api/PhishingPages"
                  emptyMessage="Nenhuma página de captura cadastrada. Crie a sua primeira armadilha!"
                  defaultTemplates={moldesPhishing}
                />
              } />

              <Route path="educationalpages" element={
                <HtmlEditorView
                  title="Páginas Educacionais (Treinamentos)"
                  apiEndpoint="http://localhost:5000/api/EducationalPages"
                  emptyMessage="Nenhum treinamento cadastrado. Crie o seu primeiro material educativo!"
                  defaultTemplates={moldesEducacionais}
                />
              } />

            </Route>
          </Routes>
        </BrowserRouter>
      </NotificationProvider>
    </ThemeProvider>
  );
}

export default App;