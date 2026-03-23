import HtmlEditorView, { type TemplateModel } from '../components/HtmlEditorView';

const phishingTemplates: TemplateModel[] = [
  {
    id: 'microsoft',
    nome: 'Login Microsoft 365',
    html: `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Entrar em sua conta</title>
  <style>
    body { background-color: #f4f4f4; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }
    .login-box { background: white; padding: 44px; width: 100%; max-width: 380px; box-shadow: 0 2px 6px rgba(0,0,0,0.2); }
    .logo { height: 24px; margin-bottom: 24px; }
    h2 { font-size: 24px; font-weight: 600; margin-bottom: 16px; color: #1b1b1b; margin-top: 0; }
    input { width: 100%; padding: 10px; margin-bottom: 16px; border: 1px solid #666; font-size: 15px; box-sizing: border-box; border-top-width: 1px; }
    input:focus { border-color: #0067b8; outline: none; }
    .btn { background-color: #0067b8; color: white; border: none; padding: 10px 32px; font-size: 15px; cursor: pointer; float: right; }
    .btn:hover { background-color: #005da6; }
    a { color: #0067b8; text-decoration: none; font-size: 13px; }
    .footer-links { margin-top: 16px; font-size: 13px; }
  </style>
</head>
<body>
  <div class="login-box">
    <img src="https://logincdn.msauth.net/shared/1.0/content/images/microsoft_logo_ee5c8d9fb6248c938fd0dc19370e90bd.svg" alt="Microsoft" class="logo">
    <h2>Entrar</h2>
    <form action="{{POST_URL}}" method="POST">
      <input type="email" name="email" placeholder="Email, telefone ou Skype" required>
      <input type="password" name="senha" placeholder="Senha" required>
      <div class="footer-links">
        <p>Nenhuma conta? <a href="#">Crie uma!</a></p>
        <p><a href="#">Não consegue acessar sua conta?</a></p>
      </div>
      <div style="margin-top: 24px; display: inline-block; width: 100%;">
        <button type="submit" class="btn">Avançar</button>
      </div>
    </form>
  </div>
</body>
</html>`
  },
  {
    id: 'intranet',
    nome: 'Portal Corporativo / Intranet Genérica',
    html: `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Acesso Restrito - Intranet</title>
  <style>
    body { background-color: #eceff1; font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }
    .container { background: white; padding: 40px; width: 100%; max-width: 400px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-top: 6px solid #2e7d32; text-align: center; }
    h2 { color: #333; margin-bottom: 10px; }
    p { color: #666; font-size: 14px; margin-bottom: 25px; }
    input { width: 100%; padding: 12px; margin-bottom: 15px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; font-size: 14px; }
    .btn { background-color: #2e7d32; color: white; border: none; padding: 12px; width: 100%; border-radius: 4px; font-size: 16px; cursor: pointer; font-weight: bold; }
    .btn:hover { background-color: #1b5e20; }
    .aviso { margin-top: 20px; font-size: 12px; color: #999; }
  </style>
</head>
<body>
  <div class="container">
    <h2>Acesso Restrito</h2>
    <p>Autentique-se com suas credenciais de rede para visualizar este documento confidencial.</p>
    <form action="{{POST_URL}}" method="POST">
      <input type="text" name="matricula" placeholder="Matrícula ou Usuário" required>
      <input type="password" name="senha" placeholder="Senha da Rede" required>
      <button type="submit" class="btn">Acessar Sistema</button>
    </form>
    <div class="aviso">Acesso monitorado pelo Departamento de TI.</div>
  </div>
</body>
</html>`
  }
];

export default function PhishingPages() {
  return (
    <HtmlEditorView
      title="Páginas Simuladas (Armadilhas)"
      apiEndpoint="http://localhost:5000/api/PhishingPages"
      emptyMessage="Nenhuma página cadastrada. Crie sua primeira armadilha!"
      defaultTemplates={phishingTemplates}
    />
  );
}